using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using static OffWithTheirNuts.NetworkConfig;

namespace OffWithTheirNuts {
    internal sealed class ServerSync {
        private readonly ConfigEntry<bool> enabledSetting;
        private readonly Action refreshHotkey;
        private readonly Action onWorldClosed;
        private readonly Action<ZNetPeer, ZDOID, bool> startChange;
        private readonly Action<ZNetPeer> startReset;
        private readonly Action<ZDO, bool, bool> setSterility;
        private ZNet network;
        private bool canManageWorld;

        internal bool HasWorld => !(network is null);
        internal bool SettingsReady {
            get;
            private set;
        }
        internal bool RestrictionsEnabled {
            get;
            private set;
        }
        internal bool RestrictionsActive => SettingsReady && RestrictionsEnabled;
        internal bool Enabled => network == null ? enabledSetting.Value : RestrictionsEnabled;
        internal bool CanChangeEnabled => network == null || CanReset;
        internal bool CanReset => network != null && (network.IsServer() || (SettingsReady && canManageWorld));

        internal ServerSync(
            ConfigEntry<bool> enabledSetting,
            Action refreshHotkey,
            Action onWorldClosed,
            Action<ZNetPeer, ZDOID, bool> startChange,
            Action<ZNetPeer> startReset,
            Action<ZDO, bool, bool> setSterility
        ) {
            this.enabledSetting = enabledSetting;
            this.refreshHotkey = refreshHotkey;
            this.onWorldClosed = onWorldClosed;
            this.startChange = startChange;
            this.startReset = startReset;
            this.setSterility = setSterility;
        }

        private enum Command : byte {
            AskSettings = 1,
            Settings = 2,
            SetEnabled = 3,
            Toggle = 4,
            ResetAll = 5,
            OwnerWrites = 6,
            Notice = 7
        }

        internal struct TargetWrite {
            internal readonly ZDOID Id;
            internal long LastOwner;
            internal ushort LastRevision;
            internal double ResendAt;

            internal TargetWrite(ZDOID id) {
                this = default;
                Id = id;
            }
        }

        internal sealed class ChangeResult {
            internal int Changed;
            internal int Unconfirmed;
        }

        private void OpenWorld(ZNet current) {
            CloseWorld();
            network = current;
            SettingsReady = current.IsServer();
            RestrictionsEnabled = SettingsReady && enabledSetting.Value;
            refreshHotkey();
        }

        internal void CloseWorld() {
            network = null;
            SettingsReady = false;
            RestrictionsEnabled = false;
            canManageWorld = false;
            onWorldClosed();
        }

        internal void RequestEnabled(bool requested) {
            if (network == null || network.IsServer()) {
                enabledSetting.Value = requested;
            } else {
                SendToServer(
                    command: Command.SetEnabled,
                    fill: package => package.Write(requested)
                );
            }
        }

        internal void RefreshEnabledSetting() {
            if (network != null && network.IsServer()) {
                RestrictionsEnabled = enabledSetting.Value;
                PublishSettings();
                refreshHotkey();
            }
        }

        internal void RequestChange(ZDOID id, bool requested) {
            if (network.IsServer()) {
                startChange(
                    arg1: null,
                    arg2: id,
                    arg3: requested
                );
            } else {
                SendToServer(
                    command: Command.Toggle,
                    fill: package => {
                        package.Write(id);
                        package.Write(requested);
                    }
                );
            }
        }

        internal void RequestReset() {
            if (network.IsServer()) {
                startReset(null);
            } else {
                SendToServer(Command.ResetAll);
            }
        }

        private void RegisterPeer(ZNetPeer peer) {
            peer.m_rpc.Register<ZPackage>(
                name: RpcName,
                f: (rpc, package) => Receive(
                    peer: peer,
                    package: package
                )
            );
        }

        private void Receive(ZNetPeer peer, ZPackage package) {
            if (network == null || package == null || package.Size() < 1 || package.Size() > MaxPacketSize ||
                !peer.IsReady() || !ReferenceEquals(
                    objA: network.GetPeer(peer.m_uid),
                    objB: peer
                )) {
                return;
            }

            try {
                Command command = (Command)package.ReadByte();
                int remaining = package.Size() - package.GetPos();
                if (network.IsServer()) {
                    switch (command) {
                        case Command.AskSettings when remaining == 0: {
                            SendSettings(peer);
                            break;
                        }
                        case Command.SetEnabled when remaining == 1 && CanAdminister(peer): {
                            enabledSetting.Value = package.ReadBool();
                            break;
                        }
                        case Command.Toggle when remaining == TargetWireSize + 1: {
                            startChange(
                                arg1: peer,
                                arg2: package.ReadZDOID(),
                                arg3: package.ReadBool()
                            );
                            break;
                        }
                        case Command.ResetAll when remaining == 0 && CanAdminister(peer): {
                            startReset(peer);
                            break;
                        }
                    }
                } else if (peer.m_server && ReferenceEquals(
                    objA: network.GetServerPeer(),
                    objB: peer
                )) {
                    switch (command) {
                        case Command.Settings when remaining == 2: {
                            RestrictionsEnabled = package.ReadBool();
                            canManageWorld = package.ReadBool();
                            SettingsReady = true;
                            refreshHotkey();
                            break;
                        }
                        case Command.OwnerWrites when SettingsReady: {
                            ApplyOwnerWrites(package);
                            break;
                        }
                        case Command.Notice when remaining > 0: {
                            ShowNotice(package.ReadString());
                            break;
                        }
                    }
                }
            } catch (IOException) {
                // Ignore truncated packets.
            }
        }

        private void SendToServer(Command command, Action<ZPackage> fill = null) {
            ZNetPeer server = network?.GetServerPeer();
            if (server != null && server.IsReady()) {
                Send(
                    peer: server,
                    command: command,
                    fill: fill
                );
            }
        }

        private static void Send(ZNetPeer peer, Command command, Action<ZPackage> fill = null) {
            var payload = new ZPackage();
            payload.Write((byte)command);
            fill?.Invoke(payload);
            peer.m_rpc.Invoke(
                method: RpcName,
                parameters: payload
            );
        }

        internal bool CanAdminister(ZNetPeer peer) {
            return network != null && network.IsServer() &&
                (peer == null || network.IsAdmin(peer.m_socket.GetHostName()));
        }

        private void PublishSettings() {
            foreach (ZNetPeer peer in network.GetPeers()) {
                if (peer.IsReady()) {
                    SendSettings(peer);
                }
            }
        }

        private void SendSettings(ZNetPeer peer) {
            Send(
                peer: peer,
                command: Command.Settings,
                fill: package => {
                    package.Write(RestrictionsEnabled);
                    package.Write(CanAdminister(peer));
                }
            );
        }

        internal static bool CanChangeTarget(ZNetPeer requester, ZDO target) {
            GameObject targetObject = ZNetScene.instance?.FindInstance(target.m_uid);
            if (requester == null) {
                Player local = Player.m_localPlayer;
                Character hovered = local != null ? local.GetHoverCreature() : null;
                return hovered != null && hovered.gameObject == targetObject && OffWithTheirNutsPlugin.CanSterilize(targetObject);
            }

            ZDO player = ZDOMan.instance.GetZDO(requester.m_characterID);
            if (player == null || player.GetOwner() != requester.m_uid) {
                return false;
            }

            ZNetScene scene = ZNetScene.instance;
            if (scene == null) {
                return false;
            }
            GameObject playerPrefab = scene.GetPrefab(player.GetPrefab());
            Player playerTemplate = playerPrefab != null ? playerPrefab.GetComponent<Player>() : null;
            if (playerTemplate == null) {
                return false;
            }
            GameObject targetPrefab = scene.GetPrefab(target.GetPrefab());
            Character targetTemplate = targetPrefab != null ? targetPrefab.GetComponent<Character>() : null;
            if (targetTemplate == null) {
                return false;
            }

            if (targetObject != null) {
                if (!OffWithTheirNutsPlugin.CanSterilize(targetObject)) {
                    return false;
                }
            } else if (!target.GetBool(
                hash: ZDOVars.s_tamed,
                defaultValue: false
            ) || targetPrefab.GetComponent<Tameable>() == null ||
                targetPrefab.GetComponent<Procreation>() == null || targetPrefab.GetComponent<Growup>() != null) {
                return false;
            }

            float distance = playerTemplate.m_maxInteractDistance + Mathf.Max(
                a: 0f,
                b: targetTemplate.GetHoverOffset()
            );
            Vector3 playerPosition = player.GetPosition();
            CapsuleCollider collider = targetObject != null ? targetObject.GetComponent<CapsuleCollider>() : null;
            if (collider != null && collider.enabled) {
                return (playerPosition - collider.ClosestPoint(playerPosition)).sqrMagnitude <= distance * distance;
            }

            CapsuleCollider prefabCollider = targetPrefab.GetComponent<CapsuleCollider>();
            if (prefabCollider != null) {
                Vector3 scale = targetPrefab.transform.localScale;
                distance += prefabCollider.radius * Mathf.Max(
                    a: Mathf.Abs(scale.x),
                    b: Mathf.Abs(scale.z)
                );
            }
            return (playerPosition - target.GetPosition()).sqrMagnitude <= distance * distance;
        }

        internal IEnumerator ApplyChanges(
            List<TargetWrite> remaining,
            bool requestedState,
            bool playEffects,
            ChangeResult result
        ) {
            if (remaining.Count == 0) {
                yield break;
            }

            double timeoutAt = Time.realtimeSinceStartupAsDouble + ChangeTimeout;
            double expiresAt = network.GetTimeSeconds() + ChangeTimeout;
            var wait = new WaitForSecondsRealtime(0.10f);
            var byOwner = new Dictionary<ZNetPeer, List<TargetWrite>>();
            while (remaining.Count > 0) {
                double now = Time.realtimeSinceStartupAsDouble;
                for (int i = remaining.Count - 1; i >= 0; i--) {
                    TargetWrite write = remaining[i];
                    ZDO zdo = ZDOMan.instance.GetZDO(write.Id);
                    if (zdo == null) {
                        result.Unconfirmed++;
                        remaining.RemoveAt(i);
                    } else if (OffWithTheirNutsPlugin.IsSterile(zdo) == requestedState) {
                        result.Changed++;
                        remaining.RemoveAt(i);
                    } else if (zdo.IsOwner() || !zdo.HasOwner()) {
                        setSterility(
                            arg1: zdo,
                            arg2: requestedState,
                            arg3: playEffects
                        );
                        result.Changed++;
                        remaining.RemoveAt(i);
                    } else {
                        long owner = zdo.GetOwner();
                        ushort revision = zdo.OwnerRevision;
                        if (write.LastOwner == owner && write.LastRevision == revision && now < write.ResendAt) {
                            continue;
                        }

                        ZNetPeer peer = network.GetPeer(owner);
                        if (peer == null || !peer.IsReady()) {
                            continue;
                        }

                        write.LastOwner = owner;
                        write.LastRevision = revision;
                        write.ResendAt = now + ResendInterval;
                        remaining[i] = write;
                        if (!byOwner.TryGetValue(
                            key: peer,
                            value: out List<TargetWrite> queued
                        )) {
                            queued = new List<TargetWrite>();
                            byOwner.Add(
                                key: peer,
                                value: queued
                            );
                        }
                        queued.Add(write);
                    }
                }

                foreach (KeyValuePair<ZNetPeer, List<TargetWrite>> batch in byOwner) {
                    if (batch.Value.Count != 0) {
                        SendOwnerWrites(
                            peer: batch.Key,
                            writes: batch.Value,
                            requestedState: requestedState,
                            playEffects: playEffects,
                            expiresAt: expiresAt
                        );
                        batch.Value.Clear();
                    }
                }

                if (remaining.Count == 0 || now >= timeoutAt) {
                    break;
                }
                yield return wait;
            }
            result.Unconfirmed += remaining.Count;
        }

        private static void SendOwnerWrites(
            ZNetPeer peer,
            List<TargetWrite> writes,
            bool requestedState,
            bool playEffects,
            double expiresAt
        ) {
            for (int start = 0; start < writes.Count; start += MaxBatch) {
                int count = Math.Min(
                    val1: MaxBatch,
                    val2: writes.Count - start
                );
                var packet = new ZPackage();
                packet.Write((byte)Command.OwnerWrites);
                packet.Write(requestedState);
                packet.Write(playEffects);
                packet.Write(expiresAt);
                packet.Write(count);
                for (int i = start; i < start + count; i++) {
                    packet.Write(writes[i].Id);
                    packet.Write(writes[i].LastRevision);
                }
                peer.m_rpc.Invoke(
                    method: RpcName,
                    parameters: packet
                );
            }
        }

        private void ApplyOwnerWrites(ZPackage packet) {
            const int headerSize = 2 + sizeof(double) + sizeof(int);
            if (packet.Size() - packet.GetPos() < headerSize) {
                return;
            }

            bool requestedState = packet.ReadBool();
            bool playEffects = packet.ReadBool();
            double expiresAt = packet.ReadDouble();
            int count = packet.ReadInt();
            if (count < 1 || count > MaxBatch || ZDOMan.instance == null ||
                network.GetTimeSeconds() > expiresAt ||
                packet.Size() - packet.GetPos() != count * (TargetWireSize + sizeof(ushort))) {
                return;
            }

            for (int i = 0; i < count; i++) {
                ZDOID id = packet.ReadZDOID();
                ushort expectedRevision = packet.ReadUShort();
                ZDO zdo = ZDOMan.instance.GetZDO(id);
                if (zdo == null || !zdo.IsOwner() || zdo.OwnerRevision != expectedRevision) {
                    continue;
                }

                setSterility(
                    arg1: zdo,
                    arg2: requestedState,
                    arg3: playEffects
                );
                ZDOMan.instance.ForceSendZDO(id);
            }
        }

        internal static void Notify(ZNetPeer peer, string message) {
            if (peer == null) {
                ShowNotice(message);
            } else if (peer.IsReady()) {
                Send(
                    peer: peer,
                    command: Command.Notice,
                    fill: package => package.Write(message)
                );
            }
        }

        private static void ShowNotice(string message) {
            MessageHud hud = MessageHud.instance;
            if (hud != null) {
                hud.ShowMessage(
                    type: MessageHud.MessageType.TopLeft,
                    text: message,
                    amount: 0,
                    icon: null,
                    showDespiteHiddenHUD: false,
                    log: false
                );
            }
        }

        [HarmonyPatch(
            declaringType: typeof(ZNet),
            methodName: "Awake"
        )]
        private static class WorldAwakePatch {
            private static void Postfix(ZNet __instance) {
                OffWithTheirNutsPlugin.ActiveServerSync?.OpenWorld(__instance);
            }
        }

        [HarmonyPatch(
            declaringType: typeof(ZNet),
            methodName: "OnDestroy"
        )]
        private static class WorldDestroyPatch {
            private static void Prefix(ZNet __instance) {
                ServerSync sync = OffWithTheirNutsPlugin.ActiveServerSync;
                if (sync != null && ReferenceEquals(
                    objA: sync.network,
                    objB: __instance
                )) {
                    sync.CloseWorld();
                }
            }
        }

        [HarmonyPatch(
            declaringType: typeof(ZNet),
            methodName: "OnNewConnection"
        )]
        private static class PeerConnectionPatch {
            private static void Postfix(ZNet __instance, ZNetPeer __0) {
                ServerSync sync = OffWithTheirNutsPlugin.ActiveServerSync;
                if (sync != null && ReferenceEquals(
                    objA: sync.network,
                    objB: __instance
                )) {
                    sync.RegisterPeer(__0);
                }
            }
        }

        [HarmonyPatch(
            declaringType: typeof(ZNet),
            methodName: "RPC_PeerInfo"
        )]
        private static class PeerInfoPatch {
            private static void Postfix(ZNet __instance, ZRpc __0) {
                ServerSync sync = OffWithTheirNutsPlugin.ActiveServerSync;
                if (sync == null || !ReferenceEquals(
                    objA: sync.network,
                    objB: __instance
                ) || __instance.IsServer()) {
                    return;
                }
                ZNetPeer server = __instance.GetServerPeer();
                if (server != null && ReferenceEquals(
                    objA: server.m_rpc,
                    objB: __0
                )) {
                    Send(
                        peer: server,
                        command: Command.AskSettings
                    );
                }
            }
        }
    }
}

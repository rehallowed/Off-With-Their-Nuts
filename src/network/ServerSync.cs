using System;
using System.IO;
using BepInEx.Configuration;
using HarmonyLib;
using static OffWithTheirNuts.NetworkConfig;

namespace OffWithTheirNuts {
    internal sealed class ServerSync {
        private readonly ConfigEntry<bool> enabledSetting;
        private readonly Action refreshHotkey;
        private readonly Action onWorldClosed;
        private readonly Action<ZNetPeer, ZDOID, bool> startChange;
        private readonly Action<ZNetPeer> startReset;
        private readonly Action<PlayerNotice> showNotice;
        private readonly Action<ZDOID> playSterilizationEffect;
        private ZNet network;
        private bool canManageWorld;

        internal ZNet CurrentNetwork => network;
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
            Action<PlayerNotice> showNotice,
            Action<ZDOID> playSterilizationEffect
        ) {
            this.enabledSetting = enabledSetting;
            this.refreshHotkey = refreshHotkey;
            this.onWorldClosed = onWorldClosed;
            this.startChange = startChange;
            this.startReset = startReset;
            this.showNotice = showNotice;
            this.playSterilizationEffect = playSterilizationEffect;
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
                    command: NetworkCommand.SetEnabled,
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
                    command: NetworkCommand.Toggle,
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
                SendToServer(NetworkCommand.ResetAll);
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
            bool shouldIgnorePacket = network == null || package == null || package.Size() < 1 ||
                package.Size() > MaxPacketSize || !peer.IsReady();
            if (shouldIgnorePacket) {
                return;
            }

            bool isRegisteredPeer = ReferenceEquals(
                objA: network.GetPeer(peer.m_uid),
                objB: peer
            );
            if (!isRegisteredPeer) {
                return;
            }

            try {
                NetworkCommand command = (NetworkCommand)package.ReadByte();
                int remaining = package.Size() - package.GetPos();
                bool isServer = network.IsServer();
                bool isCurrentServer = !isServer && peer.m_server && ReferenceEquals(
                    objA: network.GetServerPeer(),
                    objB: peer
                );
                if (isServer) {
                    switch (command) {
                        case NetworkCommand.AskSettings when remaining == 0: {
                            SendSettings(peer);
                            break;
                        }
                        case NetworkCommand.SetEnabled when remaining == 1 && IsWorldAdmin(peer): {
                            enabledSetting.Value = package.ReadBool();
                            break;
                        }
                        case NetworkCommand.Toggle when remaining == TargetWireSize + 1: {
                            startChange(
                                arg1: peer,
                                arg2: package.ReadZDOID(),
                                arg3: package.ReadBool()
                            );
                            break;
                        }
                        case NetworkCommand.ResetAll when remaining == 0 && IsWorldAdmin(peer): {
                            startReset(peer);
                            break;
                        }
                    }
                } else if (isCurrentServer) {
                    switch (command) {
                        case NetworkCommand.Settings when remaining == 2: {
                            RestrictionsEnabled = package.ReadBool();
                            canManageWorld = package.ReadBool();
                            SettingsReady = true;
                            refreshHotkey();
                            break;
                        }
                        case NetworkCommand.OwnerWrites when SettingsReady: {
                            NetworkSterilityWrites.ApplyOwnerWrites(
                                network: network,
                                packet: package
                            );
                            break;
                        }
                        case NetworkCommand.LocalizedNotice: {
                            bool wasRead = PlayerNotice.TryRead(
                                package: package,
                                remaining: remaining,
                                notice: out PlayerNotice notice
                            );
                            if (wasRead) {
                                showNotice(notice);
                            }
                            break;
                        }
                        case NetworkCommand.SterilizationEffect when remaining == TargetWireSize: {
                            playSterilizationEffect(package.ReadZDOID());
                            break;
                        }
                    }
                }
            } catch (IOException) {
                // Ignore truncated packets.
            }
        }

        private void SendToServer(NetworkCommand command, Action<ZPackage> fill = null) {
            ZNetPeer server = network?.GetServerPeer();
            if (server != null && server.IsReady()) {
                Send(
                    peer: server,
                    command: command,
                    fill: fill
                );
            }
        }

        private static void Send(ZNetPeer peer, NetworkCommand command, Action<ZPackage> fill = null) {
            var payload = new ZPackage();
            payload.Write((byte)command);
            fill?.Invoke(payload);
            peer.m_rpc.Invoke(
                method: RpcName,
                parameters: payload
            );
        }

        internal bool IsWorldAdmin(ZNetPeer peer) {
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
                command: NetworkCommand.Settings,
                fill: package => {
                    package.Write(RestrictionsEnabled);
                    package.Write(IsWorldAdmin(peer));
                }
            );
        }

        internal static ZDO GetRequestingPlayer(ZNetPeer requester) {
            ZDO player = ZDOMan.instance.GetZDO(requester.m_characterID);
            return player != null && player.GetOwner() == requester.m_uid ? player : null;
        }

        internal void PublishSterilizationEffect(ZDOID id) {
            if (network == null || !network.IsServer()) {
                return;
            }

            foreach (ZNetPeer peer in network.GetPeers()) {
                if (peer.IsReady()) {
                    Send(
                        peer: peer,
                        command: NetworkCommand.SterilizationEffect,
                        fill: package => package.Write(id)
                    );
                }
            }
            playSterilizationEffect(id);
        }

        internal void Notify(ZNetPeer peer, PlayerNoticeKind kind) {
            Notify(
                peer: peer,
                notice: new PlayerNotice(kind)
            );
        }

        internal void Notify(ZNetPeer peer, PlayerNotice notice) {
            if (peer == null) {
                showNotice(notice);
            } else if (peer.IsReady()) {
                Send(
                    peer: peer,
                    command: NetworkCommand.LocalizedNotice,
                    fill: notice.Write
                );
            }
        }

        private bool IsCurrentNetwork(ZNet current) {
            return ReferenceEquals(
                objA: network,
                objB: current
            );
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
                if (sync != null && sync.IsCurrentNetwork(__instance)) {
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
                if (sync != null && sync.IsCurrentNetwork(__instance)) {
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
                if (sync == null || !sync.IsCurrentNetwork(__instance) || __instance.IsServer()) {
                    return;
                }
                ZNetPeer server = __instance.GetServerPeer();
                bool isServerRpc = server != null && ReferenceEquals(
                    objA: server.m_rpc,
                    objB: __0
                );
                if (isServerRpc) {
                    Send(
                        peer: server,
                        command: NetworkCommand.AskSettings
                    );
                }
            }
        }
    }
}

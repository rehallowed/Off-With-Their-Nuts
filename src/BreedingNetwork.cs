using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using HarmonyLib;
using UnityEngine;

namespace OffWithTheirNuts
{
    public sealed partial class OffWithTheirNutsPlugin
    {
        private const string RpcName = PluginId + ".control";
        private const int MaxBatch = 128;
        private const int TargetWireSize = 12;
        private const int MaxPacketSize = 4096;
        private const double ChangeTimeout = 8.0;
        private const double ResendInterval = 0.65;

        private ZNet network;
        private bool settingsReady;
        private bool restrictionsEnabled;
        private bool canManageWorld;
        private bool resetting;
        private readonly HashSet<ZDOID> individualChanges = new HashSet<ZDOID>();

        private enum Command : byte
        {
            AskSettings = 1,
            Settings = 2,
            SetEnabled = 3,
            Toggle = 4,
            ResetAll = 5,
            OwnerWrites = 6,
            Notice = 7
        }

        private struct TargetWrite
        {
            internal readonly ZDOID Id;
            internal long LastOwner;
            internal ushort LastRevision;
            internal double ResendAt;

            internal TargetWrite(ZDOID id)
            {
                this = default;
                Id = id;
            }
        }

        private sealed class ChangeResult
        {
            internal int Changed;
            internal int Unconfirmed;
        }

        private void OpenWorld(ZNet current)
        {
            CloseWorld();
            network = current;
            settingsReady = current.IsServer();
            restrictionsEnabled = settingsReady && enabledSetting.Value;
            UpdateInput();
        }

        private void CloseWorld()
        {
            network = null;
            settingsReady = false;
            restrictionsEnabled = false;
            canManageWorld = false;
            resetting = false;
            StopAllCoroutines();
            individualChanges.Clear();
            DisposeInput();
        }

        private void RegisterPeer(ZNetPeer peer)
        {
            peer.m_rpc.Register<ZPackage>(RpcName, (rpc, package) => Receive(peer, package));
        }

        private void Receive(ZNetPeer peer, ZPackage package)
        {
            if (network == null || package == null || package.Size() < 1 || package.Size() > MaxPacketSize ||
                !peer.IsReady() || !ReferenceEquals(network.GetPeer(peer.m_uid), peer))
                return;

            try
            {
                Command command = (Command)package.ReadByte();
                int remaining = package.Size() - package.GetPos();
                if (network.IsServer())
                {
                    switch (command)
                    {
                        case Command.AskSettings when remaining == 0:
                            SendSettings(peer);
                            break;
                        case Command.SetEnabled when remaining == 1 && CanAdminister(peer):
                            enabledSetting.Value = package.ReadBool();
                            break;
                        case Command.Toggle when remaining == TargetWireSize + 1:
                            StartChange(peer, package.ReadZDOID(), package.ReadBool());
                            break;
                        case Command.ResetAll when remaining == 0 && CanAdminister(peer):
                            StartReset(peer);
                            break;
                    }
                }
                else if (peer.m_server && ReferenceEquals(network.GetServerPeer(), peer))
                {
                    switch (command)
                    {
                        case Command.Settings when remaining == 2:
                            restrictionsEnabled = package.ReadBool();
                            canManageWorld = package.ReadBool();
                            settingsReady = true;
                            UpdateInput();
                            break;
                        case Command.OwnerWrites when settingsReady:
                            ApplyOwnerWrites(package);
                            break;
                        case Command.Notice when remaining > 0:
                            ShowNotice(package.ReadString());
                            break;
                    }
                }
            }
            catch (IOException)
            {
                // Ignore truncated packets.
            }
        }

        private void SendToServer(Command command, Action<ZPackage> fill = null)
        {
            ZNetPeer server = network?.GetServerPeer();
            if (server != null && server.IsReady())
                Send(server, command, fill);
        }

        private static void Send(ZNetPeer peer, Command command, Action<ZPackage> fill = null)
        {
            var payload = new ZPackage();
            payload.Write((byte)command);
            fill?.Invoke(payload);
            peer.m_rpc.Invoke(RpcName, payload);
        }

        private bool CanAdminister(ZNetPeer peer)
        {
            return network.IsServer() && (peer == null || network.IsAdmin(peer.m_socket.GetHostName()));
        }

        private void PublishSettings()
        {
            foreach (ZNetPeer peer in network.GetPeers())
            {
                if (peer.IsReady())
                    SendSettings(peer);
            }
        }

        private void SendSettings(ZNetPeer peer)
        {
            Send(peer, Command.Settings, package =>
            {
                package.Write(restrictionsEnabled);
                package.Write(CanAdminister(peer));
            });
        }

        private void StartChange(ZNetPeer requester, ZDOID id, bool requestedState)
        {
            if (!RestrictionsActive)
                return;
            if (resetting || individualChanges.Contains(id))
            {
                Notify(requester, "Изменение уже выполняется. Повтори после завершения.");
                return;
            }

            ZDO target = ZDOMan.instance?.GetZDO(id);
            if (target == null || !CanChangeTarget(requester, target))
            {
                Notify(requester, "Подойди к приручённому животному, которое может размножаться.");
                return;
            }
            if (IsSterile(target) == requestedState)
            {
                Notify(requester, requestedState ? "Животное уже стерилизовано." : "Размножение уже разрешено.");
                return;
            }

            individualChanges.Add(id);
            StartCoroutine(ChangeAnimal(requester, id, requestedState));
        }

        private static bool CanChangeTarget(ZNetPeer requester, ZDO target)
        {
            GameObject targetObject = ZNetScene.instance?.FindInstance(target.m_uid);
            if (requester == null)
            {
                Player local = Player.m_localPlayer;
                Character hovered = local != null ? local.GetHoverCreature() : null;
                return hovered != null && hovered.gameObject == targetObject && CanSterilize(targetObject);
            }

            ZDO player = ZDOMan.instance.GetZDO(requester.m_characterID);
            if (player == null || player.GetOwner() != requester.m_uid)
                return false;

            ZNetScene scene = ZNetScene.instance;
            if (scene == null)
                return false;
            GameObject playerPrefab = scene.GetPrefab(player.GetPrefab());
            Player playerTemplate = playerPrefab != null ? playerPrefab.GetComponent<Player>() : null;
            if (playerTemplate == null)
                return false;
            GameObject targetPrefab = scene.GetPrefab(target.GetPrefab());
            Character targetTemplate = targetPrefab != null ? targetPrefab.GetComponent<Character>() : null;
            if (targetTemplate == null)
                return false;

            if (targetObject != null)
            {
                if (!CanSterilize(targetObject))
                    return false;
            }
            else if (!target.GetBool(ZDOVars.s_tamed, false) || targetPrefab.GetComponent<Tameable>() == null ||
                targetPrefab.GetComponent<Procreation>() == null || targetPrefab.GetComponent<Growup>() != null)
            {
                return false;
            }

            float distance = playerTemplate.m_maxInteractDistance + Mathf.Max(0f, targetTemplate.GetHoverOffset());
            Vector3 playerPosition = player.GetPosition();
            CapsuleCollider collider = targetObject != null ? targetObject.GetComponent<CapsuleCollider>() : null;
            if (collider != null && collider.enabled)
                return (playerPosition - collider.ClosestPoint(playerPosition)).sqrMagnitude <= distance * distance;

            CapsuleCollider prefabCollider = targetPrefab.GetComponent<CapsuleCollider>();
            if (prefabCollider != null)
            {
                Vector3 scale = targetPrefab.transform.localScale;
                distance += prefabCollider.radius * Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            }
            return (playerPosition - target.GetPosition()).sqrMagnitude <= distance * distance;
        }

        private IEnumerator ChangeAnimal(ZNetPeer requester, ZDOID id, bool requestedState)
        {
            var result = new ChangeResult();
            try
            {
                yield return ApplyChanges(new List<TargetWrite>(1) { new TargetWrite(id) },
                    requestedState, true, result);
                string message = "Не удалось подтвердить изменение для животного.";
                if (result.Changed == 1)
                    message = requestedState ? "Животное стерилизовано." : "Стерилизация снята.";
                Notify(requester, message);
            }
            finally
            {
                individualChanges.Remove(id);
            }
        }

        private void StartReset(ZNetPeer requester)
        {
            if (network == null || !CanAdminister(requester) || ZDOMan.instance == null)
                return;
            if (resetting)
            {
                Notify(requester, "Сброс стерилизации уже выполняется.");
                return;
            }

            resetting = true;
            var duration = Stopwatch.StartNew();
            Notify(requester, "Снимаем стерилизацию с животных…");
            StartCoroutine(ResetAnimals(requester, duration));
        }

        private IEnumerator ResetAnimals(ZNetPeer requester, Stopwatch duration)
        {
            try
            {
                // The first yield reaches the next frame. The second lets it finish, giving
                // the HUD a chance to show the queued message before the scan blocks the main thread.
                yield return null;
                yield return null;
                while (individualChanges.Count > 0)
                    yield return null;

                var scan = Stopwatch.StartNew();
                var marked = new List<TargetWrite>();
                int scanned = 0;
                foreach (ZDO zdo in worldObjects(ZDOMan.instance).Values)
                {
                    scanned++;
                    if (IsSterile(zdo))
                        marked.Add(new TargetWrite(zdo.m_uid));
                }
                scan.Stop();

                var result = new ChangeResult();
                yield return ApplyChanges(marked, false, false, result);
                duration.Stop();
                string animalWord = result.Changed % 10 == 1 && result.Changed % 100 != 11
                    ? "животного" : "животных";
                string message = string.Format(CultureInfo.GetCultureInfo("ru-RU"),
                    "Стерилизация снята у {0} {1} за {2:0.00} с.", result.Changed, animalWord,
                    duration.Elapsed.TotalSeconds);
                if (result.Unconfirmed > 0)
                    message += " Не подтверждено снятие у " + result.Unconfirmed + ".";
                Notify(requester, message);
                Logger.LogInfo(string.Format(CultureInfo.InvariantCulture,
                    "Сброс стерилизации. Записей ZDO {0}, обход {1:F1} мс, " +
                    "снято {2}, не подтверждено {3}, всего {4:F1} мс.",
                    scanned, scan.Elapsed.TotalMilliseconds, result.Changed, result.Unconfirmed,
                    duration.Elapsed.TotalMilliseconds));
            }
            finally
            {
                resetting = false;
            }
        }

        private IEnumerator ApplyChanges(List<TargetWrite> remaining, bool requestedState, bool playEffects,
            ChangeResult result)
        {
            if (remaining.Count == 0)
                yield break;

            double timeoutAt = Time.realtimeSinceStartupAsDouble + ChangeTimeout;
            double expiresAt = network.GetTimeSeconds() + ChangeTimeout;
            var wait = new WaitForSecondsRealtime(0.10f);
            var byOwner = new Dictionary<ZNetPeer, List<TargetWrite>>();
            while (remaining.Count > 0)
            {
                double now = Time.realtimeSinceStartupAsDouble;
                for (int i = remaining.Count - 1; i >= 0; i--)
                {
                    TargetWrite write = remaining[i];
                    ZDO zdo = ZDOMan.instance.GetZDO(write.Id);
                    if (zdo == null)
                    {
                        result.Unconfirmed++;
                        remaining.RemoveAt(i);
                    }
                    else if (IsSterile(zdo) == requestedState)
                    {
                        result.Changed++;
                        remaining.RemoveAt(i);
                    }
                    else if (zdo.IsOwner() || !zdo.HasOwner())
                    {
                        SetSterility(zdo, requestedState, playEffects);
                        result.Changed++;
                        remaining.RemoveAt(i);
                    }
                    else
                    {
                        long owner = zdo.GetOwner();
                        ushort revision = zdo.OwnerRevision;
                        if (write.LastOwner == owner && write.LastRevision == revision && now < write.ResendAt)
                            continue;

                        ZNetPeer peer = network.GetPeer(owner);
                        if (peer == null || !peer.IsReady())
                            continue;

                        write.LastOwner = owner;
                        write.LastRevision = revision;
                        write.ResendAt = now + ResendInterval;
                        remaining[i] = write;
                        if (!byOwner.TryGetValue(peer, out List<TargetWrite> queued))
                        {
                            queued = new List<TargetWrite>();
                            byOwner.Add(peer, queued);
                        }
                        queued.Add(write);
                    }
                }

                foreach (KeyValuePair<ZNetPeer, List<TargetWrite>> batch in byOwner)
                {
                    if (batch.Value.Count != 0)
                    {
                        SendOwnerWrites(batch.Key, batch.Value, requestedState, playEffects, expiresAt);
                        batch.Value.Clear();
                    }
                }

                if (remaining.Count == 0 || now >= timeoutAt)
                    break;
                yield return wait;
            }
            result.Unconfirmed += remaining.Count;
        }

        private static void SendOwnerWrites(ZNetPeer peer, List<TargetWrite> writes,
            bool requestedState, bool playEffects, double expiresAt)
        {
            for (int start = 0; start < writes.Count; start += MaxBatch)
            {
                int count = Math.Min(MaxBatch, writes.Count - start);
                var packet = new ZPackage();
                packet.Write((byte)Command.OwnerWrites);
                packet.Write(requestedState);
                packet.Write(playEffects);
                packet.Write(expiresAt);
                packet.Write(count);
                for (int i = start; i < start + count; i++)
                {
                    packet.Write(writes[i].Id);
                    packet.Write(writes[i].LastRevision);
                }
                peer.m_rpc.Invoke(RpcName, packet);
            }
        }

        private void ApplyOwnerWrites(ZPackage packet)
        {
            const int headerSize = 2 + sizeof(double) + sizeof(int);
            if (packet.Size() - packet.GetPos() < headerSize)
                return;

            bool requestedState = packet.ReadBool();
            bool playEffects = packet.ReadBool();
            double expiresAt = packet.ReadDouble();
            int count = packet.ReadInt();
            if (count < 1 || count > MaxBatch || ZDOMan.instance == null ||
                network.GetTimeSeconds() > expiresAt ||
                packet.Size() - packet.GetPos() != count * (TargetWireSize + sizeof(ushort)))
                return;

            for (int i = 0; i < count; i++)
            {
                ZDOID id = packet.ReadZDOID();
                ushort expectedRevision = packet.ReadUShort();
                ZDO zdo = ZDOMan.instance.GetZDO(id);
                if (zdo == null || !zdo.IsOwner() || zdo.OwnerRevision != expectedRevision)
                    continue;

                SetSterility(zdo, requestedState, playEffects);
                ZDOMan.instance.ForceSendZDO(id);
            }
        }

        private void SetSterility(ZDO zdo, bool requestedState, bool playEffects)
        {
            if (IsSterile(zdo) == requestedState)
                return;

            if (requestedState)
                ClearPregnancy(zdo);
            zdo.Set(SterileKey, requestedState);
            if (playEffects)
                PlayBirthEffects(ZNetScene.instance?.FindInstance(zdo.m_uid));
        }

        private static void Notify(ZNetPeer peer, string message)
        {
            if (peer == null)
                ShowNotice(message);
            else if (peer.IsReady())
                Send(peer, Command.Notice, package => package.Write(message));
        }

        private static void ShowNotice(string message)
        {
            MessageHud hud = MessageHud.instance;
            if (hud != null)
                hud.ShowMessage(MessageHud.MessageType.TopLeft, message, 0, null, false, false);
        }

        [HarmonyPatch(typeof(ZNet), "Awake")]
        private static class WorldAwakePatch
        {
            private static void Postfix(ZNet __instance)
            {
                instance?.OpenWorld(__instance);
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnDestroy")]
        private static class WorldDestroyPatch
        {
            private static void Prefix(ZNet __instance)
            {
                OffWithTheirNutsPlugin mod = instance;
                if (mod != null && ReferenceEquals(mod.network, __instance))
                    mod.CloseWorld();
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnNewConnection")]
        private static class PeerConnectionPatch
        {
            private static void Postfix(ZNet __instance, ZNetPeer __0)
            {
                OffWithTheirNutsPlugin mod = instance;
                if (mod != null && ReferenceEquals(mod.network, __instance))
                    mod.RegisterPeer(__0);
            }
        }

        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        private static class PeerInfoPatch
        {
            private static void Postfix(ZNet __instance, ZRpc __0)
            {
                OffWithTheirNutsPlugin mod = instance;
                if (mod == null || !ReferenceEquals(mod.network, __instance) || __instance.IsServer())
                    return;
                ZNetPeer server = __instance.GetServerPeer();
                if (server != null && ReferenceEquals(server.m_rpc, __0))
                    Send(server, Command.AskSettings);
            }
        }
    }
}

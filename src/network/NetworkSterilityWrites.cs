using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static OffWithTheirNuts.NetworkConfig;

namespace OffWithTheirNuts {
    internal static class NetworkSterilityWrites {
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

        internal static IEnumerator ApplyChanges(
            ZNet network,
            List<TargetWrite> remaining,
            bool requestedState,
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
                ProcessPendingWrites(
                    network: network,
                    remaining: remaining,
                    requestedState: requestedState,
                    result: result,
                    byOwner: byOwner,
                    now: now
                );
                SendQueuedWrites(
                    byOwner: byOwner,
                    requestedState: requestedState,
                    expiresAt: expiresAt
                );

                if (remaining.Count == 0 || now >= timeoutAt) {
                    break;
                }
                yield return wait;
            }
            result.Unconfirmed += remaining.Count;
        }

        private static void ProcessPendingWrites(
            ZNet network,
            List<TargetWrite> remaining,
            bool requestedState,
            ChangeResult result,
            Dictionary<ZNetPeer, List<TargetWrite>> byOwner,
            double now
        ) {
            for (int i = remaining.Count - 1; i >= 0; i--) {
                TargetWrite write = remaining[i];
                ZDO zdo = ZDOMan.instance.GetZDO(write.Id);
                if (zdo == null) {
                    result.Unconfirmed++;
                    remaining.RemoveAt(i);
                    continue;
                }

                bool stateMatches = ZdoSterilityOperations.IsSterile(zdo) == requestedState;
                if (!stateMatches && !zdo.IsOwner() && zdo.HasOwner()) {
                    if (TryQueueOwnerWrite(
                        network: network,
                        zdo: zdo,
                        write: ref write,
                        byOwner: byOwner,
                        now: now
                    )) {
                        remaining[i] = write;
                    }
                    continue;
                }

                if (!stateMatches) {
                    ZdoSterilityOperations.SetSterility(
                        zdo: zdo,
                        requestedState: requestedState
                    );
                }
                result.Changed++;
                remaining.RemoveAt(i);
            }
        }

        private static bool TryQueueOwnerWrite(
            ZNet network,
            ZDO zdo,
            ref TargetWrite write,
            Dictionary<ZNetPeer, List<TargetWrite>> byOwner,
            double now
        ) {
            long owner = zdo.GetOwner();
            ushort revision = zdo.OwnerRevision;
            if (write.LastOwner == owner && write.LastRevision == revision && now < write.ResendAt) {
                return false;
            }

            ZNetPeer peer = network.GetPeer(owner);
            if (peer == null || !peer.IsReady()) {
                return false;
            }

            write.LastOwner = owner;
            write.LastRevision = revision;
            write.ResendAt = now + ResendInterval;
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
            return true;
        }

        private static void SendQueuedWrites(
            Dictionary<ZNetPeer, List<TargetWrite>> byOwner,
            bool requestedState,
            double expiresAt
        ) {
            foreach (KeyValuePair<ZNetPeer, List<TargetWrite>> batch in byOwner) {
                if (batch.Value.Count == 0) {
                    continue;
                }

                SendOwnerWrites(
                    peer: batch.Key,
                    writes: batch.Value,
                    requestedState: requestedState,
                    expiresAt: expiresAt
                );
                batch.Value.Clear();
            }
        }

        private static void SendOwnerWrites(
            ZNetPeer peer,
            List<TargetWrite> writes,
            bool requestedState,
            double expiresAt
        ) {
            for (int start = 0; start < writes.Count; start += MaxBatch) {
                int count = Math.Min(
                    val1: MaxBatch,
                    val2: writes.Count - start
                );
                var packet = new ZPackage();
                packet.Write((byte)NetworkCommand.OwnerWrites);
                packet.Write(requestedState);
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

        internal static void ApplyOwnerWrites(ZNet network, ZPackage packet) {
            const int HeaderSize = sizeof(bool) + sizeof(double) + sizeof(int);
            if (packet.Size() - packet.GetPos() < HeaderSize) {
                return;
            }

            bool requestedState = packet.ReadBool();
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

                ZdoSterilityOperations.SetSterility(
                    zdo: zdo,
                    requestedState: requestedState
                );
                ZDOMan.instance.ForceSendZDO(id);
            }
        }
    }
}

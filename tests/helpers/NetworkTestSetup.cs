using System;
using System.Reflection;
using BepInEx.Configuration;

namespace OffWithTheirNuts.Tests.Helpers {
    internal static class NetworkTestSetup {
        internal static ServerSync CreateSync(
            ConfigEntry<bool> irreversibleSetting = null,
            Action<ZNetPeer> startReset = null
        ) {
            return new ServerSync(
                enabledSetting: new ConfigEntry<bool> { Value = true },
                irreversibleSterilizationSetting: irreversibleSetting ?? new ConfigEntry<bool>(),
                refreshHotkey: () => { },
                onWorldClosed: () => { },
                startChange: null,
                startReset: startReset,
                showNotice: null,
                playSterilityChangeEffect: null
            );
        }

        internal static void OpenWorld(ServerSync sync, ZNet world) {
            const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(ServerSync).GetMethod(
                name: "OpenWorld",
                bindingAttr: PrivateInstance
            ).Invoke(
                obj: sync,
                parameters: new object[] { world }
            );
            MethodInfo registerPeer = typeof(ServerSync).GetMethod(
                name: "RegisterPeer",
                bindingAttr: PrivateInstance
            );
            foreach (ZNetPeer peer in world.GetPeers()) {
                registerPeer.Invoke(
                    obj: sync,
                    parameters: new object[] { peer }
                );
                Expect.Equal(
                    scenario: "Handler is registered under the mod's RPC name",
                    expected: NetworkConfig.RpcName,
                    actual: peer.m_rpc.RegisteredName
                );
            }
        }

        internal static ZDO AddSterilizedRecord(uint id, long owner = 0) {
            var record = new ZDO {
                m_uid = new ZDOID(
                    userID: 1L,
                    id: id
                ),
                Owner = owner
            };
            ZDOMan.instance.Records.Add(
                key: record.m_uid,
                value: record
            );
            ZdoSterilityOperations.SetSterility(
                zdo: record,
                requestedState: true
            );
            return record;
        }

        internal static ZDOMan CreateRecordReplica(ZDO record, long localOwner) {
            var manager = new ZDOMan { LocalOwner = localOwner };
            var replica = new ZDO {
                m_uid = record.m_uid,
                Owner = record.Owner,
                OwnerRevision = record.OwnerRevision
            };
            ZdoSterilityOperations.SetSterility(
                zdo: replica,
                requestedState: ZdoSterilityOperations.IsSterile(record)
            );
            manager.Records.Add(
                key: replica.m_uid,
                value: replica
            );
            return manager;
        }

        internal static void DeliverOwnerWrites(ZDOMan receiver, ZPackage sent) {
            var packet = new ZPackage(sent.GetArray());
            Expect.Equal(
                scenario: "The peer receives an owner write batch",
                expected: NetworkCommand.OwnerWrites,
                actual: (NetworkCommand)packet.ReadByte()
            );

            ZDOMan previous = ZDOMan.instance;
            try {
                ZDOMan.instance = receiver;
                NetworkSterilityWrites.ApplyOwnerWrites(
                    network: new ZNet(),
                    packet: packet
                );
            } finally {
                ZDOMan.instance = previous;
            }
        }
    }
}

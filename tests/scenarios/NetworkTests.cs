using System.Collections;
using System.Collections.Generic;
using BepInEx.Configuration;
using OffWithTheirNuts.Tests.Helpers;
using UnityEngine;
using static OffWithTheirNuts.Tests.Helpers.NetworkTestSetup;

namespace OffWithTheirNuts.Tests.Scenarios {
    internal static class NetworkTests {
        internal static void CheckSettingsAndPermissions() {
            var guest = new ZNetPeer { m_uid = 2L };
            guest.m_socket.HostName = "guest";
            var admin = new ZNetPeer { m_uid = 3L };
            admin.m_socket.HostName = "admin";
            var host = new ZNet { Host = true };
            host.Admins.Add(admin.m_socket.HostName);
            host.Peers.Add(
                key: guest.m_uid,
                value: guest
            );
            host.Peers.Add(
                key: admin.m_uid,
                value: admin
            );

            var resetRequests = new List<ZNetPeer>();
            var irreversible = new ConfigEntry<bool>();
            ServerSync sync = CreateSync(
                irreversibleSetting: irreversible,
                startReset: resetRequests.Add
            );
            OpenWorld(
                sync: sync,
                world: host
            );
            var reset = new ZPackage();
            reset.Write((byte)NetworkCommand.ResetAll);
            guest.m_rpc.Deliver(reset);
            admin.m_rpc.Deliver(reset);
            Expect.Equal(
                scenario: "Only the admin's reset request is accepted",
                expected: 1,
                actual: resetRequests.Count
            );
            Expect.Equal(
                scenario: "Reset is attributed to the admin",
                expected: admin,
                actual: resetRequests[0]
            );

            var enableIrreversibility = new ZPackage();
            enableIrreversibility.Write((byte)NetworkCommand.SetIrreversibleSterilization);
            enableIrreversibility.Write(true);
            guest.m_rpc.Deliver(enableIrreversibility);
            Expect.True(
                scenario: "A guest cannot enable irreversible sterilization",
                condition: !irreversible.Value
            );
            admin.m_rpc.Deliver(enableIrreversibility);
            sync.RefreshWorldSettings();
            Expect.True(
                scenario: "An admin can enable irreversible sterilization",
                condition: irreversible.Value
            );

            var server = new ZNetPeer { m_uid = 1L, m_server = true };
            var client = new ZNet { Server = server };
            client.Peers.Add(
                key: server.m_uid,
                value: server
            );
            ServerSync clientSync = CreateSync();
            OpenWorld(
                sync: clientSync,
                world: client
            );
            server.m_rpc.Deliver(guest.m_rpc.Sent[0]);
            Expect.True(
                scenario: "The guest receives server settings without admin access",
                condition: clientSync.RestrictionsActive && clientSync.IsIrreversibleSterilizationEnabled &&
                    !clientSync.CanReset
            );
            server.m_rpc.Deliver(admin.m_rpc.Sent[0]);
            Expect.True(
                scenario: "The admin receives permission to reset",
                condition: clientSync.CanReset
            );
            clientSync.CloseWorld();
            Expect.True(
                scenario: "Closing the world clears synchronized state",
                condition: !clientSync.SettingsReady && !clientSync.CanReset
            );
        }

        internal static void CheckNoticePackets() {
            var original = new PlayerNotice(
                count: 42,
                seconds: 1.25,
                unconfirmed: 7
            );
            var outgoing = new ZPackage();
            original.Write(outgoing);
            var incoming = new ZPackage(outgoing.GetArray());
            bool wasRead = PlayerNotice.TryRead(
                package: incoming,
                remaining: incoming.Size(),
                notice: out PlayerNotice decoded
            );
            TextResources resources = LocalizationCore.ForLanguage("English");
            Expect.True(
                scenario: "A reset result round-trips through the game's ZPackage",
                condition: wasRead && decoded.Format(resources) == original.Format(resources)
            );

            var truncated = new ZPackage(new[] { (byte)PlayerNoticeKind.ResetResult });
            bool wasTruncatedRead = PlayerNotice.TryRead(
                package: truncated,
                remaining: truncated.Size(),
                notice: out _
            );
            Expect.True(
                scenario: "A reset result with no payload is rejected",
                condition: !wasTruncatedRead
            );
        }

        internal static void CheckToggleRoutes() {
            ZDOMan.instance = new ZDOMan();
            ZNetScene.instance = new ZNetScene();
            ZDO record = AddSterilizedRecord(1);
            var animal = new Tameable();
            animal.gameObject.Add(animal);
            animal.gameObject.Add(new Procreation());
            animal.gameObject.Add(new ZNetView { Record = record });
            ZNetScene.instance.Instances.Add(
                key: record.m_uid,
                value: animal.gameObject
            );
            Player.m_localPlayer = new Player { Hovered = animal };

            var server = new ZNetPeer { m_uid = 1L, m_server = true };
            var world = new ZNet { Host = true, Server = server };
            world.Peers.Add(
                key: server.m_uid,
                value: server
            );
            ServerSync sync = CreateSync();
            OpenWorld(
                sync: sync,
                world: world
            );
            var hostMod = new OffWithTheirNutsPlugin(serverSync: sync);
            hostMod.PressShortcut(animal);
            Expect.True(
                scenario: "Solo and host start the change locally",
                condition: hostMod.StartedCoroutine != null && server.m_rpc.Sent.Count == 0
            );

            world.Host = false;
            OpenWorld(
                sync: sync,
                world: world
            );
            var settings = new ZPackage();
            settings.Write((byte)NetworkCommand.Settings);
            settings.Write(true);
            settings.Write(false);
            settings.Write(false);
            server.m_rpc.Deliver(settings);
            var clientMod = new OffWithTheirNutsPlugin(serverSync: sync);
            clientMod.PressShortcut(animal);
            Expect.True(
                scenario: "The remote client sends one toggle and starts no local change",
                condition: clientMod.StartedCoroutine == null && server.m_rpc.Sent.Count == 1
            );
            ZPackage toggle = server.m_rpc.Sent[0];
            Expect.Equal(
                scenario: "The toggle targets the selected animal and requests the opposite state",
                expected: (NetworkCommand.Toggle, record.m_uid, false),
                actual: ((NetworkCommand)toggle.ReadByte(), toggle.ReadZDOID(), toggle.ReadBool())
            );
            clientMod.PressShortcut(new Character());
            sync.CloseWorld();
            clientMod.PressShortcut(animal);
            Expect.Equal(
                scenario: "Invalid targets and closed worlds send nothing",
                expected: 1,
                actual: server.m_rpc.Sent.Count
            );
        }

        internal static void CheckOwnershipTransfer() {
            ZDOMan.instance = new ZDOMan();
            Time.realtimeSinceStartupAsDouble = 0;
            var previousOwner = new ZNetPeer { m_uid = 2L };
            var currentOwner = new ZNetPeer { m_uid = 3L };
            var server = new ZNet { Host = true };
            server.Peers.Add(
                key: previousOwner.m_uid,
                value: previousOwner
            );
            server.Peers.Add(
                key: currentOwner.m_uid,
                value: currentOwner
            );

            ZDO record = AddSterilizedRecord(
                id: 1,
                owner: previousOwner.m_uid
            );
            record.OwnerRevision = 1;
            var targets = new List<NetworkSterilityWrites.TargetWrite> {
                new NetworkSterilityWrites.TargetWrite(record.m_uid)
            };
            var result = new NetworkSterilityWrites.ChangeResult();
            IEnumerator reset = NetworkSterilityWrites.ApplyChanges(
                network: server,
                remaining: targets,
                requestedState: false,
                result: result
            );
            Expect.True(
                scenario: "The server sends the first write to the original owner and waits",
                condition: reset.MoveNext() && previousOwner.m_rpc.Sent.Count == 1 &&
                    currentOwner.m_rpc.Sent.Count == 0 && result.Changed == 0
            );

            record.Owner = currentOwner.m_uid;
            record.OwnerRevision++;
            Time.realtimeSinceStartupAsDouble = NetworkConfig.ResendInterval / 2;
            Expect.True(
                scenario: "Ownership transfer reroutes the write before the resend interval",
                condition: reset.MoveNext() && previousOwner.m_rpc.Sent.Count == 1 &&
                    currentOwner.m_rpc.Sent.Count == 1 && result.Changed == 0
            );

            ZDOMan previousReplica = CreateRecordReplica(
                record: record,
                localOwner: previousOwner.m_uid
            );
            DeliverOwnerWrites(
                receiver: previousReplica,
                sent: previousOwner.m_rpc.Sent[0]
            );
            Expect.True(
                scenario: "The former owner leaves the record unchanged",
                condition: ZdoSterilityOperations.IsSterile(previousReplica.GetZDO(record.m_uid)) &&
                    previousReplica.ForcedSends.Count == 0
            );

            ZDOMan currentReplica = CreateRecordReplica(
                record: record,
                localOwner: currentOwner.m_uid
            );
            ZDO currentRecord = currentReplica.GetZDO(record.m_uid);
            DeliverOwnerWrites(
                receiver: currentReplica,
                sent: previousOwner.m_rpc.Sent[0]
            );
            Expect.True(
                scenario: "The current owner refuses a write with a stale ownership revision",
                condition: ZdoSterilityOperations.IsSterile(currentRecord) && currentReplica.ForcedSends.Count == 0
            );
            DeliverOwnerWrites(
                receiver: currentReplica,
                sent: currentOwner.m_rpc.Sent[0]
            );
            Expect.True(
                scenario: "The current owner applies the write and sends the updated record",
                condition: !ZdoSterilityOperations.IsSterile(currentRecord) &&
                    currentReplica.ForcedSends.Count == 1 && currentReplica.ForcedSends[0] == record.m_uid
            );

            Time.realtimeSinceStartupAsDouble += 0.1;
            Expect.True(
                scenario: "The server still waits before the updated record arrives",
                condition: reset.MoveNext() && result.Changed == 0 && ZdoSterilityOperations.IsSterile(record)
            );
            // Simulate replication of the record the current owner sent.
            ZdoSterilityOperations.SetSterility(
                zdo: record,
                requestedState: ZdoSterilityOperations.IsSterile(currentRecord)
            );
            Time.realtimeSinceStartupAsDouble += 0.1;
            Expect.True(
                scenario: "The server confirms the reset after observing the replicated state",
                condition: !reset.MoveNext() && result.Changed == 1 && result.Unconfirmed == 0
            );
        }

        internal static void CheckResetDiagnostics() {
            ZDOMan.instance = new ZDOMan();
            Time.realtimeSinceStartupAsDouble = 0;
            ZDO local = AddSterilizedRecord(1);
            ZDO missing = AddSterilizedRecord(
                id: 2,
                owner: 42
            );
            ZDO pending = AddSterilizedRecord(
                id: 3,
                owner: 42
            );
            var targets = new List<NetworkSterilityWrites.TargetWrite> {
                new NetworkSterilityWrites.TargetWrite(local.m_uid),
                new NetworkSterilityWrites.TargetWrite(missing.m_uid),
                new NetworkSterilityWrites.TargetWrite(pending.m_uid)
            };
            var result = new NetworkSterilityWrites.ChangeResult();
            IEnumerator reset = NetworkSterilityWrites.ApplyChanges(
                network: new ZNet { Host = true },
                remaining: targets,
                requestedState: false,
                result: result
            );
            Expect.True(
                scenario: "The local record clears while the owner's records remain pending",
                condition: reset.MoveNext() && !ZdoSterilityOperations.IsSterile(local) && result.Unconfirmed == 0
            );
            ZDOMan.instance.Records.Remove(missing.m_uid);
            Time.realtimeSinceStartupAsDouble = 1;
            Expect.True(
                scenario: "The missing record is counted before the deadline",
                condition: reset.MoveNext() && result.Missing == 1 && result.TimedOut == 0
            );
            Time.realtimeSinceStartupAsDouble = NetworkConfig.ChangeTimeout;
            Expect.True(
                scenario: "The reset ends at the confirmation deadline",
                condition: !reset.MoveNext()
            );
            Expect.Equal(
                scenario: "Mixed reset totals (cleared, missing, timed out, unconfirmed)",
                expected: (1, 1, 1, 2),
                actual: (result.Changed, result.Missing, result.TimedOut, result.Unconfirmed)
            );
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using HarmonyLib;

namespace OffWithTheirNuts {
    public sealed partial class OffWithTheirNutsPlugin {
        private AccessTools.FieldRef<ZDOMan, Dictionary<ZDOID, ZDO>> worldObjects;
        private bool resetting;

        private void StartReset(ZNetPeer requester) {
            if (!serverSync.IsWorldAdmin(requester) || ZDOMan.instance == null) {
                return;
            }
            if (resetting) {
                serverSync.Notify(
                    peer: requester,
                    kind: PlayerNoticeKind.ResetRunning
                );
                return;
            }

            resetting = true;
            var duration = Stopwatch.StartNew();
            serverSync.Notify(
                peer: requester,
                kind: PlayerNoticeKind.ResetStarted
            );
            StartCoroutine(
                ResetAnimals(
                    requester: requester,
                    duration: duration
                )
            );
        }

        private IEnumerator ResetAnimals(ZNetPeer requester, Stopwatch duration) {
            try {
                // The first yield reaches the next frame. The second lets it finish, giving
                // the HUD a chance to show the queued message before the scan blocks the main thread.
                yield return null;
                yield return null;
                while (individualChanges.Count > 0) {
                    yield return null;
                }

                var scan = Stopwatch.StartNew();
                var marked = new List<SterilityWrites.TargetWrite>();
                int scanned = 0;
                foreach (ZDO zdo in worldObjects(ZDOMan.instance).Values) {
                    scanned++;
                    if (IsSterile(zdo)) {
                        marked.Add(new SterilityWrites.TargetWrite(zdo.m_uid));
                    }
                }
                scan.Stop();

                var result = new SterilityWrites.ChangeResult();
                yield return SterilityWrites.ApplyChanges(
                    network: serverSync.CurrentNetwork,
                    remaining: marked,
                    requestedState: false,
                    result: result
                );
                duration.Stop();
                serverSync.Notify(
                    peer: requester,
                    notice: new PlayerNotice(
                        count: result.Changed,
                        seconds: duration.Elapsed.TotalSeconds,
                        unconfirmed: result.Unconfirmed
                    )
                );
                Logger.LogInfo(
                    string.Format(
                        provider: CultureInfo.InvariantCulture,
                        format: "Sterility reset. Scanned {0} ZDO records in {1:F1} ms, " +
                            "cleared {2} flags, {3} unconfirmed, {4:F1} ms total.",
                        args: new object[] {
                            scanned,
                            scan.Elapsed.TotalMilliseconds,
                            result.Changed,
                            result.Unconfirmed,
                            duration.Elapsed.TotalMilliseconds
                        }
                    )
                );
            } finally {
                resetting = false;
            }
        }
    }
}

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
            if (!serverSync.CanAdminister(requester) || ZDOMan.instance == null) {
                return;
            }
            if (resetting) {
                ServerSync.Notify(
                    peer: requester,
                    message: "Сброс стерилизации уже выполняется."
                );
                return;
            }

            resetting = true;
            var duration = Stopwatch.StartNew();
            ServerSync.Notify(
                peer: requester,
                message: "Снимаем стерилизацию с животных…"
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
                var marked = new List<ServerSync.TargetWrite>();
                int scanned = 0;
                foreach (ZDO zdo in worldObjects(ZDOMan.instance).Values) {
                    scanned++;
                    if (IsSterile(zdo)) {
                        marked.Add(new ServerSync.TargetWrite(zdo.m_uid));
                    }
                }
                scan.Stop();

                var result = new ServerSync.ChangeResult();
                yield return serverSync.ApplyChanges(
                    remaining: marked,
                    requestedState: false,
                    playEffects: false,
                    result: result
                );
                duration.Stop();
                string animalWord = result.Changed % 10 == 1 && result.Changed % 100 != 11
                    ? "животного" : "животных";
                string message = string.Format(
                    provider: CultureInfo.GetCultureInfo("ru-RU"),
                    format: "Стерилизация снята у {0} {1} за {2:0.00} с.",
                    arg0: result.Changed,
                    arg1: animalWord,
                    arg2: duration.Elapsed.TotalSeconds
                );
                if (result.Unconfirmed > 0) {
                    message += " Не подтверждено снятие у " + result.Unconfirmed + ".";
                }
                ServerSync.Notify(
                    peer: requester,
                    message: message
                );
                Logger.LogInfo(
                    string.Format(
                        provider: CultureInfo.InvariantCulture,
                        format: "Сброс стерилизации. Записей ZDO {0}, обход {1:F1} мс, " +
                            "снято {2}, не подтверждено {3}, всего {4:F1} мс.",
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

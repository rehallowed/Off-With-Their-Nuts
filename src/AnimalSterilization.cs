using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using static OffWithTheirNuts.Constants;

namespace OffWithTheirNuts {
    public sealed partial class OffWithTheirNutsPlugin {
        private readonly HashSet<ZDOID> individualChanges = new HashSet<ZDOID>();

        private void ToggleTarget(Character target) {
            ZNetView view = target.GetComponent<ZNetView>();
            if (view == null || !view.IsValid() || !CanSterilize(target.gameObject)) {
                return;
            }

            ZDO zdo = view.GetZDO();
            serverSync.RequestChange(
                id: zdo.m_uid,
                requested: !IsSterile(zdo)
            );
        }

        private void StartChange(ZNetPeer requester, ZDOID id, bool requestedState) {
            if (!RestrictionsActive) {
                return;
            }
            if (resetting || individualChanges.Contains(id)) {
                serverSync.Notify(
                    peer: requester,
                    kind: PlayerNoticeKind.ChangePending
                );
                return;
            }

            ZDO target = ZDOMan.instance?.GetZDO(id);
            if (target == null || !ServerSync.CanChangeTarget(
                requester: requester,
                target: target
            )) {
                serverSync.Notify(
                    peer: requester,
                    kind: PlayerNoticeKind.TargetUnavailable
                );
                return;
            }
            if (IsSterile(target) == requestedState) {
                serverSync.Notify(
                    peer: requester,
                    kind: requestedState ? PlayerNoticeKind.AlreadySterile : PlayerNoticeKind.AlreadyFertile
                );
                return;
            }

            individualChanges.Add(id);
            StartCoroutine(
                ChangeAnimal(
                    requester: requester,
                    id: id,
                    requestedState: requestedState
                )
            );
        }

        private IEnumerator ChangeAnimal(ZNetPeer requester, ZDOID id, bool requestedState) {
            var result = new ServerSync.ChangeResult();
            try {
                yield return serverSync.ApplyChanges(
                    remaining: new List<ServerSync.TargetWrite>(1) { new ServerSync.TargetWrite(id) },
                    requestedState: requestedState,
                    result: result
                );
                PlayerNoticeKind notice = PlayerNoticeKind.ChangeUnconfirmed;
                if (result.Changed == 1) {
                    notice = requestedState ? PlayerNoticeKind.Sterilized : PlayerNoticeKind.FertileAgain;
                    serverSync.PublishSterilizationEffect(id);
                }
                serverSync.Notify(
                    peer: requester,
                    kind: notice
                );
            } finally {
                individualChanges.Remove(id);
            }
        }

        internal static bool CanSterilize(GameObject target) {
            Tameable tameable = target.GetComponent<Tameable>();
            return tameable != null && tameable.IsTamed() &&
                target.GetComponent<Procreation>() != null && target.GetComponent<Growup>() == null;
        }

        internal static bool IsSterile(ZDO zdo) {
            return zdo != null && zdo.GetBool(
                hash: SterileKey,
                defaultValue: false
            );
        }

        private static void ClearPregnancy(ZDO zdo) {
            if (zdo.GetLong(
                hash: ZDOVars.s_pregnant,
                defaultValue: 0L
            ) != 0L) {
                zdo.Set(
                    hash: ZDOVars.s_pregnant,
                    value: 0L
                );
            }
            if (zdo.GetInt(
                hash: ZDOVars.s_lovePoints,
                defaultValue: 0
            ) != 0) {
                zdo.Set(
                    hash: ZDOVars.s_lovePoints,
                    value: 0
                );
            }
        }

        private static void SetSterility(ZDO zdo, bool requestedState) {
            if (IsSterile(zdo) == requestedState) {
                return;
            }

            if (requestedState) {
                ClearPregnancy(zdo);
            }
            zdo.Set(
                hash: SterileKey,
                value: requestedState
            );
        }

        [HarmonyPatch(
            declaringType: typeof(Procreation),
            methodName: "Procreate"
        )]
        private static class ProcreatePatch {
            private static bool Prefix(ZNetView ___m_nview) {
                ServerSync sync = instance?.serverSync;
                if (sync == null || !sync.HasWorld) {
                    return true;
                }
                if (!sync.SettingsReady) {
                    return false;
                }
                if (!sync.RestrictionsEnabled) {
                    return true;
                }

                ZDO zdo = ___m_nview?.GetZDO();
                if (!IsSterile(zdo)) {
                    return true;
                }

                if (zdo.IsOwner()) {
                    ClearPregnancy(zdo);
                }
                return false;
            }
        }

        [HarmonyPatch(
            declaringType: typeof(Procreation),
            methodName: "ReadyForProcreation"
        )]
        private static class ReadyForProcreationPatch {
            private static bool Prefix(ref bool __result, ZNetView ___m_nview) {
                ServerSync sync = instance?.serverSync;
                if (sync == null || !sync.HasWorld) {
                    return true;
                }
                if (sync.SettingsReady && (!sync.RestrictionsEnabled || !IsSterile(___m_nview?.GetZDO()))) {
                    return true;
                }

                __result = false;
                return false;
            }
        }
    }
}

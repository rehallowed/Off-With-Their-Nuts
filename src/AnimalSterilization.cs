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
            if (view == null || !view.IsValid() || !IsSterilizable(target.gameObject)) {
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
            if (target == null || !IsSterilityChangeAllowed(
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
            var result = new SterilityWrites.ChangeResult();
            try {
                yield return SterilityWrites.ApplyChanges(
                    network: serverSync.CurrentNetwork,
                    remaining: new List<SterilityWrites.TargetWrite>(1) { new SterilityWrites.TargetWrite(id) },
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

        internal static bool IsSterilizable(GameObject target) {
            Tameable tameable = target.GetComponent<Tameable>();
            return tameable != null && tameable.IsTamed() &&
                target.GetComponent<Procreation>() != null && target.GetComponent<Growup>() == null;
        }

        private static bool IsSterilityChangeAllowed(ZNetPeer requester, ZDO target) {
            GameObject targetObject = ZNetScene.instance?.FindInstance(target.m_uid);
            if (requester == null) {
                Player local = Player.m_localPlayer;
                Character hovered = local != null ? local.GetHoverCreature() : null;
                return hovered != null && hovered.gameObject == targetObject && IsSterilizable(targetObject);
            }

            ZDO player = ServerSync.GetRequestingPlayer(requester);
            if (player == null) {
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
                if (!IsSterilizable(targetObject)) {
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

        internal static void SetSterility(ZDO zdo, bool requestedState) {
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

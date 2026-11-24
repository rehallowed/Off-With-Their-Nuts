using System.Collections;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace OffWithTheirNuts {
    public sealed partial class OffWithTheirNutsPlugin {
        private readonly HashSet<ZDOID> individualChanges = new HashSet<ZDOID>();

        private void ToggleTarget(Character target) {
            ZNetView view = target.GetComponent<ZNetView>();
            if (view == null || !view.IsValid() || !IsSterilizable(target.GetComponent<Tameable>())) {
                return;
            }

            ZDO zdo = view.GetZDO();
            serverSync.RequestChange(
                id: zdo.m_uid,
                requested: !ZdoSterilityOperations.IsSterile(zdo)
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
            bool isChangeAllowed = target != null && IsSterilityChangeAllowed(
                requester: requester,
                target: target
            );
            if (!isChangeAllowed) {
                serverSync.Notify(
                    peer: requester,
                    kind: PlayerNoticeKind.TargetUnavailable
                );
                return;
            }
            if (ZdoSterilityOperations.IsSterile(target) == requestedState) {
                serverSync.Notify(
                    peer: requester,
                    kind: requestedState ? PlayerNoticeKind.AlreadySterile : PlayerNoticeKind.AlreadyFertile
                );
                return;
            }
            if (!requestedState && serverSync.IsIrreversibleSterilizationEnabled) {
                serverSync.Notify(
                    peer: requester,
                    kind: PlayerNoticeKind.FertilityRestorationBlocked
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
            var result = new NetworkSterilityWrites.ChangeResult();
            try {
                yield return NetworkSterilityWrites.ApplyChanges(
                    network: serverSync.CurrentNetwork,
                    remaining: new List<NetworkSterilityWrites.TargetWrite>(1) { new NetworkSterilityWrites.TargetWrite(id) },
                    requestedState: requestedState,
                    result: result
                );
                PlayerNoticeKind notice = PlayerNoticeKind.ChangeUnconfirmed;
                if (result.Changed == 1) {
                    notice = requestedState ? PlayerNoticeKind.Sterilized : PlayerNoticeKind.FertileAgain;
                    serverSync.PublishSterilityChangeEffect(
                        id: id,
                        isSterile: requestedState
                    );
                }
                serverSync.Notify(
                    peer: requester,
                    kind: notice
                );
            } finally {
                individualChanges.Remove(id);
            }
        }

        private static bool IsSterilizable(Tameable tameable) {
            return tameable != null && tameable.IsTamed() &&
                tameable.GetComponent<Procreation>() != null && tameable.GetComponent<Growup>() == null;
        }

        private static bool IsSterilityChangeAllowed(ZNetPeer requester, ZDO target) {
            GameObject targetObject = ZNetScene.instance?.FindInstance(target.m_uid);
            if (requester == null) {
                Player local = Player.m_localPlayer;
                Character hovered = local != null ? local.GetHoverCreature() : null;
                return hovered != null && hovered.gameObject == targetObject &&
                    IsSterilizable(targetObject.GetComponent<Tameable>());
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
                if (!IsSterilizable(targetObject.GetComponent<Tameable>())) {
                    return false;
                }
            } else {
                bool isTamed = target.GetBool(
                    hash: ZDOVars.s_tamed,
                    defaultValue: false
                );
                bool isEligible = isTamed && targetPrefab.GetComponent<Tameable>() != null &&
                    targetPrefab.GetComponent<Procreation>() != null && targetPrefab.GetComponent<Growup>() == null;
                if (!isEligible) {
                    return false;
                }
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
                if (!ZdoSterilityOperations.IsSterile(zdo)) {
                    return true;
                }

                if (zdo.IsOwner()) {
                    ZdoSterilityOperations.ClearPregnancy(zdo);
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
                bool shouldUseOriginal = sync.SettingsReady &&
                    (!sync.RestrictionsEnabled || !ZdoSterilityOperations.IsSterile(___m_nview?.GetZDO()));
                if (shouldUseOriginal) {
                    return true;
                }

                __result = false;
                return false;
            }
        }
    }
}

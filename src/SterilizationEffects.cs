using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace OffWithTheirNuts {
    internal sealed class SterilizationEffects {
        private readonly ConfigEntry<bool> enabled;
        private readonly ManualLogSource logger;

        internal SterilizationEffects(ConfigEntry<bool> enabled, ManualLogSource logger) {
            this.enabled = enabled;
            this.logger = logger;
        }

        internal void PlayIfEnabled(ZDOID id) {
            ZNetScene scene = ZNetScene.instance;
            if (!enabled.Value || Player.m_localPlayer == null || scene == null) {
                return;
            }

            GameObject target = scene.FindInstance(id);
            Procreation reproduction = target != null ? target.GetComponent<Procreation>() : null;
            if (reproduction == null || reproduction.m_birthEffects == null) {
                return;
            }

            bool forceDisableInit = ZNetView.m_forceDisableInit;
            try {
                // Keep the visual and audio objects local by skipping their ZNetView registration.
                ZNetView.m_forceDisableInit = true;
                reproduction.m_birthEffects.Create(
                    basePos: target.transform.position,
                    baseRot: Quaternion.identity,
                    baseParent: null,
                    scale: 1f,
                    variant: -1,
                    gamepadEffectsExclusiveToPlayer: ZDOID.None
                );
            } catch (Exception error) {
                logger.LogWarning("Could not play the sterilization effect. " + error.Message);
            } finally {
                ZNetView.m_forceDisableInit = forceDisableInit;
            }
        }
    }
}

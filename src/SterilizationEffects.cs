using System;
using BepInEx.Configuration;
using BepInEx.Logging;
using Jotunn.Managers;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OffWithTheirNuts {
    internal sealed class SterilizationEffects {
        private const string FertilityVisualInstanceName = Constants.PluginId + ".FertilityRestorationEffect";

        private readonly ConfigEntry<bool> enabled;
        private readonly ManualLogSource logger;

        internal SterilizationEffects(ConfigEntry<bool> enabled, ManualLogSource logger) {
            this.enabled = enabled;
            this.logger = logger;
        }

        internal void PlayIfEnabled(ZDOID id, bool isSterile) {
            ZNetScene scene = ZNetScene.instance;
            if (Player.m_localPlayer == null || scene == null) {
                return;
            }

            GameObject target = scene.FindInstance(id);
            if (target == null) {
                return;
            }

            Transform previous = target.transform.Find(FertilityVisualInstanceName);
            if (previous != null) {
                previous.SetParent(null);
                Object.Destroy(previous.gameObject);
            }
            if (!enabled.Value) {
                return;
            }

            bool forceDisableInit = ZNetView.m_forceDisableInit;
            try {
                // Keep the visual and audio objects local by skipping their ZNetView registration.
                ZNetView.m_forceDisableInit = true;
                if (isSterile) {
                    PlaySterilizationEffect(target);
                } else {
                    PlayFertilityRestorationVfx(target);
                    PlayFertilityRestorationSound(target.transform);
                }
            } catch (Exception error) {
                logger.LogWarning("Could not play the sterility change effect. " + error.Message);
            } finally {
                ZNetView.m_forceDisableInit = forceDisableInit;
            }
        }

        private static void PlaySterilizationEffect(GameObject target) {
            Procreation reproduction = target.GetComponent<Procreation>();
            if (reproduction == null || reproduction.m_birthEffects == null) {
                return;
            }

            reproduction.m_birthEffects.Create(
                basePos: target.transform.position,
                baseRot: Quaternion.identity,
                baseParent: null,
                scale: 1f,
                variant: -1,
                gamepadEffectsExclusiveToPlayer: ZDOID.None
            );
        }

        private void PlayFertilityRestorationVfx(GameObject target) {
            Character animal = target.GetComponent<Character>();
            if (animal == null) {
                return;
            }

            const string FertilityVisualPrefab = "vfx_creature_soothed";

            GameObject visual = PrefabManager.Instance.GetPrefab(FertilityVisualPrefab);
            if (visual != null) {
                GameObject instance = Object.Instantiate(
                    original: visual,
                    position: animal.GetCenterPoint(),
                    rotation: animal.transform.rotation,
                    parent: animal.transform
                );
                instance.name = FertilityVisualInstanceName;
            } else {
                logger.LogWarning("Could not find the fertility restoration visual effect " + FertilityVisualPrefab + ".");
            }
        }

        private static void PlayFertilityRestorationSound(Transform target) {
            Procreation reproduction = target.GetComponent<Procreation>();
            if (reproduction == null) {
                return;
            }

            EffectList.EffectData[] effects = reproduction.m_loveEffects?.m_effectPrefabs;
            if (effects == null) {
                return;
            }

            EffectList.EffectData[] sounds = Array.FindAll(
                array: effects,
                match: effect => effect?.m_prefab != null
                    && effect.m_prefab.GetComponentInChildren<AudioSource>(includeInactive: true) != null
            );
            var matingSound = new EffectList { m_effectPrefabs = sounds };
            GameObject[] soundInstances = matingSound.Create(
                basePos: target.position,
                baseRot: Quaternion.identity,
                baseParent: target,
                scale: 1f,
                variant: -1,
                gamepadEffectsExclusiveToPlayer: ZDOID.None
            );
            foreach (GameObject instance in soundInstances) {
                foreach (Renderer renderer in instance.GetComponentsInChildren<Renderer>(includeInactive: true)) {
                    renderer.enabled = false;
                }
            }
        }
    }
}

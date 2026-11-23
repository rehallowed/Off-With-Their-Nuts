using System;
using HarmonyLib;

namespace OffWithTheirNuts {
    internal sealed class AnimalHoverText {
        private string lastHoverText;
        private string lastSterileHoverText;
        private TextResources lastTextResources;
        private string sterileLine;

        internal string WithSterileStatus(string hoverText, TextResources textResources) {
            if (lastTextResources != textResources) {
                lastTextResources = textResources;
                lastHoverText = null;
                sterileLine = "\n<color=#FFC6A8>" + textResources.SterileStatus + "</color>";
            }

            if (hoverText != lastHoverText) {
                // Valheim starts each interaction hint on a new line with an opening bracket.
                int firstHint = hoverText.IndexOf(
                    value: "\n[",
                    comparisonType: StringComparison.Ordinal
                );
                lastSterileHoverText = firstHint >= 0 ? hoverText.Insert(
                    startIndex: firstHint,
                    value: sterileLine
                ) : hoverText + sterileLine;
                lastHoverText = hoverText;
            }
            return lastSterileHoverText;
        }
    }

    public sealed partial class OffWithTheirNutsPlugin {
        [HarmonyPatch(
            declaringType: typeof(Tameable),
            methodName: "GetHoverText"
        )]
        private static class HoverTextPatch {
            private static void Postfix(ref string __result, ZNetView ___m_nview) {
                OffWithTheirNutsPlugin mod = instance;
                if (mod != null && mod.RestrictionsActive && !string.IsNullOrEmpty(__result) && IsSterile(___m_nview?.GetZDO())) {
                    __result = mod.animalHoverText.WithSterileStatus(
                        hoverText: __result,
                        textResources: mod.uiLanguage.TextResources
                    );
                }
            }
        }
    }
}

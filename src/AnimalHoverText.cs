using System;
using HarmonyLib;

namespace OffWithTheirNuts {
    public sealed partial class OffWithTheirNutsPlugin {
        private string lastHoverText;
        private string lastSterileHoverText;

        private string WithSterileStatus(string hoverText) {
            if (hoverText != lastHoverText) {
                const string sterileLine = "\n<color=#FFC6A8>Стерилизован</color>";
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

        [HarmonyPatch(
            declaringType: typeof(Tameable),
            methodName: "GetHoverText"
        )]
        private static class HoverTextPatch {
            private static void Postfix(ref string __result, ZNetView ___m_nview) {
                OffWithTheirNutsPlugin mod = instance;
                if (mod != null && mod.RestrictionsActive && !string.IsNullOrEmpty(__result) && IsSterile(___m_nview?.GetZDO())) {
                    __result = mod.WithSterileStatus(__result);
                }
            }
        }
    }
}

using System;
using HarmonyLib;

namespace OffWithTheirNuts {
    internal sealed class AnimalHoverText {
        private string lastHoverText;
        private string lastFormattedHoverText;
        private TextResources lastTextResources;
        private bool lastIsSterile;
        private bool lastIsIrreversibleSterilizationEnabled;
        private string lastShortcutLabel;
        private string sterileStatusLine;
        private string shortcutLine;

        internal string WithSterilityInfo(
            string hoverText,
            TextResources textResources,
            bool isSterile,
            bool isIrreversibleSterilizationEnabled,
            string shortcutLabel
        ) {
            bool shouldUpdateLines = lastTextResources != textResources || lastIsSterile != isSterile ||
                lastIsIrreversibleSterilizationEnabled != isIrreversibleSterilizationEnabled ||
                lastShortcutLabel != shortcutLabel;
            if (shouldUpdateLines) {
                lastTextResources = textResources;
                lastIsSterile = isSterile;
                lastIsIrreversibleSterilizationEnabled = isIrreversibleSterilizationEnabled;
                lastShortcutLabel = shortcutLabel;
                lastHoverText = null;
                sterileStatusLine = isSterile ? "\n<color=#FFC6A8>" + textResources.SterileStatus + "</color>" : "";
                shortcutLine = FormatShortcutLine(
                    textResources: textResources,
                    isSterile: isSterile,
                    isIrreversibleSterilizationEnabled: isIrreversibleSterilizationEnabled,
                    shortcutLabel: shortcutLabel
                );
            }

            if (hoverText != lastHoverText) {
                string formattedHoverText = isSterile ? WithSterileStatus(hoverText) : hoverText;
                lastFormattedHoverText = formattedHoverText + shortcutLine;
                lastHoverText = hoverText;
            }
            return lastFormattedHoverText;
        }

        private string WithSterileStatus(string hoverText) {
            // Valheim starts each interaction hint on a new line with an opening bracket.
            int firstHint = hoverText.IndexOf(
                value: "\n[",
                comparisonType: StringComparison.Ordinal
            );
            return firstHint < 0
                ? hoverText + sterileStatusLine
                : hoverText.Insert(
                    startIndex: firstHint,
                    value: sterileStatusLine
                );
        }

        private static string FormatShortcutLine(
            TextResources textResources,
            bool isSterile,
            bool isIrreversibleSterilizationEnabled,
            string shortcutLabel
        ) {
            if (shortcutLabel == null || (isSterile && isIrreversibleSterilizationEnabled)) {
                return "";
            }

            string action = (isSterile, isIrreversibleSterilizationEnabled) switch {
                (true, _) => textResources.RestoreFertilityHint,
                (false, true) => textResources.IrreversibleSterilizationHint,
                _ => textResources.SterilizeHint
            };
            return "\n[<color=yellow><b>" + shortcutLabel + "</b></color>] " + action;
        }
    }

    public sealed partial class OffWithTheirNutsPlugin {
        [HarmonyPatch(
            declaringType: typeof(Tameable),
            methodName: "GetHoverText"
        )]
        private static class HoverTextPatch {
            private static void Postfix(Tameable __instance, ref string __result, ZNetView ___m_nview) {
                OffWithTheirNutsPlugin mod = instance;
                bool shouldKeepHoverText = mod == null || !mod.RestrictionsActive ||
                    string.IsNullOrEmpty(__result) || ___m_nview == null;
                if (shouldKeepHoverText) {
                    return;
                }

                ZDO zdo = ___m_nview.GetZDO();
                bool isSterile = ZdoSterilityOperations.IsSterile(zdo);
                string shortcutLabel = mod.hotkey.ShortcutLabel;
                bool shouldShowHint = shortcutLabel != null && ___m_nview.IsValid() && IsSterilizable(__instance);
                if (!isSterile && !shouldShowHint) {
                    return;
                }

                __result = mod.animalHoverText.WithSterilityInfo(
                    hoverText: __result,
                    textResources: mod.uiLanguage.TextResources,
                    isSterile: isSterile,
                    isIrreversibleSterilizationEnabled: mod.serverSync.IsIrreversibleSterilizationEnabled,
                    shortcutLabel: shouldShowHint ? shortcutLabel : null
                );
            }
        }
    }
}

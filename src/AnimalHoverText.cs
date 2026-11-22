using HarmonyLib;

namespace OffWithTheirNuts {
    public sealed partial class OffWithTheirNutsPlugin {
        private string lastStatus;
        private string lastSterileStatus;

        private string WithSterileStatus(string status) {
            const string sterileStatus = "<color=#FFC6A8>стерилизован</color>";
            switch (status) {
                case "$hud_tamehungry": {
                    return "$hud_tamehungry, " + sterileStatus;
                }
                case "$hud_tamehappy": {
                    return "$hud_tamehappy, " + sterileStatus;
                }
                case "$hud_tamefrightened": {
                    return "$hud_tamefrightened, " + sterileStatus;
                }
                case "$hud_tameinprogress": {
                    return "$hud_tameinprogress, " + sterileStatus;
                }
                case "":
                case null: {
                    return sterileStatus;
                }
            }

            if (status != lastStatus) {
                lastStatus = status;
                lastSterileStatus = status + ", " + sterileStatus;
            }
            return lastSterileStatus;
        }

        [HarmonyPatch(
            declaringType: typeof(Tameable),
            methodName: "GetStatusString"
        )]
        private static class StatusPatch {
            private static void Postfix(ref string __result, ZNetView ___m_nview) {
                OffWithTheirNutsPlugin mod = instance;
                if (!(mod is null) && mod.RestrictionsActive && IsSterile(___m_nview?.GetZDO())) {
                    __result = mod.WithSterileStatus(__result);
                }
            }
        }
    }
}

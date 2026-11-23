namespace OffWithTheirNuts {
    internal static class ZdoSterilityOperations {
        // Keep the original Valheim stable hash so existing worlds retain their flags.
        private const int SterileKey = 516296702;

        internal static bool IsSterile(ZDO zdo) {
            return zdo != null && zdo.GetBool(
                hash: SterileKey,
                defaultValue: false
            );
        }

        internal static void ClearPregnancy(ZDO zdo) {
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
    }
}

using System.Globalization;

namespace OffWithTheirNuts {
    internal class TextMapper {
        internal static string GetFormattedResetResult(
            TextResources localization,
            int count,
            double seconds,
            int unconfirmed
        ) {
            string result = string.Format(
                provider: CultureInfo.InvariantCulture,
                format: localization.ResetResult.GetQuantityString(count),
                arg0: count,
                arg1: seconds
            );
            if (unconfirmed > 0) {
                result += string.Format(
                    provider: CultureInfo.InvariantCulture,
                    format: localization.ResetUnconfirmed,
                    arg0: unconfirmed
                );
            }
            return result;
        }
    }
}

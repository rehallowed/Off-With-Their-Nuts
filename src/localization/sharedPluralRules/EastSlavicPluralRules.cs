using System;

namespace OffWithTheirNuts {
    internal sealed class EastSlavicPluralRules : PluralRules {
        internal override PluralForm GetPluralForm(int count) {
            long magnitude = Math.Abs((long)count);
            long lastDigit = magnitude % 10;
            long lastTwoDigits = magnitude % 100;
            if (lastDigit == 1 && lastTwoDigits != 11) {
                return PluralForm.One;
            }
            if (lastDigit >= 2 && lastDigit <= 4 && (lastTwoDigits < 12 || lastTwoDigits > 14)) {
                return PluralForm.Few;
            }
            return PluralForm.Many;
        }
    }
}

using System;

namespace OffWithTheirNuts {
    internal sealed class PolishPluralRules : PluralRules {
        internal override PluralForm GetPluralForm(int count) {
            long magnitude = Math.Abs((long)count);
            if (magnitude == 1) {
                return PluralForm.One;
            }

            long lastDigit = magnitude % 10;
            long lastTwoDigits = magnitude % 100;
            if (lastDigit >= 2 && lastDigit <= 4 && (lastTwoDigits < 12 || lastTwoDigits > 14)) {
                return PluralForm.Few;
            }
            return PluralForm.Many;
        }
    }
}

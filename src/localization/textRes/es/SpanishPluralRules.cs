using System;

namespace OffWithTheirNuts {
    internal sealed class SpanishPluralRules : PluralRules {
        internal override PluralForm GetPluralForm(int count) {
            long magnitude = Math.Abs((long)count);
            if (magnitude == 1) {
                return PluralForm.One;
            }
            if (magnitude != 0 && magnitude % 1_000_000 == 0) {
                return PluralForm.Many;
            }
            return PluralForm.Other;
        }
    }
}

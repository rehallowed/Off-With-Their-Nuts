using System;

namespace OffWithTheirNuts {
    internal sealed class OneOrOtherPluralRules : PluralRules {
        internal override PluralForm GetPluralForm(int count) {
            return Math.Abs((long)count) == 1 ? PluralForm.One : PluralForm.Other;
        }
    }
}

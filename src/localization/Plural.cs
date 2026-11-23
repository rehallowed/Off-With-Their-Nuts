using System;

namespace OffWithTheirNuts {
    internal enum PluralForm {
        Zero = 0,
        One = 1,
        Two = 2,
        Few = 3,
        Many = 4,
        Other = 5
    }

    internal sealed class Plural {
        private readonly PluralRules pluralRules;
        private readonly string other;
        private readonly string zero;
        private readonly string one;
        private readonly string two;
        private readonly string few;
        private readonly string many;

        internal Plural(
            PluralRules pluralRules,
            string other,
            string zero = null,
            string one = null,
            string two = null,
            string few = null,
            string many = null
        ) {
            this.pluralRules = pluralRules ?? throw new ArgumentNullException(nameof(pluralRules));
            this.other = other ?? throw new ArgumentNullException(nameof(other));
            this.zero = zero;
            this.one = one;
            this.two = two;
            this.few = few;
            this.many = many;
        }

        internal string GetQuantityString(int count) {
            string variant = pluralRules.GetPluralForm(count) switch {
                PluralForm.Zero => zero,
                PluralForm.One => one,
                PluralForm.Two => two,
                PluralForm.Few => few,
                PluralForm.Many => many,
                _ => other
            };
            return variant ?? other;
        }
    }
}

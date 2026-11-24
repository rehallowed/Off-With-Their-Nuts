namespace OffWithTheirNuts {
    internal sealed class InvariantPluralRules : PluralRules {
        internal override PluralForm GetPluralForm(int count) {
            return PluralForm.Other;
        }
    }
}

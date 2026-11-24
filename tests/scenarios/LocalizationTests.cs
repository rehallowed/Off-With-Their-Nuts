using System;
using System.Globalization;
using System.Reflection;
using HarmonyLib;
using OffWithTheirNuts.Tests.Helpers;

namespace OffWithTheirNuts.Tests.Scenarios {
    internal static class LocalizationTests {
        internal static void CheckLanguagesAndResetMessages() {
            (string Language, Type ResourceType)[] languages = {
                ("English", typeof(EnglishTextResources)),
                ("Russian", typeof(RussianTextResources)),
                ("German", typeof(GermanTextResources)),
                ("Portuguese_Brazilian", typeof(BrazilianPortugueseTextResources)),
                ("Spanish", typeof(SpanishTextResources)),
                ("French", typeof(FrenchTextResources)),
                ("Polish", typeof(PolishTextResources)),
                ("Ukrainian", typeof(UkrainianTextResources)),
                ("Turkish", typeof(TurkishTextResources)),
                ("Chinese", null),
                ("Chinese_Trad", null),
                ("Japanese", typeof(JapaneseTextResources))
            };
            PropertyInfo[] properties = typeof(TextResources).GetProperties(
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            int[] counts = { 0, 1, 2, 5, 21, 42, 1_000_000 };
            CultureInfo previousCulture = CultureInfo.CurrentCulture;
            try {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
                foreach ((string language, Type resourceType) in languages) {
                    TextResources resources = LocalizationCore.ForLanguage(language);
                    if (resourceType == null) {
                        Expect.True(
                            scenario: language + " resolves to a translation",
                            condition: resources != null && resources is not EnglishTextResources
                        );
                    } else {
                        Expect.Equal(
                            scenario: language + " resource type",
                            expected: resourceType,
                            actual: resources.GetType()
                        );
                    }
                    foreach (PropertyInfo property in properties) {
                        if (property.PropertyType == typeof(string)) {
                            Expect.True(
                                scenario: language + " " + property.Name,
                                condition: !string.IsNullOrWhiteSpace((string)property.GetValue(resources))
                            );
                        }
                    }

                    foreach (int count in counts) {
                        CheckResetMessage(
                            resources: resources,
                            language: language,
                            count: count
                        );
                    }
                }
            } finally {
                CultureInfo.CurrentCulture = previousCulture;
            }

            TextResources english = LocalizationCore.ForLanguage("English");
            foreach (string language in new[] { null, "Unsupported", "eNgLiSh" }) {
                Expect.Equal(
                    scenario: "English resources for " + (language ?? "<null>"),
                    expected: english,
                    actual: LocalizationCore.ForLanguage(language)
                );
            }
        }

        private static void CheckResetMessage(TextResources resources, string language, int count) {
            string reset = TextMapper.GetFormattedResetResult(
                localization: resources,
                count: count,
                seconds: 9.87,
                unconfirmed: 0
            );
            string partialReset = TextMapper.GetFormattedResetResult(
                localization: resources,
                count: count,
                seconds: 9.87,
                unconfirmed: 7
            );
            Expect.True(
                scenario: language + " reset message at " + count + " contains the count and invariant time",
                condition: reset.Contains(count.ToString(CultureInfo.InvariantCulture)) &&
                    reset.Contains("9.87") && !reset.Contains("9,87")
            );
            Expect.True(
                scenario: language + " partial reset at " + count + " includes the unconfirmed count",
                condition: partialReset.StartsWith(
                    value: reset,
                    comparisonType: StringComparison.Ordinal
                ) && partialReset.Substring(reset.Length).Contains("7")
            );
        }

        internal static void CheckPluralForms() {
            (string Language, int Count, PluralForm Form)[] cases = {
                ("English", 0, PluralForm.Other),
                ("English", 1, PluralForm.One),
                ("German", 2, PluralForm.Other),
                ("Turkish", -1, PluralForm.One),
                ("Russian", 1, PluralForm.One),
                ("Russian", 11, PluralForm.Many),
                ("Russian", 21, PluralForm.One),
                ("Ukrainian", 22, PluralForm.Few),
                ("Ukrainian", 114, PluralForm.Many),
                ("Polish", 1, PluralForm.One),
                ("Polish", 2, PluralForm.Few),
                ("Polish", 12, PluralForm.Many),
                ("Polish", 21, PluralForm.Many),
                ("Polish", 22, PluralForm.Few),
                ("Polish", int.MinValue, PluralForm.Many),
                ("French", 0, PluralForm.One),
                ("French", 1_000_000, PluralForm.Many),
                ("Portuguese_Brazilian", 0, PluralForm.One),
                ("Portuguese_Brazilian", 5, PluralForm.Other),
                ("Spanish", 0, PluralForm.Other),
                ("Spanish", 1, PluralForm.One),
                ("Spanish", 1_000_000, PluralForm.Many),
                ("Chinese", 1, PluralForm.Other),
                ("Chinese_Trad", 2, PluralForm.Other),
                ("Japanese", 0, PluralForm.Other)
            };
            foreach ((string language, int count, PluralForm form) in cases) {
                TextResources resources = LocalizationCore.ForLanguage(language);
                Expect.Equal(
                    scenario: language + " plural at " + count,
                    expected: form,
                    actual: resources.PluralRules.GetPluralForm(count)
                );
            }
        }

        internal static void CheckLanguageChanges() {
            PlatformPrefs.Language = "English";
            var language = new GameUiLanguage();
            _ = new OffWithTheirNutsPlugin(uiLanguage: language);
            var harmony = new Harmony();
            Expect.True(
                scenario: "Language patch accepts the game setup signature",
                condition: GameUiLanguage.PatchLanguageSetup(harmony)
            );

            int changes = 0;
            language.Changed += () => changes++;
            PlatformPrefs.Language = "Russian";
            harmony.Postfix.Invoke(
                obj: null,
                parameters: null
            );
            Expect.True(
                scenario: "Language setup refreshes the active resources",
                condition: language.TextResources is RussianTextResources
            );
            harmony.Postfix.Invoke(
                obj: null,
                parameters: null
            );
            Expect.Equal(
                scenario: "Setting the same language does not raise another change",
                expected: 1,
                actual: changes
            );
        }

        internal static void CheckHoverText() {
            const string VanillaHover = "Boar\n[E] Pet";
            TextResources english = LocalizationCore.ForLanguage("English");
            TextResources russian = LocalizationCore.ForLanguage("Russian");
            var hover = new AnimalHoverText();

            string first = hover.WithSterilityInfo(
                hoverText: VanillaHover,
                textResources: english,
                isSterile: true,
                isIrreversibleSterilizationEnabled: false,
                shortcutLabel: "U"
            );
            Expect.Equal(
                scenario: "Status sits above the vanilla hint and the mod hint below it",
                expected: "Boar\n<color=#FFC6A8>" + english.SterileStatus +
                    "</color>\n[E] Pet\n[<color=yellow><b>U</b></color>] " + english.RestoreFertilityHint,
                actual: first
            );
            string cached = hover.WithSterilityInfo(
                hoverText: VanillaHover,
                textResources: english,
                isSterile: true,
                isIrreversibleSterilizationEnabled: false,
                shortcutLabel: "U"
            );
            Expect.True(
                scenario: "Identical inputs reuse the formatted string",
                condition: ReferenceEquals(
                    objA: first,
                    objB: cached
                )
            );

            string irreversible = hover.WithSterilityInfo(
                hoverText: VanillaHover,
                textResources: russian,
                isSterile: false,
                isIrreversibleSterilizationEnabled: true,
                shortcutLabel: "I"
            );
            Expect.Equal(
                scenario: "Language, sterility, shortcut and irreversible mode refresh the hint",
                expected: VanillaHover + "\n[<color=yellow><b>I</b></color>] " + russian.IrreversibleSterilizationHint,
                actual: irreversible
            );
            string blocked = hover.WithSterilityInfo(
                hoverText: VanillaHover,
                textResources: russian,
                isSterile: true,
                isIrreversibleSterilizationEnabled: true,
                shortcutLabel: "I"
            );
            Expect.Equal(
                scenario: "Irreversible mode shows the sterile status with no restoration hint",
                expected: "Boar\n<color=#FFC6A8>" + russian.SterileStatus + "</color>\n[E] Pet",
                actual: blocked
            );
        }
    }
}

using System;
using System.Collections.Generic;

namespace OffWithTheirNuts {
    internal class LocalizationCore {
        internal const string DefaultLanguage = "English";
        private static readonly TextResources englishTextResources = new EnglishTextResources();
        
        private static readonly Dictionary<string, TextResources> byLanguage =
            new Dictionary<string, TextResources>(StringComparer.OrdinalIgnoreCase) {
                { "English", englishTextResources },
                { "Russian", new RussianTextResources() },
                { "German", new GermanTextResources() },
                { "Portuguese_Brazilian", new BrazilianPortugueseTextResources() },
                { "Spanish", new SpanishTextResources() },
                { "French", new FrenchTextResources() },
                { "Polish", new PolishTextResources() },
                { "Ukrainian", new UkrainianTextResources() },
                { "Turkish", new TurkishTextResources() },
                { "Chinese", new ChineseTextResources() },
                { "Chinese_Trad", new ChineseTextResources() },
                { "Japanese", new JapaneseTextResources() }
            };

        internal static TextResources ForLanguage(string language) {
            return language != null && byLanguage.TryGetValue(
                key: language,
                value: out TextResources textResources
            ) ? textResources : englishTextResources;
        }
    }
}

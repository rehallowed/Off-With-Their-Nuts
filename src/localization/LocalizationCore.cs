using System;
using System.Collections.Generic;
using System.Globalization;

namespace OffWithTheirNuts {
    internal class LocalizationCore {
        internal const string DefaultLanguage = "English";
        private static readonly TextResources englishTextResources = new EnglishTextResources();
        private static readonly Dictionary<string, TextResources> byLanguage =
            new Dictionary<string, TextResources>(StringComparer.OrdinalIgnoreCase) {
                { "English", englishTextResources },
                { "Russian", new RussianTextResources() }
            };

        internal static TextResources ForLanguage(string language) {
            return language != null && byLanguage.TryGetValue(
                key: language,
                value: out TextResources textResources
            ) ? textResources : englishTextResources;
        }
    }
}

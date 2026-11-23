using System;
using System.Reflection;
using HarmonyLib;

namespace OffWithTheirNuts {
    internal sealed class GameUiLanguage {
        internal TextResources TextResources { get; private set; } = ReadTextResources();
        internal event Action Changed;

        internal static bool PatchLanguageSetup(Harmony harmony) {
            // Valheim's automatic startup language goes through SetupLanguage without raising OnLanguageChange.
            Type localization = AccessTools.TypeByName("Localization");
            if (localization == null) {
                return false;
            }
            MethodInfo setup = AccessTools.DeclaredMethod(
                type: localization,
                name: "SetupLanguage",
                parameters: new[] { typeof(string) }
            );
            if (setup == null || setup.IsStatic || setup.ReturnType != typeof(bool)) {
                return false;
            }
            MethodInfo postfix = AccessTools.DeclaredMethod(
                type: typeof(GameUiLanguage),
                name: nameof(AfterLanguageSetup)
            );
            harmony.Patch(
                original: setup,
                postfix: new HarmonyMethod(postfix)
            );
            return true;
        }

        private static void AfterLanguageSetup() {
            OffWithTheirNutsPlugin.ActiveUiLanguage?.Refresh();
        }

        private void Refresh() {
            TextResources textResources = ReadTextResources();
            if (TextResources != textResources) {
                TextResources = textResources;
                Changed?.Invoke();
            }
        }

        private static TextResources ReadTextResources() {
            return LocalizationCore.ForLanguage(
                language: PlatformPrefs.GetString(
                    name: "language",
                    defaultValue: LocalizationCore.DefaultLanguage
                )
            );
        }
    }
}

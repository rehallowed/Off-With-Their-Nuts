using System;
using OffWithTheirNuts.Tests.Scenarios;

namespace OffWithTheirNuts.Tests {
    internal static class Program {
        private static int Main() {
            Action[] tests = {
                LocalizationTests.CheckLanguagesAndResetMessages,
                LocalizationTests.CheckPluralForms,
                LocalizationTests.CheckLanguageChanges,
                LocalizationTests.CheckHoverText,
                NetworkTests.CheckSettingsAndPermissions,
                NetworkTests.CheckNoticePackets,
                NetworkTests.CheckToggleRoutes,
                NetworkTests.CheckOwnershipTransfer,
                NetworkTests.CheckResetDiagnostics,
                ZdoSterilityTests.CheckBreedingState
            };

            int failures = 0;
            foreach (Action test in tests) {
                try {
                    test();
                    Console.WriteLine($"PASS {test.Method.Name}");
                } catch (Exception error) {
                    failures++;
                    Console.Error.WriteLine($"FAIL {test.Method.Name}: {error}");
                }
            }

            Console.WriteLine($"{tests.Length - failures}/{tests.Length} scenarios passed.");
            return failures == 0 ? 0 : 1;
        }
    }
}

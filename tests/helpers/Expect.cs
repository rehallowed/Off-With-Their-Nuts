using System;
using System.Collections.Generic;

namespace OffWithTheirNuts.Tests.Helpers {
    internal static class Expect {
        internal static void Equal<T>(string scenario, T expected, T actual) {
            bool matches = EqualityComparer<T>.Default.Equals(
                x: expected,
                y: actual
            );
            if (!matches) {
                throw new InvalidOperationException($"{scenario}: expected '{expected}', got '{actual}'.");
            }
        }

        internal static void True(string scenario, bool condition) {
            Equal(
                scenario: scenario,
                expected: true,
                actual: condition
            );
        }
    }
}

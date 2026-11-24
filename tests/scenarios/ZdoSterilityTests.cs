using OffWithTheirNuts.Tests.Helpers;

namespace OffWithTheirNuts.Tests.Scenarios {
    internal static class ZdoSterilityTests {
        internal static void CheckBreedingState() {
            var animal = new ZDO();
            animal.Set(
                hash: ZDOVars.s_pregnant,
                value: 123L
            );
            animal.Set(
                hash: ZDOVars.s_lovePoints,
                value: 3
            );
            ZdoSterilityOperations.SetSterility(
                zdo: animal,
                requestedState: true
            );
            Expect.Equal(
                scenario: "Sterilization sets the flag and clears pregnancy and breeding progress",
                expected: (true, 0L, 0),
                actual: ReadBreedingState(animal)
            );

            // Seed the fields again so an unintended clear during restoration would fail the test.
            animal.Set(
                hash: ZDOVars.s_pregnant,
                value: 456L
            );
            animal.Set(
                hash: ZDOVars.s_lovePoints,
                value: 2
            );
            ZdoSterilityOperations.SetSterility(
                zdo: animal,
                requestedState: false
            );
            Expect.Equal(
                scenario: "Restoring fertility changes only the sterility flag",
                expected: (false, 456L, 2),
                actual: ReadBreedingState(animal)
            );
        }

        private static (bool IsSterile, long Pregnancy, int LovePoints) ReadBreedingState(ZDO animal) {
            long pregnancy = animal.GetLong(
                hash: ZDOVars.s_pregnant,
                defaultValue: -1L
            );
            int lovePoints = animal.GetInt(
                hash: ZDOVars.s_lovePoints,
                defaultValue: -1
            );
            return (ZdoSterilityOperations.IsSterile(animal), pregnancy, lovePoints);
        }
    }
}

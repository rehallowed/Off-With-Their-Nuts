using System;
using System.Collections.Generic;
using System.Globalization;

namespace OffWithTheirNuts {
    internal abstract class TextResources {

        protected Plural MakePlural(
            string other,
            string zero = null,
            string one = null,
            string two = null,
            string few = null,
            string many = null
        ) {
            return new Plural(
                pluralRules: PluralRules,
                other: other,
                zero: zero,
                one: one,
                two: two,
                few: few,
                many: many
            );
        }

        internal abstract PluralRules PluralRules { get; }

        internal abstract string SterileStatus { get; }
        internal abstract string SterilizeHint { get; }
        internal abstract string RestoreFertilityHint { get; }
        internal abstract string BreedingCategory { get; }
        internal abstract string WorldManagementCategory { get; }
        internal abstract string EnabledName { get; }
        internal abstract string EnabledDescription { get; }
        internal abstract string Enabled { get; }
        internal abstract string Disabled { get; }
        internal abstract string ShortcutName { get; }
        internal abstract string ShortcutDescription { get; }
        internal abstract string SterilizationEffectsName { get; }
        internal abstract string SterilizationEffectsDescription { get; }
        internal abstract string ResetName { get; }
        internal abstract string ResetDescription { get; }
        internal abstract string ResetButton { get; }
        internal abstract string ChangePending { get; }
        internal abstract string TargetUnavailable { get; }
        internal abstract string AlreadySterile { get; }
        internal abstract string AlreadyFertile { get; }
        internal abstract string ChangeUnconfirmed { get; }
        internal abstract string Sterilized { get; }
        internal abstract string FertileAgain { get; }
        internal abstract string ResetRunning { get; }
        internal abstract string ResetStarted { get; }
        internal abstract Plural ResetResult { get; }
        internal abstract string ResetUnconfirmed { get; }
    }
}

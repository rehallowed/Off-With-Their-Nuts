namespace OffWithTheirNuts {
    internal sealed class EnglishTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new EnglishPluralRules();

        internal override string SterileStatus =>
            "Sterile";
        internal override string SterilizeHint =>
            "Sterilize";
        internal override string IrreversibleSterilizationHint =>
            "Sterilize (irreversible)";
        internal override string RestoreFertilityHint =>
            "Restore fertility";
        internal override string ModActivationCategory =>
            "Mod Activation";
        internal override string PersonalSettingsCategory =>
            "Personal Settings";
        internal override string AdministrationCategory =>
            "Administration";
        internal override string EnabledName =>
            "Enable mod";
        internal override string EnabledDescription =>
            "Apply saved breeding restrictions. In multiplayer, the server controls this setting.";
        internal override string IrreversibleSterilizationName =>
            "Irreversible sterilization";
        internal override string IrreversibleSterilizationDescription =>
            "Prevent restoring fertility to individual animals while this setting is enabled. " +
            "Admins can still reset sterilization for all animals at once.";
        internal override string Enabled =>
            "Enabled";
        internal override string Disabled =>
            "Disabled";
        internal override string ShortcutName =>
            "Sterilization key binding";
        internal override string ShortcutDescription =>
            "Aim at a tamed animal and press this key or shortcut.";
        internal override string SterilizationEffectsName =>
            "Sterilization effects";
        internal override string SterilizationEffectsDescription =>
            "Play sounds and visual effects when an animal is sterilized or its fertility is restored.";
        internal override string ResetName =>
            "Reset all breeding restrictions";
        internal override string ResetDescription =>
            "Restore fertility to all sterile animals in the current world.";
        internal override string ResetButton =>
            "Reset all breeding restrictions";
        internal override string CleanupName =>
            "Mod data cleanup";
        internal override string CleanupDescription =>
            "Remove all mod data from the current world's save. " +
            "Start the cleanup with no other players connected. " +
            "Make sure no one joins until cleanup is complete - the mod does not block connections. " +
            "To finish, save and reload the world. " +
            "You can then uninstall the mod.";
        internal override string CleanupButton =>
            "Remove mod data from world";
        internal override string CleanupPlayersConnected =>
            "Cleanup requires all other players to disconnect";
        internal override string CleanupCompleted =>
            "Mod data removed. Save the world to keep the changes";
        internal override string ChangePending =>
            "A change is already in progress. Try again when it finishes.";
        internal override string TargetUnavailable =>
            "Move closer to a tamed animal that can breed.";
        internal override string AlreadySterile =>
            "This animal is already sterile.";
        internal override string AlreadyFertile =>
            "This animal can already breed.";
        internal override string ChangeUnconfirmed =>
            "Could not confirm the change for this animal.";
        internal override string Sterilized =>
            "Animal sterilized";
        internal override string FertileAgain =>
            "Animal is fertile again";
        internal override string FertilityRestorationBlocked =>
            "Fertility restoration is disabled by admin";
        internal override string ResetRunning =>
            "A sterility reset is already in progress.";
        internal override string ResetStarted =>
            "Restoring fertility to animals...";
        internal override Plural ResetResult => MakePlural(
            other: "Restored fertility to {0} animals in {1:0.00}s",
            one: "Restored fertility to {0} animal in {1:0.00}s"
        );
        internal override string ResetUnconfirmed =>
            " ({0} unconfirmed)";
    }
}

namespace OffWithTheirNuts {
    internal sealed class GermanTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new OneOrOtherPluralRules();

        internal override string SterileStatus =>
            "Sterilisiert";
        internal override string SterilizeHint =>
            "Sterilisieren";
        internal override string IrreversibleSterilizationHint =>
            "Sterilisieren (unumkehrbar)";
        internal override string RestoreFertilityHint =>
            "Fruchtbarkeit wiederherstellen";
        internal override string ModActivationCategory =>
            "Mod-Aktivierung";
        internal override string PersonalSettingsCategory =>
            "Persönliche Einstellungen";
        internal override string AdministrationCategory =>
            "Verwaltung";
        internal override string EnabledName =>
            "Mod aktivieren";
        internal override string EnabledDescription =>
            "Gespeicherte Zuchtbeschränkungen anwenden. Im Mehrspielermodus bestimmt der Server diese Einstellung.";
        internal override string IrreversibleSterilizationName =>
            "Unumkehrbarkeit der Sterilisation";
        internal override string IrreversibleSterilizationDescription =>
            "Solange diese Einstellung aktiv ist, lässt sich die Sterilisation einzelner Tiere nicht rückgängig machen. " +
            "Admins können die Sterilisation aller Tiere auf einmal zurücksetzen.";
        internal override string Enabled =>
            "Aktiviert";
        internal override string Disabled =>
            "Deaktiviert";
        internal override string ShortcutName =>
            "Tastenbelegung";
        internal override string ShortcutDescription =>
            "Visiere ein gezähmtes Tier an und drücke diese Taste oder Tastenkombination.";
        internal override string SterilizationEffectsName =>
            "Sterilisationseffekte";
        internal override string SterilizationEffectsDescription =>
            "Ton- und Bildeffekte abspielen, wenn ein Tier sterilisiert oder seine Fruchtbarkeit wiederhergestellt wird.";
        internal override string ResetName =>
            "Aufhebung der Sterilisation";
        internal override string ResetDescription =>
            "Die Fruchtbarkeit aller sterilisierten Tiere in der aktuellen Welt wiederherstellen.";
        internal override string ResetButton =>
            "Alle wieder fruchtbar machen";
        internal override string CleanupName =>
            "Löschen der Mod-Daten";
        internal override string CleanupDescription =>
            "Alle Mod-Daten aus dem Spielstand der aktuellen Welt entfernen. " +
            "Starte die Bereinigung erst, wenn keine anderen Spieler verbunden sind. " +
            "Achte darauf, dass bis zum Ende niemand beitritt, da der Mod Verbindungen nicht blockiert. " +
            "Speichere zum Abschluss die Welt und lade sie neu. " +
            "Danach kannst du den Mod deinstallieren.";
        internal override string CleanupButton =>
            "Mod-Daten aus der Welt löschen";
        internal override string CleanupPlayersConnected =>
            "Für die Bereinigung müssen alle anderen Spieler die Verbindung trennen";
        internal override string CleanupCompleted =>
            "Mod-Daten gelöscht. Speichere die Welt, um die Änderungen zu übernehmen";
        internal override string ChangePending =>
            "Eine Änderung läuft bereits. Versuche es danach erneut.";
        internal override string TargetUnavailable =>
            "Der Status dieses Tieres konnte nicht geändert werden. Geh näher heran und versuch es erneut.";
        internal override string AlreadySterile =>
            "Dieses Tier ist bereits sterilisiert.";
        internal override string AlreadyFertile =>
            "Dieses Tier kann sich bereits fortpflanzen.";
        internal override string ChangeUnconfirmed =>
            "Die Änderung für dieses Tier konnte nicht bestätigt werden.";
        internal override string Sterilized =>
            "Tier sterilisiert";
        internal override string FertileAgain =>
            "Tier ist wieder fruchtbar";
        internal override string FertilityRestorationBlocked =>
            "Das Wiederherstellen der Fruchtbarkeit wurde vom Admin deaktiviert";
        internal override string ResetRunning =>
            "Die Sterilisation wird bereits zurückgesetzt.";
        internal override string ResetStarted =>
            "Fruchtbarkeit der Tiere wird wiederhergestellt...";
        internal override Plural ResetResult =>
            MakePlural(
                other: "Fruchtbarkeit bei {0} Tieren in {1:0.00} s wiederhergestellt",
                one: "Fruchtbarkeit bei {0} Tier in {1:0.00} s wiederhergestellt"
            );
        internal override string ResetUnconfirmed =>
            " ({0} ohne Bestätigung)";
    }
}

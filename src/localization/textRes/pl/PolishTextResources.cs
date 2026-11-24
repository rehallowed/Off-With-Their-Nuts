namespace OffWithTheirNuts {
    internal sealed class PolishTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new PolishPluralRules();

        internal override string SterileStatus =>
            "Wysterylizowane";
        internal override string SterilizeHint =>
            "Wysterylizuj";
        internal override string IrreversibleSterilizationHint =>
            "Wysterylizuj (nieodwracalnie)";
        internal override string RestoreFertilityHint =>
            "Przywróć płodność";
        internal override string ModActivationCategory =>
            "Aktywacja moda";
        internal override string PersonalSettingsCategory =>
            "Ustawienia osobiste";
        internal override string AdministrationCategory =>
            "Administracja";
        internal override string EnabledName =>
            "Włącz mod";
        internal override string EnabledDescription =>
            "Stosuj zapisane ograniczenia rozmnażania. W trybie wieloosobowym o tym ustawieniu decyduje serwer.";
        internal override string IrreversibleSterilizationName =>
            "Nieodwracalność sterylizacji";
        internal override string IrreversibleSterilizationDescription =>
            "Gdy opcja jest włączona, nie można cofnąć sterylizacji pojedynczego zwierzęcia. " +
            "Administratorzy nadal mogą przywrócić płodność wszystkim zwierzętom naraz.";
        internal override string Enabled =>
            "Włączone";
        internal override string Disabled =>
            "Wyłączone";
        internal override string ShortcutName =>
            "Przypisanie klawisza";
        internal override string ShortcutDescription =>
            "Wyceluj w oswojone zwierzę i naciśnij ten klawisz lub kombinację klawiszy.";
        internal override string SterilizationEffectsName =>
            "Efekty sterylizacji";
        internal override string SterilizationEffectsDescription =>
            "Odtwarzaj dźwięki i efekty wizualne podczas sterylizacji zwierzęcia lub przywracania mu płodności.";
        internal override string ResetName =>
            "Cofnięcie sterylizacji";
        internal override string ResetDescription =>
            "Przywróć płodność wszystkim wysterylizowanym zwierzętom w bieżącym świecie.";
        internal override string ResetButton =>
            "Przywróć płodność wszystkim";
        internal override string CleanupName =>
            "Usuwanie danych moda";
        internal override string CleanupDescription =>
            "Usuń wszystkie dane moda z zapisu bieżącego świata. " +
            "Rozpocznij czyszczenie tylko wtedy, gdy żaden inny gracz nie jest połączony. " +
            "Upewnij się, że do końca czyszczenia nikt nie dołączy, ponieważ mod nie blokuje połączeń. " +
            "Na koniec zapisz świat i wczytaj go ponownie. " +
            "Potem możesz odinstalować mod.";
        internal override string CleanupButton =>
            "Usuń dane moda ze świata";
        internal override string CleanupPlayersConnected =>
            "Pozostali gracze muszą się rozłączyć przed rozpoczęciem czyszczenia";
        internal override string CleanupCompleted =>
            "Usunięto dane moda. Zapisz świat, aby zachować zmiany";
        internal override string ChangePending =>
            "Zmiana jest już w toku. Spróbuj ponownie po jej zakończeniu.";
        internal override string TargetUnavailable =>
            "Nie udało się zmienić stanu zwierzęcia. Podejdź bliżej i spróbuj ponownie.";
        internal override string AlreadySterile =>
            "To zwierzę jest już wysterylizowane.";
        internal override string AlreadyFertile =>
            "To zwierzę może już się rozmnażać.";
        internal override string ChangeUnconfirmed =>
            "Nie udało się potwierdzić zmiany dla tego zwierzęcia.";
        internal override string Sterilized =>
            "Zwierzę wysterylizowane";
        internal override string FertileAgain =>
            "Zwierzę odzyskało płodność";
        internal override string FertilityRestorationBlocked =>
            "Administrator wyłączył możliwość przywracania płodności";
        internal override string ResetRunning =>
            "Przywracanie płodności jest już w toku.";
        internal override string ResetStarted =>
            "Trwa przywracanie płodności zwierzętom...";
        internal override Plural ResetResult =>
            MakePlural(
                other: "{0} zwierząt odzyskało płodność w {1:0.00} s",
                one: "{0} zwierzę odzyskało płodność w {1:0.00} s",
                few: "{0} zwierzęta odzyskały płodność w {1:0.00} s"
            );
        internal override string ResetUnconfirmed =>
            " ({0} bez potwierdzenia)";
    }
}

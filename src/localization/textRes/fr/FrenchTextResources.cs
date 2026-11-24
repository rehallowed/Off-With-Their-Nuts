namespace OffWithTheirNuts {
    internal sealed class FrenchTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new ZeroAndOneSingularPluralRules();

        internal override string SterileStatus =>
            "Stérilisé";
        internal override string SterilizeHint =>
            "Stériliser";
        internal override string IrreversibleSterilizationHint =>
            "Stériliser (irréversible)";
        internal override string RestoreFertilityHint =>
            "Rétablir la fertilité";
        internal override string ModActivationCategory =>
            "Activation du mod";
        internal override string PersonalSettingsCategory =>
            "Paramètres personnels";
        internal override string AdministrationCategory =>
            "Administration";
        internal override string EnabledName =>
            "Activer le mod";
        internal override string EnabledDescription =>
            "Appliquer les restrictions de reproduction enregistrées. En multijoueur, le serveur contrôle ce paramètre.";
        internal override string IrreversibleSterilizationName =>
            "Irréversibilité de la stérilisation";
        internal override string IrreversibleSterilizationDescription =>
            "Tant que cette option est activée, la stérilisation d'un animal ne peut pas être annulée individuellement. " +
            "Les administrateurs peuvent toujours rétablir la fertilité de tous les animaux à la fois.";
        internal override string Enabled =>
            "Activé";
        internal override string Disabled =>
            "Désactivé";
        internal override string ShortcutName =>
            "Raccourci clavier";
        internal override string ShortcutDescription =>
            "Vise un animal apprivoisé, puis appuie sur cette touche ou combinaison de touches.";
        internal override string SterilizationEffectsName =>
            "Effets de stérilisation";
        internal override string SterilizationEffectsDescription =>
            "Déclencher les effets sonores et visuels lorsqu'un animal est stérilisé ou retrouve sa fertilité.";
        internal override string ResetName =>
            "Rétablissement de la fertilité";
        internal override string ResetDescription =>
            "Rétablir la fertilité de tous les animaux stérilisés du monde actuel.";
        internal override string ResetButton =>
            "Rendre la fertilité à tous";
        internal override string CleanupName =>
            "Suppression des données du mod";
        internal override string CleanupDescription =>
            "Supprimer les données du mod de la sauvegarde du monde actuel. " +
            "Lance le nettoyage quand aucun autre joueur n'est connecté. " +
            "Veille à ce que personne ne rejoigne la partie avant la fin, car le mod ne bloque pas les connexions. " +
            "Pour terminer, sauvegarde le monde et recharge-le. " +
            "Tu pourras ensuite désinstaller le mod.";
        internal override string CleanupButton =>
            "Supprimer les données du mod du monde";
        internal override string CleanupPlayersConnected =>
            "Tous les autres joueurs doivent se déconnecter avant le nettoyage";
        internal override string CleanupCompleted =>
            "Données du mod supprimées. Sauvegarde le monde pour conserver les modifications";
        internal override string ChangePending =>
            "Une modification est déjà en cours. Réessaie lorsqu'elle sera terminée.";
        internal override string TargetUnavailable =>
            "Impossible de modifier le statut de cet animal. Approche-toi et réessaie.";
        internal override string AlreadySterile =>
            "Cet animal est déjà stérilisé.";
        internal override string AlreadyFertile =>
            "Cet animal peut déjà se reproduire.";
        internal override string ChangeUnconfirmed =>
            "Impossible de confirmer la modification pour cet animal.";
        internal override string Sterilized =>
            "Animal stérilisé";
        internal override string FertileAgain =>
            "L'animal est de nouveau fertile";
        internal override string FertilityRestorationBlocked =>
            "La restauration de la fertilité a été désactivée par un administrateur";
        internal override string ResetRunning =>
            "La restauration de la fertilité est déjà en cours.";
        internal override string ResetStarted =>
            "Restauration de la fertilité des animaux...";
        internal override Plural ResetResult =>
            MakePlural(
                other: "Fertilité rétablie pour {0} animaux en {1:0.00} s",
                one: "Fertilité rétablie pour {0} animal en {1:0.00} s"
            );
        internal override string ResetUnconfirmed =>
            " ({0} sans confirmation)";
    }
}

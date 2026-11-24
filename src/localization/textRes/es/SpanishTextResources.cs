namespace OffWithTheirNuts {
    internal sealed class SpanishTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new SpanishPluralRules();

        internal override string SterileStatus =>
            "Esterilizado";
        internal override string SterilizeHint =>
            "Esterilizar";
        internal override string IrreversibleSterilizationHint =>
            "Esterilizar (irreversible)";
        internal override string RestoreFertilityHint =>
            "Restaurar la fertilidad";
        internal override string ModActivationCategory =>
            "Activación del mod";
        internal override string PersonalSettingsCategory =>
            "Ajustes personales";
        internal override string AdministrationCategory =>
            "Administración";
        internal override string EnabledName =>
            "Activar el mod";
        internal override string EnabledDescription =>
            "Aplicar las restricciones de reproducción guardadas. En multijugador, el servidor controla este ajuste.";
        internal override string IrreversibleSterilizationName =>
            "Irreversibilidad de la esterilización";
        internal override string IrreversibleSterilizationDescription =>
            "Mientras este ajuste esté activo, no se puede revertir la esterilización de un animal por separado. " +
            "Los administradores pueden restaurar la fertilidad de todos los animales a la vez.";
        internal override string Enabled =>
            "Activado";
        internal override string Disabled =>
            "Desactivado";
        internal override string ShortcutName =>
            "Asignación de tecla";
        internal override string ShortcutDescription =>
            "Apunta a un animal domesticado y pulsa esta tecla o combinación de teclas.";
        internal override string SterilizationEffectsName =>
            "Efectos de esterilización";
        internal override string SterilizationEffectsDescription =>
            "Reproducir sonidos y efectos visuales al esterilizar a un animal o restaurar su fertilidad.";
        internal override string ResetName =>
            "Reversión de la esterilización";
        internal override string ResetDescription =>
            "Restaurar la fertilidad de todos los animales esterilizados del mundo actual.";
        internal override string ResetButton =>
            "Restaurar fertilidad a todos";
        internal override string CleanupName =>
            "Eliminación de datos del mod";
        internal override string CleanupDescription =>
            "Eliminar del archivo de guardado del mundo actual todos los datos del mod. " +
            "Inicia la limpieza cuando no haya otros jugadores conectados. " +
            "Asegúrate de que nadie se conecte hasta que termine, ya que el mod no bloquea las conexiones. " +
            "Para terminar, guarda el mundo y vuelve a cargarlo. " +
            "Después podrás desinstalar el mod.";
        internal override string CleanupButton =>
            "Borrar los datos del mod del mundo";
        internal override string CleanupPlayersConnected =>
            "Todos los demás jugadores deben desconectarse para iniciar la limpieza";
        internal override string CleanupCompleted =>
            "Datos del mod borrados. Guarda el mundo para conservar los cambios";
        internal override string ChangePending =>
            "Ya hay un cambio en curso. Vuelve a intentarlo cuando termine.";
        internal override string TargetUnavailable =>
            "No se ha podido cambiar el estado del animal. Acércate e inténtalo de nuevo.";
        internal override string AlreadySterile =>
            "Este animal ya está esterilizado.";
        internal override string AlreadyFertile =>
            "Este animal ya puede reproducirse.";
        internal override string ChangeUnconfirmed =>
            "No se ha podido confirmar el cambio para este animal.";
        internal override string Sterilized =>
            "Animal esterilizado";
        internal override string FertileAgain =>
            "El animal vuelve a ser fértil";
        internal override string FertilityRestorationBlocked =>
            "El administrador ha desactivado la restauración de la fertilidad";
        internal override string ResetRunning =>
            "Ya se está restaurando la fertilidad de los animales.";
        internal override string ResetStarted =>
            "Restaurando la fertilidad de los animales...";
        internal override Plural ResetResult =>
            MakePlural(
                other: "Se ha restaurado la fertilidad de {0} animales en {1:0.00} s",
                one: "Se ha restaurado la fertilidad de {0} animal en {1:0.00} s"
            );
        internal override string ResetUnconfirmed =>
            " ({0} sin confirmar)";
    }
}

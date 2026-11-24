namespace OffWithTheirNuts {
    internal sealed class BrazilianPortugueseTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new ZeroAndOneSingularPluralRules();

        internal override string SterileStatus =>
            "Esterilizado";
        internal override string SterilizeHint =>
            "Esterilizar";
        internal override string IrreversibleSterilizationHint =>
            "Esterilizar (irreversível)";
        internal override string RestoreFertilityHint =>
            "Restaurar fertilidade";
        internal override string ModActivationCategory =>
            "Ativação do mod";
        internal override string PersonalSettingsCategory =>
            "Configurações pessoais";
        internal override string AdministrationCategory =>
            "Administração";
        internal override string EnabledName =>
            "Ativar mod";
        internal override string EnabledDescription =>
            "Aplicar as restrições de reprodução salvas. No multijogador, o servidor controla esta configuração.";
        internal override string IrreversibleSterilizationName =>
            "Irreversibilidade da esterilização";
        internal override string IrreversibleSterilizationDescription =>
            "Enquanto esta opção estiver ativa, não será possível reverter a esterilização de um único animal. " +
            "Os administradores ainda podem restaurar a fertilidade de todos os animais de uma vez.";
        internal override string Enabled =>
            "Ativado";
        internal override string Disabled =>
            "Desativado";
        internal override string ShortcutName =>
            "Atalho de teclado";
        internal override string ShortcutDescription =>
            "Mire em um animal domesticado e pressione esta tecla ou combinação de teclas.";
        internal override string SterilizationEffectsName =>
            "Efeitos de esterilização";
        internal override string SterilizationEffectsDescription =>
            "Reproduzir sons e efeitos visuais ao esterilizar um animal ou restaurar sua fertilidade.";
        internal override string ResetName =>
            "Reversão da esterilização";
        internal override string ResetDescription =>
            "Restaurar a fertilidade de todos os animais esterilizados no mundo atual.";
        internal override string ResetButton =>
            "Devolver fertilidade a todos";
        internal override string CleanupName =>
            "Remoção de dados do mod";
        internal override string CleanupDescription =>
            "Remover todos os dados do mod do arquivo de salvamento do mundo atual. " +
            "Inicie a limpeza sem outros jogadores conectados. " +
            "Certifique-se de que ninguém entre até o fim da limpeza, pois o mod não bloqueia conexões. " +
            "Para concluir, salve e recarregue o mundo. " +
            "Depois disso, você pode desinstalar o mod.";
        internal override string CleanupButton =>
            "Remover dados do mod do mundo";
        internal override string CleanupPlayersConnected =>
            "Todos os outros jogadores precisam se desconectar para iniciar a limpeza";
        internal override string CleanupCompleted =>
            "Dados do mod removidos. Salve o mundo para manter as alterações";
        internal override string ChangePending =>
            "Já há uma alteração em andamento. Tente novamente quando ela terminar.";
        internal override string TargetUnavailable =>
            "Não foi possível alterar o estado do animal. Aproxime-se e tente novamente.";
        internal override string AlreadySterile =>
            "Este animal já foi esterilizado.";
        internal override string AlreadyFertile =>
            "Este animal já pode se reproduzir.";
        internal override string ChangeUnconfirmed =>
            "Não foi possível confirmar a alteração para este animal.";
        internal override string Sterilized =>
            "Animal esterilizado";
        internal override string FertileAgain =>
            "O animal está fértil de novo";
        internal override string FertilityRestorationBlocked =>
            "O administrador desativou a restauração da fertilidade";
        internal override string ResetRunning =>
            "A restauração da fertilidade já está em andamento.";
        internal override string ResetStarted =>
            "Restaurando a fertilidade dos animais...";
        internal override Plural ResetResult =>
            MakePlural(
                other: "Fertilidade de {0} animais restaurada em {1:0.00} s",
                one: "Fertilidade de {0} animal restaurada em {1:0.00} s"
            );
        internal override string ResetUnconfirmed =>
            " ({0} sem confirmação)";
    }
}

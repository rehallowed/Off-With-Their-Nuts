namespace OffWithTheirNuts {
    internal sealed class RussianTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new RussianPluralRules();

        internal override string SterileStatus =>
            "Стерилизован";
        internal override string SterilizeHint =>
            "Стерилизовать";
        internal override string IrreversibleSterilizationHint =>
            "Стерилизовать (необратимо)";
        internal override string RestoreFertilityHint =>
            "Вернуть фертильность";
        internal override string BreedingCategory =>
            "Размножение";
        internal override string WorldManagementCategory =>
            "Управление миром";
        internal override string EnabledName =>
            "Включить мод";
        internal override string EnabledDescription =>
            "Применять сохранённые статусы стерилизации. В мультиплеере эту настройку задаёт сервер.";
        internal override string IrreversibleSterilizationName =>
            "Необратимая стерилизация";
        internal override string IrreversibleSterilizationDescription =>
            "Пока настройка включена, отмена стерилизации у отдельных животных запрещена. " +
            "Администраторы по-прежнему могут сбросить стерилизацию у всех животных сразу.";
        internal override string Enabled =>
            "Включено";
        internal override string Disabled =>
            "Выключено";
        internal override string ShortcutName =>
            "Назначение клавиши стерилизации";
        internal override string ShortcutDescription =>
            "Наведи прицел на приручённое животное и нажми эту клавишу или сочетание.";
        internal override string SterilizationEffectsName =>
            "Эффекты стерилизации";
        internal override string SterilizationEffectsDescription =>
            "Воспроизводить звук и визуальные эффекты при стерилизации животного или её отмене.";
        internal override string ResetName =>
            "Сброс стерилизации";
        internal override string ResetDescription =>
            "Снять статус стерилизации со всех отмеченных животных текущего мира.";
        internal override string ResetButton =>
            "Сбросить всю стерилизацию";
        internal override string CleanupName =>
            "Удалить данные мода";
        internal override string CleanupDescription =>
            "Удалить все данные мода из сохранения текущего мира. " +
            "Запускай очистку без подключённых игроков. " +
            "Проследи, чтобы никто не подключался до окончания очистки - мод не блокирует подключения. " +
            "Для завершения сохрани и перезагрузи мир. " +
            "После этого мод можно удалить.";
        internal override string CleanupButton =>
            "Удалить данные мода из мира";
        internal override string CleanupPlayersConnected =>
            "Для очистки нужно, чтобы все остальные игроки отключились";
        internal override string CleanupCompleted =>
            "Данные мода удалены. Сохрани мир, чтобы записать изменения";
        internal override string ChangePending =>
            "Изменение уже выполняется. Повтори после завершения.";
        internal override string TargetUnavailable =>
            "Подойди к приручённому животному, которое может размножаться.";
        internal override string AlreadySterile =>
            "Животное уже стерилизовано.";
        internal override string AlreadyFertile =>
            "Размножение уже разрешено.";
        internal override string ChangeUnconfirmed =>
            "Не удалось подтвердить изменение для животного.";
        internal override string Sterilized =>
            "Животное стерилизовано";
        internal override string FertileAgain =>
            "Животное снова фертильно";
        internal override string FertilityRestorationBlocked =>
            "Восстановление фертильности запрещено админом";
        internal override string ResetRunning =>
            "Сброс стерилизации уже выполняется.";
        internal override string ResetStarted =>
            "Снимаем стерилизацию с животных...";
        internal override Plural ResetResult => MakePlural(
            other: "Стерилизация снята у {0} животных за {1:0.00}с",
            one: "Стерилизация снята у {0} животного за {1:0.00}с"
        );
        internal override string ResetUnconfirmed =>
            " (не подтверждено снятие у {0})";
    }
}

namespace OffWithTheirNuts {
    internal sealed class RussianTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new RussianPluralRules();

        internal override string SterileStatus =>
            "Стерилизован";
        internal override string BreedingCategory =>
            "01. Размножение";
        internal override string WorldManagementCategory =>
            "02. Управление миром";
        internal override string EnabledName =>
            "Включить мод";
        internal override string EnabledDescription =>
            "Применять сохранённые запреты. В мультиплеере эту настройку задаёт сервер.";
        internal override string Enabled =>
            "Включено";
        internal override string Disabled =>
            "Выключено";
        internal override string ShortcutName =>
            "Переключить стерилизацию";
        internal override string ShortcutDescription =>
            "Наведи прицел на приручённое животное и нажми эту клавишу или сочетание.";
        internal override string ResetName =>
            "Сброс запретов";
        internal override string ResetDescription =>
            "Снять запреты со всех отмеченных животных текущего мира.";
        internal override string ResetButton =>
            "Сбросить все запреты";
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

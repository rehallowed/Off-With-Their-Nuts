namespace OffWithTheirNuts {
    internal sealed class UkrainianTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new EastSlavicPluralRules();

        internal override string SterileStatus =>
            "Стерилізовано";
        internal override string SterilizeHint =>
            "Стерилізувати";
        internal override string IrreversibleSterilizationHint =>
            "Стерилізувати (незворотно)";
        internal override string RestoreFertilityHint =>
            "Відновити фертильність";
        internal override string ModActivationCategory =>
            "Увімкнення мода";
        internal override string PersonalSettingsCategory =>
            "Індивідуальні налаштування";
        internal override string AdministrationCategory =>
            "Адміністрування";
        internal override string EnabledName =>
            "Увімкнути мод";
        internal override string EnabledDescription =>
            "Застосовувати збережені обмеження на розмноження. У багатокористувацькій грі цей параметр визначає сервер.";
        internal override string IrreversibleSterilizationName =>
            "Незворотність стерилізації";
        internal override string IrreversibleSterilizationDescription =>
            "Поки цей параметр увімкнено, скасувати стерилізацію окремої тварини неможливо. " +
            "Адміністратори можуть відновити фертильність усіх тварин одразу.";
        internal override string Enabled =>
            "Увімкнено";
        internal override string Disabled =>
            "Вимкнено";
        internal override string ShortcutName =>
            "Призначення клавіші";
        internal override string ShortcutDescription =>
            "Наведи приціл на приручену тварину й натисни цю клавішу або комбінацію клавіш.";
        internal override string SterilizationEffectsName =>
            "Ефекти стерилізації";
        internal override string SterilizationEffectsDescription =>
            "Відтворювати звукові й візуальні ефекти під час стерилізації тварини або відновлення її фертильності.";
        internal override string ResetName =>
            "Скасування стерилізації";
        internal override string ResetDescription =>
            "Відновити фертильність усіх стерилізованих тварин у поточному світі.";
        internal override string ResetButton =>
            "Зняти стерилізацію в усіх";
        internal override string CleanupName =>
            "Видалення даних мода";
        internal override string CleanupDescription =>
            "Видалити всі дані мода зі збереження поточного світу. " +
            "Починай очищення, лише якщо жоден інший гравець не підключений. " +
            "Стеж, щоб ніхто не приєднався до завершення очищення, оскільки мод не блокує підключення. " +
            "Щоб завершити очищення, збережи та перезавантаж світ. " +
            "Після цього мод можна видалити.";
        internal override string CleanupButton =>
            "Видалити дані мода зі світу";
        internal override string CleanupPlayersConnected =>
            "Для очищення всі інші гравці мають від'єднатися";
        internal override string CleanupCompleted =>
            "Дані мода видалено. Збережи світ, щоб записати зміни";
        internal override string ChangePending =>
            "Зміна вже триває. Спробуй ще раз після завершення.";
        internal override string TargetUnavailable =>
            "Не вдалося змінити стан тварини. Підійди ближче й спробуй ще раз.";
        internal override string AlreadySterile =>
            "Цю тварину вже стерилізовано.";
        internal override string AlreadyFertile =>
            "Ця тварина вже може розмножуватися.";
        internal override string ChangeUnconfirmed =>
            "Не вдалося підтвердити зміну для цієї тварини.";
        internal override string Sterilized =>
            "Тварину стерилізовано";
        internal override string FertileAgain =>
            "Тварина знову фертильна";
        internal override string FertilityRestorationBlocked =>
            "Адміністратор заборонив відновлення фертильності";
        internal override string ResetRunning =>
            "Відновлення фертильності вже триває.";
        internal override string ResetStarted =>
            "Відновлюємо фертильність тварин...";
        internal override Plural ResetResult =>
            MakePlural(
                other: "Фертильність відновлено у {0} тварин за {1:0.00}с",
                one: "Фертильність відновлено у {0} тварини за {1:0.00}с"
            );
        internal override string ResetUnconfirmed =>
            " (не підтверджено відновлення у {0})";
    }
}

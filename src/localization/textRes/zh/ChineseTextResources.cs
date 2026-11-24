namespace OffWithTheirNuts {
    internal sealed class ChineseTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new InvariantPluralRules();

        internal override string SterileStatus =>
            "已絕育";
        internal override string SterilizeHint =>
            "絕育";
        internal override string IrreversibleSterilizationHint =>
            "絕育（不可逆）";
        internal override string RestoreFertilityHint =>
            "恢復繁殖能力";
        internal override string ModActivationCategory =>
            "模組啟用";
        internal override string PersonalSettingsCategory =>
            "個人設定";
        internal override string AdministrationCategory =>
            "管理";
        internal override string EnabledName =>
            "啟用模組";
        internal override string EnabledDescription =>
            "套用已儲存的繁殖限制。多人遊戲中，此設定由伺服器控制。";
        internal override string IrreversibleSterilizationName =>
            "絕育不可逆性";
        internal override string IrreversibleSterilizationDescription =>
            "啟用此設定時，無法恢復個別動物的繁殖能力。" +
            "管理員仍可一次恢復所有動物的繁殖能力。";
        internal override string Enabled =>
            "已啟用";
        internal override string Disabled =>
            "已停用";
        internal override string ShortcutName =>
            "按鍵設定";
        internal override string ShortcutDescription =>
            "瞄準已馴服的動物，再按下此按鍵或組合鍵。";
        internal override string SterilizationEffectsName =>
            "絕育效果";
        internal override string SterilizationEffectsDescription =>
            "將動物絕育或恢復其繁殖能力時，播放音效與視覺效果。";
        internal override string ResetName =>
            "解除絕育狀態";
        internal override string ResetDescription =>
            "恢復目前世界中所有已絕育動物的繁殖能力。";
        internal override string ResetButton =>
            "解除所有動物的絕育狀態";
        internal override string CleanupName =>
            "模組資料清除";
        internal override string CleanupDescription =>
            "從目前世界的存檔中移除所有模組資料。" +
            "請在沒有其他玩家連線時開始清除。" +
            "請確保清除完成前無人加入，因為模組不會阻擋連線。" +
            "最後請儲存並重新載入世界，之後就可以解除安裝模組。";
        internal override string CleanupButton =>
            "清除世界中的模組資料";
        internal override string CleanupPlayersConnected =>
            "請等所有其他玩家離線後再清除";
        internal override string CleanupCompleted =>
            "已清除模組資料，請儲存世界以保留變更";
        internal override string ChangePending =>
            "正在處理變更，請在完成後再試一次。";
        internal override string TargetUnavailable =>
            "無法變更這隻動物的狀態。請靠近後再試一次。";
        internal override string AlreadySterile =>
            "這隻動物已經絕育。";
        internal override string AlreadyFertile =>
            "這隻動物已經可以繁殖。";
        internal override string ChangeUnconfirmed =>
            "無法確認這隻動物的狀態是否已變更。";
        internal override string Sterilized =>
            "動物已絕育";
        internal override string FertileAgain =>
            "動物已恢復繁殖能力";
        internal override string FertilityRestorationBlocked =>
            "管理員已停用繁殖能力恢復功能";
        internal override string ResetRunning =>
            "正在重設絕育狀態。";
        internal override string ResetStarted =>
            "正在恢復動物的繁殖能力……";
        internal override Plural ResetResult =>
            MakePlural(other: "已在 {1:0.00} 秒內恢復 {0} 隻動物的繁殖能力");
        internal override string ResetUnconfirmed =>
            "（有 {0} 隻尚未確認）";
    }
}

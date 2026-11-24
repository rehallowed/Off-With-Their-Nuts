namespace OffWithTheirNuts {
    internal sealed class JapaneseTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new InvariantPluralRules();

        internal override string SterileStatus =>
            "不妊化済み";
        internal override string SterilizeHint =>
            "不妊化する";
        internal override string IrreversibleSterilizationHint =>
            "不妊化する（元に戻せない）";
        internal override string RestoreFertilityHint =>
            "繁殖能力を回復する";
        internal override string ModActivationCategory =>
            "MODの有効化";
        internal override string PersonalSettingsCategory =>
            "個人設定";
        internal override string AdministrationCategory =>
            "管理";
        internal override string EnabledName =>
            "MODを有効にする";
        internal override string EnabledDescription =>
            "保存済みの繁殖制限を適用します。マルチプレイではサーバーがこの設定を管理します。";
        internal override string IrreversibleSterilizationName =>
            "不妊化の不可逆性";
        internal override string IrreversibleSterilizationDescription =>
            "この設定が有効な間は、個別の動物の不妊化を取り消せません。" +
            "管理者は引き続き、すべての動物の繁殖能力を一括で回復できます。";
        internal override string Enabled =>
            "有効";
        internal override string Disabled =>
            "無効";
        internal override string ShortcutName =>
            "キー設定";
        internal override string ShortcutDescription =>
            "手なずけた動物に照準を合わせ、このキーまたはキーの組み合わせを押してください。";
        internal override string SterilizationEffectsName =>
            "不妊化エフェクト";
        internal override string SterilizationEffectsDescription =>
            "動物を不妊化したときや繁殖能力を回復したときに、効果音と視覚エフェクトを再生します。";
        internal override string ResetName =>
            "不妊化の一括解除";
        internal override string ResetDescription =>
            "現在のワールドで不妊化されたすべての動物の繁殖能力を回復します。";
        internal override string ResetButton =>
            "すべての動物の不妊化を解除";
        internal override string CleanupName =>
            "MODデータの削除";
        internal override string CleanupDescription =>
            "現在のワールドのセーブデータから、このMODのデータをすべて削除します。" +
            "ほかのプレイヤーが接続していない状態で開始してください。" +
            "このMODは接続をブロックしないため、削除が終わるまで誰も参加しないようにしてください。" +
            "最後にワールドを保存し、読み込み直してください。" +
            "その後、このMODをアンインストールできます。";
        internal override string CleanupButton =>
            "ワールドからMODデータを削除";
        internal override string CleanupPlayersConnected =>
            "削除するには、ほかのプレイヤー全員が切断する必要があります";
        internal override string CleanupCompleted =>
            "MODデータを削除しました。変更を反映するにはワールドを保存してください";
        internal override string ChangePending =>
            "変更を処理中です。完了してからもう一度お試しください。";
        internal override string TargetUnavailable =>
            "動物の状態を変更できませんでした。近づいてからもう一度試してください。";
        internal override string AlreadySterile =>
            "この動物はすでに不妊化されています。";
        internal override string AlreadyFertile =>
            "この動物はすでに繁殖できます。";
        internal override string ChangeUnconfirmed =>
            "この動物の不妊化状態が変わったか確認できませんでした。";
        internal override string Sterilized =>
            "動物を不妊化しました";
        internal override string FertileAgain =>
            "動物の繁殖能力を回復しました";
        internal override string FertilityRestorationBlocked =>
            "管理者が繁殖能力の回復を無効にしています";
        internal override string ResetRunning =>
            "繁殖能力の一括回復はすでに進行中です。";
        internal override string ResetStarted =>
            "動物の繁殖能力を回復しています…";
        internal override Plural ResetResult =>
            MakePlural(other: "{1:0.00}秒で{0}匹の動物の繁殖能力を回復しました");
        internal override string ResetUnconfirmed =>
            "（{0}匹は未確認）";
    }
}

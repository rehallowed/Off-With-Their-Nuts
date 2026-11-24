namespace OffWithTheirNuts {
    internal sealed class TurkishTextResources : TextResources {
        internal override PluralRules PluralRules { get; } = new OneOrOtherPluralRules();

        internal override string SterileStatus =>
            "Kısırlaştırıldı";
        internal override string SterilizeHint =>
            "Kısırlaştır";
        internal override string IrreversibleSterilizationHint =>
            "Kısırlaştır (geri alınamaz)";
        internal override string RestoreFertilityHint =>
            "Üreme yetisini geri kazandır";
        internal override string ModActivationCategory =>
            "Modu Etkinleştirme";
        internal override string PersonalSettingsCategory =>
            "Kişisel Ayarlar";
        internal override string AdministrationCategory =>
            "Yönetim";
        internal override string EnabledName =>
            "Modu etkinleştir";
        internal override string EnabledDescription =>
            "Kayıtlı üreme kısıtlamalarını uygula. Çok oyunculu oyunda bu ayarı sunucu belirler.";
        internal override string IrreversibleSterilizationName =>
            "Kısırlaştırmanın kalıcılığı";
        internal override string IrreversibleSterilizationDescription =>
            "Bu ayar açıkken tek tek hayvanların üreme yetisi geri kazandırılamaz. " +
            "Yöneticiler tüm hayvanların üreme yetisini bir kerede geri kazandırabilir.";
        internal override string Enabled =>
            "Etkin";
        internal override string Disabled =>
            "Devre dışı";
        internal override string ShortcutName =>
            "Tuş ataması";
        internal override string ShortcutDescription =>
            "Evcilleştirilmiş bir hayvanı hedef al ve bu tuşa veya tuş kombinasyonuna bas.";
        internal override string SterilizationEffectsName =>
            "Kısırlaştırma efektleri";
        internal override string SterilizationEffectsDescription =>
            "Hayvan kısırlaştırıldığında veya üreme yetisini geri kazandığında ses ve görsel efektleri oynat.";
        internal override string ResetName =>
            "Kısırlaştırmayı geri alma";
        internal override string ResetDescription =>
            "Mevcut dünyadaki tüm kısırlaştırılmış hayvanların üreme yetisini geri kazandır.";
        internal override string ResetButton =>
            "Tüm kısırlaştırmaları kaldır";
        internal override string CleanupName =>
            "Mod verilerinin silinmesi";
        internal override string CleanupDescription =>
            "Mevcut dünyanın kayıt dosyasından tüm mod verilerini kaldır. " +
            "Temizliğe başka oyuncu bağlı değilken başla. " +
            "Mod bağlantıları engellemediği için temizlik bitene kadar kimsenin katılmadığından emin ol. " +
            "İşlemi tamamlamak için dünyayı kaydet ve yeniden yükle. " +
            "Bundan sonra modu kaldırabilirsin.";
        internal override string CleanupButton =>
            "Mod verilerini dünyadan kaldır";
        internal override string CleanupPlayersConnected =>
            "Temizliğe başlamak için diğer tüm oyuncuların bağlantıyı kesmesi gerekir";
        internal override string CleanupCompleted =>
            "Mod verileri kaldırıldı. Değişiklikleri korumak için dünyayı kaydet";
        internal override string ChangePending =>
            "Bir değişiklik zaten yapılıyor. İşlem bitince tekrar dene.";
        internal override string TargetUnavailable =>
            "Hayvanın durumu değiştirilemedi. Yaklaş ve tekrar dene.";
        internal override string AlreadySterile =>
            "Bu hayvan zaten kısırlaştırılmış.";
        internal override string AlreadyFertile =>
            "Bu hayvan zaten üreyebiliyor.";
        internal override string ChangeUnconfirmed =>
            "Bu hayvanın üreme durumundaki değişiklik doğrulanamadı.";
        internal override string Sterilized =>
            "Hayvan kısırlaştırıldı";
        internal override string FertileAgain =>
            "Hayvan yeniden üreyebilir";
        internal override string FertilityRestorationBlocked =>
            "Yönetici üreme yetisini geri kazandırmayı devre dışı bıraktı";
        internal override string ResetRunning =>
            "Hayvanların üreme yetisi zaten geri kazandırılıyor.";
        internal override string ResetStarted =>
            "Hayvanların üreme yetisi geri kazandırılıyor...";
        internal override Plural ResetResult =>
            MakePlural(other: "{0} hayvanın üreme yetisi {1:0.00} saniyede geri kazandırıldı");
        internal override string ResetUnconfirmed =>
            " ({0} doğrulanamadı)";
    }
}

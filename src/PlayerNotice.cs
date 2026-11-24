using System;

namespace OffWithTheirNuts {
    internal enum PlayerNoticeKind : byte {
        ChangePending = 1,
        TargetUnavailable = 2,
        AlreadySterile = 3,
        AlreadyFertile = 4,
        ChangeUnconfirmed = 5,
        Sterilized = 6,
        FertileAgain = 7,
        ResetRunning = 8,
        ResetStarted = 9,
        ResetResult = 10,
        FertilityRestorationBlocked = 11
    }

    internal readonly struct PlayerNotice {
        private readonly PlayerNoticeKind kind;
        private readonly int count;
        private readonly double seconds;
        private readonly int unconfirmed;

        internal PlayerNotice(PlayerNoticeKind kind) {
            this = default;
            this.kind = kind;
        }

        internal PlayerNotice(int count, double seconds, int unconfirmed) {
            kind = PlayerNoticeKind.ResetResult;
            this.count = count;
            this.seconds = seconds;
            this.unconfirmed = unconfirmed;
        }

        internal void Write(ZPackage package) {
            package.Write((byte)kind);
            if (kind == PlayerNoticeKind.ResetResult) {
                package.Write(count);
                package.Write(seconds);
                package.Write(unconfirmed);
            }
        }

        internal static bool TryRead(ZPackage package, int remaining, out PlayerNotice notice) {
            notice = default;
            if (remaining < sizeof(byte)) {
                return false;
            }

            var kind = (PlayerNoticeKind)package.ReadByte();
            if (kind == PlayerNoticeKind.ResetResult) {
                if (remaining != sizeof(byte) + sizeof(int) + sizeof(double) + sizeof(int)) {
                    return false;
                }
                int count = package.ReadInt();
                double seconds = package.ReadDouble();
                int unconfirmed = package.ReadInt();
                if (count < 0 || seconds < 0 || double.IsNaN(seconds) || double.IsInfinity(seconds) || unconfirmed < 0) {
                    return false;
                }
                notice = new PlayerNotice(
                    count: count,
                    seconds: seconds,
                    unconfirmed: unconfirmed
                );
                return true;
            }

            bool isNoticeValid = remaining == sizeof(byte) && Enum.IsDefined(
                enumType: typeof(PlayerNoticeKind),
                value: kind
            );
            if (!isNoticeValid) {
                return false;
            }
            notice = new PlayerNotice(kind);
            return true;
        }

        internal string Format(TextResources textResources) {
            return kind switch {
                PlayerNoticeKind.ChangePending => textResources.ChangePending,
                PlayerNoticeKind.TargetUnavailable => textResources.TargetUnavailable,
                PlayerNoticeKind.AlreadySterile => textResources.AlreadySterile,
                PlayerNoticeKind.AlreadyFertile => textResources.AlreadyFertile,
                PlayerNoticeKind.ChangeUnconfirmed => textResources.ChangeUnconfirmed,
                PlayerNoticeKind.Sterilized => textResources.Sterilized,
                PlayerNoticeKind.FertileAgain => textResources.FertileAgain,
                PlayerNoticeKind.FertilityRestorationBlocked => textResources.FertilityRestorationBlocked,
                PlayerNoticeKind.ResetRunning => textResources.ResetRunning,
                PlayerNoticeKind.ResetStarted => textResources.ResetStarted,
                PlayerNoticeKind.ResetResult => TextMapper.GetFormattedResetResult(
                    localization: textResources,
                    count: count,
                    seconds: seconds,
                    unconfirmed: unconfirmed
                ),
                _ => null
            };
        }

        internal void Show(TextResources textResources) {
            MessageHud hud = MessageHud.instance;
            if (hud == null) {
                return;
            }
            string message = Format(textResources);
            if (message != null) {
                hud.ShowMessage(
                    type: MessageHud.MessageType.TopLeft,
                    text: message,
                    amount: 0,
                    icon: null,
                    showDespiteHiddenHUD: false,
                    log: false
                );
            }
        }
    }
}

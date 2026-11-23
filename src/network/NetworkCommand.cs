namespace OffWithTheirNuts {
    internal enum NetworkCommand : byte {
        AskSettings = 1,
        Settings = 2,
        SetEnabled = 3,
        Toggle = 4,
        ResetAll = 5,
        OwnerWrites = 6,
        LocalizedNotice = 7,
        SterilizationEffect = 8
    }
}

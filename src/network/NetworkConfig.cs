namespace OffWithTheirNuts {
    internal static class NetworkConfig {
        internal const string RpcName = Constants.PluginId + ".control";
        internal const int MaxBatch = 128;
        internal const int TargetWireSize = 12;
        internal const int MaxPacketSize = 4096;
        internal const double ChangeTimeout = 8.0;
        internal const double ResendInterval = 0.65;
    }
}

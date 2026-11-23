namespace OffWithTheirNuts {
    public sealed partial class OffWithTheirNutsPlugin {
        private static bool IsCleanupAvailable(ZNet network) {
            return network != null && network.IsServer() && Player.m_localPlayer != null &&
                ZDOMan.instance != null;
        }

        private void ClearWorldModData() {
            ZNet network = serverSync.CurrentNetwork;
            if (!IsCleanupAvailable(network)) {
                return;
            }

            TextResources textResources = uiLanguage.TextResources;
            if (network.GetPeers().Count != 0) {
                ShowCleanupNotice(textResources.CleanupPlayersConnected);
                return;
            }
            if (resetting || individualChanges.Count != 0) {
                ShowCleanupNotice(textResources.ChangePending);
                return;
            }

            ZDOMan manager = ZDOMan.instance;
            int removed = 0;
            foreach (ZDO zdo in worldObjects(manager).Values) {
                if (ZdoSterilityOperations.TryRemoveSterility(zdo)) {
                    // RemoveBool does not mark the sector for the next save.
                    manager.SetDirtySector(zdo);
                    removed++;
                }
            }

            Logger.LogInfo("World cleanup removed " + removed + " sterility fields.");
            ShowCleanupNotice(textResources.CleanupCompleted);
        }

        private static void ShowCleanupNotice(string message) {
            MessageHud hud = MessageHud.instance;
            if (hud != null) {
                hud.ShowMessage(
                    type: MessageHud.MessageType.Center,
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

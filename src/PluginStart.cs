using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using HarmonyLib;
using Jotunn.Utils;
using static OffWithTheirNuts.Constants;

[assembly: AssemblyTitle(PluginName)]
[assembly: AssemblyVersion(PluginVersion)]
[assembly: AssemblyFileVersion(PluginVersion)]
[assembly: AssemblyInformationalVersion(PluginVersion)]

namespace OffWithTheirNuts {
    
    [BepInPlugin(
        GUID: PluginId,
        Name: PluginName,
        Version: PluginVersion
    )]
    [BepInDependency("com.jotunn.jotunn")]
    [NetworkCompatibility(
        enforceMod: CompatibilityLevel.EveryoneMustHaveMod,
        enforceVersion: VersionStrictness.Patch
    )]
    public sealed partial class OffWithTheirNutsPlugin : BaseUnityPlugin {
        private static OffWithTheirNutsPlugin instance;
        private readonly AnimalHoverText animalHoverText = new AnimalHoverText();
        private Harmony harmony;
        private ServerSync serverSync;
        private SterilizationHotkey hotkey;
        private GameUiLanguage uiLanguage;

        internal static ServerSync ActiveServerSync => instance == null ? null : instance.serverSync;
        internal static GameUiLanguage ActiveUiLanguage => instance == null ? null : instance.uiLanguage;
        private bool RestrictionsActive => serverSync?.RestrictionsActive == true;

        private void Awake() {
            harmony = new Harmony(PluginId);
            try {
                uiLanguage = new GameUiLanguage();
                worldObjects = AccessTools.FieldRefAccess<ZDOMan, Dictionary<ZDOID, ZDO>>("m_objectsByID");
                hotkey = new SterilizationHotkey(
                    restrictionsActive: () => RestrictionsActive,
                    onTargetSelected: ToggleTarget
                );
                BindSettings();
                uiLanguage.Changed += OnUiLanguageChanged;
                var effects = new SterilizationEffects(
                    enabled: effectsSetting,
                    logger: Logger
                );
                serverSync = new ServerSync(
                    enabledSetting: enabledSetting,
                    refreshHotkey: RefreshHotkey,
                    onWorldClosed: OnWorldClosed,
                    startChange: StartChange,
                    startReset: StartReset,
                    showNotice: notice => notice.Show(uiLanguage.TextResources),
                    playSterilizationEffect: effects.PlayIfEnabled
                );
                harmony.PatchAll(typeof(OffWithTheirNutsPlugin).Assembly);
                if (!GameUiLanguage.PatchLanguageSetup(harmony)) {
                    Logger.LogWarning("Valheim's language setup method was not found. Using the startup language.");
                }
                instance = this;
                Logger.LogInfo(PluginName + " " + PluginVersion + " loaded.");
            } catch (Exception error) {
                harmony.UnpatchSelf();
                ReleaseUiLanguage();
                Logger.LogError(PluginName + " was disabled during initialization. " + error);
            }
        }

        private void OnDestroy() {
            if (instance == this) {
                serverSync.CloseWorld();
                instance = null;
            }
            if (shortcutSetting != null) {
                shortcutSetting.SettingChanged -= OnShortcutChanged;
            }
            if (enabledSetting != null) {
                enabledSetting.SettingChanged -= OnEnabledChanged;
            }
            ReleaseUiLanguage();
            harmony?.UnpatchSelf();
        }

        private void ReleaseUiLanguage() {
            if (uiLanguage != null) {
                uiLanguage.Changed -= OnUiLanguageChanged;
            }
        }

        private void OnWorldClosed() {
            resetting = false;
            StopAllCoroutines();
            individualChanges.Clear();
            hotkey.Dispose();
        }
    }
}

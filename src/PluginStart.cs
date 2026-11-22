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
        private Harmony harmony;
        private ServerSync serverSync;
        private SterilizationHotkey hotkey;

        internal static ServerSync ActiveServerSync => instance == null ? null : instance.serverSync;
        private bool RestrictionsActive => serverSync?.RestrictionsActive == true;

        private void Awake() {
            harmony = new Harmony(PluginId);
            try {
                worldObjects = AccessTools.FieldRefAccess<ZDOMan, Dictionary<ZDOID, ZDO>>("m_objectsByID");
                hotkey = new SterilizationHotkey(
                    restrictionsActive: () => RestrictionsActive,
                    onTargetSelected: ToggleTarget
                );
                BindSettings();
                serverSync = new ServerSync(
                    enabledSetting: enabledSetting,
                    refreshHotkey: RefreshHotkey,
                    onWorldClosed: OnWorldClosed,
                    startChange: StartChange,
                    startReset: StartReset,
                    setSterility: SetSterility
                );
                harmony.PatchAll(typeof(OffWithTheirNutsPlugin).Assembly);
                instance = this;
                Logger.LogInfo(PluginName + " " + PluginVersion + " загружен.");
            } catch (Exception error) {
                harmony.UnpatchSelf();
                Logger.LogError(PluginName + " отключён при инициализации. " + error);
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
            harmony?.UnpatchSelf();
        }

        private void OnWorldClosed() {
            resetting = false;
            StopAllCoroutines();
            individualChanges.Clear();
            hotkey.Dispose();
        }
    }
}

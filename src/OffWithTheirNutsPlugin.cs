using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

[assembly: AssemblyTitle(OffWithTheirNuts.OffWithTheirNutsPlugin.PluginName)]
[assembly: AssemblyVersion(OffWithTheirNuts.OffWithTheirNutsPlugin.PluginVersion)]
[assembly: AssemblyFileVersion(OffWithTheirNuts.OffWithTheirNutsPlugin.PluginVersion)]
[assembly: AssemblyInformationalVersion(OffWithTheirNuts.OffWithTheirNutsPlugin.PluginVersion)]

namespace OffWithTheirNuts
{
    [BepInPlugin(PluginId, PluginName, PluginVersion)]
    [BepInDependency("com.jotunn.jotunn")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Patch)]
    public sealed partial class OffWithTheirNutsPlugin : BaseUnityPlugin
    {
        internal const string PluginId = "rehallowed.OffWithTheirNuts";
        internal const string PluginName = "Off With Their Nuts!";
        internal const string PluginVersion = "0.1.0";
        // Keep the original Valheim stable hash so existing worlds retain their flags.
        private const int SterileKey = 516296702;
        private const string SterileStatus = "<color=#FFC6A8>стерилизован</color>";

        private static OffWithTheirNutsPlugin instance;
        private Harmony harmony;
        private ConfigEntry<bool> enabledSetting;
        private ConfigEntry<KeyboardShortcut> shortcutSetting;
        private AccessTools.FieldRef<ZDOMan, Dictionary<ZDOID, ZDO>> worldObjects;
        private Func<Player, bool> takeInput;
        private Func<KeyCode, bool, string> keyPath;
        private InputAction shortcutAction;
        private KeyCode[] shortcutModifiers;
        private string lastStatus;
        private string lastSterileStatus;

        private bool RestrictionsActive => settingsReady && restrictionsEnabled;

        private void Awake()
        {
            harmony = new Harmony(PluginId);
            try
            {
                worldObjects = AccessTools.FieldRefAccess<ZDOMan, Dictionary<ZDOID, ZDO>>("m_objectsByID");
                takeInput = AccessTools.MethodDelegate<Func<Player, bool>>(
                    AccessTools.DeclaredMethod(typeof(Player), "TakeInput", Type.EmptyTypes));
                keyPath = AccessTools.MethodDelegate<Func<KeyCode, bool, string>>(
                    AccessTools.DeclaredMethod(typeof(ZInput), "KeyCodeToPath", new[] { typeof(KeyCode), typeof(bool) }));

                BindSettings();
                harmony.PatchAll(typeof(OffWithTheirNutsPlugin).Assembly);
                instance = this;
                Logger.LogInfo(PluginName + " " + PluginVersion + " загружен.");
            }
            catch (Exception error)
            {
                harmony.UnpatchSelf();
                Logger.LogError(PluginName + " отключён при инициализации. " + error);
            }
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                CloseWorld();
                instance = null;
            }
            if (shortcutSetting != null)
                shortcutSetting.SettingChanged -= OnShortcutChanged;
            if (enabledSetting != null)
                enabledSetting.SettingChanged -= OnEnabledChanged;
            harmony?.UnpatchSelf();
        }

        private void BindSettings()
        {
            enabledSetting = Config.Bind(
                "01. Размножение",
                "Включить мод",
                true,
                new ConfigDescription(
                    "Применять сохранённые запреты. В мультиплеере эту настройку задаёт сервер.",
                    null,
                    new ConfigurationManagerAttributes { CustomDrawer = DrawEnabled, HideDefaultButton = true }));
            shortcutSetting = Config.Bind(
                "01. Размножение",
                "Переключить стерилизацию",
                new KeyboardShortcut(KeyCode.U),
                "Наведи прицел на приручённое животное и нажми эту клавишу или сочетание.");
            Config.Bind(
                "02. Управление миром",
                "Сброс запретов",
                "",
                new ConfigDescription(
                    "Снять запреты со всех отмеченных животных текущего мира.",
                    null,
                    new ConfigurationManagerAttributes { CustomDrawer = DrawReset, HideDefaultButton = true }));
            enabledSetting.SettingChanged += OnEnabledChanged;
            shortcutSetting.SettingChanged += OnShortcutChanged;
        }

        private void DrawEnabled(ConfigEntryBase entry)
        {
            bool current = network == null ? enabledSetting.Value : restrictionsEnabled;
            bool interactable = instance == this &&
                (network == null || network.IsServer() || (settingsReady && canManageWorld));
            bool guiEnabled = GUI.enabled;
            try
            {
                GUI.enabled = guiEnabled && interactable;
                bool requested = GUILayout.Toggle(current, current ? "Включено" : "Выключено");
                if (requested == current)
                    return;

                if (network == null || network.IsServer())
                    enabledSetting.Value = requested;
                else
                    SendToServer(Command.SetEnabled, package => package.Write(requested));
            }
            finally
            {
                GUI.enabled = guiEnabled;
            }
        }

        private void DrawReset(ConfigEntryBase entry)
        {
            bool guiEnabled = GUI.enabled;
            try
            {
                GUI.enabled = guiEnabled && instance == this && network != null &&
                    (network.IsServer() || (settingsReady && canManageWorld));
                if (!GUILayout.Button("Сбросить все запреты"))
                    return;

                if (network.IsServer())
                    StartReset(null);
                else
                    SendToServer(Command.ResetAll);
            }
            finally
            {
                GUI.enabled = guiEnabled;
            }
        }

        private void OnEnabledChanged(object sender, EventArgs args)
        {
            if (network != null && network.IsServer())
            {
                restrictionsEnabled = enabledSetting.Value;
                PublishSettings();
                UpdateInput();
            }
        }

        private void OnShortcutChanged(object sender, EventArgs args)
        {
            DisposeInput();
            UpdateInput();
        }

        private void UpdateInput()
        {
            if (!RestrictionsActive || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                DisposeInput();
                return;
            }
            if (shortcutAction != null)
                return;

            KeyboardShortcut binding = shortcutSetting.Value;
            if (binding.MainKey == KeyCode.None)
                return;

            string path = keyPath(binding.MainKey, false);
            if (string.IsNullOrEmpty(path) || path.EndsWith("/None", StringComparison.OrdinalIgnoreCase))
                return;

            shortcutModifiers = new List<KeyCode>(binding.Modifiers).ToArray();
            shortcutAction = new InputAction(name: PluginId, type: InputActionType.Button, binding: path);
            shortcutAction.performed += OnShortcutPressed;
            shortcutAction.Enable();
        }

        private void DisposeInput()
        {
            if (shortcutAction == null)
                return;

            shortcutAction.performed -= OnShortcutPressed;
            shortcutAction.Dispose();
            shortcutAction = null;
        }

        private void OnShortcutPressed(InputAction.CallbackContext context)
        {
            Player player = Player.m_localPlayer;
            if (!RestrictionsActive || player == null || !Application.isFocused ||
                Cursor.lockState != CursorLockMode.Locked || !takeInput(player))
                return;

            foreach (KeyCode modifier in shortcutModifiers)
            {
                if (!ZInput.GetKey(modifier, false))
                    return;
            }

            Character target = player.GetHoverCreature();
            if (target == null)
                return;

            ZNetView view = target.GetComponent<ZNetView>();
            if (view == null || !view.IsValid() || !CanSterilize(target.gameObject))
                return;

            ZDO zdo = view.GetZDO();
            bool requested = !IsSterile(zdo);
            if (network.IsServer())
                StartChange(null, zdo.m_uid, requested);
            else
                SendToServer(Command.Toggle, package =>
                {
                    package.Write(zdo.m_uid);
                    package.Write(requested);
                });
        }

        private static bool CanSterilize(GameObject target)
        {
            Tameable tameable = target.GetComponent<Tameable>();
            return tameable != null && tameable.IsTamed() &&
                target.GetComponent<Procreation>() != null && target.GetComponent<Growup>() == null;
        }

        private static bool IsSterile(ZDO zdo)
        {
            return zdo != null && zdo.GetBool(SterileKey, false);
        }

        private static void ClearPregnancy(ZDO zdo)
        {
            if (zdo.GetLong(ZDOVars.s_pregnant, 0L) != 0L)
                zdo.Set(ZDOVars.s_pregnant, 0L);
            if (zdo.GetInt(ZDOVars.s_lovePoints, 0) != 0)
                zdo.Set(ZDOVars.s_lovePoints, 0);
        }

        private void PlayBirthEffects(GameObject target)
        {
            if (target == null)
                return;

            try
            {
                Procreation reproduction = target.GetComponent<Procreation>();
                if (reproduction != null)
                {
                    reproduction.m_birthEffects?.Create(target.transform.position, Quaternion.identity, null,
                        1f, -1, ZDOID.None);
                }
            }
            catch (Exception error)
            {
                Logger.LogWarning("Не удалось воспроизвести эффект стерилизации. " + error.Message);
            }
        }

        private string WithSterileStatus(string status)
        {
            switch (status)
            {
                case "$hud_tamehungry":
                    return "$hud_tamehungry, " + SterileStatus;
                case "$hud_tamehappy":
                    return "$hud_tamehappy, " + SterileStatus;
                case "$hud_tamefrightened":
                    return "$hud_tamefrightened, " + SterileStatus;
                case "$hud_tameinprogress":
                    return "$hud_tameinprogress, " + SterileStatus;
                case "":
                case null:
                    return SterileStatus;
            }

            if (status != lastStatus)
            {
                lastStatus = status;
                lastSterileStatus = status + ", " + SterileStatus;
            }
            return lastSterileStatus;
        }

        [HarmonyPatch(typeof(Procreation), "Procreate")]
        private static class ProcreatePatch
        {
            private static bool Prefix(ZNetView ___m_nview)
            {
                OffWithTheirNutsPlugin mod = instance;
                if (mod is null || mod.network is null)
                    return true;
                if (!mod.settingsReady)
                    return false;
                if (!mod.restrictionsEnabled)
                    return true;

                ZDO zdo = ___m_nview?.GetZDO();
                if (!IsSterile(zdo))
                    return true;

                if (zdo.IsOwner())
                    ClearPregnancy(zdo);
                return false;
            }
        }

        [HarmonyPatch(typeof(Procreation), "ReadyForProcreation")]
        private static class ReadyForProcreationPatch
        {
            private static bool Prefix(ref bool __result, ZNetView ___m_nview)
            {
                OffWithTheirNutsPlugin mod = instance;
                if (mod is null || mod.network is null)
                    return true;
                if (mod.settingsReady && (!mod.restrictionsEnabled || !IsSterile(___m_nview?.GetZDO())))
                    return true;

                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(Tameable), "GetStatusString")]
        private static class StatusPatch
        {
            private static void Postfix(ref string __result, ZNetView ___m_nview)
            {
                OffWithTheirNutsPlugin mod = instance;
                if (!(mod is null) && mod.RestrictionsActive && IsSterile(___m_nview?.GetZDO()))
                    __result = mod.WithSterileStatus(__result);
            }
        }
    }

    // Configuration Manager discovers these members by name.
    internal sealed class ConfigurationManagerAttributes
    {
        public Action<ConfigEntryBase> CustomDrawer;
        public bool? HideDefaultButton;
    }
}

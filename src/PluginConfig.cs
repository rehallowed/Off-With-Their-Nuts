using System;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;

namespace OffWithTheirNuts {
    public sealed partial class OffWithTheirNutsPlugin {
        private const string KeySectionModActivation = "keySectionModActivation";
        private const string KeyEnableMod = "keyEnableMod";
        private const int OrderEnableMod = 0;

        private const string KeySectionPersonalSettings = "keySectionPersonalSettings";
        private const string KeySterilizationShortcut = "keySterilizationShortcut";
        private const int OrderToggleSterilization = 1;
        private const string KeySterilizationEffects = "keySterilizationEffects";
        private const int OrderSterilizationEffects = 0;

        private const string KeySectionWorldAdministration = "keySectionWorldAdministration";
        private const string KeyIrreversibleSterilization = "keyIrreversibleSterilization";
        private const int OrderIrreversibleSterilization = 2;
        private const string KeyResetRestrictions = "keyResetRestrictions";
        private const int OrderResetRestrictions = 1;
        private const string KeyRemoveModData = "keyRemoveModData";
        private const int OrderRemoveModData = 0;

        private readonly ConfigurationManagerAttributes enabledAttributes = new ConfigurationManagerAttributes();
        private readonly ConfigurationManagerAttributes irreversibleSterilizationAttributes = new ConfigurationManagerAttributes();
        private readonly ConfigurationManagerAttributes shortcutAttributes = new ConfigurationManagerAttributes();
        private readonly ConfigurationManagerAttributes effectsAttributes = new ConfigurationManagerAttributes();
        private readonly ConfigurationManagerAttributes resetAttributes = new ConfigurationManagerAttributes();
        private readonly ConfigurationManagerAttributes cleanupAttributes = new ConfigurationManagerAttributes();
        private ConfigEntry<bool> enabledSetting;
        private ConfigEntry<bool> irreversibleSterilizationSetting;
        private ConfigEntry<KeyboardShortcut> shortcutSetting;
        private ConfigEntry<bool> effectsSetting;

        private void BindSettings() {
            enabledAttributes.CustomDrawer = DrawEnabled;
            enabledAttributes.HideDefaultButton = true;
            enabledAttributes.Order = OrderEnableMod;
            shortcutAttributes.Order = OrderToggleSterilization;
            effectsAttributes.CustomDrawer = DrawEffects;
            effectsAttributes.Order = OrderSterilizationEffects;
            irreversibleSterilizationAttributes.CustomDrawer = DrawIrreversibleSterilization;
            irreversibleSterilizationAttributes.HideDefaultButton = true;
            irreversibleSterilizationAttributes.Order = OrderIrreversibleSterilization;
            resetAttributes.CustomDrawer = DrawReset;
            resetAttributes.HideDefaultButton = true;
            resetAttributes.Order = OrderResetRestrictions;
            cleanupAttributes.CustomDrawer = DrawCleanup;
            cleanupAttributes.HideDefaultButton = true;
            cleanupAttributes.Order = OrderRemoveModData;
            UpdateConfigLabels();

            enabledSetting = Config.Bind(
                section: KeySectionModActivation,
                key: KeyEnableMod,
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: enabledAttributes.Description,
                    acceptableValues: null,
                    tags: enabledAttributes
                )
            );
            shortcutSetting = Config.Bind(
                section: KeySectionPersonalSettings,
                key: KeySterilizationShortcut,
                defaultValue: new KeyboardShortcut(KeyCode.U),
                configDescription: new ConfigDescription(
                    description: shortcutAttributes.Description,
                    acceptableValues: null,
                    tags: shortcutAttributes
                )
            );
            effectsSetting = Config.Bind(
                section: KeySectionPersonalSettings,
                key: KeySterilizationEffects,
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: effectsAttributes.Description,
                    acceptableValues: null,
                    tags: effectsAttributes
                )
            );
            irreversibleSterilizationSetting = Config.Bind(
                section: KeySectionWorldAdministration,
                key: KeyIrreversibleSterilization,
                defaultValue: false,
                configDescription: new ConfigDescription(
                    description: irreversibleSterilizationAttributes.Description,
                    acceptableValues: null,
                    tags: irreversibleSterilizationAttributes
                )
            );
            Config.Bind(
                section: KeySectionWorldAdministration,
                key: KeyResetRestrictions,
                defaultValue: "",
                configDescription: new ConfigDescription(
                    description: resetAttributes.Description,
                    acceptableValues: null,
                    tags: resetAttributes
                )
            );
            Config.Bind(
                section: KeySectionWorldAdministration,
                key: KeyRemoveModData,
                defaultValue: "",
                configDescription: new ConfigDescription(
                    description: cleanupAttributes.Description,
                    acceptableValues: null,
                    tags: cleanupAttributes
                )
            );
            enabledSetting.SettingChanged += OnWorldSettingChanged;
            irreversibleSterilizationSetting.SettingChanged += OnWorldSettingChanged;
            shortcutSetting.SettingChanged += OnShortcutChanged;
        }

        private void UpdateConfigLabels() {
            TextResources textResources = uiLanguage.TextResources;
            enabledAttributes.SetText(
                category: textResources.ModActivationCategory,
                name: textResources.EnabledName,
                description: textResources.EnabledDescription
            );
            shortcutAttributes.SetText(
                category: textResources.PersonalSettingsCategory,
                name: textResources.ShortcutName,
                description: textResources.ShortcutDescription
            );
            effectsAttributes.SetText(
                category: textResources.PersonalSettingsCategory,
                name: textResources.SterilizationEffectsName,
                description: textResources.SterilizationEffectsDescription
            );
            irreversibleSterilizationAttributes.SetText(
                category: textResources.AdministrationCategory,
                name: textResources.IrreversibleSterilizationName,
                description: textResources.IrreversibleSterilizationDescription
            );
            resetAttributes.SetText(
                category: textResources.AdministrationCategory,
                name: textResources.ResetName,
                description: textResources.ResetDescription
            );
            cleanupAttributes.SetText(
                category: textResources.AdministrationCategory,
                name: textResources.CleanupName,
                description: textResources.CleanupDescription
            );
        }

        private void OnUiLanguageChanged() {
            UpdateConfigLabels();

            // Configuration Manager reads the tags when it builds the list, including when it opens.
            try {
                foreach (PluginInfo plugin in Chainloader.PluginInfos.Values) {
                    BaseUnityPlugin manager = plugin.Instance;
                    if (manager == null) {
                        continue;
                    }
                    Type type = manager.GetType();
                    if (type.FullName != "ConfigurationManager.ConfigurationManager") {
                        continue;
                    }
                    bool isDisplayingWindow = AccessTools.Property(
                        type: type,
                        name: "DisplayingWindow"
                    )?.GetValue(manager) is true;
                    if (isDisplayingWindow) {
                        AccessTools.Method(
                            type: type,
                            name: "BuildSettingList",
                            parameters: Type.EmptyTypes
                        )?.Invoke(
                            obj: manager,
                            parameters: null
                        );
                    }
                    return;
                }
            } catch (Exception error) {
                Logger.LogWarning("Could not refresh the open Configuration Manager window. " + error.Message);
            }
        }

        private void DrawEnabled(ConfigEntryBase entry) {
            bool current = serverSync?.Enabled ?? enabledSetting.Value;
            bool requested = DrawWorldToggle(current);
            if (requested != current) {
                serverSync.RequestEnabled(requested);
            }
        }

        private void DrawIrreversibleSterilization(ConfigEntryBase entry) {
            bool current = serverSync?.IsIrreversibleSterilizationEnabled ?? irreversibleSterilizationSetting.Value;
            bool requested = DrawWorldToggle(current);
            if (requested != current) {
                serverSync.RequestIrreversibleSterilization(requested);
            }
        }

        private bool DrawWorldToggle(bool current) {
            bool guiEnabled = GUI.enabled;
            try {
                GUI.enabled = guiEnabled && instance == this && serverSync.CanChangeWorldSettings;
                return DrawLocalizedToggle(current);
            } finally {
                GUI.enabled = guiEnabled;
            }
        }

        private void DrawEffects(ConfigEntryBase entry) {
            bool current = effectsSetting.Value;
            bool requested = DrawLocalizedToggle(current);
            if (requested != current) {
                effectsSetting.Value = requested;
            }
        }

        private bool DrawLocalizedToggle(bool current) {
            TextResources textResources = uiLanguage.TextResources;
            return GUILayout.Toggle(
                value: current,
                text: current ? textResources.Enabled : textResources.Disabled
            );
        }

        private void DrawReset(ConfigEntryBase entry) {
            bool guiEnabled = GUI.enabled;
            try {
                GUI.enabled = guiEnabled && instance == this && serverSync.CanReset;
                if (!GUILayout.Button(uiLanguage.TextResources.ResetButton)) {
                    return;
                }

                serverSync.RequestReset();
            } finally {
                GUI.enabled = guiEnabled;
            }
        }

        private void DrawCleanup(ConfigEntryBase entry) {
            bool guiEnabled = GUI.enabled;
            try {
                GUI.enabled = guiEnabled && instance == this && IsCleanupAvailable(serverSync.CurrentNetwork);
                if (!GUILayout.Button(uiLanguage.TextResources.CleanupButton)) {
                    return;
                }

                ClearWorldModData();
            } finally {
                GUI.enabled = guiEnabled;
            }
        }

        private void OnWorldSettingChanged(object sender, EventArgs args) {
            serverSync?.RefreshWorldSettings();
        }

        private void OnShortcutChanged(object sender, EventArgs args) {
            hotkey.Dispose();
            RefreshHotkey();
        }

        private void RefreshHotkey() {
            hotkey.Refresh(shortcutSetting.Value);
        }
    }

    // Configuration Manager discovers these members by name.
    internal sealed class ConfigurationManagerAttributes {
        public string Category;
        public string DispName;
        public string Description;
        public Action<ConfigEntryBase> CustomDrawer;
        public bool? HideDefaultButton;
        public int? Order;

        internal void SetText(string category, string name, string description) {
            Category = category;
            DispName = name;
            Description = description;
        }
    }
}

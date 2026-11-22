using System;
using BepInEx.Configuration;
using UnityEngine;

namespace OffWithTheirNuts {
    public sealed partial class OffWithTheirNutsPlugin {
        private ConfigEntry<bool> enabledSetting;
        private ConfigEntry<KeyboardShortcut> shortcutSetting;

        private void BindSettings() {
            enabledSetting = Config.Bind(
                section: "01. Размножение",
                key: "Включить мод",
                defaultValue: true,
                configDescription: new ConfigDescription(
                    description: "Применять сохранённые запреты. В мультиплеере эту настройку задаёт сервер.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { CustomDrawer = DrawEnabled, HideDefaultButton = true }
                )
            );
            shortcutSetting = Config.Bind(
                section: "01. Размножение",
                key: "Переключить стерилизацию",
                defaultValue: new KeyboardShortcut(KeyCode.U),
                description: "Наведи прицел на приручённое животное и нажми эту клавишу или сочетание."
            );
            Config.Bind(
                section: "02. Управление миром",
                key: "Сброс запретов",
                defaultValue: "",
                configDescription: new ConfigDescription(
                    description: "Снять запреты со всех отмеченных животных текущего мира.",
                    acceptableValues: null,
                    tags: new ConfigurationManagerAttributes { CustomDrawer = DrawReset, HideDefaultButton = true }
                )
            );
            enabledSetting.SettingChanged += OnEnabledChanged;
            shortcutSetting.SettingChanged += OnShortcutChanged;
        }

        private void DrawEnabled(ConfigEntryBase entry) {
            bool current = serverSync?.Enabled ?? enabledSetting.Value;
            bool interactable = instance == this && serverSync.CanChangeEnabled;
            bool guiEnabled = GUI.enabled;
            try {
                GUI.enabled = guiEnabled && interactable;
                bool requested = GUILayout.Toggle(
                    value: current,
                    text: current ? "Включено" : "Выключено"
                );
                if (requested == current) {
                    return;
                }

                serverSync.RequestEnabled(requested);
            } finally {
                GUI.enabled = guiEnabled;
            }
        }

        private void DrawReset(ConfigEntryBase entry) {
            bool guiEnabled = GUI.enabled;
            try {
                GUI.enabled = guiEnabled && instance == this && serverSync.CanReset;
                if (!GUILayout.Button("Сбросить все запреты")) {
                    return;
                }

                serverSync.RequestReset();
            } finally {
                GUI.enabled = guiEnabled;
            }
        }

        private void OnEnabledChanged(object sender, EventArgs args) {
            serverSync?.RefreshEnabledSetting();
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
        public Action<ConfigEntryBase> CustomDrawer;
        public bool? HideDefaultButton;
    }
}

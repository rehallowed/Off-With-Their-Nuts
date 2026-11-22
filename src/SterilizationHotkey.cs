using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace OffWithTheirNuts {
    internal sealed class SterilizationHotkey : IDisposable {
        private readonly Func<bool> restrictionsActive;
        private readonly Action<Character> onTargetSelected;
        private readonly Func<Player, bool> takeInput;
        private readonly Func<KeyCode, bool, string> keyPath;
        private InputAction shortcutAction;
        private KeyCode[] shortcutModifiers;

        internal SterilizationHotkey(Func<bool> restrictionsActive, Action<Character> onTargetSelected) {
            this.restrictionsActive = restrictionsActive;
            this.onTargetSelected = onTargetSelected;
            takeInput = AccessTools.MethodDelegate<Func<Player, bool>>(
                AccessTools.DeclaredMethod(
                    type: typeof(Player),
                    name: "TakeInput",
                    parameters: Type.EmptyTypes
                )
            );
            keyPath = AccessTools.MethodDelegate<Func<KeyCode, bool, string>>(
                AccessTools.DeclaredMethod(
                    type: typeof(ZInput),
                    name: "KeyCodeToPath",
                    parameters: new[] { typeof(KeyCode), typeof(bool) }
                )
            );
        }

        internal void Refresh(KeyboardShortcut binding) {
            if (!restrictionsActive() || SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) {
                Dispose();
                return;
            }
            if (shortcutAction != null) {
                return;
            }

            if (binding.MainKey == KeyCode.None) {
                return;
            }

            string path = keyPath(
                arg1: binding.MainKey,
                arg2: false
            );
            if (string.IsNullOrEmpty(path) || path.EndsWith(
                value: "/None",
                comparisonType: StringComparison.OrdinalIgnoreCase
            )) {
                return;
            }

            shortcutModifiers = new List<KeyCode>(binding.Modifiers).ToArray();
            shortcutAction = new InputAction(
                name: Constants.PluginId,
                type: InputActionType.Button,
                binding: path
            );
            shortcutAction.performed += OnShortcutPressed;
            shortcutAction.Enable();
        }

        public void Dispose() {
            if (shortcutAction == null) {
                return;
            }

            shortcutAction.performed -= OnShortcutPressed;
            shortcutAction.Dispose();
            shortcutAction = null;
        }

        private void OnShortcutPressed(InputAction.CallbackContext context) {
            Player player = Player.m_localPlayer;
            if (!restrictionsActive() || player == null || !Application.isFocused ||
                Cursor.lockState != CursorLockMode.Locked || !takeInput(player)) {
                return;
            }

            foreach (KeyCode modifier in shortcutModifiers) {
                if (!ZInput.GetKey(
                    key: modifier,
                    logWarning: false
                )) {
                    return;
                }
            }

            Character target = player.GetHoverCreature();
            if (target != null) {
                onTargetSelected(target);
            }
        }
    }
}

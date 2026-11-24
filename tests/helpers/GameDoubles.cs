using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace BepInEx.Configuration {
    internal sealed class ConfigEntry<T> {
        internal T Value { get; set; }
    }
}

namespace HarmonyLib {
    internal sealed class HarmonyPatch : Attribute {
        internal HarmonyPatch(Type declaringType, string methodName) {
        }
    }

    internal sealed class HarmonyMethod {
        internal readonly MethodInfo Method;

        internal HarmonyMethod(MethodInfo method) {
            Method = method;
        }
    }

    internal sealed class Harmony {
        internal MethodInfo Postfix { get; private set; }

        internal void Patch(MethodInfo original, HarmonyMethod postfix) {
            Postfix = postfix.Method;
        }
    }

    internal static class AccessTools {
        internal static Type TypeByName(string name) {
            return typeof(AccessTools).Assembly.GetType(name);
        }

        internal static MethodInfo DeclaredMethod(Type type, string name, Type[] parameters = null) {
            return type.GetMethod(
                name: name,
                bindingAttr: BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                    BindingFlags.NonPublic | BindingFlags.DeclaredOnly,
                binder: null,
                types: parameters ?? Type.EmptyTypes,
                modifiers: null
            );
        }
    }
}

namespace UnityEngine {
    internal static class Time {
        internal static double realtimeSinceStartupAsDouble { get; set; }
    }

    internal sealed class WaitForSecondsRealtime {
        internal WaitForSecondsRealtime(float seconds) {
        }
    }

    internal sealed class GameObject {
        private readonly Dictionary<Type, object> components = new Dictionary<Type, object>();

        internal Transform transform { get; } = new Transform();

        internal void Add<T>(T component) where T : class {
            components.Add(
                key: typeof(T),
                value: component
            );
        }

        internal T GetComponent<T>() where T : class {
            components.TryGetValue(
                key: typeof(T),
                value: out object component
            );
            return (T)component;
        }
    }

    internal sealed class Transform {
        internal Vector3 localScale { get; set; }
    }

    internal sealed class CapsuleCollider {
        internal bool enabled { get; set; }
        internal float radius { get; set; }

        internal Vector3 ClosestPoint(Vector3 position) {
            throw new NotSupportedException("Collider queries require Unity.");
        }
    }
}

internal static class PlatformPrefs {
    internal static string Language { get; set; }

    internal static string GetString(string name, string defaultValue) {
        return name == "language" ? Language ?? defaultValue : defaultValue;
    }
}

internal sealed class Localization {
    private bool SetupLanguage(string language) {
        PlatformPrefs.Language = language;
        return true;
    }
}

internal sealed class ZNet {
    internal bool Host { get; set; }
    internal ZNetPeer Server { get; set; }
    internal readonly Dictionary<long, ZNetPeer> Peers = new Dictionary<long, ZNetPeer>();
    internal readonly HashSet<string> Admins = new HashSet<string>();

    internal bool IsServer() => Host;
    internal bool IsAdmin(string hostName) => Admins.Contains(hostName);
    internal ZNetPeer GetServerPeer() => Server;
    internal IEnumerable<ZNetPeer> GetPeers() => Peers.Values;
    internal double GetTimeSeconds() => UnityEngine.Time.realtimeSinceStartupAsDouble;

    internal ZNetPeer GetPeer(long uid) {
        Peers.TryGetValue(
            key: uid,
            value: out ZNetPeer peer
        );
        return peer;
    }
}

internal sealed class ZNetPeer {
    internal long m_uid { get; set; }
    internal bool m_server { get; set; }
    internal ZDOID m_characterID { get; set; }
    internal PeerSocket m_socket { get; } = new PeerSocket();
    internal ZRpc m_rpc { get; } = new ZRpc();

    internal bool IsReady() => m_uid != 0L;
}

internal sealed class PeerSocket {
    internal string HostName { get; set; }

    internal string GetHostName() => HostName;
}

internal sealed class ZRpc {
    private Action<ZPackage> receive;

    internal string RegisteredName { get; private set; }
    internal readonly List<ZPackage> Sent = new List<ZPackage>();

    internal void Register<T>(string name, Action<ZRpc, T> f) {
        RegisteredName = name;
        receive = package => f(
            arg1: this,
            arg2: (T)(object)package
        );
    }

    internal void Invoke(string method, ZPackage parameters) {
        Sent.Add(new ZPackage(parameters.GetArray()));
    }

    internal void Deliver(ZPackage packet) {
        receive(new ZPackage(packet.GetArray()));
    }
}

internal sealed class ZDO {
    private readonly Dictionary<int, object> values = new Dictionary<int, object>();

    internal ZDOID m_uid { get; set; }
    internal ushort OwnerRevision { get; set; }
    internal long Owner { get; set; }

    internal bool IsOwner() => Owner == ZDOMan.instance.LocalOwner;
    internal bool HasOwner() => Owner != 0L;
    internal long GetOwner() => Owner;
    internal int GetPrefab() => 0;
    internal Vector3 GetPosition() => default;

    internal bool GetBool(int hash, bool defaultValue) {
        return GetValue(
            hash: hash,
            defaultValue: defaultValue
        );
    }

    internal long GetLong(int hash, long defaultValue) {
        return GetValue(
            hash: hash,
            defaultValue: defaultValue
        );
    }

    internal int GetInt(int hash, int defaultValue) {
        return GetValue(
            hash: hash,
            defaultValue: defaultValue
        );
    }

    private T GetValue<T>(int hash, T defaultValue) {
        bool found = values.TryGetValue(
            key: hash,
            value: out object value
        );
        return found ? (T)value : defaultValue;
    }

    internal void Set<T>(int hash, T value) => values[hash] = value;
    internal bool RemoveBool(int hash) => values.Remove(hash);
}

internal sealed class ZDOMan {
    internal static ZDOMan instance { get; set; } = new ZDOMan();
    internal long LocalOwner { get; set; } = 1L;
    internal readonly Dictionary<ZDOID, ZDO> Records = new Dictionary<ZDOID, ZDO>();
    internal readonly List<ZDOID> ForcedSends = new List<ZDOID>();

    internal ZDO GetZDO(ZDOID id) {
        Records.TryGetValue(
            key: id,
            value: out ZDO record
        );
        return record;
    }

    internal void ForceSendZDO(ZDOID id) {
        ForcedSends.Add(id);
    }
}

internal static class ZDOVars {
    internal const int s_pregnant = 1;
    internal const int s_lovePoints = 2;
    internal const int s_tamed = 3;
}

internal static class GameStringExtensions {
    internal static int GetStableHashCode(this string value) {
        return StringComparer.Ordinal.GetHashCode(value);
    }
}

internal sealed class ZNetView {
    internal ZDO Record { get; set; }
    internal bool IsValid() => Record != null;
    internal ZDO GetZDO() => Record;
}

internal sealed class ZNetScene {
    internal static ZNetScene instance { get; set; }
    internal readonly Dictionary<ZDOID, UnityEngine.GameObject> Instances =
        new Dictionary<ZDOID, UnityEngine.GameObject>();

    internal UnityEngine.GameObject FindInstance(ZDOID id) {
        Instances.TryGetValue(
            key: id,
            value: out UnityEngine.GameObject target
        );
        return target;
    }

    internal UnityEngine.GameObject GetPrefab(int id) {
        throw new NotSupportedException("The test double does not load prefabs.");
    }
}

internal class Character {
    internal UnityEngine.GameObject gameObject { get; } = new UnityEngine.GameObject();
    internal T GetComponent<T>() where T : class => gameObject.GetComponent<T>();
    internal float GetHoverOffset() => 0f;
}

internal sealed class Player : Character {
    internal static Player m_localPlayer { get; set; }
    internal float m_maxInteractDistance { get; set; }
    internal Character Hovered { get; set; }

    internal Character GetHoverCreature() => Hovered;
}

internal sealed class Tameable : Character {
    internal bool IsTamed() => true;
}

internal sealed class Procreation {
}

internal sealed class Growup {
}

internal sealed class MessageHud {
    internal enum MessageType {
        TopLeft
    }

    internal static MessageHud instance { get; set; }

    internal void ShowMessage(
        MessageType type,
        string text,
        int amount,
        object icon,
        bool showDespiteHiddenHUD,
        bool log
    ) {
        throw new NotSupportedException("The test double does not render the HUD.");
    }
}

namespace OffWithTheirNuts {
    // Supplies the fields and Unity entry points normally provided by PluginStart.
    public sealed partial class OffWithTheirNutsPlugin {
        private static OffWithTheirNutsPlugin instance;
        private readonly ServerSync serverSync;
        private readonly GameUiLanguage uiLanguage;
        private readonly AnimalHoverText animalHoverText = new AnimalHoverText();
        private readonly Hotkey hotkey = new Hotkey();
        private bool resetting => false;

        internal static ServerSync ActiveServerSync => instance?.serverSync;
        internal static GameUiLanguage ActiveUiLanguage => instance?.uiLanguage;
        private bool RestrictionsActive => serverSync?.RestrictionsActive == true;
        internal IEnumerator StartedCoroutine { get; private set; }

        internal OffWithTheirNutsPlugin(ServerSync serverSync = null, GameUiLanguage uiLanguage = null) {
            this.serverSync = serverSync;
            this.uiLanguage = uiLanguage;
            instance = this;
        }

        internal void PressShortcut(Character target) => ToggleTarget(target);

        private void StartCoroutine(IEnumerator routine) {
            StartedCoroutine = routine;
        }

        private sealed class Hotkey {
            internal string ShortcutLabel => "U";
        }
    }
}

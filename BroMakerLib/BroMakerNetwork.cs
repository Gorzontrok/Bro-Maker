using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.CustomObjects.Projectiles;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using Networking;
using RocketLib.Network;
using UnityEngine;

namespace BroMakerLib
{
    internal static class BroMakerNetwork
    {
        internal const string ModId = "BroMaker";
        internal const string ProjectilePrefix = "BroMaker:";
        internal const string HeroPrefix = "BroMaker:hero:";
        internal const string HeroSpawnMarker = "BroMaker:hero-spawn";
        private const string BrosKey = "bros";

        private static readonly Dictionary<string, string[]> knownPrefabComponents = new Dictionary<string, string[]>(StringComparer.Ordinal);
        private static bool initialized;
        private static bool active;
        private static bool manifestsPublished;
        private static bool leaveScheduled;

        internal static void Initialize()
        {
            if (initialized) return;

            ModNegotiation.RegisterMod(ModId, Info.VERSION);
            NetworkPrefabs.RegisterResolver(ProjectilePrefix, ResolveProjectilePrefab, EnableCustomProjectile);
            NetworkPrefabs.RegisterResolver(HeroPrefix, ResolveHeroPrefab);
            NetworkPrefabs.OnResolveFailed += OnResolveFailed;
            ModNegotiation.OnSessionEnded += OnSessionEnded;
            initialized = true;
            active = true;
            RegisterKnownProjectilePrefabs();
        }

        internal static void PublishInitialManifests()
        {
            ModNegotiation.SetExtensionData(ModId, BrosKey, SerializeBros(BroMakerStorage.Bros));
            manifestsPublished = true;
            PublishPrefabs();
        }

        internal static void RegisterProjectilePrefab(Component prefab)
        {
            if (prefab == null) return;

            Type projectileType = prefab.GetType();
            if (PresetManager.FindCustomProjectileType(projectileType.Name) != projectileType)
            {
                BMLogger.Error("Custom projectile type name is ambiguous and cannot be networked: " + projectileType.FullName);
                return;
            }

            string id = ProjectilePrefix + projectileType.Name;
            string[] componentNames = GetAdditionalComponentNames(prefab.gameObject, projectileType);
            string[] previous;
            if (!knownPrefabComponents.TryGetValue(id, out previous) || componentNames.Length != 0)
                knownPrefabComponents[id] = componentNames;

            NetworkPrefabs.RegisterInstance(id, prefab.gameObject, EnableCustomProjectile);
            if (manifestsPublished) PublishPrefabs();
        }

        internal static void RegisterHeroPrefab(GameObject prefab, string broName, Type presetType)
        {
            if (prefab == null) return;
            if (presetType == null || BroMakerStorage.IsBroNameAmbiguous(broName))
            {
                BMLogger.Error("Cannot register network bro with an invalid or ambiguous name: " + broName);
                return;
            }

            string id = HeroPrefix + broName;
            knownPrefabComponents[id] = new[] { presetType.FullName };
            NetworkPrefabs.RegisterInstance(id, prefab);
            if (manifestsPublished) PublishPrefabs();
        }

        internal static bool IsBroAvailable(StoredHero hero)
        {
            return GetBroUnavailableReason(hero) == null;
        }

        internal static bool IsBroAvailable(PID publisher, StoredHero hero)
        {
            return GetBroUnavailableReason(publisher, hero) == null;
        }

        internal static string GetBroUnavailableReason(StoredHero hero)
        {
            if (!active) return "BroMaker networking is not initialized";
            if (hero == null || hero.mod == null) return "bro metadata or owning mod is missing";
            if (BroMakerStorage.IsBroNameAmbiguous(hero.name)) return "bro name is duplicated locally";

            string localVersion = ModNegotiation.GetModVersion(PID.MyID, ModId);
            if (string.IsNullOrEmpty(localVersion)) return "local BroMaker manifest is unavailable";
            foreach (PID player in ModNegotiation.GetAllPlayers())
            {
                if (player == PID.MyID) continue;
                string reason = GetPeerBroUnavailableReason(player, hero, localVersion);
                if (reason != null) return reason;
            }
            return null;
        }

        internal static string GetBroUnavailableReason(PID publisher, StoredHero hero)
        {
            if (!active) return "BroMaker networking is not initialized";
            if (hero == null || hero.mod == null) return "bro metadata or owning mod is missing";
            if (BroMakerStorage.IsBroNameAmbiguous(hero.name)) return "bro name is duplicated locally";
            string localVersion = ModNegotiation.GetModVersion(PID.MyID, ModId);
            if (string.IsNullOrEmpty(localVersion)) return "local BroMaker manifest is unavailable";
            return GetPeerBroUnavailableReason(publisher, hero, localVersion);
        }

        private static string GetPeerBroUnavailableReason(PID player, StoredHero hero, string localVersion)
        {
            string peerVersion = ModNegotiation.GetModVersion(player, ModId);
            if (string.IsNullOrEmpty(peerVersion)) return "a peer has not published a BroMaker manifest";
            if (peerVersion != localVersion) return "BroMaker versions differ on a peer";

            Dictionary<string, string> bros = DeserializeBros(ModNegotiation.GetExtensionData(player, ModId, BrosKey));
            if (bros == null) return "a peer's BroMaker character manifest is missing or invalid";
            string peerBroVersion;
            if (!bros.TryGetValue(hero.name, out peerBroVersion)) return "a peer is missing bro " + hero.name;
            if (peerBroVersion != hero.mod.Version) return "owning mod versions differ for bro " + hero.name;
            PrefabIdEntry prefabEntry = NetworkPrefabs.GetPrefabEntry(HeroPrefix + hero.name, player);
            if (prefabEntry == null || !prefabEntry.ComponentsKnown)
                return "a peer's hero prefab manifest is missing or incomplete for bro " + hero.name;
            return null;
        }

        internal static bool IsHeroSpawnData(object[] data, out string broName, out int playerNum, out int variant)
        {
            broName = null;
            playerNum = -1;
            variant = -1;
            if (data == null || data.Length != 4 || !(data[0] is string) ||
                (string)data[0] != HeroSpawnMarker || !(data[1] is string) ||
                !(data[2] is int) || !(data[3] is int)) return false;

            broName = (string)data[1];
            playerNum = (int)data[2];
            variant = (int)data[3];
            return !string.IsNullOrEmpty(broName) && playerNum >= 0 && playerNum < 4 && variant >= 0;
        }

        internal static void Shutdown()
        {
            if (!initialized) return;
            ModNegotiation.SetExtensionData(ModId, BrosKey, SerializeBros(new StoredHero[0]));
            ModNegotiation.SetExtensionData(ModId, "prefabs", NetworkPrefabs.SerializeIdList(new PrefabIdEntry[0]));
            NetworkPrefabs.OnResolveFailed -= OnResolveFailed;
            ModNegotiation.OnSessionEnded -= OnSessionEnded;
            active = false;
            manifestsPublished = false;
            initialized = false;
        }

        private static byte[] SerializeBros(IEnumerable<StoredHero> heroes)
        {
            List<StoredHero> ordered = new List<StoredHero>();
            foreach (StoredHero hero in heroes)
            {
                if (hero == null || hero.mod == null || BroMakerStorage.IsBroNameAmbiguous(hero.name)) continue;
                try
                {
                    if (hero.GetInfo() != null && !string.IsNullOrEmpty(hero.mod.Version)) ordered.Add(hero);
                }
                catch (Exception ex)
                {
                    BMLogger.Error("Cannot publish network bro " + hero.name + ": " + ex.Message);
                }
            }
            ordered.Sort(delegate(StoredHero left, StoredHero right)
            {
                return StringComparer.Ordinal.Compare(left.name, right.name);
            });

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(ordered.Count);
                foreach (StoredHero hero in ordered)
                {
                    writer.Write(hero.name);
                    writer.Write(hero.mod.Version);
                }
                return stream.ToArray();
            }
        }

        private static Dictionary<string, string> DeserializeBros(byte[] data)
        {
            if (data == null) return null;

            try
            {
                using (MemoryStream stream = new MemoryStream(data, false))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int count = reader.ReadInt32();
                    if (count < 0 || count > (stream.Length - stream.Position) / 2)
                        throw new InvalidDataException("Invalid bro count");

                    Dictionary<string, string> bros = new Dictionary<string, string>(StringComparer.Ordinal);
                    for (int i = 0; i < count; i++)
                    {
                        string name = reader.ReadString();
                        string version = reader.ReadString();
                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(version) || bros.ContainsKey(name))
                            throw new InvalidDataException("Invalid or duplicate bro entry");
                        bros.Add(name, version);
                    }
                    if (stream.Position != stream.Length) throw new InvalidDataException("Trailing bro data");
                    return bros;
                }
            }
            catch (IOException)
            {
            }
            catch (InvalidDataException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (FormatException)
            {
            }

            return null;
        }

        private static void PublishPrefabs()
        {
            Dictionary<string, string[]> components = new Dictionary<string, string[]>(knownPrefabComponents, StringComparer.Ordinal);
            foreach (Type type in PresetManager.GetCustomProjectileTypes())
                if (!components.ContainsKey(ProjectilePrefix + type.Name)) components.Add(ProjectilePrefix + type.Name, null);
            foreach (StoredHero hero in BroMakerStorage.Bros)
            {
                CustomBroInfo info;
                try { info = hero.GetInfo(); }
                catch (Exception ex)
                {
                    BMLogger.Error("Cannot publish network bro prefab " + hero.name + ": " + ex.Message);
                    continue;
                }
                if (info == null) continue;
                Type presetType = PresetManager.GetHeroPreset(info.CharacterPreset);
                if (presetType == null || BroMakerStorage.IsBroNameAmbiguous(hero.name)) continue;
                string id = HeroPrefix + hero.name;
                if (!components.ContainsKey(id)) components.Add(id, new[] { presetType.FullName });
            }

            List<PrefabIdEntry> entries = new List<PrefabIdEntry>();
            foreach (KeyValuePair<string, string[]> entry in components)
                entries.Add(new PrefabIdEntry(entry.Key, entry.Value));
            ModNegotiation.SetExtensionData(ModId, "prefabs", NetworkPrefabs.SerializeIdList(entries));
        }

        private static string[] GetAdditionalComponentNames(GameObject prefab, Type projectileType)
        {
            List<string> names = new List<string>();
            Component[] components = prefab.GetComponents<Component>();
            foreach (Component component in components)
            {
                if (component == null) continue;
                Type type = component.GetType();
                if (type == projectileType || type == typeof(Transform) || type == typeof(MeshFilter) ||
                    type == typeof(MeshRenderer) || type == typeof(SpriteSM)) continue;
                names.Add(type.FullName);
            }
            return names.ToArray();
        }

        private static GameObject ResolveProjectilePrefab(string id, PID publisher)
        {
            if (!active) return null;
            string typeName = id.Substring(ProjectilePrefix.Length);
            Type projectileType = PresetManager.FindCustomProjectileType(typeName);
            if (projectileType == null) return FailResolve(id, publisher, "Custom projectile type is missing or ambiguous: " + typeName);

            PrefabIdEntry entry = NetworkPrefabs.GetPrefabEntry(id, publisher);
            if (entry == null || !entry.ComponentsKnown)
                return FailResolve(id, publisher, "Publisher component list is unavailable");

            List<Type> additionalComponents = new List<Type>();
            foreach (string componentName in entry.ComponentNames)
            {
                Type componentType = PresetManager.FindType(componentName);
                if (componentType == null || !typeof(Component).IsAssignableFrom(componentType) || componentType.IsAbstract)
                    return FailResolve(id, publisher, "Component type cannot be resolved: " + componentName);
                if (!additionalComponents.Contains(componentType)) additionalComponents.Add(componentType);
            }

            Type prefabBase = GetCustomProjectileBase(projectileType);
            MethodInfo createPrefab = prefabBase.GetMethod("CreatePrefab", BindingFlags.Public | BindingFlags.Static,
                null, new[] { typeof(List<Type>) }, null);
            Component created = (Component)createPrefab.MakeGenericMethod(projectileType).Invoke(null, new object[] { additionalComponents });
            if (created == null) return FailResolve(id, publisher, "CreatePrefab returned null");
            foreach (Type componentType in additionalComponents)
                if (created.GetComponent(componentType) == null)
                    return FailResolve(id, publisher, "CreatePrefab omitted component: " + componentType.FullName);
            return created.gameObject;
        }

        private static Type GetCustomProjectileBase(Type type)
        {
            if (typeof(CustomProjectile).IsAssignableFrom(type)) return typeof(CustomProjectile);
            if (typeof(CustomGrenade).IsAssignableFrom(type)) return typeof(CustomGrenade);
            return typeof(CustomSachelPack);
        }

        private static GameObject ResolveHeroPrefab(string id, PID publisher)
        {
            if (!active) return null;
            string broName = id.Substring(HeroPrefix.Length);
            StoredHero hero;
            if (!BroMakerStorage.GetStoredHeroByName(broName, out hero))
                return FailResolve(id, publisher, "Bro is not installed: " + broName);
            string unavailableReason = GetBroUnavailableReason(publisher, hero);
            if (unavailableReason != null)
                return FailResolve(id, publisher, unavailableReason);

            PrefabIdEntry entry = NetworkPrefabs.GetPrefabEntry(id, publisher);
            if (entry == null || !entry.ComponentsKnown)
                return FailResolve(id, publisher, "Publisher hero component list is unavailable");

            CustomBroInfo info = hero.GetInfo();
            if (info == null) return FailResolve(id, publisher, "Local bro metadata is unavailable: " + broName);
            Type presetType = PresetManager.GetHeroPreset(info.CharacterPreset);
            if (presetType == null)
                return FailResolve(id, publisher, "Hero preset is unavailable: " + info.CharacterPreset);
            bool presetPublished = false;
            foreach (string componentName in entry.ComponentNames)
                if (componentName == presetType.FullName) presetPublished = true;
            if (!presetPublished)
                return FailResolve(id, publisher, "Published hero preset differs from local preset: " + presetType.FullName);

            return Loaders.LoadHero.GetOrCreateNetworkPrefab(info, presetType);
        }

        private static GameObject FailResolve(string id, PID publisher, string reason)
        {
            BMLogger.Error("Cannot resolve " + id + " from " + publisher + ": " + reason);
            return null;
        }

        private static void EnableCustomProjectile(GameObject instance)
        {
            foreach (Behaviour behaviour in instance.GetComponents<Behaviour>())
                if (behaviour is ICustomProjectile) behaviour.enabled = true;
        }

        private static void OnResolveFailed(string id, PID publisher, string reason)
        {
            if (!active || id == null || !id.StartsWith(HeroPrefix, StringComparison.Ordinal) || leaveScheduled) return;

            leaveScheduled = true;
            string broName = id.Substring(HeroPrefix.Length);
            StoredHero hero;
            string localVersion = BroMakerStorage.GetStoredHeroByName(broName, out hero) && hero.mod != null
                ? hero.mod.Version
                : "not installed";
            string message = "Cannot build networked bro '" + broName + "' from publisher " + publisher +
                " (local owning mod version: " + localVersion + "): " + reason + ". Returning to the main menu.";
            BMLogger.Error(message);
            Main.Log(message, Log.PREFIX);

            new GameObject("BroMaker.NetworkResolveFailure").AddComponent<DeferredNetworkFailureLeave>();
        }

        private static void OnSessionEnded()
        {
            leaveScheduled = false;
        }

        private static void RegisterKnownProjectilePrefabs()
        {
            foreach (CustomProjectile prefab in CustomProjectile.CustomProjectilePrefabs.Values)
                RegisterProjectilePrefab(prefab);
            foreach (CustomGrenade prefab in CustomGrenade.CustomGrenadePrefabs.Values)
                RegisterProjectilePrefab(prefab);
            foreach (CustomSachelPack prefab in CustomSachelPack.CustomSachelPackPrefabs.Values)
                RegisterProjectilePrefab(prefab);
        }

        private sealed class DeferredNetworkFailureLeave : MonoBehaviour
        {
            private void Update()
            {
                Connect.Disconnect();
                GameState.GoBackToFreshMainMenuFromAnywhere();
                Destroy(gameObject);
            }
        }
    }
}

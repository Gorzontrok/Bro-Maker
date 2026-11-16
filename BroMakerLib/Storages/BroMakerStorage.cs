using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Cutscenes;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Unlocks;
using UnityModManagerNet;

namespace BroMakerLib.Storages
{
    public static class BroMakerStorage
    {
        internal static ReadOnlyCollection<BroMakerMod> Mods { get; private set; } = new List<BroMakerMod>().AsReadOnly();

        internal static StoredAbility[] Abiltities
        {
            get { return _abilities.ToArray(); }
        }

        internal static ReadOnlyCollection<StoredHero> Bros
        {
            get { return _brosReadOnly; }
        }

        internal static ReadOnlyCollection<string> BroNames
        {
            get { return _broNamesReadOnly; }
        }

        private static List<BroMakerMod> _mods = new List<BroMakerMod>();
        private static readonly List<BroMakerMod> _incompatibleMods = new List<BroMakerMod>();
        internal static ReadOnlyCollection<BroMakerMod> IncompatibleMods { get; } = _incompatibleMods.AsReadOnly();
        private static List<StoredAbility> _abilities = new List<StoredAbility>();
        private static List<StoredHero> _bros = new List<StoredHero>();
        private static ReadOnlyCollection<StoredHero> _brosReadOnly = new List<StoredHero>().AsReadOnly();
        private static Dictionary<string, StoredHero> _brosByName = new Dictionary<string, StoredHero>();
        private static Dictionary<Type, StoredHero> _brosByType = new Dictionary<Type, StoredHero>();
        private static List<string> _broNames = new List<string>();
        private static ReadOnlyCollection<string> _broNamesReadOnly = new List<string>().AsReadOnly();
        private static readonly Dictionary<Assembly, BroMakerMod> _modByAssembly = new Dictionary<Assembly, BroMakerMod>();

        internal static void Initialize()
        {
            _mods = new List<BroMakerMod>();
            Mods = _mods.AsReadOnly();
            LoadMods();

            _bros = new List<StoredHero>();
            _abilities = new List<StoredAbility>();
            _brosByName = new Dictionary<string, StoredHero>();
            _brosByType = new Dictionary<Type, StoredHero>();
            _broNames = new List<string>();
            _modByAssembly.Clear();

            foreach (BroMakerMod mod in _mods)
            {
                StoreCharactersFromMod(mod);
                StoreAbilitiesFromMod(mod);
            }

            _broNames.Sort();
            _brosReadOnly = _bros.AsReadOnly();
            _broNamesReadOnly = _broNames.AsReadOnly();

            BMLogger.Debug("BroMakerStorage Initialized.");
        }

        /// <summary>Looks up a loaded mod by name, or null if not found.</summary>
        internal static BroMakerMod GetModByName(string modName)
        {
            if (string.IsNullOrEmpty(modName))
            {
                return null;
            }
            foreach (BroMakerMod mod in _mods)
            {
                if (string.Equals(mod.Name, modName, StringComparison.Ordinal))
                {
                    return mod;
                }
            }
            return null;
        }

        /// <summary>Looks up the owning mod's name for a registered bro.</summary>
        internal static string TryResolveModFromBro(string broName)
        {
            if (string.IsNullOrEmpty(broName))
            {
                return null;
            }
            StoredHero hero;
            if (_brosByName.TryGetValue(broName, out hero) && hero != null && hero.mod != null)
            {
                return hero.mod.Name;
            }
            return null;
        }

        /// <summary>Returns the name of the most deeply nested loaded mod whose folder contains the given path, or null if none does.</summary>
        internal static string TryResolveModFromPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }
            string file = filePath.Replace('\\', '/');
            BroMakerMod best = null;
            int bestLength = -1;
            foreach (BroMakerMod mod in _mods)
            {
                if (string.IsNullOrEmpty(mod.Path))
                {
                    continue;
                }
                string folder = mod.Path.Replace('\\', '/').TrimEnd('/');
                if (folder.Length > bestLength && IsSameOrUnder(file, folder))
                {
                    best = mod;
                    bestLength = folder.Length;
                }
            }
            return best?.Name;
        }

        private static bool IsSameOrUnder(string path, string folder)
        {
            return path.StartsWith(folder, StringComparison.OrdinalIgnoreCase) && (path.Length == folder.Length || path[folder.Length] == '/');
        }

        /// <summary>Looks up the mod that owns the given Assembly.</summary>
        internal static BroMakerMod GetModByAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return null;
            }
            BroMakerMod mod;
            return _modByAssembly.TryGetValue(assembly, out mod) ? mod : null;
        }

        internal static void MarkModIncompatible(BroMakerMod mod)
        {
            if (mod != null && !_incompatibleMods.Contains(mod))
            {
                _incompatibleMods.Add(mod);
                UnregisterMod(mod);
            }
        }

        internal static void UnregisterMod(BroMakerMod mod)
        {
            if (mod == null || mod.StoredHeroes.Length == 0)
            {
                return;
            }

            foreach (StoredHero hero in mod.StoredHeroes)
            {
                if (hero == null)
                {
                    continue;
                }

                _bros.Remove(hero);
                _broNames.Remove(hero.name);
                BroSpawnManager.RemoveBro(hero);
                BroUnlockManager.RemoveBro(hero);

                StoredHero registered;
                if (_brosByName.TryGetValue(hero.name, out registered) && ReferenceEquals(registered, hero))
                {
                    _brosByName.Remove(hero.name);
                }
            }

            List<Type> removedTypes = new List<Type>();
            foreach (KeyValuePair<Type, StoredHero> kvp in _brosByType)
            {
                if (Array.IndexOf(mod.StoredHeroes, kvp.Value) >= 0)
                {
                    removedTypes.Add(kvp.Key);
                }
            }
            foreach (Type type in removedTypes)
            {
                _brosByType.Remove(type);
            }

            mod.StoredHeroes = new StoredHero[0];

            BroSpawnManager.BroStatusChanged();
        }

        internal static void RegisterModAssembly(Assembly assembly, BroMakerMod mod)
        {
            if (assembly == null || mod == null)
            {
                return;
            }
            _modByAssembly[assembly] = mod;
        }

        private static void LoadMods()
        {
            BMLogger.Debug("Loading Mods");
            string[] jsonFiles = Directory.GetFiles(DirectoriesManager.StorageDirectory, "*.mod.json", SearchOption.AllDirectories);

            foreach (string jsonFile in jsonFiles)
            {
                BroMakerMod mod = null;
                try
                {
                    mod = BroMakerMod.TryLoad(jsonFile);
                    if (mod != null)
                    {
                        Version modVersion = UnityModManager.ParseVersion(mod.BroMakerVersion);
                        // Mod requires a newer version of BroMaker
                        if (Info.ParsedVersion < modVersion)
                        {
                            _incompatibleMods.Add(mod);
                            BMLogger.Error($"{mod.Name} requires a version of BroMaker >={mod.BroMakerVersion}. Current BroMaker version is {Info.VERSION}", mod: mod.Name);
                            mod.ErrorMessage = mod.Name + " requires a newer version of BroMaker: " + mod.BroMakerVersion + ". Current BroMaker version is: " + Info.VERSION + ". You must update BroMaker.";
                        }
                        // Mod is too old for this version of BroMaker
                        else if (Info.ParsedMinimumVersion > modVersion)
                        {
                            _incompatibleMods.Add(mod);
                            BMLogger.Error($"{mod.Name} will not work with this version of BroMaker. You must update {mod.Name}.", mod: mod.Name);
                            mod.ErrorMessage = mod.Name + " was created using an outdated version of BroMaker (" + mod.BroMakerVersion + ") you must update " + mod.Name + " to a version that supports BroMaker " + Info.ParsedMinimumVersion + ".";
                        }
                        // Versions are compatible
                        else
                        {
                            mod.Initialize();
                            _mods.Add(mod);
                        }
                    }
                }
                catch (Exception ex)
                {
                    string modName = mod != null ? mod.Name : new DirectoryInfo(Path.GetDirectoryName(jsonFile)).Name;
                    BMLogger.Error($"Unable to load BroMaker Mod at {jsonFile}", ex, modName);
                }
            }

            BMLogger.Debug("Finish Loading Mods");
        }

        internal static void PopulateTypesDictionary()
        {
            _brosByType.Clear();

            foreach (StoredHero hero in _bros)
            {
                Type heroType = PresetManager.GetHeroPreset(hero.GetInfo().CharacterPreset);
                if (heroType != null)
                {
                    _brosByType[heroType] = hero;
                }
            }
        }

        public static bool GetStoredHeroByName(string name, out StoredHero hero)
        {
            hero = null;

            if (name.IsNullOrEmpty())
            {
                return false;
            }

            return _brosByName.TryGetValue(name, out hero);
        }

        public static bool GetStoredHeroByCustomHeroType<T>(out StoredHero hero) where T : CustomHero
        {
            return _brosByType.TryGetValue(typeof(T), out hero);
        }

        public static bool GetStoredHeroByCustomHeroType(Type customHeroType, out StoredHero hero)
        {
            hero = null;

            if (customHeroType == null || !typeof(CustomHero).IsAssignableFrom(customHeroType))
            {
                return false;
            }

            return _brosByType.TryGetValue(customHeroType, out hero);
        }

        internal static void StoreCharactersFromMod(BroMakerMod mod)
        {
            if (mod == null || mod.CustomBros == null || mod.CustomBros.Length == 0)
            {
                return;
            }

            List<StoredHero> temp = new List<StoredHero>();
            List<string> names = new List<string>();
            foreach (object broObject in mod.CustomBros)
            {
                if (broObject as string != null && broObject.As<string>().IsNotNullOrEmpty())
                {
                    string path = Path.Combine(mod.Path, broObject as string);
                    if (File.Exists(path))
                    {
                        StoredHero hero = new StoredHero(path, mod);
                        names.Add(hero.name);
                        if (hero.info == null)
                        {
                            continue;
                        }
                        temp.Add(hero);
                        _bros.Add(hero);
                        _brosByName[hero.name] = hero;
                        _broNames.Add(hero.name);
                        BMLogger.Debug($"Found file: '{path}'");
                    }
                }
                else if (broObject as CustomBroInfo != null)
                {
                    CustomBroInfo info = broObject as CustomBroInfo;
                    using (BMLogger.Scope(mod.Name, info.name))
                    {
                        try
                        {
                            info.path = mod.Path;
                            info.Mod = mod;
                            foreach (CustomIntroCutscene cutscene in info.Cutscene)
                            {
                                cutscene.path = mod.Path;
                                cutscene.Owner = info;
                            }
                        }
                        catch (Exception ex)
                        {
                            BMLogger.Error($"Failed to load the bro '{info.name}' written in the mod's manifest", ex);
                            continue;
                        }
                        info.WarnUnknownKeys(mod.ManifestFileName);
                    }

                    StoredHero hero = new StoredHero(info, mod);
                    names.Add(hero.name);
                    temp.Add(hero);
                    _bros.Add(hero);
                    _brosByName[hero.name] = hero;
                    _broNames.Add(hero.name);
                }
            }

            mod.StoredHeroes = temp.ToArray();
            mod.BrosNames = names.ToArray();
        }

        public static void StoreAbilitiesFromMod(BroMakerMod mod)
        {
            if (mod == null || mod.Abilities == null || mod.Abilities.Length == 0)
            {
                return;
            }

            List<StoredAbility> temp = new List<StoredAbility>();
            foreach (object abilityObject in mod.Abilities)
            {
                if (abilityObject == null)
                {
                    continue;
                }

                if (abilityObject as string != null && abilityObject.As<string>().IsNotNullOrEmpty())
                {
                    string path = Path.Combine(mod.Path, abilityObject as string);
                    if (File.Exists(path))
                    {
                        StoredAbility ability;
                        try
                        {
                            ability = new StoredAbility(path);
                        }
                        catch (Exception e)
                        {
                            BMLogger.Error($"Failed to load ability JSON at '{path}'", e, mod.Name);
                            continue;
                        }
                        temp.Add(ability);
                        _abilities.Add(ability);
                        BMLogger.Debug($"Found file: '{path}'");
                    }
                }
                else if (abilityObject as AbilityInfo != null)
                {
                    AbilityInfo info = abilityObject as AbilityInfo;
                    info.path = mod.Path;
                    info.Mod = mod;
                    StoredAbility ability = new StoredAbility(info);
                    temp.Add(ability);
                    _abilities.Add(ability);
                }
            }
            mod.StoredAbilities = temp.ToArray();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using UnityModManagerNet;

namespace BroMakerLib.Storages
{
    public static class BroMakerStorage
    {
        public static List<BroMakerMod> mods = new List<BroMakerMod>();
        public static List<BroMakerMod> incompatibleMods = new List<BroMakerMod>();

        public static StoredAbility[] Abiltities
        {
            get { return _abilities.ToArray(); }
        }
        public static ReadOnlyCollection<StoredHero> Bros
        {
            get { return _brosReadOnly; }
        }
        public static ReadOnlyCollection<string> BroNames
        {
            get { return _broNamesReadOnly; }
        }

        private static List<StoredAbility> _abilities = new List<StoredAbility>();
        private static List<StoredHero> _bros = new List<StoredHero>();
        private static ReadOnlyCollection<StoredHero> _brosReadOnly;
        private static Dictionary<string, StoredHero> _brosByName = new Dictionary<string, StoredHero>();
        private static Dictionary<Type, StoredHero> _brosByType = new Dictionary<Type, StoredHero>();
        private static List<string> _broNames = new List<string>();
        private static ReadOnlyCollection<string> _broNamesReadOnly;

        public static void Initialize()
        {
            mods = new List<BroMakerMod>();
            LoadMods();

            _bros = new List<StoredHero>();
            _abilities = new List<StoredAbility>();
            _brosByName = new Dictionary<string, StoredHero>();
            _brosByType = new Dictionary<Type, StoredHero>();
            _broNames = new List<string>();

            foreach (BroMakerMod mod in mods)
            {
                StoreCharactersFromMod(mod);
                StoreAbilitiesFromMod(mod);
            }

            _broNames.Sort();
            _brosReadOnly = _bros.AsReadOnly();
            _broNamesReadOnly = _broNames.AsReadOnly();

            BMLogger.Debug("BroMakerStorage Initialized.");
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
                        string modName = mod.Name != string.Empty ? mod.Name : "This bro";
                        string modNameLowerCase = mod.Name != string.Empty ? mod.Name : "this bro";
                        // Mod requires a newer version of BroMaker
                        if (Info.ParsedVersion < modVersion)
                        {
                            incompatibleMods.Add(mod);
                            BMLogger.Error($"{modName} requires a version of BroMaker >={mod.BroMakerVersion}. Current BroMaker version is {Info.VERSION}");
                            mod.ErrorMessage = modName + " requires a newer version of BroMaker: " + mod.BroMakerVersion + ". Current BroMaker version is: " + Info.VERSION + ". You must update BroMaker.";
                        }
                        // Mod is too old for this version of BroMaker
                        else if (Info.ParsedMinimumVersion > modVersion)
                        {
                            incompatibleMods.Add(mod);
                            BMLogger.Error($"{modName} will not work with this version of BroMaker. You must update {modName}.");
                            mod.ErrorMessage = modName + " was created using an outdated version of BroMaker (" + mod.BroMakerVersion + ") you must update " + modNameLowerCase + " to a version that supports BroMaker " + Info.ParsedMinimumVersion + ".";
                        }
                        // Versions are compatible
                        else
                        {
                            // Let user know the bro may have compatibility issues with this version
                            if (Info.ParsedSuggestedMinimumVersion > modVersion)
                            {
                                mod.ErrorMessage = modName + " was created using an outdated version of BroMaker (" + mod.BroMakerVersion + ") you may experience bugs using it on this version of BroMaker. It's recommended that you update " + modNameLowerCase + " to a newer version if possible.";
                            }
                            mod.Initialize();
                            mods.Add(mod);
                        }
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog($"Unable to load BroMaker Mod at {jsonFile}.", ex);
                }
            }
            BMLogger.Debug("Finish Loading Mods");
        }

        public static void PopulateTypesDictionary()
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
                return false;

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
                return false;

            return _brosByType.TryGetValue(customHeroType, out hero);
        }

        public static void StoreCharactersFromMod(BroMakerMod mod)
        {
            if (mod == null || mod.CustomBros == null || mod.CustomBros.Length == 0)
                return;

            var temp = new List<StoredHero>();
            foreach (var broObject in mod.CustomBros)
            {
                if (broObject as string != null && broObject.As<string>().IsNotNullOrEmpty())
                {
                    var path = Path.Combine(mod.Path, broObject as string);
                    if (File.Exists(path))
                    {
                        var hero = new StoredHero(path, mod);
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
                    info.path = mod.Path;
                    foreach (var cutscene in info.Cutscene)
                    {
                        cutscene.path = mod.Path;
                    }
                    var hero = new StoredHero(info, mod);
                    temp.Add(hero);
                    _bros.Add(hero);
                    _brosByName[hero.name] = hero;
                    _broNames.Add(hero.name);
                }
            }
            mod.StoredHeroes = temp.ToArray();
        }

        public static void StoreAbilitiesFromMod(BroMakerMod mod)
        {
            if (mod == null || mod.Abilities == null || mod.Abilities.Length == 0)
                return;

            var temp = new List<StoredAbility>();
            foreach (var abilityObject in mod.Abilities)
            {
                if (abilityObject == null)
                    continue;

                if (abilityObject as string != null && abilityObject.As<string>().IsNotNullOrEmpty())
                {
                    var path = Path.Combine(mod.Path, abilityObject as string);
                    if (File.Exists(path))
                    {
                        var ability = new StoredAbility(path);
                        temp.Add(ability);
                        _abilities.Add(ability);
                        BMLogger.Debug($"Found file: '{path}'");
                    }
                }
                else if (abilityObject as AbilityInfo != null)
                {
                    AbilityInfo info = abilityObject as AbilityInfo;
                    info.path = mod.Path;
                    var ability = new StoredAbility(info);
                    temp.Add(ability);
                    _abilities.Add(ability);
                }
            }
            mod.StoredAbilities = temp.ToArray();
        }
    }
}

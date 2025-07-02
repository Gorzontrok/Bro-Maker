using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        public static StoredHero[] Bros
        {
            get { return _bros.ToArray(); }
        }

        private static List<StoredAbility> _abilities = new List<StoredAbility>();
        private static List<StoredHero> _bros = new List<StoredHero>();

        public static void Initialize()
        {
            mods = new List<BroMakerMod>();
            LoadMods();

            _bros = new List<StoredHero>();
            _abilities = new List<StoredAbility>();

            foreach (BroMakerMod mod in mods)
            {
                StoreCharactersFromMod(mod);
                StoreAbilitiesFromMod(mod);
            }

            BMLogger.Debug("MakerObjectStorage Initialized.");
        }

        private static void LoadMods()
        {
            BMLogger.Debug( "Loading Mods" );
            string[] jsonFiles = Directory.GetFiles( DirectoriesManager.StorageDirectory, "*.mod.json", SearchOption.AllDirectories );

            foreach ( string jsonFile in jsonFiles )
            {
                BroMakerMod mod = null;
                try
                {
                    mod = BroMakerMod.TryLoad( jsonFile );
                    if ( mod != null )
                    {
                        Version modVersion = UnityModManager.ParseVersion( mod.BroMakerVersion );
                        string modName = mod.Name != string.Empty ? mod.Name : "This bro";
                        string modNameLowerCase = mod.Name != string.Empty ? mod.Name : "this bro";
                        // Mod requires a newer version of BroMaker
                        if ( Info.ParsedVersion < modVersion )
                        {
                            incompatibleMods.Add( mod );
                            BMLogger.Error( $"{modName} requires a version of BroMaker >={mod.BroMakerVersion}. Current BroMaker version is {Info.VERSION}" );
                            mod.ErrorMessage = modName + " requires a newer version of BroMaker: " + mod.BroMakerVersion + ". Current BroMaker version is: " + Info.VERSION + ". You must update BroMaker.";
                        }
                        // Mod is too old for this version of BroMaker
                        else if ( Info.ParsedMinimumVersion > modVersion )
                        {
                            incompatibleMods.Add( mod );
                            BMLogger.Error( $"{modName} will not work with this version of BroMaker. You must update {modName}." );
                            mod.ErrorMessage = modName + " was created using an outdated version of BroMaker (" + mod.BroMakerVersion + ") you must update " + modNameLowerCase + " to a version that supports BroMaker " + Info.ParsedMinimumVersion + ".";
                        }
                        // Versions are compatible
                        else
                        {
                            // Let user know the bro may have compatibility issues with this version
                            if ( Info.ParsedSuggestedMinimumVersion > modVersion )
                            {
                                mod.ErrorMessage = modName + " was created using an outdated version of BroMaker (" + mod.BroMakerVersion + ") you may experience bugs using it on this version of BroMaker. It's recommended that you update " + modNameLowerCase + " to a newer version if possible.";
                            }
                            mod.Initialize();
                            mods.Add( mod );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    BMLogger.ExceptionLog( $"Unable to load BroMaker Mod at {jsonFile}.", ex );
                }
            }
            BMLogger.Debug( "Finish Loading Mods" );
        }

        public static StoredAbility GetAbilityByName(string name)
        {
            if (_abilities.IsNullOrEmpty())
                return new StoredAbility();

            return _abilities.FirstOrDefault(s => s.name == name);
        }

        public static StoredHero GetHeroByObject(object obj, BroMakerMod mod)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            if (_bros.IsNullOrEmpty())
                return new StoredHero();

            string name = null;
            if (obj is string)
            {

            }

            foreach (StoredHero storedHero in _bros)
            {
                if (storedHero.mod == mod && storedHero.name == name)
                    return storedHero;
            }

            return new StoredHero();
        }

        public static StoredHero GetHeroByName(string name, BroMakerMod mod)
        {
            if (_bros.IsNullOrEmpty() || name.IsNullOrEmpty())
                return new StoredHero();

            foreach (StoredHero storedHero in _bros)
            {
                if (storedHero.mod == mod && storedHero.name == name)
                    return storedHero;
            }

            return new StoredHero();
        }

        public static StoredHero GetHeroByName( string name )
        {
            if ( _bros.IsNullOrEmpty() || name.IsNullOrEmpty() )
                return new StoredHero();

            foreach ( StoredHero storedHero in _bros )
            {
                if ( storedHero.name == name )
                    return storedHero;
            }

            return new StoredHero();
        }

        public static StoredHero GetHeroByType<T>() where T : CustomHero
        {
            if ( _bros.IsNullOrEmpty() )
                return new StoredHero();

            foreach ( StoredHero storedHero in _bros )
            {
                if ( PresetManager.GetHeroPreset( storedHero.GetInfo().characterPreset ) == typeof( T ) )
                {
                    return storedHero;
                }
            }

            return new StoredHero();
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

using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.ModManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static UnityModManagerNet.UnityModManager.Param;

namespace BroMakerLib.Storages
{
    public static class MakerObjectStorage
    {
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
            _bros = new List<StoredHero>();
            _abilities = new List<StoredAbility>();

            foreach (BroMakerMod mod in ModLoader.mods)
            {
                StoreCharactersFromMod(mod);
                StoreAbilitiesFromMod(mod);
            }

            BMLogger.Debug("MakerObjectStorage Initialized.");
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
                    info.cutscene.path = mod.Path;
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

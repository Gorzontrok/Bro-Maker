using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BroMakerLib.Loggers;

namespace BroMakerLib.Storages
{
    public static class MakerObjectStorage
    {
        public static StoredAbility[] Abiltities
        {
            get { return _abilities.ToArray(); }
        }
        public static StoredCharacter[] Bros
        {
            get { return _bros.ToArray(); }
        }
        public static string[] BrosNames
        {
            get
            {
                if (_brosNames == null || _brosNames.Length != _bros.Count)
                {
                    _brosNames = _bros.Select((sc) => sc.ToString()).ToArray();
                }
                return _brosNames;
            }
        }
        public static StoredGrenade[] Grenades
        {
            get { return _grenades.ToArray(); }
        }
        public static StoredProjectile[] Projectiles
        {
            get { return _projectiles.ToArray(); }
        }
        public static StoredWeapon[] Weapons
        {
            get { return _weapons.ToArray(); }
        }

        private static List<StoredAbility> _abilities = new List<StoredAbility>();
        private static List<StoredCharacter> _bros = new List<StoredCharacter>();
        private static string[] _brosNames = null;
        private static List<StoredGrenade> _grenades = new List<StoredGrenade>();
        private static List<StoredProjectile> _projectiles = new List<StoredProjectile>();
        private static List<StoredWeapon> _weapons = new List<StoredWeapon>();

        public static void Initialize()
        {
            _bros = new List<StoredCharacter>();
            _weapons = new List<StoredWeapon>();
            _grenades = new List<StoredGrenade>();
            _abilities = new List<StoredAbility>();
            _projectiles = new List<StoredProjectile>();

            BMLogger.Debug("MakerObjectStorage Initialized.");
        }

        public static StoredAbility GetAbilityByName(string name)
        {
            if (_abilities.IsNullOrEmpty())
                return new StoredAbility();

            return _abilities.FirstOrDefault(s => s.name == name);
        }

        public static void StoreCharactersFromMod(BroMakerMod mod)
        {
            if (mod == null || mod.CustomBros == null || mod.CustomBros.Length == 0)
                return;

            foreach (var broFile in mod.CustomBros)
            {
                if (broFile.IsNotNullOrEmpty())
                {
                    var path = Path.Combine(mod.Path, broFile);
                    if (File.Exists(path))
                    {
                        _bros.Add(new StoredCharacter(path));
                        BMLogger.Debug($"Found file: '{path}'");
                    }
                }
            }
        }

        public static void StoreAbilitiesFromMod(BroMakerMod mod)
        {
            if (mod == null || mod.Abilities == null || mod.Abilities.Length == 0)
                return;

            foreach (var broFile in mod.Abilities)
            {
                if (broFile.IsNotNullOrEmpty())
                {
                    var path = Path.Combine(mod.Path, broFile);
                    if (File.Exists(path))
                    {
                        _abilities.Add(new StoredAbility(path));
                        BMLogger.Debug($"Found file: '{path}'");
                    }
                }
            }
        }

        [Obsolete("Objects are now loaded from Mods")]
        private static void StoreJsonFiles()
        {
            StoreCharacterJsonFiles();
            StoreWeaponJsonFiles();
            StoreAbilityJsonFiles();
            StoreGrenadeJsonFiles();
            StoreProjectileJsonFiles();
        }

        private static void StoreCharacterJsonFiles()
        {
            var jsonFiles = Directory.GetFiles(DirectoriesManager.BrosDirectory, "*.json", SearchOption.AllDirectories);
            foreach (var file in jsonFiles)
            {
                if (!file.Contains(".mod."))
                {
                    _bros.Add(new StoredCharacter(file));
                    BMLogger.Debug($"Found file: '{file}'");
                }
            }
        }
        private static void StoreWeaponJsonFiles()
        {
            var  jsonFiles = Directory.GetFiles(DirectoriesManager.WeaponsDirectory, "*.json", SearchOption.AllDirectories);
            foreach (var file in jsonFiles)
            {
                _weapons.Add(new StoredWeapon(file));
                BMLogger.Debug($"Found file: '{file}'");
            }
        }
        private static void StoreAbilityJsonFiles()
        {
            var jsonFiles = Directory.GetFiles(DirectoriesManager.AbilitiesDirectory, "*.json", SearchOption.AllDirectories);
            foreach (var file in jsonFiles)
            {
                _abilities.Add(new StoredAbility(file));
                BMLogger.Debug($"Found file: '{file}'");
            }
        }
        private static void StoreGrenadeJsonFiles()
        {
            var jsonFiles = Directory.GetFiles(DirectoriesManager.GrenadesDirectory, "*.json", SearchOption.AllDirectories);
            foreach (var file in jsonFiles)
            {
                _grenades.Add(new StoredGrenade(file));
                BMLogger.Debug($"Found file: '{file}'");
            }
        }
        private static void StoreProjectileJsonFiles()
        {
            var jsonFiles = Directory.GetFiles(DirectoriesManager.ProjectilesDirectory, "*.json", SearchOption.AllDirectories);
            foreach (var file in jsonFiles)
            {
                _projectiles.Add(new StoredProjectile(file));
                BMLogger.Debug($"Found file: '{file}'");
            }
        }
    }
}

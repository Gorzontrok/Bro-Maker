﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using BroMakerLib.Loggers;
using HarmonyLib;
using BroMakerLib;

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

            StoreJsonFiles();
            BMLogger.Debug("MakerObjectStorage Initialized.");
        }

        public static StoredAbility GetAbiltyByName(string name)
        {
            return _abilities.First(s => s.name == name);
        }

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
                _bros.Add(new StoredCharacter(file));
                BMLogger.Debug($"Found file: '{file}'");
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
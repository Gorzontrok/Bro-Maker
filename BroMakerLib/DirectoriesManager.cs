using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BroMakerLib.Loggers;

namespace BroMakerLib
{
    public static class DirectoriesManager
    {
        public const string JSON_EXTENSION = ".json";

        public static string StorageDirectory
        {
            get { return _storageDirectory; }
            set
            {
                _storageDirectory = value;
                UpdateDirectories();
                CheckDirectories();
            }
        }
        public static string BrosDirectory { get; private set; }
        public static string WeaponsDirectory { get; private set; }
        public static string AbilitiesDirectory { get; private set; }
        public static string ProjectilesDirectory { get; private set; }
        public static string GrenadesDirectory { get; private set; }
        public static string CutscenesDirectory { get; private set; }

        private static string _storageDirectory;

        public static void Initialize()
        {
            UpdateDirectories();
            CheckDirectories();
            BMLogger.Debug("DirectoriesManager Initialized.");
        }

        public static void UpdateDirectories()
        {
            if (string.IsNullOrEmpty(_storageDirectory))
                _storageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "BroMaker_Storage\\");

            BrosDirectory = Path.Combine(StorageDirectory, "Bros\\");
            AbilitiesDirectory = Path.Combine(StorageDirectory, "Abilities\\");
            WeaponsDirectory = Path.Combine(StorageDirectory, "Weapons\\");
            AbilitiesDirectory = Path.Combine(StorageDirectory, "Abilties\\");
            GrenadesDirectory = Path.Combine(StorageDirectory, "Grenades\\");
            ProjectilesDirectory = Path.Combine(StorageDirectory, "Projectiles\\");
            CutscenesDirectory = Path.Combine(StorageDirectory, "Cutscenes\\");
        }

        public static void CheckDirectories()
        {
            if (!Directory.Exists(_storageDirectory))
                Directory.CreateDirectory(_storageDirectory);

            // Legacy
            if (!Directory.Exists(BrosDirectory))
                Directory.CreateDirectory(BrosDirectory);

            if (!Directory.Exists(WeaponsDirectory))
                Directory.CreateDirectory(WeaponsDirectory);

            if (!Directory.Exists(AbilitiesDirectory))
                Directory.CreateDirectory(AbilitiesDirectory);

            if (!Directory.Exists(GrenadesDirectory))
                Directory.CreateDirectory(GrenadesDirectory);

            if (!Directory.Exists(ProjectilesDirectory))
                Directory.CreateDirectory(ProjectilesDirectory);

            if (!Directory.Exists(CutscenesDirectory))
                Directory.CreateDirectory(CutscenesDirectory);
        }

        [Obsolete("Assemblies are loaded from Mods now")]
        public static Assembly[] LoadAssembliesInStorage()
        {
            var assemblies = new List<Assembly>();
            var assembliesFiles = Directory.GetFiles(StorageDirectory, "*.dll", SearchOption.AllDirectories);
            foreach( var file in assembliesFiles)
            {
                assemblies.Add(Assembly.LoadFile(file));
            }
            return assemblies.ToArray();
        }
    }
}

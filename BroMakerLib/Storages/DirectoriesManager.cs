using System.IO;
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
        }
    }
}

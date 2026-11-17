using System.IO;
using BroMakerLib.Loggers;
using RocketLib.Utils;

namespace BroMakerLib
{
    public static class DirectoriesManager
    {
        public const string JSON_EXTENSION = ".json";

        public static string StorageDirectory
        {
            get { return _storageDirectory; }
        }

        public static string BrosDirectory { get; private set; }
        public static string WeaponsDirectory { get; private set; }
        public static string ProjectilesDirectory { get; private set; }
        public static string CutscenesDirectory { get; private set; }

        private static string _storageDirectory;

        internal static void Initialize()
        {
            UpdateDirectories();
            CheckDirectories();
            BMLogger.Debug("DirectoriesManager Initialized.");
        }

        internal static void UpdateDirectories()
        {
            if (string.IsNullOrEmpty(_storageDirectory))
            {
                _storageDirectory = Path.Combine(RocketLibUtils.GetRootDirectory(), "BroMaker_Storage");
            }

            BrosDirectory = Path.Combine(StorageDirectory, "Bros");
            WeaponsDirectory = Path.Combine(StorageDirectory, "Weapons");
            ProjectilesDirectory = Path.Combine(StorageDirectory, "Projectiles");
            CutscenesDirectory = Path.Combine(StorageDirectory, "Cutscenes");
        }

        internal static void CheckDirectories()
        {
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
        }
    }
}

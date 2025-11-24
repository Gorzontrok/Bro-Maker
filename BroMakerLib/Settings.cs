using System.Collections.Generic;
using Newtonsoft.Json;
using RocketLib.Settings;

namespace BroMakerLib
{
    public class Settings : JsonModSettings
    {
        [JsonIgnore]
        public static Settings instance;
        [JsonIgnore]
        public static string Directory => Main.mod.ConfigPath;

        // These variables allow other mods to control custom bro spawning
        [JsonIgnore]
        public bool overrideNextBroSpawn = false;
        [JsonIgnore]
        public string nextBroSpawn = "";
        [JsonIgnore]
        public bool disableSpawning = false;

        public bool equalSpawnProbability = true;
        public bool automaticSpawn = true;
        public float automaticSpawnProbabilty = 25;
        public bool maxHealthAtOne = true;
        public bool onlyCustomInHardcore = false;
        public bool disableCustomAvatarFlash = true;
        public bool developerMode = false;
        public bool scaleUIWithWindowWidth = false;
        public bool disableTooltips = false;
        public bool scaleUIHeight = false;
        public bool debugLogs = false;

        public List<string> _enabledBros = new List<string>();
        // These lists are for tracking bro unlocks in IronBro
        public List<List<string>> _notUnlockedBros;
        public List<List<string>> _availableBros;

        // Settings menu
        public bool showGeneralSettings = true;
        public bool showSpawnSettings = true;
        public bool showDeveloperSettings = false;

        public static void Load()
        {
            instance = JsonModSettings.Load<Settings>(Main.mod);
            if (instance._notUnlockedBros == null)
            {
                instance._notUnlockedBros = new List<List<string>> { new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>() };
            }
            if (instance._availableBros == null)
            {
                instance._availableBros = new List<List<string>> { new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>() };
            }
            instance.Save(Main.mod);
        }
    }
}

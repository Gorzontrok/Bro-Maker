using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace BroMakerLib
{
    public class Settings
    {
        public static string FilePath
        {
            get
            {
                return Path.Combine(directory, nameof(Settings) + ".json");
            }
        }
        [JsonIgnore]
        public static Settings instance;
        [JsonIgnore]
        public static string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

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

        public Dictionary<string, bool> EnabledBros = new Dictionary<string, bool>();
        public int enabledBroCount = 0;
        public List<string> seenBros;
        // These lists are for tracking bro unlocks in IronBro
        public List<List<string>> _notUnlockedBros;
        public List<List<string>> _availableBros;

        // Settings menu
        public bool showGeneralSettings = true;
        public bool showSpawnSettings = true;
        public bool showDeveloperSettings = false;

        public static void Load()
        {
            if (File.Exists(FilePath))
                instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath));
            if (instance == null)
                instance = new Settings();
            if (instance.seenBros == null)
            {
                instance.seenBros = new List<string>();
            }
            if (instance._notUnlockedBros == null)
            {
                instance._notUnlockedBros = new List<List<string>> { new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>() };
            }
            if (instance._availableBros == null)
            {
                instance._availableBros = new List<List<string>> { new List<string>(), new List<string>(), new List<string>(), new List<string>(), new List<string>() };
            }
            instance.Save();
        }

        public void Save()
        {
            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            File.WriteAllText(FilePath, json);
        }
    }
}

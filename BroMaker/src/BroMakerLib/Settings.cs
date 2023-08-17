using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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

        public bool automaticSpawn = false;
        public float automaticSpawnProbabilty = 25;
        public bool maxHealthAtOne = true;

        public static void Load()
        {
            if (File.Exists(FilePath))
                instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath));
            if (instance == null)
                instance = new Settings();
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

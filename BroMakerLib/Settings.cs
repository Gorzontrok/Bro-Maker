using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BroMakerLib.Storages;

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

        public bool equalSpawnProbability = true;
        public bool automaticSpawn = true;
        public float automaticSpawnProbabilty = 25;
        public bool maxHealthAtOne = true;

        public Dictionary<string, bool> enabledBros = new Dictionary<string, bool>();
        public int enabledBroCount = 0;

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

        public float calculateSpawnProbability()
        {
            int enabledBroCount = 0;
            foreach (KeyValuePair<string, bool> bro in this.enabledBros)
            {
                if (bro.Value)
                {
                    ++enabledBroCount;
                }
            }
            // 41 Characters in the normal game
            return (enabledBroCount / (41.0f + enabledBroCount)) * 100.0f;
        }

        public void addBroEnabled(string name, bool enabled)
        {
            if ( !enabledBros.ContainsKey(name) )
            {
                enabledBros.Add(name, enabled);
                if (enabled)
                {
                    ++enabledBroCount;
                }
            } 
        }

        public void setBroEnabled(string name, bool enabled)
        {
            enabledBros[name] = enabled;
            if ( enabled )
            {
                ++enabledBroCount;
            }
            else
            {
                --enabledBroCount;
            }
        }

        public bool getBroEnabled(string name)
        {
            return enabledBros[name];
        }

        public void countEnabledBros()
        {
            enabledBroCount = 0;
            foreach (KeyValuePair<string, bool> bro in this.enabledBros)
            {
                if (bro.Value)
                {
                    ++enabledBroCount;
                }
            }
        }

        public StoredCharacter getRandomEnabledBro()
        {
            int chosen = UnityEngine.Random.Range(0, enabledBroCount);
            string chosenName = "";
            foreach (KeyValuePair<string, bool> bro in this.enabledBros)
            {
                if (bro.Value)
                {
                    if ( chosen == 0 )
                    {
                        chosenName = bro.Key;
                        break;
                    }
                    --chosen;
                }
            }

            for ( int i = 0; i < MakerObjectStorage.Bros.Length; ++i )
            {
                if ( MakerObjectStorage.Bros[i].name == chosenName )
                {
                    return MakerObjectStorage.Bros[i];
                }
            }

            return MakerObjectStorage.Bros[0];
        }
    }
}

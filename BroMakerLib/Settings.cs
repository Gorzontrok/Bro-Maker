using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using BroMakerLib.Storages;
using System.Collections;

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

        public Dictionary<string, bool> enabledBros = new Dictionary<string, bool>();
        public int enabledBroCount = 0;
        public List<string> seenBros;
        // These lists are for tracking bro unlocks in IronBro
        public List<List<string>> _notUnlockedBros;
        public List<List<string>> _availableBros;

        [JsonIgnore]
        public List<string> notUnlockedBros
        {
            get => _notUnlockedBros[PlayerProgress.currentWorldMapSaveSlot];
            set => _notUnlockedBros[PlayerProgress.currentWorldMapSaveSlot] = value;
        }

        [JsonIgnore]
        public List<string> availableBros
        {
            get => _availableBros[PlayerProgress.currentWorldMapSaveSlot];
            set => _availableBros[PlayerProgress.currentWorldMapSaveSlot] = value;
        }

        public static void Load()
        {
            if (File.Exists(FilePath))
                instance = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(FilePath));
            if (instance == null)
                instance = new Settings();
            if ( instance.seenBros == null )
            {
                instance.seenBros = new List<string>();
            }
            if ( instance._notUnlockedBros == null )
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

        public void checkForDeletedBros()
        {
            List<string> toBeRemoved = new List<string>();
            foreach (KeyValuePair<string, bool> bro in this.enabledBros)
            {
                bool found = false;
                for (int i = 0; i < MakerObjectStorage.Bros.Length; ++i)
                {
                    if (MakerObjectStorage.Bros[i].name == bro.Key )
                    {
                        found = true;
                        break;
                    }
                }
                if ( !found )
                {
                    toBeRemoved.Add(bro.Key);
                }
            }
            foreach ( string remove in toBeRemoved )
            {
                this.enabledBros.Remove(remove);
            }
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

        public StoredCharacter getStoredCharacter( string name )
        {
            for (int i = 0; i < MakerObjectStorage.Bros.Length; ++i)
            {
                if (MakerObjectStorage.Bros[i].name == name)
                {
                    return MakerObjectStorage.Bros[i];
                }
            }

            return MakerObjectStorage.Bros[0];
        }

        public StoredCharacter getRandomEnabledBro()
        {
            if ( this.overrideNextBroSpawn )
            {
                this.overrideNextBroSpawn = false;
                return getStoredCharacter(this.nextBroSpawn);
            }

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

            return getStoredCharacter(chosenName);
        }

        public StoredCharacter getRandomHardcoreBro(bool isRescue)
        {
            if (this.overrideNextBroSpawn)
            {
                this.overrideNextBroSpawn = false;
                return getStoredCharacter(this.nextBroSpawn);
            }

            if ( isRescue && this.notUnlockedBros.Count() > 0 )
            {
                int chosen = UnityEngine.Random.Range(0, this.notUnlockedBros.Count());
                string chosenName = this.notUnlockedBros[chosen];
                this.availableBros.Add(this.notUnlockedBros[chosen]);
                this.notUnlockedBros.RemoveAt(chosen);

                return getStoredCharacter(chosenName);
            }
            else
            {
                int chosen = UnityEngine.Random.Range(0, this.availableBros.Count());
                string chosenName = this.availableBros[chosen];

                return getStoredCharacter(chosenName);
            }
        }
    }
}

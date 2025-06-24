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
        public bool disableCustomAvatarFlash = true;
        public bool developerMode = false;
        public bool scaleUIWithWindowWidth = false;
        public bool disableTooltips = false;
        public bool scaleUIHeight = false;

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

        public float CalculateSpawnProbability()
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

        public void AddBroEnabled(string name, bool enabled)
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

        public void SetBroEnabled(string name, bool enabled)
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
            // Update spawn probability to account for additional enabled / disabled bro
            if ( this.equalSpawnProbability )
            {
                this.automaticSpawnProbabilty = this.CalculateSpawnProbability();
            }
        }

        public bool GetBroEnabled(string name)
        {
            return enabledBros[name];
        }

        public void CheckForDeletedBros()
        {
            List<string> toBeRemoved = new List<string>();
            foreach (KeyValuePair<string, bool> bro in this.enabledBros)
            {
                bool found = false;
                for (int i = 0; i < BroMakerStorage.Bros.Length; ++i)
                {
                    if (BroMakerStorage.Bros[i].name == bro.Key )
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

        public void CountEnabledBros()
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

        public StoredHero GetRandomEnabledBro()
        {
            if ( this.overrideNextBroSpawn )
            {
                this.overrideNextBroSpawn = false;
                return BroMakerStorage.GetHeroByName(this.nextBroSpawn);
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

            return BroMakerStorage.GetHeroByName( chosenName);
        }

        public StoredHero GetRandomHardcoreBro(bool isRescue)
        {
            if (this.overrideNextBroSpawn)
            {
                this.overrideNextBroSpawn = false;
                return BroMakerStorage.GetHeroByName( this.nextBroSpawn);
            }

            if ( isRescue && this.notUnlockedBros.Count() > 0 )
            {
                int chosen = UnityEngine.Random.Range(0, this.notUnlockedBros.Count());
                string chosenName = this.notUnlockedBros[chosen];
                this.availableBros.Add(this.notUnlockedBros[chosen]);
                this.notUnlockedBros.RemoveAt(chosen);

                return BroMakerStorage.GetHeroByName( chosenName);
            }
            else
            {
                int chosen = UnityEngine.Random.Range(0, this.availableBros.Count());
                string chosenName = this.availableBros[chosen];

                return BroMakerStorage.GetHeroByName( chosenName);
            }
        }
    }
}

using System;
using System.IO;
using BroMakerLib.Cutscenes;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;

namespace BroMakerLib.Storages
{
    public class StoredHero : IStoredObject
    {
        public const string JSON_EXTENSION = ".json";

        public string path { get; set; }
        public string name { get; set; }

        public CustomBroInfo info;
        public BroMakerMod mod;

        public StoredHero(string path, BroMakerMod mod)
        {
            this.path = path;
            info = null;
            this.mod = mod;
            name = "";
            try
            {
                info = GetInfo();
                name = info.name;
            }
            catch (Exception e)
            {
                string fallbackName = Path.GetFileNameWithoutExtension(this.path);
                BMLogger.Error($"Failed to load bro JSON at '{this.path}'", e, mod?.Name, fallbackName);
                name = fallbackName;
            }
            BroSpawnManager.AddBroIfEnabled(this);
        }

        public StoredHero(CustomBroInfo bInfo, BroMakerMod mod)
        {
            info = bInfo;
            path = bInfo.path;
            name = bInfo.name;
            BroSpawnManager.AddBroIfEnabled(this);
            this.mod = mod;
        }

        public CustomBroInfo GetInfo()
        {
            if (info != null)
            {
                return info;
            }

            BMLogger.Debug($"Start Deserialization of '{path}'");
            string extension = Path.GetExtension(path).ToLower();
            if (extension == JSON_EXTENSION)
            {
                using (BMLogger.Scope(mod?.Name, string.IsNullOrEmpty(name) ? Path.GetFileNameWithoutExtension(path) : name))
                {
                    info = CustomBroInfo.DeserializeJSON<CustomBroInfo>(path);
                }
                info.path = Path.GetDirectoryName(path);
                info.Mod = mod;
                foreach (CustomIntroCutscene cutscene in info.Cutscene)
                {
                    cutscene.path = info.path;
                    cutscene.Owner = info;
                }
            }
            BMLogger.Debug("End Deserialization");
            return info;
        }

        public TestVanDammeAnim LoadBro(int playerNum)
        {
            try
            {
                BMLogger.Debug("Spawning Hero " + name);
                CustomBroInfo info = GetInfo();
                return Loaders.LoadHero.WithCustomBroInfo(playerNum, info, PresetManager.GetHeroPreset(info.CharacterPreset));
            }
            catch (Exception e)
            {
                BMLogger.Error($"Failed to load bro '{name}'", e, mod?.Name, name);
            }
            return null;
        }

        public override string ToString()
        {
            return name;
        }
    }
}

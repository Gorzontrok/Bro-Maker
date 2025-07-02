using System;
using System.IO;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.CustomObjects.Bros;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib.Storages
{
    public struct StoredHero : IStoredObject
    {
        public const string JSON_EXTENSION = ".json";

        public bool IsEmpty
        {
            get
            {
                return path.IsNullOrEmpty();
            }
        }

        public string path { get; set; }
        public string name { get; set; }

        public CustomBroInfo info;
        public BroMakerMod mod;

        public StoredHero(string path, BroMakerMod mod)
        {
            this.path = path;
            this.info = null;
            this.mod = mod;
            this.name = "";
            try
            {
                this.info = GetInfo();
                this.name = this.info.name;
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
                this.name = Path.GetFileNameWithoutExtension(this.path);
            }
            BSett.instance.AddBroEnabled(this.name, true);
        }
        public StoredHero(CustomBroInfo bInfo, BroMakerMod mod)
        {
            info = bInfo;
            path = bInfo.path;
            name = bInfo.name;
            BSett.instance.AddBroEnabled(this.name, true);
            this.mod = mod;
        }

        public CustomBroInfo GetInfo()
        {
            if (this.info != null)
                return this.info;

            CustomBroInfo info = null;

            BMLogger.Debug($"Start Deserialization of '{path}'");
            string extension = Path.GetExtension(path).ToLower();
            if (extension == JSON_EXTENSION)
            {
                info = CustomBroInfo.DeserializeJSON<CustomBroInfo>(path);
                info.path = Path.GetDirectoryName(path);
                foreach (var cutscene in info.Cutscene)
                {
                    cutscene.path = info.path;
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
                var info = GetInfo();
                return Loaders.LoadHero.WithCustomBroInfo(playerNum, info, PresetManager.GetHeroPreset(info.characterPreset));
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
            return null;
        }

        public override string ToString()
        {
            return name;
        }
    }
}

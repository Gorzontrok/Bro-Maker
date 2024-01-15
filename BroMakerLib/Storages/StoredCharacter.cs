using System;
using System.IO;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.CustomObjects.Bros;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib.Storages
{
    public struct StoredCharacter : IStoredObject
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

        public Type type;

        public StoredCharacter(string path, Type type = null)
        {
            this.path = path;
            name = Path.GetFileNameWithoutExtension(this.path);
            this.type = type ?? typeof(CustomHero);
            BSett.instance.addBroEnabled(this.name, true);
        }

        public TInfo GetInfo<TInfo>() where TInfo : CustomCharacterInfo
        {
            TInfo info = null;

            BMLogger.Debug($"Start Deserialization of '{path}'");
            string extension = Path.GetExtension(path).ToLower();
            if (extension == JSON_EXTENSION)
            {
                info = CustomBroInfo.DeserializeJSON<TInfo>(path);
                info.path = Path.GetDirectoryName(path);
                info.cutscene.path = info.path;
            }
            BMLogger.Debug("End Deserialization");
            return info;
        }

        public void LoadCharacter(int playernum)
        {
            LoadCharacter<CustomCharacterInfo>(playernum);
        }
        public void LoadCharacter<TInfo>(int playerNum) where TInfo : CustomCharacterInfo
        {
            try
            {
                BMLogger.Log("Spawning Character " + name);
                TInfo info = GetInfo<TInfo>();
                //Loaders.LoadCharacter.WithCustomBroInfo(playerNum, info, PresetManager.GetPreset(info.characterPreset));
            }
            catch(Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }

        public void LoadBro(int playerNum)
        {
            try
            {
                BMLogger.Debug("Spawning Hero " + name);
                var info = GetInfo<CustomBroInfo>();
                Loaders.LoadHero.WithCustomBroInfo(playerNum, info, PresetManager.GetHeroPreset(info.characterPreset));
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }

        public override string ToString()
        {
            return name;
        }
    }
}

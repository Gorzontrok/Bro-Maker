﻿using System;
using System.IO;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.CustomObjects.Bros;

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
                BMLogger.Log("Spawning Hero " + name);
                var info = GetInfo<CustomBroInfo>();
                Loaders.LoadHero.WithCustomBroInfo(playerNum, info, PresetManager.GetPreset(info.characterPreset));
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
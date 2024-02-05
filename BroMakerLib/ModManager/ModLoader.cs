using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using System;
using System.Collections.Generic;
using System.IO;
using UnityModManagerNet;

namespace BroMakerLib.ModManager
{
    internal static class ModLoader
    {
        public static List<BroMakerMod> mods =  new List<BroMakerMod>();
        public static List<BroMakerMod> incompatibleMods = new List<BroMakerMod>();

        public static void Initialize()
        {
            mods = new List<BroMakerMod>();
            LoadMods();
            LoadModsContents();
        }

        private static void LoadMods()
        {
            BMLogger.Debug("Loading Mods");
            string[] jsonFiles = Directory.GetFiles(DirectoriesManager.StorageDirectory, "*.mod.json", SearchOption.AllDirectories);

            foreach (string jsonFile in jsonFiles)
            {
                BroMakerMod mod = null;
                try
                {
                    mod = BroMakerMod.TryLoad(jsonFile);
                    if (mod != null)
                    {
                        if (Info.ParsedVersion >= UnityModManager.ParseVersion(mod.BroMakerVersion))
                        {
                            mod.Initialize();
                            mods.Add(mod);
                        }
                        else
                        {
                            incompatibleMods.Add(mod);
                            BMLogger.Error($"{mod.Name} require version of BroMaker >={mod.BroMakerVersion}. Current BroMaker version is {Info.VERSION}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog($"Unable to load BroMaker Mod at {jsonFile}.", ex);
                }
            }
            BMLogger.Debug("Finish Loading Mods");
        }

        private static void LoadModsContents()
        {
            BMLogger.Debug("Loading Mods Content");
            foreach (BroMakerMod mod in mods)
            {
                MakerObjectStorage.StoreCharactersFromMod(mod);
            }
            BMLogger.Debug("Finish Loading Mods Content");
        }
    }
}

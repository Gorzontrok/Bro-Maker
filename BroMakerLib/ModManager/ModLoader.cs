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

        public static void Initialize()
        {
            mods = new List<BroMakerMod>();
            LoadMods();

            // Error. See 'BroMakerMod::CheckModUpdate_DownloadStringCompleted'
            // CheckModsUpdate();

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
                            BMLogger.Error($"{mod.Id} require version of BroMaker >={mod.BroMakerVersion}. Current BroMaker version is {Info.VERSION}");
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

        private static void CheckModsUpdate()
        {
            BMLogger.Debug("Checking Mods Update");
            foreach(BroMakerMod mod in mods)
            {
                mod.CheckModUpdate();
            }
            BMLogger.Debug("Finish Checking Mods Update");
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

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
                        Version modVersion = UnityModManager.ParseVersion(mod.BroMakerVersion);
                        // Mod requires a newer version of BroMaker
                        if ( Info.ParsedVersion < modVersion )
                        {
                            incompatibleMods.Add(mod);
                            BMLogger.Error($"{mod.Name} require version of BroMaker >={mod.BroMakerVersion}. Current BroMaker version is {Info.VERSION}");
                            mod.ErrorMessage = "This mod requires a newer version of BroMaker: " + mod.BroMakerVersion;
                        }
                        // Mod is too old for this version of BroMaker
                        else if (Info.ParsedMinimumVersion > modVersion)
                        {
                            incompatibleMods.Add(mod);
                            BMLogger.Error($"{mod.Name} will not work with this version of BroMaker. You must update {mod.Name} or downgrade BroMaker.");
                            mod.ErrorMessage = "This mod uses an outdated version of BroMaker (" + mod.BroMakerVersion + ") you must update this mod to a version that supports BroMaker " + Info.ParsedMinimumVersion + " or downgrade BroMaker.";
                        }
                        // Versions are compatible
                        else
                        {
                            mod.Initialize();
                            mods.Add(mod);
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

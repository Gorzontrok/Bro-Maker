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
                        string modName = mod.Name != string.Empty ? mod.Name : "This bro";
                        string modNameLowerCase = mod.Name != string.Empty ? mod.Name : "this bro";
                        // Mod requires a newer version of BroMaker
                        if ( Info.ParsedVersion < modVersion )
                        {
                            incompatibleMods.Add(mod);
                            BMLogger.Error($"{modName} requires a version of BroMaker >={mod.BroMakerVersion}. Current BroMaker version is {Info.VERSION}");
                            mod.ErrorMessage = modName + " requires a newer version of BroMaker: " + mod.BroMakerVersion + ". Current BroMaker version is: " + Info.VERSION + ". You must update BroMaker.";
                        }
                        // Mod is too old for this version of BroMaker
                        else if (Info.ParsedMinimumVersion > modVersion)
                        {
                            incompatibleMods.Add(mod);
                            BMLogger.Error($"{modName} will not work with this version of BroMaker. You must update {modName}.");
                            mod.ErrorMessage = modName + " uses an outdated version of BroMaker (" + mod.BroMakerVersion + ") you must update " + modNameLowerCase + " to a version that supports BroMaker " + Info.ParsedMinimumVersion + ".";
                        }
                        // Versions are compatible
                        else
                        {
                            // Let user know the bro may have compatibility issues with this version
                            if ( Info.ParsedSuggestedMinimumVersion > modVersion )
                            {
                                mod.ErrorMessage = modName + " uses an outdated version of BroMaker (" + mod.BroMakerVersion + ") you may experience bugs using it on this version of BroMaker. It's recommended that you update " + modNameLowerCase +  " to a newer version if possible.";
                            }
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

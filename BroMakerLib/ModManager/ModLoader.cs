using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BroMakerLib.ModManager
{
    internal static class ModLoader
    {
        public static List<BroMakerMod> mods =  new List<BroMakerMod>();

        public static void Initialize()
        {
            LoadMods();
            CheckUpdates();
        }

        private static void LoadMods()
        {
            string[] jsonFiles = Directory.GetFiles(DirectoriesManager.StorageDirectory, "*.mod.json", SearchOption.AllDirectories);

            foreach (string jsonFile in jsonFiles)
            {
                BroMakerMod mod = null;
                try
                {
                    mod = BroMakerMod.TryLoad(jsonFile);
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog($"Unable to load BroMaker Mod at {jsonFile}.", ex);
                }
                if (mod != null)
                {
                    mods.Add(mod);

                }
            }
        }

        public static void CheckUpdates()
        {
            foreach(BroMakerMod mod in mods)
            {
                mod.CheckModUpdate();
            }
        }
    }
}

using System;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Loggers;
using BroMakerLib.Unlocks;
using UnityModManagerNet;

namespace BroMakerLib
{
    static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool enabled;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            // Initialize mod
            mod = modEntry;

            modEntry.OnGUI = OnGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;
            modEntry.Info.DisplayName = "<color=\"#d68c16\">BroMaker Unity</color>";

            try
            {
                // Initialize BroMaker
                BroMaker.Initialize();
            }
            catch (Exception ex)
            {
                Main.Log("Error while loading BroMaker.\n" + ex);
            }

            // Log missed messages
            for (int i = 0; i < BMLogger.logs.Count; ++i)
            {
                Main.Log(BMLogger.logs[i], BroMakerLib.Loggers.Log.PREFIX);
            }

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                ModUI.UI();
            }
            catch (Exception e)
            {
                Log("UI\n" + e);
            }
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            BroUnlockManager.OnModUnload();
            return true;
        }

        public static void Log(object str)
        {
            mod.Logger.Log(str.ToString());
        }
        public static void Log(object str, string prefix)
        {
            UnityModManager.Logger.Log(str.ToString(), prefix);
        }

        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            SaveAll();
        }

        public static void SaveAll()
        {
            CustomHero.SaveAll();
            BroMakerLib.Settings.instance.Save(mod);
            BroUnlockManager.SaveProgressData();
        }
    }
}


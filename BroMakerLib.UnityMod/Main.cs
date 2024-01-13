using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityModManagerNet;

namespace BroMakerLib.UnityMod
{
    static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool enabled;
        public static Settings settings;
        public static int selectedPlayerNum = 0;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            // Initialize mod
            mod = modEntry;

            modEntry.OnGUI = OnGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnSaveGUI = OnSaveGUI;
            settings = Settings.Load<Settings>(modEntry);

            // Initialize Harmony Instance
            try
            {
                var harmony = new Harmony(modEntry.Info.Id);
                var assembly = Assembly.GetExecutingAssembly();
                harmony.PatchAll(assembly);
            }
            catch (Exception ex)
            {
                Main.Log(ex);
            }
            try
            {
                // Set BroMaker Path
                DirectoriesManager.StorageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Storage\\");
                // Initialize BroMaker
                BroMaker.Initialize();
            }
            catch(Exception ex)
            {
                Main.Log("Error while loading BroMaker.\n" + ex);
            }

            try
            {
                ModUI.Initialize();
            }
            catch ( Exception ex)
            {
                Main.Log("Error while intializing the GUI.\n" + ex);
            }

            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            try
            {
                ModUI.UI();
            }
            catch(Exception e)
            {
                Log("UI\n" + e);
            }
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
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
            BroMakerLib.Settings.instance.Save();
            settings.Save(modEntry);
        }
    }

    public class Settings : UnityModManager.ModSettings
    {
        public bool debugLogs = false;

        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}


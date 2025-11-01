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
                // Initialize Unlock Manager
                BroUnlockManager.Initialize();
                // Preload all bro assets
                BroMaker.PreloadBroAssets();
            }
            catch (Exception ex)
            {
                Main.Log("Error while loading BroMaker.\n" + ex);
            }

            try
            {
                ModUI.Initialize();
            }
            catch (Exception ex)
            {
                Main.Log("Error while intializing the GUI.\n" + ex);
            }

            // Register Custom Bros menu with MainMenu
            try
            {
                RocketLib.Menus.Core.MenuRegistry.RegisterAction(
                    displayText: "CUSTOM BROS",
                    onSelect: (menu) => BroMakerLib.Menus.CustomBrosGridMenu.Show(menu),
                    targetMenu: RocketLib.Menus.Core.TargetMenu.MainMenu,
                    position: RocketLib.Menus.Core.PositionMode.After,
                    positionReference: "START"
                );
            }
            catch (Exception ex)
            {
                Main.Log("Error while registering Custom Bros menu.\n" + ex);
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
            BroMakerLib.Settings.instance.Save();
            BroUnlockManager.SaveProgressData();
        }
    }
}


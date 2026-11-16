using System;
using System.Threading;
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
        internal static int mainThreadId;

        static bool Load(UnityModManager.ModEntry modEntry)
        {
            // Initialize mod
            mod = modEntry;
            mainThreadId = Thread.CurrentThread.ManagedThreadId;

            modEntry.OnGUI = OnGUI;
            modEntry.OnUpdate = OnUpdate;
            modEntry.OnToggle = OnToggle;
            modEntry.OnSaveGUI = OnSaveGUI;
            modEntry.OnUnload = OnUnload;
            modEntry.OnShowGUI = _ => ModUI.OnShowGUI();
            modEntry.OnHideGUI = _ => ModUI.OnHideGUI();
            modEntry.Info.DisplayName = "<color=\"#d68c16\">BroMaker Unity</color>";

            try
            {
                // Initialize BroMaker
                BroMaker.Initialize();
            }
            catch (Exception ex)
            {
                BMLogger.Error("BroMaker failed to load", ex);
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
                BMLogger.Error("BroMaker's settings window failed to draw", e);
            }
        }

        private static void OnUpdate(UnityModManager.ModEntry modEntry, float deltaTime)
        {
            BMLogger.Flush();
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            if (!value)
            {
                // UMM skips OnHideGUI for an inactive mod, so a filter set by ShowIssue would stay.
                ModUI.OnHideGUI();
            }
            return true;
        }

        static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            SaveAll();
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
            BroUnlockManager.SaveProgressData(synchronous: true);
        }
    }
}


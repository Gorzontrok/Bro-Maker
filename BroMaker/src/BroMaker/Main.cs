using System;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using BroMakerLib;

namespace BroMakerLoadMod
{
    static class Main
    {
        public static UnityModManager.ModEntry mod;
        public static bool enabled;
        public static Settings settings;

        // internal static CreateBroFromJSON createFromJSON = new CreateBroFromJSON();

        internal static int CustomBroSelected = -1;

        internal static bool Switch = false;

        internal static string cantSwapMessage;
        private static GUIStyle cantSwapMsgStyle = new GUIStyle();

        private static int playerNumSelected = 1;
        static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnGUI = OnGUI;
            modEntry.OnToggle = OnToggle;
            modEntry.OnSaveGUI = OnSaveGUI;
            settings = Settings.Load<Settings>(modEntry);

            mod = modEntry;

            var harmony = new Harmony(modEntry.Info.Id);
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                harmony.PatchAll(assembly);
            }
            catch (Exception ex)
            {
                Main.Log(ex);
            }
            cantSwapMsgStyle.normal.textColor = Color.yellow;
            cantSwapMsgStyle.alignment = TextAnchor.MiddleCenter;
            //createFromJSON.Load();
            return true;
        }

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            settings.DebugMode = GUILayout.Toggle(settings.DebugMode, "Debug Log");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Player Num: " + playerNumSelected);
            playerNumSelected = (int)GUILayout.HorizontalSlider(playerNumSelected, 1, 4, GUILayout.Width(200));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.Space(15);
            if(NewBroInfo.Names.Length != 0)
            {
                GUILayout.Label(cantSwapMessage, cantSwapMsgStyle);
                GUILayout.Label("Swap to :");
                GUILayout.BeginHorizontal();
                if (CustomBroSelected != (CustomBroSelected = GUILayout.SelectionGrid(CustomBroSelected, NewBroInfo.Names, 5, GUILayout.Height(20 * NewBroInfo.Names.Length % 5))))
                {
                    //if (NewBroInfo_Controller.newBroInfos[CustomBroSelected])
                    NewBroInfo.newBroInfos[CustomBroSelected].Spawn(playerNumSelected - 1);
                    CustomBroSelected = -1;
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("No custom bro mod install");
            }
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            enabled = value;
            return true;
        }

        internal static void Log(object str)
        {
            mod.Logger.Log(str.ToString());
        }
        internal static void ExceptionLog(object str, Exception ex)
        {
            Log(str + "\n" + ex);
        }
        internal static void Debug(object str)
        {
            if (!settings.DebugMode) return;

            mod.Logger.Log("[DEBUG] " + str.ToString());
        }
        static void OnSaveGUI(UnityModManager.ModEntry modEntry)
        {
            settings.Save(modEntry);
        }
    }
    /// <summary>
    ///
    /// </summary>
    public class Settings : UnityModManager.ModSettings
    {
        /// <summary>
        ///
        /// </summary>
        public bool DebugMode;
        /// <summary>
        ///
        /// </summary>
        /// <param name="modEntry"></param>
        public override void Save(UnityModManager.ModEntry modEntry)
        {
            Save(this, modEntry);
        }
    }
}


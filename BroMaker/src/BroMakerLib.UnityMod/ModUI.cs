using System;
using System.Collections.Generic;
using System.Linq;
using BroMakerLib.Editor;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using UnityEngine;
using RocketLib;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib.UnityMod
{
    public static class ModUI
    {
        private static Dictionary<string, Action> _tabs = new Dictionary<string, Action>()
        {
            { "Main", Spawner },
            { "Create New Object", CreateFileEditor.UnityUI },
            { "Edit file", EditCurrentFile},
            { "Settings", Settings }
        };
        private static Settings _Settings
        {
            get { return Main.settings; }
        }

        private static object _objectToEdit = null;
        private static GUIStyle _toolTipStyle = new GUIStyle();
        private static int _tabSelected = 0;
        private static GUIStyle _errorSwapingMessageStyle = new GUIStyle();
        private static Rect _toolTipRect = Rect.zero;
        private static int _selectedCustomBros = -1;
        private static string[] _brosNames;
        private static StoredCharacter _selectedBro;

        public static void Initialize()
        {
            // Create style
            _errorSwapingMessageStyle.normal.textColor = Color.yellow;
            _errorSwapingMessageStyle.alignment = TextAnchor.MiddleCenter;

            _toolTipStyle = new GUIStyle
            {
                fontStyle = FontStyle.Bold,
                fontSize = 15,
            };
            _toolTipStyle.normal.textColor = Color.white;

            _brosNames = MakerObjectStorage.Bros.Select((sc) => sc.ToString()).ToArray();

            BSett.instance.countEnabledBros();

            if ( BSett.instance.equalSpawnProbability )
            {
                BSett.instance.automaticSpawnProbabilty = BSett.instance.calculateSpawnProbability();
            }
        }

        public static void UI()
        {
            string[] tabsNames = _tabs.Keys.ToArray();
            _tabSelected = GUILayout.SelectionGrid(_tabSelected, tabsNames, 4);
            GUILayout.Space(15);
            _toolTipRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(10);


            GUILayout.BeginVertical("box");
            _tabs.TryGetValue(tabsNames[_tabSelected], out Action action);
            action.Invoke();
            GUILayout.EndVertical();
            GUI.Label(_toolTipRect, GUI.tooltip, _toolTipStyle);
        }

        public static void Spawner()
        {
            GUILayout.Label(BMLogger.errorSwapingMessage, _errorSwapingMessageStyle);
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reload Files", GUILayout.ExpandWidth(false)))
                ReloadFiles();
            if (GUILayout.Button("Reload Preset", GUILayout.ExpandWidth(false)))
                PresetManager.Initialize();
            GUILayout.EndHorizontal();

            StoredCharacter[] bros = MakerObjectStorage.Bros;
            if (bros.Length != 0)
            {
                if (_selectedCustomBros >= 0 && _selectedCustomBros < bros.Length)
                {

                    SelectedBroUI(_selectedBro);
                }

                GUILayout.Label("Select bros from JSON :");
                GUILayout.BeginHorizontal();
                if (_selectedCustomBros != (_selectedCustomBros = GUILayout.SelectionGrid(_selectedCustomBros, _brosNames, 5, GUILayout.Height(20 * bros.Length % 5))))
                {
                    _selectedBro = bros[_selectedCustomBros];
                    _objectToEdit = bros[_selectedCustomBros].GetInfo<CustomBroInfo>();
                    FieldsEditor.editHasError = false;
                }

                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("No Bros File Found");
            }
        }

        private static void SelectedBroUI(StoredCharacter bro)
        {
            if (bro.Equals(null)) return;

            Main.selectedPlayerNum = RGUI.HorizontalSliderInt("Player Num: ", Main.selectedPlayerNum, 0, 3, 200);

            GUILayout.BeginVertical("box");
            //_objectToEdit = bro.GetInfo<CustomBroInfo>();
            FileEditor.makerObjectType = MakerObjectType.Bros;

            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

            if (GUILayout.Button(new GUIContent("Load Bro")))
                bro.LoadBro(Main.selectedPlayerNum);
            bool broEnabled = BSett.instance.getBroEnabled(bro.name);
            if (GUILayout.Button( (broEnabled ? "Autospawn Enabled" : "Autospawn Disabled") ))
            {
                BSett.instance.setBroEnabled(bro.name, !broEnabled);
                if ( BSett.instance.equalSpawnProbability )
                {
                    BSett.instance.automaticSpawnProbabilty = BSett.instance.calculateSpawnProbability();
                }
            }
            if (GUILayout.Button("Edit File"))
            {
                _tabSelected = 2;
            }
            if (GUILayout.Button("Duplicate File"))
                CreateFileEditor.DuplicateFile(bro.path);
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        public static void EditCurrentFile()
        {
            GUILayout.Label("Under Construction");
            return;

            try
            {
                if (_objectToEdit == null)
                    GUILayout.Label("Select an object first", _errorSwapingMessageStyle);
                else if (FieldsEditor.editHasError)
                    GUILayout.Label("Edit has error", _errorSwapingMessageStyle);
                else
                    FileEditor.UnityUI(_objectToEdit);
            }
            catch(Exception e)
            {
                FieldsEditor.editHasError = true;
                Main.Log(e.ToString());
            }
        }
        public static void Settings()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Check Directories", GUILayout.ExpandWidth(false)))
                DirectoriesManager.Initialize();
            if (GUILayout.Button("Show Presets", GUILayout.ExpandWidth(false)))
            {
                foreach(KeyValuePair<string, Type> kvp in PresetManager.heroesPreset)
                {
                    Main.Log($"{kvp.Key}\t{kvp.Value}");
                }
            }
            GUILayout.EndHorizontal ();

            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            BSett.instance.automaticSpawn = GUILayout.Toggle(BSett.instance.automaticSpawn, "Automatic Spawn");
            if ( BSett.instance.equalSpawnProbability != (BSett.instance.equalSpawnProbability = GUILayout.Toggle(BSett.instance.equalSpawnProbability, "Custom characters have an equal chance of spawning as normal characters")) )
            {
                if ( BSett.instance.equalSpawnProbability )
                {
                    BSett.instance.automaticSpawnProbabilty = BSett.instance.calculateSpawnProbability();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            //if ( BSett.instance.automaticSpawnProbabilty != (BSett.instance.automaticSpawnProbabilty = RGUI.HorizontalSlider("Spawn Probability: ", BSett.instance.automaticSpawnProbabilty, 0f, 100f)) )
            if (BSett.instance.automaticSpawnProbabilty != (BSett.instance.automaticSpawnProbabilty = RGUI.HorizontalSlider("Spawn Probability: ", BSett.instance.automaticSpawnProbabilty, 0f, 100f)))
            {
                BSett.instance.equalSpawnProbability = false;
            }
            GUILayout.Space(15);
            BSett.instance.maxHealthAtOne = GUILayout.Toggle(BSett.instance.maxHealthAtOne, "Max health always at 1");
            GUILayout.Space(15);
            _Settings.debugLogs = GUILayout.Toggle(_Settings.debugLogs, "Debug Logs");
        }

        private static void ReloadFiles()
        {
            BroMaker.ReloadFiles();
            _brosNames = MakerObjectStorage.Bros.Select((sc) => sc.ToString()).ToArray();
            _selectedCustomBros = -1;
        }
    }
}

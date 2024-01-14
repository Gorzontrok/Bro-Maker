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
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.ModManager;
using System.IO;
using static HarmonyLib.Code;

namespace BroMakerLib.UnityMod
{
    internal static class ModUI
    {
        public static List<BroMakerMod> Mods
        {
            get
            {
                return ModLoader.mods;
            }
        }

        private static Dictionary<string, Action> _tabs = new Dictionary<string, Action>()
        {
            { "Spawner", Spawner },
            { "Mods", ModsUI },
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
        private static int _selectedCustomBrosIndex = -1;
        private static StoredCharacter _selectedBro;
        private static CustomHero _selectedHero;
        private static GameObject heroHolder;
        private static bool heroCreated = false;

        private static Vector2 _spawnerScrollView;
        private static int _broPerLines = 6;

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

            BSett.instance.checkForDeletedBros();
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
            {
                ReloadFiles();
                PresetManager.disableWarnings = true;
                PresetManager.Initialize();
                PresetManager.disableWarnings = false;
                BSett.instance.checkForDeletedBros();
                BSett.instance.countEnabledBros();
                if (BSett.instance.equalSpawnProbability)
                {
                    BSett.instance.automaticSpawnProbabilty = BSett.instance.calculateSpawnProbability();
                }
            }
            if (GUILayout.Button("Reload Preset", GUILayout.ExpandWidth(false)))
                PresetManager.Initialize();
            GUILayout.EndHorizontal();

            // New UI
            GUILayout.BeginVertical();
            _spawnerScrollView = GUILayout.BeginScrollView(_spawnerScrollView, GUILayout.Height(400));
            if (Mods.Count <= 0)
            {
                GUILayout.Label("No mod intalled.");
                return;
            }

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Name", GUILayout.Width(200));
            GUILayout.Label("Author", GUILayout.Width(100));
            GUILayout.Label("Version", GUILayout.Width(50));
            GUILayout.Label("BroMaker Version", GUILayout.Width(100));
            GUILayout.EndHorizontal();
            GUILayout.Space(30);

            int broIndex = 0;
            try
            {
                foreach (BroMakerMod mod in Mods)
                {
                    var name = mod.Name;
                    if (name.IsNullOrEmpty())
                        name = mod.Id;

                    // show mod informations
                    GUILayout.BeginHorizontal("box");
                    GUILayout.Label(name, GUILayout.Width(200));
                    GUILayout.Label(mod.Author, GUILayout.Width(100));
                    GUILayout.Label(mod.Version, GUILayout.Width(50));
                    GUILayout.Label(mod.BroMakerVersion, GUILayout.Width(100));
                    GUILayout.EndHorizontal();

                    // Show bros
                    GUILayout.BeginVertical("box");
                    bool isHorizontalOpen = false;
                    int horizontalIndex = 0;
                    bool willShowOptions = false;
                    int modIndex = 0;
                    foreach (string bro in mod.CustomBros)
                    {
                        if (horizontalIndex == 0)
                        {
                            GUILayout.BeginHorizontal();
                            isHorizontalOpen = true;
                        }

                        if (GUILayout.Button(mod.BrosNames[modIndex], GUILayout.Width(100)))
                        {
                            _selectedCustomBrosIndex = broIndex;
                            _selectedBro = new StoredCharacter(Path.Combine(mod.Path, mod.CustomBros[modIndex]));
                        }

                        if (_selectedCustomBrosIndex == broIndex)
                        {
                            willShowOptions = true;
                        }

                        if (horizontalIndex == _broPerLines)
                        {
                            horizontalIndex = 0;
                            isHorizontalOpen = false;
                            GUILayout.EndHorizontal();

                            if (willShowOptions)
                            {
                                SelectedBroUI(_selectedBro);
                            }
                        }
                        broIndex++;
                        horizontalIndex++;
                        modIndex++;
                    }
                    // prevent any horizontal open and not closed inside the loop
                    if (isHorizontalOpen)
                    {
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();

                    GUILayout.Space(30);
                }

                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }

        private static void ModsUI()
        {
            GUILayout.Label("Mods :");
            if (GUILayout.Button("ReloadMods"))
                ModManager.ModLoader.Initialize();
            foreach (BroMakerMod mod in ModLoader.mods)
            {
                var name = mod.Name;
                if (name.IsNullOrEmpty())
                    name = mod.Id;

                GUILayout.BeginHorizontal();
                GUILayout.Label(name);
                GUILayout.Label(mod.Author);
                GUILayout.Label(mod.Version);
                if (mod.CanBeUpdated)
                {
                    if (GUILayout.Button("Update Mod"))
                    {
                        ModUpdater.Update(mod);
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.TextField((int)ModUpdater.DownloadingProgression + "%");
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndHorizontal();
            }
        }
        private static void CreateSelectedBro()
        {
            try
            {
                if (_selectedHero != null)
                {
                    _selectedHero.DestroyMe();
                }
                string preset = (_objectToEdit as CustomBroInfo).characterPreset;
                if (preset.IsNullOrEmpty())
                {
                    throw new NullReferenceException("'characterPreset' is null or empty");
                }
                if (!PresetManager.heroesPreset.ContainsKey(preset))
                {
                    throw new Exception($"'characterPreset': {preset} doesn't exist. Check if you have the preset installed or if there is a typo.");
                }
                if (heroHolder == null)
                {
                    heroHolder = new GameObject();
                    heroHolder.SetActive(false);
                }
                _selectedHero = heroHolder.AddComponent(PresetManager.heroesPreset[preset]) as CustomHero;
                _selectedHero.enabled = false;
                heroCreated = true;
            }
            catch (Exception ex)
            {
                BMLogger.Log(ex.ToString());
                heroCreated = false;
            }
        }


        private static void SelectedBroUI(StoredCharacter bro)
        {
            if (bro.Equals(null))
                return;

            Main.selectedPlayerNum = RGUI.HorizontalSliderInt("Player Num: ", Main.selectedPlayerNum, 0, 3, 200);

            GUILayout.BeginVertical("box");
            //_objectToEdit = bro.GetInfo<CustomBroInfo>();
            //FileEditor.makerObjectType = MakerObjectType.Bros;

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
            /*if (GUILayout.Button("Edit File"))
            {
                _tabSelected = 2;
            }*/
            if (GUILayout.Button("Duplicate File"))
            {
                CreateFileEditor.DuplicateFile(bro.path);
            }
            if (GUILayout.Button("Load Cutscene"))
            {
                Cutscenes.CustomCutsceneController.LoadHeroCutscene(bro.GetInfo<CustomCharacterInfo>().cutscene);
            }
            GUILayout.EndHorizontal();

            // Display any UI options the bro developer added
            if ( _selectedHero != null )
            {
                try
                {
                    _selectedHero.UIOptions();
                }
                catch (Exception ex)
                {
                }
            }
            // If hero was created before but is now null, the game object was probably deleted
            else if ( heroCreated )
            {
                CreateSelectedBro();
            }

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
            BSett.instance.onlyCustomInHardcore = GUILayout.Toggle(BSett.instance.onlyCustomInHardcore, "Only custom characters will spawn in IronBro mode");
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
            _selectedCustomBrosIndex = -1;
        }
    }
}

using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Editor;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using BroMakerLib.ModManager;
using BroMakerLib.Storages;
using RocketLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEngine;
using static HarmonyLib.Code;
using static UnityModManagerNet.UnityModManager;
using BSett = BroMakerLib.Settings;

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
        public static List<BroMakerMod> IncompatibleMods
        {
            get
            {
                return ModLoader.incompatibleMods;
            }
        }

        private static Dictionary<string, Action> _normalTabs = new Dictionary<string, Action>()
        {
            { "Custom Bros", Spawner },
            { "Settings", Settings }
        };
        private static Dictionary<string, Action> _developerModeTabs = new Dictionary<string, Action>()
        {
            { "Custom Bros", Spawner },
            { "Create New Object", CreateFileEditor.UnityUI },
            { "Settings", Settings }
        };
        private static Dictionary<string, Action> _tabs
        {
            get => BSett.instance.developerMode ? _developerModeTabs : _normalTabs;
        }
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
        private static int _selectedModIndex = -1;
        private static StoredHero _selectedBro;
        private static CustomHero _selectedHero;
        private static GameObject heroHolder;
        private static bool heroCreated = false;

        private static Vector2 _spawnerScrollView;
        private static int _broPerLines = 6;

        private static GUIStyle _buttonStyle = null;
        private static GUIStyle _warningStyle = null;
        private static GUIStyle _incompatibleStyle = null;
        private static GUIStyle _enabledStyle = null;
        private static GUIStyle _disabledStyle = null;

        // Settings Menu
        private static GUIStyle _headerStyle;
        private static bool _showSpawnSettings = true;
        private static bool _showDeveloperSettings = false;

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
            if (_buttonStyle == null)
                _buttonStyle = new GUIStyle(GUI.skin.button);

            if (_warningStyle == null)
            {
                _warningStyle = new GUIStyle(GUI.skin.label);
                _warningStyle.normal.textColor = Color.yellow;
            }

            if (_incompatibleStyle == null)
            {
                _incompatibleStyle = new GUIStyle(GUI.skin.label);
                _incompatibleStyle.normal.textColor = Color.red;
            }

            if ( _headerStyle == null )
            {
                _headerStyle = new GUIStyle( GUI.skin.button );
                _headerStyle.fontStyle = FontStyle.Bold;
                _headerStyle.normal.textColor = new Color( 0.639216f, 0.909804f, 1f );
            }

            if ( _enabledStyle == null )
            {
                _enabledStyle = new GUIStyle( GUI.skin.label );
                _enabledStyle.normal.textColor = Color.green;
            }

            if ( _disabledStyle == null )
            {
                _disabledStyle = new GUIStyle( GUI.skin.label );
                _disabledStyle.normal.textColor = Color.red;
            }

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
            if ( BSett.instance.developerMode )
            {
                GUILayout.BeginHorizontal();
                if ( GUILayout.Button( "Reload Mods", GUILayout.ExpandWidth( false ) ) )
                {
                    ModManager.ModLoader.Initialize();
                }
                if ( GUILayout.Button( "Reload Files", GUILayout.ExpandWidth( false ) ) )
                {
                    ReloadFiles();
                    PresetManager.disableWarnings = true;
                    PresetManager.Initialize();
                    PresetManager.disableWarnings = false;
                    BSett.instance.checkForDeletedBros();
                    BSett.instance.countEnabledBros();
                    if ( BSett.instance.equalSpawnProbability )
                    {
                        BSett.instance.automaticSpawnProbabilty = BSett.instance.calculateSpawnProbability();
                    }
                }
                if ( GUILayout.Button( "Reload Preset", GUILayout.ExpandWidth( false ) ) )
                    PresetManager.Initialize();
                GUILayout.EndHorizontal();
                GUILayout.Space( 15 );
            }

            // New UI
            GUILayout.BeginVertical();
            if (Mods.Count <= 0 && IncompatibleMods.Count <= 0)
            {
                GUILayout.Label("No mods installed.");
                GUILayout.EndVertical();
                return;
            }

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Name", GUILayout.Width(150));
            GUILayout.Space(50);
            GUILayout.Label("Author", GUILayout.Width(200));
            GUILayout.Label("Version", GUILayout.Width(200));
            GUILayout.Label("BroMaker Version", GUILayout.Width(200));
            GUILayout.Label( "Autospawn Enabled", GUILayout.Width( 200 ) );
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            if (Mods.Count > 8)
                _spawnerScrollView = GUILayout.BeginScrollView(_spawnerScrollView, GUILayout.Height(320));

            int broIndex = 0;
            int modCount = 0;
            try
            {
                foreach (BroMakerMod mod in Mods)
                {
                    if (mod.StoredHeroes.IsNullOrEmpty())
                    {
                        continue;
                    }

                    var name = mod.Name;
                    // If only one custom bro is in the mod, we can just display them where the mod name would be
                    if ( mod.StoredHeroes.Length == 1 )
                    {
                        SpawnerUIOneHero(mod, broIndex);

                        broIndex++;
                    }

                    // If more than one custom bros are in the mod, allow user to toggle to see them
                    else
                    {
                        // show mod information
                        GUILayout.BeginHorizontal("box");
                        if ((_selectedModIndex == modCount) != GUILayout.Toggle(_selectedModIndex == modCount, name, _buttonStyle, GUILayout.Width(150)) )
                        {
                            // Deselect
                            if ( _selectedModIndex == modCount )
                            {
                                _selectedModIndex = -1;
                            }
                            else
                            {
                                _selectedModIndex = modCount;
                            }
                        }
                        GUILayout.Space(50);
                        GUILayout.Label(mod.Author, GUILayout.Width(200));
                        GUILayout.Label(mod.Version, GUILayout.Width(200));
                        if ( mod.ErrorMessage == string.Empty )
                        {
                            GUILayout.Label(mod.BroMakerVersion, GUILayout.Width(200));
                        }
                        else
                        {
                            GUILayout.Label(mod.BroMakerVersion, _warningStyle, GUILayout.Width(200));
                        }
                        GUILayout.EndHorizontal();

                        // Only show custom bros of this mod if this mod is selected
                        if ( _selectedModIndex == modCount )
                        {
                            // Show bros
                            GUILayout.BeginVertical("box");
                            bool isHorizontalOpen = false;
                            int horizontalIndex = 0;
                            bool willShowOptions = false;
                            int modIndex = 0;
                            foreach (StoredHero hero in mod.StoredHeroes)
                            {
                                if (horizontalIndex == 0)
                                {
                                    GUILayout.BeginHorizontal();
                                    isHorizontalOpen = true;
                                }

                                if ( (_selectedCustomBrosIndex == broIndex) != GUILayout.Toggle(_selectedCustomBrosIndex == broIndex, mod.StoredHeroes[modIndex].name, _buttonStyle, GUILayout.Width(150)) )
                                {
                                    if (_selectedCustomBrosIndex == broIndex)
                                    {
                                        _selectedCustomBrosIndex = -1;
                                    }
                                    else
                                    {
                                        _selectedCustomBrosIndex = broIndex;
                                        _selectedBro = hero;
                                        _objectToEdit = _selectedBro.GetInfo();
                                        CreateSelectedBro();
                                    }
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
                                        SelectedBroUI(_selectedBro, mod);
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
                                if (willShowOptions)
                                {
                                    SelectedBroUI(_selectedBro, mod);
                                }
                            }
                            GUILayout.EndVertical();
                        }
                        // Just count up how many bros are in the mod if not displaying the custom bros
                        else
                        {
                            broIndex += mod.StoredHeroes.Length;
                        }

                        GUILayout.Space(13);
                    }
                    ++modCount;
                }

                // List mods that require a higher BroMaker version to make it clearer to the user what the issue is
                foreach (BroMakerMod mod in IncompatibleMods)
                {
                    // show mod information
                    GUILayout.BeginHorizontal("box");
                    if ((_selectedModIndex == modCount) != GUILayout.Toggle(_selectedModIndex == modCount, mod.Name, _buttonStyle, GUILayout.Width(150)))
                    {
                        // Deselect
                        if (_selectedModIndex == modCount)
                        {
                            _selectedModIndex = -1;
                        }
                        else
                        {
                            _selectedModIndex = modCount;
                        }
                    }
                    GUILayout.Space(50);

                    GUILayout.Label(mod.Author, GUILayout.Width(200));
                    GUILayout.Label(mod.Version, GUILayout.Width(200));
                    GUILayout.Label(mod.BroMakerVersion, _incompatibleStyle, GUILayout.Width(200));
                    GUILayout.EndHorizontal();

                    if (_selectedModIndex == modCount)
                    {
                        // Show bros
                        GUILayout.BeginVertical("box");

                        GUILayout.Label(mod.ErrorMessage, _incompatibleStyle);

                        GUILayout.EndVertical();
                    }


                    ++modCount;
                }

                GUILayout.Space( 5 );
                GUILayout.BeginHorizontal();
                if ( GUILayout.Button( "Enable All", GUILayout.Width(100) ) )
                {
                    foreach ( BroMakerMod mod in Mods )
                    {
                        foreach ( StoredHero hero in mod.StoredHeroes )
                        {
                            BSett.instance.setBroEnabled( hero.name, true );
                        }
                    }
                }
                GUILayout.Space( 5 );
                if ( GUILayout.Button( "Disable All", GUILayout.Width(100) ) )
                {
                    foreach ( BroMakerMod mod in Mods )
                    {
                        foreach ( StoredHero hero in mod.StoredHeroes )
                        {
                            BSett.instance.setBroEnabled( hero.name, false );
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                if (Mods.Count > 8)
                    GUILayout.EndScrollView();
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
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
                    UnityEngine.Object.DontDestroyOnLoad(heroHolder);
                }
                _selectedHero = heroHolder.AddComponent(PresetManager.heroesPreset[preset]) as CustomHero;
                _selectedHero.directoryPath = _selectedBro.info.path;
                _selectedHero.LoadSettings();
                _selectedHero.enabled = false;
                heroCreated = true;
            }
            catch (Exception ex)
            {
                BMLogger.Log(ex.ToString());
                heroCreated = false;
            }
        }


        private static void SelectedBroUI(StoredHero bro, BroMakerMod mod)
        {
            if (bro.Equals(null))
                return;

            GUILayout.BeginVertical("box");
            // Show warning for potentially incompatible version
            if (mod.ErrorMessage != string.Empty)
            {
                GUILayout.Label(mod.ErrorMessage, _warningStyle);
            }

            // Player Selector
            //Main.selectedPlayerNum = RGUI.HorizontalSliderInt("Player Num: ", Main.selectedPlayerNum, 0, 3, 200);
            GUILayout.BeginHorizontal( GUILayout.ExpandWidth( false ), GUILayout.Width(500) );
            for ( int i = 0; i < 4; ++i )
            {
                if ( ( Main.selectedPlayerNum == i ) != GUILayout.Toggle( Main.selectedPlayerNum == i, "Player " + ( i + 1 ), _buttonStyle ) )
                {
                    Main.selectedPlayerNum = i;
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

            if (GUILayout.Button(new GUIContent("Switch to Bro")))
            {
                LoadHero.previousSpawnInfo[Main.selectedPlayerNum] = Player.SpawnType.TriggerSwapBro;
                LoadHero.wasFirstDeployment[Main.selectedPlayerNum] = false;
                bro.LoadBro(Main.selectedPlayerNum);
            }
            bool broEnabled = BSett.instance.getBroEnabled(bro.name);
            if (GUILayout.Button( (broEnabled ? "Autospawn Enabled" : "Autospawn Disabled") ))
            {
                BSett.instance.setBroEnabled(bro.name, !broEnabled);
                if ( BSett.instance.equalSpawnProbability )
                {
                    BSett.instance.automaticSpawnProbabilty = BSett.instance.calculateSpawnProbability();
                }
            }
            if ( BSett.instance.developerMode )
            {
                if ( GUILayout.Button( "Duplicate File" ) )
                {
                    CreateFileEditor.DuplicateFile( bro.path );
                }
            }
            if (GUILayout.Button("Play Unlock Cutscene"))
            {
                try
                {
                    Cutscenes.CustomCutsceneController.LoadHeroCutscene(bro.GetInfo().cutscene);
                }
                catch (ArgumentNullException ex)
                {
                    BMLogger.ExceptionLog("The bro has no cutscene: " + ex.ToString());
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog(ex);
                }
            }
            GUILayout.EndHorizontal();

            // Display any UI options the bro developer added
            if ( _selectedHero != null )
            {
                try
                {
                    _selectedHero.UIOptions();
                }
                catch
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

        private static void SpawnerUIOneHero(BroMakerMod mod, int broIndex)
        {
            // show mod information
            GUILayout.BeginHorizontal("box");
            if ((_selectedCustomBrosIndex == broIndex) != GUILayout.Toggle(_selectedCustomBrosIndex == broIndex, mod.StoredHeroes[0].name, _buttonStyle, GUILayout.Width(150)))
            {
                if (_selectedCustomBrosIndex == broIndex)
                {
                    _selectedCustomBrosIndex = -1;
                }
                else
                {
                    _selectedCustomBrosIndex = broIndex;
                    _selectedBro = mod.StoredHeroes[0];
                    _objectToEdit = _selectedBro.GetInfo();
                    CreateSelectedBro();
                }
            }
            GUILayout.Space(50);
            GUILayout.Label(mod.Author, GUILayout.Width(200));
            GUILayout.Label(mod.Version, GUILayout.Width(200));
            if ( mod.ErrorMessage == string.Empty )
            {
                GUILayout.Label(mod.BroMakerVersion, GUILayout.Width(200));
            }
            else
            {
                GUILayout.Label(mod.BroMakerVersion, _warningStyle, GUILayout.Width(200));
            }
            if ( BSett.instance.getBroEnabled( mod.StoredHeroes[0].name ) )
            {
                GUILayout.Label( "Enabled", _enabledStyle, GUILayout.Width( 200 ) );
            }
            else
            {
                GUILayout.Label( "Disabled", _disabledStyle, GUILayout.Width( 200 ) );
            }
            GUILayout.EndHorizontal();

            // Show bros
            if (_selectedCustomBrosIndex == broIndex)
            {
                SelectedBroUI(_selectedBro, mod);
                GUILayout.Space(25);
            }
            else
            {
                GUILayout.Space(13);
            }
        }

        public static void Settings()
        {
            _showSpawnSettings = GUILayout.Toggle( _showSpawnSettings, "Spawn Options", _headerStyle );

            if ( _showSpawnSettings )
            {
                ShowSpawnSettings();
            }

            _showDeveloperSettings = GUILayout.Toggle( _showDeveloperSettings, "Developer Options", _headerStyle );

            if ( _showDeveloperSettings )
            {
                ShowDeveloperSettings();
            }

        }

        private static void ShowSpawnSettings()
        {
            GUILayout.Space( 15 );
            GUILayout.BeginHorizontal();
            BSett.instance.automaticSpawn = GUILayout.Toggle( BSett.instance.automaticSpawn, "Automatic Spawn" );
            if ( BSett.instance.equalSpawnProbability != ( BSett.instance.equalSpawnProbability = GUILayout.Toggle( BSett.instance.equalSpawnProbability, "Custom characters have an equal chance of spawning as normal characters" ) ) )
            {
                if ( BSett.instance.equalSpawnProbability )
                {
                    BSett.instance.automaticSpawnProbabilty = BSett.instance.calculateSpawnProbability();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space( 15 );

            if ( BSett.instance.automaticSpawnProbabilty != ( BSett.instance.automaticSpawnProbabilty = RGUI.HorizontalSlider( "Spawn Probability: ", BSett.instance.automaticSpawnProbabilty, 0f, 100f ) ) )
            {
                BSett.instance.equalSpawnProbability = false;
            }
            GUILayout.Space( 15 );
            BSett.instance.onlyCustomInHardcore = GUILayout.Toggle( BSett.instance.onlyCustomInHardcore, "Only custom characters will spawn in IronBro mode" );
            GUILayout.Space( 15 );
            BSett.instance.maxHealthAtOne = GUILayout.Toggle( BSett.instance.maxHealthAtOne, "Max health always at 1" );
            GUILayout.Space( 15 );
            BSett.instance.disableCustomAvatarFlash = GUILayout.Toggle( BSett.instance.disableCustomAvatarFlash, "Disable avatar flashing for custom bros" );
        }

        private static void ShowDeveloperSettings()
        {
            GUILayout.Space( 15 );
            GUILayout.BeginHorizontal();
            if ( GUILayout.Button( "Check Directories", GUILayout.ExpandWidth( false ) ) )
                DirectoriesManager.Initialize();
            if ( GUILayout.Button( "Show Presets", GUILayout.ExpandWidth( false ) ) )
            {
                foreach ( KeyValuePair<string, Type> kvp in PresetManager.heroesPreset )
                {
                    Main.Log( $"{kvp.Key}\t{kvp.Value}" );
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space( 15 );
            _Settings.debugLogs = GUILayout.Toggle( _Settings.debugLogs, "Debug Logs" );
            if ( BSett.instance.developerMode != (BSett.instance.developerMode = GUILayout.Toggle( BSett.instance.developerMode, "Developer Mode" ) ) )
            {
                // Enabled developer mode
                if ( BSett.instance.developerMode )
                {
                    _tabSelected = _developerModeTabs.Count() - 1;
                }
                else
                {
                    _tabSelected = _normalTabs.Count() - 1;
                }
            }
        }

        private static void ReloadFiles()
        {
            BroMaker.ReloadFiles();
            _selectedCustomBrosIndex = -1;
        }
    }
}

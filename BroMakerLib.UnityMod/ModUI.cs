using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Editor;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using RocketLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using UnityEngine;
using UnityModManagerNet;
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
                return BroMakerStorage.mods;
            }
        }
        public static List<BroMakerMod> IncompatibleMods
        {
            get
            {
                return BroMakerStorage.incompatibleMods;
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
        private static GUIStyle _enabledStyleButton = null;
        private static GUIStyle _disabledStyleButton = null;

        // Settings Menu
        private static GUIStyle _headerStyle;
        private static bool _showGeneralSettings = true;
        private static bool _showSpawnSettings = true;
        private static bool _showDeveloperSettings = false;

        private static float _windowWidth = -1f;

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

            BSett.instance.CheckForDeletedBros();
            BSett.instance.CountEnabledBros();

            if ( BSett.instance.equalSpawnProbability )
            {
                BSett.instance.automaticSpawnProbabilty = BSett.instance.CalculateSpawnProbability();
            }
        }

        public static void UI()
        {
            if ( _windowWidth < 0 )
            {
                try
                {
                    GUILayout.BeginHorizontal();
                    if ( Event.current.type == EventType.Repaint )
                    {
                        Rect rect = GUILayoutUtility.GetRect( 0, 0, GUILayout.ExpandWidth( true ) );
                        if ( rect.width > 1 )
                        {
                            _windowWidth = rect.width;
                        }
                    }
                    GUILayout.Label( " " );
                    GUILayout.EndHorizontal();
                }
                catch ( Exception )
                {
                }
                return;
            }

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

            if ( _enabledStyleButton == null )
            {
                _enabledStyleButton = new GUIStyle( GUI.skin.button );
                _enabledStyleButton.normal.textColor = Color.green;
            }

            if ( _disabledStyleButton == null )
            {
                _disabledStyleButton = new GUIStyle( GUI.skin.button );
                _disabledStyleButton.normal.textColor = Color.red;
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
            if ( !BSett.instance.disableTooltips )
            {
                GUI.Label( _toolTipRect, GUI.tooltip, _toolTipStyle );
            }
        }

        private static GUILayoutOption ScaledWidth( float width )
        {
            if ( BSett.instance.scaleUIWithWindowWidth && _windowWidth > 0 )
            {
                float scaleFactor = _windowWidth / 1200f;
                return GUILayout.Width( width * scaleFactor );
            }
            return GUILayout.Width( width );
        }

        public static void Spawner()
        {
            GUILayout.Label(BMLogger.errorSwapingMessage, _errorSwapingMessageStyle);
            if ( BSett.instance.developerMode )
            {
                GUILayout.BeginHorizontal();
                if ( GUILayout.Button( new GUIContent( "Reload Mods", "Reloads all BroMaker mods" ), GUILayout.ExpandWidth( false ) ) )
                {
                    BroMakerStorage.Initialize();
                }
                if ( GUILayout.Button( new GUIContent( "Reload Bros", "Reloads all custom bros in the currently loaded BroMaker mods" ), GUILayout.ExpandWidth( false ) ) )
                {
                    ReloadFiles();
                    PresetManager.disableWarnings = true;
                    PresetManager.Initialize();
                    PresetManager.disableWarnings = false;
                    BSett.instance.CheckForDeletedBros();
                    BSett.instance.CountEnabledBros();
                    if ( BSett.instance.equalSpawnProbability )
                    {
                        BSett.instance.automaticSpawnProbabilty = BSett.instance.CalculateSpawnProbability();
                    }
                }
                if ( GUILayout.Button( new GUIContent( "Reload Preset", "Reloads custom bro presets" ), GUILayout.ExpandWidth( false ) ) )
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
            GUILayout.Label("Name", ScaledWidth(150));
            GUILayout.Space(50);
            GUILayout.Label("Author", ScaledWidth(200));
            GUILayout.Label("Version", ScaledWidth(200));
            GUILayout.Label("BroMaker Version", ScaledWidth(200));
            GUILayout.Label( "Autospawn Enabled", ScaledWidth( 200 ) );
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            if (Mods.Count > 8 && !BSett.instance.scaleUIHeight)
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
                        if ((_selectedModIndex == modCount) != GUILayout.Toggle(_selectedModIndex == modCount, name, _buttonStyle, ScaledWidth(150)) )
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
                        GUILayout.Label(mod.Author, ScaledWidth(200));
                        GUILayout.Label(mod.Version, ScaledWidth(200));
                        if ( mod.ErrorMessage == string.Empty )
                        {
                            GUILayout.Label(mod.BroMakerVersion, ScaledWidth(200));
                        }
                        else
                        {
                            GUILayout.Label(mod.BroMakerVersion, _warningStyle, ScaledWidth(200));
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

                                if ( (_selectedCustomBrosIndex == broIndex) != GUILayout.Toggle(_selectedCustomBrosIndex == broIndex, mod.StoredHeroes[modIndex].name, _buttonStyle, ScaledWidth(150)) )
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
                    if ((_selectedModIndex == modCount) != GUILayout.Toggle(_selectedModIndex == modCount, mod.Name, _buttonStyle, ScaledWidth(150)))
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

                    GUILayout.Label(mod.Author, ScaledWidth(200));
                    GUILayout.Label(mod.Version, ScaledWidth(200));
                    GUILayout.Label(mod.BroMakerVersion, _incompatibleStyle, ScaledWidth(200));
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
                if ( GUILayout.Button( new GUIContent( "Enable All", "Enable autospawn for all custom bros" ), ScaledWidth(100) ) )
                {
                    foreach ( BroMakerMod mod in Mods )
                    {
                        foreach ( StoredHero hero in mod.StoredHeroes )
                        {
                            BSett.instance.SetBroEnabled( hero.name, true );
                        }
                    }
                }
                GUILayout.Space( 5 );
                if ( GUILayout.Button( new GUIContent( "Disable All", "Disable autospawn for all custom bros" ), ScaledWidth(100) ) )
                {
                    foreach ( BroMakerMod mod in Mods )
                    {
                        foreach ( StoredHero hero in mod.StoredHeroes )
                        {
                            BSett.instance.SetBroEnabled( hero.name, false );
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                if (Mods.Count > 8 && !BSett.instance.scaleUIHeight)
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
            GUILayout.BeginHorizontal( GUILayout.ExpandWidth( false ), ScaledWidth(500) );
            for ( int i = 0; i < 4; ++i )
            {
                if ( ( Main.selectedPlayerNum == i ) != GUILayout.Toggle( Main.selectedPlayerNum == i, new GUIContent( "Player " + ( i + 1 ), "Select which player to switch to this custom bro when you press switch to bro" ), _buttonStyle ) )
                {
                    Main.selectedPlayerNum = i;
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

            if (GUILayout.Button(new GUIContent("Switch to Bro", "Switch selected player to this custom bro"), ScaledWidth( 300 ) ))
            {
                LoadHero.previousSpawnInfo[Main.selectedPlayerNum] = Player.SpawnType.TriggerSwapBro;
                LoadHero.wasFirstDeployment[Main.selectedPlayerNum] = false;
                bro.LoadBro(Main.selectedPlayerNum);
            }
            bool broEnabled = BSett.instance.GetBroEnabled(bro.name);
            if ( mod.StoredHeroes.Count() > 1 )
            {
                if ( GUILayout.Button( new GUIContent( ( broEnabled ? "Autospawn Enabled" : "Autospawn Disabled" ), "Toggle whether this bro is part of the spawn rotation" ), ScaledWidth( 300 ) ) )
                {
                    BSett.instance.SetBroEnabled( bro.name, !broEnabled );
                    if ( BSett.instance.equalSpawnProbability )
                    {
                        BSett.instance.automaticSpawnProbabilty = BSett.instance.CalculateSpawnProbability();
                    }
                }
            }
            if (GUILayout.Button(new GUIContent("Play Unlock Cutscene", "Play this bro's unlock cutscene if in-game"), ScaledWidth( 300 ) ))
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
            if ((_selectedCustomBrosIndex == broIndex) != GUILayout.Toggle(_selectedCustomBrosIndex == broIndex, mod.StoredHeroes[0].name, _buttonStyle, ScaledWidth(150)))
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
            GUILayout.Label(mod.Author, ScaledWidth(200));
            GUILayout.Label(mod.Version, ScaledWidth(200));
            if ( mod.ErrorMessage == string.Empty )
            {
                GUILayout.Label(mod.BroMakerVersion, ScaledWidth(200));
            }
            else
            {
                GUILayout.Label(mod.BroMakerVersion, _warningStyle, ScaledWidth(200));
            }
            if ( !BSett.instance.automaticSpawn )
            {
                GUILayout.Button( new GUIContent( "Disabled in Settings", "Automatic spawning of bros is disabled in Spawn Options in the Settings tab" ), _disabledStyleButton, ScaledWidth( 130 ) );
            }
            else if ( BSett.instance.GetBroEnabled( mod.StoredHeroes[0].name ) )
            {
                if ( GUILayout.Button( new GUIContent("Enabled", "Click to disable autospawn for this bro"), _enabledStyleButton, ScaledWidth( 110 ) ) )
                {
                    BSett.instance.SetBroEnabled( mod.StoredHeroes[0].name, false );
                }
            }
            else
            {
                if ( GUILayout.Button( new GUIContent( "Disabled", "Click to enable autospawn for this bro"), _disabledStyleButton, ScaledWidth( 110 ) ) )
                {
                    BSett.instance.SetBroEnabled( mod.StoredHeroes[0].name, true );
                }
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
            _showGeneralSettings = GUILayout.Toggle( _showGeneralSettings, new GUIContent( "General Options", "Click to " + (_showGeneralSettings ? "collapse" : "expand") + " section" ), _headerStyle );

            if ( _showGeneralSettings )
            {
                ShowGeneralSettings();
            }

            _showSpawnSettings = GUILayout.Toggle( _showSpawnSettings, new GUIContent( "Spawn Options", "Click to " + ( _showSpawnSettings ? "collapse" : "expand" ) + " section" ), _headerStyle );

            if ( _showSpawnSettings )
            {
                ShowSpawnSettings();
            }

            _showDeveloperSettings = GUILayout.Toggle( _showDeveloperSettings, new GUIContent( "Developer Options", "Click to " + ( _showDeveloperSettings ? "collapse" : "expand" ) + " section" ), _headerStyle );

            if ( _showDeveloperSettings )
            {
                ShowDeveloperSettings();
            }

        }

        private static void ShowGeneralSettings()
        {
            GUILayout.Space( 15 );
            BSett.instance.scaleUIWithWindowWidth = GUILayout.Toggle( BSett.instance.scaleUIWithWindowWidth, new GUIContent( "Scale UI width based on window width", "Scales BroMaker settings UI elements based on the width of the UnityModManager window" ) );
            GUILayout.Space( 5 );
            BSett.instance.scaleUIHeight = GUILayout.Toggle( BSett.instance.scaleUIHeight, new GUIContent( "Scale UI height based on bro count", "Increases the height of the BroMaker settings window when more than 8 bros are installed rather than switching to using a scrollbar." ) );
            GUILayout.Space( 5 );
            BSett.instance.disableTooltips = GUILayout.Toggle( BSett.instance.disableTooltips, new GUIContent( "Disable Tooltips", "Disables tooltips in the BroMaker settings" ) );
            GUILayout.Space( 15 );
        }

        private static void ShowSpawnSettings()
        {
            GUILayout.Space( 15 );
            GUILayout.BeginHorizontal();
            BSett.instance.automaticSpawn = GUILayout.Toggle( BSett.instance.automaticSpawn, new GUIContent( "Automatic Spawn", "Enable automatic spawning of custom bros" ) );
            if ( BSett.instance.equalSpawnProbability != ( BSett.instance.equalSpawnProbability = GUILayout.Toggle( BSett.instance.equalSpawnProbability, new GUIContent( "Custom bros have an equal chance of spawning as vanilla bros", "Automatically adjusts spawn probability so that custom bros have the same probability of spawning as vanilla bros" ) ) ) )
            {
                if ( BSett.instance.equalSpawnProbability )
                {
                    BSett.instance.automaticSpawnProbabilty = BSett.instance.CalculateSpawnProbability();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space( 15 );

            if ( BSett.instance.automaticSpawnProbabilty != ( BSett.instance.automaticSpawnProbabilty = RGUI.HorizontalSlider( "Spawn Probability: ", "Probability of a custom bro spawning. The probability of any given custom bro spawning is equal to this divided by however many bros you have installed and enabled, which is " + (BSett.instance.automaticSpawnProbabilty / BSett.instance.enabledBroCount )+ "%", BSett.instance.automaticSpawnProbabilty, 0f, 100f ) ) )
            {
                BSett.instance.equalSpawnProbability = false;
            }
            GUILayout.Space( 15 );
            BSett.instance.onlyCustomInHardcore = GUILayout.Toggle( BSett.instance.onlyCustomInHardcore, new GUIContent( "Only custom characters will spawn in IronBro mode", "Only custom bros will be unlockable in IronBro, once you have unlocked them all you will be unable to gain more lives" ) );
            GUILayout.Space( 15 );
            BSett.instance.maxHealthAtOne = GUILayout.Toggle( BSett.instance.maxHealthAtOne, new GUIContent( "Max health always at 1", "This makes sure that bros default to 1 health even if it's not explicitly set in the json file" ) );
            GUILayout.Space( 15 );
            BSett.instance.disableCustomAvatarFlash = GUILayout.Toggle( BSett.instance.disableCustomAvatarFlash, new GUIContent( "Disable avatar flashing for custom bros", "Prevents avatar flash effect on custom bros that plays when invulnerable or idle. This is disabled on vanilla bros by default." ) );
        }

        private static void ShowDeveloperSettings()
        {
            GUILayout.Space( 15 );
            GUILayout.BeginHorizontal();
            if ( GUILayout.Button( new GUIContent( "Check Directories", "Creates BroMaker_Storage directory if it doesn't already exist" ), GUILayout.ExpandWidth( false ) ) )
                DirectoriesManager.Initialize();
            if ( GUILayout.Button( new GUIContent( "Show Presets", "Lists all available presets to the log" ), GUILayout.ExpandWidth( false ) ) )
            {
                foreach ( KeyValuePair<string, Type> kvp in PresetManager.heroesPreset )
                {
                    Main.Log( $"{kvp.Key}\t{kvp.Value}" );
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space( 15 );
            _Settings.debugLogs = GUILayout.Toggle( _Settings.debugLogs, new GUIContent( "Debug Logs", "Enables debug logging which can be viewed in the UnityModManager log" ) );
            if ( BSett.instance.developerMode != (BSett.instance.developerMode = GUILayout.Toggle( BSett.instance.developerMode, new GUIContent( "Developer Mode", "Enables more options in the BroMakerSettings window for bro developers" ) ) ) )
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
            GUILayout.Space( 10 );
        }

        private static void ReloadFiles()
        {
            BroMaker.ReloadFiles();
            _selectedCustomBrosIndex = -1;
        }
    }
}

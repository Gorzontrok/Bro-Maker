using System;
using System.Collections.Generic;
using System.Linq;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using BroMakerLib.Unlocks;
using RocketLib;
using RocketLib.UMM;
using UnityEngine;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib
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

        private static readonly Dictionary<string, Action> _tabs = new Dictionary<string, Action>()
        {
            { "Custom Bros", Spawner },
            { "Settings", Settings }
        };

        private static int selectedPlayerNum = 0;
        private static object _objectToEdit = null;
        private static GUIStyle _toolTipStyle = null;
        private static int _tabSelected = 0;
        private static Rect _toolTipRect = Rect.zero;
        private static int _selectedCustomBrosIndex = -1;
        private static int _selectedModIndex = -1;
        private static StoredHero _selectedBro;
        private static CustomHero _selectedHero;
        private static GameObject heroHolder;
        private static bool heroCreated = false;

        private static Vector2 _spawnerScrollView;
        private static readonly int _broPerLines = 6;

        private static GUIStyle _buttonStyle = null;
        private static GUIStyle _warningStyle = null;
        private static GUIStyle _incompatibleStyle = null;
        private static GUIStyle _enabledStyle = null;
        private static GUIStyle _disabledStyle = null;
        private static GUIStyle _enabledStyleButton = null;
        private static GUIStyle _disabledStyleButton = null;

        // Settings Menu
        private static GUIStyle _headerStyle;

        public static void UI()
        {
            WindowScaling.Enabled = BSett.instance.scaleUIWithWindowWidth;

            if (!WindowScaling.TryCaptureWidth())
                return;

            if (_toolTipStyle == null)
            {
                _toolTipStyle = new GUIStyle
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 15,
                };
                _toolTipStyle.normal.textColor = Color.white;
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

            if (_headerStyle == null)
            {
                _headerStyle = new GUIStyle(GUI.skin.button);
                _headerStyle.fontStyle = FontStyle.Bold;
                _headerStyle.normal.textColor = new Color(0.639216f, 0.909804f, 1f);
            }

            if (_enabledStyle == null)
            {
                _enabledStyle = new GUIStyle(GUI.skin.label);
                _enabledStyle.normal.textColor = Color.green;
            }

            if (_disabledStyle == null)
            {
                _disabledStyle = new GUIStyle(GUI.skin.label);
                _disabledStyle.normal.textColor = Color.red;
            }

            if (_enabledStyleButton == null)
            {
                _enabledStyleButton = new GUIStyle(GUI.skin.button);
                _enabledStyleButton.normal.textColor = Color.green;
            }

            if (_disabledStyleButton == null)
            {
                _disabledStyleButton = new GUIStyle(GUI.skin.button);
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
            if (!BSett.instance.disableTooltips)
            {
                GUI.Label(_toolTipRect, GUI.tooltip, _toolTipStyle);
            }
        }

        public static void Spawner()
        {
            GUILayout.BeginVertical();
            if (Mods.Count <= 0 && IncompatibleMods.Count <= 0)
            {
                GUILayout.Label("No mods installed.");
                GUILayout.EndVertical();
                return;
            }

            GUILayout.BeginHorizontal("box");
            GUILayout.Label("Name", WindowScaling.ScaledWidth(150));
            WindowScaling.ScaledSpace(50);
            GUILayout.Label("Author", WindowScaling.ScaledWidth(200));
            GUILayout.Label("Version", WindowScaling.ScaledWidth(200));
            GUILayout.Label("BroMaker Version", WindowScaling.ScaledWidth(200));
            GUILayout.Label("Autospawn Enabled", WindowScaling.ScaledWidth(200));
            GUILayout.Label("Unlock Status", WindowScaling.ScaledWidth(100));
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
                    if (mod.StoredHeroes.Length == 1)
                    {
                        SpawnerUIOneHero(mod, broIndex);

                        broIndex++;
                    }

                    // If more than one custom bros are in the mod, allow user to toggle to see them
                    else
                    {
                        // show mod information
                        GUILayout.BeginHorizontal("box");
                        if ((_selectedModIndex == modCount) != GUILayout.Toggle(_selectedModIndex == modCount, name, _buttonStyle, WindowScaling.ScaledWidth(150)))
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
                        WindowScaling.ScaledSpace(50);
                        GUILayout.Label(mod.Author, WindowScaling.ScaledWidth(200));
                        GUILayout.Label(mod.Version, WindowScaling.ScaledWidth(200));
                        if (mod.ErrorMessage == string.Empty)
                        {
                            GUILayout.Label(mod.BroMakerVersion, WindowScaling.ScaledWidth(200));
                        }
                        else
                        {
                            GUILayout.Label(mod.BroMakerVersion, _warningStyle, WindowScaling.ScaledWidth(200));
                        }
                        GUILayout.EndHorizontal();

                        // Only show custom bros of this mod if this mod is selected
                        if (_selectedModIndex == modCount)
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

                                if ((_selectedCustomBrosIndex == broIndex) != GUILayout.Toggle(_selectedCustomBrosIndex == broIndex, mod.StoredHeroes[modIndex].name, _buttonStyle, WindowScaling.ScaledWidth(150)))
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
                    if ((_selectedModIndex == modCount) != GUILayout.Toggle(_selectedModIndex == modCount, mod.Name, _buttonStyle, WindowScaling.ScaledWidth(150)))
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
                    WindowScaling.ScaledSpace(50);

                    GUILayout.Label(mod.Author, WindowScaling.ScaledWidth(200));
                    GUILayout.Label(mod.Version, WindowScaling.ScaledWidth(200));
                    GUILayout.Label(mod.BroMakerVersion, _incompatibleStyle, WindowScaling.ScaledWidth(200));
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

                GUILayout.Space(5);
                GUILayout.BeginHorizontal();
                if (GUILayout.Button(new GUIContent("Enable All", "Enable autospawn for all custom bros"), WindowScaling.ScaledWidth(100)))
                {
                    foreach (BroMakerMod mod in Mods)
                    {
                        foreach (StoredHero hero in mod.StoredHeroes)
                        {
                            BroSpawnManager.SetBroEnabled(hero.name, true);
                        }
                    }
                }
                GUILayout.Space(5);
                if (GUILayout.Button(new GUIContent("Disable All", "Disable autospawn for all custom bros"), WindowScaling.ScaledWidth(100)))
                {
                    foreach (BroMakerMod mod in Mods)
                    {
                        foreach (StoredHero hero in mod.StoredHeroes)
                        {
                            BroSpawnManager.SetBroEnabled(hero.name, false);
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
                string preset = (_objectToEdit as CustomBroInfo).CharacterPreset;
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
                _selectedHero.AssignDirectoryPaths(_selectedBro.info.path);
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

            bool broEnabled = BroSpawnManager.IsBroEnabled(bro.name);
            bool broUnlocked = BroUnlockManager.IsBroUnlocked(bro.name);

            if (broUnlocked)
            {
                // Player Selector
                GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), WindowScaling.ScaledWidth(500));
                for (int i = 0; i < 4; ++i)
                {
                    if ((selectedPlayerNum == i) != GUILayout.Toggle(selectedPlayerNum == i, new GUIContent("Player " + (i + 1), "Select which player to switch to this custom bro when you press switch to bro"), _buttonStyle))
                    {
                        selectedPlayerNum = i;
                    }
                }
                GUILayout.EndHorizontal();
            }


            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

            if (broUnlocked)
            {
                if (GUILayout.Button(new GUIContent("Switch to Bro", "Switch selected player to this custom bro"), WindowScaling.ScaledWidth(300)))
                {
                    LoadHero.previousSpawnInfo[selectedPlayerNum] = Player.SpawnType.TriggerSwapBro;
                    LoadHero.wasFirstDeployment[selectedPlayerNum] = false;
                    bro.LoadBro(selectedPlayerNum);
                }

                if (GUILayout.Button(new GUIContent("Play Unlock Cutscene", "Play this bro's unlock cutscene if in-game"), WindowScaling.ScaledWidth(300)))
                {
                    try
                    {
                        var cutscenes = bro.GetInfo().Cutscene;
                        if (cutscenes.Count > 0)
                        {
                            // Choose a random cutscene variant
                            int randomIndex = UnityEngine.Random.Range(0, cutscenes.Count);
                            Cutscenes.CustomCutsceneController.LoadHeroCutscene(cutscenes[randomIndex]);
                        }
                        else
                        {
                            throw new ArgumentNullException("The bro has no cutscene");
                        }
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
            }

            // Only displaly spawn options for mods with multiple bros
            if (mod.StoredHeroes.Length > 1)
            {
                if (broUnlocked)
                {
                    WindowScaling.ScaledSpace(200);
                    // Show disabled in settings for enable / disable toggle
                    if (!BSett.instance.automaticSpawn)
                    {
                        GUILayout.Button(new GUIContent("Disabled in Settings", "Automatic spawning of bros is disabled in Spawn Options in the Settings tab"), _disabledStyleButton, WindowScaling.ScaledWidth(130));
                        WindowScaling.ScaledSpace(70);
                    }
                    // Show locked for enable / disable toggle
                    else if (!broUnlocked)
                    {
                        GUILayout.Button(new GUIContent("Locked", "Bro is locked, rescue more lives or play their unlock level to unlock them. The unlock level can be accessed from the Custom Bros menu on the main menu."), _disabledStyleButton, WindowScaling.ScaledWidth(110));
                        WindowScaling.ScaledSpace(90);
                    }
                    // Show Enabled for enable / disable toggle
                    else if (broEnabled)
                    {
                        if (GUILayout.Button(new GUIContent("Enabled", "Click to disable autospawn for this bro"), _enabledStyleButton, WindowScaling.ScaledWidth(110)))
                        {
                            BroSpawnManager.SetBroEnabled(bro.name, false);
                        }
                        WindowScaling.ScaledSpace(90);
                    }
                    // Show Disabled for enable / disable toggle
                    else
                    {
                        if (GUILayout.Button(new GUIContent("Disabled", "Click to enable autospawn for this bro"), _disabledStyleButton, WindowScaling.ScaledWidth(110)))
                        {
                            BroSpawnManager.SetBroEnabled(bro.name, true);
                        }
                        WindowScaling.ScaledSpace(90);
                    }
                    // Show locked status
                    if (broUnlocked)
                    {
                        GUILayout.Label(new GUIContent("Unlocked", "Bro is unlocked"), _enabledStyle, WindowScaling.ScaledWidth(100));
                    }
                    else
                    {
                        GUILayout.Label(new GUIContent("Locked", "Bro is locked, rescue more lives or play their unlock level to unlock them. The unlock level can be accessed from the Custom Bros menu on the main menu."), _disabledStyle, WindowScaling.ScaledWidth(100));
                    }
                }
                else
                {
                    GUILayout.Button(new GUIContent("Locked", "Bro is locked, rescue more lives or play their unlock level to unlock them. The unlock level can be accessed from the Custom Bros menu on the main menu."), _disabledStyleButton, WindowScaling.ScaledWidth(110));
                }
            }

            GUILayout.EndHorizontal();

            // Display any UI options the bro developer added
            if (_selectedHero != null)
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
            else if (heroCreated)
            {
                CreateSelectedBro();
            }

            GUILayout.EndVertical();
        }

        private static void SpawnerUIOneHero(BroMakerMod mod, int broIndex)
        {
            // show mod information
            GUILayout.BeginHorizontal("box");
            if ((_selectedCustomBrosIndex == broIndex) != GUILayout.Toggle(_selectedCustomBrosIndex == broIndex, mod.StoredHeroes[0].name, _buttonStyle, WindowScaling.ScaledWidth(150)))
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
            WindowScaling.ScaledSpace(50);
            GUILayout.Label(mod.Author, WindowScaling.ScaledWidth(200));
            GUILayout.Label(mod.Version, WindowScaling.ScaledWidth(200));
            if (mod.ErrorMessage == string.Empty)
            {
                GUILayout.Label(mod.BroMakerVersion, WindowScaling.ScaledWidth(200));
            }
            else
            {
                GUILayout.Label(mod.BroMakerVersion, _warningStyle, WindowScaling.ScaledWidth(200));
            }

            bool broEnabled = BroSpawnManager.IsBroEnabled(mod.StoredHeroes[0].name);
            bool broUnlocked = BroUnlockManager.IsBroUnlocked(mod.StoredHeroes[0].name);

            // Show disabled in settings for enable / disable toggle
            if (!BSett.instance.automaticSpawn)
            {
                GUILayout.Button(new GUIContent("Disabled in Settings", "Automatic spawning of bros is disabled in Spawn Options in the Settings tab"), _disabledStyleButton, WindowScaling.ScaledWidth(130));
                WindowScaling.ScaledSpace(70);
            }
            // Show locked for enable / disable toggle
            else if (!broUnlocked)
            {
                GUILayout.Button(new GUIContent("Locked", "Bro is locked, rescue more lives or play their unlock level to unlock them. The unlock level can be accessed from the Custom Bros menu on the main menu."), _disabledStyleButton, WindowScaling.ScaledWidth(110));
                WindowScaling.ScaledSpace(90);
            }
            // Show Enabled for enable / disable toggle
            else if (broEnabled)
            {
                if (GUILayout.Button(new GUIContent("Enabled", "Click to disable autospawn for this bro"), _enabledStyleButton, WindowScaling.ScaledWidth(110)))
                {
                    BroSpawnManager.SetBroEnabled(mod.StoredHeroes[0].name, false);
                }
                WindowScaling.ScaledSpace(90);
            }
            // Show Disabled for enable / disable toggle
            else
            {
                if (GUILayout.Button(new GUIContent("Disabled", "Click to enable autospawn for this bro"), _disabledStyleButton, WindowScaling.ScaledWidth(110)))
                {
                    BroSpawnManager.SetBroEnabled(mod.StoredHeroes[0].name, true);
                }
                WindowScaling.ScaledSpace(90);
            }

            // Show locked status
            if (broUnlocked)
            {
                GUILayout.Label(new GUIContent("Unlocked", "Bro is unlocked"), _enabledStyle, WindowScaling.ScaledWidth(100));
            }
            else
            {
                GUILayout.Label(new GUIContent("Locked", "Bro is locked, rescue more lives or play their unlock level to unlock them. The unlock level can be accessed from the Custom Bros menu on the main menu."), _disabledStyle, WindowScaling.ScaledWidth(100));
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
            BSett.instance.showGeneralSettings = GUILayout.Toggle(BSett.instance.showGeneralSettings, new GUIContent("General Options", "Click to " + (BSett.instance.showGeneralSettings ? "collapse" : "expand") + " section"), _headerStyle);

            if (BSett.instance.showGeneralSettings)
            {
                ShowGeneralSettings();
            }

            BSett.instance.showSpawnSettings = GUILayout.Toggle(BSett.instance.showSpawnSettings, new GUIContent("Spawn Options", "Click to " + (BSett.instance.showSpawnSettings ? "collapse" : "expand") + " section"), _headerStyle);

            if (BSett.instance.showSpawnSettings)
            {
                ShowSpawnSettings();
            }

            BSett.instance.showDeveloperSettings = GUILayout.Toggle(BSett.instance.showDeveloperSettings, new GUIContent("Developer Options", "Click to " + (BSett.instance.showDeveloperSettings ? "collapse" : "expand") + " section"), _headerStyle);

            if (BSett.instance.showDeveloperSettings)
            {
                ShowDeveloperSettings();
            }

        }

        private static void ShowGeneralSettings()
        {
            GUILayout.Space(15);
            BSett.instance.scaleUIWithWindowWidth = GUILayout.Toggle(BSett.instance.scaleUIWithWindowWidth, new GUIContent("Scale UI width based on window width", "Scales BroMaker settings UI elements based on the width of the UnityModManager window"));
            GUILayout.Space(5);
            BSett.instance.scaleUIHeight = GUILayout.Toggle(BSett.instance.scaleUIHeight, new GUIContent("Scale UI height based on bro count", "Increases the height of the BroMaker settings window when more than 8 bros are installed rather than switching to using a scrollbar."));
            GUILayout.Space(5);
            BSett.instance.disableTooltips = GUILayout.Toggle(BSett.instance.disableTooltips, new GUIContent("Disable Tooltips", "Disables tooltips in the BroMaker settings"));
            GUILayout.Space(15);
        }

        private static void ShowSpawnSettings()
        {
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            BSett.instance.automaticSpawn = GUILayout.Toggle(BSett.instance.automaticSpawn, new GUIContent("Automatic Spawn", "Enable automatic spawning of custom bros"));
            if (BSett.instance.equalSpawnProbability != (BSett.instance.equalSpawnProbability = GUILayout.Toggle(BSett.instance.equalSpawnProbability, new GUIContent("Custom bros have an equal chance of spawning as vanilla bros", "Automatically adjusts spawn probability so that custom bros have the same probability of spawning as vanilla bros"))))
            {
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);

            if (BSett.instance.equalSpawnProbability)
            {
                float current = BroSpawnManager.CalculateSpawnProbability();
                float now = RGUI.HorizontalSlider("Spawn Probability: ", "Probability of a custom bro spawning. The probability of any given custom bro spawning is equal to this divided by however many bros you have installed and enabled, which is " + (BroSpawnManager.CalculateSpawnProbability() / BroSpawnManager.EnabledBros.Count) + "%", BroSpawnManager.CalculateSpawnProbability(), 0f, 100f);
                if (current != now)
                {
                    BSett.instance.automaticSpawnProbabilty = now;
                    BSett.instance.equalSpawnProbability = false;
                }
            }
            else
            {
                BSett.instance.automaticSpawnProbabilty = RGUI.HorizontalSlider("Spawn Probability: ", "Probability of a custom bro spawning. The probability of any given custom bro spawning is equal to this divided by however many bros you have installed and enabled, which is " + (BroSpawnManager.CalculateSpawnProbability() / BroSpawnManager.EnabledBros.Count) + "%", BroSpawnManager.CalculateSpawnProbability(), 0f, 100f);
            }

            GUILayout.Space(15);
            BSett.instance.onlyCustomInHardcore = GUILayout.Toggle(BSett.instance.onlyCustomInHardcore, new GUIContent("Only custom characters will spawn in IronBro mode", "Only custom bros will be unlockable in IronBro, once you have unlocked them all you will be unable to gain more lives"));
            GUILayout.Space(15);
            BSett.instance.maxHealthAtOne = GUILayout.Toggle(BSett.instance.maxHealthAtOne, new GUIContent("Max health always at 1", "This makes sure that bros default to 1 health even if it's not explicitly set in the json file"));
            GUILayout.Space(15);
            BSett.instance.disableCustomAvatarFlash = GUILayout.Toggle(BSett.instance.disableCustomAvatarFlash, new GUIContent("Disable avatar flashing for custom bros", "Prevents avatar flash effect on custom bros that plays when invulnerable or idle. This is disabled on vanilla bros by default."));
        }

        private static void ShowDeveloperSettings()
        {
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(new GUIContent("Check Directories", "Creates BroMaker_Storage directory if it doesn't already exist"), GUILayout.ExpandWidth(false)))
                DirectoriesManager.Initialize();
            if (GUILayout.Button(new GUIContent("Show Presets", "Lists all available presets to the log"), GUILayout.ExpandWidth(false)))
            {
                foreach (KeyValuePair<string, Type> kvp in PresetManager.heroesPreset)
                {
                    Main.Log($"{kvp.Key}\t{kvp.Value}");
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15);
            BSett.instance.debugLogs = GUILayout.Toggle(BSett.instance.debugLogs, new GUIContent("Debug Logs", "Enables debug logging which can be viewed in the UnityModManager log"));
            BSett.instance.developerMode = GUILayout.Toggle(BSett.instance.developerMode, new GUIContent("Developer Mode", "Enables more options in the BroMakerSettings window for bro developers"));
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Unlock All Bros"))
            {
                BroUnlockManager.UnlockAllBros();
            }
            if (GUILayout.Button("Lock All Bros"))
            {
                BroUnlockManager.LockAllBros();
            }
            GUILayout.EndHorizontal();
        }
    }
}

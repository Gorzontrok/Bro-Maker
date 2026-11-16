using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Cutscenes;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using BroMakerLib.Unlocks;
using HarmonyLib;
using RocketLib;
using RocketLib.UMM;
using UnityEngine;
using UnityModManagerNet;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib
{
    internal static class ModUI
    {
        public static ReadOnlyCollection<BroMakerMod> Mods
        {
            get
            {
                return BroMakerStorage.Mods;
            }
        }

        public static ReadOnlyCollection<BroMakerMod> IncompatibleMods
        {
            get
            {
                return BroMakerStorage.IncompatibleMods;
            }
        }

        private static readonly Dictionary<string, Action> _tabs = new Dictionary<string, Action>
        {
            { "Custom Bros", Spawner },
            { "Settings", Settings },
            { "Issues", Issues }
        };

        private static string _pendingTab;
        private static IssueEntry _pendingHighlight;
        private static string _filterBeforeShowIssue;

        private static Vector2 _issuesScrollView;
        private static readonly HashSet<IssueEntry> _expandedIssues = new HashSet<IssueEntry>();
        private static readonly HashSet<string> _expandedGroups = new HashSet<string>();
        private static GUIStyle _issueModHeaderStyle = null;
        private static GUIStyle _issueBroHeaderStyle = null;
        private static GUIStyle _issueCountStyle = null;
        private static GUIStyle _issueMessageStyle = null;
        private static GUIStyle _issueExceptionStyle = null;
        private static GUIStyle _issueCopyButtonStyle = null;
        private static Texture2D _highlightTex = null;
        private static Texture2D _ruleTex = null;
        private static Texture2D _dotTex = null;
        private static int _issueStylesScale = -1;
        private static float _highlightStart = -1f;
        private static Rect _cachedHighlightRect;
        private static IssueEntry _cachedHighlightEntry;
        private static IssueEntry _lastCopiedEntry;
        private static float _lastCopiedAt = -1f;
        private const float HighlightHold = 1.0f;
        private const float HighlightFade = 0.5f;
        private const float HighlightDuration = HighlightHold + HighlightFade;
        private const float CopyFeedbackDuration = 1.5f;

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
        private static bool _uiOptionsFailed = false;
        private static bool _spawnerRenderFailed = false;
        private static bool _settingsRenderFailed = false;
        private static bool _issuesRenderFailed = false;

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
        private static GUIStyle _tabSelectionStyle;

        public static void UI()
        {
            WindowScaling.Enabled = BSett.instance.scaleUIWithWindowWidth;

            if (!WindowScaling.TryCaptureWidth())
            {
                return;
            }

            if (_toolTipStyle == null)
            {
                _toolTipStyle = new GUIStyle
                {
                    fontStyle = FontStyle.Bold,
                    fontSize = 15
                };
                _toolTipStyle.normal.textColor = Color.white;
            }

            if (_buttonStyle == null)
            {
                _buttonStyle = new GUIStyle(GUI.skin.button);
            }

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

            ReadOnlyCollection<IssueEntry> allIssues = BMLogger.GetIssues();
            int issueCount = allIssues.Count;
            int errorCount = 0;
            for (int i = 0; i < allIssues.Count; ++i)
            {
                if (allIssues[i].Severity == IssueSeverity.Error)
                {
                    ++errorCount;
                }
            }

            List<string> tabKeys = new List<string>(_tabs.Keys.Count);
            List<string> tabLabels = new List<string>(_tabs.Keys.Count);
            foreach (string key in _tabs.Keys)
            {
                if (key == "Issues")
                {
                    if (issueCount == 0)
                    {
                        continue;
                    }
                    string tabColor = errorCount > 0 ? "#FF6F6F" : "#FFC44D";
                    tabKeys.Add(key);
                    tabLabels.Add($"<color={tabColor}>Issues ({issueCount})</color>");
                }
                else
                {
                    tabKeys.Add(key);
                    tabLabels.Add(key);
                }
            }

            if (!string.IsNullOrEmpty(_pendingTab))
            {
                int pendingIndex = tabKeys.IndexOf(_pendingTab);
                if (pendingIndex >= 0)
                {
                    _tabSelected = pendingIndex;
                    _pendingTab = null;
                }
                else
                {
                    _pendingTab = null;
                    _pendingHighlight = null;
                }
            }

            if (_tabSelected >= tabKeys.Count)
            {
                _tabSelected = 0;
            }

            if (_tabSelectionStyle == null)
            {
                _tabSelectionStyle = new GUIStyle(GUI.skin.button) { richText = true };
            }
            int previousTab = _tabSelected;
            _tabSelected = GUILayout.SelectionGrid(_tabSelected, tabLabels.ToArray(), 4, _tabSelectionStyle);
            if (_tabSelected != previousTab)
            {
                ResetRenderFailures();
            }
            GUILayout.Space(15);
            _toolTipRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(10);

            GUILayout.BeginVertical("box");
            _tabs.TryGetValue(tabKeys[_tabSelected], out Action action);
            action.Invoke();
            GUILayout.EndVertical();
            if (!BSett.instance.disableTooltips)
            {
                GUI.Label(_toolTipRect, GUI.tooltip, _toolTipStyle);
            }
        }

        public static void Spawner()
        {
            if (_spawnerRenderFailed)
            {
                GUILayout.Label("The custom bro list could not be drawn. Check the UnityModManager log for details.", _warningStyle);
                return;
            }

            GUILayout.BeginVertical();
            EventType entryType = Event.current.type;
            try
            {
                SpawnerContent();
            }
            catch (Exception e)
            {
                _spawnerRenderFailed = ReportGUIFailure(entryType, e, "The Custom Bros tab failed to draw", "An action in the Custom Bros tab failed");
            }
            GUILayout.EndVertical();
        }

        // Only content entered on an unused input event can run a control's action, whose failure doesn't repeat every frame.
        private static bool ReportGUIFailure(EventType entryType, Exception e, string drawMessage, string actionMessage)
        {
            if (entryType == EventType.Layout || entryType == EventType.Repaint || entryType == EventType.Used)
            {
                BMLogger.Warning(drawMessage, e);
                return true;
            }
            BMLogger.Warning(actionMessage, e);
            return false;
        }

        private static void SpawnerContent()
        {
            if (Mods.Count <= 0 && IncompatibleMods.Count <= 0)
            {
                GUILayout.Label("No mods installed.");
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
            bool usesScrollView = Mods.Count > 8 && !BSett.instance.scaleUIHeight;
            if (usesScrollView)
            {
                _spawnerScrollView = GUILayout.BeginScrollView(_spawnerScrollView, GUILayout.Height(320));
            }

            int broIndex = 0;
            int modCount = 0;
            try
            {
                foreach (BroMakerMod mod in Mods)
                {
                    if (mod.StoredHeroes.Length == 0 || IncompatibleMods.Contains(mod))
                    {
                        continue;
                    }

                    string name = mod.Name;
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
                        if (_selectedModIndex == modCount != GUILayout.Toggle(_selectedModIndex == modCount, name, _buttonStyle, WindowScaling.ScaledWidth(150)))
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

                                if (_selectedCustomBrosIndex == broIndex != GUILayout.Toggle(_selectedCustomBrosIndex == broIndex, mod.StoredHeroes[modIndex].name, _buttonStyle, WindowScaling.ScaledWidth(150)))
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

                                broIndex++;
                                horizontalIndex++;
                                modIndex++;

                                if (horizontalIndex >= _broPerLines)
                                {
                                    GUILayout.EndHorizontal();
                                    isHorizontalOpen = false;
                                    horizontalIndex = 0;

                                    if (willShowOptions)
                                    {
                                        SelectedBroUI(_selectedBro, mod);
                                        willShowOptions = false;
                                    }
                                }
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
                    if (_selectedModIndex == modCount != GUILayout.Toggle(_selectedModIndex == modCount, mod.Name, _buttonStyle, WindowScaling.ScaledWidth(150)))
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
            }
            finally
            {
                if (usesScrollView)
                {
                    GUILayout.EndScrollView();
                }
            }
        }

        private static void CreateSelectedBro()
        {
            _uiOptionsFailed = false;
            using (BMLogger.Scope(_selectedBro?.mod?.Name, _selectedBro?.name))
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
                    Type presetType = PresetManager.GetHeroPreset(preset);
                    if (presetType == null)
                    {
                        throw new Exception($"'characterPreset': {preset} doesn't exist. Check if you have the preset installed or if there is a typo.");
                    }
                    if (!typeof(CustomHero).IsAssignableFrom(presetType))
                    {
                        _selectedHero = null;
                        heroCreated = false;
                        return;
                    }
                    if (heroHolder == null)
                    {
                        heroHolder = new GameObject();
                        heroHolder.SetActive(false);
                        UnityEngine.Object.DontDestroyOnLoad(heroHolder);
                    }
                    _selectedHero = heroHolder.AddComponent(presetType) as CustomHero;
                    _selectedHero.AssignDirectoryPaths(_selectedBro.info.path);
                    _selectedHero.LoadSettings();
                    _selectedHero.enabled = false;
                    heroCreated = true;
                }
                catch (Exception ex)
                {
                    BMLogger.Warning($"Failed to create preview for '{_selectedBro?.name}'", ex);
                    heroCreated = false;
                }
            }
        }


        private static void SelectedBroUI(StoredHero bro, BroMakerMod mod)
        {
            if (bro.Equals(null))
            {
                return;
            }

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
                    if (selectedPlayerNum == i != GUILayout.Toggle(selectedPlayerNum == i, new GUIContent("Player " + (i + 1), "Select which player to switch to this custom bro when you press switch to bro"), _buttonStyle))
                    {
                        selectedPlayerNum = i;
                    }
                }
                GUILayout.EndHorizontal();
            }


            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));

            if (broUnlocked)
            {
                if (HeroController.players[selectedPlayerNum] == null)
                {
                    GUILayout.Button(new GUIContent("Switch to Bro", "Join a level as Player " + (selectedPlayerNum + 1) + " to switch to this custom bro"), _disabledStyleButton, WindowScaling.ScaledWidth(300));
                }
                else if (GUILayout.Button(new GUIContent("Switch to Bro", "Switch selected player to this custom bro"), WindowScaling.ScaledWidth(300)))
                {
                    LoadHero.previousSpawnInfo[selectedPlayerNum] = Player.SpawnType.TriggerSwapBro;
                    LoadHero.wasFirstDeployment[selectedPlayerNum] = false;
                    bro.LoadBro(selectedPlayerNum);
                }

                List<CustomIntroCutscene> cutscenes = bro.GetInfo().Cutscene;
                if (cutscenes.Count > 0 && !CutsceneController.HasInstance())
                {
                    GUILayout.Button(new GUIContent("Play Unlock Cutscene", "Start a level to play this bro's unlock cutscene"), _disabledStyleButton, WindowScaling.ScaledWidth(300));
                }
                else if (cutscenes.Count > 0 && GUILayout.Button(new GUIContent("Play Unlock Cutscene", "Play this bro's unlock cutscene"), WindowScaling.ScaledWidth(300)))
                {
                    try
                    {
                        // Choose a random cutscene variant
                        int randomIndex = UnityEngine.Random.Range(0, cutscenes.Count);
                        CustomCutsceneController.LoadHeroCutscene(cutscenes[randomIndex]);
                    }
                    catch (Exception ex)
                    {
                        BMLogger.Warning($"Failed to play cutscene for '{bro.name}'", ex, mod?.Name, bro.name);
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
                if (!_uiOptionsFailed)
                {
                    using (BMLogger.Scope(mod?.Name, bro.name))
                    {
                        EventType entryType = Event.current.type;
                        try
                        {
                            _selectedHero.UIOptions();
                        }
                        catch (Exception ex)
                        {
                            _uiOptionsFailed = ReportGUIFailure(entryType, ex, $"The options of '{bro.name}' failed to draw", $"An action in the options of '{bro.name}' failed");
                        }
                    }
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
            if (_selectedCustomBrosIndex == broIndex != GUILayout.Toggle(_selectedCustomBrosIndex == broIndex, mod.StoredHeroes[0].name, _buttonStyle, WindowScaling.ScaledWidth(150)))
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
            if (_settingsRenderFailed)
            {
                GUILayout.Label("The settings could not be drawn. Check the UnityModManager log for details.", _warningStyle);
                return;
            }

            GUILayout.BeginVertical();
            EventType entryType = Event.current.type;
            try
            {
                SettingsContent();
            }
            catch (Exception e)
            {
                _settingsRenderFailed = ReportGUIFailure(entryType, e, "The Settings tab failed to draw", "An action in the Settings tab failed");
            }
            GUILayout.EndVertical();
        }

        private static void SettingsContent()
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
            {
                DirectoriesManager.Initialize();
            }
            if (GUILayout.Button(new GUIContent("Show Presets", "Lists all available presets to the log"), GUILayout.ExpandWidth(false)))
            {
                foreach (KeyValuePair<string, Type> kvp in PresetManager.GetAllHeroPresets())
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

        private static void ResetRenderFailures()
        {
            _spawnerRenderFailed = false;
            _settingsRenderFailed = false;
            _issuesRenderFailed = false;
        }

        internal static void OnShowGUI()
        {
            ResetRenderFailures();
        }

        internal static void OnHideGUI()
        {
            if (_filterBeforeShowIssue == null || UnityModManager.UI.Instance == null)
            {
                return;
            }
            try
            {
                Traverse filter = Traverse.Create(UnityModManager.UI.Instance).Field("mModFilter");
                if (filter.GetValue<string>() == Main.mod.Info.Id)
                {
                    filter.SetValue(_filterBeforeShowIssue);
                }
            }
            catch (Exception ex)
            {
                BMLogger.Warning("Failed to restore the mod list filter after showing an issue", ex);
            }
            _filterBeforeShowIssue = null;
        }

        internal static void ShowIssue(IssueEntry entry)
        {
            _pendingHighlight = entry;
            _pendingTab = "Issues";
            _highlightStart = -1f;
            _cachedHighlightEntry = null;

            UnityModManager.UI umm = UnityModManager.UI.Instance;
            if (umm == null)
            {
                return;
            }

            try
            {
                Traverse t = Traverse.Create(umm);
                t.Field("tabId").SetValue(0);
                if (_filterBeforeShowIssue == null)
                {
                    _filterBeforeShowIssue = t.Field("mModFilter").GetValue<string>() ?? string.Empty;
                }
                t.Field("mModFilter").SetValue(Main.mod.Info.Id);
                umm.ToggleWindow(true);

                int index = UnityModManager.modEntries.FindIndex(x => x.Info.Id == Main.mod.Info.Id);
                if (index >= 0)
                {
                    t.Property("ShowModSettings").SetValue(index);
                }
            }
            catch (Exception ex)
            {
                BMLogger.Warning("Failed to open the issue in BroMaker's settings", ex);
            }
        }

        public static void Issues()
        {
            if (_issuesRenderFailed)
            {
                GUILayout.Label("The issue list could not be drawn. Check the UnityModManager log for details.", _warningStyle);
                return;
            }

            GUILayout.BeginVertical();
            EventType entryType = Event.current.type;
            try
            {
                IssuesContent();
            }
            catch (Exception e)
            {
                _issuesRenderFailed = ReportGUIFailure(entryType, e, "The Issues tab failed to draw", "An action in the Issues tab failed");
            }
            GUILayout.EndVertical();
        }

        private static void IssuesContent()
        {
            EnsureIssueStyles();

            ReadOnlyCollection<IssueEntry> issues = BMLogger.GetIssues();

            if (_pendingHighlight != null && !issues.Contains(_pendingHighlight))
            {
                _pendingHighlight = null;
                _highlightStart = -1f;
                _cachedHighlightEntry = null;
            }

            if (_pendingHighlight != null && _highlightStart < 0f)
            {
                EnsureGroupsExpandedForEntry(_pendingHighlight);
            }

            if (issues.Count == 0)
            {
                GUILayout.Label("No issues recorded.");
                return;
            }

            _issuesScrollView = GUILayout.BeginScrollView(_issuesScrollView, GUILayout.Height(320));
            try
            {
                List<string> modOrder = new List<string>();
                Dictionary<string, List<IssueEntry>> byMod = new Dictionary<string, List<IssueEntry>>();
                for (int i = 0; i < issues.Count; ++i)
                {
                    string key = issues[i].ModName ?? string.Empty;
                    if (!byMod.ContainsKey(key))
                    {
                        byMod[key] = new List<IssueEntry>();
                        modOrder.Add(key);
                    }
                    byMod[key].Add(issues[i]);
                }

                foreach (string modKey in modOrder)
                {
                    if (modKey != string.Empty)
                    {
                        RenderModGroup(modKey, byMod[modKey]);
                    }
                }
                if (byMod.ContainsKey(string.Empty))
                {
                    RenderContextGap(byMod[string.Empty]);
                }
            }
            finally
            {
                GUILayout.EndScrollView();
            }

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear All", _issueCopyButtonStyle, WindowScaling.ScaledWidth(110)))
            {
                BMLogger.ClearIssues();
                _pendingHighlight = null;
                _expandedIssues.Clear();
                _expandedGroups.Clear();
                _highlightStart = -1f;
                _cachedHighlightEntry = null;
            }
            GUILayout.EndHorizontal();

            if (Event.current.type == EventType.Repaint && _pendingHighlight != null)
            {
                if (_highlightStart < 0f)
                {
                    _highlightStart = Time.unscaledTime;
                }
                else if (Time.unscaledTime - _highlightStart >= HighlightDuration)
                {
                    _pendingHighlight = null;
                    _highlightStart = -1f;
                    _cachedHighlightEntry = null;
                }
            }
        }

        private static float ComputeHighlightAlpha()
        {
            if (_highlightStart < 0f)
            {
                return 1f;
            }
            float elapsed = Time.unscaledTime - _highlightStart;
            if (elapsed < HighlightHold)
            {
                return 1f;
            }
            if (elapsed >= HighlightDuration)
            {
                return 0f;
            }
            return 1f - ((elapsed - HighlightHold) / HighlightFade);
        }

        private static void EnsureGroupsExpandedForEntry(IssueEntry entry)
        {
            string modName = entry.ModName ?? string.Empty;

            if (modName == string.Empty)
            {
                string headerLabel = string.IsNullOrEmpty(entry.BroName) ? "BroMaker" : entry.BroName;
                _expandedGroups.Add("ctx:" + headerLabel);
                return;
            }

            _expandedGroups.Add("mod:" + modName);

            if (!BMLogger.IsSingleBroMod(modName) && !string.IsNullOrEmpty(entry.BroName))
            {
                _expandedGroups.Add("bro:" + modName + "/" + entry.BroName);
            }
        }

        private static void RenderModGroup(string modName, List<IssueEntry> entries)
        {
            bool isSingleBro = BMLogger.IsSingleBroMod(modName);

            GUILayout.Space(16);

            string headerLabel = isSingleBro ? BroMakerStorage.GetModByName(modName).BrosNames[0] : modName;

            string groupKey = "mod:" + modName;
            bool expanded = _expandedGroups.Contains(groupKey);
            if (DrawGroupHeader(headerLabel, entries.Count, expanded, _issueModHeaderStyle, indent: 0, showCount: true))
            {
                if (expanded)
                {
                    _expandedGroups.Remove(groupKey);
                }
                else
                {
                    _expandedGroups.Add(groupKey);
                }
                expanded = !expanded;
            }
            if (!expanded)
            {
                return;
            }

            DrawHorizontalRule(0);

            List<string> broOrder = new List<string>();
            Dictionary<string, List<IssueEntry>> byBro = new Dictionary<string, List<IssueEntry>>();
            for (int i = 0; i < entries.Count; ++i)
            {
                string broKey = entries[i].BroName ?? string.Empty;
                if (!byBro.ContainsKey(broKey))
                {
                    byBro[broKey] = new List<IssueEntry>();
                    broOrder.Add(broKey);
                }
                byBro[broKey].Add(entries[i]);
            }

            if (!isSingleBro)
            {
                if (byBro.ContainsKey(string.Empty))
                {
                    RenderEntries(byBro[string.Empty]);
                }
                foreach (string broKey in broOrder)
                {
                    if (broKey == string.Empty)
                    {
                        continue;
                    }
                    GUILayout.Space(10);
                    string broGroupKey = "bro:" + modName + "/" + broKey;
                    bool broExpanded = _expandedGroups.Contains(broGroupKey);
                    if (DrawGroupHeader(broKey, byBro[broKey].Count, broExpanded, _issueBroHeaderStyle, indent: 20, showCount: false))
                    {
                        if (broExpanded)
                        {
                            _expandedGroups.Remove(broGroupKey);
                        }
                        else
                        {
                            _expandedGroups.Add(broGroupKey);
                        }
                        broExpanded = !broExpanded;
                    }
                    if (broExpanded)
                    {
                        DrawHorizontalRule(20);
                        RenderEntries(byBro[broKey]);
                    }
                }
            }
            else
            {
                RenderEntries(entries);
            }
        }

        private static void RenderContextGap(List<IssueEntry> entries)
        {
            List<string> broOrder = new List<string>();
            Dictionary<string, List<IssueEntry>> byBro = new Dictionary<string, List<IssueEntry>>();
            for (int i = 0; i < entries.Count; ++i)
            {
                string broKey = entries[i].BroName ?? string.Empty;
                if (!byBro.ContainsKey(broKey))
                {
                    byBro[broKey] = new List<IssueEntry>();
                    broOrder.Add(broKey);
                }
                byBro[broKey].Add(entries[i]);
            }

            foreach (string broKey in broOrder)
            {
                GUILayout.Space(16);
                string headerLabel = broKey == string.Empty ? "BroMaker" : broKey;
                string groupKey = "ctx:" + headerLabel;
                bool expanded = _expandedGroups.Contains(groupKey);
                if (DrawGroupHeader(headerLabel, byBro[broKey].Count, expanded, _issueModHeaderStyle, indent: 0, showCount: true))
                {
                    if (expanded)
                    {
                        _expandedGroups.Remove(groupKey);
                    }
                    else
                    {
                        _expandedGroups.Add(groupKey);
                    }
                    expanded = !expanded;
                }
                if (!expanded)
                {
                    continue;
                }
                DrawHorizontalRule(0);
                RenderEntries(byBro[broKey]);
            }
        }

        private static bool DrawGroupHeader(string name, int count, bool expanded, GUIStyle style, int indent, bool showCount)
        {
            string arrow = expanded ? "▼" : "▶";

            GUILayout.BeginHorizontal();
            if (indent > 0)
            {
                GUILayout.Space(indent);
            }

            GUIContent btnContent = new GUIContent($"{arrow}  {name}");
            Rect btnRect = GUILayoutUtility.GetRect(btnContent, style, GUILayout.ExpandWidth(true));
            // Manual hover detection — Unity's GUIStyle.hover.textColor doesn't activate reliably for label-base styles.
            bool hovered = btnRect.Contains(Event.current.mousePosition);
            string displayText = hovered ? $"{arrow}  <color=#FFE066>{name}</color>" : btnContent.text;
            bool clicked = GUI.Button(btnRect, displayText, style);

            if (showCount)
            {
                string countLabel = count == 1 ? "1 issue" : count + " issues";
                GUILayout.Label(countLabel, _issueCountStyle, GUILayout.ExpandWidth(false), WindowScaling.ScaledWidth(140));
            }
            GUILayout.EndHorizontal();
            return clicked;
        }

        private static void DrawHorizontalRule(int indent)
        {
            GUILayout.BeginHorizontal();
            if (indent > 0)
            {
                GUILayout.Space(indent);
            }
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUI.skin.label, GUILayout.Height(1), GUILayout.ExpandWidth(true));
            GUI.DrawTexture(r, _ruleTex);
            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        private static void RenderEntries(List<IssueEntry> entries)
        {
            for (int i = 0; i < entries.Count; ++i)
            {
                IssueEntry entry = entries[i];
                bool hasException = !string.IsNullOrEmpty(entry.ExceptionDetails);
                bool expanded = hasException && _expandedIssues.Contains(entry);
                bool isHighlighted = _pendingHighlight == entry;

                if (isHighlighted && Event.current.type == EventType.Repaint && _cachedHighlightEntry == entry && _highlightTex != null)
                {
                    float alpha = ComputeHighlightAlpha();
                    if (alpha > 0f)
                    {
                        Color prevAlpha = GUI.color;
                        GUI.color = new Color(1f, 1f, 1f, alpha);
                        GUI.DrawTexture(_cachedHighlightRect, _highlightTex);
                        GUI.color = prevAlpha;
                    }
                }

                Color dotColor = entry.Severity == IssueSeverity.Error ? new Color(0.95f, 0.4f, 0.4f) : new Color(0.95f, 0.8f, 0.3f);
                Color textColor = new Color(0.95f, 0.96f, 0.97f);

                GUILayout.BeginHorizontal();
                GUILayout.Space(20);

                int dotSize = UnityModManager.UI.Scale(10);
                int dotColWidth = dotSize + UnityModManager.UI.Scale(6);
                Rect dotRect = GUILayoutUtility.GetRect(dotColWidth, dotSize, GUILayout.Width(dotColWidth));

                _issueMessageStyle.normal.textColor = textColor;
                string baseText = entry.OccurrenceCount > 1 ? entry.Message + "  (×" + entry.OccurrenceCount + ")" : entry.Message;
                if (hasException)
                {
                    string fullText = baseText + "  " + (expanded ? "▼" : "▶");
                    GUIContent btnContent = new GUIContent(fullText);
                    Rect btnRect = GUILayoutUtility.GetRect(btnContent, _issueMessageStyle, GUILayout.ExpandWidth(true));
                    bool hovered = btnRect.Contains(Event.current.mousePosition);
                    string displayText = hovered ? "<color=#FFE066>" + fullText + "</color>" : fullText;
                    if (GUI.Button(btnRect, displayText, _issueMessageStyle))
                    {
                        if (expanded)
                        {
                            _expandedIssues.Remove(entry);
                        }
                        else
                        {
                            _expandedIssues.Add(entry);
                        }
                        expanded = !expanded;
                    }
                }
                else
                {
                    GUILayout.Label(baseText, _issueMessageStyle, GUILayout.ExpandWidth(true));
                }

                bool justCopied = _lastCopiedEntry == entry && Time.unscaledTime - _lastCopiedAt < CopyFeedbackDuration;
                string copyLabel = justCopied ? "✓ Copied" : "Copy";
                if (GUILayout.Button(copyLabel, _issueCopyButtonStyle, WindowScaling.ScaledWidth(90)))
                {
                    GUIUtility.systemCopyBuffer = FormatEntryForClipboard(entry);
                    _lastCopiedEntry = entry;
                    _lastCopiedAt = Time.unscaledTime;
                }
                Rect copyRect = GUILayoutUtility.GetLastRect();

                GUILayout.EndHorizontal();
                Rect rowRect = GUILayoutUtility.GetLastRect();

                if (Event.current.type == EventType.Repaint)
                {
                    // Centre on the height a one-line row would have, so a wrapped message keeps the dot beside its first line.
                    float oneLineMessageHeight = _issueMessageStyle.CalcHeight(new GUIContent("A"), 10000f);
                    float firstLineHeight = Mathf.Max(dotSize, Mathf.Max(oneLineMessageHeight, copyRect.height));
                    Color prevColor = GUI.color;
                    GUI.color = dotColor;
                    // Nudge down past the geometric center to land on the text's visual center (caps midpoint).
                    float dotY = rowRect.y + ((firstLineHeight - dotSize) * 0.5f) + UnityModManager.UI.Scale(3);
                    GUI.DrawTexture(new Rect(dotRect.x, dotY, dotSize, dotSize), _dotTex);
                    GUI.color = prevColor;
                }

                if (isHighlighted && _pendingHighlight != null)
                {
                    if (Event.current.type == EventType.Repaint)
                    {
                        _cachedHighlightRect = rowRect;
                        _cachedHighlightEntry = entry;
                        if (_highlightStart < 0f)
                        {
                            _issuesScrollView.y = Mathf.Max(0f, rowRect.y - 50f);
                        }
                    }
                }

                if (expanded && hasException)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(40);
                    GUILayout.Label(entry.ExceptionDetails, _issueExceptionStyle);
                    GUILayout.EndHorizontal();
                }

                GUILayout.Space(6);
            }
        }

        private static string FormatEntryForClipboard(IssueEntry entry)
        {
            string source = BMLogger.FormatSource(entry.ModName, entry.BroName);
            string sev = entry.Severity.ToString();
            string time = entry.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            string ex = string.IsNullOrEmpty(entry.ExceptionDetails) ? string.Empty : "\n" + entry.ExceptionDetails;
            return $"[{time}] [{sev}] {source}: {entry.Message}{ex}";
        }

        private static void EnsureIssueStyles()
        {
            if (_highlightTex == null)
            {
                _highlightTex = MakeSolidTex(new Color(1f, 0.92f, 0.35f, 0.45f));
                _ruleTex = MakeSolidTex(new Color(1f, 1f, 1f, 0.12f));
                _dotTex = MakeCircleTex(32);
            }

            int uiScale = UnityModManager.UI.Scale(100);
            if (_issueStylesScale != uiScale)
            {
                _issueStylesScale = uiScale;
                _issueModHeaderStyle = null;
                _issueBroHeaderStyle = null;
                _issueCountStyle = null;
                _issueMessageStyle = null;
                _issueExceptionStyle = null;
            }

            if (_issueModHeaderStyle == null)
            {
                _issueModHeaderStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = UnityModManager.UI.Scale(19), alignment = TextAnchor.MiddleLeft, padding = new RectOffset(4, 4, 4, 4), border = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0), richText = true };
                _issueModHeaderStyle.normal.background = null;
                _issueModHeaderStyle.hover.background = null;
                _issueModHeaderStyle.active.background = null;
                _issueModHeaderStyle.focused.background = null;
                _issueModHeaderStyle.normal.textColor = new Color(0.96f, 0.97f, 1f);
            }
            if (_issueBroHeaderStyle == null)
            {
                _issueBroHeaderStyle = new GUIStyle(GUI.skin.button) { fontStyle = FontStyle.Bold, fontSize = UnityModManager.UI.Scale(16), alignment = TextAnchor.MiddleLeft, padding = new RectOffset(4, 4, 4, 4), border = new RectOffset(0, 0, 0, 0), margin = new RectOffset(0, 0, 0, 0), richText = true };
                _issueBroHeaderStyle.normal.background = null;
                _issueBroHeaderStyle.hover.background = null;
                _issueBroHeaderStyle.active.background = null;
                _issueBroHeaderStyle.focused.background = null;
                _issueBroHeaderStyle.normal.textColor = new Color(0.9f, 0.93f, 0.98f);
            }
            if (_issueCountStyle == null)
            {
                _issueCountStyle = new GUIStyle(GUI.skin.label) { fontSize = UnityModManager.UI.Scale(14), alignment = TextAnchor.MiddleRight, padding = new RectOffset(0, 0, 8, 8) };
                _issueCountStyle.normal.textColor = new Color(0.6f, 0.65f, 0.7f);
            }
            if (_issueMessageStyle == null)
            {
                _issueMessageStyle = new GUIStyle(GUI.skin.label) { fontSize = UnityModManager.UI.Scale(14), alignment = TextAnchor.MiddleLeft, wordWrap = true, clipping = TextClipping.Clip, padding = new RectOffset(0, 0, 2, 2), richText = true };
            }
            if (_issueExceptionStyle == null)
            {
                _issueExceptionStyle = new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = UnityModManager.UI.Scale(12), padding = new RectOffset(0, 0, 0, 2), margin = new RectOffset(0, 0, 0, 0) };
                _issueExceptionStyle.normal.textColor = new Color(0.7f, 0.7f, 0.72f);
            }
            if (_issueCopyButtonStyle == null)
            {
                _issueCopyButtonStyle = new GUIStyle(GUI.skin.button) { padding = new RectOffset(8, 8, 4, 4) };
            }
        }

        private static Texture2D MakeSolidTex(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        private static Texture2D MakeCircleTex(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float radius = size * 0.5f;
            Vector2 center = new Vector2(radius - 0.5f, radius - 0.5f);
            for (int x = 0; x < size; ++x)
            {
                for (int y = 0; y < size; ++y)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01(radius - dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            return tex;
        }
    }
}

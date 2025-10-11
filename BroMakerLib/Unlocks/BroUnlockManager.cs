using System;
using System.Collections.Generic;
using System.IO;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using Newtonsoft.Json;

namespace BroMakerLib.Unlocks
{
    public static class BroUnlockManager
    {
        private static BroUnlockProgressData progressData;
        private static readonly List<string> unlockedBroNames = new List<string>();
        private static readonly List<StoredHero> unlockedBros = new List<StoredHero>();
        private static readonly string saveFilePath = Path.Combine(Settings.directory, "BroMaker_UnlockProgress.json");

        public static List<StoredHero> UnlockedBros => unlockedBros;
        public static List<string> UnlockedBroNames => unlockedBroNames;
        public static bool Initialized = false;

        public static void Initialize()
        {
            if (Initialized) return;

            if (progressData != null || LoadProgressData())
            {
                ProcessNewlyInstalledBros();
                UpdateUnlockedBrosLists();
                Initialized = true;
            }
        }

        public static void SetupProgressData(int rescueCount)
        {
            if (progressData != null)
            {
                return;
            }
            progressData = new BroUnlockProgressData();
            progressData.LastKnownTotalRescues = rescueCount;
            SaveProgressData();
        }

        private static bool LoadProgressData()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    string json = File.ReadAllText(saveFilePath);
                    progressData = JsonConvert.DeserializeObject<BroUnlockProgressData>(json);

                    if (progressData != null && progressData.Version != 1)
                    {
                        progressData = BroUnlockProgressData.MigrateData(progressData);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Failed to load unlock progress: {ex.Message}");
                progressData = new BroUnlockProgressData();
                return false;
            }
        }

        public static void SaveProgressData()
        {
            try
            {
                if (progressData == null) return;

                string json = JsonConvert.SerializeObject(progressData, Formatting.Indented);
                File.WriteAllText(saveFilePath, json);
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Failed to save unlock progress: {ex.Message}");
            }
        }

        private static void ProcessNewlyInstalledBros()
        {
            if (progressData == null)
            {
                progressData = new BroUnlockProgressData();
            }

            var allBros = BroMakerStorage.Bros;
            if (allBros == null) return;

            bool anyNewBros = false;
            foreach (var bro in allBros)
            {
                if (!progressData.BroStates.ContainsKey(bro.name))
                {
                    var unlockConfig = bro.GetInfo()?.UnlockConfig ?? new BroUnlockConfig();
                    var state = CreateNewBroUnlockState(bro, unlockConfig);

                    if (state.IsUnlocked)
                    {
                        BroSpawnManager.AddBroEnabled(bro, true);
                    }

                    progressData.BroStates[bro.name] = state;
                    anyNewBros = true;
                }
                else if (progressData.BroStates.TryGetValue(bro.name, out BroUnlockState currentState))
                {
                    // Check if unlock config has changed at all, in case bro was updated
                    if (bro.GetInfo()?.UnlockConfig != null)
                    {
                        BroUnlockConfig config = bro.GetInfo()?.UnlockConfig;
                        // If it has changed, recreate bro unlock state
                        if (currentState.ConfiguredMethod != config.Method || currentState.UnlockLevelPath != config.UnlockLevelPath)
                        {
                            progressData.BroStates[bro.name] = CreateNewBroUnlockState(bro, config);
                            anyNewBros = true;
                        }
                    }
                }
            }

            if (anyNewBros)
            {
                SaveProgressData();
            }
        }

        private static void UpdateUnlockedBrosLists()
        {
            unlockedBroNames.Clear();
            unlockedBros.Clear();

            if (progressData?.BroStates == null) return;

            foreach (var kvp in progressData.BroStates)
            {
                if (kvp.Value.IsUnlocked)
                {
                    unlockedBroNames.Add(kvp.Key);
                    var bro = BroMakerStorage.GetStoredHeroByName(kvp.Key);
                    if (bro != null)
                    {
                        unlockedBros.Add(bro);
                    }
                }
                // Disable all locked bros if they're enabled
                else if (BroSpawnManager.IsBroEnabled(kvp.Key))
                {
                    BroSpawnManager.SetBroEnabled(kvp.Key, false, true);
                }
            }
        }

        public static bool IsBroUnlocked(string broName)
        {
            if (progressData?.BroStates == null || !progressData.BroStates.ContainsKey(broName))
            {
                return true;
            }

            return progressData.BroStates[broName].IsUnlocked;
        }

        public static BroUnlockState GetBroUnlockState(string broName)
        {
            if (progressData?.BroStates == null || !progressData.BroStates.ContainsKey(broName))
                return null;

            return progressData.BroStates[broName];
        }

        public static bool HasPendingUnlockedBro()
        {
            return progressData?.PendingUnlocks != null && progressData.PendingUnlocks.Count > 0;
        }

        public static string GetAndClearPendingUnlockedBro()
        {
            if (progressData?.PendingUnlocks == null || progressData.PendingUnlocks.Count < 0)
                return null;
            for (int i = 0; i < progressData.PendingUnlocks.Count; i++)
            {
                string broName = progressData.PendingUnlocks[i];
                if (BroSpawnManager.IsBroSpawnable(broName))
                {
                    progressData.PendingUnlocks.RemoveAt(i);
                    SaveProgressData();
                    return broName;
                }
            }
            return null;
        }

        public static bool IsBroPendingUnlock(string broName)
        {
            return progressData?.PendingUnlocks != null && progressData.PendingUnlocks.Count > 0 && progressData.PendingUnlocks.Contains(broName);
        }

        public static void ClearPendingUnlock(string broName)
        {
            if (progressData?.PendingUnlocks != null && progressData.PendingUnlocks.Count > 0)
            {
                progressData.PendingUnlocks.Remove(broName);
                SaveProgressData();
            }
        }

        public static void CheckRescueUnlocks(int currentRescueCount)
        {
            if (progressData?.BroStates == null) return;

            bool anyUnlocked = false;
            foreach (var kvp in progressData.BroStates)
            {
                var state = kvp.Value;
                if (!state.IsUnlocked &&
                    (state.ConfiguredMethod == UnlockMethod.RescueCount ||
                     state.ConfiguredMethod == UnlockMethod.RescueOrLevel))
                {
                    if (currentRescueCount >= state.TargetRescueCount)
                    {
                        UnlockBro(state);
                        anyUnlocked = true;
                    }
                }
            }

            if (anyUnlocked)
            {
                Main.SaveAll();
            }

            progressData.LastKnownTotalRescues = currentRescueCount;
        }

        public static bool CheckLevelUnlocks(string levelName)
        {
            if (progressData?.BroStates == null || string.IsNullOrEmpty(levelName))
                return false;

            bool anyUnlocked = false;
            foreach (var kvp in progressData.BroStates)
            {
                var state = kvp.Value;
                if (!state.IsUnlocked &&
                    (state.ConfiguredMethod == UnlockMethod.UnlockLevel ||
                     state.ConfiguredMethod == UnlockMethod.RescueOrLevel))
                {
                    if (!string.IsNullOrEmpty(state.UnlockLevelName))
                    {
                        if (levelName.Equals(state.UnlockLevelName, StringComparison.OrdinalIgnoreCase))
                        {
                            UnlockBro(state, false);
                            anyUnlocked = true;
                        }
                    }
                }
            }

            if (anyUnlocked)
            {
                Main.SaveAll();
            }

            return anyUnlocked;
        }

        public static void UnlockBro(BroUnlockState state, bool queueUnlock = true)
        {
            if (state.IsUnlocked) return;

            state.IsUnlocked = true;
            state.UnlockedDate = DateTime.UtcNow;

            if (queueUnlock)
            {
                if (progressData.PendingUnlocks == null)
                {
                    progressData.PendingUnlocks = new List<string>();
                }
                progressData.PendingUnlocks.Add(state.BroName);
            }

            // Update the unlocked lists
            unlockedBroNames.Add(state.BroName);
            var bro = BroMakerStorage.GetStoredHeroByName(state.BroName);
            if (bro != null)
            {
                unlockedBros.Add(bro);
            }

            // Set bro to enabled immediately
            BroSpawnManager.SetBroEnabled(state.BroName, true, true);
            BroSpawnManager.BroStatusChanged();
        }

        public static void UnlockAllBros()
        {
            if (progressData?.BroStates == null) return;

            foreach (var kvp in progressData.BroStates)
            {
                if (!kvp.Value.IsUnlocked)
                {
                    kvp.Value.IsUnlocked = true;
                    kvp.Value.UnlockedDate = DateTime.UtcNow;
                }
            }

            UpdateUnlockedBrosLists();
            SaveProgressData();
            BroSpawnManager.BroStatusChanged();
        }

        public static void LockAllBros()
        {
            if (progressData?.BroStates == null) return;

            foreach (var kvp in progressData.BroStates)
            {
                if (kvp.Value.IsUnlocked)
                {
                    kvp.Value.IsUnlocked = false;
                    kvp.Value.UnlockedDate = null;
                }
            }

            UpdateUnlockedBrosLists();
            SaveProgressData();
            BroSpawnManager.BroStatusChanged();
        }

        private static BroUnlockState CreateNewBroUnlockState(StoredHero bro, BroUnlockConfig unlockConfig)
        {
            var state = new BroUnlockState
            {
                BroName = bro.name,
                ConfiguredMethod = unlockConfig.Method,
                FirstSeenDate = DateTime.UtcNow
            };

            if (unlockConfig.Method == UnlockMethod.AlwaysUnlocked)
            {
                UnlockBro(state);
            }
            else if (unlockConfig.Method == UnlockMethod.RescueCount ||
                     unlockConfig.Method == UnlockMethod.RescueOrLevel)
            {
                state.TargetRescueCount = CalculateStaggeredRescueTarget(unlockConfig.RescueCountRequired);
            }

            if (unlockConfig.Method == UnlockMethod.UnlockLevel ||
                unlockConfig.Method == UnlockMethod.RescueOrLevel)
            {
                state.UnlockLevelPath = unlockConfig.UnlockLevelPath;
                state.UnlockLevelName = unlockConfig.UnlockLevelName;

                if (!string.IsNullOrEmpty(unlockConfig.UnlockLevelPath) &&
                    !ValidateUnlockLevel(bro, unlockConfig.UnlockLevelPath))
                {
                    BMLogger.Error($"Bro '{bro.name}' has invalid unlock level: {unlockConfig.UnlockLevelPath}");
                }
            }

            return state;
        }

        private static int CalculateStaggeredRescueTarget(int baseRequirement)
        {
            if (progressData?.BroStates == null) return baseRequirement;

            int highestTarget = progressData.LastKnownTotalRescues;

            foreach (var kvp in progressData.BroStates)
            {
                var state = kvp.Value;
                if (!state.IsUnlocked &&
                    (state.ConfiguredMethod == UnlockMethod.RescueCount ||
                     state.ConfiguredMethod == UnlockMethod.RescueOrLevel))
                {
                    if (state.TargetRescueCount > highestTarget)
                    {
                        highestTarget = state.TargetRescueCount;
                    }
                }
            }

            return highestTarget + baseRequirement;
        }

        private static bool ValidateUnlockLevel(StoredHero bro, string levelPath)
        {
            if (string.IsNullOrEmpty(levelPath))
                return false;

            string fullLevelPath = System.IO.Path.Combine(bro.GetInfo().path, levelPath);
            return File.Exists(fullLevelPath) || File.Exists(fullLevelPath + ".bfc");
        }

        public static void OnModUnload()
        {
            SaveProgressData();
        }

        public static bool LoadUnlockLevel(string broName)
        {
            if (progressData?.BroStates == null || !progressData.BroStates.ContainsKey(broName))
            {
                BMLogger.Error($"Cannot load unlock level - bro '{broName}' not found");
                return false;
            }

            var state = progressData.BroStates[broName];
            if (string.IsNullOrEmpty(state.UnlockLevelPath))
            {
                BMLogger.Error($"Bro '{broName}' has no unlock level configured");
                return false;
            }

            var bro = BroMakerStorage.GetStoredHeroByName(broName);
            if (bro == null)
            {
                BMLogger.Error($"Cannot find bro '{broName}' in storage");
                return false;
            }

            string fullLevelPath = System.IO.Path.Combine(bro.GetInfo().path, state.UnlockLevelPath);

            if (!fullLevelPath.EndsWith(".bfc"))
            {
                fullLevelPath += ".bfc";
            }

            if (!File.Exists(fullLevelPath))
            {
                BMLogger.Error($"Unlock level file not found: {fullLevelPath}");
                return false;
            }

            try
            {
                byte[] campaignBytes = File.ReadAllBytes(fullLevelPath);
                Campaign campaign = FileIO.LoadCampaignBytes(campaignBytes, false, true);

                if (campaign != null && campaign.levels != null && campaign.levels.Length > 0)
                {
                    LevelSelectionController.ResetLevelAndGameModeToDefault();

                    LevelSelectionController.campaignToLoad = campaign.name;
                    LevelSelectionController.loadCustomCampaign = true;
                    LevelSelectionController.loadPublishedCampaign = false;
                    LevelSelectionController.CurrentLevelNum = 0;
                    LevelSelectionController.VictoryScene = "MainMenu";

                    LevelSelectionController.currentCampaign = campaign;

                    GameModeController.GameMode = campaign.header != null ? campaign.header.gameMode : GameMode.Campaign;

                    GameState.Instance.loadMode = MapLoadMode.Campaign;

                    LevelEditorGUI.levelEditorActive = false;

                    UnityEngine.SceneManagement.SceneManager.LoadScene("newJoin", UnityEngine.SceneManagement.LoadSceneMode.Single);

                    return true;
                }
                else
                {
                    BMLogger.Error($"Invalid campaign file or no levels found: {fullLevelPath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Error loading unlock level for '{broName}': {ex.Message}");
                return false;
            }
        }

        public static List<string> GetBrosWithUnlockLevels()
        {
            var result = new List<string>();
            if (progressData?.BroStates == null) return result;

            foreach (var kvp in progressData.BroStates)
            {
                var state = kvp.Value;
                if (!state.IsUnlocked &&
                    (state.ConfiguredMethod == UnlockMethod.UnlockLevel ||
                     state.ConfiguredMethod == UnlockMethod.RescueOrLevel) &&
                    !string.IsNullOrEmpty(state.UnlockLevelPath))
                {
                    result.Add(kvp.Key);
                }
            }

            return result;
        }
    }
}

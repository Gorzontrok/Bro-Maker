using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Unlocks
{
    internal static class BroUnlockManager
    {
        private static BroUnlockProgressData progressData;
        private static readonly List<string> unlockedBroNames = new List<string>();
        private static readonly List<StoredHero> unlockedBros = new List<StoredHero>();
        private static readonly string saveFilePath = Path.Combine(Settings.Directory, "BroMaker_UnlockProgress.json");

        private static string currentUnlockLevelCampaignName = string.Empty;
        private static string currentUnlockLevelBroName = string.Empty;

        internal static List<StoredHero> UnlockedBros => unlockedBros;
        internal static List<string> UnlockedBroNames => unlockedBroNames;
        internal static bool Initialized = false;

        internal static void Initialize()
        {
            if (Initialized) return;

            LoadProgressData();
            ProcessNewlyInstalledBros();
            CheckForDeletedBros();
            UpdateUnlockedBrosLists();
            Initialized = true;
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

        // Determine if progress data or pending unlocks contains any bros that are no longer installed
        private static void CheckForDeletedBros()
        {
            if (progressData == null) return;

            bool anyBrosRemoved = false;

            // Check BroStates for deleted bros
            var broStatesToRemove = new List<string>();
            foreach (var kvp in progressData.BroStates)
            {
                if (!BroMakerStorage.GetStoredHeroByName(kvp.Key, out _))
                {
                    broStatesToRemove.Add(kvp.Key);
                    anyBrosRemoved = true;
                }
            }

            foreach (var broName in broStatesToRemove)
            {
                progressData.BroStates.Remove(broName);
            }

            // Check PendingUnlocks for deleted bros
            if (progressData.PendingUnlocks != null)
            {
                int beforeCount = progressData.PendingUnlocks.Count;
                progressData.PendingUnlocks.RemoveAll(broName => !BroMakerStorage.GetStoredHeroByName(broName, out _));
                if (progressData.PendingUnlocks.Count < beforeCount)
                {
                    anyBrosRemoved = true;
                }
            }

            if (anyBrosRemoved)
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
                    if (BroMakerStorage.GetStoredHeroByName(kvp.Key, out StoredHero bro))
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

        internal static bool IsBroUnlocked(string broName)
        {
            if (progressData?.BroStates == null || !progressData.BroStates.ContainsKey(broName))
            {
                return true;
            }

            return progressData.BroStates[broName].IsUnlocked;
        }

        internal static BroUnlockState GetBroUnlockState(string broName)
        {
            if (progressData?.BroStates == null || !progressData.BroStates.TryGetValue(broName, out BroUnlockState state))
                return null;

            return state;
        }

        internal static bool HasPendingUnlockedBro()
        {
            // Check if there are any pending unlocks and make sure at least one of the pending unlocks is enabled and spawnable
            return progressData?.PendingUnlocks != null && progressData.PendingUnlocks.Count > 0 && progressData.PendingUnlocks.Any(broName => BroSpawnManager.IsBroSpawnable(broName));
        }

        internal static string GetAndClearPendingUnlockedBro()
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

        internal static bool IsBroPendingUnlock(string broName)
        {
            return progressData?.PendingUnlocks != null && progressData.PendingUnlocks.Count > 0 && progressData.PendingUnlocks.Contains(broName);
        }

        internal static void ClearPendingUnlock(string broName)
        {
            if (progressData?.PendingUnlocks != null && progressData.PendingUnlocks.Count > 0)
            {
                progressData.PendingUnlocks.Remove(broName);
                SaveProgressData();
            }
        }

        internal static void RescuedBro()
        {
            ++progressData.TotalRescues;
        }

        internal static int GetRemainingRescues(string broName)
        {
            if (progressData?.BroStates == null || !progressData.BroStates.TryGetValue(broName, out BroUnlockState state))
                return 0;

            int remaining = state.TargetRescueCount - progressData.TotalRescues;
            return Mathf.Max(0, remaining);
        }

        internal static void CheckRescueUnlocks()
        {
            if (progressData?.BroStates == null) return;

            bool anyUnlocked = false;
            bool anyStillLocked = false;
            foreach (var kvp in progressData.BroStates)
            {
                var state = kvp.Value;
                if (!state.IsUnlocked && (state.ConfiguredMethod == UnlockMethod.RescueCount || state.ConfiguredMethod == UnlockMethod.RescueOrLevel))
                {
                    anyStillLocked = true;

                    if (progressData.TotalRescues >= state.TargetRescueCount)
                    {
                        anyUnlocked = true;
                        UnlockBro(state);
                    }
                }
            }

            if (anyUnlocked)
            {
                Main.SaveAll();
            }
            else if (anyStillLocked)
            {
                SaveProgressData();
            }
        }

        internal static bool CheckLevelUnlocks(string campaignName)
        {
            if (progressData?.BroStates == null || string.IsNullOrEmpty(currentUnlockLevelCampaignName))
                return false;

            bool anyUnlocked = false;
            foreach (var kvp in progressData.BroStates)
            {
                var state = kvp.Value;
                if (!state.IsUnlocked &&
                    (state.ConfiguredMethod == UnlockMethod.UnlockLevel ||
                     state.ConfiguredMethod == UnlockMethod.RescueOrLevel))
                {
                    if (currentUnlockLevelCampaignName.Equals(campaignName, StringComparison.OrdinalIgnoreCase) && progressData.BroStates.ContainsKey(currentUnlockLevelBroName) && progressData.BroStates[currentUnlockLevelBroName] == state)
                    {
                        UnlockBro(state, false);
                        anyUnlocked = true;

                        if (state.ConfiguredMethod == UnlockMethod.RescueOrLevel)
                        {
                            RecalculateTargetsAfterLevelUnlock(state);
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

        internal static void UnlockBro(BroUnlockState state, bool queueUnlock = true)
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
            if (BroMakerStorage.GetStoredHeroByName(state.BroName, out StoredHero bro))
            {
                unlockedBros.Add(bro);
            }

            // Set bro to enabled immediately
            BroSpawnManager.SetBroEnabled(state.BroName, true, true);
            BroSpawnManager.BroStatusChanged();
        }

        internal static void UnlockAllBros()
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

        internal static void LockAllBros()
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
                state.OriginalRescueCount = unlockConfig.RescueCountRequired;
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

            int highestTarget = progressData.TotalRescues;

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

        private static void RecalculateTargetsAfterLevelUnlock(BroUnlockState unlockedBroState)
        {
            if (progressData?.BroStates == null)
                return;

            int currentRemaining = unlockedBroState.TargetRescueCount - progressData.TotalRescues;
            int skippedAmount;
            // If the bro was still more than its original rescue count away from being unlocked, consider the full original rescue count as skipped
            if (currentRemaining >= unlockedBroState.OriginalRescueCount)
            {
                skippedAmount = unlockedBroState.OriginalRescueCount;
            }
            // Otherwise, only consider the remaining amount as skipped
            else
            {
                skippedAmount = currentRemaining;
            }

            foreach (var kvp in progressData.BroStates)
            {
                var state = kvp.Value;
                if (!state.IsUnlocked && (state.ConfiguredMethod == UnlockMethod.RescueCount || state.ConfiguredMethod == UnlockMethod.RescueOrLevel) && state.TargetRescueCount > unlockedBroState.TargetRescueCount)
                {

                    state.TargetRescueCount -= skippedAmount;
                }
            }
        }

        private static bool ValidateUnlockLevel(StoredHero bro, string levelPath)
        {
            if (string.IsNullOrEmpty(levelPath))
                return false;

            string fullLevelPath = System.IO.Path.Combine(bro.GetInfo().path, levelPath);
            return File.Exists(fullLevelPath) || File.Exists(fullLevelPath + ".bfc");
        }

        internal static bool LoadUnlockLevel(string broName)
        {
            if (progressData?.BroStates == null || !progressData.BroStates.TryGetValue(broName, out BroUnlockState state))
            {
                BMLogger.Error($"Cannot load unlock level - bro '{broName}' not found");
                return false;
            }

            if (string.IsNullOrEmpty(state.UnlockLevelPath))
            {
                BMLogger.Error($"Bro '{broName}' has no unlock level configured");
                return false;
            }

            if (!BroMakerStorage.GetStoredHeroByName(broName, out StoredHero bro))
            {
                BMLogger.Error($"Cannot find bro '{broName}' in storage");
                return false;
            }

            string fullLevelPath = Path.Combine(bro.GetInfo().path, state.UnlockLevelPath);

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

                    currentUnlockLevelCampaignName = campaign.name;
                    currentUnlockLevelBroName = broName;

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

        internal static List<string> GetBrosWithUnlockLevels()
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

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
        private static readonly Queue<string> pendingUnlockedBros = new Queue<string>();
        private static readonly HashSet<string> unlockedBroNames = new HashSet<string>();
        private static readonly List<StoredHero> unlockedBros = new List<StoredHero>();
        private static readonly string saveFilePath = Path.Combine(Settings.directory, "BroMaker_UnlockProgress.json");

        public static List<StoredHero> UnlockedBros => unlockedBros;
        public static HashSet<string> UnlockedBroNames => unlockedBroNames;

        public static void Initialize()
        {
            LoadProgressData();
            ProcessNewlyInstalledBros();
            UpdateUnlockedBrosLists();
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

        public static bool HasPendingUnlockedBro()
        {
            return pendingUnlockedBros.Count > 0;
        }

        public static string GetAndClearPendingUnlockedBro()
        {
            if (pendingUnlockedBros.Count > 0)
            {
                return pendingUnlockedBros.Dequeue();
            }
            return null;
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
                        UnlockBro(state.BroName, "rescue count");
                        anyUnlocked = true;
                    }
                }
            }

            if (anyUnlocked)
            {
                SaveProgressData();
            }

            progressData.LastKnownTotalRescues = currentRescueCount;
        }

        public static bool CheckLevelUnlocks(string levelName)
        {
            if (progressData?.BroStates == null || string.IsNullOrEmpty(levelName))
                return false;

            BMLogger.Debug($"Checking level unlocks for: {levelName}");

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
                        BMLogger.Debug($"Comparing level '{levelName}' with required '{state.UnlockLevelName}'");

                        if (levelName.Equals(state.UnlockLevelName, StringComparison.OrdinalIgnoreCase))
                        {
                            UnlockBro(state.BroName, "level completion");
                            anyUnlocked = true;
                        }
                    }
                }
            }

            if (anyUnlocked)
            {
                SaveProgressData();
            }

            return anyUnlocked;
        }

        public static void UnlockBro(string broName, string reason = null)
        {
            if (progressData?.BroStates == null || !progressData.BroStates.ContainsKey(broName))
                return;

            var state = progressData.BroStates[broName];
            if (state.IsUnlocked) return;

            state.IsUnlocked = true;
            state.UnlockedDate = DateTime.UtcNow;

            pendingUnlockedBros.Enqueue(broName);

            // Update the unlocked lists
            unlockedBroNames.Add(broName);
            var bro = BroMakerStorage.GetStoredHeroByName(broName);
            if (bro != null)
            {
                unlockedBros.Add(bro);
            }

            string logMessage = $"Bro '{broName}' unlocked";
            if (!string.IsNullOrEmpty(reason))
            {
                logMessage += $" via {reason}";
            }
            BMLogger.Log(logMessage);
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
            BMLogger.Log("All bros have been unlocked via Developer Options");
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
                    var state = new BroUnlockState
                    {
                        BroName = bro.name,
                        ConfiguredMethod = unlockConfig.Method,
                        FirstSeenDate = DateTime.UtcNow
                    };

                    if (unlockConfig.Method == UnlockMethod.AlwaysUnlocked)
                    {
                        state.IsUnlocked = true;
                        state.UnlockedDate = DateTime.UtcNow;
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

                    progressData.BroStates[bro.name] = state;
                    anyNewBros = true;

                    BMLogger.Debug($"Registered new bro '{bro.name}' with unlock method: {unlockConfig.Method}");
                }
            }

            if (anyNewBros)
            {
                SaveProgressData();
            }
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
            bool exists = File.Exists(fullLevelPath) || File.Exists(fullLevelPath + ".bfc");

            if (!exists)
            {
                BMLogger.Debug($"Level file not found at: {fullLevelPath}");
            }

            return exists;
        }

        private static void LoadProgressData()
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

                    BMLogger.Debug($"Loaded unlock progress for {progressData?.BroStates?.Count ?? 0} bros");
                }
                else
                {
                    progressData = new BroUnlockProgressData();
                    BMLogger.Debug("No existing unlock progress file, creating new one");
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Failed to load unlock progress: {ex.Message}");
                progressData = new BroUnlockProgressData();
            }
        }

        private static void SaveProgressData()
        {
            try
            {
                if (progressData == null) return;

                string json = JsonConvert.SerializeObject(progressData, Formatting.Indented);
                File.WriteAllText(saveFilePath, json);
                BMLogger.Debug("Saved unlock progress data");
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Failed to save unlock progress: {ex.Message}");
            }
        }

        public static void OnModUnload()
        {
            SaveProgressData();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BroMakerLib.Loggers;
using BroMakerLib.Unlocks;
using RocketLib.CustomTriggers;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib.Storages
{
    public static class BroSpawnManager
    {
        #region Properties and Fields
        public enum BroState
        {
            NotInstalled,
            Locked,
            Disabled,
            Available
        }

        public static List<string> EnabledBrosNames
        {
            get => BSett.instance._enabledBros;
            set => BSett.instance._enabledBros = value;
        }

        public static List<StoredHero> EnabledBros { get; set; } = new List<StoredHero>();

        public static List<string> HardcoreBrosNotYetUnlocked
        {
            get => BSett.instance._notUnlockedBros[PlayerProgress.currentWorldMapSaveSlot];
            set => BSett.instance._notUnlockedBros[PlayerProgress.currentWorldMapSaveSlot] = value;
        }

        public static List<string> HardcoreAvailableBros
        {
            get => BSett.instance._availableBros[PlayerProgress.currentWorldMapSaveSlot];
            set => BSett.instance._availableBros[PlayerProgress.currentWorldMapSaveSlot] = value;
        }

        public static bool ForceCustomThisLevel => CustomTriggerStateManager.Get<bool>("forceCustomBros");
        public static List<StoredHero> ForcedCustoms => CustomTriggerStateManager.Get<List<StoredHero>>("forcedCustomBroList", new List<StoredHero>());

        public static bool RescuingHardcoreBro = false;
        public static bool LastSpawnWasUnlock { get; private set; }
        public static int BroStatusCount = int.MinValue;

        public static bool Initialized
        {
            get => BroUnlockManager.Initialized;
        }
        #endregion

        #region Queries
        public static ReadOnlyCollection<StoredHero> GetAllBros()
        {
            return BroMakerStorage.Bros;
        }

        public static List<StoredHero> GetAllUnlockedBros()
        {
            return BroUnlockManager.UnlockedBros;
        }

        public static List<string> GetAllUnlockedBrosNames()
        {
            return BroUnlockManager.UnlockedBroNames;
        }

        public static List<StoredHero> GetAllEnabledBros()
        {
            return EnabledBros;
        }

        public static List<StoredHero> GetAllSpawnableBros()
        {
            // If in IronBro, only return available bros
            if (GameModeController.IsHardcoreMode)
            {
                List<StoredHero> availableHeroes = new List<StoredHero>();
                foreach (string name in HardcoreAvailableBros)
                {
                    if (BroMakerStorage.GetStoredHeroByName(name, out StoredHero hero))
                    {
                        if (BroMakerNetwork.IsBroAvailable(hero)) availableHeroes.Add(hero);
                    }
                }
                return availableHeroes;
            }
            // If forcing customs this level, only return allowed customs
            else if (ForceCustomThisLevel)
            {
                return GetNetworkAvailableBros(ForcedCustoms);
            }
            // Otherwise return all enabled bros
            else
            {
                return GetNetworkAvailableBros(GetAllEnabledBros());
            }
        }

        public static List<string> GetAllSpawnableBrosNames()
        {
            // If in IronBro, only return available bros
            if (GameModeController.IsHardcoreMode)
            {
                return GetNetworkAvailableNames(HardcoreAvailableBros);
            }
            // If forcing customs this level, only return allowed customs
            else if (ForceCustomThisLevel)
            {
                List<string> names = new List<string>();
                foreach (StoredHero hero in ForcedCustoms)
                {
                    if (hero != null && IsBroAvailableByName(hero.name)) names.Add(hero.name);
                }
                return names;
            }
            // Otherwise return all enabled bros
            else
            {
                return EnabledBrosNames.Where(IsBroAvailableByName).ToList();
            }
        }

        public static bool IsBroUnlocked(string broName)
        {
            return BroUnlockManager.IsBroUnlocked(broName);
        }

        public static bool IsBroSpawnable(string broName)
        {
            if (!IsBroAvailableByName(broName))
                return false;

            if (!BroUnlockManager.IsBroUnlocked(broName))
                return false;

            if (GameModeController.IsHardcoreMode)
            {
                return HardcoreAvailableBros.Contains(broName);
            }
            else if (ForceCustomThisLevel)
            {
                foreach (StoredHero hero in ForcedCustoms)
                {
                    if (hero.name == broName)
                    {
                        return true;
                    }
                }
                return false;
            }

            return EnabledBrosNames.Contains(broName);
        }

        public static bool IsBroEnabled(string broName)
        {
            return EnabledBrosNames.Contains(broName);
        }

        public static BroState GetBroState(string broName)
        {
            if (!BroMakerStorage.GetStoredHeroByName(broName, out _))
                return BroState.NotInstalled;

            if (!BroUnlockManager.IsBroUnlocked(broName))
                return BroState.Locked;

            if (!IsBroAvailableByName(broName))
                return BroState.Disabled;

            if (GameModeController.IsHardcoreMode)
            {
                return HardcoreAvailableBros.Contains(broName)
                    ? BroState.Available
                    : BroState.Disabled;
            }

            bool isEnabled = EnabledBrosNames.Contains(broName);

            return isEnabled ? BroState.Available : BroState.Disabled;
        }

        public static bool HasPendingUnlockedBro()
        {
            return BroUnlockManager.HasPendingUnlockedBro();
        }

        internal static int GetCompatibleHardcoreBrosNotYetUnlockedCount()
        {
            return GetNetworkAvailableNames(HardcoreBrosNotYetUnlocked).Count;
        }
        #endregion

        private static bool IsBroAvailableByName(string broName)
        {
            StoredHero hero;
            return BroMakerStorage.GetStoredHeroByName(broName, out hero) && BroMakerNetwork.IsBroAvailable(hero);
        }

        private static List<string> GetNetworkAvailableNames(IEnumerable<string> names)
        {
            List<string> compatible = new List<string>();
            if (names == null) return compatible;
            foreach (string name in names)
                if (IsBroAvailableByName(name) && !compatible.Contains(name)) compatible.Add(name);
            return compatible;
        }

        private static List<StoredHero> GetNetworkAvailableBros(IEnumerable<StoredHero> heroes)
        {
            List<StoredHero> compatible = new List<StoredHero>();
            if (heroes == null) return compatible;
            foreach (StoredHero hero in heroes)
            {
                StoredHero registered;
                if (hero != null && BroMakerStorage.GetStoredHeroByName(hero.name, out registered) &&
                    ReferenceEquals(hero, registered) && BroMakerNetwork.IsBroAvailable(hero) && !compatible.Contains(hero))
                    compatible.Add(hero);
            }
            return compatible;
        }

        #region Spawning and Modification
        public static StoredHero GetRandomSpawnableBro(bool allowPendingUnlocks = false)
        {
            LastSpawnWasUnlock = false;

            StoredHero chosenBro = null;

            if (BSett.instance.overrideNextBroSpawn)
            {
                BSett.instance.overrideNextBroSpawn = false;
                if (!BroMakerStorage.GetStoredHeroByName(BSett.instance.nextBroSpawn, out chosenBro) ||
                    !BroMakerNetwork.IsBroAvailable(chosenBro))
                {
                    chosenBro = null;
                }
            }
            // If in ironbro, only return unlocked bros
            else if (GameModeController.IsHardcoreMode)
            {
                // Unlock additional bro if more are left to unlock
                if (RescuingHardcoreBro && HardcoreBrosNotYetUnlocked.Count > 0)
                {
                    List<string> compatible = GetNetworkAvailableNames(HardcoreBrosNotYetUnlocked);
                    if (compatible.Count == 0) return null;
                    int chosen = UnityEngine.Random.Range(0, compatible.Count);
                    string chosenName = compatible[chosen];
                    HardcoreAvailableBros.Add(chosenName);
                    HardcoreBrosNotYetUnlocked.Remove(chosenName);
                    RescuingHardcoreBro = false;

                    if (!BroMakerStorage.GetStoredHeroByName(chosenName, out chosenBro))
                    {
                        chosenBro = null;
                    }
                }
                // No more bros to unlock or not rescuing
                else
                {
                    List<string> compatible = GetNetworkAvailableNames(HardcoreAvailableBros);
                    if (compatible.Count == 0) return null;
                    int chosen = UnityEngine.Random.Range(0, compatible.Count);
                    string chosenName = compatible[chosen];
                    RescuingHardcoreBro = false;

                    if (!BroMakerStorage.GetStoredHeroByName(chosenName, out chosenBro))
                    {
                        chosenBro = null;
                    }
                }
            }
            // If forcing customs, only return allowed bros
            else if (ForceCustomThisLevel)
            {
                List<StoredHero> compatible = GetNetworkAvailableBros(ForcedCustoms);
                if (compatible.Count == 0) return null;
                chosenBro = compatible[UnityEngine.Random.Range(0, compatible.Count)];
            }
            else if (allowPendingUnlocks && BroUnlockManager.HasPendingUnlockedBro())
            {
                string pendingBro = BroUnlockManager.GetAndClearPendingUnlockedBro();
                if (pendingBro != null)
                {
                    LastSpawnWasUnlock = true;
                    if (!BroMakerStorage.GetStoredHeroByName(pendingBro, out chosenBro))
                    {
                        chosenBro = null;
                    }
                }
                else
                {
                    List<StoredHero> compatible = GetNetworkAvailableBros(EnabledBros);
                    if (compatible.Count > 0)
                        chosenBro = compatible[UnityEngine.Random.Range(0, compatible.Count)];
                }
            }
            // Return any enabled bro
            else
            {
                List<StoredHero> compatible = GetNetworkAvailableBros(EnabledBros);
                if (compatible.Count > 0)
                {
                    chosenBro = compatible[UnityEngine.Random.Range(0, compatible.Count)];
                }
            }

            // Check if chosen bro is in pending unlock queue
            if (chosenBro != null && allowPendingUnlocks && !LastSpawnWasUnlock && BroUnlockManager.IsBroPendingUnlock(chosenBro.name))
            {
                BroUnlockManager.ClearPendingUnlock(chosenBro.name);
                LastSpawnWasUnlock = true;
            }

            return chosenBro;
        }

        public static float CalculateSpawnProbability()
        {
            if (BSett.instance.equalSpawnProbability)
            {
                int enabledBroCount = GetNetworkAvailableBros(EnabledBros).Count;
                return (enabledBroCount / (41.0f + enabledBroCount)) * 100.0f;
            }
            else
            {
                return BSett.instance.automaticSpawnProbabilty;
            }
        }

        public static void SetBroEnabled(string broName, bool enabled, bool forced = false)
        {
            if (!forced && enabled && !BroUnlockManager.IsBroUnlocked(broName))
            {
                return;
            }
            bool wasEnabled = EnabledBrosNames.Contains(broName);
            if (wasEnabled != enabled)
            {
                if (enabled && !EnabledBrosNames.Contains(broName))
                {
                    EnabledBrosNames.Add(broName);
                    if (BroMakerStorage.GetStoredHeroByName(broName, out StoredHero bro))
                    {
                        EnabledBros.Add(bro);
                    }
                }
                else
                {
                    if (BroMakerStorage.GetStoredHeroByName(broName, out StoredHero bro))
                    {
                        EnabledBros.Remove(bro);
                    }
                    EnabledBrosNames.Remove(broName);
                }
                BroStatusChanged();
            }
        }

        internal static void AddBroIfEnabled(StoredHero bro)
        {
            if (EnabledBrosNames.Contains(bro.name) && !EnabledBros.Contains(bro))
            {
                EnabledBros.Add(bro);
            }
        }

        public static void RegisterNotifyBroUnlocked(Action<string, StoredHero> callback)
        {
            BroUnlockManager.RegisterNotifyBroUnlocked(callback);
        }
        #endregion

        #region Internal Helpers
        internal static void CreateHardcoreLists(int slot)
        {
            BSett.instance._notUnlockedBros[slot] = new List<string>();
            BSett.instance._availableBros[slot] = new List<string>();
            foreach (StoredHero bro in GetAllBros())
            {
                if (BroUnlockManager.IsBroUnlocked(bro.name) && IsBroEnabled(bro.name))
                {
                    BSett.instance._notUnlockedBros[slot].Add(bro.name);
                }
            }
        }

        internal static void CheckForDeletedBros()
        {
            if (GameModeController.IsHardcoreMode)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (BSett.instance._notUnlockedBros[i] != null)
                    {
                        BSett.instance._notUnlockedBros[i].RemoveAll(name => !BroMakerStorage.GetStoredHeroByName(name, out _));
                    }
                    if (BSett.instance._availableBros[i] != null)
                    {
                        BSett.instance._availableBros[i].RemoveAll(name => !BroMakerStorage.GetStoredHeroByName(name, out _));
                    }
                }
            }
        }

        internal static void BroStatusChanged()
        {
            if (BroStatusCount == int.MaxValue)
            {
                BroStatusCount = int.MinValue;
            }
            ++BroStatusCount;
        }
        #endregion
    }
}

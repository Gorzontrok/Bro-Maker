using System.Collections.Generic;
using System.Linq;
using BroMakerLib.Loggers;
using BroMakerLib.Unlocks;
using RocketLib.CustomTriggers;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib.Storages
{
    public static class BroSpawnManager
    {
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

        internal static bool ForceCustomThisLevel => CustomTriggerStateManager.Get<bool>("forceCustomBros");
        internal static List<StoredHero> ForcedCustoms => CustomTriggerStateManager.Get<List<StoredHero>>("forcedCustomBroList", new List<StoredHero>());

        public static bool RescuingHardcoreBro = false;

        public static bool LastSpawnWasUnlock { get; private set; }
        public static int BroStatusCount = int.MinValue;

        public static List<StoredHero> GetAllBros()
        {
            return BroMakerStorage._bros;
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
                        availableHeroes.Add(hero);
                    }
                }
                return availableHeroes;
            }
            // If forcing customs this level, only return allowed customs
            else if (ForceCustomThisLevel)
            {
                return ForcedCustoms;
            }
            // Otherwise return all enabled bros
            else
            {
                return GetAllEnabledBros();
            }
        }

        public static List<string> GetAllSpawnableBrosNames()
        {
            // If in IronBro, only return available bros
            if (GameModeController.IsHardcoreMode)
            {
                return HardcoreAvailableBros;
            }
            // If forcing customs this level, only return allowed customs
            else if (ForceCustomThisLevel)
            {
                List<string> names = new List<string>();
                foreach (StoredHero hero in ForcedCustoms)
                {
                    names.Add(hero.name);
                }
                return names;
            }
            // Otherwise return all enabled bros
            else
            {
                return EnabledBrosNames;
            }
        }

        public static StoredHero GetRandomSpawnableBro(bool allowPendingUnlocks = false)
        {
            LastSpawnWasUnlock = false;

            StoredHero chosenBro = null;

            if (BSett.instance.overrideNextBroSpawn)
            {
                BSett.instance.overrideNextBroSpawn = false;
                chosenBro = BroMakerStorage.GetStoredHeroByName(BSett.instance.nextBroSpawn);
            }
            // If in ironbro, only return unlocked bros
            else if (GameModeController.IsHardcoreMode)
            {
                // Unlock additional bro if more are left to unlock
                if (RescuingHardcoreBro && HardcoreBrosNotYetUnlocked.Count > 0)
                {
                    int chosen = UnityEngine.Random.Range(0, HardcoreBrosNotYetUnlocked.Count);
                    string chosenName = HardcoreBrosNotYetUnlocked[chosen];
                    HardcoreAvailableBros.Add(HardcoreBrosNotYetUnlocked[chosen]);
                    HardcoreBrosNotYetUnlocked.RemoveAt(chosen);
                    RescuingHardcoreBro = false;

                    chosenBro = BroMakerStorage.GetStoredHeroByName(chosenName);
                }
                // No more bros to unlock or not rescuing
                else
                {
                    int chosen = UnityEngine.Random.Range(0, HardcoreAvailableBros.Count);
                    string chosenName = HardcoreAvailableBros[chosen];
                    RescuingHardcoreBro = false;

                    chosenBro = BroMakerStorage.GetStoredHeroByName(chosenName);
                }
            }
            // If forcing customs, only return allowed bros
            else if (BroSpawnManager.ForceCustomThisLevel)
            {
                chosenBro = ForcedCustoms[UnityEngine.Random.Range(0, BroSpawnManager.ForcedCustoms.Count())];
            }
            else if (allowPendingUnlocks && BroUnlockManager.HasPendingUnlockedBro())
            {
                string pendingBro = BroUnlockManager.GetAndClearPendingUnlockedBro();
                if (pendingBro != null)
                {
                    LastSpawnWasUnlock = true;
                    chosenBro = BroMakerStorage.GetStoredHeroByName(pendingBro);
                }
                else
                {
                    if (EnabledBros.Count > 0)
                    {
                        chosenBro = EnabledBros[UnityEngine.Random.Range(0, EnabledBros.Count)];
                    }
                    else
                    {
                        BMLogger.Error("Trying to spawn with no enabled bros");
                        chosenBro = GetAllBros()[0];
                    }
                }
            }
            // Return any enabled bro
            else
            {
                if (EnabledBros.Count > 0)
                {
                    chosenBro = EnabledBros[UnityEngine.Random.Range(0, EnabledBros.Count)];
                }
                else
                {
                    BMLogger.Error("Trying to spawn with no enabled bros");
                    chosenBro = GetAllBros()[0];
                }
            }

            // Check if chosen bro is in pending unlock queue
            if (chosenBro!= null && allowPendingUnlocks && !LastSpawnWasUnlock && BroUnlockManager.IsBroPendingUnlock(chosenBro.name))
            {
                BroUnlockManager.ClearPendingUnlock(chosenBro.name);
                LastSpawnWasUnlock = true;
            }

            return chosenBro;
        }

        public static bool IsBroSpawnable(string broName)
        {
            if (!BroUnlockManager.IsBroUnlocked(broName))
                return false;

            if (GameModeController.IsHardcoreMode)
            {
                return HardcoreAvailableBros.Contains(broName);
            }
            else if (BroSpawnManager.ForceCustomThisLevel)
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

        public static void AddBroIfEnabled(StoredHero bro)
        {
            if (EnabledBrosNames.Contains(bro.name) && !EnabledBros.Contains(bro))
            {
                EnabledBros.Add(bro);
            }
        }

        public static void AddBroEnabled(StoredHero bro, bool enabled)
        {
            if (enabled)
            {
                EnabledBros.Add(bro);
                EnabledBrosNames.Add(bro.name);
            }
        }

        public static void SetBroEnabled(string broName, bool enabled, bool forced = false)
        {
            // Don't allow enabling locked bros
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
                    EnabledBros.Add(BroMakerStorage.GetStoredHeroByName(broName));
                }
                else
                {
                    EnabledBros.Remove(BroMakerStorage.GetStoredHeroByName(broName));
                    EnabledBrosNames.Remove(broName);
                }
                BroStatusChanged();
            }
        }

        public static bool IsBroEnabled(string broName)
        {
            return EnabledBrosNames.Contains(broName);
        }

        public static void CreateHardcoreLists(int slot)
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

        public static void CheckForDeletedBros()
        {
            List<StoredHero> toBeRemoved = new List<StoredHero>();
            foreach (StoredHero bro in EnabledBros)
            {
                bool found = false;
                for (int i = 0; i < BroMakerStorage.Bros.Length; ++i)
                {
                    if (BroMakerStorage.Bros[i].name == bro.name)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    toBeRemoved.Add(bro);
                }
            }
            foreach (StoredHero remove in toBeRemoved)
            {
                EnabledBros.Remove(remove);
                EnabledBrosNames.Remove(remove.name);
            }

            if (GameModeController.IsHardcoreMode)
            {
                for (int i = 0; i < 4; ++i)
                {
                    if (BSett.instance._notUnlockedBros[i] != null)
                    {
                        BSett.instance._notUnlockedBros[i].RemoveAll(name => BroMakerStorage.GetStoredHeroByName(name) == null);
                    }
                    if (BSett.instance._availableBros[i] != null)
                    {
                        BSett.instance._availableBros[i].RemoveAll(name => BroMakerStorage.GetStoredHeroByName(name) == null);
                    }
                }
            }
        }

        public static float CalculateSpawnProbability()
        {
            if (BSett.instance.equalSpawnProbability)
            {
                int enabledBroCount = EnabledBrosNames.Count();
                return (enabledBroCount / (41.0f + enabledBroCount)) * 100.0f;
            }
            else
            {
                return BSett.instance.automaticSpawnProbabilty;
            }
        }

        public static BroState GetBroState(string broName)
        {
            var bro = BroMakerStorage.GetStoredHeroByName(broName);
            if (bro == null)
                return BroState.NotInstalled;

            if (!BroUnlockManager.IsBroUnlocked(broName))
                return BroState.Locked;

            if (GameModeController.IsHardcoreMode)
            {
                return HardcoreAvailableBros.Contains(broName)
                    ? BroState.Available
                    : BroState.Disabled;
            }

            bool isEnabled = EnabledBrosNames.Contains(broName);

            return isEnabled ? BroState.Available : BroState.Disabled;
        }

        public static void BroStatusChanged()
        {
            if (BroStatusCount == int.MaxValue)
            {
                BroStatusCount = int.MinValue;
            }
            ++BroStatusCount;
        }
    }
}

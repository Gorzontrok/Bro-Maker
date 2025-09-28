using System.Collections.Generic;
using System.Linq;
using BroMakerLib.Unlocks;
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

        // Variables for setting a specific custom bro to continually spawn on a certain level
        public static bool StartForcingCustom = false;
        public static bool ForceCustomThisLevel = false;
        public static List<StoredHero> ForcedCustoms = new List<StoredHero>();

        public static bool RescuingHardcoreBro = false;

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
                List<StoredHero> enabledHeroes = new List<StoredHero>();
                foreach (KeyValuePair<string, bool> bro in BSett.instance.EnabledBros)
                {
                    if (bro.Value && BroMakerStorage.GetStoredHeroByName(bro.Key, out StoredHero hero))
                    {
                        enabledHeroes.Add(hero);
                    }
                }
                return enabledHeroes;
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
                List<string> names = new List<string>();
                foreach (KeyValuePair<string, bool> bro in BSett.instance.EnabledBros)
                {
                    if (bro.Value)
                    {
                        names.Add(bro.Key);
                    }
                }
                return names;
            }
        }

        public static StoredHero GetRandomSpawnableBro()
        {
            if (BSett.instance.overrideNextBroSpawn)
            {
                BSett.instance.overrideNextBroSpawn = false;
                return BroMakerStorage.GetStoredHeroByName(BSett.instance.nextBroSpawn);
            }

            string chosenName = "";
            // If in ironbro, only return unlocked bros
            if (GameModeController.IsHardcoreMode)
            {
                // Unlock additional bro if more are left to unlock
                if (RescuingHardcoreBro && HardcoreBrosNotYetUnlocked.Count > 0)
                {
                    int chosen = UnityEngine.Random.Range(0, HardcoreBrosNotYetUnlocked.Count);
                    HardcoreAvailableBros.Add(HardcoreBrosNotYetUnlocked[chosen]);
                    HardcoreBrosNotYetUnlocked.RemoveAt(chosen);
                    RescuingHardcoreBro = false;

                    return BroMakerStorage.GetStoredHeroByName(chosenName);
                }
                // No more bros to unlock or not rescuing
                else
                {
                    int chosen = UnityEngine.Random.Range(0, HardcoreAvailableBros.Count);
                    chosenName = HardcoreAvailableBros[chosen];
                    RescuingHardcoreBro = false;

                    return BroMakerStorage.GetStoredHeroByName(chosenName);
                }
            }
            // If forcing customs, only return allowed bros
            else if (BroSpawnManager.ForceCustomThisLevel)
            {
                return ForcedCustoms[UnityEngine.Random.Range(0, BroSpawnManager.ForcedCustoms.Count())];
            }
            // Return any enabled bro
            else
            {
                int chosen = UnityEngine.Random.Range(0, BSett.instance.enabledBroCount);

                foreach (KeyValuePair<string, bool> bro in BSett.instance.EnabledBros)
                {
                    if (bro.Value)
                    {
                        if (chosen == 0)
                        {
                            chosenName = bro.Key;
                            break;
                        }
                        --chosen;
                    }
                }
            }

            return BroMakerStorage.GetStoredHeroByName(chosenName);
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

            return BSett.instance.EnabledBros.ContainsKey(broName) && BSett.instance.EnabledBros[broName];
        }

        public static void AddBroEnabled(string name, bool enabled)
        {
            if (!BSett.instance.EnabledBros.ContainsKey(name))
            {
                BSett.instance.EnabledBros.Add(name, enabled);
                if (enabled)
                {
                    ++BSett.instance.enabledBroCount;
                }
            }
        }

        public static void SetBroEnabled(string broName, bool enabled, bool forced = false)
        {
            if (BSett.instance.EnabledBros.ContainsKey(broName))
            {
                // Don't allow enabling locked bros
                if (!forced && enabled && !BroUnlockManager.IsBroUnlocked(broName))
                {
                    return;
                }
                bool wasEnabled = BSett.instance.EnabledBros[broName];
                BSett.instance.EnabledBros[broName] = enabled;

                if (wasEnabled != enabled)
                {
                    if (enabled)
                    {
                        ++BSett.instance.enabledBroCount;
                    }
                    else
                    {
                        --BSett.instance.enabledBroCount;
                    }
                }
            }
        }

        public static bool IsBroEnabled(string name)
        {
            return BSett.instance.EnabledBros.ContainsKey(name) && BSett.instance.EnabledBros[name];
        }

        public static void CheckForDeletedBros()
        {
            List<string> toBeRemoved = new List<string>();
            foreach (KeyValuePair<string, bool> bro in BSett.instance.EnabledBros)
            {
                bool found = false;
                for (int i = 0; i < BroMakerStorage.Bros.Length; ++i)
                {
                    if (BroMakerStorage.Bros[i].name == bro.Key)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    toBeRemoved.Add(bro.Key);
                }
            }
            foreach (string remove in toBeRemoved)
            {
                BSett.instance.EnabledBros.Remove(remove);
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

        public static void CountEnabledBros()
        {
            int count = 0;
            foreach (var kvp in BSett.instance.EnabledBros)
            {
                if (kvp.Value)
                {
                    ++count;
                }
            }
            BSett.instance.enabledBroCount = count;
        }

        public static float CalculateSpawnProbability()
        {
            int enabledBroCount = 0;
            foreach (var kvp in BSett.instance.EnabledBros)
            {
                if (kvp.Value)
                {
                    ++enabledBroCount;
                }
            }
            return (enabledBroCount / (41.0f + enabledBroCount)) * 100.0f;
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

            bool isEnabled = BSett.instance.EnabledBros.ContainsKey(broName) &&
                           BSett.instance.EnabledBros[broName];

            return isEnabled ? BroState.Available : BroState.Disabled;
        }
    }
}

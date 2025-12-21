using System;
using BroMakerLib.Loggers;
using HarmonyLib;

namespace BroMakerLib.Unlocks
{
    [HarmonyPatch(typeof(PlayerProgress), "FreeBro")]
    public static class PlayerProgress_FreeBro_Patch
    {
        public static void Prefix()
        {
            try
            {
                HeroUnlockController_GetNumberOfRescuesToNextUnlock_Patch.IgnoreNextCall = true;
                // If a vanilla bro and custom bro are about to be unlocked at the same time, skip unlocking the custom bro
                if (!(HeroUnlockController.GetNumberOfRescuesToNextUnlock() == 1 && BroUnlockManager.GetRemainingRescuesToUnlockNextBro() == 1))
                {
                    BroUnlockManager.RescuedBro();
                    BroUnlockManager.CheckRescueUnlocks();
                }
                HeroUnlockController_GetNumberOfRescuesToNextUnlock_Patch.IgnoreNextCall = false;
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Error in FreeBro patch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(HeroUnlockController), "GetNumberOfRescuesToNextUnlock")]
    static class HeroUnlockController_GetNumberOfRescuesToNextUnlock_Patch
    {
        internal static bool IgnoreNextCall = false;
        public static void Postfix(ref int __result)
        {
            if (!Main.enabled || IgnoreNextCall)
            {
                return;
            }

            int customBroRemaining = BroUnlockManager.GetRemainingRescuesToUnlockNextBro();
            if (__result == -1 || __result > customBroRemaining)
            {
                __result = customBroRemaining;
            }
        }
    }
}

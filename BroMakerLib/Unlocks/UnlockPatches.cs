using System;
using BroMakerLib.Loggers;
using HarmonyLib;

namespace BroMakerLib.Unlocks
{
    [HarmonyPatch(typeof(PlayerProgress), "FreeBro")]
    public static class PlayerProgress_FreeBro_Patch
    {
        public static void Postfix()
        {
            try
            {
                int currentRescueCount = PlayerProgress.Instance.freedBros;
                BroUnlockManager.CheckRescueUnlocks(currentRescueCount);
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Error in FreeBro patch: {ex.Message}");
            }
        }
    }
}

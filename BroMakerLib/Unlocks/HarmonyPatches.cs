using System;
using BroMakerLib.Loggers;
using HarmonyLib;

namespace BroMakerLib.Unlocks
{
    [HarmonyPatch(typeof(GameModeController), "LevelFinish")]
    public static class GameModeController_LevelFinish_Patch
    {
        [HarmonyPrefix]
        static void Prefix(LevelResult result)
        {
            try
            {
                if (result == LevelResult.Success)
                {
                    string currentLevelName = null;

                    if (LevelSelectionController.currentCampaign != null)
                    {
                        currentLevelName = LevelSelectionController.currentCampaign.name;
                    }

                    if (!string.IsNullOrEmpty(currentLevelName))
                    {
                        BMLogger.Debug($"Level completed successfully: {currentLevelName}");

                        BroUnlockManager.CheckLevelUnlocks(currentLevelName);
                    }
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Error in LevelFinish patch: {ex.Message}");
            }
        }
    }

    [HarmonyPatch(typeof(PlayerProgress), "FreeBro")]
    public static class PlayerProgress_FreeBro_Patch
    {
        [HarmonyPostfix]
        static void Postfix()
        {
            try
            {
                int currentRescueCount = PlayerProgress.Instance.freedBros;
                BMLogger.Debug($"Rescue count increased to: {currentRescueCount}");

                BroUnlockManager.CheckRescueUnlocks(currentRescueCount);
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Error in FreeBro patch: {ex.Message}");
            }
        }
    }
}
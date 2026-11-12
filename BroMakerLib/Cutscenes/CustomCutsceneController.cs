using System;

namespace BroMakerLib.Cutscenes
{
    public static class CustomCutsceneController
    {
        public static bool willLoadCustomCutscene = false;
        public static bool finishCampaignAfterCutscene = false;
        public static CustomIntroCutscene cutsceneToLoad;

        public static void LoadHeroCutscene(CustomIntroCutscene cutscene, float delay = 0.2f, bool setFinishCampaignAfterCutscene = false)
        {
            if (cutscene == null)
            {
                throw new ArgumentNullException(nameof(cutscene));
            }
            // The game's cutscene start ends a frozen pause only from the top pause menu; otherwise the cutscene's own pause drops the frozen level for good
            bool canEndPause = PauseController.pauseStatus == PauseStatus.MenuPause && (PauseMenu.instance == null || PauseMenu.instance.MenuActive);
            if (BroMakerUtilities.IsLevelFrozenByPause() && !canEndPause)
            {
                return;
            }
            cutsceneToLoad = cutscene;
            willLoadCustomCutscene = true;
            if (setFinishCampaignAfterCutscene)
            {
                finishCampaignAfterCutscene = true;
            }
            CutsceneController.LoadCutScene(CutsceneName.Rambro, delay);
        }
    }
}

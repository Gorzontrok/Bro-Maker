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

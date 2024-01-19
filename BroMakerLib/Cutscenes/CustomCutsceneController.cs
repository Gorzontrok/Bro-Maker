using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Cutscenes
{
    public static class CustomCutsceneController
    {
        public static bool willLoadCustomCutscene = false;
        public static CustomIntroCutscene cutsceneToLoad;

        public static void LoadHeroCutscene(CustomIntroCutscene cutscene, float delay = 0.2f)
        {
            if (cutscene == null)
            {
                throw new ArgumentNullException(nameof(cutscene));
            }
            cutsceneToLoad = cutscene;
            willLoadCustomCutscene = true;
            CutsceneController.LoadCutScene(CutsceneName.Rambro, delay);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Cutscenes
{
    public static class CustomCustsceneController
    {
        public static bool willLoadCustomCutscene = false;
        public static CustomIntroCutscene cutsceneToLoad;

        public static void LoadHeroCutscene(CustomIntroCutscene cutscene, float delay = 0.2f)
        {
            cutsceneToLoad = cutscene;
            willLoadCustomCutscene = true;
            CutsceneController.LoadCutScene(CutsceneName.Rambro, delay);
        }
    }
}
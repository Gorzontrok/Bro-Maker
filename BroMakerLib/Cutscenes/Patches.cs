using BroMakerLib.Loggers;
using HarmonyLib;
using RocketLib;

namespace BroMakerLib.Cutscenes
{
     [HarmonyPatch(typeof(CutsceneIntroRoot), "OnLoadComplete", typeof(string), typeof(object))]
     static class CutsceneIntroRoot_StartCutscene_Patch
     {
         static void Prefix(CutsceneIntroRoot __instance, ref string resourceName, ref object asset)
         {
             if (CustomCustsceneController.willLoadCustomCutscene)
             {
                 asset = CustomCustsceneController.cutsceneToLoad.ToCutsceneIntroData(__instance);
             }
         }
     }
}

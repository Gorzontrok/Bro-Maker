using HarmonyLib;
using System;
using System.Linq;
using BroMakerLib.Storages;
using BroMakerLib.Loggers;
using UnityEngine;
using BroMakerLib.Loaders;
using BSett = BroMakerLib.Settings;

namespace BroMakerLib.UnityMod.HarmonyPatches
{
    // Collect Logs
    [HarmonyPatch(typeof(BMLogger), "Log", new Type[] {typeof(string), typeof(LogType), typeof(bool)})]
    static class BMLogger_Log_Patch
    {
        static void Postfix()
        {
            Main.Log(BMLogger.logs.Last(), Log.PREFIX);
        }
    }

    [HarmonyPatch(typeof(BMLogger), "Debug")]
    static class BMLogger_Debug_Patch
    {
        static void Postfix()
        {
            if(Main.settings.debugLogs)
                Main.Log(BMLogger.debugLogs.Last(), Log.PREFIX);
        }
    }

    // Automatic spawn
    [HarmonyPatch(typeof(Player), "SpawnHero")]
    static class Player_InstantiateHero_Patch
    {
        static void Postfix(Player __instance)
        {
            try
            {
                if (__instance.rescuingThisBro != null && BSett.instance.automaticSpawn && UnityEngine.Random.value < (BSett.instance.automaticSpawnProbabilty / 100))
                {
                    var choice = MakerObjectStorage.Bros.RandomElement();
                    LoadHero.spawnFromPlayer = true;
                    choice.LoadBro(__instance.playerNum);
                }
            }
            catch(Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }
    }
}

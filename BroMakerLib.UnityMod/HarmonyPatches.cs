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
            if ( Main.enabled )
            {
                Main.Log(BMLogger.logs.Last(), Log.PREFIX);
            }
            
        }
    }

    [HarmonyPatch(typeof(BMLogger), "Debug")]
    static class BMLogger_Debug_Patch
    {
        static void Postfix()
        {
            if( Main.enabled && Main.settings.debugLogs)
                Main.Log(BMLogger.debugLogs.Last(), Log.PREFIX);
        }
    }

    // Automatic spawn
    [HarmonyPatch(typeof(Player), "SpawnHero")]
    static class Player_InstantiateHero_Patch
    {
        static void Prefix(Player __instance)
        {
            if ( Main.enabled && BSett.instance.automaticSpawn && BSett.instance.enabledBroCount > 0 )
            {
                LoadHero.willReplaceBro[__instance.playerNum] =  UnityEngine.Random.value < (BSett.instance.automaticSpawnProbabilty / 100);
            } 
        }
        static void Postfix(Player __instance)
        {
            try
            {
                if ( Main.enabled && LoadHero.willReplaceBro[__instance.playerNum] )
                {
                    LoadHero.willReplaceBro[__instance.playerNum] = false;
                    LoadHero.spawningCustomBro[__instance.playerNum] = true;
                    LoadHero.anyCustomSpawning = true;
                    var choice = BSett.instance.getRandomEnabledBro();
                    LoadHero.spawnFromPlayer = (__instance.rescuingThisBro != null);
                    choice.LoadBro(__instance.playerNum);
                    __instance.changingBroFromTrigger = false;
                    LoadHero.spawningCustomBro[__instance.playerNum] = false;
                    LoadHero.anyCustomSpawning = false;
                    LoadHero.broBeingRescued = false;
                }
            }
            catch(Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }
    }

    [HarmonyPatch(typeof(Map), "AddBroToHeroTransport")]
    static class Map_AddBroToHeroTransport_Patch
    {
        static bool Prefix(Map __instance, ref TestVanDammeAnim Bro)
        {
            // If mod is disabled or if we aren't loading a custom character don't disable
            return !Main.enabled || !LoadHero.willReplaceBro[Bro.playerNum];
        }
    }

    [HarmonyPatch(typeof(EffectsController), "CreateHeroIndicator")]
    static class EffectsController_CreateHeroIndicator_Patch
    {
        static bool Prefix(ref Unit unit)
        {
            if ( !Main.enabled )
            {
                return true;
            }
            // Check Unit's pos because sometimes this function is called with a unit that has not had its position set yet
            else if ( LoadHero.anyCustomSpawning && (!LoadHero.broBeingRescued || (unit.X <= 0 && unit.Y <= 0) ) )
            {      
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Player), "WorkOutSpawnPosition")]
    static class Player_WorkOutSpawnPosition_Patch
    {
        static bool Prefix(Player __instance, ref TestVanDammeAnim bro)
        {
            if (!Main.enabled || !LoadHero.spawningCustomBro[__instance.playerNum] )
            {
                return true;
            }
            else if ( LoadHero.previousSpawnInfo[__instance.playerNum] == Player.SpawnType.AddBroToTransport )
            {
                // Need to manually call AddBroToHeroTransport, because WorkoutSpawnPosition won't do it for custom characters for some reason
                Map.AddBroToHeroTransport(bro);

                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(Player), "WorkOutSpawnScenario")]
    static class Player_WorkOutSpawnScenario_Patch
    {
        static void Postfix(Player __instance, ref Player.SpawnType __result)
        {
            if ( !Main.enabled )
            {
                return;
            }
            // Store spawning info of normal character so we can pass it on to the custom character
            else if ( LoadHero.willReplaceBro[__instance.playerNum] )
            {
                LoadHero.previousSpawnInfo[__instance.playerNum] = __result;
                LoadHero.broBeingRescued = __result == Player.SpawnType.RespawnAtRescueBro;
            }
            // Replace custom characters spawning info with stored info
            else if ( LoadHero.spawningCustomBro[__instance.playerNum] )
            {
                __result = LoadHero.previousSpawnInfo[__instance.playerNum];
            }
        }
    }
}

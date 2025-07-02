using BroMakerLib.CustomObjects;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.CustomObjects.Projectiles;
using BroMakerLib.Cutscenes;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
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
    static class Player_SpawnHero_Patch
    {
        static void Prefix(Player __instance, ref HeroType nextHeroType)
        {
            if (Main.enabled)
            {
                CustomPockettedSpecial.ClearPockettedSpecials( __instance.playerNum );
                if ( BSett.instance.overrideNextBroSpawn )
                {
                    LoadHero.willReplaceBro[__instance.playerNum] = true;
                    nextHeroType = HeroType.Rambro;
                    return;
                }
                else if ( BSett.instance.disableSpawning )
                {
                    LoadHero.willReplaceBro[__instance.playerNum] = false;
                    return;
                }
                if ( GameModeController.IsHardcoreMode && BSett.instance.enabledBroCount > 0 )
                {
                    // Check if we're unlocking a bro or just normally spawning one
                    if ( PlayerProgress.Instance.yetToBePlayedUnlockedHeroes.Count() > 0 )
                    {
                        if (BSett.instance.onlyCustomInHardcore)
                        {
                            LoadHero.willReplaceBro[__instance.playerNum] = true;

                            PlayerProgress.Instance.yetToBePlayedUnlockedHeroes.Remove(nextHeroType);
                            GameState.Instance.currentWorldmapSave.hardcoreModeAvailableBros.Remove(nextHeroType);
                            if ( BSett.instance.NotUnlockedBros.Count() > 0 )
                            {
                                LoadHero.playCutscene = true;
                            }
                        }
                        else
                        {
                            // Probability of a custom bro being unlocked should be the number of custom bros / number of custom bros + notUnlocked or dead normal bros
                            // We add 1 because the chosen bro will have already been added to the available bros
                            LoadHero.willReplaceBro[__instance.playerNum] = UnityEngine.Random.value <= (BSett.instance.NotUnlockedBros.Count() /
                                (BSett.instance.NotUnlockedBros.Count() + PlayerProgress.Instance.unlockedHeroes.Count() -
                                GameState.Instance.currentWorldmapSave.hardcoreModeAvailableBros.Count() - GameState.Instance.currentWorldmapSave.hardcoreModeDeadBros.Count() + 1.0f));

                            // If replacing hero, remove previously unlocked one from available bros
                            if (LoadHero.willReplaceBro[__instance.playerNum])
                            {
                                PlayerProgress.Instance.yetToBePlayedUnlockedHeroes.Remove(nextHeroType);
                                GameState.Instance.currentWorldmapSave.hardcoreModeAvailableBros.Remove(nextHeroType);
                                LoadHero.playCutscene = true;
                            }
                        }
                    }
                    else if (BSett.instance.onlyCustomInHardcore)
                    {
                        // Check if this is the first character being spawned
                        if (BSett.instance.AvailableBros.Count() == 0)
                        {
                            // We use this function to add a character to the pool to start with
                            BSett.instance.GetRandomHardcoreBro(true);
                        }
                        LoadHero.willReplaceBro[__instance.playerNum] = true;
                    }
                    else if (BSett.instance.AvailableBros.Count() > 0)
                    {
                        LoadHero.willReplaceBro[__instance.playerNum] = UnityEngine.Random.value <= (BSett.instance.AvailableBros.Count() /
                        ((float)BSett.instance.AvailableBros.Count() + GameState.Instance.currentWorldmapSave.hardcoreModeAvailableBros.Count()));
                    }
                }
                else if (BSett.instance.automaticSpawn && BSett.instance.enabledBroCount > 0)
                {
                    LoadHero.willReplaceBro[__instance.playerNum] = UnityEngine.Random.value <= (BSett.instance.automaticSpawnProbabilty / 100.0f);
                }
                if ( LoadHero.willReplaceBro[__instance.playerNum] )
                {
                    // Ensure player doesn't spawn as boondock bros
                    nextHeroType = HeroType.Rambro;
                }
                LoadHero.playerNum = __instance.playerNum;
            }

        }
        static void Postfix(Player __instance)
        {
            try
            {
                if (Main.enabled && LoadHero.willReplaceBro[__instance.playerNum])
                {
                    LoadHero.willReplaceBro[__instance.playerNum] = false;
                    LoadHero.spawningCustomBro[__instance.playerNum] = true;
                    LoadHero.anyCustomSpawning = true;
                    Storages.StoredHero choice;
                    if ( GameModeController.IsHardcoreMode )
                    {
                        choice = BSett.instance.GetRandomHardcoreBro(LoadHero.playCutscene);
                    }
                    else
                    {
                        choice = BSett.instance.GetRandomEnabledBro();
                        if ( LoadHero.playCutscene = !BSett.instance.seenBros.Contains(choice.name) && choice.GetInfo().Cutscene.Count > 0 && choice.GetInfo().Cutscene[0].playCutsceneOnFirstSpawn )
                        {
                            BSett.instance.seenBros.Add(choice.name);
                        }
                    }
                    LoadHero.spawnFromPlayer = (__instance.rescuingThisBro != null);

                    TestVanDammeAnim bro = choice.LoadBro(__instance.playerNum);

                    if ( LoadHero.playCutscene )
                    {
                        Cutscenes.CustomCutsceneController.LoadHeroCutscene( BroMakerUtilities.GetVariantValue( choice.GetInfo().Cutscene, (bro as CustomHero).CurrentVariant ) );
                        LoadHero.playCutscene = false;
                    }
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

    // Fix vanilla bro being visible for 1 frame in certain spawning situations
    [HarmonyPatch(typeof(Player), "InstantiateHero")]
    static class Player_InstantiateHero_Patch
    {
        static void Postfix(Player __instance, ref TestVanDammeAnim __result)
        {
            // If mod is disabled or if we aren't loading a custom character don't disable
            if (!Main.enabled || !LoadHero.willReplaceBro[__instance.playerNum])
            {
                return;
            }

            __result.gameObject.SetActive(false);
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
        static void Prefix(Player __instance, ref TestVanDammeAnim bro)
        {
            if ( !Main.enabled )
            {
                return;
            }

            if ( !LoadHero.spawningCustomBro[__instance.playerNum] && LoadHero.willReplaceBro[__instance.playerNum])
            {
                LoadHero.wasFirstDeployment[__instance.playerNum] = __instance.firstDeployment;
            }
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

    [HarmonyPatch(typeof(Player), "SetSpawnPositon")]
    static class Player_SetSpawnPositon_Patch
    {
        public static void Prefix(Player __instance, ref TestVanDammeAnim bro)
        {
            if (!Main.enabled || !LoadHero.willReplaceBro[__instance.playerNum] || LoadHero.spawningCustomBro[__instance.playerNum] )
            {
                return;
            }

            // Prevent yeah sound from playing for replaced bro
            if ( bro != null )
            {
                __instance.firstDeployment = true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerHUD), "SetGrenadeMaterials", new Type[] { typeof(HeroType) })]
    static class PlayerHUD_SetGrenadeMaterials_Patch
    {
        public static bool Prefix(PlayerHUD __instance, ref HeroType type)
        {
            if ( !Main.enabled )
            {
                return true;
            }

            int playerNum = Convert.ToInt32(Traverse.Create(__instance).Field("playerNum").GetValue());
            TestVanDammeAnim currentCharacter = HeroController.players[playerNum].character;
            if (currentCharacter is ICustomHero)
            {
                ICustomHero customHero = currentCharacter as ICustomHero;
                var materials = BroMakerUtilities.GetVariantValue(customHero.info.SpecialMaterials, customHero.CurrentVariant);
                
                if (materials != null && materials.Count > 0)
                {
                    var offset = BroMakerUtilities.GetVariantValue(customHero.info.SpecialMaterialOffset, customHero.CurrentVariant);
                    var spacing = BroMakerUtilities.GetVariantValue(customHero.info.SpecialMaterialSpacing, customHero.CurrentVariant);
                    
                    BroMakerUtilities.SetSpecialMaterials(playerNum, materials, offset, spacing);
                    return false;
                }
            }
            else
            {
                for (int i = 0; i < __instance.grenadeIcons.Length; i++)
                {
                    __instance.grenadeIcons[i].SetOffset(new Vector3(0f, 0f, 0f));
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(CutsceneIntroRoot), "OnLoadComplete")]
    static class CutsceneIntroRoot_OnLoadComplete_Patch
    {
        public static void Prefix(CutsceneIntroRoot __instance, ref string resourceName, ref object asset)
        {
            if ( !Main.enabled || !CustomCutsceneController.willLoadCustomCutscene )
            {
                return;
            }

            Traverse.Create(__instance).Field("_curIntroResourceName").SetValue(string.Format("{0}:{1}", "cutscenes", "Intro_Bro_Rambro"));

            CutsceneIntroData data = CustomCutsceneController.cutsceneToLoad.ToCutsceneIntroData(__instance);

            if ( CustomCutsceneController.cutsceneToLoad.fanfarePath.IsNotNullOrEmpty() )
            {
                __instance.fanfareSource = __instance.gameObject.AddComponent<AudioSource>();
            }

            asset = data;
        }

        public static void Postfix(CutsceneIntroRoot __instance)
        {
            if (!Main.enabled || !CustomCutsceneController.willLoadCustomCutscene)
            {
                return;
            }

            if ( CustomCutsceneController.cutsceneToLoad.fanfarePath.IsNotNullOrEmpty() )
            {
                __instance.fanfareSource.Play();
            }

            if ( !CustomCutsceneController.cutsceneToLoad.playDefaultFanfare )
            {
                AudioSource[] allSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();
                for (int i = 0; i < allSources.Length; ++i)
                {
                    if (allSources[i].isPlaying && allSources[i].name == "camShake")
                    {
                        // For whatever reason the default bro fanfare isn't passed to the fanfare source to be played,
                        // instead it is played by an AudioSource called camShake.
                        allSources[i].Pause();
                    }
                }
            }

            CustomCutsceneController.willLoadCustomCutscene = false;
        }
    }

    [HarmonyPatch(typeof(HeroController), "GetHeroType")]
    static class HeroController_GetHeroType_Patch
    {
        public static bool Prefix(HeroController __instance, ref HeroType __result )
        {
            if ( !Main.enabled )
            {
                return true;
            }

            // If there are no available vanilla bros but still more custom bros, make sure the herotype is set to rambro so the game still tries to spawn the player in
            if (GameModeController.IsHardcoreMode && GameState.Instance.currentWorldmapSave != null && GameState.Instance.currentWorldmapSave.hardcoreModeAvailableBros.Count() == 0 && BSett.instance.AvailableBros.Count() > 0 )
            {
                __result = HeroType.Rambro;
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch(typeof(SaveSlotsMenu), "SelectSlot")]
    static class SaveSlotsMenu_SelectSlot_Patch
    {
        public static void Prefix(SaveSlotsMenu __instance, ref int slot)
        {
            if ( !Main.enabled )
            {
                return;
            }

            if ( SaveSlotsMenu.createNewGame )
            {
                try
                {
                    // If a new save is being created, remake IronBro lists
                    BSett.instance._notUnlockedBros[slot] = new List<string>();
                    BSett.instance._availableBros[slot] = new List<string>();
                    foreach (KeyValuePair<string, bool> bro in BSett.instance.enabledBros)
                    {
                        if (bro.Value)
                        {
                            BSett.instance._notUnlockedBros[slot].Add(bro.Key);
                        }
                    }
                }
                catch(Exception e)
                {
                    BMLogger.ExceptionLog(e);
                }
            }
        }
    }

    // Ensure we can't gain more lives than we should be able to in IronBro, to prevent the game from softlocking
    [HarmonyPatch(typeof(Player), "AddLife")]
    static class Player_AddLife_Patch
    {
        public static bool Prefix(Player __instance)
        {
            if ( !Main.enabled )
            {
                return true;
            }

            if (GameModeController.IsHardcoreMode)
            {
                if ( BSett.instance.onlyCustomInHardcore && BSett.instance.NotUnlockedBros.Count() == 0 )
                {
                    return false;
                }
                else if ( BSett.instance.NotUnlockedBros.Count() +
                    GameState.Instance.currentWorldmapSave.hardcoreModeAvailableBros.Count() + PlayerProgress.Instance.yetToBePlayedUnlockedHeroes.Count() == 0 )
                {
                    return false;
                }
            }
            return true;
        }
    }

    // Ensure we don't remove other characters if we die with a custom character in IronBro
    [HarmonyPatch(typeof(Player), "RemoveLife")]
    static class Player_RemoveLife_Patch
    {
        public static bool Prefix(Player __instance)
        {
            if ( !Main.enabled )
            {
                return true;
            }

            if ( GameModeController.IsHardcoreMode && __instance.character is ICustomHero )
            {
                ICustomHero customHero = (__instance.character as ICustomHero);
                string broName = customHero.info.name;
                BSett.instance.AvailableBros.Remove(broName);
                __instance.Lives--;
                return false;
            }
            return true;
        }
    }

    // Count custom bros for this function so that cages still spawn on the map
    [HarmonyPatch(typeof(HeroUnlockController), "AreAnyMoreBrosAvailableToBeSavedInHardcoreMode")]
    static class HeroUnlockController_AreAnyMoreBrosAvailableToBeSavedInHardcoreMode_Patch
    {
        public static void Postfix(HeroUnlockController __instance, ref bool __result)
        {
            if ( !Main.enabled )
            {
                return;
            }

            if ( BSett.instance.onlyCustomInHardcore )
            {
                __result = BSett.instance.NotUnlockedBros.Count() > 0;
            }
            else
            {
                __result = __result || BSett.instance.NotUnlockedBros.Count() > 0;
            }
        }
    }

    // Fix being unable to pilot mech
    [HarmonyPatch(typeof(Unit), "PilotUnit")]
    static class Unit_PilotUnit_Patch
    {
        public static bool Prefix(Unit __instance, ref Unit pilotUnit)
        {
            if (!Main.enabled)
            {
                return true;
            }

            // Check CanPilotUnit to ensure custom bros can't pilot things they're not supposed to be able to
            if ( pilotUnit is ICustomHero && __instance.CanPilotUnit(pilotUnit.playerNum) )
            {
                __instance.PilotUnitRPC(pilotUnit);
                return false;
            }

            return true;
        }
    }

    // Fix being unable to enter worm tunnel
    [HarmonyPatch(typeof(AssMouthOrifice), "TryConsumeObject")]
    static class AssMouthOrifice_TryConsumeObject_Patch
    {
        public static int customObjectIsConsumed = 0;
        public static void Postfix(AssMouthOrifice __instance, ref BroforceObject obj, ref bool __result )
        {
            if (!Main.enabled)
            {
                return;
            }

            if ( __result && ( obj is ICustomHero || obj is ICustomProjectile ) )
            {
                ++customObjectIsConsumed;
                Traverse assTraverse = Traverse.Create(__instance);
                assTraverse.SetFieldValue("playSwallowAnim", true);
                assTraverse.SetFieldValue("swallowFrameTimer", 0f);
                assTraverse.SetFieldValue("swallowFrame", 0);
                AssMouthTransportWrapper assMouthTransportWrapper = UnityEngine.Object.Instantiate<AssMouthTransportWrapper>(__instance.wrapperPrefab);
                assMouthTransportWrapper.Setup(obj, __instance.root);
                (assTraverse.GetFieldValue("consumedThisFrame") as List<BroforceObject>).Add(obj);
                __instance.open = true;
                if (__instance.enterSound)
                {
                    Sound.GetInstance().PlaySoundEffect(__instance.enterSound, 0.7f);
                }
            }
            
        }
    }

    // Fix being unable to exit worm tunnel
    [HarmonyPatch(typeof(AssMouthTransportWrapper), "RunAssMouthMovement")]
    static class AssMouthTransportWrapper_RunAssMouthMovement_Patch
    {
        public static void Postfix(AssMouthTransportWrapper __instance)
        {
            if (!Main.enabled)
            {
                return;
            }

            if ( AssMouthOrifice_TryConsumeObject_Patch.customObjectIsConsumed > 0 )
            {
                Traverse assTraverse = Traverse.Create(__instance);
                object obj = assTraverse.GetFieldValue( "transportedObject" );
                if ( assTraverse.GetFieldValue("CurrentAssMouthBlock") == null && ( obj is ICustomHero || obj is ICustomProjectile ) )
                {
                    __instance.ExitAssMouth((assTraverse.GetFieldValue("PrevAssMouthBlock") as AssMouthBlock).orificeInstance);
                    --AssMouthOrifice_TryConsumeObject_Patch.customObjectIsConsumed;
                }
            }
        }
    }

    // Fix being unable to be impaled by spikes
    [HarmonyPatch(typeof(Impaler), "ImpaleUnit")]
    static class Impaler_ImpaleUnit_Patch
    {
        public static bool Prefix(Impaler __instance, ref TestVanDammeAnim unit)
        {
            if (!Main.enabled)
            {
                return true;
            }

            if ( unit.impaledByTransform == null && unit is ICustomHero )
            {
                __instance.ImpaleUnitRPC(unit);
                return false;
            }

            return true;
        }
    }

    // Fix being unable to pickup items
    [HarmonyPatch(typeof(PickupableController), "UsePickupables")]
    static class PickupableController_UsePickupables_Patch
    {
        public static bool Prefix(ref TestVanDammeAnim self, ref float range, ref float x, ref float y, ref bool onlyAmmo )
        {
            if (!Main.enabled)
            {
                return true;
            }

            if ( self is ICustomHero )
            {
                List<Pickupable> pickupables = Traverse.Create(typeof(PickupableController)).GetFieldValue("pickupables") as List<Pickupable>;
                for (int i = pickupables.Count - 1; i >= 0; i--)
                {
                    Pickupable pickupable = pickupables[i];
                    if (pickupable != null && (pickupable.pickupType == PickupType.Ammo || !onlyAmmo))
                    {
                        float f = pickupable.X - x;
                        if (Mathf.Abs(f) - range < pickupable.collectionRadius)
                        {
                            float f2 = pickupable.Y + pickupable.yOffset - y;
                            if (Mathf.Abs(f2) - range < pickupable.collectionRadius && pickupable.pickupDelay <= 0f && !pickupable.collected)
                            {
                                if (pickupable.pickupType == PickupType.FlexAirJump || pickupable.pickupType == PickupType.FlexGoldenLight || pickupable.pickupType == PickupType.FlexInvulnerability ||
                                    pickupable.pickupType == PickupType.FlexTeleport || pickupable.pickupType == PickupType.FlexAlluring )
                                {
                                    pickupable.AddFlexPowerRPC(self, pickupable.pickupType, false);
                                }
                                else
                                {
                                    pickupable.Collect(self);
                                }
                            }
                        }
                    }
                }
                return false;
            }

            return true;
        }
    }

    // Fix default character avatar's being overwritten by custom ones
    [HarmonyPatch(typeof(HeroController), "SwitchAvatarMaterial")]
    static class HeroController_SwitchAvatarMaterial_Patch
    {
        public static bool Prefix(ref SpriteSM sprite, ref bool __result)
        {
            if (!Main.enabled)
            {
                return true;
            }

            if ( LoadHero.tryReplaceAvatar && HeroController.players[LoadHero.playerNum].character is ICustomHero )
            {
                LoadHero.tryReplaceAvatar = false;

                ICustomHero customHero = HeroController.players[LoadHero.playerNum].character as ICustomHero;
                Material mat = BroMakerUtilities.GetVariantValue(customHero.info.FirstAvatar, customHero.CurrentVariant);
                    
                if (mat != null)
                {
                    sprite.GetComponent<Renderer>().material = mat;

                    __result = false;
                    return false;
                }
            }

            return true;
        }
    }

    // Fix custom bros having wrong sprite show up on death screen
    [HarmonyPatch(typeof(GameModeController), "Start")]
    static class GameModeController_Start_Patch
    {
        public static void Prefix()
        {
            if (!Main.enabled)
            {
                return;
            }

            LoadHero.customBroDeaths = new Dictionary<int, CustomBroDeath>();

            return;
        }
    }

    // Fix custom bros having wrong sprite show up on death screen
    [HarmonyPatch(typeof(StatisticsController), "NotifyMookDeathType", new Type[] { typeof(TestVanDammeAnim), typeof(DeathType) })]
    static class StatisticsController_NotifyMookDeathType_Patch
    {
        public static void Prefix(ref TestVanDammeAnim vanDamme, ref DeathType deathType )
        {
            if (!Main.enabled || GameModeController.LevelFinished)
            {
                return;
            }

            if (vanDamme is ICustomHero && deathType != DeathType.None )
            {
                ICustomHero customHero = vanDamme as ICustomHero;
                LoadHero.customBroDeaths.Add(SingletonNetObj<StatisticsController>.Instance.currentStats.deathList.Count, 
                    new CustomBroDeath(customHero.info, customHero.CurrentVariant));
            }
        }
    }

    // Fix custom bros having wrong sprite show up on death screen
    [HarmonyPatch(typeof(LevelOverScreen), "AddDeath")]
    static class LevelOverScreen_AddDeath_Patch
    {
        public static bool Prefix(LevelOverScreen __instance, ref int deathNum, ref int totalDeaths, ref Transform parent, ref ShakeM shakeObject)
        {
            if (!Main.enabled)
            {
                return true;
            }

            if ( LoadHero.customBroDeaths.ContainsKey(deathNum) )
            {
                int num = 40 + totalDeaths / 6;
                int num2 = 1 + totalDeaths / num;
                int num3 = deathNum / num;
                int num4 = deathNum - num3 * num;
                float num5 = 190f / (float)Mathf.Min(num, totalDeaths);
                float num6 = (num2 > 1) ? (50f / ((float)num2 + 0.85f)) : 0f;
                if (num5 > 16f)
                {
                    num5 = 16f;
                }
                float num7 = -num5 * (float)(Mathf.Min(num, totalDeaths) - 1) / 2f;
                float num8 = (num2 > 1) ? (num6 * ((float)(num2 - 1) / 2f) + 10f) : 10f;
                DeathObject deathObject = StatisticsController.GetDeathObject(deathNum);
                Vector3 position = __instance.mookDeathsHolder.transform.position + new Vector3(num7 + (float)num4 * num5, num8 - (float)num3 * num6, 35f * (float)deathNum / (float)totalDeaths - (float)(num3 * 45));

                if ( deathObject != null )
                {
                    VictoryMookDeath victoryMookDeath = UnityEngine.Object.Instantiate<VictoryMookDeath>(__instance.broDeathGenericPrefab, position, Quaternion.identity);
                    CustomBroDeath broDeath = LoadHero.customBroDeaths[deathNum];
                    CustomBroInfo bro = broDeath.info;
                    int variant = broDeath.variantIndex;
                    
                    victoryMookDeath.Setup(deathObject, 0.2f, parent, shakeObject);
                    
                    string spritePath = BroMakerUtilities.GetVariantValue(bro.SpritePath, variant);
                    string gunSpritePath = BroMakerUtilities.GetVariantValue(bro.GunSpritePath, variant);
                    Vector2 gunOffset = BroMakerUtilities.GetVariantValue(bro.GunSpriteOffset, variant);
                    
                    victoryMookDeath.GetComponent<MeshRenderer>().material.mainTexture = ResourcesController.GetTexture(spritePath);
                    victoryMookDeath.gunSprite.GetComponent<MeshRenderer>().material.mainTexture = ResourcesController.GetTexture(gunSpritePath);
                    victoryMookDeath.gunSprite.SetOffset(gunOffset.x, gunOffset.y, 0);
                }
                return false;
            }

            return true;
        }
    }

    // Fix avatars flashing for custom bros, for some reason vanilla bros don't have flashing avatars, other than the boondock bros
    [HarmonyPatch(typeof(HeroController), "FlashAvatar")]
    static class HeroController_FlashAvatar_Patch
    {
        public static bool Prefix(ref int playerNum)
        {
            if (!Main.enabled || !BSett.instance.disableCustomAvatarFlash)
            {
                return true;
            }

            return !(HeroController.players[playerNum].character is ICustomHero);
        }
    }

    // Fix grenades thrown by custom bros not having trails setup correctly
    [HarmonyPatch( typeof( ProjectileController ), "SpawnGrenadeOverNetwork" )]
    static class ProjectileController_SpawnGrenadeOverNetwork_Patch
    {
        public static bool Prefix( ref Grenade grenadePrefab, ref MonoBehaviour firedBy, ref float x, ref float y, ref float radius, ref float force, ref float xI, ref float yI, ref int playerNum, ref float lifeM, ref Grenade __result )
        {
            if ( !Main.enabled || !( firedBy is ICustomHero ) )
            {
                return true;
            }

            __result = ProjectileController.SpawnGrenadeLocally( grenadePrefab, firedBy, x, y, radius, force, xI, yI, playerNum, UnityEngine.Random.Range( 0, 10000 ) );
            if ( lifeM < 1f )
            {
                __result.ReduceLife( lifeM );
            }
            return false;
        }
    }

    // Fix airstrike projectiles not working when airstrike grenade is spawned locally
    [HarmonyPatch( typeof( ProjectileController ), "SpawnProjectileOverNetwork" )]
    static class ProjectileController_SpawnProjectileOverNetwork_Patch
    {
        public static bool Prefix( ref Projectile prefab, ref MonoBehaviour FiredBy, ref float x, ref float y, ref float xI, ref float yI, ref bool synced, ref int playerNum, ref bool AddTemporaryPlayerTarget, ref bool executeImmediately, ref float _zOffset, ref Projectile __result )
        {
            if ( !Main.enabled || !( FiredBy is ICustomHero ) )
            {
                return true;
            }

            __result = ProjectileController.SpawnProjectileLocally( prefab, FiredBy, x, y, xI, yI, playerNum, AddTemporaryPlayerTarget, _zOffset );
            __result.SetSeed( UnityEngine.Random.Range( -10000, 10000 ) );
            if ( AddTemporaryPlayerTarget )
            {
                HeroController.AddTemporaryPlayerTarget( playerNum, __result.transform );
            }
            return false;
        }
    }

    // Ensures custom projectiles are activated after instantiation.
    [HarmonyPatch( typeof( ProjectileController ), "SpawnGrenadeLocally" )]
    static class ProjectileController_SpawnGrenadeLocally_Patch
    {
        public static bool Prefix( ref Grenade grenadePrefab, ref MonoBehaviour firedBy, ref float x, ref float y, ref float radius, ref float force, ref float xI, ref float yI, ref int playerNum, ref int seed, ref Grenade __result )
        {
            if ( !Main.enabled || !( grenadePrefab is ICustomProjectile ) )
            {
                return true;
            }

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>( grenadePrefab.gameObject );
            gameObject.SetActive( true );
            __result = gameObject.GetComponent<Grenade>();
            __result.SetupGrenade( seed, playerNum, firedBy );
            __result.Launch( x, y, xI, yI );
            __result.NetworkSetup( PID.MyID );
            return false;
        }
    }

    // Ensures custom projectiles are activated after instantiation.
    [HarmonyPatch( typeof( ProjectileController ), "SpawnProjectileLocally", new Type[] { typeof(Projectile), typeof(MonoBehaviour), typeof(float), typeof(float), typeof(float), typeof(float), typeof(int) } )]
    static class ProjectileController_SpawnProjectileLocally_Patch
    {
        public static bool Prefix( ref Projectile projectilePrefab, ref MonoBehaviour FiredBy, ref float x, ref float y, ref float xI, ref float yI, ref int playerNum, ref Projectile __result )
        {
            if ( !Main.enabled || !( projectilePrefab is ICustomProjectile ) )
            {
                return true;
            }

            __result = UnityEngine.Object.Instantiate<Projectile>( projectilePrefab, new Vector3( x, y, 0f ), Quaternion.identity );
            __result.gameObject.SetActive( true );
            __result.Fire( x, y, xI, yI, 0f, playerNum, FiredBy );
            return false;
        }
    }

    // Ensures custom projectiles are activated after instantiation.
    [HarmonyPatch( typeof( ProjectileController ), "SpawnProjectileLocally", new Type[] { typeof( Projectile ), typeof( MonoBehaviour ), typeof( float ), typeof( float ), typeof( float ), typeof( float ), typeof( int ), typeof( bool ), typeof( float ) } )]
    static class ProjectileController_SpawnProjectileLocally2_Patch
    {
        public static bool Prefix( ref Projectile prefab, ref MonoBehaviour FiredBy, ref float x, ref float y, ref float xI, ref float yI, ref int playerNum, ref bool AddTemporaryPlayerTarget, ref float _zOffset, ref Projectile __result )
        {
            if ( !Main.enabled || !( prefab is ICustomProjectile ) )
            {
                return true;
            }

            __result = UnityEngine.Object.Instantiate<Projectile>( prefab, new Vector3( x, y, 0f ), Quaternion.identity );
            __result.gameObject.SetActive( true );
            __result.Fire( x, y, xI, yI, _zOffset, playerNum, FiredBy );
            return false;
        }
    }

    // Ensures custom projectiles are activated after instantiation.
    [HarmonyPatch( typeof( ProjectileController ), "SpawnProjectileLocally", new Type[] { typeof( Projectile ), typeof( MonoBehaviour ), typeof( float ), typeof( float ), typeof( float ), typeof( float ), typeof(bool), typeof( int ), typeof( bool ), typeof( bool ), typeof( float ) } )]
    static class ProjectileController_SpawnProjectileLocally3_Patch
    {
        public static bool Prefix( ref Projectile prefab, ref MonoBehaviour FiredBy, ref float x, ref float y, ref float xI, ref float yI, ref bool synced, ref int playerNum, ref bool AddTemporaryPlayerTarget, ref bool executeImmediately, ref float _zOffset, ref Projectile __result )
        {
            if ( !Main.enabled || !( prefab is ICustomProjectile ) )
            {
                return true;
            }

            __result = UnityEngine.Object.Instantiate( prefab, new Vector3( x, y, 0f ), Quaternion.identity );
            __result.gameObject.SetActive( true );
            __result.Fire( x, y, xI, yI, _zOffset, playerNum, FiredBy );
            return false;
        }
    }

    // Handle custom pocketted special ammo
    [HarmonyPatch( typeof( BroBase ), "SetPlayerHUDAmmo" )]
    static class BroBase_SetPlayerHUDAmmo_Patch
    {
        public static bool Prefix( ref BroBase __instance )
        {
            if ( !Main.enabled )
            {
                return true;
            }

            if ( __instance.pockettedSpecialAmmo.Count > 0 && __instance.pockettedSpecialAmmo[__instance.pockettedSpecialAmmo.Count - 1] == PockettedSpecialAmmoType.None && CustomPockettedSpecial.pockettedSpecials[__instance.playerNum].Count > 0 )
            {
                CustomPockettedSpecial.pockettedSpecials[__instance.playerNum].Last().SetSpecialMaterials( __instance );
                __instance.player.hud.SetGrenades( 1 );
                return false;
            }

            return true;
        }
    }

    [HarmonyPatch( typeof( BroBase ), "StartPockettedSpecial" )]
    static class BroBase_StartPockettedSpecial_Patch
    {
        public static void Postfix( ref BroBase __instance )
        {
            if ( !Main.enabled )
            {
                return;
            }

            if ( __instance.pockettedSpecialAmmo.Count > 0 && __instance.pockettedSpecialAmmo[__instance.pockettedSpecialAmmo.Count - 1] == PockettedSpecialAmmoType.None && CustomPockettedSpecial.pockettedSpecials[__instance.playerNum].Count > 0 )
            {
                // If the pocketted special uses the throwing animation then we need to set the usingPockettedSpecialType to something other than None, which defaults to the flex animation
                if ( CustomPockettedSpecial.pockettedSpecials[__instance.playerNum].Last().UseThrowingAnimation() )
                {
                    __instance.SetFieldValue( "usingPockettedSpecialType", PockettedSpecialAmmoType.Airstrike );
                }
            }
        }
    }

    [HarmonyPatch( typeof( BroBase ), "UsePockettedSpecial" )]
    static class BroBase_UsePockettedSpecial_Patch
    {
        public static bool Prefix( ref BroBase __instance )
        {
            if ( !Main.enabled )
            {
                return true;
            }

            if ( __instance.pockettedSpecialAmmo.Count > 0 && __instance.pockettedSpecialAmmo[__instance.pockettedSpecialAmmo.Count - 1] == PockettedSpecialAmmoType.None && CustomPockettedSpecial.pockettedSpecials[__instance.playerNum].Count > 0 )
            {
                __instance.SetFieldValue( "pressSpecialFacingDirection", 0 );
                CustomPockettedSpecial special = CustomPockettedSpecial.pockettedSpecials[__instance.playerNum].Last();
                special.UseSpecial( __instance );
                CustomPockettedSpecial.pockettedSpecials[__instance.playerNum].Remove( special );
                __instance.pockettedSpecialAmmo.RemoveAt( __instance.pockettedSpecialAmmo.Count - 1 );

                // Reset bro's specials to original count if this pocketted special allows it
                if ( special.RefreshAmmo() )
                {
                    __instance.ResetSpecialAmmo();
                }

                // Call private method SetPlayerHUDAmmo
                typeof( BroBase ).GetMethod( "SetPlayerHUDAmmo", BindingFlags.NonPublic | BindingFlags.Instance ).Invoke( __instance, new object[] {} );
                return false;
            }

            return true;
        }
    }
}

using HarmonyLib;
using System;
using System.Linq;
using System.Collections.Generic;
using BroMakerLib.Loggers;
using BroMakerLib.Cutscenes;
using BroMakerLib.Infos;
using UnityEngine;
using BroMakerLib.Loaders;
using BSett = BroMakerLib.Settings;
using BroMakerLib.CustomObjects.Bros;

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
                            if ( BSett.instance.notUnlockedBros.Count() > 0 )
                            {
                                LoadHero.playCutscene = true;
                            }
                        }
                        else
                        {
                            // Probability of a custom bro being unlocked should be the number of custom bros / number of custom bros + notUnlocked or dead normal bros
                            // We add 1 because the chosen bro will have already been added to the available bros
                            LoadHero.willReplaceBro[__instance.playerNum] = UnityEngine.Random.value <= (BSett.instance.notUnlockedBros.Count() /
                                (BSett.instance.notUnlockedBros.Count() + PlayerProgress.Instance.unlockedHeroes.Count() -
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
                        if (BSett.instance.availableBros.Count() == 0)
                        {
                            // We use this function to add a character to the pool to start with
                            BSett.instance.getRandomHardcoreBro(true);
                        }
                        LoadHero.willReplaceBro[__instance.playerNum] = true;
                    }
                    else if (BSett.instance.availableBros.Count() > 0)
                    {
                        LoadHero.willReplaceBro[__instance.playerNum] = UnityEngine.Random.value <= (BSett.instance.availableBros.Count() /
                        ((float)BSett.instance.availableBros.Count() + GameState.Instance.currentWorldmapSave.hardcoreModeAvailableBros.Count()));
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
                        choice = BSett.instance.getRandomHardcoreBro(LoadHero.playCutscene);
                    }
                    else
                    {
                        choice = BSett.instance.getRandomEnabledBro();
                        if ( LoadHero.playCutscene = !BSett.instance.seenBros.Contains(choice.name) && choice.GetInfo().cutscene.playCutsceneOnFirstSpawn )
                        {
                            BSett.instance.seenBros.Add(choice.name);
                        }
                    }
                    LoadHero.spawnFromPlayer = (__instance.rescuingThisBro != null);

                    if (LoadHero.playCutscene)
                    {
                        Cutscenes.CustomCutsceneController.LoadHeroCutscene(choice.GetInfo().cutscene);
                        LoadHero.playCutscene = false;
                    }

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
        static bool Prefix(Player __instance, ref TestVanDammeAnim bro)
        {
            if (!Main.enabled || !LoadHero.spawningCustomBro[__instance.playerNum] )
            {
                return true;
            }
            else if ( LoadHero.previousSpawnInfo[__instance.playerNum] == Player.SpawnType.AddBroToTransport )
            {
                var hero = bro as CustomHero;
                if (hero != null)
                {
                    hero.info.BeforeStart(hero);
                    // Need to manually call AddBroToHeroTransport, because WorkoutSpawnPosition won't do it for custom characters for some reason
                    Map.AddBroToHeroTransport(bro);
                }

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
            if ( currentCharacter is CustomHero && (currentCharacter as CustomHero).specialMaterials != null )
            {
                CustomHero customHero = (currentCharacter as CustomHero);

                BroMakerUtilities.SetSpecialMaterials(playerNum, customHero.specialMaterials, customHero.specialMaterialOffset, customHero.specialMaterialSpacing);
                return false;
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
            if (GameModeController.IsHardcoreMode && GameState.Instance.currentWorldmapSave != null && GameState.Instance.currentWorldmapSave.hardcoreModeAvailableBros.Count() == 0 && BSett.instance.availableBros.Count() > 0 )
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
                if ( BSett.instance.onlyCustomInHardcore && BSett.instance.notUnlockedBros.Count() == 0 )
                {
                    return false;
                }
                else if ( BSett.instance.notUnlockedBros.Count() +
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

            if ( GameModeController.IsHardcoreMode && __instance.character is CustomHero )
            {
                CustomHero customHero = (__instance.character as CustomHero);
                string broName = customHero.info.name;
                BSett.instance.availableBros.Remove(broName);
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
                __result = BSett.instance.notUnlockedBros.Count() > 0;
            }
            else
            {
                __result = __result || BSett.instance.notUnlockedBros.Count() > 0;
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
            if ( pilotUnit is CustomHero && __instance.CanPilotUnit(pilotUnit.playerNum) )
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
        public static int customBroIsConsumed = 0;
        public static void Postfix(AssMouthOrifice __instance, ref BroforceObject obj, ref bool __result )
        {
            if (!Main.enabled)
            {
                return;
            }

            if ( __result && obj is CustomHero )
            {
                ++customBroIsConsumed;
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

            if ( AssMouthOrifice_TryConsumeObject_Patch.customBroIsConsumed > 0 )
            {
                Traverse assTraverse = Traverse.Create(__instance);
                if ( assTraverse.GetFieldValue("CurrentAssMouthBlock") == null && assTraverse.GetFieldValue("transportedObject") is CustomHero )
                {
                    __instance.ExitAssMouth((assTraverse.GetFieldValue("PrevAssMouthBlock") as AssMouthBlock).orificeInstance);
                    --AssMouthOrifice_TryConsumeObject_Patch.customBroIsConsumed;
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

            if ( unit.impaledByTransform == null && unit is CustomHero )
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

            if ( self is CustomHero )
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

            if ( LoadHero.tryReplaceAvatar && HeroController.players[LoadHero.playerNum].character is CustomHero )
            {
                LoadHero.tryReplaceAvatar = false;

                CustomHero customHero = (HeroController.players[LoadHero.playerNum].character as CustomHero);
                Material mat = customHero.firstAvatar;
                if ( mat != null )
                {
                    sprite.GetComponent<Renderer>().material = mat;
                    // Move custom bro avatar down one pixel
                    HeroController.players[LoadHero.playerNum].hud.avatar.SetOffset(new Vector3(0, 15, 10));

                    __result = false;
                    return false;
                }
            }

            for ( int i = 0; i < 4; ++i )
            {
                // Ensure all non custom bros have normal offset
                if (HeroController.players[i] && !(HeroController.players[i].character is CustomHero) )
                {
                    HeroController.players[i].hud.avatar.SetOffset(new Vector3(0, 16, 10));
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

            LoadHero.customBroDeaths = new Dictionary<int, CustomBroInfo>();

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

            if (vanDamme is CustomHero && deathType != DeathType.None )
            {
                LoadHero.customBroDeaths.Add(SingletonNetObj<StatisticsController>.Instance.currentStats.deathList.Count, (vanDamme as CustomHero).info);
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
                    CustomBroInfo bro = LoadHero.customBroDeaths[deathNum];
                    victoryMookDeath.Setup(deathObject, 0.2f, parent, shakeObject);
                    victoryMookDeath.GetComponent<MeshRenderer>().material.mainTexture = ResourcesController.GetTexture(bro.spritePath);
                    victoryMookDeath.gunSprite.GetComponent<MeshRenderer>().material.mainTexture = ResourcesController.GetTexture(bro.gunSpritePath);
                    victoryMookDeath.gunSprite.SetOffset(bro.gunSpriteOffset.x, bro.gunSpriteOffset.y, 0);
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

            return !(HeroController.players[playerNum].character is CustomHero);
        }
    }

}

﻿using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using UnityEngine;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.CustomObjects;
using World.Generation.MapGenV4;
using Net = Networking.Networking;
using HarmonyLib;

namespace BroMakerLib.Loaders
{
    public static class LoadHero
    {
        public const string GAMEOBJECT_PREFIX = "BM_";
        public static CustomBroInfo currentInfo;
        public static int playerNum = 0;

        private static Dictionary<string, int> prefabIndex = new Dictionary<string, int>();

        public static bool spawnFromPlayer = false;

        public static void WithCustomBroInfo<T>(int selectedPlayerNum, CustomBroInfo customBroInfo) where T : CustomHero
        {
            WithCustomBroInfo(selectedPlayerNum, customBroInfo, typeof(T));
        }

        public static void WithCustomBroInfo(int selectedPlayerNum, CustomBroInfo customBroInfo)
        {
            string preset = customBroInfo.characterPreset;
            if (preset.IsNullOrEmpty())
            {
                throw new NullReferenceException("'characterPreset' is null or empty");
            }
            if (!PresetManager.heroPreset.ContainsKey(preset))
            {
                throw new Exception($"'characterPreset': {preset} doesn't exist. Check if you have the preset install or if there is a typo.");
            }
            WithCustomBroInfo(selectedPlayerNum, customBroInfo, PresetManager.heroPreset[preset]);
        }


        public static void WithCustomBroInfo(int selectedPlayerNum, CustomBroInfo customBroInfo, Type type)
        {
            try
            {
                if (!typeof(ICustomHero).IsAssignableFrom(type))
                    throw new ArgumentException($"Type {type.Name} should inherited from 'ICustomHero'", "type");

                // Assign static variables
                if (selectedPlayerNum < 0)
                    throw new ArgumentException("Player Num must be greater or equal to 0");
                else if (selectedPlayerNum > 3)
                    throw new ArgumentException("Player Num must be smaller or equal to 3");
                else
                    playerNum = selectedPlayerNum;

                if (customBroInfo == null)
                    throw new NullReferenceException("Info is null");
                else
                    currentInfo = customBroInfo;

                // Start Spawning Process
                BMLogger.Debug("Spawner: Start Spawning Process.");

                // Check Player
                Player player = HeroController.players[playerNum];
                if(player == null)
                    throw new NullReferenceException($"Player number {playerNum} doesn't exist.");
                Vector3 previousPosition = Vector3.zero;
                if (player.character != null && player.character.IsAlive())
                {
                    previousPosition = player.character.GetFollowPosition();
                    Net.RPC(PID.TargetAll, new RpcSignature(player.character.RecallBro), false);
                }

                HeroType heroType = GetBaseHeroTypeOfPreset(type);

                GameObject original = null;

                var originalGO = GameObject.Find(GAMEOBJECT_PREFIX + customBroInfo.name);
                if (prefabIndex.ContainsKey(customBroInfo.name))
                {
                    int index = prefabIndex[customBroInfo.name];
                    original = InstantiationController.GetPrefabFromIndex(index);
                }

                if (original == null)
                {
                    original = CreateOriginal(heroType, type);
                }


                TestVanDammeAnim hero = Net.InstantiateBuffered<GameObject>(original, previousPosition, Quaternion.identity, new object[0], false).GetComponent(type) as TestVanDammeAnim;
                BMLogger.Debug($"AfterInstantiation: InstantiateBuffered.");
                switch ( playerNum )
                {
                    case 0:
                        hero.player1Bubble.SetPosition( hero.player1Bubble.transform.localPosition );
                        break;
                    case 1:
                        hero.player2Bubble.SetPosition( hero.player2Bubble.transform.localPosition );
                        break;
                    case 2:
                        hero.player3Bubble.SetPosition( hero.player3Bubble.transform.localPosition );
                        break;
                    case 3:
                        hero.player4Bubble.SetPosition( hero.player4Bubble.transform.localPosition );
                        break;
                }


                var bro = AfterInstantiation(hero, heroType, playerNum, type, previousPosition);
                BMLogger.Debug("Spawner: Finished AfterInstantiation.");

                BMLogger.Debug("Spawner: Spawning Process has ended.");
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
            spawnFromPlayer = false;
        }

        private static TestVanDammeAnim AfterInstantiation(TestVanDammeAnim hero, HeroType heroTypeEnum, int playerNum, Type type, Vector3 position)
        {
            hero.gameObject.SetActive(true);
            if (Registry.GetNID(hero) == NID.NoID)
            {
                Registry.RegisterDeterminsiticObject(hero);
                BMLogger.Debug($"AfterInstantiation: Register Object cause it didn't.");
            }

            Net.RPC<int, HeroType, bool>(PID.TargetAll, new RpcSignature<int, HeroType, bool>(hero.SetUpHero), playerNum, heroTypeEnum, true, false);
            BMLogger.Debug("AfterInstantiation: SetUpHero");

            FixSpawnPosition(hero, position);

            /*if (!GameModeController.ShowStandardHUDS())
            {
                Net.RPC<TestVanDammeAnim>(PID.TargetAll, new RpcSignature<TestVanDammeAnim>(player.SetUpDeathMatchHUD), testVanDammeAnim, false);
            }*/
            if ((ProcGenGameMode.isEnabled || ProcGenGameMode.ProcGenTestBuild) && ProcGenGameMode.isInsideRoom && ProcGenGameMode.diedInsideRoom)
            {
                ProcGenGameMode.UnlockAfterDyingInsideRoom(hero);
                hero.player.Invoke("UnlockProcGenCamera", 0.1f);
            }

            AssignFlexPower(hero);
            BMLogger.Debug("AfterInstantiation: Flex Power Assigned");
            return hero;
        }

        private static void FixSpawnPosition(TestVanDammeAnim hero, Vector3 position)
        {
            Player player = hero.player;
            if(spawnFromPlayer)
            {
                if (position == Vector3.zero)
                {
                    position = HeroController.GetCheckPointPosition(playerNum, Map.IsCheckPointAnAirdrop(HeroController.GetCurrentCheckPointID()));
                }
                player.playerFollowPos = position;
                player.changingBroFromTrigger = true;
            }

            player.WorkOutSpawnPosition(hero);
        }

        private static void AssignFlexPower(TestVanDammeAnim testVanDammeAnim)
        {
            var player = testVanDammeAnim.player;

            if (player.GetFieldValue<PickupType>("_forceFlexPowerupSpawn") != PickupType.None)
            {
                player.AddFlexPower(player.GetFieldValue<PickupType>("_forceFlexPowerupSpawn"), true);
                Net.RPC<PickupType, bool>(PID.TargetOthers, new RpcSignature<PickupType, bool>(player.AddFlexPower), player.GetFieldValue<PickupType>("_forceFlexPowerupSpawn"), true, false);
            }
            else
            {
                switch (Map.MapData.flexPowerType)
                {
                    case FlexPowerMapType.SpawnWithRandomFlex:
                        switch (UnityEngine.Random.Range(0, 4))
                        {
                            case 0:
                                player.AddFlexPower(PickupType.FlexGoldenLight, true);
                                break;
                            case 1:
                                player.AddFlexPower(PickupType.FlexInvulnerability, true);
                                break;
                            case 2:
                                player.AddFlexPower(PickupType.FlexAirJump, true);
                                break;
                            case 3:
                                player.AddFlexPower(PickupType.FlexTeleport, true);
                                break;
                        }
                        break;
                    case FlexPowerMapType.SpawnWithAllure:
                        player.AddFlexPower(PickupType.FlexAlluring, true);
                        break;
                    case FlexPowerMapType.SpawnWithGoldenLight:
                        player.AddFlexPower(PickupType.FlexGoldenLight, true);
                        break;
                    case FlexPowerMapType.SpawnWithInvincible:
                        player.AddFlexPower(PickupType.FlexInvulnerability, true);
                        break;
                    case FlexPowerMapType.SpawnWithAirFlex:
                        player.AddFlexPower(PickupType.FlexAirJump, true);
                        break;
                    case FlexPowerMapType.SpawnWithTeleport:
                        player.AddFlexPower(PickupType.FlexTeleport, true);
                        break;
                    case FlexPowerMapType.SpawnWithEarnedFlex:
                        switch (UnityEngine.Random.Range(0, 4))
                        {
                            case 0:
                                if (PlayerProgress.IsPickupUnlockedInAnySave(PickupType.FlexGoldenLight))
                                {
                                    player.AddFlexPower(PickupType.FlexGoldenLight, true);
                                }
                                break;
                            case 1:
                                if (PlayerProgress.IsPickupUnlockedInAnySave(PickupType.FlexInvulnerability))
                                {
                                    player.AddFlexPower(PickupType.FlexInvulnerability, true);
                                }
                                break;
                            case 2:
                                if (PlayerProgress.IsPickupUnlockedInAnySave(PickupType.FlexAirJump))
                                {
                                    player.AddFlexPower(PickupType.FlexAirJump, true);
                                }
                                break;
                            case 3:
                                if (PlayerProgress.IsPickupUnlockedInAnySave(PickupType.FlexTeleport))
                                {
                                    player.AddFlexPower(PickupType.FlexTeleport, true);
                                }
                                break;
                        }
                        break;
                }
            }
        }
        private static TestVanDammeAnim GetHeroPrefab(HeroType heroType) => HeroController.GetHeroPrefab(heroType);

        private static HeroType GetBaseHeroTypeOfPreset(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(HeroPresetAttribute), true);
            if (attributes.IsNotNullOrEmpty())
            {
                return attributes[0].As<HeroPresetAttribute>().baseType;
            }
            throw new NotImplementedException($"Type {type} as no attribute of {nameof(HeroPresetAttribute)}");
        }

        private static GameObject CreateOriginal(HeroType heroType, Type type)
        {
            //GameObject prefab = GetPrefab("networkobjects:Rambo");
            GameObject prefab = GetHeroPrefab(heroType).gameObject;
            GameObject inst  = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);

            inst.AddComponent(type);
            inst.name = GAMEOBJECT_PREFIX + currentInfo.name;
            if (inst == null)
                throw new Exception("CreateOriginal: Instantiate has failed is null");
            BMLogger.Debug("CreateOriginal: Has Instantiate Hero.");
            inst.SetActive(false);

            AddObjectToPrefabList(inst);
            BMLogger.Debug("CreateOriginal: inst added to list");

            return inst;
        }

        private static void AddObjectToPrefabList(GameObject gameObject)
        {
            var prefabListT = Traverse.Create<InstantiationController>().Field("prefabList");
            var list = prefabListT.GetValue<List<UnityEngine.Object>>();
            list.Add(gameObject);
            prefabListT.SetValue(list);
            if (prefabIndex.ContainsKey(currentInfo.name))
                prefabIndex[currentInfo.name] = InstantiationController.GetPrefabIndex(gameObject);
            else
                prefabIndex.Add(currentInfo.name, InstantiationController.GetPrefabIndex(gameObject));
        }

        private static GameObject GetPrefab(string resourceName)
        {
            return InstantiationController.GetPrefabFromResourceName(resourceName);
        }
    }
}
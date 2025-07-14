using BroMakerLib.Infos;
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
    public struct CustomBroDeath
    {
        public CustomBroInfo info;
        public int variantIndex;
        
        public CustomBroDeath(CustomBroInfo info, int variantIndex)
        {
            this.info = info;
            this.variantIndex = variantIndex;
        }
    }
    
    public static class LoadHero
    {
        public const string GAMEOBJECT_PREFIX = "BM_";
        public static CustomBroInfo currentInfo;
        public static int playerNum = 0;

        private static Dictionary<string, int> prefabIndex = new Dictionary<string, int>();

        public static bool spawnFromPlayer = false;
        public static bool[] willReplaceBro = new bool[] { false, false, false, false };
        public static bool[] spawningCustomBro = new bool[] { false, false, false, false };
        public static bool anyCustomSpawning = false;
        public static bool broBeingRescued = false;
        public static bool playCutscene = false;
        public static bool tryReplaceAvatar = false;
        public static Player.SpawnType[] previousSpawnInfo = new Player.SpawnType[] { Player.SpawnType.Unknown, Player.SpawnType.Unknown, Player.SpawnType.Unknown, Player.SpawnType.Unknown };
        public static bool[] wasFirstDeployment = new bool[] { false, false, false, false };
        public static Dictionary<int, CustomBroDeath> customBroDeaths;

        public static TestVanDammeAnim WithCustomBroInfo(int selectedPlayerNum, CustomBroInfo customBroInfo, Type type)
        {
            tryReplaceAvatar = true;
            TestVanDammeAnim hero = null;
            try
            {
                if (!typeof(ICustomHero).IsAssignableFrom(type))
                    throw new ArgumentException($"Type '{type.Name}' should inherit from 'ICustomHero'", "type");

                if (selectedPlayerNum < 0)
                    throw new IndexOutOfRangeException("Player Num must be greater than or equal to 0");
                else if (selectedPlayerNum > 3)
                    throw new IndexOutOfRangeException("Player Num must be smaller than or equal to 3");

                playerNum = selectedPlayerNum;

                if (customBroInfo == null)
                    throw new NullReferenceException("Info is null");

                currentInfo = customBroInfo;

                // Start Spawning Process
                BMLogger.Debug("Spawner: Start Spawning Process.");

                // Check Player
                Player player = HeroController.players[playerNum];
                if(player == null)
                    throw new NullReferenceException($"Player number {playerNum} doesn't exist.");


                Vector3 previousPosition = Vector3.zero;
                TestVanDammeAnim previousCharacter = player.character;
                ReactionBubble previousCharacterBubble = null;
                Traverse previousCharacterTraverse = null;
                if (player.character != null && player.character.IsAlive())
                {
                    previousPosition = player.character.GetFollowPosition();
                    previousCharacterTraverse = Traverse.Create(player.character);
                    previousCharacterBubble = player.character.playerBubble;
                    Net.RPC(PID.TargetAll, new RpcSignature(player.character.RecallBro), false);
                }

                HeroType heroType = GetBaseHeroTypeOfPreset(type);

                GameObject original = null;

                if (prefabIndex.ContainsKey(customBroInfo.name))
                {
                    int index = prefabIndex[customBroInfo.name];
                    original = InstantiationController.GetPrefabFromIndex(index);
                }

                if (original == null)
                {
                    original = CreateOriginal(heroType, type);
                }


                hero = Net.InstantiateBuffered<GameObject>(original, previousPosition, Quaternion.identity, new object[0], false).GetComponent(type) as TestVanDammeAnim;
                hero.gameObject.SetActive(true);
                BMLogger.Debug($"AfterInstantiation: InstantiateBuffered.");

                var bro = AfterInstantiation(hero, heroType, playerNum, type, previousPosition);
                if ( previousCharacterTraverse != null )
                {
                    // Make previous character sprite disappear faster
                    previousCharacterTraverse.Field("recallCounter").SetValue(1f);
                    if ( previousCharacterBubble != null )
                    {
                        previousCharacterBubble.GoAway();
                    }
                }
                // For some reason the change made to the WorkOutSpawn Position function changed how this stuff works
                if (previousSpawnInfo[playerNum] != Player.SpawnType.AddBroToTransport)
                {
                    hero.playerBubble.SetPosition(hero.playerBubble.transform.localPosition + new Vector3(0f, 5f));
                }
                else
                {
                    hero.playerBubble.SetPosition(hero.playerBubble.transform.localPosition);
                }
                Traverse bubbleTrav = Traverse.Create(hero.playerBubble);
                hero.playerBubble.RestartBubble();
                bubbleTrav.Field("yStart").SetValue(hero.playerBubble.transform.localPosition.y + 5f);

                // This ensures the high5Bubble is 5 pixels higher than the player bubble, which is apparently the correct place based off of how the vanilla bros work
                hero.high5Bubble.SetPosition(hero.high5Bubble.transform.localPosition);
                Traverse high5BubbleTrav = Traverse.Create(hero.high5Bubble);
                hero.high5Bubble.gameObject.SetActive(true);
                high5BubbleTrav.Field("yStart").SetValue(hero.high5Bubble.transform.localPosition.y);

                // Destroy character we replaced
                if ( previousCharacter != null )
                {
                    UnityEngine.Object.Destroy( previousCharacter.gameObject );
                }

                BMLogger.Debug("Spawner: Finished AfterInstantiation.");

                BMLogger.Debug("Spawner: Spawning Process has ended.");
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
            spawnFromPlayer = false;
            return hero;
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

            WorkOutSpawnPosition(player, hero);
        }

        // Rewritten to remove RPC calls
        private static void WorkOutSpawnPosition(Player player, TestVanDammeAnim bro)
        {
            player.firstDeployment = wasFirstDeployment[player.playerNum];
            Vector3 arg = new Vector3(100f, 100f);
            Player.SpawnType arg2 = LoadHero.previousSpawnInfo[player.playerNum];
            bool flag = false;
            bool flag2 = false;
            switch (arg2)
            {
                case Player.SpawnType.Unknown:
                    flag = true;
                    goto IL_1E7;
                case Player.SpawnType.AddBroToTransport:
                    {
                        ICustomHero customHero = bro as ICustomHero;
                        if (customHero != null)
                        {
                            // Ensure character has the right sprite when spawning attached to a vehicle
                            customHero.SetSprites();
                        }
                        Map.AddBroToHeroTransport(bro);
                        arg = bro.transform.position;
                        goto IL_1E7;
                    }
                case Player.SpawnType.CheckpointRespawn:
                    flag2 = Map.IsCheckPointAnAirdrop(HeroController.GetCurrentCheckPointID());
                    arg = HeroController.GetCheckPointPosition(player.playerNum, flag2);
                    goto IL_1E7;
                case Player.SpawnType.RespawnAtRescueBro:
                    if (player.rescuingThisBro == null)
                    {
                        flag = true;
                    }
                    else
                    {
                        arg = player.rescuingThisBro.transform.position;
                    }
                    goto IL_1E7;
                case Player.SpawnType.DropInDuringGame:
                    flag = true;
                    goto IL_1E7;
                case Player.SpawnType.SpawnInCage:
                    {
                        SpawnPoint spawnPoint = Map.GetSpawnPoint(player.playerNum);
                        if (spawnPoint == null)
                        {
                            flag = true;
                        }
                        else
                        {
                            arg = spawnPoint.transform.position;
                            if (spawnPoint.cage != null)
                            {
                                spawnPoint.cage.SetPlayerColor(player.playerNum);
                                arg.x -= 8f;
                            }
                        }
                        goto IL_1E7;
                    }
                case Player.SpawnType.LevelEditorReload:
                    arg = LevelEditorGUI.lastPayerPos;
                    LevelEditorGUI.lastPayerPos = -Vector3.one;
                    Map.CallInHeroTransportAnyway();
                    goto IL_1E7;
                case Player.SpawnType.TriggerSwapBro:
                    arg = player.playerFollowPos;
                    goto IL_1E7;
                case Player.SpawnType.CustomSpawnPoint:
                    {
                        SpawnPoint spawnPoint2 = Map.GetSpawnPoint(player.playerNum);
                        arg = spawnPoint2.transform.position;
                        if (spawnPoint2 != null && spawnPoint2.cage != null)
                        {
                            arg.x -= 8f;
                        }
                        goto IL_1E7;
                    }
                case Player.SpawnType.AirDropRespawn:
                    arg = HeroController.GetCheckPointPosition(player.playerNum, true);
                    flag2 = true;
                    goto IL_1E7;
            }
            flag = true;
            IL_1E7:
            if (flag)
            {
                arg = HeroController.GetFirstPlayerPosition(player.playerNum);
            }
            player.SetSpawnPositon(bro, arg2, flag2, arg);
        }

        private static void AssignFlexPower( TestVanDammeAnim testVanDammeAnim )
        {
            var player = testVanDammeAnim.player;

            if ( player.GetFieldValue<PickupType>( "_forceFlexPowerupSpawn" ) != PickupType.None )
            {
                player.AddFlexPower( player.GetFieldValue<PickupType>( "_forceFlexPowerupSpawn" ), true );
                Net.RPC<PickupType, bool>( PID.TargetOthers, new RpcSignature<PickupType, bool>( player.AddFlexPower ), player.GetFieldValue<PickupType>( "_forceFlexPowerupSpawn" ), true, false );
            }
            else
            {
                switch ( Map.MapData.flexPowerType )
                {
                    case FlexPowerMapType.SpawnWithRandomFlex:
                        switch ( UnityEngine.Random.Range( 0, 4 ) )
                        {
                            case 0:
                                player.AddFlexPower( PickupType.FlexGoldenLight, true );
                                break;
                            case 1:
                                player.AddFlexPower( PickupType.FlexInvulnerability, true );
                                break;
                            case 2:
                                player.AddFlexPower( PickupType.FlexAirJump, true );
                                break;
                            case 3:
                                player.AddFlexPower( PickupType.FlexTeleport, true );
                                break;
                        }
                        break;
                    case FlexPowerMapType.SpawnWithAllure:
                        player.AddFlexPower( PickupType.FlexAlluring, true );
                        break;
                    case FlexPowerMapType.SpawnWithGoldenLight:
                        player.AddFlexPower( PickupType.FlexGoldenLight, true );
                        break;
                    case FlexPowerMapType.SpawnWithInvincible:
                        player.AddFlexPower( PickupType.FlexInvulnerability, true );
                        break;
                    case FlexPowerMapType.SpawnWithAirFlex:
                        player.AddFlexPower( PickupType.FlexAirJump, true );
                        break;
                    case FlexPowerMapType.SpawnWithTeleport:
                        player.AddFlexPower( PickupType.FlexTeleport, true );
                        break;
                    case FlexPowerMapType.SpawnWithEarnedFlex:
                        switch ( UnityEngine.Random.Range( 0, 4 ) )
                        {
                            case 0:
                                if ( PlayerProgress.IsPickupUnlockedInAnySave( PickupType.FlexGoldenLight ) )
                                {
                                    player.AddFlexPower( PickupType.FlexGoldenLight, true );
                                }
                                break;
                            case 1:
                                if ( PlayerProgress.IsPickupUnlockedInAnySave( PickupType.FlexInvulnerability ) )
                                {
                                    player.AddFlexPower( PickupType.FlexInvulnerability, true );
                                }
                                break;
                            case 2:
                                if ( PlayerProgress.IsPickupUnlockedInAnySave( PickupType.FlexAirJump ) )
                                {
                                    player.AddFlexPower( PickupType.FlexAirJump, true );
                                }
                                break;
                            case 3:
                                if ( PlayerProgress.IsPickupUnlockedInAnySave( PickupType.FlexTeleport ) )
                                {
                                    player.AddFlexPower( PickupType.FlexTeleport, true );
                                }
                                break;
                        }
                        break;
                }
            }
        }

        public static HeroType GetBaseHeroTypeOfPreset(Type type)
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
            GameObject prefab = HeroController.GetHeroPrefab( heroType).gameObject;
            prefab.SetActive(false);
            GameObject inst  = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
            prefab.SetActive(true);

            // Ensure prefab is only created once
            UnityEngine.Object.DontDestroyOnLoad( inst );

            inst.AddComponent(type);
            inst.name = GAMEOBJECT_PREFIX + currentInfo.name;
            if (inst == null)
                throw new Exception("CreateOriginal: Instantiate has failed is null");
            BMLogger.Debug("CreateOriginal: Has Instantiate Hero.");
            inst.SetActive(false);

            inst.GetComponent<CustomHero>().PrefabSetup();

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
    }
}

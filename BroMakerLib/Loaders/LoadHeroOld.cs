using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using System;
using UnityEngine;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.CustomObjects;
using Net = Networking.Networking;

namespace BroMakerLib.Loaders
{
    public static class LoadHeroOld
    {
        public static CustomBroInfo currentInfo;
        public static int playerNum = 0;

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
            if (!PresetManager.heroesPreset.ContainsKey(preset))
            {
                throw new Exception($"'characterPreset': {preset} doesn't exist. Check if you have the preset install or if there is a typo.");
            }
            WithCustomBroInfo(selectedPlayerNum, customBroInfo, PresetManager.heroesPreset[preset]);
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
                Player player = null;
                TestVanDammeAnim character = null;

                // Check if can spawn, if can't return
                if (!CanSpawn(ref player, ref character, type))
                    return;

                player.character.RecallBro();
                Net.RPC(PID.TargetAll, new RpcSignature(player.character.RecallBro), false);
                BMLogger.Debug("Spawner: Has RPC RecallBro.");

                //Net.RPC<Type>(PID.TargetAll, new RpcSignature<Type>(character.gameObject.AddComponent), type, true);
                TestVanDammeAnim bro =  character.gameObject.AddComponent(type) as TestVanDammeAnim;
                BMLogger.Debug("Spawner: Has AddComponent: " + nameof(type) + ".");

                Registry.RegisterDeterminsiticObject(bro);
                BMLogger.Debug($"Spawner: Register Object.");

                bro.As<ICustomHero>().info = customBroInfo;
                BMLogger.Debug("Spawner: Info added.");

                Net.InstantiateBuffered<TestVanDammeAnim>(bro, Vector3.zero, Quaternion.identity, new object[0], false);
                BMLogger.Debug("Spawner: Has InstantiateBuffered.");

                SetUpAndAssignCharacter(ref player, ref bro);
                BMLogger.Debug("Spawner: Spawning Process has end.");
            }
            catch (Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }

        private static bool CanSpawn(ref Player player, ref TestVanDammeAnim character, Type type)
        {
            player = HeroController.players[playerNum];
            if (player == null)
            {
                BMLogger.errorSwapingMessage = "The player don't exist.";
                return false;
            }
            //if (player.heroType != HeroType.Rambro)
            if (BroMaker.GetBroType(player.character.heroType) == null)
            {
                BMLogger.errorSwapingMessage = "You can't swap with this hero type.";
                return false;
            }

            character = player.character;
            if(character == null)
            {
                BMLogger.errorSwapingMessage = "The player has no character.";
                return false;
            }
           /* if (type != typeof(CustomHero) && character.GetComponent(type) != null)
            {
                BMLogger.errorSwapingMessage = "The bro is actually fighting terrorism.";
                return false;
            }*/
            BMLogger.errorSwapingMessage = string.Empty;
            BMLogger.Debug("Spawner: No problem founded.");
            return true;
        }

        private static void SetUpAndAssignCharacter(ref Player player, ref TestVanDammeAnim character)
        {
            try
            {
                //character.SetUpHero(playerNum, HeroType.Rambro, true);
                Net.RPC<int, HeroType, bool>(PID.TargetAll, new RpcSignature<int, HeroType, bool>(character.SetUpHero), playerNum, character.heroType, true, true);
                BMLogger.Debug("Spawner: Has Done RPC SetUpHero.");

                player.AssignCharacter(character);
                BMLogger.Debug("Spawner: Has Assign Character to Player.");
            }
            catch(Exception e)
            {
                BMLogger.ExceptionLog(e);
            }
        }
    }
}

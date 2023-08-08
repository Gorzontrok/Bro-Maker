using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Networking;
using BroMakerLib.Loggers;

namespace BroMakerLib
{
    /// <summary>
    /// Class to add a new character
    /// </summary>
    [Obsolete("Use \'LoadCharacter\' class instead.")]
    public class NewBroInfo2<TBro> where TBro : TestVanDammeAnim
    {
        /// <summary>
        /// The class of the bro
        /// </summary>
        public Type broType;
        /// <summary>
        /// Name of the bro
        /// </summary>
        public string name;
        /// <summary>
        /// Is fighting terrorist in game?
        /// </summary>
        public bool Swaped;

        private static int _nbrNoName;

        public NewBroInfo2(string _name = "")
        {
            if (string.IsNullOrEmpty(_name))
            {
                name = "NO_NAME_" + _nbrNoName;
                _nbrNoName++;
            }
            else
            {
                name = _name;
            }
            //AddBro(this);
        }

        /// <summary>
        /// Spawn the character
        /// </summary>
        /// <param name="playerNum"></param>
        public void Spawn(int playerNum)
        {
            try
            {
                Player oldPlayer = HeroController.players[playerNum];
                if (HeroController.players[playerNum] == null)
                {
                    BMLogger.errorSwapingMessage = "The player don't exist.";
                    return;
                }
                if (BroMaker.GetBroType(oldPlayer.character.heroType) == null)
                {
                    BMLogger.errorSwapingMessage = "You can't swap with this bro.";
                    return;
                }
                if (HeroController.players[playerNum].character.GetComponent(broType) != null)
                {
                    BMLogger.errorSwapingMessage = "The bro is actually fighting terrorism.";
                    return;
                }

                BMLogger.errorSwapingMessage = string.Empty;
                Networking.Networking.RPC(PID.TargetAll, new RpcSignature(HeroController.players[playerNum].character.RecallBro), false);

                TestVanDammeAnim character = HeroController.GetHeroPrefab(HeroType.Rambro);

                UnityEngine.Object.Destroy(character.GetComponent<WavyGrassEffector>());
                UnityEngine.Object.Destroy(character.GetComponent<Rambro>());

                TestVanDammeAnim bro = character.gameObject.AddComponent<TBro>();
                Networking.Networking.InstantiateBuffered<TestVanDammeAnim>(bro, Vector3.zero, Quaternion.identity);

                Networking.Networking.RPC<int, HeroType, bool>(PID.TargetAll, new RpcSignature<int, HeroType, bool>(bro.SetUpHero), playerNum, HeroType.Rambro, true, false);
                EffectsController.CreateHeroIndicator(bro);

                Swaped = true;
            }
            catch (Exception ex)
            {
                BMLogger.Log(ex);
            }
        }

    }

    [Obsolete]
    public class BrosInfoController
    {
        /// <summary>
        /// Custom character list
        /// </summary>
        public static List<NewBroInfo2<TestVanDammeAnim>> newBroInfos
        {
            get
            {
                return _newBroInfos;
            }
        }

        /// <summary>
        /// Names array of the custom character
        /// </summary>
        public static string[] Names
        {
            get
            {
                return _names;
            }
        }

        private static string[] _names = new string[] { };

        private static List<NewBroInfo2<TestVanDammeAnim>> _newBroInfos = new List<NewBroInfo2<TestVanDammeAnim>>();

        public static void AddBro<T>(NewBroInfo2<T> newBro) where T : TestVanDammeAnim
        {
            //_newBroInfos.Add(newBro);
            List<string> names = new List<string>(_names);
            names.Add(newBro.name);
            _names = names.ToArray();
        }
    }
}

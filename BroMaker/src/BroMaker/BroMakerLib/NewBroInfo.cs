using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BroMakerLoadMod;

namespace BroMakerLib
{
    /// <summary>
    /// Class to add a new character
    /// </summary>
    public class NewBroInfo
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

        /// <summary>
        /// Add a new character
        /// </summary>
        /// <param name="broType">class of the bro</param>
        /// <param name="_name">Name</param>
        public NewBroInfo(Type broType, string _name = "")
        {
            this.broType = broType;
            if(string.IsNullOrEmpty(_name))
            {
                name = "NO_NAME_" + _nbrNoName;
                _nbrNoName++;
            }
            else
            {
                name = _name;
            }
            AddBro(this);
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
                if(HeroController.players[playerNum] = null)
                {
                    Main.cantSwapMessage = "The player don't exist.";
                    return;
                }
                if(BroMaker.GetBroType(oldPlayer.character.heroType) == null)
                {
                    Main.cantSwapMessage = "You can't swap with this bro.";
                    return;
                }
                if (HeroController.players[playerNum].GetComponent(broType) != null)
                {
                    Main.cantSwapMessage = "The bro is actually fighting terrorism.";
                    return;
                }

                Main.cantSwapMessage = string.Empty;
                HeroController.players[playerNum].character.RecallBro();
                //Player oldPlayer = Player.Instantiate(HeroController.players[playerNum]);
                var Bro = HeroController.players[playerNum].character.gameObject.AddComponent(broType) as BroBaseMaker;
                Bro.playerNum = playerNum;
                //UnityEngine.Object.Destroy(Bro.gameObject.GetComponent<WavyGrassEffector>());
                UnityEngine.Object.Destroy(HeroController.players[playerNum].character.gameObject.GetComponent<WavyGrassEffector>());

                Bro.bm_SetupBro(HeroController.players[playerNum]);

                /* if (HeroController.players[playerNum].character.gameObject.GetComponent(broType) != null)
                 {
                     HeroController.players[playerNum].character = (TestVanDammeAnim)HeroController.players[playerNum].character.gameObject.GetComponent(broType);
                     // Main.Debug("dsqdsq");
                 }*/
                // UnityEngine.Object.Destroy(Bro.gameObject.GetComponent(HeroController.GetHeroPrefab(Bro.heroType).GetType()));
                //UnityEngine.Object.Destroy(HeroController.players[playerNum].character.gameObject.GetComponent(HeroController.GetHeroPrefab(HeroController.players[playerNum].character.heroType).GetType()));

                //Bro.SetUpHero(playerNum, HeroController.players[playerNum].character.heroType, true);

                //UnityEngine.Object.Destroy(oldPlayer);
                HeroController.players[playerNum].AssignCharacter(Bro);
                Swaped = true;
            }
            catch(Exception ex)
            {
                Main.Log(ex);
            }
        }

        /// <summary>
        /// Custom character list
        /// </summary>
        public static List<NewBroInfo> newBroInfos
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

        private static List<NewBroInfo> _newBroInfos = new List<NewBroInfo>();

        private static void AddBro(NewBroInfo newBro)
        {
            _newBroInfos.Add(newBro);
            List<string> names = new List<string>(_names);
            names.Add(newBro.name);
            _names = names.ToArray();
        }
    }
}

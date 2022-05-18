using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace BroMakerLib
{
	/// <summary>
	///
	/// </summary>
    public class MeleeHolder : MonoBehaviour
    {
		/// <summary>
		///
		/// </summary>
		public BroBaseMaker character;
		/// <summary>
		///
		/// </summary>
		/// <param name="meleeType"></param>
        public void StartMelee(BroBase.MeleeType meleeType)
        {
			/*if(character != null)
			{
				switch (meleeType)
				{
					case BroBase.MeleeType.Punch:
					case BroBase.MeleeType.JetpackPunch:
						this.StartPunch();
						break;
					case BroBase.MeleeType.Disembowel:
					case BroBase.MeleeType.FlipKick:
					case BroBase.MeleeType.Tazer:
					case BroBase.MeleeType.Custom:
					case BroBase.MeleeType.ChuckKick:
					case BroBase.MeleeType.VanDammeKick:
					case BroBase.MeleeType.ChainSaw:
					case BroBase.MeleeType.ThrowingKnife:
					case BroBase.MeleeType.Smash:
					case BroBase.MeleeType.BrobocopPunch:
					case BroBase.MeleeType.PistolWhip:
					case BroBase.MeleeType.HeadButt:
					case BroBase.MeleeType.TeleportStab:
						this.StartCustomMelee();
						break;
				}
			}*/
		}

		private void Awake()
        {

        }

	}
}

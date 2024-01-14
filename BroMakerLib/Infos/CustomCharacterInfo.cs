using BroMakerLib.Loaders;
using BroMakerLib.Stats;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.Infos
{
    public class CustomCharacterInfo : CustomBroforceObjectInfo
    {
        protected new string _defaultName = "CHARACTER";
        public CustomCharacterInfo() : base() { }
        public CustomCharacterInfo(string name) : base(name) { }

        public string characterPreset = "CustomHero";

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void ReadParameters(object obj)
        {
            base.ReadParameters(obj);
            TestVanDammeAnim character = obj as TestVanDammeAnim;

            /*string special = GetParameterValue<string>("Special");
            if (special.IsNotNullOrEmpty())
            {
                // Get Ability
            }
            string attack = GetParameterValue<string>("Attack");
            if (attack.IsNotNullOrEmpty())
            {
                // Get Ability
            }
            string melee = GetParameterValue<string>("melee");
            if (melee.IsNotNullOrEmpty())
            {
                // Get Ability
            }*/
        }
    }
}

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

        //public string weaponName = string.Empty;
        [JsonIgnore, EditorIgnore]
        public CustomWeaponInfo weaponInfo;

        //public string specialName = string.Empty;
        [JsonIgnore, EditorIgnore]
        public CustomSpecialInfo specialInfo;


        public override void Initialize()
        {
            base.Initialize();

            /*if(string.IsNullOrEmpty(weaponName))
                weaponInfo = new CustomWeaponInfo();
            else
                weaponInfo = DeserializeJSON<CustomWeaponInfo>(weaponName);
            weaponInfo.Initialize();

            if(string.IsNullOrEmpty(specialName))
                specialInfo = new CustomSpecialInfo();
            else
                specialInfo = DeserializeJSON<CustomSpecialInfo>(weaponName);
            specialInfo.Initialize();*/
        }

        public override void ReadParameters(object obj)
        {
            base.ReadParameters(obj);
            TestVanDammeAnim character = obj as TestVanDammeAnim;
            /*string grenadeName = GetParameterValue<string>("Grenade");
            if (grenadeName.IsNotNullOrEmpty())
            {
                character.specialGrenade = CustomGrenadeInfo.GetGrenadeFromName(grenadeName);
            }
            string projectileName = GetParameterValue<string>("Projectile");
            if (projectileName.IsNotNullOrEmpty())
            {
                character.projectile = CustomProjectileInfo.GetProjectileFromName(projectileName);
            }*/
        }
    }
}

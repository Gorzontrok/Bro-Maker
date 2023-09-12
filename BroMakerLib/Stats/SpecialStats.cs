using BroMakerLib.Stats.SpecialsTypes;
using System;
using System.Collections.Generic;
using RocketLib.JsonConverters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace BroMakerLib.Stats
{
    public class SpecialStats
    {
        public int originalSpecialAmmo = 3;
        public bool turnAroundWhileUsingSpecials = true;
        [JsonConverter(typeof(Vector2Converter))] public Vector2 specialAttackIBoost = Vector2.zero;
        [JsonConverter(typeof(StringEnumConverter))] public SpecialType specialType = SpecialType.Grenade;
        //public SpecialTypeStats specialTypeInfo = null;

        public virtual void Initialize()
        {
            SetSpecialTypeStats();
        }

        public virtual void SetSpecialTypeStats()
        {
            /*if (specialType == SpecialType.Grenade)
                specialTypeInfo = new GrenadeTypeStats();*/
        }
    }
}

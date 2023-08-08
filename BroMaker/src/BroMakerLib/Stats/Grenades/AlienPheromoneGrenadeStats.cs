using System;
using System.Collections.Generic;
using RocketLib.JsonConverters;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Stats.Grenades
{
    public class AlienPheromoneGrenadeStats : StickyGrenadeStats
    {
	    public float pheromoneDelay = 0.3f;
        public float pheromoneRate = 0.1f;
        [JsonConverter(typeof(Vector2Converter))] public Vector2 pheromoneRange = new Vector2(128f, 96f);
        public float pheromoneYOffset = 32f;
        public float smokeRateAirborn = 0.1f;
        public float smokeRateStuck = 0.034f;
        public float smokeZ = 7f;
    }
}

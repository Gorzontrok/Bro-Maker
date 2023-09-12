using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using RocketLib.JsonConverters;
using UnityEngine;

namespace BroMakerLib.Stats.SortOut
{
    public class ImpaleStats
    {
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 impaleOffset;
        public bool useImpaledFrames = false;
    }
}

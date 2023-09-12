using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Stats.SortOut
{
    public class BloodStats
    {
        [JsonConverter(typeof(StringEnumConverter))] public BloodColor bloodColor = BloodColor.Red;
        public int bloodCountAmount = 80;
        public int maxSpurtCount = 5;
    }
}

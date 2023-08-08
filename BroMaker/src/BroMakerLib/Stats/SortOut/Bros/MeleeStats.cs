using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BroMakerLib.Stats.SortOut.Bros
{
    public class MeleeStats
    {
        public bool cancelMeleeOnChangeDirection = false;
        public bool canDoIndependentMeleeAnimation = false;
        [JsonConverter(typeof(StringEnumConverter))]
        public BroBase.MeleeType meleeType = BroBase.MeleeType.Knife;
    }
}

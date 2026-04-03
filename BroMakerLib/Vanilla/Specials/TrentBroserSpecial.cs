using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TrentBroser")]
    public class TrentBroserSpecial : GrenadeThrowSpecial
    {
        public TrentBroserSpecial()
        {
            grenadeName = "AirStrike";
        }
    }
}

using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BrodellWalker")]
    public class BrodellWalkerSpecial : GrenadeThrowSpecial
    {
        public BrodellWalkerSpecial()
        {
            grenadeName = "AirStrike";
        }
    }
}

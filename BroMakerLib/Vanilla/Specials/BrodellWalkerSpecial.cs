using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BrodellWalker")]
    public class BrodellWalkerSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.BrodellWalker;

        public BrodellWalkerSpecial()
        {
            grenadeName = "AirStrike";
        }
    }
}

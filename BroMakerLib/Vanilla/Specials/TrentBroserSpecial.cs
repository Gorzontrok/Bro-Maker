using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TrentBroser")]
    public class TrentBroserSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.TrentBroser;

        public TrentBroserSpecial()
        {
            grenadeName = "AirStrike";
        }
    }
}

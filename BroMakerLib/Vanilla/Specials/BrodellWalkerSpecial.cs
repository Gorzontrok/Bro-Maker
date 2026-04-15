using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Brodell Walker's air-strike grenade.</summary>
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

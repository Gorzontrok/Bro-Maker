using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("DirtyBrory")]
    public class DirtyHarrySpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.DirtyHarry;

        public DirtyHarrySpecial()
        {
            grenadeName = "Molotove";
        }
    }
}

using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Dirty Brory's molotov cocktail throw special.</summary>
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

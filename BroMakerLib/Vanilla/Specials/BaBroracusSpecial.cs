using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>B.A. Broracus's flame-wave grenade.</summary>
    [SpecialPreset("BaBroracus")]
    public class BaBroracusSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.BaBroracus;

        public BaBroracusSpecial()
        {
            grenadeName = "FlameWave";
        }
    }
}

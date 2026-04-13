using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
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

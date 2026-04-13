using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BroHard")]
    public class BroHardSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.BroHard;

        public BroHardSpecial()
        {
            grenadeName = "FlashBang";
        }
    }
}

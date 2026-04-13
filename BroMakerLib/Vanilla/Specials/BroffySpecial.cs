using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Broffy")]
    public class BroffySpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.Broffy;

        public BroffySpecial()
        {
            grenadeName = "HolyWater";
            triggerSpecialEvent = false;
        }
    }
}

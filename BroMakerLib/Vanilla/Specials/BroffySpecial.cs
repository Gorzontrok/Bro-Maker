using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Broffy's holy water grenade.</summary>
    [SpecialPreset("Broffy")]
    public class BroffySpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.Broffy;

        public BroffySpecial()
        {
            grenade.name = "HolyWater";
            triggerSpecialEvent = false;
        }
    }
}

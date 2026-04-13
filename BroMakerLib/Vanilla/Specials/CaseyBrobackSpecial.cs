using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("CaseyBroback")]
    public class CaseyBrobackSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.CaseyBroback;

        public CaseyBrobackSpecial()
        {
            grenadeName = "Grenade";
        }
    }
}

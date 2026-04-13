using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("DemolitionBro")]
    public class DemolitionBroSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.DemolitionBro;

        public DemolitionBroSpecial()
        {
            grenadeName = "Freeze";
        }
    }
}

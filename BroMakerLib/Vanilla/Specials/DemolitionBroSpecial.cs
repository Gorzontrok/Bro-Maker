using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Demolition Bro's freeze-grenade throw special.</summary>
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

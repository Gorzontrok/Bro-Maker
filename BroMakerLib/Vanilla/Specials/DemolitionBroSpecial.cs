using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("DemolitionBro")]
    public class DemolitionBroSpecial : GrenadeThrowSpecial
    {
        public DemolitionBroSpecial()
        {
            grenadeName = "Freeze";
        }
    }
}

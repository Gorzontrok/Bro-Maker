using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Rambro")]
    public class RambroSpecial : GrenadeThrowSpecial
    {
        public RambroSpecial()
        {
            grenadeName = "Grenade";
        }
    }
}

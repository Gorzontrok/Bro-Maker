using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Rambro's grenade throw special.</summary>
    [SpecialPreset("Rambro")]
    public class RambroSpecial : GrenadeThrowSpecial
    {
        public RambroSpecial()
        {
            grenadeName = "Grenade";
        }
    }
}

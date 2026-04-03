using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("DirtyHarry")]
    public class DirtyHarrySpecial : GrenadeThrowSpecial
    {
        public DirtyHarrySpecial()
        {
            grenadeName = "Molotove";
        }
    }
}

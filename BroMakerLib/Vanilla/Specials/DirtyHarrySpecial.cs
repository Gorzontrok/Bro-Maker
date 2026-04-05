using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("DirtyBrory")]
    public class DirtyHarrySpecial : GrenadeThrowSpecial
    {
        public DirtyHarrySpecial()
        {
            grenadeName = "Molotove";
        }
    }
}

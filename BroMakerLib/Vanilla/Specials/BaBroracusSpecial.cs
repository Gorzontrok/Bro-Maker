using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BaBroracus")]
    public class BaBroracusSpecial : GrenadeThrowSpecial
    {
        public BaBroracusSpecial()
        {
            grenadeName = "FlameWave";
        }
    }
}

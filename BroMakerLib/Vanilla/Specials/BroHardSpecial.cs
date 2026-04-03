using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BroHard")]
    public class BroHardSpecial : GrenadeThrowSpecial
    {
        public BroHardSpecial()
        {
            grenadeName = "FlashBang";
        }
    }
}

using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Broffy")]
    public class BroffySpecial : GrenadeThrowSpecial
    {
        public BroffySpecial()
        {
            grenadeName = "HolyWater";
            triggerSpecialEvent = false;
        }
    }
}

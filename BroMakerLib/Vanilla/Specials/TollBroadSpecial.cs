using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TollBroad")]
    public class TollBroadSpecial : GrenadeThrowSpecial
    {
        public TollBroadSpecial()
        {
            grenadeName = "GrenadeTollBroad";
        }
    }
}

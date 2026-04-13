using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TollBroad")]
    public class TollBroadSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.TollBroad;

        public TollBroadSpecial()
        {
            grenadeName = "GrenadeTollBroad";
        }
    }
}

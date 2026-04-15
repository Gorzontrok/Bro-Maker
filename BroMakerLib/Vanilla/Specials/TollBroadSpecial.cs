using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Toll Broad's grenade throw special.</summary>
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

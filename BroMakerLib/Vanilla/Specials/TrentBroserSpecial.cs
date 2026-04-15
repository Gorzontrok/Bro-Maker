using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Trent Broser's airstrike throw special.</summary>
    [SpecialPreset("TrentBroser")]
    public class TrentBroserSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.TrentBroser;

        public TrentBroserSpecial()
        {
            grenadeName = "AirStrike";
        }
    }
}

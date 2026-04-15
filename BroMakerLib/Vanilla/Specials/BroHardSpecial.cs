using BroMakerLib.Attributes;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Bro Hard's flash-bang grenade.</summary>
    [SpecialPreset("BroHard")]
    public class BroHardSpecial : GrenadeThrowSpecial
    {
        protected override HeroType SourceBroType => HeroType.BroHard;

        public BroHardSpecial()
        {
            grenadeName = "FlashBang";
        }
    }
}

using BroMakerLib.Loggers;
using BroMakerLib.Stats.SortOut.Bros;
using Newtonsoft.Json;

namespace BroMakerLib.Stats
{
    public class BroStats : CharacterStats
    {
        public BroStats() { maxHealth = 1; }

        public float belowScreenCounter = 2f;
        public bool canChimneyFlip = false;
        /// <summary>
        /// Unimplemented
        /// </summary>
        public float holyWaterRevivePerformanceEnhancedTime = 0.02f;

        public AvatarStats avatar;
        public MeleeStats melee;
        public SpecialStats special;
        public HighFiveStats highFive;
        public ClimbingStats climbing;
        public PushingStats pushing;
        public FlexPowerStats flexPower;

        // Change Stats Type
        public new BroSoundStats sound;

        public override void Initialize()
        {
            if(sound == null)
                sound = new BroSoundStats();

            base.Initialize();

            if(avatar == null)
                avatar = new AvatarStats();
            if(melee == null)
                melee = new MeleeStats();
            if (special == null)
                special = new SpecialStats();
            if (highFive == null)
                highFive = new HighFiveStats();
            if (climbing == null)
                climbing = new ClimbingStats();
            if (pushing == null)
                pushing = new PushingStats();
            if(flexPower == null)
                flexPower = new FlexPowerStats();
        }
    }
}

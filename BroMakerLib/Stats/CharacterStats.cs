using System;
using System.IO;
using BroMakerLib.Stats.SortOut;
using Newtonsoft.Json;

namespace BroMakerLib.Stats
{
    [Serializable]
    public class CharacterStats
    {
        public bool canBeStrungUp = false;
        public bool canDisembowel = false;
        public bool canGib = true;
        public bool canLedgeGrapple = true;
        public bool canUnFreeze = false;
        /// <summary>
        /// Unimplemented
        /// </summary>
        public float gravityMultiplier = 1f;
        public float maxFallSpeed = -400f;
        public int maxHealth = 1;
        /// <summary>
        /// Unimplemented
        /// </summary>
        public float multiplierOfBouncyJumpMultiplyer = 1f;
        public float quicksandChokeCounter = 2f;
        public float reviveZombieTime = 2f;
        public float speed = 110f;
        public bool willComeBackToLife = false;
        public bool canCeilingHang = false;
        public float hangGraceTime = 0.3f;
        public bool breakDoorsOpen = false;


        public AcidStats acid;
        public AirDashStats airDash;
        public AnimationStats animation;
        public BloodStats blood;
        public CheatStats cheats;
        public DashStats dash;
        public DuckStats duck;
        public JumpStats jump;
        public ImpaleStats impale;
        public SoundStats sound;

        public virtual void Initialize()
        {
            if(acid == null) acid = new AcidStats();
            if(airDash == null) airDash = new AirDashStats();
            if(animation == null) animation = new AnimationStats();
            if(blood == null) blood = new BloodStats();
            if(cheats == null) cheats = new CheatStats();
            if(dash == null) dash = new DashStats();
            if(duck == null) duck = new DuckStats();
            if(jump == null) jump = new JumpStats();
            if(impale == null) impale = new ImpaleStats();
            try
            {
                if(sound == null) sound = new SoundStats();
            }
            catch { }
        }
    }
}

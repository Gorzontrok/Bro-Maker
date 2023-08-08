using System;
using BroMakerLib.CustomObjects.Components;
using BroMakerLib.Stats;

namespace BroMakerLib
{
    public static class BroBaseExtensions
    {
        public static void LoadStats(this BroBase bro, BroStats stats)
        {

            CharacterExtended characterExtended = bro.GetComponent<CharacterExtended>();
            if(characterExtended != null)
            {
                //characterExtended.holyWaterRevivePerformanceEnhancedTime = stats.holyWaterRevivePerformanceEnhancedTime;
                //characterExtended.flexInvulnerability = stats.flexPower.flexInvulnerability;
                characterExtended.multiplierOfBouncyJumpMultiplyer = stats.multiplierOfBouncyJumpMultiplyer;
                characterExtended.gravityMultiplier = stats.gravityMultiplier;
                characterExtended.acidMeltTimer = stats.acid.acidMeltTimer;
                characterExtended.acidParticleTimer = stats.acid.acidParticleTimer;
                characterExtended.maxSpurtCount = stats.blood.maxSpurtCount;
            }
        }
    }
}

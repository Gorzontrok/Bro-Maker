using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BroMakerLib.Stats;
using RocketLib;

namespace BroMakerLib
{
    public static class TestVanDammeAnimExtensions
    {
        public static void LoadStats(this TestVanDammeAnim character, CharacterStats stats)
        {
            character.canBeStrungUp = stats.canBeStrungUp;
            character.canDisembowel = stats.canDisembowel;
            character.canGib = stats.canGib;
            character.canLedgeGrapple = stats.canLedgeGrapple;
            character.canUnFreeze = stats.canUnFreeze;
            //character._gravityMultiplier = stats.gravityMultiplier;
            character.maxFallSpeed = stats.maxFallSpeed;
            character.maxHealth = stats.maxHealth;
            //character._multiplierOfBouncyJumpMultiplyer = stats.multiplierOfBouncyJumpMultiplyer;
            character.SetFieldValue("quicksandChokeCounter", stats.quicksandChokeCounter);
            character.SetFieldValue("reviveZombieTime", stats.reviveZombieTime);
            character.speed = stats.speed;
            character.willComeBackToLife = stats.willComeBackToLife;
            character.canCeilingHang = stats.canCeilingHang;
            character.SetFieldValue("hangGraceTime", stats.hangGraceTime);
            character.breakDoorsOpen = stats.breakDoorsOpen;
            // Acid
            character.canBeCoveredInAcid = stats.acid.canBeCoveredInAcid;
            character.SetFieldValue("meltDuration", stats.acid.meltDuration);
            //character._acidMeltTimer = stats.acid.acidMeltTimer;
            //character._acidParticleTimer = stats.acid.acidParticleTimer;
            // AirDash
            character.canAirdash = stats.airDash.canAirdash;
            character.SetFieldValue("airdashDownAvailable", stats.airDash.airdashDownAvailable);
            character.SetFieldValue("airdashLeftAvailable", stats.airDash.airdashLeftAvailable);
            character.SetFieldValue("airdashRightAvailable", stats.airDash.airdashRightAvailable);
            character.SetFieldValue("airdashUpAvailable", stats.airDash.airdashUpAvailable);
            character.airdashMaxTime = stats.airDash.airdashMaxTime;
            character.SetFieldValue("dashSpeedM",stats.airDash.dashSpeedM);
            character.SetFieldValue("defaultAirdashDelay", stats.airDash.defaultAirdashDelay);
            // Animation Stats
            character.doRollOnLand = stats.animation.doRollOnLand;
            character.useNewFrames = stats.animation.useNewFrames;
            character.useNewKnifingFrames = stats.animation.useNewKnifingFrames;
            character.useNewLadderClimbingFrames = stats.animation.useNewLadderClimbingFrames;
            character.useLadderClimbingTransition = stats.animation.useLadderClimbingTransition;
            character.useNewLedgeGrappleFrames = stats.animation.useNewLedgeGrappleFrames;
            character.useNewThrowingFrames = stats.animation.useNewThrowingFrames;
            // Blood Stats
            character.bloodColor = stats.blood.bloodColor;
            character.bloodCountAmount = stats.blood.bloodCountAmount;
            //character._maxSpurtCount = stats.blood.maxSpurtCount;
            // Cheats Stats
            if (BroMaker.areCheatsActive)
            {
                character.SetFieldValue("immuneToOutOfBounds", stats.cheats.immuneToOutOfBounds);
                //character.hide = stats.cheats.hide;
                //character.noClipSpeed = stats.cheats.noClipSpeed;
            }
            // Dash Stats
            character.canDash = stats.dash.canDash;
            character.useDashFrames = stats.dash.useDashFrames;
            character.SetFieldValue("minDashTapTime", stats.dash.minDashTapTime);
            // Dusck Stats
            character.canDuck = stats.duck.canDuck;
            character.useDuckingFrames = stats.duck.useDuckingFrames;
            character.useNewDuckingFrames = stats.duck.useNewDuckingFrames;
            // Jump Stats
            character._jumpForce = stats.jump.jumpForce;
            character.SetFieldValue("jumpTime", stats.jump.jumpTime);
            character.useAttackJumpForceForAttack = stats.jump.useAttackJumpForceForAttack;
            character.attackJumpForce = stats.jump.attackJumpForce;
            // Impale
            character.SetFieldValue("impaleXOffset", stats.impale.impaleOffset.x);
            character.SetFieldValue("impaleYOffset", stats.impale.impaleOffset.y);
            character.useImpaledFrames = stats.impale.useImpaledFrames;
            // Sound
            character.pitchShiftAmount = stats.sound.pitchShiftAmount;
            character.deathSoundVolume = stats.sound.deathSoundVolume;
        }
        public static void LoadWeaponStats(this TestVanDammeAnim character, WeaponStats stats)
        {
            character.fireDelay = stats.fireDelay;
            character.fireRate = stats.fireRate;
            character.rumbleAmountPerShot = stats.rumbleAmountPerShot;
        }
    }
}

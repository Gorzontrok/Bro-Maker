using System.Collections.Generic;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>TankBro's smash melee.</summary>
    [MeleePreset("TankBro")]
    public class TankBroMelee : SmashMelee
    {
        protected override HeroType SourceBroType => HeroType.TankBro;

        /// <summary>Horizontal impulse applied to the target on TankBro's uppercut hit.</summary>
        public float tankBroUpperCutXI = 50f;
        /// <summary>Vertical impulse applied to the target on TankBro's uppercut hit.</summary>
        public float tankBroUpperCutYI = 550f;

        public TankBroMelee()
        {
            smashExplodeRange = 48f;
            smashExplodeHeight = 12f;
            smashGroundWaveSize = 128f;
        }

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro != null)
            {
                if (alternateMeleeHitSounds2 == null) alternateMeleeHitSounds2 = sourceBro.soundHolder.alternateMeleeHitSound2.CloneArray();
                if (special3Sounds == null) special3Sounds = sourceBro.soundHolder.special3Sounds.CloneArray();
            }
        }

        protected override void SetPunchAnimationRow()
        {
            currentPunchAnimationRow = 9;
        }

        protected override bool ShouldSmash()
        {
            return owner.actionState != ActionState.ClimbingLadder;
        }

        protected override void OnAfterStartMeleeCommon()
        {
            sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            hero.HasPlayedMissSound = true;
        }

        protected override void OnAfterSmashSetup()
        {
            if (hero.MeleeChosenUnit == null)
            {
                owner.xI = owner.transform.localScale.x * 20f;
            }
        }

        protected override void AnimatePunchHitDetection()
        {
            if (owner.frame >= 3 && owner.frame < 6 && !hero.MeleeHasHit)
            {
                PerformTankBroUpperCut(true, true);
            }
        }

        protected override void AnimatePunchMissSound()
        {
            if (!hero.MeleeHasHit && !hero.HasPlayedMissSound)
            {
                hero.HasPlayedMissSound = true;
            }
        }

        protected override void SmashPerformAttackCheck()
        {
            if (owner.frame == 4 && !hero.MeleeHasHit)
            {
                PerformSmashAttack();
            }
        }

        private void PerformTankBroUpperCut(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(24f);
            List<Unit> list = new List<Unit>();
            if (Map.HitUnits(owner, PlayerNum, upperCutDamage, DamageType.Melee, 6f, X + Direction * 8f, Y + 8f, owner.xI + Direction * tankBroUpperCutXI, owner.yI + tankBroUpperCutYI, false, false, false, list, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds2, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
            else
            {
                owner.Y += 6f;
                if (shouldTryHitTerrain && TryMeleeTerrain(0, 2))
                {
                    hero.MeleeHasHit = true;
                }
                owner.Y -= 6f;
            }
        }

        protected override void MakeSmashBlast(float xPoint, float yPoint, bool groundWave)
        {
            Map.ExplodeUnits(owner, 10, DamageType.Crush, smashExplodeRange, smashExplodeHeight, xPoint, yPoint, smashExplodeXI, smashExplodeYI, PlayerNum, false, false, true);
            MapController.DamageGround(owner, 15, DamageType.Explosion, smashGroundDamageRange, xPoint, yPoint, null, false);
            EffectsController.CreateWhiteFlashPop(xPoint, yPoint);
            sound.PlaySoundEffectAt(special3Sounds, 0.25f, owner.transform.position, 1f, true, false, false, 0f);
            if (groundWave)
            {
                float headHeight = owner.headHeight;
                EffectsController.CreateGroundWave(xPoint, yPoint + headHeight, smashGroundWaveSize);
                Map.ShakeTrees(X, Y, 64f, 32f, 64f);
            }
            Map.DisturbWildLife(X, Y, 48f, PlayerNum);
        }
    }
}

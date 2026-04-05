using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("Brominator")]
    public class BrominatorMelee : SmashMelee
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var brominator = owner as Brominator;
            if (brominator == null)
            {
                brominator = HeroController.GetHeroPrefab(HeroType.Brominator) as Brominator;
            }
            if (brominator != null)
            {
                soundHolder.alternateMeleeHitSound = brominator.soundHolder.alternateMeleeHitSound;
                soundHolder.missSounds = brominator.soundHolder.missSounds;
                soundHolder.alternateMeleeMissSound = brominator.soundHolder.alternateMeleeMissSound;
            }
        }

        protected override void SetPunchAnimationRow()
        {
            if (Random.value > 0.5f)
            {
                currentPunchAnimationRow = 9;
            }
            else
            {
                currentPunchAnimationRow = 10;
            }
        }

        protected override bool ShouldSmash()
        {
            return owner.actionState != ActionState.ClimbingLadder;
        }

        protected override void SmashPerformAttackCheck()
        {
            if (owner.frame == 4 && !hero.MeleeHasHit && !owner.IsOnGround())
            {
                PerformSmashAttack();
            }
        }

        protected override void AnimatePunchHitDetection()
        {
            if (owner.frame == 3 && !hero.MeleeHasHit)
            {
                PerformUpperCut(true, true);
            }
        }

        protected override void AnimatePunchMissSound()
        {
            if (owner.frame == 3 && !hero.MeleeHasHit && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
            {
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeMissSound, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
        }

        protected override void OnUpperCutMissSound()
        {
            sound.PlaySoundEffectAt(soundHolder.missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
        }

        protected override void OnUpperCutTerrainHit()
        {
            sound.PlaySoundEffectAt(soundHolder.alternateMeleeMissSound, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
        }
    }

    [MeleePreset("TankBro")]
    public class TankBroMelee : SmashMelee
    {
        public float tankBroUpperCutXI = 50f;
        public float tankBroUpperCutYI = 550f;

        public TankBroMelee()
        {
            smashExplodeRange = 48f;
            smashExplodeHeight = 12f;
            smashGroundWaveSize = 128f;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var tankBro = owner as TankBro;
            if (tankBro == null)
            {
                tankBro = HeroController.GetHeroPrefab(HeroType.TankBro) as TankBro;
            }
            if (tankBro != null)
            {
                soundHolder.alternateMeleeHitSound = tankBro.soundHolder.alternateMeleeHitSound;
                soundHolder.alternateMeleeHitSound2 = tankBro.soundHolder.alternateMeleeHitSound2;
                soundHolder.alternateMeleeMissSound = tankBro.soundHolder.alternateMeleeMissSound;
                soundHolder.missSounds = tankBro.soundHolder.missSounds;
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
            sound.PlaySoundEffectAt(soundHolder.missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            owner.SetFieldValue("hasPlayedMissSound", true);
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
            if (!hero.MeleeHasHit && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
            {
                owner.SetFieldValue("hasPlayedMissSound", true);
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
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound2, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
            else
            {
                owner.Y += 6f;
                if (shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
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
            owner.PlaySpecial3Sound(0.25f);
            if (groundWave)
            {
                float headHeight = owner.GetFieldValue<float>("headHeight");
                EffectsController.CreateGroundWave(xPoint, yPoint + headHeight, smashGroundWaveSize);
                Map.ShakeTrees(X, Y, 64f, 32f, 64f);
            }
            Map.DisturbWildLife(X, Y, 48f, PlayerNum);
        }
    }
}

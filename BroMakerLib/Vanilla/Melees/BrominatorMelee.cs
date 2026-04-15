using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Brominator's smash melee.</summary>
    [MeleePreset("Brominator")]
    public class BrominatorMelee : SmashMelee
    {
        protected override HeroType SourceBroType => HeroType.Brominator;


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
            if (owner.frame == 3 && !hero.MeleeHasHit && !hero.HasPlayedMissSound)
            {
                sound.PlaySoundEffectAt(alternateMeleeMissSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                hero.HasPlayedMissSound = true;
            }
        }

        protected override void OnUpperCutMissSound()
        {
            sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
        }

        protected override void OnUpperCutTerrainHit()
        {
            sound.PlaySoundEffectAt(alternateMeleeMissSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
        }
    }
}

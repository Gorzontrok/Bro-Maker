using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using BroMakerLib.Vanilla.Specials;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Desperabro's knife melee.</summary>
    [MeleePreset("Desperabro")]
    public class DesperabroMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.Desperabro;

        public DesperabroMelee()
        {
            meleeType = BroBase.MeleeType.Knife;
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (meleeHitSounds == null) meleeHitSounds = sourceBro.soundHolder.meleeHitSound.CloneArray();
            if (missSounds == null) missSounds = sourceBro.soundHolder.missSounds.CloneArray();
            if (meleeHitTerrainSounds == null) meleeHitTerrainSounds = sourceBro.soundHolder.meleeHitTerrainSound.CloneArray();
        }

        public override void StartMelee()
        {
            var desperabro = owner as Desperabro;
            if (desperabro != null && desperabro.mariachiBroType != Desperabro.MariachiBroType.Desperabro)
            {
                return;
            }
            if (desperabro != null && desperabro.GetFieldValue<bool>("isSerenading"))
            {
                desperabro.CallMethod("FinishSerenadingAndUnleashHell");
                return;
            }
            var desperabroSpecial = hero != null ? hero.SpecialAbility as DesperabroSpecial : null;
            if (desperabroSpecial != null && desperabroSpecial.IsSerenading)
            {
                desperabroSpecial.CallMethod("FinishSerenadingAndUnleashHell");
                return;
            }
            sound.PlaySoundEffectAt(missSounds, 0.7f, owner.transform.position, 1f, true, false, false, 0f);
            hero.ShowHighFiveAfterMeleeTimer = 0f;
            hero.JumpTime = 0f;
            hero.DeactivateGun();
            hero.SetMeleeType();
            hero.MeleeHasHit = false;
            if (!hero.DoingMelee || owner.frame > 3)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else if (hero.DoingMelee)
            {
                hero.MeleeFollowUp = true;
            }
            hero.DoingMelee = true;
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            if (owner.frame >= 2 && owner.frame <= 3)
            {
                hero.FrameRate = 0.0125f;
            }
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 6);
            int num2 = animationRow;
            if (!hero.StandingMelee)
            {
                if (hero.JumpingMelee)
                {
                    num = jumpingAnimationColumn + Mathf.Clamp(owner.frame, 0, 6);
                    num2 = jumpingAnimationRow;
                }
                else if (hero.DashingMelee)
                {
                    num = jumpingAnimationColumn + Mathf.Clamp(owner.frame, 0, 6);
                    num2 = jumpingAnimationRow;
                    if (owner.frame == 4)
                    {
                        owner.counter -= 0.0334f;
                    }
                    else if (owner.frame == 5)
                    {
                        owner.counter -= 0.0334f;
                    }
                }
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3)
            {
                owner.counter -= 0.066f;
                PerformKnifeMeleeAttack(true, false);
            }
            else if (owner.frame > 3 && !hero.MeleeHasHit)
            {
                PerformKnifeMeleeAttack(false, false);
            }
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformKnifeMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + (float)(Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(24f);
            if (Map.HitClosestUnit(owner, PlayerNum, 4, DamageType.Knifed, 14f, 24f, X + owner.transform.localScale.x * 8f, Y + 8f, owner.transform.localScale.x * 200f, 500f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(meleeHitSounds, 0.7f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(missSounds, 0.7f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

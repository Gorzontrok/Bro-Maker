using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Broffy's knife melee.</summary>
    [MeleePreset("Broffy")]
    public class BroffyMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.Broffy;

        public BroffyMelee()
        {
            meleeType = BroBase.MeleeType.Knife;
            damageType = "SilencedBullet";
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (meleeHitSounds == null) meleeHitSounds = sourceBro.soundHolder.meleeHitSound.CloneArray();
            if (missSounds == null) missSounds = sourceBro.soundHolder.missSounds.CloneArray();
            if (meleeHitTerrainSounds == null) meleeHitTerrainSounds = sourceBro.soundHolder.meleeHitTerrainSound.CloneArray();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 6);
            int num2 = animationRow;
            if (!hero.StandingMelee)
            {
                if (hero.JumpingMelee)
                {
                    num = jumpingAnimationColumn + Mathf.Clamp(owner.frame, 0, 9);
                    num2 = jumpingAnimationRow;
                }
                else if (hero.DashingMelee)
                {
                    num = jumpingAnimationColumn + Mathf.Clamp(owner.frame, 0, 9);
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
            int hitFrame = (!hero.DashingMelee) ? 3 : 5;
            if (owner.frame == hitFrame)
            {
                owner.counter -= 0.066f;
                PerformKnifeMeleeAttack(true, true);
            }
            else if (owner.frame > hitFrame && !hero.MeleeHasHit)
            {
                PerformKnifeMeleeAttack(false, false);
            }
            int frameCount = (!hero.DashingMelee) ? 6 : 9;
            if (owner.frame >= frameCount)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformKnifeMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            DamageType damageType = (!hero.DashingMelee) ? parsedDamageType : DamageType.Melee;
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + (float)(Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(24f);
            bool applyLaunch = false;
            float launchX = 0f;
            float launchY = 0f;
            if (hero.DashingMelee)
            {
                applyLaunch = true;
                launchX = (float)(Direction * 300);
                launchY = 240f;
            }
            if (Map.HitClosestUnit(owner, PlayerNum, 4, damageType, 14f, 24f, X + owner.transform.localScale.x * 8f, Y + 8f, launchX, launchY, applyLaunch, true, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(meleeHitSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && TryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }

        public override void RunMeleeMovement()
        {
            if (!owner.useNewKnifingFrames)
            {
                if (owner.Y > owner.groundHeight + 1f)
                {
                    hero.ApplyFallingGravity();
                }
            }
            else if (hero.JumpingMelee)
            {
                hero.ApplyFallingGravity();
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame <= 3)
                {
                    if (hero.MeleeChosenUnit == null)
                    {
                        if (!hero.IsInQuicksand)
                        {
                            owner.xI = owner.speed * 1f * owner.transform.localScale.x;
                        }
                        owner.yI = 0f;
                    }
                    else if (!hero.IsInQuicksand)
                    {
                        owner.xI = owner.speed * 0.5f * owner.transform.localScale.x + (hero.MeleeChosenUnit.X - owner.X) * 6f;
                    }
                }
                else if (owner.frame <= 5)
                {
                    if (!hero.IsInQuicksand)
                    {
                        owner.xI = owner.speed * 0.3f * owner.transform.localScale.x;
                    }
                    hero.ApplyFallingGravity();
                }
                else
                {
                    hero.ApplyFallingGravity();
                }
            }
            else if (owner.Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }
    }
}

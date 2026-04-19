using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Bro Dredd's taser melee.</summary>
    [MeleePreset("BroDredd")]
    public class BroDreddMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.BroDredd;

        public AudioClip[] alternateMeleeHitSounds2;

        [JsonIgnore]
        private int shockFrameCounter;
        [JsonIgnore]
        private Unit previouslyTasedUnit;
        [JsonIgnore]
        private int tasedCount;

        public BroDreddMelee()
        {
            meleeType = BroBase.MeleeType.Tazer;
            startType = MeleeStartType.Custom;
            restartFrame = 0;
            animationRow = 9;
            jumpingAnimationRow = 10;
            damageType = "Shock";
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (alternateMeleeHitSounds2 == null) alternateMeleeHitSounds2 = sourceBro.soundHolder.alternateMeleeHitSound.CloneArray();
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 2)
            {
                shockFrameCounter = 0;
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else
            {
                hero.MeleeFollowUp = true;
            }
            hero.StartMeleeCommon();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animationRow;
            if (hero.StandingMelee)
            {
                num = animationRow;
            }
            else if (hero.JumpingMelee)
            {
                num = jumpingAnimationRow;
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame > 5)
                {
                    num = animationRow;
                }
                else
                {
                    num = jumpingAnimationRow;
                }
            }
            if (owner.frame == 3 || owner.frame == 4)
            {
                owner.counter -= 0.033f;
            }
            if (owner.frame > 6)
            {
                shockFrameCounter++;
                owner.frame = 5;
            }
            if (owner.frame >= 3)
            {
                EffectsController.CreateSparkParticle(X + owner.transform.localScale.x * 10f, Y + 11f, 0f, 1.5f, 0f, 0f, 0f, Color.Lerp(Color.cyan, Color.white, Random.value), Random.Range(0.1f, 0.4f));
            }
            int num2 = animationColumn + owner.frame;
            hero.Sprite.SetLowerLeftPixel((float)(num2 * hero.SpritePixelWidth), (float)(num * hero.SpritePixelHeight));
            if (owner.frame > 2 && owner.frame % 2 == 1)
            {
                PerformTazerMeleeAttack(true, false);
                hero.PerformedMeleeAttack = true;
                sound.PlaySoundEffectAt(alternateMeleeHitSounds2, 0.35f, owner.transform.position, Random.Range(0.9f, 1.1f), true, false, false, 0f);
            }
            if (shockFrameCounter > 5 && !owner.buttonHighFive)
            {
                hero.CancelMelee();
            }
        }

        public override void RunMeleeMovement()
        {
            hero.ApplyFallingGravity();
            if (owner.yI < owner.maxFallSpeed)
            {
                owner.yI = owner.maxFallSpeed;
            }
            if (hero.JumpingMelee)
            {
                if (owner.IsOnGround())
                {
                    hero.JumpingMelee = false;
                }
            }
            else if (hero.DashingMelee)
            {
                if (hero.MeleeChosenUnit != null)
                {
                    if (owner.frame < 2)
                    {
                        owner.xI = 0f;
                        owner.yI = 0f;
                    }
                    else if (owner.frame < 5)
                    {
                        float num = hero.MeleeChosenUnit.X - (float)owner.Direction * 8f - owner.X;
                        owner.xI = num / 0.1f;
                        owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                    }
                    else
                    {
                        owner.xI = 0f;
                    }
                }
                else if (hero.MeleeChosenUnit == null)
                {
                    if (owner.frame < 2)
                    {
                        owner.xI = 0f;
                        owner.yI = 0f;
                    }
                    else if (owner.frame < 5)
                    {
                        owner.xI = owner.speed * (float)owner.Direction;
                        owner.yI = 0f;
                    }
                    else
                    {
                        owner.xI = 0f;
                    }
                }
            }
            else
            {
                owner.xI = 0f;
                if (owner.Y > owner.groundHeight + 1f)
                {
                    hero.CancelMelee();
                }
            }
        }

        public override void HandleAfterCheckInput()
        {
            if (owner.left || owner.right)
            {
                tasedCount = 0;
            }
        }

        private void PerformTazerMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Shock, X + (float)(owner.Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            Unit unit = Map.HitClosestUnit(owner, PlayerNum, 1, parsedDamageType, 13f, 24f, X + owner.transform.localScale.x * 4f, Y + 8f, owner.transform.localScale.x * 0f, 0f, false, true, owner.IsMine, false, true);
            if (unit != null)
            {
                hero.MeleeHasHit = true;
                if (unit == previouslyTasedUnit)
                {
                    tasedCount++;
                    if (tasedCount > 12)
                    {
                        unit.Damage(tasedCount / 12, DamageType.Plasma, 0f, 0f, owner.Direction, owner, unit.X, unit.Y + 5f);
                    }
                }
                else
                {
                    tasedCount = 0;
                    previouslyTasedUnit = unit;
                }
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds2, 0.3f, owner.transform.position, Random.Range(0.9f, 1.1f), true, false, false, 0f);
            }
            if (shouldTryHitTerrain && TryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

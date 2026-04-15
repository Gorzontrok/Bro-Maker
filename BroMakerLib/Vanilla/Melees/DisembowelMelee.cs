using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Shared base for disembowel melee. Extended by: BrocheteMelee, SnakeBroskinMelee.</summary>
    public abstract class DisembowelMelee : MeleeAbility
    {
        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 4)
            {
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

        public override void RunMeleeMovement()
        {
            hero.ApplyFallingGravity();
            if (hero.JumpingMelee)
            {
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame < 2)
                {
                    owner.xI = 0f;
                    owner.yI = 0f;
                }
                else if (owner.frame <= 4)
                {
                    if (hero.MeleeChosenUnit != null)
                    {
                        float num = GetDisembowelOffset();
                        float num2 = hero.MeleeChosenUnit.X - (float)owner.Direction * num - owner.X;
                        owner.xI = num2 / 0.1f;
                        owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                    }
                    else
                    {
                        owner.xI = owner.speed * (float)owner.Direction;
                        owner.yI = 0f;
                    }
                }
                else if (owner.frame <= 7)
                {
                    owner.xI = 0f;
                }
            }
            else if (owner.Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }

        /// <summary>Returns the X offset used when homing toward the chosen unit during frames 2-4.</summary>
        protected virtual float GetDisembowelOffset()
        {
            return 8f;
        }
    }

    /// <summary>Brochete's disembowel melee.</summary>
    [MeleePreset("Brochete")]
    public class BrocheteMelee : DisembowelMelee
    {
        protected override HeroType SourceBroType => HeroType.Brochete;

        /// <summary>Sprite sheet column for the start of the disembowel animation.</summary>
        public int animColumn = 25;

        /// <summary>Sprite sheet row for the disembowel animation.</summary>
        public int animRow = 9;

        public AudioClip[] alternateMeleeHitSounds2;

        [JsonIgnore]
        private SpriteSM disembowelmentViscera;

        public BrocheteMelee()
        {
            meleeType = BroBase.MeleeType.Disembowel;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as Brochete;
            if (sourceBro != null)
            {
                disembowelmentViscera = sourceBro.disembowelmentViscera;
            }
        }

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro != null)
            {
                if (alternateMeleeHitSounds2 == null) alternateMeleeHitSounds2 = sourceBro.soundHolder.alternateMeleeHitSound2.CloneArray();
            }
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animColumn + Mathf.Clamp(owner.frame, 0, 7);
            int num2 = animRow;
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame >= 4)
            {
                PerformDisembowelAttack(true, true);
                if (hero.HighFive && !hero.JumpingMelee && hero.MeleeChosenUnit != null)
                {
                    owner.frame = 3;
                }
            }
            if (owner.frame == 3)
            {
                if (hero.MeleeChosenUnit != null)
                {
                    owner.counter -= 0.1f;
                }
                else
                {
                    owner.counter -= 0.066f;
                }
            }
            if (owner.frame == 4)
            {
                owner.counter -= 0.066f;
                if (hero.MeleeChosenUnit != null && hero.MeleeChosenUnit.canDisembowel)
                {
                    sound.PlaySoundEffectAt(alternateMeleeHitSounds2, 1f, owner.transform.position, 1f, true, false, false, 0f);
                    SpriteSM spriteSM = UnityEngine.Object.Instantiate<SpriteSM>(disembowelmentViscera);
                    spriteSM.transform.localPosition = hero.MeleeChosenUnit.transform.position;
                    spriteSM.transform.localScale = hero.MeleeChosenUnit.transform.localScale;
                    hero.MeleeChosenUnit = null;
                }
            }
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        public override void RunMeleeMovement()
        {
            if (hero.JumpingMelee)
            {
                hero.ApplyFallingGravity();
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            if (!hero.JumpingMelee)
            {
                if (hero.MeleeChosenUnit != null)
                {
                    if (owner.frame < 2)
                    {
                        owner.xI = 0f;
                        owner.yI = 0f;
                    }
                    else if (owner.frame <= 4)
                    {
                        float num = (hero.MeleeChosenUnit.transform.position - Vector3.right * (float)owner.Direction * 12f).x - owner.X;
                        owner.xI = num / 0.1f;
                        owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                    }
                    else if (owner.frame <= 7)
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
                    else if (owner.frame <= 3)
                    {
                        owner.xI = owner.speed * 2f * owner.transform.localScale.x;
                    }
                    else if (owner.frame <= 7)
                    {
                        owner.xI = 0f;
                    }
                }
                hero.ApplyFallingGravity();
            }
            if (!hero.JumpingMelee && !hero.DashingMelee && hero.MeleeChosenUnit == null)
            {
                hero.ApplyFallingGravity();
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
                owner.xI = 0f;
                if (owner.Y > owner.groundHeight + 1f)
                {
                    hero.CancelMelee();
                }
            }
        }

        private void PerformDisembowelAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            if (hero.MeleeChosenUnit != null)
            {
                if (hero.MeleeChosenUnit.CanDisembowel)
                {
                    hero.MeleeChosenUnit.ForceFaceDirection(-owner.Direction);
                    hero.MeleeChosenUnit.Damage(25, DamageType.Disembowel, 0f, 0f, owner.Direction, owner, owner.X, owner.Y);
                }
                else
                {
                    hero.MeleeChosenUnit.Damage(4, DamageType.Melee, 0f, 0f, owner.Direction, owner, owner.X, owner.Y);
                }
                if (!hero.MeleeHasHit)
                {
                    EffectsController.CreateProjectilePopWhiteEffect(hero.MeleeChosenUnit.X - 8f * owner.transform.localScale.x, owner.Y + owner.height);
                }
                Map.PanicUnits(owner.X, owner.Y, 84f, 24f, 2f, true, false);
                if (!hero.MeleeHasHit)
                {
                    sound.PlaySoundEffectAt(alternateMeleeHitSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.MeleeHasHit = true;
            }
            else
            {
                if (!hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            if (shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

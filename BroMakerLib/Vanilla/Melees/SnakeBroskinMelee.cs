using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("SnakeBroskin")]
    public class SnakeBroskinMelee : DisembowelMelee
    {
        protected override HeroType SourceBroType => HeroType.SnakeBroSkin;

        public SnakeBroskinMelee()
        {
            meleeType = BroBase.MeleeType.Custom;
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            hero.FrameRate = 0.025f;
            int num = 24 + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = 10;
            if (owner.frame == 2)
            {
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3 && !hero.MeleeHasHit)
            {
                PerformBaseBallBatHit();
            }
            if (owner.frame >= 8)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
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
                        float num = 8f;
                        if (owner.GetFieldValue<BroBase.MeleeType>("meleeType") == BroBase.MeleeType.Disembowel)
                        {
                            num = 14f;
                        }
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

        private void PerformBaseBallBatHit()
        {
            float num = 8f;
            Vector3 vector = new Vector3(owner.X + (float)owner.Direction * (num + 7f), owner.Y + 10f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(25f);
            if (Map.HitClosestUnit(owner, owner.playerNum, 5, DamageType.Melee, num + 4f, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 450f, 220f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.9f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(owner.X + (owner.width + 10f) * owner.transform.localScale.x, owner.Y + owner.height + 4f);
            }
            else
            {
                if (!hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

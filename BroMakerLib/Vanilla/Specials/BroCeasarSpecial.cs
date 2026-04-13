using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BroCeasar")]
    public class BroCeasarSpecial : ShockwaveSpecial
    {
        [JsonIgnore]
        private float specialShootBoostTime;

        protected override HeroType SourceBroType => HeroType.Broc;

        public BroCeasarSpecial()
        {
            lethal = false;
        }

        public override void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.UsingSpecial || owner.health <= 0)
            {
                return;
            }
            if (owner.SpecialAmmo > 0)
            {
                MapController.DamageGround(owner, groundDamage, DamageType.Normal, groundDamageRange, X, Y, null, false);
                EffectsController.CreateGroundWave(X, Y, 2f);
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                if (!hero.UsingSpecial)
                {
                    specialAttackDirection = owner.transform.localScale.x;
                }
                hero.UsingSpecial = true;
                owner.frame = 0;
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                if (owner.actionState == ActionState.ClimbingLadder)
                {
                    owner.actionState = ActionState.Idle;
                }
                specialShootBoostTime = 0.4f;
                owner.xI = owner.transform.localScale.x * owner.speed;
                owner.dashing = true;
                owner.yI = 230f;
                owner.xIBlast = owner.transform.localScale.x * 140f;
                readyForBlast = false;
                setupBlastReadiness = false;
                hero.ChangeFrame();
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override bool HandleRunGun()
        {
            if (hero.UsingSpecial && specialShootBoostTime > 0f)
            {
                specialShootBoostTime -= hero.DeltaTime;
                owner.fire = true;
                owner.wasFire = true;
                owner.fireDelay = 0f;
            }
            else if (hero.UsingSpecial)
            {
                owner.fire = false;
                owner.wasFire = false;
            }
            return true;
        }

        public override void HandleAfterCheckInput()
        {
            base.HandleAfterCheckInput();
            if (hero.UsingSpecial && owner.transform.localScale.x > 0f)
            {
                owner.left = false;
                owner.right = true;
            }
            else if (hero.UsingSpecial && owner.transform.localScale.x < 0f)
            {
                owner.right = false;
                owner.left = true;
            }
        }

        public override void HandleAfterAddSpeedLeft()
        {
            if (owner.xIBlast > owner.speed * 1.6f)
            {
                owner.xIBlast = owner.speed * 1.6f;
            }
        }

        public override void HandleAfterAddSpeedRight()
        {
            if (owner.xIBlast < owner.speed * -1.6f)
            {
                owner.xIBlast = owner.speed * -1.6f;
            }
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = 0.0334f;
            int column = Mathf.Clamp(owner.frame, 0, 8);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, hero.SpritePixelHeight * 8);
            if (owner.frame == 0)
            {
                hero.FrameRate = 0.18f;
            }
            else
            {
                hero.FrameRate = 0.034f;
                if (owner.frame == 3)
                {
                    hero.FrameRate = 0.1f;
                }
                hero.CreateFaderTrailInstance();
            }
            if (owner.frame == useSpecialAttackFrame)
            {
                if (!setupBlastReadiness)
                {
                    readyForBlast = true;
                    setupBlastReadiness = true;
                }
                if (readyForBlast)
                {
                    owner.frame -= 2;
                }
                else
                {
                    owner.counter = -0.06f;
                }
            }
            if (owner.frame == 8)
            {
                owner.counter -= 0.15f;
            }
            if (owner.frame >= 10)
            {
                hero.GunFrame = 0;
                owner.frame = 0;
                hero.ActivateGun();
                hero.UsingSpecial = false;
                hero.ChangeFrame();
                currentStampDelay = 0f;
            }
        }

    }
}

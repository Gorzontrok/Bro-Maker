using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using System.Collections.Generic;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Brobocop's data-stab melee.</summary>
    [MeleePreset("Brobocop")]
    public class BrobocopMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.Brobocop;

        public BrobocopMelee()
        {
            meleeType = BroBase.MeleeType.BrobocopPunch;
            startType = MeleeStartType.Custom;
            restartFrame = 0;
            animationRow = 9;
            jumpingAnimationRow = 10;
            damageType = "SilencedBullet";
        }

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
        }

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

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = animationRow;
            if (hero.JumpingMelee)
            {
                num2 = jumpingAnimationRow;
            }
            if (owner.frame == 3)
            {
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 4 && !hero.MeleeHasHit)
            {
                PerformBroboCopPunchAttack(true, true);
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
                    else if (owner.frame <= 5)
                    {
                        float num = (hero.MeleeChosenUnit.transform.position - Vector3.right * (float)owner.Direction * 12f).x - owner.X;
                        owner.xI = num / 0.08f;
                        owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.8f, owner.speed * 1.8f);
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
                    else if (owner.frame <= 5)
                    {
                        owner.xI = owner.speed * 1.7f * owner.transform.localScale.x;
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

        private void PerformBroboCopPunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 6f;
            Vector3 vector = new Vector3(X + (float)owner.Direction * (num + 7f), Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(24f);
            List<Unit> list = new List<Unit>();
            Unit firstUnit = Map.GetFirstUnit(owner, PlayerNum, num, vector.x, vector.y, true, true, list);
            if (firstUnit)
            {
                firstUnit.Damage(4, parsedDamageType, owner.xI, owner.yI, (int)Mathf.Sign(owner.xI), owner, X, Y);
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else
            {
                if (playMissSound && !hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && TryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("BroDredd")]
    public class BroDreddMelee : MeleeAbility
    {
        [JsonIgnore]
        private AudioClip[] alternateMeleeHitSounds2;

        [JsonIgnore]
        private int shockFrameCounter;
        [JsonIgnore]
        private Unit previouslyTasedUnit;
        [JsonIgnore]
        private int tasedCount;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var broDredd = owner as BroDredd;
            if (broDredd == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BroDredd);
                broDredd = prefab as BroDredd;
            }
            if (broDredd != null)
            {
                alternateMeleeHitSounds2 = broDredd.soundHolder.alternateMeleeHitSound;
            }
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
            int num = 9;
            if (hero.StandingMelee)
            {
                num = 9;
            }
            else if (hero.JumpingMelee)
            {
                num = 10;
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame > 5)
                {
                    num = 9;
                }
                else
                {
                    num = 10;
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
            int num2 = 25 + owner.frame;
            hero.Sprite.SetLowerLeftPixel((float)(num2 * hero.SpritePixelWidth), (float)(num * hero.SpritePixelHeight));
            if (owner.frame > 2 && owner.frame % 2 == 1)
            {
                PerformTazerMeleeAttack(true, false);
                owner.SetFieldValue("performedMeleeAttack", true);
                sound.PlaySoundEffectAt(alternateMeleeHitSounds2, 0.35f, owner.transform.position, Random.Range(0.9f, 1.1f), true, false, false, 0f);
            }
            if (shockFrameCounter > 5 && !owner.buttonHighFive)
            {
                hero.CancelMelee();
            }
        }

        public override void RunMeleeMovement()
        {
            owner.CallMethod("ApplyFallingGravity");
            if (owner.yI < owner.maxFallSpeed)
            {
                owner.yI = owner.maxFallSpeed;
            }
            if (hero.JumpingMelee)
            {
                if (owner.IsOnGround())
                {
                    owner.SetFieldValue("jumpingMelee", false);
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

        public override void Update()
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
            Unit unit = Map.HitClosestUnit(owner, PlayerNum, 1, DamageType.Shock, 13f, 24f, X + owner.transform.localScale.x * 4f, Y + 8f, owner.transform.localScale.x * 0f, 0f, false, true, owner.IsMine, false, true);
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
            if (shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

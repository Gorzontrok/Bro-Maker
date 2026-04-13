using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("BroveHeart")]
    public class BroveHeartMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.BroveHeart;
        public float sliceVolume = 0.7f;
        public float wallHitVolume = 0.6f;
        public int groundSwordDamage = 10;
        public int enemySwordDamage = 8;

        [JsonIgnore] private List<Unit> alreadyHit = new List<Unit>();
        [JsonIgnore] private bool throwingSword;
        [JsonIgnore] private bool disarmed;
        [JsonIgnore] private Projectile thrownSword;
        [JsonIgnore] private float savedCounter;

        [JsonIgnore] private Shrapnel shrapnelSpark;
        [JsonIgnore] private FlickerFader hitPuff;
        [JsonIgnore] private BroveheartSword swordPrefab;

        public BroveHeartMelee()
        {
            meleeType = BroBase.MeleeType.Punch;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as BroveHeart;
            if (sourceBro != null)
            {
                shrapnelSpark = sourceBro.shrapnelSpark;
                hitPuff = sourceBro.hitPuff;
                swordPrefab = sourceBro.swordPrefab;
                groundSwordDamage = sourceBro.groundSwordDamage;
                enemySwordDamage = sourceBro.enemySwordDamage;
                sliceVolume = sourceBro.sliceVolume;
                wallHitVolume = sourceBro.wallHitVolume;
            }
        }

        public override void StartMelee()
        {
            if (hero.DoingMelee)
            {
                owner.counter = savedCounter;
                return;
            }
            bool isDisarmed = GetDisarmed();
            if (isDisarmed)
            {
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
            else
            {
                hero.ShowHighFiveAfterMeleeTimer = 0f;
                hero.DoingMelee = true;
                hero.MeleeHasHit = false;
                if (owner.frame > 3)
                {
                    owner.frame = 0;
                    owner.counter = -0.05f;
                    AnimateMelee();
                }
                else if (hero.DoingMelee)
                {
                    hero.MeleeFollowUp = true;
                }
                if (!owner.IsOnGround())
                {
                    hero.JumpingMelee = true;
                }
            }
        }

        public override void AnimateMelee()
        {
            bool isDisarmed = GetDisarmed();
            if (isDisarmed && !throwingSword)
            {
                hero.AnimateMeleeCommon();
                int num = 25 + Mathf.Clamp(owner.frame, 0, 6);
                int num2 = 1;
                if (!hero.StandingMelee)
                {
                    if (hero.JumpingMelee)
                    {
                        num = 17 + Mathf.Clamp(owner.frame, 0, 6);
                        num2 = 6;
                    }
                    else if (hero.DashingMelee)
                    {
                        num = 17 + Mathf.Clamp(owner.frame, 0, 6);
                        num2 = 6;
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
                hero.SetSpriteOffset(0f, 0f);
                hero.RollingFrames = 0;
                if (owner.frame == 3)
                {
                    owner.counter -= 0.066f;
                    PerformKnifeMeleeAttack(true, true);
                }
                else if (owner.frame > 3 && !hero.MeleeHasHit)
                {
                    PerformKnifeMeleeAttack(false, false);
                }
                if (owner.frame >= 6)
                {
                    owner.frame = 0;
                    hero.CancelMelee();
                }
            }
            else
            {
                owner.gunSprite.gameObject.SetActive(true);
                hero.SetSpriteOffset(0f, 0f);
                hero.RollingFrames = 0;
                if (owner.frame == 1)
                {
                    owner.counter -= 0.0334f;
                }
                hero.SetGunSprite(9 + owner.frame, 0);
                hero.FrameRate = 0.025f;
                if (hero.StandingMelee)
                {
                    int num = 25 + Mathf.Clamp(owner.frame, 0, 6);
                    hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)hero.SpritePixelHeight);
                }
                else if (hero.JumpingMelee)
                {
                    int num2 = 17 + Mathf.Clamp(owner.frame, 0, 6);
                    hero.Sprite.SetLowerLeftPixel((float)(num2 * hero.SpritePixelWidth), (float)(hero.SpritePixelHeight * 6));
                }
                else if (hero.DashingMelee)
                {
                    int num3 = 17 + Mathf.Clamp(owner.frame, 0, 6);
                    hero.Sprite.SetLowerLeftPixel((float)(num3 * hero.SpritePixelWidth), (float)(hero.SpritePixelHeight * 6));
                    if (owner.frame == 4)
                    {
                        owner.counter -= 0.0334f;
                    }
                    else if (owner.frame == 5)
                    {
                        owner.counter -= 0.0334f;
                    }
                }
                if (owner.frame == 4 && owner.IsMine)
                {
                    float num4 = (!owner.down) ? 270f : 50f;
                    thrownSword = ProjectileController.SpawnProjectileOverNetwork(swordPrefab, owner, owner.X + owner.transform.localScale.x * 6f, owner.Y + 15f, owner.transform.localScale.x * num4, (!owner.down) ? 0f : (-25f), false, owner.playerNum, false, false, 0f);
                    throwingSword = true;
                }
                if (owner.frame >= 6)
                {
                    owner.frame = 0;
                    throwingSword = false;
                    SetDisarmed(true);
                    hero.CancelMelee();
                }
                if (owner.frame == 2 && hero.NearbyMook != null && hero.NearbyMook.CanBeThrown() && hero.HighFive)
                {
                    hero.CancelMelee();
                    hero.ThrowBackMook(hero.NearbyMook);
                    hero.NearbyMook = null;
                    throwingSword = false;
                }
                if (owner.frame == 2 && owner.buttonHighFive)
                {
                    hero.CancelMelee();
                }
            }
        }

        public override void RunMeleeMovement()
        {
            if (owner.Y > owner.groundHeight + 1f)
            {
                hero.ApplyFallingGravity();
            }
        }

        public override void CancelMelee()
        {
            throwingSword = false;
        }

        private void PerformKnifeMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, owner.X + (float)(owner.Direction * 4), owner.Y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(24f);
            if (Map.HitClosestUnit(owner, owner.playerNum, 4, DamageType.Knifed, 14f, 24f, owner.X + owner.transform.localScale.x * 8f, owner.Y + 8f, owner.transform.localScale.x * 200f, 500f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(meleeHitSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
            hero.TriggerBroMeleeEvent();
        }

        public override void Update()
        {
            savedCounter = owner.counter;
            if (disarmed && thrownSword == null)
            {
                ReArm();
            }
        }

        private bool GetDisarmed()
        {
            var broveHeart = owner as BroveHeart;
            if (broveHeart != null)
            {
                return broveHeart.GetFieldValue<bool>("disarmed");
            }
            return disarmed;
        }

        private void SetDisarmed(bool value)
        {
            disarmed = value;
            var broveHeart = owner as BroveHeart;
            if (broveHeart != null)
            {
                broveHeart.CallMethod("SetDisarmed", value);
            }
        }

        private void ReArm()
        {
            SetDisarmed(false);
            thrownSword = null;
            if (hero.DoingMelee)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }
    }
}

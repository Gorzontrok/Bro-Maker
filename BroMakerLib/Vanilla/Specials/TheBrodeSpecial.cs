using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TheBrode")]
    public class TheBrodeSpecial : SpecialAbility
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.TheBrode);
            var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
            if (specialAttackSounds == null)
            {
                specialAttackSounds = sourceBro.soundHolder.specialAttackSounds;
            }
            if (special3Sounds == null)
            {
                special3Sounds = sourceBro.soundHolder.special3Sounds;
            }
            if (owner.faderSpritePrefab == null)
            {
                owner.faderSpritePrefab = sourceBro.faderSpritePrefab;
            }
        }

        public int palmDamage = 3;
        public float palmXRange = 8f;
        public float palmYRange = 8f;
        public float palmXOffset = 8f;
        public float palmYOffset = 13f;
        public float palmXI = 480f;
        public float palmYI = 220f;
        public float specialSoundVolume = 0.7f;
        public float hitShakeAmount = 0.6f;
        public float hitShakeDuration = 3f;
        public float dashSpeed = 300f;
        public float failCooldown = 1.5f;
        public int maxHoldFrames = 10;
        public float cooldownSpeedMultiplier = 0.5f;

        public AudioClip[] special3Sounds;
        [JsonIgnore]
        private int punchHoldFrames;
        [JsonIgnore]
        private bool hasHitSpecial = true;
        [JsonIgnore]
        private float specialDelay;

        public TheBrodeSpecial()
        {
            animationRow = 7;
            animationColumn = 25;
            animationFrameCount = 8;
            triggerFrame = 3;
        }

        public override void PressSpecial()
        {
            if (!owner.hasBeenCoverInAcid && owner.health > 0 && owner.SpecialAmmo > 0 && specialDelay <= 0f && !hero.UsingSpecial)
            {
                if (owner.actionState == ActionState.ClimbingLadder)
                {
                    owner.actionState = ActionState.Jumping;
                }
                punchHoldFrames = 0;
                hasHitSpecial = false;
                hero.UsingSpecial = true;
                owner.frame = 0;
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, specialSoundVolume,
                    owner.transform.position, 1f, true, false, false, 0f);
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
            }
        }

        public override void AnimateSpecial()
        {
            hero.DeactivateGun();
            hero.FrameRate = 0.0334f;
            int column = animationColumn + Mathf.Clamp(owner.frame, 0, 6);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, animationRow * hero.SpritePixelHeight);
            if (owner.frame == triggerFrame)
            {
                hero.CreateFaderTrailInstance();
                UseSpecial();
            }
            if (owner.frame >= animationFrameCount - 1)
            {
                owner.frame = 0;
                hero.ActivateGun();
                hero.UsingSpecial = false;
                owner.invulnerable = false;
                hero.ChangeFrame();
            }
        }

        public override void UseSpecial()
        {
            if (owner.health > 0)
            {
                if (Map.FivePointPalmExplodingHeartTechnique(owner, PlayerNum, palmDamage, palmXRange, palmYRange,
                    X + owner.transform.localScale.x * palmXOffset, Y + palmYOffset,
                    owner.transform.localScale.x * palmXI, palmYI, false, true))
                {
                    SortOfFollow.Shake(hitShakeAmount, hitShakeDuration);
                    Sound.GetInstance().PlaySoundEffectAt(special3Sounds, specialSoundVolume,
                        owner.transform.position, 1f, true, false, false, 0f);
                    owner.xI = 0f;
                    owner.xIBlast = 0f;
                    hasHitSpecial = true;
                    owner.SpecialAmmo--;
                    owner.invulnerable = true;
                    hero.InvulnerableTime = 2f;
                }
                else
                {
                    punchHoldFrames++;
                    if (punchHoldFrames < maxHoldFrames)
                    {
                        owner.frame--;
                    }
                    else
                    {
                        specialDelay = failCooldown;
                    }
                }
            }
        }

        public override bool HandleRunMovement()
        {
            if (hero.UsingSpecial && !hasHitSpecial && owner.health > 0 && (owner.frame == 2 || owner.frame == 3))
            {
                owner.xI = owner.transform.localScale.x * dashSpeed;
            }
            return true;
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if ((damageType == DamageType.Melee || damageType == DamageType.Knifed)
                && (hero.UsingSpecial || hero.GunFrame > 0)
                && Mathf.Sign(xI) != Mathf.Sign(owner.transform.localScale.x))
            {
                return false;
            }
            return true;
        }

        public override void Update()
        {
            specialDelay -= hero.DeltaTime;
        }

        public override void HandleAfterAddSpeedLeft()
        {
            if (specialDelay > 0f)
            {
                float maxSpeed = owner.speed * cooldownSpeedMultiplier;
                owner.xI = Mathf.Clamp(owner.xI, -maxSpeed, maxSpeed);
            }
        }

        public override void HandleAfterAddSpeedRight()
        {
            if (specialDelay > 0f)
            {
                float maxSpeed = owner.speed * cooldownSpeedMultiplier;
                owner.xI = Mathf.Clamp(owner.xI, -maxSpeed, maxSpeed);
            }
        }

        public override void HandleAfterAnimateNewRunningFrames()
        {
            if (specialDelay > 0f)
            {
                hero.FrameRate = owner.GetFieldValue<float>("runningFrameRate") * 2f;
            }
        }
    }
}

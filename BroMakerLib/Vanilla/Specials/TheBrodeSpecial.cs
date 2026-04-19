using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>The Brode's five-point-palm dash-strike special.</summary>
    [SpecialPreset("TheBrode")]
    public class TheBrodeSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.TheBrode;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special3Sounds == null) special3Sounds = sourceBro.soundHolder.special3Sounds.CloneArray();
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.TheBrode);
            var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
            if (owner.faderSpritePrefab == null)
            {
                owner.faderSpritePrefab = sourceBro.faderSpritePrefab;
            }
        }

        /// <summary>Damage dealt per palm strike hit.</summary>
        public int palmDamage = 3;
        /// <summary>Horizontal hit radius of the palm strike, in world units.</summary>
        public float palmXRange = 8f;
        /// <summary>Vertical hit radius of the palm strike, in world units.</summary>
        public float palmYRange = 8f;
        /// <summary>Horizontal offset from the bro's position to the palm strike origin.</summary>
        public float palmXOffset = 8f;
        /// <summary>Vertical offset from the bro's position to the palm strike origin.</summary>
        public float palmYOffset = 13f;
        /// <summary>Horizontal knockback impulse applied to struck enemies.</summary>
        public float palmXI = 480f;
        /// <summary>Vertical knockback impulse applied to struck enemies.</summary>
        public float palmYI = 220f;
        /// <summary>Playback volume for the special attack sounds.</summary>
        public float specialSoundVolume = 0.7f;
        /// <summary>Camera shake magnitude on a successful palm hit.</summary>
        public float hitShakeAmount = 0.6f;
        /// <summary>Camera shake duration on a successful palm hit.</summary>
        public float hitShakeDuration = 3f;
        /// <summary>Horizontal speed applied to the bro during the dash frames of the strike.</summary>
        public float dashSpeed = 300f;
        /// <summary>Cooldown in seconds after a failed strike before the special can be used again.</summary>
        public float failCooldown = 1.5f;
        /// <summary>Maximum animation frames the palm hold extends while waiting to connect.</summary>
        public int maxHoldFrames = 10;
        /// <summary>Movement speed multiplier applied to the bro during the post-miss cooldown.</summary>
        public float cooldownSpeedMultiplier = 0.5f;

        /// <summary>Sound played on a successful palm hit.</summary>
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
            hero.FrameRate = frameRate;
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
                && hero.UsingSpecial
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

using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using Rogueforce;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Ash Brolliams's chainsaw rampage.</summary>
    [SpecialPreset("AshBrolliams")]
    public class AshBrolliamsSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.AshBrolliams;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (effortSounds == null) effortSounds = sourceBro.soundHolder.effortSounds.CloneArray();
        }
        /// <summary>How long the rampage lasts in seconds.</summary>
        public float rampageDuration = 5.5f;
        /// <summary>Seconds between each chainsaw damage tick during rampage.</summary>
        public float rampageDamageRate = 0.03334f;
        /// <summary>Damage per chainsaw tick against units.</summary>
        public int chainsawDamage = 1;
        /// <summary>Horizontal hit range of the chainsaw sweep.</summary>
        public float chainsawRange = 16f;
        /// <summary>Vertical hit range of the chainsaw sweep.</summary>
        public float chainsawYRange = 16f;
        /// <summary>Horizontal impulse applied to units hit by the chainsaw.</summary>
        public float chainsawHitXI = 70f;
        /// <summary>Vertical impulse applied to units hit by the chainsaw.</summary>
        public float chainsawHitYI = 70f;
        /// <summary>Radius within which incoming projectiles are deflected during rampage.</summary>
        public float deflectRange = 20f;
        /// <summary>Horizontal radius within which nearby enemies are panicked each tick.</summary>
        public float panicRange = 64f;
        /// <summary>Vertical radius within which nearby enemies are panicked each tick.</summary>
        public float panicYRange = 16f;
        /// <summary>How long the panic effect lasts on affected enemies.</summary>
        public float panicDuration = 0.5f;
        /// <summary>Damage dealt to terrain per chainsaw tick.</summary>
        public int groundDamage = 3;
        /// <summary>Speed multiplier applied when moving in the facing direction during rampage.</summary>
        public float speedForwardMultiplier = 1.6f;
        /// <summary>Speed multiplier applied when not actively moving forward during rampage.</summary>
        public float speedDefaultMultiplier = 1.3f;
        /// <summary>Duration of the invulnerability window granted when rampage ends.</summary>
        public float endInvulnerableTime = 0.5f;
        /// <summary>Number of red-blood chainsaw hits before the HUD avatar switches to the bloody sprite.</summary>
        public int bloodyAvatarThreshold = 15;
        /// <summary>Sprite sheet column of the first rampage gun animation frame.</summary>
        public int rampageGunColumn = 11;
        /// <summary>Column offset added when the chainsaw hit an enemy on the last frame.</summary>
        public int rampageGunHitOffset = 3;

        [JsonIgnore]
        private bool onRampage;
        [JsonIgnore]
        private float rampageTime;
        [JsonIgnore]
        private float rampageDamageDelay;
        [JsonIgnore]
        private float rampageFrameDelay;
        [JsonIgnore]
        private int rampageFrame;
        [JsonIgnore]
        private float normalSpeed;
        [JsonIgnore]
        private int chainsawHits;
        [JsonIgnore]
        private bool haveSwitchedMaterial;
        [JsonIgnore]
        private bool hitChainsawLastFrame;
        public AudioClip[] effortSounds;
        [JsonIgnore]
        private AudioSource chainsawAudio;
        public AudioClip chainsawStart;
        public AudioClip chainsawSpin;
        public AudioClip chainsawWindDown;
        [JsonIgnore]
        private Material bloodyAvatar;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            normalSpeed = owner.speed;

            chainsawAudio = owner.gameObject.AddComponent<AudioSource>();
            chainsawAudio.rolloffMode = AudioRolloffMode.Linear;
            chainsawAudio.dopplerLevel = 0.1f;
            chainsawAudio.minDistance = 500f;
            chainsawAudio.volume = 0.4f;

            var ashBrolliams = owner as AshBrolliams;
            if (ashBrolliams == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.AshBrolliams);
                ashBrolliams = prefab as AshBrolliams;
            }
            if (ashBrolliams != null)
            {
                if (chainsawStart == null)
                    chainsawStart = ashBrolliams.chainsawStart;
                if (chainsawSpin == null)
                    chainsawSpin = ashBrolliams.chainsawSpin;
                if (chainsawWindDown == null)
                    chainsawWindDown = ashBrolliams.chainsawWindDown;
                bloodyAvatar = ashBrolliams.bloodyAvatar;
            }
        }

        public override void UseSpecial()
        {
            if (!onRampage && owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                rampageTime = rampageDuration;
                onRampage = true;
                if (chainsawAudio != null && chainsawStart != null)
                {
                    chainsawAudio.clip = chainsawStart;
                    chainsawAudio.loop = false;
                    chainsawAudio.Play();
                }
                HeroController.SetAvatarAngry(PlayerNum, owner.usePrimaryAvatar);
            }
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if ((damageType == DamageType.Melee || damageType == DamageType.Knifed || damageType == DamageType.Blade)
                && (hero.UsingSpecial || onRampage)
                && Mathf.Sign(owner.transform.localScale.x) != Mathf.Sign(xI))
            {
                return false;
            }
            return true;
        }

        public override void Update()
        {
            if (!onRampage)
            {
                return;
            }

            if (chainsawAudio != null && chainsawAudio.clip == chainsawStart && !chainsawAudio.isPlaying)
            {
                chainsawAudio.loop = true;
                chainsawAudio.clip = chainsawSpin;
                chainsawAudio.Play();
            }

            rampageTime -= hero.DeltaTime;
            if (rampageTime < 0f || owner.isOnHelicopter || !CutsceneController.PlayersCanMove())
            {
                onRampage = false;
                owner.speed = normalSpeed;
                hero.SetInvulnerable(endInvulnerableTime, true, false);
                StopChainsawAudio();
                HeroController.SetAvatarCalm(PlayerNum, true);
                return;
            }

            rampageDamageDelay -= hero.DeltaTime;
            if (rampageDamageDelay < 0f)
            {
                rampageDamageDelay += rampageDamageRate;
                BloodColor bloodColor = BloodColor.None;
                float halfWidth = owner.width / 2f;
                float halfHeight = owner.height / 2f;
                if (Map.HitUnits(owner, owner, PlayerNum, chainsawDamage, DamageType.Chainsaw,
                    chainsawRange, chainsawYRange,
                    X + owner.transform.localScale.x * halfWidth, Y + halfHeight,
                    owner.transform.localScale.x * chainsawHitXI, chainsawHitYI,
                    false, true, true, true, ref bloodColor, null, false))
                {
                    sound.PlaySoundEffectAt(effortSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                    if (bloodColor == BloodColor.Green || bloodColor == BloodColor.Red)
                    {
                        EffectsController.CreateBloodParticles(bloodColor,
                            X + owner.transform.localScale.x * owner.width * 0.25f, Y + halfHeight,
                            5, 4f, 4f, 60f, owner.transform.localScale.x * owner.speed, 350f);
                    }
                    else
                    {
                        EffectsController.CreateSparkParticles(
                            X + owner.transform.localScale.x * owner.width, Y + owner.height * 2f,
                            1f, 5, 2f, 70f, owner.transform.localScale.x * owner.speed, 250f,
                            Random.value, 1f);
                    }
                    if (chainsawAudio != null)
                    {
                        chainsawAudio.pitch = Mathf.Clamp(chainsawAudio.pitch + 0.03f, 0.85f, 1.25f);
                    }
                    if (bloodColor == BloodColor.Red && !haveSwitchedMaterial && chainsawHits++ > bloodyAvatarThreshold)
                    {
                        haveSwitchedMaterial = true;
                        if (bloodyAvatar != null)
                        {
                            HeroController.players[PlayerNum].hud.SetAvatar(bloodyAvatar);
                        }
                    }
                    hitChainsawLastFrame = true;
                }
                else
                {
                    if (chainsawAudio != null)
                    {
                        chainsawAudio.pitch = Mathf.Lerp(chainsawAudio.pitch, 0.85f, 0.1667f);
                    }
                    hitChainsawLastFrame = false;
                }
                MapController.DamageGround(owner, groundDamage, DamageType.Normal, 4f,
                    X + owner.transform.localScale.x * owner.width / 2f, Y + owner.height / 2f, null, false);
                Map.DeflectProjectiles(owner, PlayerNum, deflectRange,
                    X + Mathf.Sign(owner.transform.localScale.x) * 6f, Y + 6f,
                    Mathf.Sign(owner.transform.localScale.x) * 200f, true);
                Map.PanicUnits(X, Y, panicRange, panicYRange,
                    (int)owner.transform.localScale.x, panicDuration, false);
                bool flag;
                Map.DamageDoodads(groundDamage, DamageType.Knifed,
                    X + owner.Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            }
        }

        public override bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            if (onRampage)
            {
                if (owner.transform.localScale.x < 0f)
                {
                    if (!owner.right && owner.left)
                    {
                        owner.speed = normalSpeed * speedForwardMultiplier;
                    }
                    else if (!owner.right)
                    {
                        owner.speed = normalSpeed * speedDefaultMultiplier;
                        owner.left = true;
                    }
                }
                else
                {
                    if (!owner.left && owner.right)
                    {
                        owner.speed = normalSpeed * speedForwardMultiplier;
                    }
                    else if (!owner.left)
                    {
                        owner.speed = normalSpeed * speedDefaultMultiplier;
                        owner.right = true;
                    }
                }
            }
            return true;
        }

        public override void HandleAfterChangeFrame()
        {
            if (onRampage && owner.health > 0)
            {
                owner.gunSprite.gameObject.SetActive(true);
                rampageFrameDelay -= hero.DeltaTime;
                if (rampageFrameDelay < 0f)
                {
                    rampageFrameDelay = 0.02f;
                    rampageFrame = (rampageFrame + 1) % 4;
                }
                owner.gunSprite.SetLowerLeftPixel(32f * (rampageGunColumn + rampageFrame + (hitChainsawLastFrame ? rampageGunHitOffset : 0)), 32f);
            }
        }

        public override bool HandleRunFiring()
        {
            if (onRampage)
            {
                owner.fire = false;
            }
            return true;
        }

        public override void HandleAfterDeath()
        {
            rampageTime -= 100f;
            onRampage = false;
            StopChainsawAudio();
        }

        public override bool HandleCanInseminate(ref bool result)
        {
            if (onRampage && Mathf.Sign(owner.transform.localScale.x) != Mathf.Sign(owner.xI))
            {
                result = false;
                return false;
            }
            return true;
        }

        public override bool HandleStartPilotingUnit()
        {
            StopChainsawAudio();
            rampageTime -= 100f;
            onRampage = false;
            owner.dashing = false;
            owner.speed = normalSpeed;
            hero.DoingMelee = false;
            return true;
        }

        public override bool HandleStartMelee()
        {
            return !onRampage;
        }

        private void StopChainsawAudio()
        {
            if (chainsawAudio != null && chainsawAudio.isPlaying && chainsawAudio.clip == chainsawSpin)
            {
                chainsawAudio.loop = false;
                chainsawAudio.clip = chainsawWindDown;
                chainsawAudio.Play();
            }
        }

        public override void Cleanup()
        {
            if (chainsawAudio != null)
            {
                Object.Destroy(chainsawAudio);
            }
        }
    }
}

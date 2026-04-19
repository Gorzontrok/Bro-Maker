using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Passives
{
    /// <summary>BrondleFly's toggle hover.</summary>
    [PassivePreset("BrondleFly")]
    public class BrondleFlyPassive : PassiveAbility
    {
        protected override HeroType SourceBroType => HeroType.BrondleFly;

        protected override bool IsOwnerRedundant(BroBase owner) => owner is BrondleFly;

        /// <summary>Total hover duration available per jump, in seconds.</summary>
        public float hoverDuration = 4f;
        /// <summary>Rate at which vertical velocity damps while hovering, applied as a per-second multiplier.</summary>
        public float yDampingRate = 12f;
        /// <summary>Rate at which horizontal velocity damps while hovering with no directional input.</summary>
        public float xDampingIdle = 5f;
        /// <summary>Rate at which horizontal velocity damps while hovering with directional input held.</summary>
        public float xDampingMoving = 3f;
        /// <summary>Fraction of vertical velocity retained when hover starts with downward momentum.</summary>
        public float initialYDampen = 0.4f;
        /// <summary>Upward velocity impulse applied when hover time expires naturally.</summary>
        public float expireYBoost = 40f;
        /// <summary>Upward velocity impulse applied when hover is cancelled by a second jump press.</summary>
        public float cancelYBoost = 90f;
        public AudioClip hoverClip;

        [JsonIgnore] private bool hasHovered;
        [JsonIgnore] private float hoverTime;
        [JsonIgnore] private int hoverFrame;
        [JsonIgnore] private AudioSource hoverAudio;

        public BrondleFlyPassive()
        {
            animationRow = 7;
            animationColumn = 23;
            frameRate = 0.033f;
        }

        private bool IsHovering => hoverTime > 0f && owner.health > 0 && owner.actionState == ActionState.Jumping;

        private void SetHovering(bool active)
        {
            if (active)
            {
                hoverTime = hoverDuration;
                if (hoverAudio != null && !hoverAudio.isPlaying) hoverAudio.Play();
            }
            else
            {
                hoverTime = 0f;
                if (hoverAudio != null && hoverAudio.isPlaying) hoverAudio.Stop();
            }
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);

            hoverAudio = owner.gameObject.AddComponent<AudioSource>();
            hoverAudio.rolloffMode = AudioRolloffMode.Linear;
            hoverAudio.maxDistance = 500f;
            hoverAudio.minDistance = 128f;
            hoverAudio.clip = hoverClip;
            hoverAudio.playOnAwake = false;
            hoverAudio.dopplerLevel = 0.1f;
            hoverAudio.volume = 0.2f;
            hoverAudio.pitch = 0.7f;
            hoverAudio.spatialBlend = 0.5f;
            hoverAudio.loop = true;
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as BrondleFly;
            if (sourceBro == null) return;
            if (hoverClip == null) hoverClip = sourceBro.hoverClip;
        }

        public override bool HandleAirJump()
        {
            if (owner.health <= 0 || owner.hasBeenCoverInAcid) return true;
            if (owner.GetFieldValue<bool>("wasButtonJump")) return true;
            if (owner.actionState == ActionState.ClimbingLadder) return true;
            if (Time.time - hero.LastJumpTime <= 0.05f) return true;

            if (!hasHovered)
            {
                hasHovered = true;
                SetHovering(true);
                if (owner.yI < 0f)
                {
                    owner.yI *= initialYDampen;
                }
                return false;
            }
            else if (hoverTime > 0f)
            {
                SetHovering(false);
                owner.yI += cancelYBoost;
                return false;
            }
            return true;
        }

        public override bool HandleApplyFallingGravity()
        {
            if (!IsHovering) return true;
            owner.yI *= 1f - hero.DeltaTime * yDampingRate;
            return false;
        }

        public override bool HandleAddSpeedLeft()
        {
            if (!IsHovering) return true;
            owner.xI -= owner.speed * 1f * hero.DeltaTime;
            owner.xI *= 1f - hero.DeltaTime * xDampingMoving;
            return false;
        }

        public override bool HandleAddSpeedRight()
        {
            if (!IsHovering) return true;
            owner.xI += owner.speed * 1f * hero.DeltaTime;
            owner.xI *= 1f - hero.DeltaTime * xDampingMoving;
            return false;
        }

        public override void HandleAfterCalculateMovement()
        {
            if (IsHovering && !owner.left && !owner.right)
            {
                owner.xI *= 1f - hero.DeltaTime * xDampingIdle;
            }
        }

        public override bool HandleAnimateActualJumpingFrames()
        {
            if (!IsHovering) return true;
            AnimateHovering();
            return false;
        }

        public override bool HandleAnimateActualJumpingDuckingFrames()
        {
            if (!IsHovering) return true;
            AnimateHovering();
            return false;
        }

        private void AnimateHovering()
        {
            hero.DeactivateGun();
            hoverFrame++;
            hero.FrameRate = frameRate;
            hero.Sprite.SetLowerLeftPixel((float)((animationColumn + hoverFrame % 6) * hero.SpritePixelWidth), (float)(hero.SpritePixelHeight * animationRow));
        }

        public override bool HandleWallDrag(bool value)
        {
            if (value)
            {
                if (hoverTime > 0f) SetHovering(false);
                hasHovered = false;
            }
            return true;
        }

        public override bool HandleIsOverLadder()
        {
            return !IsHovering;
        }

        public override bool HandleDeath(float xI, float yI, DamageObject damage)
        {
            SetHovering(false);
            return true;
        }

        public override bool HandleLand()
        {
            hasHovered = false;
            return true;
        }

        public override bool HandlePressHighFiveMelee()
        {
            SetHovering(false);
            return true;
        }

        public override void Update()
        {
            if (hoverTime > 0f)
            {
                if (hero.ChimneyFlip || owner.GetFieldValue<float>("stunTime") > 0f)
                {
                    SetHovering(false);
                }
                hoverTime -= Time.deltaTime;
                if (hoverTime <= 0f)
                {
                    owner.yI += expireYBoost;
                    SetHovering(false);
                }
            }

            if (owner.actionState != ActionState.Jumping || hero.ChimneyFlip)
            {
                hasHovered = false;
                if (hoverTime > 0f) SetHovering(false);
            }

            if (hoverAudio != null && hoverAudio.isPlaying)
            {
                hoverAudio.pitch = 0.6f + Mathf.Abs(owner.xI) / 150f;
            }
        }

        public override void Cleanup()
        {
            if (hoverAudio != null)
            {
                Object.Destroy(hoverAudio);
            }
        }
    }
}

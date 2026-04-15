using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Passives
{
    /// <summary>SnakeBroskin's airplane glider.</summary>
    [PassivePreset("SnakeBroskin")]
    [ConflictsWithPreset("BrondleFly")]
    public class SnakeBroskinPassive : PassiveAbility
    {
        protected override HeroType SourceBroType => HeroType.SnakeBroSkin;

        protected override bool IsOwnerRedundant(TestVanDammeAnim owner) => owner is SnakeBroskin;

        public float gliderTerminalVelocity = 400f;
        /// <summary>Per-second drag multiplier applied to the velocity vector while gliding.</summary>
        public float dragCoefficient = 0.5f;
        /// <summary>Degrees per second at which the glide vector rotates when the bro pitches up or down.</summary>
        public float pitchRate = 180f;
        public AudioClip gliderSound;
        public AudioClip[] activateSounds;
        public AudioClip[] deactivateSounds;

        [JsonIgnore] private bool gliderActive;
        [JsonIgnore] private AudioSource gliderAudio;
        [JsonIgnore] private float defaultGliderTerminalVelocity;

        public SnakeBroskinPassive()
        {
            animationRow = 10;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            defaultGliderTerminalVelocity = gliderTerminalVelocity;

            gliderAudio = owner.gameObject.AddComponent<AudioSource>();
            gliderAudio.rolloffMode = AudioRolloffMode.Linear;
            gliderAudio.minDistance = 350f;
            gliderAudio.maxDistance = 500f;
            gliderAudio.spatialBlend = 1f;
            gliderAudio.volume = 0.075f;
            gliderAudio.dopplerLevel = 0f;
            gliderAudio.pitch = 0.3f;
            gliderAudio.clip = gliderSound;
            gliderAudio.loop = true;
            gliderAudio.playOnAwake = false;
            gliderAudio.Stop();
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as SnakeBroskin;
            if (sourceBro == null) return;
            if (gliderSound == null && sourceBro.gliderAudio != null)
                gliderSound = sourceBro.gliderAudio.clip;
            if (activateSounds == null) activateSounds = sourceBro.soundHolder.special3Sounds.CloneArray();
            if (deactivateSounds == null) deactivateSounds = sourceBro.soundHolder.special4Sounds.CloneArray();
        }

        private void ActivateGlider()
        {
            if (gliderActive) return;
            gliderActive = true;
            gliderTerminalVelocity = defaultGliderTerminalVelocity;
            if (gliderAudio != null && gliderSound != null)
            {
                gliderAudio.clip = gliderSound;
                gliderAudio.Play();
            }
            if (activateSounds != null && activateSounds.Length > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(activateSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.DeactivateGun();
        }

        private void DeactivateGlider()
        {
            if (!gliderActive) return;
            gliderActive = false;
            if (gliderAudio != null && gliderAudio.isPlaying) gliderAudio.Stop();
            hero.ActivateGun();
            if (deactivateSounds != null && deactivateSounds.Length > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(deactivateSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
        }

        public override bool HandleRunFalling()
        {
            if (!gliderActive
                && owner.GetFieldValue<bool>("buttonJump")
                && !owner.GetFieldValue<bool>("wallClimbing"))
            {
                ActivateGlider();
            }
            return true;
        }

        public override bool HandleApplyFallingGravity()
        {
            if (!gliderActive) return true;
            if (owner.GetFieldValue<bool>("chimneyFlip") && owner.GetFieldValue<bool>("chimneyFlipConstrained")) return false;
            if (owner.actionState == ActionState.ClimbingLadder) return false;
            if (owner.GetFieldValue<bool>("isInQuicksand")) return true;

            Vector2 vector = Vector2.ClampMagnitude(new Vector2(owner.xI, owner.yI), gliderTerminalVelocity);
            vector *= 1f - hero.DeltaTime * dragCoefficient;

            if (owner.up)
            {
                if (Mathf.Abs(owner.xI) > 50f || owner.yI < 0f)
                {
                    vector = Quaternion.AngleAxis(pitchRate * hero.DeltaTime * owner.transform.localScale.x, Vector3.forward) * vector;
                }
            }
            else if (owner.down && (Mathf.Abs(owner.xI) > 50f || owner.yI > 0f))
            {
                vector = Quaternion.AngleAxis(-pitchRate * hero.DeltaTime * owner.transform.localScale.x, Vector3.forward) * vector;
            }

            owner.xI = vector.x;
            owner.yI = vector.y;
            float magnitude = vector.magnitude;

            if (owner.yI > 0f)
            {
                owner.yI -= 1100f * hero.DeltaTime * 0.1f;
            }
            if (magnitude < 150f)
            {
                owner.yI -= 1100f * hero.DeltaTime * (1f - magnitude / 150f) * 0.75f;
            }
            if (Mathf.Abs(owner.xI) < 200f && owner.yI < 0f)
            {
                owner.yI -= 1100f * hero.DeltaTime * (1f - Mathf.Abs(vector.x) / 200f) * 0.5f;
            }

            if (gliderAudio != null)
            {
                gliderAudio.pitch = Mathf.Lerp(0.3f, 2.5f, magnitude / gliderTerminalVelocity);
                gliderAudio.volume = Mathf.Lerp(0.075f, 0.35f, magnitude / gliderTerminalVelocity);
            }
            return false;
        }

        public override bool HandleAddSpeedLeft()
        {
            return !gliderActive;
        }

        public override bool HandleAddSpeedRight()
        {
            return !gliderActive;
        }

        public override bool HandleAnimateActualJumpingFrames()
        {
            if (!gliderActive) return true;
            int num = 18 - (int)((Math.GetAngle(Mathf.Abs(owner.xI), owner.yI) / 3.1415927f + 0.5f) * 18.99f);
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(animationRow * hero.SpritePixelHeight));
            return false;
        }

        public override bool HandleLand()
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandleWallDrag(bool value)
        {
            if (value && gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandleFireWeapon()
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandleStartMelee()
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandlePressSpecial()
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandlePressHighFiveMelee()
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandleActivateGun()
        {
            return !gliderActive;
        }

        public override bool HandleLedgeGrapple(bool left, bool right, float radius, float heightOpenOffset)
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandleAnimateWallAnticipation()
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandleAnimateWallClimb()
        {
            if (gliderActive) DeactivateGlider();
            return true;
        }

        public override bool HandleCheckForQuicksand()
        {
            // Hook runs before base updates isInQuicksand, so query the map directly.
            if (gliderActive && Map.IsBlockQuicksand(owner.X, owner.Y + 1f)) DeactivateGlider();
            return true;
        }

        public override void Update()
        {
            if (!gliderActive) return;
            if (owner.actionState == ActionState.ClimbingLadder)
            {
                DeactivateGlider();
            }
        }

        public override void Cleanup()
        {
            if (gliderAudio != null)
            {
                Object.Destroy(gliderAudio);
            }
        }
    }
}

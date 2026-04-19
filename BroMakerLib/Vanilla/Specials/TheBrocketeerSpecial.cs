using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using BroMakerLib.Vanilla.Passives;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>TheBrocketeer's dive-bomb special. Jetpack hover lives in `TheBrocketeerPassive`.</summary>
    [SpecialPreset("Brocketeer")]
    public class TheBrocketeerSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.TheBrocketeer;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special2Sounds == null) special2Sounds = sourceBro.soundHolder.special2Sounds.CloneArray();
        }

        public float downSlamInvulnerabilityTime = 0.33f;
        /// <summary>Damage dealt to enemies on dive impact.</summary>
        public int blastDamage = 25;
        /// <summary>Horizontal radius of the dive impact explosion, in world units.</summary>
        public float blastXRange = 80f;
        /// <summary>Vertical radius of the dive impact explosion, in world units.</summary>
        public float blastYRange = 30f;
        /// <summary>Horizontal knockback impulse applied to enemies on dive impact.</summary>
        public float blastXI = 300f;
        /// <summary>Vertical knockback impulse applied to enemies on dive impact.</summary>
        public float blastYI = 380f;
        /// <summary>Damage dealt to ground tiles on dive impact.</summary>
        public int blastGroundDamage = 15;
        /// <summary>Radius of ground tile damage on dive impact, in world units.</summary>
        public float blastGroundRange = 36f;
        /// <summary>Damage per hit dealt to enemies during the diagonal dash phase.</summary>
        public float dashDamage = 2;
        /// <summary>Seconds between successive dash-phase damage ticks.</summary>
        public float dashDamageRate = 0.025f;
        /// <summary>Radius of each dash-phase damage check, in world units.</summary>
        public float dashDamageRange = 9f;
        /// <summary>Maximum seconds spent in the suspended-hang phase before automatically committing to the dive.</summary>
        public float suspendedAirdashDelayTime = 0.45f;
        /// <summary>Sound played on dive impact.</summary>
        public AudioClip[] special2Sounds;

        /// <summary>When true, plays the stamp landing animation after a ground impact.</summary>
        public bool enableStampAnimation = true;
        public int stampAnimationRow = 8;
        public int stampAnimationStartColumn = 6;
        public int stampAnimationEndFrame = 7;
        public float stampFrameRate = 0.09f;

        /// <summary>Volume of the dive impact sound played by `MakeDashBlast`.</summary>
        public float dashBlastSoundVolume = 0.6f;
        public AudioClip jetpackLiftSound;
        public AudioClip jetpackDiveSound;

        [JsonIgnore] private float stampDelay;
        [JsonIgnore] private DirectionEnum downwardDashDirection;
        [JsonIgnore] public float suspendedAirdashDelay;
        [JsonIgnore] private float jetPackFlameCounter;
        [JsonIgnore] private float specialAttackDashCounter;
        [JsonIgnore] private Vector3 lastFlamePos;
        [JsonIgnore] private AudioSource diveAudio;
        [JsonIgnore] private bool isDiving;
        [JsonIgnore] private Vector3 currentJetpackDirection;
        [JsonIgnore] private Vector3 jetpackDownDirection = new Vector3(0f, -370f, 0f);

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);

            diveAudio = owner.gameObject.AddComponent<AudioSource>();
            diveAudio.rolloffMode = AudioRolloffMode.Linear;
            diveAudio.minDistance = 350f;
            diveAudio.maxDistance = 500f;
            diveAudio.spatialBlend = 1f;
            diveAudio.volume = 0.13f;
            diveAudio.dopplerLevel = 0f;
            diveAudio.pitch = 1f;
            diveAudio.loop = true;
            diveAudio.playOnAwake = false;
            diveAudio.Stop();

            var brocketeer = owner as TheBrocketeer ?? HeroController.GetHeroPrefab(HeroType.TheBrocketeer) as TheBrocketeer;
            if (brocketeer != null)
            {
                if (jetpackLiftSound == null) jetpackLiftSound = brocketeer.jetpackLiftSound;
                if (jetpackDiveSound == null) jetpackDiveSound = brocketeer.jetpackDiveSound;
                jetpackDownDirection = brocketeer.jetpackDownDirection;
            }
            currentJetpackDirection = jetpackDownDirection;
            hero.CanAirdash = true;
        }

        public override void PressSpecial()
        {
            if (owner.health <= 0) return;

            float airdashTime = hero.AirdashTime;
            DirectionEnum airdashDirection = hero.AirdashDirection;
            if (owner.SpecialAmmo > 0 && (airdashTime <= 0f || airdashDirection == DirectionEnum.Any))
            {
                owner.SpecialAmmo--;
                if (owner.IsOnGround())
                {
                    PerformAirDashDown();
                    owner.yI = 300f;
                    hero.GetPassive<TheBrocketeerPassive>()?.BlastOff();
                }
                else
                {
                    PerformAirDashDown();
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
            }
        }

        private void PerformAirDashDown()
        {
            hero.AirdashTime = owner.GetFieldValue<float>("airdashMaxTime") * 5f;
            hero.AirdashDirection = DirectionEnum.Down;
            owner.yI = owner.jumpForce * 0.8f;
            owner.xI = 0f;
            owner.SetFieldValue("airdashDownAvailable", false);
            owner.actionState = ActionState.Jumping;
            owner.CallMethod("PlayAirDashChargeUpSound");
            hero.JumpTime = 0f;

            downwardDashDirection = owner.transform.localScale.x > 0f ? DirectionEnum.Right : DirectionEnum.Left;

            isDiving = true;
            suspendedAirdashDelay = suspendedAirdashDelayTime;
            jetPackFlameCounter = 0f;
            // Dive flame direction derives from held input; vanilla read it from hover state which this special doesn't track.
            if (owner.left)
            {
                var side = new Vector3(160f, -330f, 0f);
                currentJetpackDirection = side;
            }
            else if (owner.right)
            {
                currentJetpackDirection = new Vector3(-160f, -330f, 0f);
            }
            else
            {
                currentJetpackDirection = jetpackDownDirection;
            }
            if (diveAudio != null)
            {
                if (diveAudio.clip != jetpackLiftSound)
                    diveAudio.clip = jetpackLiftSound;
                if (!diveAudio.isPlaying)
                    diveAudio.Play();
            }
        }

        public override bool HandleGetGroundLayer(ref int result)
        {
            if (isDiving)
            {
                result = owner.GetFieldValue<LayerMask>("groundLayer");
                return false;
            }
            return true;
        }

        public override bool HandleRunDownwardDash()
        {
            // Only take over when the dive was launched via PressSpecial (isDiving flag).
            // Otherwise a non-special down airdash (e.g. Nebro passive's melee+down) would trigger
            // dive physics without consuming SpecialAmmo.
            if (!isDiving) return true;
            DirectionEnum airdashDirection = owner.GetFieldValue<DirectionEnum>("airdashDirection");
            if (airdashDirection != DirectionEnum.Down)
            {
                return true;
            }

            if (owner.yI > -50f)
            {
                owner.yI = Mathf.Clamp(owner.yI - 1500f * hero.DeltaTime + Mathf.Clamp(owner.yI, -1000f, 18f) * 20f * hero.DeltaTime,
                    owner.maxFallSpeed * 1.25f, 350f);
                if (owner.yI <= -50f)
                {
                    StartDownwardCharge();
                }
                if (suspendedAirdashDelay > 0f)
                {
                    suspendedAirdashDelay -= hero.DeltaTime;
                    if (suspendedAirdashDelay <= 0f)
                    {
                        StartDownwardCharge();
                    }
                }
                if (!owner.buttonJump)
                {
                    jetPackFlameCounter += hero.DeltaTime;
                    if (jetPackFlameCounter > 0.0225f)
                    {
                        jetPackFlameCounter -= 0.02f;
                        owner.CallMethod("CreateJetpackFlames", currentJetpackDirection);
                    }
                }
                lastFlamePos = new Vector3(X, Y, 0f);
                if (diveAudio != null)
                {
                    diveAudio.pitch = Mathf.Lerp(diveAudio.pitch, 1.2f, hero.DeltaTime * 3f);
                }
            }
            else
            {
                owner.yI = Mathf.Clamp(owner.yI - 1500f * hero.DeltaTime + Mathf.Clamp(owner.yI, -1000f, 18f) * 20f * hero.DeltaTime,
                    owner.maxFallSpeed * 1.25f, 300f);
                if (downwardDashDirection == DirectionEnum.Left)
                    owner.xI = -Mathf.Abs(owner.yI);
                else if (downwardDashDirection == DirectionEnum.Right)
                    owner.xI = Mathf.Abs(owner.yI);

                jetPackFlameCounter += hero.DeltaTime;
                if (jetPackFlameCounter > 0.0225f)
                {
                    jetPackFlameCounter -= 0.025f;
                    CreateJetpackFlamesDash();
                }
                if (diveAudio != null)
                {
                    diveAudio.pitch = Mathf.Lerp(diveAudio.pitch, 3f, hero.DeltaTime * 5f);
                }
            }

            RunAirDashDamage();
            if (owner.yI < -100f && Y < owner.groundHeight + 30f)
            {
                hero.SetInvulnerable(0.35f, false, false);
            }
            return false;
        }

        private void StartDownwardCharge()
        {
            owner.yI = -50f;
            Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
            suspendedAirdashDelay = 0f;
            EffectsController.CreateAirDashPoofEffect(X, Y + 8f, new Vector3(0f, 100f, 0f));
            downwardDashDirection = owner.transform.localScale.x > 0f ? DirectionEnum.Right : DirectionEnum.Left;
            hero.ChangeFrame();
            if (diveAudio != null)
            {
                if (diveAudio.clip != jetpackDiveSound)
                    diveAudio.clip = jetpackDiveSound;
                if (!diveAudio.isPlaying)
                    diveAudio.Play();
                diveAudio.pitch = 0.4f;
                diveAudio.volume = 0.25f;
            }
        }

        private void RunAirDashDamage()
        {
            specialAttackDashCounter += hero.DeltaTime;
            if (specialAttackDashCounter > 0f)
            {
                specialAttackDashCounter -= dashDamageRate;
                Map.HitUnits(owner, owner, PlayerNum, (int)dashDamage, DamageType.Crush, dashDamageRange,
                    X, Y, owner.xI * 0.3f, 150f + Random.value * 70f, true, true);
            }
        }

        private void CreateJetpackFlamesDash()
        {
            Vector3 currentPos = new Vector3(X, Y, 0f);
            Vector3 diff = currentPos - lastFlamePos;
            float magnitude = diff.magnitude;
            Vector3 step = diff / magnitude;
            int i = 0;
            while (i < Mathf.Min(magnitude / 2f, 50f))
            {
                lastFlamePos += step * 2f;
                EffectsController.CreatePlumeParticle(lastFlamePos.x + 2.5f * owner.transform.localScale.x - owner.transform.localScale.x * 4f, lastFlamePos.y + 15f, 4f, 4f, owner.xI * 0.05f, owner.yI * 0.05f, 0.4f, 1.3f);
                EffectsController.CreatePlumeParticle(lastFlamePos.x - 2.5f * owner.transform.localScale.x - owner.transform.localScale.x * 4f, lastFlamePos.y + 10f, 4f, 4f, owner.xI * 0.05f, owner.yI * 0.05f, 0.4f, 1.3f);
                i++;
            }
        }

        private void EndDive()
        {
            isDiving = false;
            hero.AirdashDirection = DirectionEnum.Any;
            downwardDashDirection = DirectionEnum.None;
            hero.AirdashTime = 0f;
        }

        private void MakeDashBlast(float xPoint, float yPoint, bool groundWave)
        {
            owner.SetFieldValue("airDashJumpGrace", 0.15f);
            Map.ExplodeUnits(owner, blastDamage, DamageType.Explosion, blastXRange, blastYRange,
                xPoint, yPoint, blastXI, blastYI, PlayerNum, false, false, true);
            MapController.DamageGround(owner, blastGroundDamage, DamageType.Explosion, blastGroundRange, xPoint, yPoint, null, false);
            EffectsController.CreateWhiteFlashPop(xPoint, yPoint);
            SortOfFollow.Shake(0.9f);
            Sound.GetInstance().PlaySoundEffectAt(special2Sounds, dashBlastSoundVolume, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
            if (groundWave)
            {
                EffectsController.CreateGroundWave(xPoint, yPoint + 1f, 80f);
                Map.ShakeTrees(X, Y, 120f, 40f, 90f);
            }
            Map.DisturbWildLife(X, Y, 48f, PlayerNum);
            owner.SetFieldValue("rollingFrames", 0);
            if (diveAudio != null)
            {
                diveAudio.volume = 0.13f;
            }
        }

        public override void HandleAfterHitRightWall()
        {
            float airdashTime = hero.AirdashTime;
            DirectionEnum airdashDirection = hero.AirdashDirection;
            if (airdashTime > 0f && (airdashDirection == DirectionEnum.Right ||
                (airdashDirection == DirectionEnum.Down && owner.transform.localScale.x > 0f)))
            {
                MakeDashBlast(X + 7f, Y + 5f, true);
                owner.xIBlast = -100f;
                owner.yI += 50f;
                EndDive();
                if (hero.PressedJumpInAirSoJumpIfTouchGroundGrace > 0f)
                {
                    hero.Jump(false);
                }
            }
        }

        public override void HandleAfterHitLeftWall()
        {
            float airdashTime = hero.AirdashTime;
            DirectionEnum airdashDirection = hero.AirdashDirection;
            if (airdashTime > 0f && (airdashDirection == DirectionEnum.Left ||
                (airdashDirection == DirectionEnum.Down && owner.transform.localScale.x < 0f)))
            {
                MakeDashBlast(X - 7f, Y + 5f, true);
                owner.xIBlast = 100f;
                owner.yI += 50f;
                EndDive();
                if (hero.PressedJumpInAirSoJumpIfTouchGroundGrace > 0f)
                {
                    hero.Jump(false);
                }
            }
        }

        public override bool HandleAnimateIdle()
        {
            if (enableStampAnimation && stampDelay > 0f)
            {
                hero.DeactivateGun();
                hero.FrameRate = stampFrameRate;
                hero.Sprite.SetLowerLeftPixel(
                    (float)((stampAnimationStartColumn + Mathf.Clamp(owner.frame, 0, 3)) * hero.SpritePixelWidth),
                    (float)(hero.SpritePixelHeight * stampAnimationRow));
                if (owner.frame >= stampAnimationEndFrame)
                {
                    stampDelay = 0f;
                }
                return false;
            }
            return true;
        }

        public override void Update()
        {
            stampDelay -= hero.DeltaTime;

            if (owner.health <= 0)
            {
                if (diveAudio != null && diveAudio.isPlaying)
                    diveAudio.Stop();
                return;
            }

            float airdashTime = hero.AirdashTime;
            if (airdashTime <= 0f && !isDiving && diveAudio != null && diveAudio.isPlaying)
            {
                diveAudio.Stop();
            }
        }

        public override bool HandleDeath(float xI, float yI, DamageObject damage)
        {
            isDiving = false;
            if (diveAudio != null && diveAudio.isPlaying)
                diveAudio.Stop();
            return true;
        }

        public override bool HandleLand()
        {
            float airdashTime = hero.AirdashTime;
            DirectionEnum airdashDirection = hero.AirdashDirection;
            if (airdashTime > 0f && airdashDirection == DirectionEnum.Down)
            {
                MakeDashBlast(X, owner.groundHeight + 9f, true);
                float groundHeightGround = owner.CallMethod<float>("GetGroundHeightGround");
                if (Mathf.Abs(groundHeightGround - Y) < 24f)
                {
                    owner.Y = groundHeightGround;
                    owner.groundHeight = groundHeightGround;
                }
                if (hero.PressedJumpInAirSoJumpIfTouchGroundGrace <= 0f)
                {
                    if (enableStampAnimation)
                    {
                        stampDelay = 0.6f;
                        owner.frame = -3;
                    }
                    owner.yI = 0f;
                }
                hero.SetInvulnerable(downSlamInvulnerabilityTime, false, false);
                EndDive();
                owner.xI = 0f;
                owner.xIBlast = 0f;
            }
            return true;
        }

        public override void HandleAfterLand()
        {
            // TheBrocketeerPassive owns the buffered jump when attached (so it can re-engage hover too).
            if (hero.GetPassive<TheBrocketeerPassive>() == null
                && hero.PressedJumpInAirSoJumpIfTouchGroundGrace > 0f
                && owner.yI <= 0f)
            {
                hero.Jump(true);
            }
            jetPackFlameCounter = 0f;
        }

        public override void Cleanup()
        {
            hero.CanAirdash = false;
            if (diveAudio != null)
            {
                Object.Destroy(diveAudio);
            }
        }
    }
}

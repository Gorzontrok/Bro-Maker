using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("TheBrocketeer")]
    public class TheBrocketeerSpecial : SpecialAbility
    {
        public float downSlamInvulnerabilityTime = 0.33f;
        public int blastDamage = 25;
        public float blastXRange = 80f;
        public float blastYRange = 30f;
        public float blastXI = 300f;
        public float blastYI = 380f;
        public int blastGroundDamage = 15;
        public float blastGroundRange = 36f;
        public float dashDamage = 2;
        public float dashDamageRate = 0.025f;
        public float dashDamageRange = 9f;
        public float suspendedAirdashDelayTime = 0.45f;
        public AudioClip[] special2Sounds;

        [JsonIgnore]
        private DirectionEnum downwardDashDirection;
        [JsonIgnore]
        private float suspendedAirdashDelay;
        [JsonIgnore]
        private float jetPackFlameCounter;
        [JsonIgnore]
        private float specialAttackDashCounter;
        [JsonIgnore]
        private Vector3 lastFlamePos;
        [JsonIgnore]
        private AudioSource jetpackAudio;
        [JsonIgnore]
        private AudioClip jetpackLiftSound;
        [JsonIgnore]
        private AudioClip jetpackDiveSound;
        [JsonIgnore]
        private FlameWallExplosion liftOffBlastFlameWall;

        // Hover state
        [JsonIgnore]
        private float hoverTime;
        [JsonIgnore]
        private float hoverDuration = 1.5f;
        [JsonIgnore]
        private float hoverSinCounter;
        [JsonIgnore]
        private float hoverSinStart = 4.8f;
        [JsonIgnore]
        private float hoverSinSpeed = 5f;
        [JsonIgnore]
        private float hoverSinForce = 0.15f;
        [JsonIgnore]
        private float defaultHoverForce = 0.25f;
        [JsonIgnore]
        private float jetPackCounter;
        [JsonIgnore]
        private float jetPackRate = 0.04f;
        [JsonIgnore]
        private int jetPackFlameCount;
        [JsonIgnore]
        private Vector3 currentJetpackDirection;
        [JsonIgnore]
        private Vector3 jetpackSideDirection = new Vector3(160f, -330f, 0f);
        [JsonIgnore]
        private Vector3 jetpackDownDirection = new Vector3(0f, -370f, 0f);

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            jetpackAudio = owner.gameObject.AddComponent<AudioSource>();
            jetpackAudio.rolloffMode = AudioRolloffMode.Linear;
            jetpackAudio.minDistance = 350f;
            jetpackAudio.maxDistance = 500f;
            jetpackAudio.spatialBlend = 1f;
            jetpackAudio.volume = 0.13f;
            jetpackAudio.dopplerLevel = 0f;
            jetpackAudio.pitch = 1f;
            jetpackAudio.loop = true;
            jetpackAudio.playOnAwake = false;
            jetpackAudio.Stop();

            var brocketeer = owner as TheBrocketeer;
            if (brocketeer == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.TheBrocketeer);
                brocketeer = prefab as TheBrocketeer;
            }
            if (brocketeer != null)
            {
                jetpackLiftSound = brocketeer.jetpackLiftSound;
                jetpackDiveSound = brocketeer.jetpackDiveSound;
                liftOffBlastFlameWall = brocketeer.liftOffBlastFlameWall;
                hoverDuration = brocketeer.hoverDuration;
                hoverSinStart = brocketeer.hoverSinStart;
                hoverSinSpeed = brocketeer.hoverSinSpeed;
                hoverSinForce = brocketeer.hoverSinForce;
                defaultHoverForce = brocketeer.defaultHoverForce;
                jetPackRate = brocketeer.GetFieldValue<float>("jetPackRate");
                jetpackSideDirection = brocketeer.jetpackSideDirection;
                jetpackDownDirection = brocketeer.jetpackDownDirection;
                if (specialAttackSounds == null)
                    specialAttackSounds = brocketeer.soundHolder.specialAttackSounds;
                if (special2Sounds == null)
                    special2Sounds = brocketeer.soundHolder.special2Sounds;
            }
            currentJetpackDirection = jetpackDownDirection;
            if (jetpackLiftSound != null)
            {
                jetpackAudio.clip = jetpackLiftSound;
            }
        }

        public override void PressSpecial()
        {
            if (owner.health > 0)
            {
                float airdashTime = owner.GetFieldValue<float>("airdashTime");
                DirectionEnum airdashDirection = owner.GetFieldValue<DirectionEnum>("airdashDirection");
                if (owner.SpecialAmmo > 0 && (airdashTime <= 0f || airdashDirection == DirectionEnum.Any))
                {
                    owner.SpecialAmmo--;
                    if (owner.IsOnGround())
                    {
                        PerformAirDashDown();
                        owner.yI = 300f;
                        BlastOff();
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
        }

        private void PerformAirDashDown()
        {
            owner.SetFieldValue("airdashTime", owner.GetFieldValue<float>("airdashMaxTime") * 5f);
            owner.SetFieldValue("airdashDirection", DirectionEnum.Down);
            owner.yI = owner.jumpForce * 0.8f;
            owner.xI = 0f;
            owner.SetFieldValue("airdashDownAvailable", false);
            owner.actionState = ActionState.Jumping;
            owner.SetFieldValue("jumpTime", 0f);

            if (owner.transform.localScale.x > 0f)
                downwardDashDirection = DirectionEnum.Right;
            else
                downwardDashDirection = DirectionEnum.Left;

            suspendedAirdashDelay = suspendedAirdashDelayTime;
            jetPackFlameCounter = 0f;
            if (jetpackAudio != null)
            {
                if (jetpackAudio.clip != jetpackLiftSound)
                    jetpackAudio.clip = jetpackLiftSound;
                if (!jetpackAudio.isPlaying)
                    jetpackAudio.Play();
            }
        }

        private void BlastOff()
        {
            owner.CallMethod("CreateBlastOffFlames", Y);
            hoverSinCounter = hoverSinStart;
            owner.yI += 100f;
            hoverTime = hoverDuration;
            jetPackFlameCounter = 0f;
            if (jetpackAudio != null)
            {
                jetpackAudio.pitch = 1f;
                jetpackAudio.volume = 0.13f;
            }
        }

        public override bool HandleRunDownwardDash()
        {
            DirectionEnum airdashDirection = owner.GetFieldValue<DirectionEnum>("airdashDirection");
            if (airdashDirection != DirectionEnum.Down)
            {
                return true;
            }

            if (owner.yI > -50f && suspendedAirdashDelay > 0f)
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
                if (!owner.GetFieldValue<bool>("buttonJump"))
                {
                    jetPackFlameCounter += hero.DeltaTime;
                    if (jetPackFlameCounter > 0.0225f)
                    {
                        jetPackFlameCounter -= 0.02f;
                        owner.CallMethod("CreateJetpackFlames", currentJetpackDirection);
                    }
                }
                lastFlamePos = new Vector3(X, Y, 0f);
                if (jetpackAudio != null)
                {
                    jetpackAudio.pitch = Mathf.Lerp(jetpackAudio.pitch, 1.2f, hero.DeltaTime * 3f);
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
                if (jetpackAudio != null)
                {
                    jetpackAudio.pitch = Mathf.Lerp(jetpackAudio.pitch, 3f, hero.DeltaTime * 5f);
                }
            }

            RunAirDashDamage();
            if (owner.yI < -100f && Y < owner.groundHeight + 30f)
            {
                owner.CallMethod("SetInvulnerable", 0.35f, false, false);
            }
            return false;
        }

        private void StartDownwardCharge()
        {
            owner.yI = -50f;
            Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
            suspendedAirdashDelay = 0f;
            EffectsController.CreateAirDashPoofEffect(X, Y + 8f, new Vector3(0f, 100f, 0f));
            if (owner.transform.localScale.x > 0f)
                downwardDashDirection = DirectionEnum.Right;
            else
                downwardDashDirection = DirectionEnum.Left;
            hero.ChangeFrame();
            if (jetpackAudio != null)
            {
                if (jetpackAudio.clip != jetpackDiveSound)
                    jetpackAudio.clip = jetpackDiveSound;
                if (!jetpackAudio.isPlaying)
                    jetpackAudio.Play();
                jetpackAudio.pitch = 0.4f;
                jetpackAudio.volume = 0.25f;
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

        private void MakeDashBlast(float xPoint, float yPoint, bool groundWave)
        {
            owner.SetFieldValue("airDashJumpGrace", 0.15f);
            Map.ExplodeUnits(owner, blastDamage, DamageType.Explosion, blastXRange, blastYRange,
                xPoint, yPoint, blastXI, blastYI, PlayerNum, false, false, true);
            MapController.DamageGround(owner, blastGroundDamage, DamageType.Explosion, blastGroundRange, xPoint, yPoint, null, false);
            EffectsController.CreateWhiteFlashPop(xPoint, yPoint);
            SortOfFollow.Shake(0.9f);
            Sound.GetInstance().PlaySoundEffectAt(special2Sounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
            if (groundWave)
            {
                EffectsController.CreateGroundWave(xPoint, yPoint + 1f, 80f);
                Map.ShakeTrees(X, Y, 120f, 40f, 90f);
            }
            Map.DisturbWildLife(X, Y, 48f, PlayerNum);
            owner.SetFieldValue("rollingFrames", 0);
            if (jetpackAudio != null)
            {
                jetpackAudio.volume = 0.13f;
            }
        }

        public override void HandleAfterHitRightWall()
        {
            float airdashTime = owner.GetFieldValue<float>("airdashTime");
            DirectionEnum airdashDirection = owner.GetFieldValue<DirectionEnum>("airdashDirection");
            if (airdashTime > 0f && (airdashDirection == DirectionEnum.Right ||
                (airdashDirection == DirectionEnum.Down && owner.transform.localScale.x > 0f)))
            {
                MakeDashBlast(X + 7f, Y + 5f, true);
                owner.xIBlast = -100f;
                owner.yI += 50f;
                owner.SetFieldValue("airdashDirection", DirectionEnum.Any);
                owner.SetFieldValue("airdashTime", 0f);
                downwardDashDirection = DirectionEnum.None;
                if (owner.GetFieldValue<float>("pressedJumpInAirSoJumpIfTouchGroundGrace") > 0f)
                {
                    owner.CallMethod("Jump", false);
                }
            }
        }

        public override void HandleAfterHitLeftWall()
        {
            float airdashTime = owner.GetFieldValue<float>("airdashTime");
            DirectionEnum airdashDirection = owner.GetFieldValue<DirectionEnum>("airdashDirection");
            if (airdashTime > 0f && (airdashDirection == DirectionEnum.Left ||
                (airdashDirection == DirectionEnum.Down && owner.transform.localScale.x < 0f)))
            {
                MakeDashBlast(X - 7f, Y + 5f, true);
                owner.xIBlast = 100f;
                owner.yI += 50f;
                owner.SetFieldValue("airdashDirection", DirectionEnum.Any);
                owner.SetFieldValue("airdashTime", 0f);
                downwardDashDirection = DirectionEnum.None;
                if (owner.GetFieldValue<float>("pressedJumpInAirSoJumpIfTouchGroundGrace") > 0f)
                {
                    owner.CallMethod("Jump", false);
                }
            }
        }

        public override void Update()
        {
            if (owner.health <= 0)
            {
                if (jetpackAudio != null && jetpackAudio.isPlaying)
                    jetpackAudio.Stop();
                return;
            }
            float airdashTime = owner.GetFieldValue<float>("airdashTime");
            bool buttonJump = owner.GetFieldValue<bool>("buttonJump");
            if (buttonJump && (airdashTime <= 0f || suspendedAirdashDelay > 0f))
            {
                RunJetpack();
            }
            else if (airdashTime <= 0f && jetpackAudio != null && jetpackAudio.isPlaying)
            {
                jetpackAudio.Stop();
            }
        }

        private void RunJetpack()
        {
            hoverTime -= hero.DeltaTime;
            if (hoverTime > 0f)
            {
                if (jetpackAudio != null)
                {
                    if (!jetpackAudio.isPlaying)
                    {
                        jetpackAudio.Play();
                        if (jetpackAudio.clip != jetpackLiftSound)
                            jetpackAudio.clip = jetpackLiftSound;
                    }
                    jetpackAudio.pitch = Mathf.Lerp(jetpackAudio.pitch, 0.8f + ((owner.yI <= 100f) ? 0f : 0.5f), hero.DeltaTime * 7f);
                }
                if (Y > owner.groundHeight + 1f && owner.yI < 70f)
                {
                    hoverSinCounter = Mathf.Clamp(hoverSinCounter - hero.DeltaTime * hoverSinSpeed, 0f, 50f);
                }
                jetPackCounter += hero.DeltaTime * (0.4f + ((hoverTime <= 1f) ? 0f : 0.3f) + ((hoverTime <= 0.5f) ? 0f : 0.3f) + ((hoverTime <= 0f) ? 0f : 0.2f));
                if (jetPackCounter > jetPackRate)
                {
                    jetPackCounter -= jetPackRate;
                    if (owner.left)
                        currentJetpackDirection = Vector3.RotateTowards(currentJetpackDirection, jetpackSideDirection, 0.133f, 45f);
                    else if (owner.right)
                        currentJetpackDirection = Vector3.RotateTowards(currentJetpackDirection, new Vector3(jetpackSideDirection.x * -1f, jetpackSideDirection.y, 0f), 0.133f, 45f);
                    else
                        currentJetpackDirection = Vector3.RotateTowards(currentJetpackDirection, jetpackDownDirection, 0.133f, 45f);

                    if (owner.yI < 30f)
                        owner.yI -= currentJetpackDirection.y * (defaultHoverForce + hoverSinForce * Mathf.Sin(hoverSinCounter));
                    else if (owner.yI < 80f)
                        owner.yI -= currentJetpackDirection.y * 0.05f;
                    else
                        owner.yI -= currentJetpackDirection.y * 0.016f;

                    if (hoverTime > hoverDuration - 0.5f)
                        owner.yI -= currentJetpackDirection.y * 0.03f;

                    if ((currentJetpackDirection.x < 0f && owner.right) || (currentJetpackDirection.x > 0f && owner.left) || (!owner.right && !owner.left))
                    {
                        if (owner.GetFieldValue<bool>("dashing"))
                            owner.xIBlast -= currentJetpackDirection.x * 0.01f;
                        else
                            owner.xIBlast -= currentJetpackDirection.x * 0.016f;
                    }
                }
                jetPackFlameCounter += hero.DeltaTime;
                if (jetPackFlameCounter > 0.0225f)
                {
                    if ((double)hoverTime > 1.7)
                        jetPackFlameCounter -= 0.0225f;
                    else if (hoverTime > 0.8f)
                    {
                        jetPackFlameCount++;
                        jetPackFlameCounter -= (jetPackFlameCount % 8 < 6) ? 0.0225f : 0.066f;
                    }
                    else if (hoverTime > 0.4f)
                    {
                        jetPackFlameCount++;
                        jetPackFlameCounter -= (jetPackFlameCount % 6 < 4) ? 0.0225f : 0.066f;
                    }
                    else
                    {
                        jetPackFlameCount++;
                        jetPackFlameCounter -= (jetPackFlameCount % 4 < 2) ? 0.0225f : 0.066f;
                    }
                    if (hoverTime > 1.5f || Y > owner.groundHeight + 16f)
                    {
                        owner.CallMethod("CreateJetpackFlames", currentJetpackDirection);
                    }
                }
            }
            else if (jetpackAudio != null && jetpackAudio.isPlaying)
            {
                jetpackAudio.Stop();
            }
        }

        public override bool HandleJump(bool wallJump)
        {
            if (!wallJump)
            {
                BlastOff();
            }
            else
            {
                hoverTime = hoverDuration;
            }
            if (owner.left)
            {
                currentJetpackDirection = jetpackSideDirection;
            }
            else if (owner.right)
            {
                currentJetpackDirection = new Vector3(jetpackSideDirection.x * -1f, jetpackSideDirection.y, 0f);
            }
            return true;
        }

        public override bool HandleWallDrag(bool value)
        {
            if (value)
            {
                hoverTime = 0f;
            }
            return true;
        }

        public override bool HandleDeath()
        {
            if (jetpackAudio != null && jetpackAudio.isPlaying)
                jetpackAudio.Stop();
            return true;
        }

        public override bool HandleLand()
        {
            float airdashTime = owner.GetFieldValue<float>("airdashTime");
            DirectionEnum airdashDirection = owner.GetFieldValue<DirectionEnum>("airdashDirection");
            if (airdashTime > 0f && airdashDirection == DirectionEnum.Down)
            {
                float groundHeightGround = owner.CallMethod<float>("GetGroundHeightGround");
                if (Mathf.Abs(groundHeightGround - Y) < 24f)
                {
                    owner.Y = groundHeightGround;
                    owner.groundHeight = groundHeightGround;
                }
                MakeDashBlast(X, owner.groundHeight + 9f, true);
                if (owner.GetFieldValue<float>("pressedJumpInAirSoJumpIfTouchGroundGrace") <= 0f)
                {
                    owner.SetFieldValue("stampDelay", 0.6f);
                    owner.frame = -3;
                    owner.yI = 0f;
                }
                else
                {
                    owner.CallMethod("Jump", true);
                }
                owner.CallMethod("SetInvulnerable", downSlamInvulnerabilityTime, false, false);
                owner.SetFieldValue("airdashDirection", DirectionEnum.Any);
                downwardDashDirection = DirectionEnum.None;
                owner.SetFieldValue("airdashTime", 0f);
                owner.xI = 0f;
                owner.xIBlast = 0f;
            }
            else
            {
                if (owner.GetFieldValue<float>("pressedJumpInAirSoJumpIfTouchGroundGrace") > 0f && owner.yI <= 0f)
                {
                    owner.CallMethod("Jump", true);
                }
                else
                {
                    hoverTime = 0f;
                }
            }
            jetPackFlameCounter = 0f;
            return true;
        }

        public override void Cleanup()
        {
            if (jetpackAudio != null)
            {
                Object.Destroy(jetpackAudio);
            }
        }
    }
}

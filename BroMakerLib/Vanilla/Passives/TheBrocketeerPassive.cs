using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using BroMakerLib.Loggers;
using BroMakerLib.Vanilla.Specials;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Passives
{
    /// <summary>TheBrocketeer's jetpack hover.</summary>
    [PassivePreset("Brocketeer")]
    public class TheBrocketeerPassive : PassiveAbility
    {
        protected override HeroType SourceBroType => HeroType.TheBrocketeer;

        public float hoverDuration = 2.2f;
        public float defaultHoverForce = 0.09f;
        public float hoverSinForce = 0.08f;
        public float hoverSinStart = 20f;
        public float hoverSinSpeed = 12f;
        public float jetPackRate = 0.04f;
        /// <summary>Upward velocity impulse added on takeoff.</summary>
        public float blastOffYBoost = 100f;

        /// <summary>When true, takeoff spawns a FlameWallExplosion. Disable for bros without a flame-wall prefab on the owner.</summary>
        public bool enableBlastOffExplosion = true;
        public Vector3 jetpackSideDirection = new Vector3(160f, -330f, 0f);
        public Vector3 jetpackDownDirection = new Vector3(0f, -370f, 0f);
        public AudioClip jetpackLiftSound;

        [JsonIgnore] private float hoverTime;
        [JsonIgnore] private float hoverSinCounter;
        [JsonIgnore] private float jetPackCounter;
        [JsonIgnore] private float jetPackFlameCounter;
        [JsonIgnore] private int jetPackFlameCount;
        [JsonIgnore] private Vector3 currentJetpackDirection;
        [JsonIgnore] private AudioSource jetpackAudio;

        protected override bool IsOwnerRedundant(TestVanDammeAnim owner) => owner is TheBrocketeer;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            if (IsRedundant) return;

            jetpackAudio = owner.gameObject.AddComponent<AudioSource>();
            jetpackAudio.rolloffMode = AudioRolloffMode.Linear;
            jetpackAudio.minDistance = 350f;
            jetpackAudio.maxDistance = 500f;
            jetpackAudio.spatialBlend = 1f;
            jetpackAudio.volume = 0.13f;
            jetpackAudio.dopplerLevel = 0f;
            jetpackAudio.pitch = 1f;
            jetpackAudio.clip = jetpackLiftSound;
            jetpackAudio.loop = true;
            jetpackAudio.playOnAwake = false;
            jetpackAudio.Stop();

            currentJetpackDirection = jetpackDownDirection;
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as TheBrocketeer;
            if (sourceBro == null) return;
            if (jetpackLiftSound == null) jetpackLiftSound = sourceBro.jetpackLiftSound;
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

        public override void HandleAfterLand()
        {
            if (owner.GetFieldValue<float>("pressedJumpInAirSoJumpIfTouchGroundGrace") > 0f && owner.yI <= 0f)
            {
                hero.Jump(true);
            }
            else
            {
                hoverTime = 0f;
            }
            jetPackFlameCounter = 0f;
        }

        public override void Update()
        {
            if (owner.health <= 0)
            {
                if (jetpackAudio != null && jetpackAudio.isPlaying)
                    jetpackAudio.Stop();
                return;
            }

            bool buttonJump = owner.GetFieldValue<bool>("buttonJump");
            float airdashTime = owner.GetFieldValue<float>("airdashTime");
            float suspendedAirdashDelay = (hero.SpecialAbility as TheBrocketeerSpecial)?.suspendedAirdashDelay ?? 0f;

            if (buttonJump && (airdashTime <= 0f || suspendedAirdashDelay > 0f))
            {
                RunJetpack();
            }
            else if (jetpackAudio != null && jetpackAudio.isPlaying)
            {
                jetpackAudio.Stop();
            }
        }

        public void BlastOff()
        {
            if (enableBlastOffExplosion)
            {
                owner.CallMethod("CreateBlastOffFlames", Y);
            }
            hoverSinCounter = hoverSinStart;
            owner.yI += blastOffYBoost;
            hoverTime = hoverDuration;
            jetPackFlameCounter = 0f;
            if (jetpackAudio != null)
            {
                jetpackAudio.pitch = 1f;
                jetpackAudio.volume = 0.13f;
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

        public override void Cleanup()
        {
            if (jetpackAudio != null)
            {
                Object.Destroy(jetpackAudio);
            }
        }
    }
}

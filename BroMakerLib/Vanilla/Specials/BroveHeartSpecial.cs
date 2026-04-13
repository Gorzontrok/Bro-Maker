using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Effects;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BroHeart")]
    public class BroveHeartSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BroveHeart;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special3Sounds == null) special3Sounds = sourceBro.soundHolder.special3Sounds.CloneArray();
            if (attack4Sounds == null) attack4Sounds = sourceBro.soundHolder.attack4Sounds.CloneArray();
        }
        public float heroBoostDuration = 5f;
        public float specialDuration = 6.7f;
        public float initialScareRadius = 24f;
        public float maxScareRadius = 96f;
        public float scareInterval = 0.0667f;
        public float scareRadiusGrowth = 4f;
        public float scareRadiusShrinkThreshold = 2f;
        public float shrinkRadius = 32f;
        public float trailInterval = 0.1f;
        public float audioVolume = 0.45f;
        public float audioPitch = 0.87f;
        public float attack4SoundVolume = 0.33f;
        public float attack4SoundPitch = 0.9f;
        public float attack4SoundTime = 1.7f;
        public float bubbleLife = 4f;
        public AudioClip[] attack4Sounds;

        public AudioClip[] special3Sounds;
        [JsonIgnore]
        private float broveheartSpecialTime;
        [JsonIgnore]
        private float specialScareDelay;
        [JsonIgnore]
        private float specialScareRadius;
        [JsonIgnore]
        private float trailDelay;
        [JsonIgnore]
        private int lastPlayernumScared;
        [JsonIgnore]
        private int trailCount;
        [JsonIgnore]
        private float avatarAnimDelay = 0.05f;
        [JsonIgnore]
        private int avatarFrame;
        [JsonIgnore]
        private AudioSource freedomCryAudio;
        [JsonIgnore]
        private ReactionBubble freedomBubble;
        [JsonIgnore]
        private bool resolvedBubble;
        [JsonIgnore]
        private SpriteSM faderSpritePrefab;
        [JsonIgnore]
        private List<IPoolableEffect> bubbleFaderEffects = new List<IPoolableEffect>();

        public BroveHeartSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            freedomCryAudio = owner.gameObject.AddComponent<AudioSource>();
            freedomCryAudio.rolloffMode = AudioRolloffMode.Linear;
            freedomCryAudio.minDistance = 550f;
            freedomCryAudio.maxDistance = 600f;
            freedomCryAudio.spatialBlend = 1f;
            freedomCryAudio.volume = audioVolume;
            freedomCryAudio.dopplerLevel = 0f;
            freedomCryAudio.pitch = audioPitch;
            if (special3Sounds != null && special3Sounds.Length > 0)
            {
                freedomCryAudio.clip = special3Sounds[Random.Range(0, special3Sounds.Length)];
            }
            freedomCryAudio.loop = false;
            freedomCryAudio.playOnAwake = false;
            freedomCryAudio.Stop();

            faderSpritePrefab = owner.faderSpritePrefab;
            if (faderSpritePrefab == null)
            {
                var broveHeartPrefab = HeroController.GetHeroPrefab(HeroType.BroveHeart) as BroveHeart;
                if (broveHeartPrefab != null)
                {
                    faderSpritePrefab = broveHeartPrefab.faderSpritePrefab;
                }
            }

        }

        private ReactionBubble GetFreedomBubble()
        {
            if (!resolvedBubble)
            {
                resolvedBubble = true;
                var broveHeart = owner as BroveHeart;
                if (broveHeart != null)
                {
                    freedomBubble = broveHeart.freedomBubble;
                }
                else
                {
                    var prefab = HeroController.GetHeroPrefab(HeroType.BroveHeart) as BroveHeart;
                    if (prefab != null && prefab.freedomBubble != null)
                    {
                        freedomBubble = Object.Instantiate(prefab.freedomBubble, owner.transform);
                        freedomBubble.transform.localPosition = prefab.freedomBubble.transform.localPosition;
                        freedomBubble.gameObject.SetActive(false);
                    }
                }
            }
            return freedomBubble;
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                freedomCryAudio.time = 0f;
                freedomCryAudio.Play();
                owner.SpecialAmmo--;
                specialScareDelay = 0.15f;
                HeroController.BoostHeroes(heroBoostDuration);
                ReturnBubbleFaderEffects();
                broveheartSpecialTime = specialDuration;
                specialScareRadius = initialScareRadius;
                var bubble = GetFreedomBubble();
                if (bubble != null)
                {
                    bubble.ShowBubble();
                    bubble.life = bubbleLife;
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override void Update()
        {
            if (owner.health > 0 && broveheartSpecialTime > 0f)
            {
                freedomCryAudio.pitch = 0.42f + 0.45f * Time.timeScale;
                broveheartSpecialTime -= hero.DeltaTime;
                if (broveheartSpecialTime <= 0f)
                {
                    ReturnBubbleFaderEffects();
                    HeroController.SetAvatarCalm(PlayerNum, true);
                }
                specialScareDelay -= hero.DeltaTime;
                trailDelay -= hero.DeltaTime;
                if (broveheartSpecialTime < attack4SoundTime && broveheartSpecialTime + hero.DeltaTime >= attack4SoundTime)
                {
                    Sound.GetInstance().PlaySoundEffectAt(attack4Sounds, attack4SoundVolume, owner.transform.position, attack4SoundPitch);
                }
                if (trailDelay < 0f && broveheartSpecialTime > 0.6f)
                {
                    CreateBubbleFaderTrailInstance();
                    trailDelay = trailInterval;
                }
                if (specialScareDelay < 0f)
                {
                    SortOfFollow.Shake(0.15f);
                    specialScareDelay = scareInterval;
                    for (int i = 0; i < 4; i++)
                    {
                        lastPlayernumScared = (lastPlayernumScared + 1) % 4;
                        if (HeroController.IsPlaying(i) && HeroController.players[i] != null && HeroController.players[i].character != null && HeroController.players[i].IsAlive())
                        {
                            lastPlayernumScared = i;
                            Map.PanicUnits(HeroController.players[i].character.X, HeroController.players[i].character.Y, specialScareRadius, 1f, false, false);
                            i = 5;
                            if (broveheartSpecialTime > scareRadiusShrinkThreshold)
                            {
                                if (specialScareRadius < maxScareRadius)
                                {
                                    specialScareRadius += scareRadiusGrowth;
                                }
                            }
                            else if (specialScareRadius > shrinkRadius)
                            {
                                specialScareRadius -= scareRadiusGrowth;
                            }
                        }
                    }
                    if (GameModeController.IsDeathMatchMode || GameModeController.GameMode == GameMode.BroDown)
                    {
                        Map.StunUnits(PlayerNum, X, Y, specialScareRadius, 0.4f);
                    }
                }
            }
        }

        public override bool HandleRunAvatarFiring()
        {
            if (broveheartSpecialTime <= 0f || owner.health <= 0)
            {
                return true;
            }
            avatarAnimDelay -= hero.DeltaTime;
            if (avatarAnimDelay < 0f)
            {
                avatarAnimDelay = 0.05f;
                avatarFrame = (avatarFrame + 1) % 4;
            }
            typeof(HeroController).CallMethod("SetAvatarFrame", PlayerNum, 4 + avatarFrame);
            return false;
        }

        public override bool HandleRunGun()
        {
            if (owner is BroveHeart)
            {
                return true;
            }
            if (broveheartSpecialTime > 0f)
            {
                int gunFrame = hero.GunFrame;
                if (gunFrame == 5)
                {
                    if (owner.IsOnGround())
                    {
                        owner.xI = 0f;
                        owner.yI = 0f;
                    }
                }
            }
            return true;
        }

        public override bool HandleDeath()
        {
            ReturnBubbleFaderEffects();
            return true;
        }

        public override void HandleAfterDeath()
        {
            freedomCryAudio.Stop();
        }

        private void ReturnBubbleFaderEffects()
        {
            for (int i = 0; i < bubbleFaderEffects.Count; i++)
            {
                bubbleFaderEffects[i].EffectDie();
            }
            bubbleFaderEffects.Clear();
        }

        private void CreateBubbleFaderTrailInstance()
        {
            var bubble = GetFreedomBubble();
            if (faderSpritePrefab == null || bubble == null)
            {
                return;
            }
            trailCount++;
            FaderSpriteBroveHeart component = faderSpritePrefab.GetComponent<FaderSpriteBroveHeart>();
            if (component == null)
            {
                return;
            }
            FaderSpriteBroveHeart fader = EffectsController.InstantiateEffect(component, bubble.transform.position + Vector3.forward * 3f, owner.transform.rotation) as FaderSpriteBroveHeart;
            if (fader != null)
            {
                bubbleFaderEffects.Add(fader);
                fader.SetMaterial(bubble.GetComponent<Renderer>().material, bubble.GetComponent<SpriteSM>().lowerLeftPixel, bubble.GetComponent<SpriteSM>().pixelDimensions, bubble.GetComponent<SpriteSM>().offset);
                fader.growToSize = 1.4f;
                fader.transform.parent = bubble.transform;
                int num = trailCount % 3;
                if (num == 0)
                {
                    fader.SetColor(Color.red);
                }
                else if (num == 1)
                {
                    fader.SetColor(Color.white);
                }
                else if (num == 2)
                {
                    fader.SetColor(Color.blue);
                }
            }
        }

        public override void Cleanup()
        {
            if (freedomCryAudio != null)
            {
                Object.Destroy(freedomCryAudio);
            }
            if (freedomBubble != null && !(owner is BroveHeart))
            {
                Object.Destroy(freedomBubble.gameObject);
            }
        }
    }
}

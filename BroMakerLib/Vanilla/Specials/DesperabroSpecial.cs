using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Desperabro")]
    public class DesperabroSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.Desperabro;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special4Sounds == null) special4Sounds = sourceBro.soundHolder.special4Sounds.CloneArray();
        }

        public float blindRange = 200f;
        public float blindDuration = 9f;
        public float shockWaveRange = 64f;
        public float danceRange = 200f;
        public float danceYRange = 64f;
        public float danceDuration = 0.2f;
        public float danceInterval = 0.0334f;
        public float serenadeAutoFinishTime = 4.2f;

        public AudioClip[] special4Sounds;
        [JsonIgnore]
        private bool isSerenading;
        [JsonIgnore]
        private float serenadeTimer;
        [JsonIgnore]
        private float attractCounter;
        [JsonIgnore]
        private float guitarSpriteCounter;
        [JsonIgnore]
        private int guitarSpriteFrame;
        [JsonIgnore]
        private Desperabro campaBro;
        [JsonIgnore]
        private Desperabro quinoBro;
        [JsonIgnore]
        private AudioSource serenadeAudio;
        [JsonIgnore]
        private ParticleSystem musicParticles;
        [JsonIgnore]
        private SpriteSM guitarGunSprite;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var desperabro = owner as Desperabro;
            if (desperabro != null)
            {
                musicParticles = desperabro.musicParticles;
                guitarGunSprite = desperabro.guitarGunSprite;
            }
            else
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.Desperabro) as Desperabro;
                if (prefab != null)
                {
                    if (prefab.musicParticles != null)
                    {
                        musicParticles = Object.Instantiate(prefab.musicParticles, owner.transform);
                        musicParticles.transform.localPosition = prefab.musicParticles.transform.localPosition;
                        musicParticles.Stop();
                    }
                    if (prefab.guitarGunSprite != null)
                    {
                        guitarGunSprite = Object.Instantiate(prefab.guitarGunSprite, owner.transform);
                        guitarGunSprite.transform.localPosition = prefab.guitarGunSprite.transform.localPosition;
                        guitarGunSprite.gameObject.SetActive(false);
                    }
                }
            }
        }

        public override void UseSpecial()
        {
            if (isSerenading)
            {
                FinishSerenadingAndUnleashHell();
                return;
            }
            if (owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    StartSerenading();
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }

        private void StartSerenading()
        {
            if (serenadeAudio != null)
            {
                serenadeAudio.Stop();
                serenadeAudio = null;
            }
            isSerenading = true;
            hero.UsingSpecial = true;
            attractCounter = 0f;
            serenadeTimer = 0f;
            guitarSpriteCounter = 0f;
            owner.canDuck = false;
            owner.canWallClimb = false;
            if (musicParticles != null)
            {
                musicParticles.Play();
            }
            hero.DeactivateGun();
            if (guitarGunSprite != null)
            {
                guitarGunSprite.gameObject.SetActive(true);
            }
            Sound.GetInstance().StartDippingMusicVolume(0f);
            var desperabro = owner as Desperabro;
            if (desperabro != null)
            {
                var idleAudio = desperabro.GetFieldValue<AudioSource>("idleSerenadeAudio");
                if (idleAudio != null)
                {
                    idleAudio.volume = 0f;
                }
            }
            SpawnMariachiBand();
            serenadeAudio = sound.PlaySoundEffectAt(special4Sounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
        }

        private void SpawnMariachiBand()
        {
            if (!owner.IsMine) return;
            if (campaBro != null)
            {
                campaBro.Death(0f, 0f, null);
                campaBro = null;
            }
            if (quinoBro != null)
            {
                quinoBro.Death(0f, 0f, null);
                quinoBro = null;
            }
            var prefab = HeroController.GetHeroPrefab(HeroType.Desperabro);
            float pushSpeed = owner.IsOnGround() ? 300f : 30f;
            campaBro = Networking.Networking.InstantiateBuffered<TestVanDammeAnim>(prefab,
                owner.transform.position, owner.transform.rotation, new object[0], false) as Desperabro;
            quinoBro = Networking.Networking.InstantiateBuffered<TestVanDammeAnim>(prefab,
                owner.transform.position, owner.transform.rotation, new object[0], false) as Desperabro;
            if (campaBro != null)
            {
                campaBro.X = X;
                campaBro.name = "[CAMPA] " + campaBro.name;
                campaBro.xI = -pushSpeed;
                campaBro.ForceFaceDirection(-1);
                campaBro.SetFieldValue("desperabro", owner as Desperabro);
            }
            if (quinoBro != null)
            {
                quinoBro.X = X;
                quinoBro.name = "[QUINO] " + quinoBro.name;
                quinoBro.xI = pushSpeed;
                if (campaBro != null)
                {
                    campaBro.ForceFaceDirection(1);
                }
                quinoBro.SetFieldValue("desperabro", owner as Desperabro);
            }
            if (campaBro != null)
            {
                campaBro.CallMethod("SetupMariachiBro", campaBro, PlayerNum, 0.7f, 0);
            }
            if (quinoBro != null)
            {
                quinoBro.CallMethod("SetupMariachiBro", quinoBro, PlayerNum, 0.1f, 1);
            }
        }

        private void FinishSerenadingAndUnleashHell()
        {
            Sound.GetInstance().StopDippingMusicVolume();
            StatisticsController.AddBrotality(2);
            StopSerenading();
            if (campaBro != null && campaBro.IsAlive())
            {
                campaBro.CallMethod("StopSerenading");
                campaBro.SetFieldValue("mookScanTimer", 0.5f);
            }
            if (quinoBro != null && quinoBro.IsAlive())
            {
                quinoBro.CallMethod("StopSerenading");
                quinoBro.SetFieldValue("mookScanTimer", 0.5f);
            }
            Map.BlindUnits(PlayerNum, X, Y, blindRange, blindDuration);
            ExplosionGroundWave wave = EffectsController.CreateMusicalShockWave(X, Y + owner.headHeight, shockWaveRange);
            FullScreenFlashEffect.FlashHot(1f, owner.transform.position);
            wave.playerNum = PlayerNum;
            wave.avoidObject = owner;
            wave.origins = owner;
            if (owner.IsOnGround())
            {
                hero.Jump(false);
            }
        }

        private void StopSerenading()
        {
            if (isSerenading)
            {
                isSerenading = false;
                hero.UsingSpecial = false;
                owner.canDuck = true;
                owner.canWallClimb = true;
                if (musicParticles != null) musicParticles.Stop();
                if (guitarGunSprite != null) guitarGunSprite.gameObject.SetActive(false);
                hero.ActivateGun();
                if (serenadeAudio != null)
                {
                    serenadeAudio.Stop();
                    serenadeAudio = null;
                }
            }
        }

        public override void Update()
        {
            if (!isSerenading) return;

            if (serenadeAudio != null && !serenadeAudio.isPlaying)
            {
                serenadeAudio = sound.PlaySoundEffectAt(special4Sounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            attractCounter += hero.DeltaTime;
            if (attractCounter >= danceInterval)
            {
                attractCounter -= danceInterval;
                Map.MakeMooksDance(X, Y, danceRange, danceYRange, danceDuration);
            }
            serenadeTimer += hero.DeltaTime;
            if (serenadeTimer >= serenadeAutoFinishTime)
            {
                FinishSerenadingAndUnleashHell();
                return;
            }
            owner.CallMethod("set_WallDrag", false);
            AnimateGuitarSprite();
        }

        private void AnimateGuitarSprite()
        {
            if (guitarGunSprite == null) return;
            guitarSpriteCounter += hero.DeltaTime;
            if (guitarSpriteCounter > 0.0334f)
            {
                guitarSpriteCounter -= 0.0334f;
                guitarSpriteFrame--;
                guitarGunSprite.SetLowerLeftPixel(32f * guitarSpriteFrame, 32f);
                if (guitarSpriteFrame <= 0)
                {
                    guitarSpriteFrame = 27;
                }
            }
        }

        public override bool HandleJump(bool wallJump)
        {
            if (wallJump && isSerenading)
            {
                return false;
            }
            return true;
        }

        public override bool HandleStartMelee()
        {
            if (isSerenading)
            {
                FinishSerenadingAndUnleashHell();
                return false;
            }
            return true;
        }

        public override bool HandleStartFiring()
        {
            if (isSerenading)
            {
                FinishSerenadingAndUnleashHell();
                return false;
            }
            return true;
        }

        public override bool HandleDeath()
        {
            if (isSerenading)
            {
                Sound.GetInstance().StopDippingMusicVolume();
            }
            StopSerenading();
            if (guitarGunSprite != null) guitarGunSprite.gameObject.SetActive(false);
            return true;
        }

        public override bool HandleGib(DamageType damageType, float xI, float yI)
        {
            if (isSerenading)
            {
                Sound.GetInstance().StopDippingMusicVolume();
            }
            return true;
        }

        public override void Cleanup()
        {
            if (!(owner is Desperabro))
            {
                if (musicParticles != null) Object.Destroy(musicParticles.gameObject);
                if (guitarGunSprite != null) Object.Destroy(guitarGunSprite.gameObject);
            }
        }
    }
}

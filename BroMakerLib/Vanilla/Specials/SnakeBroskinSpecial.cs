using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("SnakeBroskin")]
    public class SnakeBroskinSpecial : SpecialAbility
    {
        public string grenadeName = "Hologram";
        public float throwSoundVolume = 0.4f;
        public float teleportSoundVolume = 0.4f;
        public float fadeToHoloDuration = 0.75f;
        public int teleportExplosionDamage = 20;
        public float teleportExplosionRange = 16f;

        // Standing throw
        public float standingXOffset = 8f;
        public float standingYOffset = 8f;
        public float standingThrowXI = 200f;
        public float standingThrowYI = 150f;

        // Ducking throw
        public float duckingXOffset = 6f;
        public float duckingYOffset = 3f;
        public float duckingThrowXI = 30f;
        public float duckingThrowYI = 70f;

        // Flipping off animation (when hologram active)
        public int flipAnimationRow = 11;
        public int flipAnimationColumn = 25;
        public int flipAnimationFrameCount = 5;
        public float flipFrameRate = 0.075f;

        // Normal throw animation
        public int throwAnimationRow = 5;
        public int throwAnimationColumn = 17;
        public int throwAnimationFrameCount = 8;
        public float throwFrameRate = 0.0334f;

        public AudioClip[] special2Sounds;

        [JsonIgnore]
        private Grenade specialGrenade;
        [JsonIgnore]
        private GrenadeHologram grenadeHologram;
        [JsonIgnore]
        private bool flippingOff;
        [JsonIgnore]
        private bool fadeToHolo;
        [JsonIgnore]
        private float fadeToHoloTimeLeft;
        [JsonIgnore]
        private Material hologramMaterial;
        [JsonIgnore]
        private SpriteSM faderSpritePrefab;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            specialGrenade = LoadBroforceObjects.GetGrenadeFromName(grenadeName);
            faderSpritePrefab = owner.faderSpritePrefab;

            var snakeBroskin = owner as SnakeBroskin;
            if (snakeBroskin == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.SnakeBroSkin);
                snakeBroskin = prefab as SnakeBroskin;
            }
            if (snakeBroskin != null)
            {
                hologramMaterial = snakeBroskin.hologramMaterial;
                if (faderSpritePrefab == null)
                    faderSpritePrefab = snakeBroskin.faderSpritePrefab;
                if (throwSounds == null) throwSounds = snakeBroskin.soundHolder.throwSounds;
                if (special2Sounds == null) special2Sounds = snakeBroskin.soundHolder.special2Sounds;
            }
        }

        public override void PressSpecial()
        {
            if (grenadeHologram != null && grenadeHologram.hologramActive)
            {
                flippingOff = true;
                grenadeHologram.SetMinLife(1f);
            }
            base.PressSpecial();
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            if (flippingOff)
            {
                hero.FrameRate = flipFrameRate;
                int column = flipAnimationColumn + Mathf.Clamp(owner.frame, 0, flipAnimationFrameCount - 1);
                hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, flipAnimationRow * hero.SpritePixelHeight);
            }
            else
            {
                hero.FrameRate = throwFrameRate;
                int column = throwAnimationColumn + Mathf.Clamp(owner.frame, 0, throwAnimationFrameCount - 1);
                hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, throwAnimationRow * hero.SpritePixelHeight);
            }
            if (owner.frame == 4)
            {
                UseSpecial();
            }
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.UsingSpecial = false;
                hero.UsingPockettedSpecial = false;
                hero.ActivateGun();
                hero.ChangeFrame();
            }
        }

        public override void UseSpecial()
        {
            if (grenadeHologram != null)
            {
                if (grenadeHologram.hologramActive)
                {
                    StartTeleportToHologram();
                }
            }
            else if (owner.SpecialAmmo > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(throwSounds, throwSoundVolume, owner.transform.position);
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    if (owner.down && owner.IsOnGround() && hero.Ducking)
                    {
                        Grenade grenade = ProjectileController.SpawnGrenadeOverNetwork(specialGrenade, owner,
                            X + Mathf.Sign(owner.transform.localScale.x) * duckingXOffset, Y + duckingYOffset,
                            0.001f, 0.011f,
                            Mathf.Sign(owner.transform.localScale.x) * duckingThrowXI, duckingThrowYI,
                            PlayerNum, 1f);
                        grenadeHologram = grenade.GetComponent<GrenadeHologram>();
                    }
                    else
                    {
                        Grenade grenade = ProjectileController.SpawnGrenadeOverNetwork(specialGrenade, owner,
                            X + Mathf.Sign(owner.transform.localScale.x) * standingXOffset, Y + standingYOffset,
                            0.001f, 0.011f,
                            Mathf.Sign(owner.transform.localScale.x) * standingThrowXI, standingThrowYI,
                            PlayerNum, 1f);
                        grenadeHologram = grenade.GetComponent<GrenadeHologram>();
                    }
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if (grenadeHologram != null && grenadeHologram.hologramActive)
            {
                if (!fadeToHolo)
                {
                    StartTeleportToHologram();
                }
                return false;
            }
            return true;
        }

        public override bool HandleUpdate()
        {
            if (fadeToHolo)
            {
                return false;
            }
            return true;
        }

        public override void Update()
        {
            if (fadeToHolo)
            {
                fadeToHoloTimeLeft -= Time.deltaTime;
                if (fadeToHoloTimeLeft < 0f)
                {
                    fadeToHolo = false;
                    FinishTeleportToHologram();
                }
            }
        }

        public override bool HandleApplyFallingGravity()
        {
            if (fadeToHolo) return false;
            return true;
        }

        public override bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            if (fadeToHolo)
            {
                xI = 0f;
                yI = 0f;
                return false;
            }
            return base.HandleCalculateMovement(ref xI, ref yI);
        }

        private void StartTeleportToHologram()
        {
            fadeToHolo = true;
            fadeToHoloTimeLeft = fadeToHoloDuration;
            grenadeHologram.SetMinLife(1f);
            CreateHoloTrailInstance();
            owner.GetComponent<Renderer>().enabled = false;
            owner.gunSprite.enabled = false;
            hero.DeactivateGun();
            owner.invulnerable = true;
            owner.xIBlast = 0f;
            owner.yIBlast = 0f;
            owner.xI = 0f;
            owner.yI = 0f;
        }

        private void FinishTeleportToHologram()
        {
            fadeToHolo = false;
            owner.GetComponent<Renderer>().enabled = true;
            owner.invulnerable = false;
            owner.SetXY(grenadeHologram.X, grenadeHologram.Y);
            owner.xIBlast = 0f;
            owner.yIBlast = 0f;
            owner.xI = 0f;
            owner.yI = 0f;
            Sound.GetInstance().PlaySoundEffectAt(special2Sounds, teleportSoundVolume, owner.transform.position, 1f);
            flippingOff = false;
            Map.ExplodeUnits(owner, teleportExplosionDamage, DamageType.Explosion, teleportExplosionRange, teleportExplosionRange,
                X, Y, 0f, 0f, PlayerNum, false, false, false);
            if (grenadeHologram != null)
            {
                grenadeHologram.Death();
            }
            grenadeHologram = null;
        }

        private void CreateHoloTrailInstance()
        {
            if (faderSpritePrefab == null || hologramMaterial == null)
            {
                return;
            }
            FaderSprite component = faderSpritePrefab.GetComponent<FaderSprite>();
            if (component == null)
            {
                return;
            }
            FaderSprite fader = EffectsController.InstantiateEffect(component, owner.transform.position, owner.transform.rotation) as FaderSprite;
            if (fader != null)
            {
                fader.transform.localScale = owner.transform.localScale;
                fader.SetMaterial(hologramMaterial,
                    new Vector2(28 * hero.SpritePixelWidth, 11 * hero.SpritePixelHeight),
                    hero.Sprite.pixelDimensions, hero.Sprite.offset);
                fader.fadeM = 1.5f;
                fader.maxLife = 1f;
            }
        }
    }
}

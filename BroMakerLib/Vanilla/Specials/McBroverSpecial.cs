using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>MacBrover's turkey-bomb throw special.</summary>
    [SpecialPreset("MacBrover")]
    public class McBroverSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.McBrover;
        /// <summary>Playback volume for the throw sound.</summary>
        public float specialSoundVolume = 0.5f;
        /// <summary>Horizontal launch speed when standing.</summary>
        public float standingSpeedX = 100f;
        /// <summary>Vertical launch speed when standing.</summary>
        public float standingSpeedY = 100f;
        /// <summary>Horizontal launch speed when ducking.</summary>
        public float duckingSpeedX = 35f;
        /// <summary>Vertical launch speed when ducking.</summary>
        public float duckingSpeedY = 70f;
        /// <summary>Fraction of the bro's horizontal velocity added to the turkey's launch speed when standing.</summary>
        public float momentumX = 0.7f;
        /// <summary>Fraction of the bro's upward velocity added to the turkey's launch speed when standing.</summary>
        public float momentumY = 0.5f;
        /// <summary>Fraction of the bro's horizontal velocity added to the turkey's launch speed when ducking.</summary>
        public float duckingMomentumX = 0.3f;

        [JsonIgnore]
        protected Projectile turkeyProjectile;
        [JsonIgnore]
        protected Projectile currentTurkey;

        public McBroverSpecial()
        {
            spawnOffsetX = 6f;
            spawnOffsetY = 6.5f;
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.McBrover);
            var mcBrover = prefab.GetComponent<McBrover>();
            if (mcBrover != null)
            {
                turkeyProjectile = mcBrover.turkeyProjectile;
            }
        }

        public override void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || owner.health <= 0)
            {
                return;
            }
            if (owner.SpecialAmmo > 0 || currentTurkey != null)
            {
                UseSpecial();
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
            }
        }

        public override void UseSpecial()
        {
            hero.GunFrame = 3;
            owner.gunSprite.SetLowerLeftPixel(32f * hero.GunFrame, 32f);
            if (currentTurkey != null)
            {
                currentTurkey.Death();
            }
            else if (owner.SpecialAmmo > 0)
            {
                Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, specialSoundVolume, owner.transform.position);
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                if (owner.IsMine)
                {
                    if (owner.down && owner.IsOnGround() && hero.Ducking)
                    {
                        currentTurkey = ProjectileController.SpawnProjectileOverNetwork(turkeyProjectile, owner, X + Direction * spawnOffsetX, Y + spawnOffsetY, Direction * duckingSpeedX + owner.xI * duckingMomentumX, duckingSpeedY, false, PlayerNum, false, false, 0f);
                    }
                    else
                    {
                        currentTurkey = ProjectileController.SpawnProjectileOverNetwork(turkeyProjectile, owner, X + Direction * spawnOffsetX, Y + spawnOffsetY, Direction * standingSpeedX + owner.xI * momentumX, standingSpeedY + ((owner.yI <= 0f) ? 0f : (owner.yI * momentumY)), false, PlayerNum, false, false, 0f);
                    }
                }
                hero.UsingSpecial = false;
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                owner.gunSprite.gameObject.SetActive(true);
                hero.UsingSpecial = false;
            }
            hero.PressSpecialFacingDirection = 0;
        }
    }
}

using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("McBrover")]
    public class McBroverSpecial : SpecialAbility
    {
        public float specialSoundVolume = 0.5f;
        public float standingSpeedX = 100f;
        public float standingSpeedY = 100f;
        public float duckingSpeedX = 35f;
        public float duckingSpeedY = 70f;
        public float momentumX = 0.7f;
        public float momentumY = 0.5f;
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

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.McBrover);
            var mcBrover = prefab.GetComponent<McBrover>();
            if (mcBrover != null)
            {
                turkeyProjectile = mcBrover.turkeyProjectile;
            }
            if (specialAttackSounds == null)
            {
                var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
                specialAttackSounds = sourceBro.soundHolder.specialAttackSounds;
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

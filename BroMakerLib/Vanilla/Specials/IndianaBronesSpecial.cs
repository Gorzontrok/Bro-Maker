using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Indiana Brones's shotgun blast special.</summary>
    [SpecialPreset("IndianaBrones")]
    public class IndianaBronesSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.IndianaBrones;
        /// <summary>Name of the projectile prefab fired during the special.</summary>
        public string projectileName = "IndianaBrones";
        /// <summary>Horizontal speed of the fired projectile.</summary>
        public float fireSpeedX = 800f;
        /// <summary>Maximum random vertical speed variance applied to each shot.</summary>
        public float fireSpeedYVariance = 10f;
        /// <summary>Vertical recoil applied to the bro on firing.</summary>
        public float recoilY = 50f;
        /// <summary>Horizontal recoil applied to the bro on firing.</summary>
        public float recoilX = 50f;
        /// <summary>Camera shake magnitude on firing.</summary>
        public float shakeAmount = 0.4f;

        [JsonIgnore]
        protected Projectile projectile;

        public IndianaBronesSpecial()
        {
            spawnOffsetX = 14f;
            spawnOffsetY = 9f;
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            projectile = LoadBroforceObjects.GetProjectileFromName(projectileName);
        }

        public override void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.UsingSpecial || owner.health <= 0)
            {
                return;
            }
            if (owner.SpecialAmmo > 0 && owner.health > 0)
            {
                owner.SpecialAmmo--;
                float x = X + Direction * spawnOffsetX;
                float y = Y + spawnOffsetY;
                float xSpeed = Direction * fireSpeedX;
                float ySpeed = (float)Random.Range(-(int)fireSpeedYVariance, (int)fireSpeedYVariance);
                hero.GunFrame = 3;
                hero.SetGunSprite(hero.GunFrame, 1);
                EffectsController.CreateMuzzleFlashBigEffect(x + Mathf.Sign(xSpeed) * 5f, y, -25f, xSpeed * 0.03f, ySpeed * 0.03f);
                ProjectileController.SpawnProjectileLocally(projectile, owner, x, y, xSpeed, ySpeed, PlayerNum);
                Sound.GetInstance().PlaySoundEffectAt(attackSounds, 0.55f, owner.transform.position, 1f + owner.pitchShiftAmount);
                Map.DisturbWildLife(X, Y, 60f, PlayerNum);
                SortOfFollow.Shake(shakeAmount, shakeAmount);
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                owner.yI += recoilY;
                owner.xIBlast = -Direction * recoilX;
            }
        }
    }
}

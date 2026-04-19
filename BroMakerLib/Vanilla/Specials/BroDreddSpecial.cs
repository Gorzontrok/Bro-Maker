using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Bro Dredd's remote-controlled rocket.</summary>
    [SpecialPreset("BroDredd")]
    public class BroDreddSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BroDredd;
        /// <summary>Initial horizontal launch speed of the remote rocket.</summary>
        public float rocketSpeed = 90f;

        [JsonIgnore]
        protected Projectile specialRocketPrefab;

        public BroDreddSpecial()
        {
            animationColumn = 17;
            animationRow = 5;
            animationFrameCount = 8;
            triggerFrame = 4;
            spawnOffsetX = 6f;
            spawnOffsetY = 11f;
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.BroDredd);
            var broDredd = prefab.GetComponent<BroDredd>();
            if (broDredd != null)
            {
                specialRocketPrefab = broDredd.specialRocketPrefab;
            }
        }

        public override void PressSpecial()
        {
            if (owner.remoteProjectile != null && owner.remoteProjectile.gameObject.activeInHierarchy)
                return;
            base.PressSpecial();
        }

        public override void HandleAfterCheckInput()
        {
            if (owner.remoteProjectile == null || !owner.remoteProjectile.gameObject.activeInHierarchy)
                return;

            // Boost RPC fails (no NID on locally-spawned projectile), call directly
            if (owner.special && !owner.wasSpecial && !owner.remoteProjectile.doubleSpeed)
            {
                owner.remoteProjectile.SetDoubleSpeed();
            }
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                if (owner.IsMine)
                {
                    owner.remoteProjectile = ProjectileController.SpawnProjectileOverNetwork(specialRocketPrefab, owner, X + Direction * spawnOffsetX, Y + spawnOffsetY, Direction * rocketSpeed, 0f, true, PlayerNum, true, false, 0f);
                    if (owner.remoteProjectile != null)
                    {
                        hero.UsingSpecial = false;
                        owner.fire = false;
                        hero.ControllingProjectile = true;
                        owner.SetFieldValue("projectileTime", Time.time);
                        owner.frame = 0;
                    }
                }
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                owner.gunSprite.gameObject.SetActive(true);
                hero.UsingSpecial = false;
            }
        }
    }
}

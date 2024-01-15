using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Stats;
using RocketLib;
using System.Collections;
using UnityEngine;

namespace BroMakerLib.Abilities.Weapons
{
    [WeaponPreset("Default")]
    public class Weapon : CharacterAbility
    {
        public virtual Projectile Projectile
        {
            get
            {
                if(projectile != null)
                    return projectile;

                return owner.projectile;
            }
            set
            {
                projectile = value;
            }
        }

        public Projectile projectile;
        public bool spawnShrapnel = false;
        public bool makeEffect = true;

        protected Shrapnel bulletShell;
        protected TwoFloat shrapnelSpawnPositionMultipliyer = new TwoFloat(-15f, -3f);
        protected TwoFloat pushBackForce = TwoFloat.zero;

        protected override void Awake()
        {
            base.Awake();
            FixNullVariables();
        }
        protected virtual void FixNullVariables()
        {
            Rambro rambro = HeroController.GetHeroPrefab(HeroType.Rambro) as Rambro;
            bulletShell = rambro.bulletShell;
        }

        public virtual void CreateShrapnel(float x, float y, float radius, float force, float count, float xI, float yI)
        {
            EffectsController.CreateShrapnel(this.bulletShell,
                x + owner.transform.localScale.x * shrapnelSpawnPositionMultipliyer.x,
                y + owner.transform.localScale.y * shrapnelSpawnPositionMultipliyer.y,
                radius,
                force,
                count,
                xI,
                yI
            );
        }

        public bool CanFire()
        {
            return true;
        }

        public override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if (!CanFire())
                return;

            PrepareWeapon(x, y, xSpeed, ySpeed);
            SpawnProjectile(Projectile, x, y, xSpeed, ySpeed);
            AfterFire(x, y, xSpeed, ySpeed);
        }

        public virtual void SpawnProjectile(Projectile projectile, float x, float y, float xSpeed, float ySpeed)
        {
            ProjectileController.SpawnProjectileLocally(projectile, owner, x, y, xSpeed, ySpeed, owner.playerNum);
        }


        protected virtual void PrepareWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if (spawnShrapnel)
                CreateShrapnel(x, y, 1f, 30f, 1f, -owner.transform.localScale.x * 40f, 70f);
            if (makeEffect)
                OnFireEffect(x, y, xSpeed, ySpeed);
        }

        protected virtual void AfterFire(float x, float y, float xSpeed, float ySpeed)
        {
            owner.xIBlast -= owner.transform.localScale.x * pushBackForce.x;
            owner.yIBlast -= owner.transform.localScale.y * pushBackForce.y;
        }

        protected virtual void OnFireEffect(float x, float y, float xI, float yI)
        {
            EffectsController.CreateMuzzleFlashEffect(x, y, -25f, xI * 0.01f, yI * 0.01f, owner.transform);
        }
    }
}
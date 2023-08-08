using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Weapons
{
    public class Shotgun : WeaponBase
    {
        public int projectileNumber = 3;
        protected float yInterval = 0f;

        protected override void Start()
        {
            base.Start();
            yInterval = Mathf.Abs(stats.projectileRandomRangeY.Min) + Mathf.Abs(stats.projectileRandomRangeY.Max) / projectileNumber;
        }

        public override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            BeforeFireWeapon(x, y, xSpeed, ySpeed);

            SpawnProjectiles(x, y, xSpeed, ySpeed);

            AfterFireWeapon(x, y, xSpeed, ySpeed);
        }

        public virtual void SpawnProjectiles(float x, float y, float xSpeed, float ySpeed)
        {
            Projectile projectile = GetProjectile();
            for(int i = 0; i < projectileNumber; i++)
            {
                SpawnProjectile(projectile, x, y, xSpeed, ySpeed + (i * yInterval));
            }
        }

        public override void SpawnProjectile(Projectile projectile, float x, float y, float xSpeed, float ySpeed)
        {
            base.SpawnProjectile(projectile, x, y, xSpeed, ySpeed);
        }
    }
}

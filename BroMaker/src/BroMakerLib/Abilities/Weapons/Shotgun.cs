using System;
using System.Collections.Generic;
using System.Linq;
using BroMakerLib.Attributes;
using UnityEngine;

namespace BroMakerLib.Abilities.Weapons
{
    [WeaponPreset("Shotgun")]
    public class Shotgun : Weapon
    {
        public int numberOfProjectile = 3;
        public TwoFloat rangeY = new TwoFloat(40f, 40f);
        protected float yInterval = 0f;


        protected override void Start()
        {
            base.Start();
            yInterval = Mathf.Abs(rangeY.x) + Mathf.Abs(rangeY.y) / numberOfProjectile;
        }

        public override void SpawnProjectile(Projectile projectile, float x, float y, float xSpeed, float ySpeed)
        {
            for (int i = 0; i < numberOfProjectile; i++)
            {
                base.SpawnProjectile(projectile, x, y, xSpeed, ySpeed + (i * yInterval));
            }
        }
    }
}

using BroMakerLib.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Weapons
{
    public class WeaponBase : MonoBehaviour
    {
        public TestVanDammeAnim character;
        public WeaponStats stats;
        public Shrapnel bulletShell;

        protected virtual void Awake()
        {
            character = GetComponent<TestVanDammeAnim>();
        }

        protected virtual void Start() { }
        protected virtual void Update() { }

        public virtual void FireWeapon(Vector2 position, Vector2 speed)
        {
            FireWeapon(position.x, position.y, speed.x, speed.y);
        }

        protected virtual void BeforeFireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if (stats.useShrapnel)
                SpawnShrapnel(x, y, xSpeed, ySpeed);

            EffectOnFireWeapon(x, y, xSpeed, ySpeed);
        }

        protected virtual void AfterFireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            // Add force like brominator
            character.xIBlast -= character.transform.localScale.x * 4f * stats.pushBackForce.x;
            character.yIBlast -= character.transform.localScale.y * 4f * stats.pushBackForce.y;
        }

        public virtual void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            SpawnProjectile(GetProjectile(), x, y, xSpeed, ySpeed);
        }

        public virtual Projectile GetProjectile()
        {
            return character.projectile;
        }

        public virtual void SpawnShrapnel(float x, float y, float xSpeed, float ySpeed)
        {
            if (stats.useShrapnel)
                EffectsController.CreateShrapnel(bulletShell, x + character.transform.localScale.x * -5f, y, 1f, 30f, 1f, -character.transform.localScale.x * 40f, 70f);
        }

        public virtual void EffectOnFireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            EffectsController.CreateMuzzleFlashEffect(x, y, -25f, xSpeed * 0.01f, ySpeed * 0.01f, character.transform);
        }

        public virtual void SpawnProjectile(Projectile projectile, float x, float y, float xSpeed, float ySpeed)
        {
            ProjectileController.SpawnProjectileLocally(projectile, character, x, y, xSpeed, ySpeed + stats.projectileRandomRangeY.ChooseRandom, character.playerNum);
        }
    }
}

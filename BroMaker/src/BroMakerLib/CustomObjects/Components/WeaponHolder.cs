using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Stats;
using RocketLib;
using System.Collections;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Components
{
    public class WeaponHolder : MonoBehaviour
    {
        public ICustomHero customBro;
        public CustomWeaponInfo info;
        public  WeaponStats Stats
        {
            get
            {
                return null;
            }
        }
        public  BroBase Character
        {
            get
            {
                return customBro.character;
            }
        }

        protected Shrapnel bulletShell;
        public Projectile projectile;

        protected virtual void Awake()
        {
            customBro = GetComponent<ICustomHero>();
            Rambro r = HeroController.GetHeroPrefab(HeroType.Rambro) as Rambro;
            bulletShell = r.bulletShell;
        }

        public virtual void SetStats()
        {
            if(Stats.shrapnelTexture != null)
                bulletShell.GetFieldValue<SpriteSM>("sprite").SetTexture(Stats.shrapnelTexture);
        }

        protected virtual void GetProjectile()
        {

        }

        public void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if(Stats.useShrapnel)
                EffectsController.CreateShrapnel(bulletShell, x + Character.transform.localScale.x * -5f, y, 1f, 30f, 1f, -Character.transform.localScale.x * 40f, 70f);
            EffectsController.CreateMuzzleFlashEffect(x, y, -25f, xSpeed * 0.01f, ySpeed * 0.01f, Character.transform);
            switch (Stats.type)
            {
                case WeaponType.Shotgun:
                    Shotgun(x, y, xSpeed, ySpeed);
                    break;
                default:
                    ProjectileController.SpawnProjectileLocally(projectile, this, x, y, xSpeed, ySpeed + Stats.projectileRandomRangeY.ChooseRandom, Character.playerNum);
                    break;
            }

            // Add force like brominator
            Character.xIBlast -= Character.transform.localScale.x * 4f * Stats.pushBackForce.x;
            Character.yIBlast -= Character.transform.localScale.y * 4f * Stats.pushBackForce.y;
        }

        public void Shotgun(float x, float y, float xSpeed, float ySpeed)
        {
            ProjectileController.SpawnProjectileLocally(this.projectile, this, x, y, xSpeed * 0.94f, ySpeed + 50f + Stats.projectileRandomRangeY.ChooseRandom * 5f, Character.playerNum);
            ProjectileController.SpawnProjectileLocally(this.projectile, this, x, y, xSpeed * 0.96f, ySpeed + 25f + Stats.projectileRandomRangeY.ChooseRandom * 10f, Character.playerNum);
            ProjectileController.SpawnProjectileLocally(this.projectile, this, x, y, xSpeed * 0.98f, ySpeed - 3f + Stats.projectileRandomRangeY.ChooseRandom * 6f, Character.playerNum);
            ProjectileController.SpawnProjectileLocally(this.projectile, this, x, y, xSpeed * 0.96f, ySpeed - 25f - Stats.projectileRandomRangeY.ChooseRandom * 10f, Character.playerNum);
            ProjectileController.SpawnProjectileLocally(this.projectile, this, x, y, xSpeed * 0.94f, ySpeed - 50f + Stats.projectileRandomRangeY.ChooseRandom * 5f, Character.playerNum);
        }
    }
}
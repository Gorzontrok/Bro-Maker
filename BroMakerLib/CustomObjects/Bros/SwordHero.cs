using System.Collections.Generic;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Bros
{
    [HeroPreset("SwordHero", HeroType.Blade)]
    public class SwordHero : CustomHero
    {
        public int enemySwordDamage = 5;
        public int groundSwordDamage = 3;

        protected float deflectProjectilesEnergy;
        protected float deflectProjectilesCounter;
        protected List<Unit> alreadyHit = new List<Unit>();
        protected bool hasHitWithSlice;
        protected bool hasHitWithWall;
        protected FlickerFader hitPuff;
        protected Shrapnel shrapnelSpark;
        protected bool hasPlayedAttackHitSound;

        protected override void Awake()
        {
            hitPuff = (HeroController.GetHeroPrefab(HeroType.Blade) as Blade).hitPuff;
            shrapnelSpark = (HeroController.GetHeroPrefab(HeroType.Blade) as Blade).shrapnelSpark;
            base.Awake();
        }

        protected virtual void DealDamageToGround(float x, float y, Vector3 point, Collider hitCollider)
        {
            MakeEffects(point.x, point.y);
            bool flag = hitCollider.GetComponent<Cage>();
            int num = groundSwordDamage;
            MapController.Damage_Local(this, hitCollider.gameObject, num + (!flag ? 0 : 5), DamageType.Bullet, xI, 0f, x, y);
            if (!hasHitWithWall)
            {
                SortOfFollow.Shake(0.15f);
            }
            hasHitWithWall = true;
        }

        protected virtual void DeflectProjectiles()
        {
            if (Map.DeflectProjectiles(this, playerNum, 16f, X + (Mathf.Sign(transform.localScale.x) * 6f), Y + 6f, Mathf.Sign(transform.localScale.x) * 200f, true))
            {
                PlaySpecial4Sound(1f);
                hasHitWithWall = true;
            }
        }

        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            Map.HurtWildLife(x + (transform.localScale.x * 13f), y + 5f, 12f);
            deflectProjectilesCounter = deflectProjectilesEnergy;
            deflectProjectilesEnergy = 0f;
            DeflectProjectiles();
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, x + (float)(Direction * 4), y, 0f, 0f, 6f, playerNum, out flag, null);
            SetGunSprite(gunFrame, 0);
            float num = transform.localScale.x * 12f;
            ConstrainToFragileBarriers(ref num, 16f);
            if (!TryHitWalls(x, y, xSpeed, ySpeed, true))
            {
                hasHitWithWall = false;
                SwingSword(false);
            }
        }

        protected virtual Vector3 GetSwingAtGroundPosition(float x, float y)
        {
            return new Vector3(x - (Mathf.Sign(transform.localScale.x) * 12f), y + 5.5f, 0f);
        }

        protected virtual void MakeEffects(float x, float y)
        {
            EffectsController.CreateShrapnel(shrapnelSpark, raycastHit.point.x + (raycastHit.normal.x * 3f), raycastHit.point.y + (raycastHit.normal.y * 3f), 4f, 30f, 3f, raycastHit.normal.x * 60f, raycastHit.normal.y * 30f);
            EffectsController.CreateEffect(hitPuff, raycastHit.point.x + (raycastHit.normal.x * 3f), raycastHit.point.y + (raycastHit.normal.y * 3f));
        }

        protected override void RunGun()
        {
            deflectProjectilesEnergy += t * 0.5f;
            if (deflectProjectilesEnergy > 0.45f)
            {
                deflectProjectilesEnergy = 0.45f;
            }
            deflectProjectilesCounter -= t;
            if (!WallDrag && gunFrame > 0)
            {
                if (deflectProjectilesCounter > 0f)
                {
                    DeflectProjectiles();
                }
                gunCounter += t;
                if (gunCounter > 0.0334f)
                {
                    gunCounter -= 0.0334f;
                    gunFrame--;
                    if (gunFrame < 0)
                    {
                        gunFrame = 0;
                    }
                    SetGunSprite(gunFrame, 0);
                    /*  if (!this.hasPlayedAttackHitSound)
                      {
                          if (this.hasHitWithSlice)
                          {
                              this.PlaySliceSound();
                              this.hasPlayedAttackHitSound = true;
                          }
                          else if (this.hasHitWithWall)
                          {
                              this.PlayWallSound();
                              this.hasPlayedAttackHitSound = true;
                          }
                      }*/
                    if (gunFrame >= 3)
                    {
                        SwingSword(hasHitWithWall);
                        if (!hasHitWithWall)
                        {
                            TryHitWalls(X + (10f * transform.localScale.x), Y + 6.5f, xI, 0f, false);
                        }
                    }
                }
            }
        }

        protected virtual void SwingSword(bool ground)
        {
            if (Map.HitUnits(this, playerNum, enemySwordDamage, DamageType.Blade, ground ? 5f : 15f, X, Y, transform.localScale.x * 420f, 360f, true, true, true, alreadyHit, false, false))
            {
                if (!hasHitWithSlice)
                {
                    SortOfFollow.Shake(0.15f);
                }
                hasHitWithSlice = true;
                if (ground)
                {
                    PlayWallSound();
                }
                else
                {
                    PlaySliceSound();
                }
            }
            else
            {
                hasHitWithSlice = false;
            }
        }

        protected virtual bool TryHitWalls(float x, float y, float xSpeed, float ySpeed, bool alsoHitUnits = true)
        {
            if (Physics.Raycast(GetSwingAtGroundPosition(x, y), new Vector3(transform.localScale.x, 0f, 0f), out raycastHit, 19f, groundLayer) || Physics.Raycast(new Vector3(x - (Mathf.Sign(transform.localScale.x) * 12f), y + 10.5f, 0f), new Vector3(transform.localScale.x, 0f, 0f), out raycastHit, 19f, groundLayer))
            {
                DealDamageToGround(x, y, raycastHit.point, raycastHit.collider);
                if (alsoHitUnits)
                {
                    SwingSword(true);
                }
                return true;
            }
            Collider[] array = Physics.OverlapSphere(GetSwingAtGroundPosition(x, y), 1f, groundLayer | (1 << LayerMask.NameToLayer("FLUI")));
            if (array.Length > 0)
            {
                DealDamageToGround(x, y, GetSwingAtGroundPosition(x, y), array[0]);
                if (alsoHitUnits)
                {
                    SwingSword(true);
                }
                return true;
            }
            return false;
        }

        protected override void UseFire()
        {
            alreadyHit.Clear();
            hasHitWithWall = false;
            hasHitWithSlice = false;
            hasPlayedAttackHitSound = false;
            gunFrame = 6;
            FireWeapon(X + (transform.localScale.x * 10f), Y + 6.5f, transform.localScale.x * 400f, (float)(Random.Range(0, 40) - 20));
            PlayAttackSound();
            Map.DisturbWildLife(X, Y, 60f, playerNum);
        }

        public virtual void PlaySliceSound()
        {
            PlaySpecial2Sound(0.7f);
        }

        public virtual void PlayWallSound()
        {

            if (sound == null)
            {
                sound = Sound.GetInstance();
            }
            if (sound != null)
            {
                sound.PlaySoundEffectAt(soundHolder.defendSounds, 0.7f, transform.position, 1f, true, false, false, 0f);
            }
        }
    }
}

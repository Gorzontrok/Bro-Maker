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
            this.MakeEffects(point.x, point.y);
            bool flag = hitCollider.GetComponent<Cage>();
            int num = this.groundSwordDamage;
            MapController.Damage_Local(this, hitCollider.gameObject, num + ((!flag) ? 0 : 5), DamageType.Bullet, this.xI, 0f, x, y);
            if (!this.hasHitWithWall)
            {
                SortOfFollow.Shake(0.15f);
            }
            this.hasHitWithWall = true;
        }
        protected virtual void DeflectProjectiles()
        {
            if (Map.DeflectProjectiles(this, base.playerNum, 16f, base.X + Mathf.Sign(base.transform.localScale.x) * 6f, base.Y + 6f, Mathf.Sign(base.transform.localScale.x) * 200f, true))
            {
                PlaySpecial4Sound(1f);
                this.hasHitWithWall = true;
            }
        }
        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            Map.HurtWildLife(x + base.transform.localScale.x * 13f, y + 5f, 12f);
            this.deflectProjectilesCounter = this.deflectProjectilesEnergy;
            this.deflectProjectilesEnergy = 0f;
            this.DeflectProjectiles();
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, x + (float)(base.Direction * 4), y, 0f, 0f, 6f, base.playerNum, out flag, null);
            this.SetGunSprite(this.gunFrame, 0);
            float num = base.transform.localScale.x * 12f;
            this.ConstrainToFragileBarriers(ref num, 16f);
            if (!this.TryHitWalls(x, y, xSpeed, ySpeed, true))
            {
                this.hasHitWithWall = false;
                this.SwingSword(false);
            }
        }
        protected virtual Vector3 GetSwingAtGroundPosition(float x, float y)
        {
            return new Vector3(x - Mathf.Sign(base.transform.localScale.x) * 12f, y + 5.5f, 0f);
        }
        protected virtual void MakeEffects(float x, float y)
        {
            EffectsController.CreateShrapnel(this.shrapnelSpark, this.raycastHit.point.x + this.raycastHit.normal.x * 3f, this.raycastHit.point.y + this.raycastHit.normal.y * 3f, 4f, 30f, 3f, this.raycastHit.normal.x * 60f, this.raycastHit.normal.y * 30f);
            EffectsController.CreateEffect(this.hitPuff, this.raycastHit.point.x + this.raycastHit.normal.x * 3f, this.raycastHit.point.y + this.raycastHit.normal.y * 3f);
        }
        protected override void RunGun()
        {
            this.deflectProjectilesEnergy += this.t * 0.5f;
            if (this.deflectProjectilesEnergy > 0.45f)
            {
                this.deflectProjectilesEnergy = 0.45f;
            }
            this.deflectProjectilesCounter -= this.t;
            if (!this.WallDrag && this.gunFrame > 0)
            {
                if (this.deflectProjectilesCounter > 0f)
                {
                    this.DeflectProjectiles();
                }
                this.gunCounter += this.t;
                if (this.gunCounter > 0.0334f)
                {
                    this.gunCounter -= 0.0334f;
                    this.gunFrame--;
                    if (this.gunFrame < 0)
                    {
                        this.gunFrame = 0;
                    }
                    this.SetGunSprite(this.gunFrame, 0);
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
                    if (this.gunFrame >= 3)
                    {
                        this.SwingSword(this.hasHitWithWall);
                        if (!this.hasHitWithWall)
                        {
                            this.TryHitWalls(base.X + 10f * base.transform.localScale.x, base.Y + 6.5f, this.xI, 0f, false);
                        }
                    }
                }
            }
        }
        protected virtual void SwingSword(bool ground)
        {
            if (Map.HitUnits(this, base.playerNum, this.enemySwordDamage, DamageType.Blade, ground ? 5f : 15f, base.X, base.Y, base.transform.localScale.x * 420f, 360f, true, true, true, this.alreadyHit, false, false))
            {
                if (!this.hasHitWithSlice)
                {
                    SortOfFollow.Shake(0.15f);
                }
                this.hasHitWithSlice = true;
                if (ground)
                    this.PlayWallSound();
                else
                    this.PlaySliceSound();
            }
            else
            {
                this.hasHitWithSlice = false;
            }
        }
        protected virtual bool TryHitWalls(float x, float y, float xSpeed, float ySpeed, bool alsoHitUnits = true)
        {
            if (Physics.Raycast(this.GetSwingAtGroundPosition(x, y), new Vector3(base.transform.localScale.x, 0f, 0f), out this.raycastHit, 19f, this.groundLayer) || Physics.Raycast(new Vector3(x - Mathf.Sign(base.transform.localScale.x) * 12f, y + 10.5f, 0f), new Vector3(base.transform.localScale.x, 0f, 0f), out this.raycastHit, 19f, this.groundLayer))
            {
                this.DealDamageToGround(x, y, this.raycastHit.point, this.raycastHit.collider);
                if (alsoHitUnits)
                {
                    this.SwingSword(true);
                }
                return true;
            }
            Collider[] array = Physics.OverlapSphere(this.GetSwingAtGroundPosition(x, y), 1f, this.groundLayer | 1 << LayerMask.NameToLayer("FLUI"));
            if (array.Length > 0)
            {
                this.DealDamageToGround(x, y, this.GetSwingAtGroundPosition(x, y), array[0]);
                if (alsoHitUnits)
                {
                    this.SwingSword(true);
                }
                return true;
            }
            return false;
        }
        protected override void UseFire()
        {
            this.alreadyHit.Clear();
            this.hasHitWithWall = false;
            this.hasHitWithSlice = false;
            this.hasPlayedAttackHitSound = false;
            this.gunFrame = 6;
            this.FireWeapon(base.X + base.transform.localScale.x * 10f, base.Y + 6.5f, base.transform.localScale.x * 400f, (float)(UnityEngine.Random.Range(0, 40) - 20));
            this.PlayAttackSound();
            Map.DisturbWildLife(base.X, base.Y, 60f, base.playerNum);
        }

        public virtual void PlaySliceSound()
        {
            PlaySpecial2Sound(0.7f);
        }

        public virtual void PlayWallSound()
        {

            if (this.sound == null)
            {
                this.sound = Sound.GetInstance();
            }
            if (this.sound != null)
            {
                this.sound.PlaySoundEffectAt(this.soundHolder.defendSounds, 0.7f, base.transform.position, 1f, true, false, false, 0f);
            }
        }
    }
}

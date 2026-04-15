using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Bro Ceasar's shockwave charge.</summary>
    [SpecialPreset("BroCeasar")]
    public class BroCeasarSpecial : ShockwaveSpecial
    {
        /// <summary>Projectile fired backward during the shockwave dash (provides the reverse recoil thrust).</summary>
        public Projectile shockwaveProjectile;
        /// <summary>Shell casing shrapnel spawned at each shockwave bullet.</summary>
        public Shrapnel shockwaveBulletShell;
        /// <summary>Sounds played as each shockwave bullet fires.</summary>
        public AudioClip[] shockwaveAttackSounds;
        /// <summary>Seconds between consecutive shockwave bullets.</summary>
        public float shockwaveFireRate = 0.033f;

        protected override HeroType SourceBroType => HeroType.HaleTheBro;

        [JsonIgnore] private float specialShootBoostTime;
        [JsonIgnore] private float shockwaveFireCounter;
        [JsonIgnore] private float pushBackForceM = 1f;

        public BroCeasarSpecial()
        {
            lethal = false;
            animationRow = 8;
        }

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as BroCeasar;
            if (sourceBro == null) return;
            if (shockwaveProjectile == null) shockwaveProjectile = sourceBro.projectile;
            if (shockwaveBulletShell == null) shockwaveBulletShell = sourceBro.bulletShell;
            if (shockwaveAttackSounds == null) shockwaveAttackSounds = sourceBro.soundHolder.attackSounds.CloneArray();
        }

        public override void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.UsingSpecial || owner.health <= 0)
            {
                return;
            }
            if (owner.SpecialAmmo > 0)
            {
                MapController.DamageGround(owner, groundDamage, DamageType.Normal, groundDamageRange, X, Y, null, false);
                EffectsController.CreateGroundWave(X, Y, 2f);
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                if (!hero.UsingSpecial)
                {
                    specialAttackDirection = owner.transform.localScale.x;
                }
                hero.UsingSpecial = true;
                owner.frame = 0;
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                if (owner.actionState == ActionState.ClimbingLadder)
                {
                    owner.actionState = ActionState.Idle;
                }
                specialShootBoostTime = 0.4f;
                shockwaveFireCounter = 0f;
                pushBackForceM = 1f;
                owner.xI = owner.transform.localScale.x * owner.speed;
                owner.dashing = true;
                owner.yI = 230f;
                owner.xIBlast = owner.transform.localScale.x * 140f;
                readyForBlast = false;
                setupBlastReadiness = false;
                hero.ChangeFrame();
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override bool HandleRunGun()
        {
            if (hero.UsingSpecial && specialShootBoostTime > 0f)
            {
                specialShootBoostTime -= hero.DeltaTime;
                shockwaveFireCounter += hero.DeltaTime;
                while (shockwaveFireCounter >= shockwaveFireRate)
                {
                    shockwaveFireCounter -= shockwaveFireRate;
                    FireShockwaveBullet();
                    pushBackForceM = Mathf.Clamp(pushBackForceM + hero.DeltaTime * 6f, 1f, 12f);
                }
                owner.fire = false;
                owner.wasFire = false;
                owner.fireDelay = 0f;
                return false;
            }
            if (hero.UsingSpecial)
            {
                owner.fire = false;
                owner.wasFire = false;
                pushBackForceM = 1f;
                return false;
            }
            return true;
        }

        private void FireShockwaveBullet()
        {
            if (shockwaveProjectile == null) return;

            float x = X - owner.transform.localScale.x * 16f;
            float y = Y + 10f;
            float xSpeed = -(owner.transform.localScale.x * 400f) * 0.75f;
            float ySpeed = Mathf.Abs(xSpeed) * -0.75f;

            owner.xIBlast += owner.transform.localScale.x * 1f * pushBackForceM;
            owner.yI += 3f * pushBackForceM;

            hero.GunFrame = 3;
            if (shockwaveBulletShell != null)
                EffectsController.CreateShrapnel(shockwaveBulletShell, x + owner.transform.localScale.x * -5f, y, 1f, 30f, 1f, -owner.transform.localScale.x * 40f, 70f);
            EffectsController.CreateMuzzleFlashEffect(x, y, -25f, xSpeed * 0.01f, ySpeed * 0.01f, owner.transform);
            ProjectileController.SpawnProjectileLocally(shockwaveProjectile, owner, x, y, xSpeed, ySpeed, PlayerNum);

            if (y > owner.groundHeight)
            {
                owner.yI += Mathf.Clamp(3f * pushBackForceM, 3f, 16f);
            }

            if (shockwaveAttackSounds != null && shockwaveAttackSounds.Length > 0)
                sound.PlaySoundEffectAt(shockwaveAttackSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
            Map.DisturbWildLife(X, Y, 60f, PlayerNum);
        }

        public override void HandleAfterCheckInput()
        {
            base.HandleAfterCheckInput();
            if (hero.UsingSpecial && owner.transform.localScale.x > 0f)
            {
                owner.left = false;
                owner.right = true;
            }
            else if (hero.UsingSpecial && owner.transform.localScale.x < 0f)
            {
                owner.right = false;
                owner.left = true;
            }
        }

        public override void HandleAfterAddSpeedLeft()
        {
            if (owner.xIBlast > owner.speed * 1.6f)
            {
                owner.xIBlast = owner.speed * 1.6f;
            }
        }

        public override void HandleAfterAddSpeedRight()
        {
            if (owner.xIBlast < owner.speed * -1.6f)
            {
                owner.xIBlast = owner.speed * -1.6f;
            }
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = frameRate;
            int column = Mathf.Clamp(owner.frame, 0, 8);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
            if (owner.frame == 0)
            {
                hero.FrameRate = 0.18f;
            }
            else
            {
                hero.FrameRate = 0.034f;
                if (owner.frame == 3)
                {
                    hero.FrameRate = 0.1f;
                }
                hero.CreateFaderTrailInstance();
            }
            if (owner.frame == useSpecialAttackFrame)
            {
                if (!setupBlastReadiness)
                {
                    readyForBlast = true;
                    setupBlastReadiness = true;
                }
                if (readyForBlast)
                {
                    owner.frame -= 2;
                }
                else
                {
                    owner.counter = -0.06f;
                }
            }
            if (owner.frame == 8)
            {
                owner.counter -= 0.15f;
            }
            if (owner.frame >= 10)
            {
                hero.GunFrame = 0;
                owner.frame = 0;
                hero.ActivateGun();
                hero.UsingSpecial = false;
                hero.ChangeFrame();
                currentStampDelay = 0f;
            }
        }

    }
}

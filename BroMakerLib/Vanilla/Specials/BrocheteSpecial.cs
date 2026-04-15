using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Brochete's machete spray.</summary>
    [SpecialPreset("Brochete")]
    public class BrocheteSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.Brochete;
        /// <summary>Name of the projectile prefab to load for each thrown machete.</summary>
        public string projectileName = "MacheteSpray";
        public float knifeSpraySpeed = 400f;
        public int knifeSprayCount = 6;
        /// <summary>Volume of the throw sound played for each fired machete.</summary>
        public float throwSoundVolume = 0.44f;
        /// <summary>Angular offset applied to the base spray direction.</summary>
        public float sprayAngleOffset = -13.25f;
        /// <summary>Angle step applied per group-of-three shots within the spray.</summary>
        public float sprayAngleStep1 = 7.5f;
        /// <summary>Angle step applied per group-of-six shots within the spray.</summary>
        public float sprayAngleStep2 = 2.5f;
        /// <summary>Lerp speed used to smooth the spray angle toward the target direction each shot.</summary>
        public float angleLerpSpeed = 0.33f;

        [JsonIgnore]
        protected Projectile projectile;
        [JsonIgnore]
        private int knifeSprayCountLeft;
        [JsonIgnore]
        private Vector3 knifeSprayDirection;
        [JsonIgnore]
        private float knifeSprayAngle;
        [JsonIgnore]
        private float lastKnifeSprayAngle;
        [JsonIgnore]
        private int fireCount;
        [JsonIgnore]
        private bool spraying;

        public int upDiagonalColumn = 4;
        public int upColumn = 8;
        public int downDiagonalColumn = 12;
        public int downColumn = 16;

        public BrocheteSpecial()
        {
            blockMovement = false;
            animationRow = 8;
            animationColumn = 0;
        }

        public override void Initialize(TestVanDammeAnim owner)
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
            base.PressSpecial();
            if (owner.SpecialAmmo > 0)
            {
                knifeSprayCountLeft = knifeSprayCount;
                knifeSprayDirection = GetKnifeSprayDirection();
                lastKnifeSprayAngle = (knifeSprayAngle = GetKnifeSprayAngle(true));
                fireCount = -1;
                spraying = true;
            }
            else
            {
                knifeSprayCountLeft = 0;
            }
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = frameRate;

            Vector3 dir = global::Math.Point3OnCircle((720f + lastKnifeSprayAngle) / 180f * 3.1415927f, 1f);

            if (Mathf.Abs(dir.x) * 0.25f > Mathf.Abs(dir.y))
            {
                AnimateSprayFrame();
                int col = animationColumn + Mathf.Clamp(owner.frame, 0, 3);
                hero.Sprite.SetLowerLeftPixel(col * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
            }
            else if (Mathf.Abs(dir.x) * 1.3f > Mathf.Abs(dir.y))
            {
                AnimateSprayFrame();
                if (dir.y > 0f)
                {
                    int col = upDiagonalColumn + Mathf.Clamp(owner.frame, 0, 3);
                    hero.Sprite.SetLowerLeftPixel(col * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
                }
                else
                {
                    int col = downDiagonalColumn + Mathf.Clamp(owner.frame, 0, 3);
                    hero.Sprite.SetLowerLeftPixel(col * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
                }
            }
            else
            {
                AnimateSprayFrame();
                if (dir.y > 0f)
                {
                    int col = upColumn + Mathf.Clamp(owner.frame, 0, 3);
                    hero.Sprite.SetLowerLeftPixel(col * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
                }
                else
                {
                    int col = downColumn + Mathf.Clamp(owner.frame, 0, 3);
                    hero.Sprite.SetLowerLeftPixel(col * hero.SpritePixelWidth, hero.SpritePixelHeight * animationRow);
                }
            }
        }

        private void AnimateSprayFrame()
        {
            if (owner.frame == 1 && owner.SpecialAmmo > 0)
            {
                UseSpecial();
            }
            if (owner.frame == 3 && owner.SpecialAmmo > 0)
            {
                UseSpecial();
            }
            if (owner.frame >= 4 && knifeSprayCountLeft > 0)
            {
                owner.frame = 0;
                knifeSprayCountLeft--;
                hero.FrameRate = 0.02f;
            }
            if (owner.frame >= 5)
            {
                EndSpray();
            }
        }

        private void EndSpray()
        {
            if (spraying)
            {
                owner.SpecialAmmo--;
                HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                spraying = false;
            }
            owner.Stop();
            owner.frame = 0;
            hero.ActivateGun();
            hero.UsingSpecial = false;
            if (owner.Y < owner.groundHeight + 1f)
            {
                owner.actionState = ActionState.Idle;
            }
            else
            {
                owner.actionState = ActionState.Jumping;
            }
            hero.ChangeFrame();
        }

        public override void UseSpecial()
        {
            fireCount++;
            float targetAngle = GetKnifeSprayAngle(false);
            knifeSprayAngle = Mathf.LerpAngle(knifeSprayAngle, targetAngle, angleLerpSpeed);
            knifeSprayDirection = global::Math.Point3OnCircle(
                (720f + knifeSprayAngle + sprayAngleOffset + sprayAngleStep1 * (fireCount % 3) + sprayAngleStep2 * (fireCount % 6)) / 180f * 3.1415927f,
                1f);

            Sound.GetInstance().PlaySoundEffectAt(attackSounds, throwSoundVolume, owner.transform.position);
            ProjectileController.SpawnProjectileLocally(projectile, owner,
                X + knifeSprayDirection.x * 10f, Y + knifeSprayDirection.y * 8f + 8f,
                knifeSprayDirection.x * knifeSpraySpeed, knifeSprayDirection.y * knifeSpraySpeed,
                PlayerNum);

            lastKnifeSprayAngle = knifeSprayAngle;
            hero.PressSpecialFacingDirection = 0;
        }

        public override bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            if (hero.UsingSpecial && owner.SpecialAmmo > 0)
            {
                xI = 0f; yI = 0f;
                return false;
            }
            return true;
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            owner.Stop();
            return true;
        }

        public override bool HandleDeath()
        {
            if (spraying)
            {
                EndSpray();
            }
            return true;
        }

        private Vector3 GetKnifeSprayDirection()
        {
            Vector3 dir = Vector3.zero;
            if (owner.right) dir += Vector3.right;
            if (owner.left) dir += Vector3.left;
            if (owner.up) dir += Vector3.up;
            if (owner.down) dir += Vector3.down;
            if (dir.x == 0f && dir.y == 0f)
            {
                dir += new Vector3(owner.transform.localScale.x, 0f, 0f);
            }
            return dir;
        }

        private float GetKnifeSprayAngle(bool useFacing)
        {
            float angle;
            if (owner.right)
            {
                if (owner.up) angle = 45f;
                else if (owner.down) angle = -45f;
                else angle = 0f;
            }
            else if (owner.left)
            {
                if (owner.up) angle = 135f;
                else if (owner.down) angle = -135f;
                else angle = -180f;
            }
            else if (owner.up)
            {
                angle = 90f;
            }
            else if (owner.down)
            {
                angle = -90f;
            }
            else
            {
                if (!useFacing) return knifeSprayAngle;
                angle = owner.transform.localScale.x > 0f ? 0f : -180f;
            }
            return angle;
        }
    }
}

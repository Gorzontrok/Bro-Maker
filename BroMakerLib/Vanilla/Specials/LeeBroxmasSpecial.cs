using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Lee Broxmas's knife-spray special.</summary>
    [SpecialPreset("LeeBroxmas")]
    public class LeeBroxmasSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.LeeBroxmas;
        /// <summary>Name of the projectile prefab used for each thrown knife.</summary>
        public string projectileName = "KnifeSpray";
        public float knifeSpraySpeed = 420f;
        public int knifeSprayCount = 4;
        /// <summary>Playback volume for the throw sound.</summary>
        public float throwSoundVolume = 0.44f;
        /// <summary>Base angle offset applied to the spray direction, in degrees.</summary>
        public float sprayAngleOffset = -18.25f;
        /// <summary>Per-knife angle step applied based on fire index mod 3, in degrees.</summary>
        public float sprayAngleStep1 = 9.5f;
        /// <summary>Per-knife angle step applied based on fire index mod 6, in degrees.</summary>
        public float sprayAngleStep2 = 4.5f;
        /// <summary>Minimum random multiplier for each knife's horizontal speed.</summary>
        public float xSpeedVarianceMin = 0.95f;
        /// <summary>Maximum random multiplier for each knife's horizontal speed.</summary>
        public float xSpeedVarianceMax = 1.125f;
        /// <summary>Minimum random multiplier for each knife's vertical speed.</summary>
        public float ySpeedVarianceMin = 0.9f;
        /// <summary>Maximum random multiplier for each knife's vertical speed.</summary>
        public float ySpeedVarianceMax = 1.125f;

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

        public LeeBroxmasSpecial()
        {
            blockMovement = false;
            animationRow = 8;
            animationColumn = 0;
        }

        public override void Initialize(BroBase owner)
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
                lastKnifeSprayAngle = (knifeSprayAngle = GetKnifeSprayAngle());
                fireCount = -1;
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
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
            if (owner.frame == 2 && owner.SpecialAmmo > 0)
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
            knifeSprayAngle = GetKnifeSprayAngle();
            knifeSprayDirection = global::Math.Point3OnCircle(
                (720f + knifeSprayAngle + sprayAngleOffset + sprayAngleStep1 * (fireCount % 3) + sprayAngleStep2 * (fireCount % 6)) / 180f * 3.1415927f,
                1f);

            Sound.GetInstance().PlaySoundEffectAt(attackSounds, throwSoundVolume, owner.transform.position);
            ProjectileController.SpawnProjectileLocally(projectile, owner,
                X + knifeSprayDirection.x * 10f, Y + knifeSprayDirection.y * 8f + 8f,
                Mathf.Sign(knifeSprayDirection.x) * knifeSpraySpeed * Random.Range(xSpeedVarianceMin, xSpeedVarianceMax),
                knifeSprayDirection.y * knifeSpraySpeed * Random.Range(ySpeedVarianceMin, ySpeedVarianceMax),
                PlayerNum);

            lastKnifeSprayAngle = knifeSprayAngle;
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

        public override bool HandleDeath(float xI, float yI, DamageObject damage)
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

        private float GetKnifeSprayAngle()
        {
            if (owner.transform.localScale.x > 0f)
            {
                return 45f;
            }
            return 135f;
        }
    }
}

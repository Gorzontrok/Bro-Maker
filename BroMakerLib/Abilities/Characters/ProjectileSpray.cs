using BroMakerLib.Attributes;
using UnityEngine;

namespace BroMakerLib.Abilities.Characters
{
    [AbilityPreset(nameof(ProjectileSpray))]
    public class ProjectileSpray : CharacterAbility
    {
        public Projectile projectile;
        public int numberOfProjectile = 10;
        public bool blockMovement = false;
        public float projectileSpeed = 320f;
        public bool useDirection = false;
        public float delayBetweenProjectile = 0.05f;

        protected int _remainingProjectile = 0;
        protected float _sprayAngle = 0f;
        protected Vector3 _sprayDirection = Vector3.zero;
        protected int _fireCount = 0;
        protected float _fireDelay = 0f;

        public override void All(string calledFromMethod, params object[] objects)
        {
            StartSpray();
        }

        public void StartSpray()
        {
            if (_remainingProjectile > 0)
                return;

            _remainingProjectile = numberOfProjectile;
        }

        public void SprayProjectile()
        {
            if (blockMovement)
            {
                owner.xI = 0f;
                owner.yI = 0f;
            }
            ShootProjectile();
            _remainingProjectile--;
            if (_remainingProjectile == 0)
            {
                StopSpray();
            }
        }

        protected override void Update()
        {
            base.Update();
            if (_remainingProjectile > 0 && (_fireDelay -= DT) <= 0)
            {
                SprayProjectile();
            }
        }

        public void ShootProjectile()
        {
            _fireDelay = delayBetweenProjectile;
            _fireCount++;
            float angle = GetSprayAngle();
            _sprayAngle = Mathf.LerpAngle(_sprayAngle, angle, 0.33f);
            _sprayDirection = Math.Point3OnCircle((720f + _sprayAngle - 13.25f + 7.5f * (float)(_fireCount % 3) + 2.5f * (float)(_fireCount % 6)) / 180f * Mathf.PI, 1f);

            float x = owner.X + _sprayDirection.x * 10f;
            float y = owner.Y + _sprayDirection.y * 8f + 8f;
            float xSpeed = _sprayDirection.x * projectileSpeed;
            float ySpeed = _sprayDirection.y * projectileSpeed;
            ProjectileController.SpawnProjectileLocally(projectile, owner, x, y, xSpeed, ySpeed, PlayerNum);
        }

        public void StopSpray()
        {

        }

        protected float GetSprayAngle()
        {
            if (!useDirection)
            {
                if (owner.transform.localScale.x > 0f)
                {
                    return 0f;
                }
                else
                {
                    return -180f;
                }
            }

            // Brochete.GetKnifeSprayAngle
            float angle = 0f;
            if (owner.right)
            {
                if (owner.up)
                {
                    angle = 45f;
                }
                else if (owner.down)
                {
                    angle = -45f;
                }
                else
                {
                    angle = 0f;
                }
            }
            else if (owner.left)
            {
                if (owner.up)
                {
                    angle = 135f;
                }
                else if (owner.down)
                {
                    angle = -135f;
                }
                else
                {
                    angle = -180f;
                }
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
                if (owner.transform.localScale.x > 0f)
                {
                    angle = 0f;
                }
                else
                {
                    angle = -180f;
                }
            }
            return angle;
        }
    }
}

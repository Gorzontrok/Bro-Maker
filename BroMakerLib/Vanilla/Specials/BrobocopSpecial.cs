using System.Collections;
using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Brobocop's targeting-system missile barrage.</summary>
    [SpecialPreset("Brobocop")]
    public class BrobocopSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.Brobocop;
        public BrobocopSpecial()
        {
            instantUse = true;
        }

        /// <summary>How long the targeting cursor stays active before auto-firing.</summary>
        public float targetingDuration = 5f;
        /// <summary>Radius used when scanning for the next target to lock onto.</summary>
        public float scanningRange = 10f;
        /// <summary>Minimum time between successive special uses.</summary>
        public float specialCooldownDelay = 0.13f;
        /// <summary>Launch speed of each guided missile.</summary>
        public float missileSpeed = 400f;
        /// <summary>Delay in seconds between firing successive missiles at locked targets.</summary>
        public float missileFireInterval = 0.12f;
        /// <summary>Gun sprite column shown while firing missiles.</summary>
        public int targetingGunColumn = 3;

        [JsonIgnore]
        private bool usingTargetingSystem;
        [JsonIgnore]
        private bool firingSpecial;
        [JsonIgnore]
        private float targetingTime;
        [JsonIgnore]
        private float specialCooldownTimer;
        [JsonIgnore]
        private float currentTargetingStreakTime;
        [JsonIgnore]
        private RobrocopTargetingSystem targetSystem;
        [JsonIgnore]
        private RobrocopTargetingSystem targetSystemPrefab;
        [JsonIgnore]
        private Projectile remoteProjectilePrefab;
        [JsonIgnore]
        private List<BroforceObject> targettedUnits = new List<BroforceObject>();
        [JsonIgnore]
        private List<AnimatedIcon> targettedTargets = new List<AnimatedIcon>();
        [JsonIgnore]
        private List<Unit> currentTargetingUnitStreak = new List<Unit>();
        [JsonIgnore]
        private List<TargetableObject> currentTargetingObjectStreak = new List<TargetableObject>();

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var brobocop = owner as Brobocop;
            if (brobocop == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.Brobocop);
                brobocop = prefab as Brobocop;
            }
            if (brobocop != null)
            {
                targetSystemPrefab = brobocop.targetSystemPrefab;
            }
            remoteProjectilePrefab = owner.remoteProjectile;
            if (remoteProjectilePrefab == null && brobocop != null)
            {
                remoteProjectilePrefab = brobocop.remoteProjectile;
            }
        }

        public override void UseSpecial()
        {
            if (specialCooldownTimer < specialCooldownDelay)
            {
                return;
            }
            specialCooldownTimer = 0f;
            if (owner.IsMine)
            {
                if (owner.SpecialAmmo > 0 && !usingTargetingSystem && !firingSpecial)
                {
                    owner.SpecialAmmo--;
                    HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                    usingTargetingSystem = true;
                    targetingTime = targetingDuration;
                    DirectionEnum direction = owner.transform.localScale.x <= 0f ? DirectionEnum.Left : DirectionEnum.Right;
                    if (targetSystemPrefab != null)
                    {
                        targetSystem = Networking.Networking.Instantiate(targetSystemPrefab,
                            owner.transform.position + Vector3.up * 6.5f, Quaternion.identity, false);
                        targetSystem.SetupTargetting(owner, direction);
                    }
                    targettedUnits = new List<BroforceObject>();
                    currentTargetingObjectStreak = new List<TargetableObject>();
                    currentTargetingUnitStreak = new List<Unit>();
                    targettedTargets = new List<AnimatedIcon>();
                }
                else if (usingTargetingSystem && !firingSpecial)
                {
                    FireSpecial();
                }
                else
                {
                    if (owner.SpecialAmmo <= 0)
                    {
                        HeroController.FlashSpecialAmmo(PlayerNum);
                    }
                    hero.ActivateGun();
                }
            }
        }

        private void FireSpecial()
        {
            if (targetSystem != null)
            {
                targetSystem.DestroyNetworked();
            }
            firingSpecial = true;
            owner.StartCoroutine(ShootSpecialEnumerator());
        }

        private IEnumerator ShootSpecialEnumerator()
        {
            while (targettedUnits.Count > 0)
            {
                if (owner.health <= 0)
                {
                    yield break;
                }
                if (targettedUnits[0] != null && targettedUnits[0].health > 0)
                {
                    if (remoteProjectilePrefab != null)
                    {
                        Projectile missile = ProjectileController.SpawnProjectileOverNetwork(remoteProjectilePrefab, owner,
                            X, Y + 6.5f, Direction * missileSpeed, 0f, false, PlayerNum, false, false, 0f);
                        var guided = missile.GetComponent<BulletGuidedRobrocop>();
                        if (guided != null)
                        {
                            guided.targetUnits = new List<BroforceObject> { targettedUnits[0] };
                            guided.targetIcons = new List<AnimatedIcon> { targettedTargets[0] };
                        }
                    }
                    targettedUnits.RemoveAt(0);
                    targettedTargets.RemoveAt(0);
                    Sound.GetInstance().PlaySoundEffectAt(attackSounds, 0.3f, owner.transform.position, 1f + owner.pitchShiftAmount);
                    hero.GunFrame = 3;
                    owner.gunSprite.SetLowerLeftPixel(32f * targetingGunColumn, 32f);
                    yield return new WaitForSeconds(missileFireInterval);
                }
                else
                {
                    targettedUnits.RemoveAt(0);
                    if (targettedTargets.Count > 0 && targettedTargets[0] != null)
                    {
                        Object.Destroy(targettedTargets[0].gameObject);
                    }
                    if (targettedTargets.Count > 0)
                    {
                        targettedTargets.RemoveAt(0);
                    }
                }
            }
            targettedUnits = null;
            currentTargetingUnitStreak = null;
            currentTargetingObjectStreak = null;
            usingTargetingSystem = false;
            firingSpecial = false;
        }

        private void CreateTargetOnUnit(Unit unit)
        {
            FollowingObject marker = Object.Instantiate(ProjectileController.instance.targetPrefab);
            marker.Follow(unit.transform, Vector3.up * 8f, 0);
            targettedTargets.Add(marker.GetComponent<AnimatedIcon>());
            if (targetSystem != null)
            {
                targetSystem.PlayHitSound();
            }
        }

        private void CreateTargetOnObject(TargetableObject target)
        {
            FollowingObject marker = Object.Instantiate(ProjectileController.instance.targetPrefab);
            marker.Follow(target.transform, Vector3.zero, 0);
            targettedTargets.Add(marker.GetComponent<AnimatedIcon>());
            if (targetSystem != null)
            {
                targetSystem.PlayHitSound();
            }
        }

        public override void HandleAfterCheckInput()
        {
            if (!usingTargetingSystem || targetSystem == null)
            {
                return;
            }
            currentTargetingStreakTime += hero.DeltaTime;
            DirectionEnum prevDirection = targetSystem.TravelDirection;
            if (owner.up) targetSystem.TravelDirection = DirectionEnum.Up;
            if (owner.down) targetSystem.TravelDirection = DirectionEnum.Down;
            if (owner.left) targetSystem.TravelDirection = DirectionEnum.Left;
            if (owner.right) targetSystem.TravelDirection = DirectionEnum.Right;
            if (prevDirection != targetSystem.TravelDirection && currentTargetingStreakTime > 0.3f)
            {
                currentTargetingStreakTime = 0f;
                currentTargetingUnitStreak = new List<Unit>();
                currentTargetingObjectStreak = new List<TargetableObject>();
            }
            owner.up = false;
            owner.left = false;
            owner.right = false;
            owner.down = false;
            owner.buttonJump = false;
            targetingTime -= hero.DeltaTime;
            if (targetingTime < 0f)
            {
                FireSpecial();
            }
        }

        public override void Update()
        {
            if (specialCooldownTimer < specialCooldownDelay)
            {
                specialCooldownTimer += Time.deltaTime;
            }
            if (usingTargetingSystem && targetSystem != null)
            {
                Unit nextUnit = Map.GetNextClosestUnit(PlayerNum, targetSystem.TravelDirection,
                    scanningRange, scanningRange, targetSystem.transform.position.x, targetSystem.transform.position.y,
                    currentTargetingUnitStreak);
                if (nextUnit != null)
                {
                    targettedUnits.Add(nextUnit);
                    currentTargetingUnitStreak.Add(nextUnit);
                    currentTargetingStreakTime = 0f;
                    CreateTargetOnUnit(nextUnit);
                }
                else
                {
                    TargetableObject nextTarget = Map.GetNextClosestTargetableObject(PlayerNum, targetSystem.TravelDirection,
                        scanningRange, scanningRange, targetSystem.transform.position.x, targetSystem.transform.position.y,
                        currentTargetingObjectStreak);
                    if (nextTarget != null)
                    {
                        targettedUnits.Add(nextTarget);
                        currentTargetingObjectStreak.Add(nextTarget);
                        currentTargetingStreakTime = 0f;
                        CreateTargetOnObject(nextTarget);
                    }
                }
            }
        }

        public override void HandleAfterDeath()
        {
            if (targettedTargets != null)
            {
                for (int i = 0; i < targettedTargets.Count; i++)
                {
                    if (targettedTargets[i] != null)
                    {
                        targettedTargets[i].GoAway();
                    }
                }
            }
        }
    }
}

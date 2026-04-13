using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("BroGummer")]
    public class BroGummerSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.BroGummer;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special2Sounds == null) special2Sounds = sourceBro.soundHolder.special2Sounds.CloneArray();
            if (special3Sounds == null) special3Sounds = sourceBro.soundHolder.special3Sounds.CloneArray();
            if (special4Sounds == null) special4Sounds = sourceBro.soundHolder.special4Sounds.CloneArray();
        }
        public float targetingDuration = 5f;
        public float scanningRange = 10f;
        public float specialCooldownDelay = 0.13f;
        public float remoteProjectileSpeed = 800f;
        public float recoilForce = 110f;

        public AudioClip[] special2Sounds;
        public AudioClip[] special3Sounds;
        public AudioClip[] special4Sounds;

        [JsonIgnore]
        private bool usingTargetingSystem;
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
        private Projectile specialSniperBulletPrefab;
        [JsonIgnore]
        private Projectile remB;
        [JsonIgnore]
        private AudioSource heartbeatSound;
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

            var broGummer = owner as BroGummer;
            if (broGummer == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BroGummer);
                broGummer = prefab as BroGummer;
            }
            if (broGummer != null)
            {
                targetSystemPrefab = broGummer.broGummerSniperTargetPrefab;
                specialSniperBulletPrefab = broGummer.specialSniperBulletPrefab;
            }
        }

        public override void PressSpecial()
        {
            if (owner.IsAlive() && !owner.hasBeenCoverInAcid && !hero.DoingMelee)
            {
                UseSpecial();
            }
        }

        public override void UseSpecial()
        {
            if (specialCooldownTimer < specialCooldownDelay)
            {
                return;
            }
            specialCooldownTimer = 0f;
            if (remB != null)
            {
                return;
            }
            if (owner.IsMine)
            {
                if (owner.SpecialAmmo > 0 && !usingTargetingSystem)
                {
                    owner.SpecialAmmo--;
                    HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
                    hero.GunFrame = 12;
                    hero.SetGunSprite(12, 0);
                    owner.fire = false;
                    owner.SetFieldValue("controllingProjectile", true);
                    if (owner.player != null)
                    {
                        owner.player.SetAvatarSpecialFrame(5f);
                    }
                    owner.frame = 0;
                    Sound.GetInstance().PlaySoundEffectAt(special2Sounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                    heartbeatSound = Sound.GetInstance().PlaySoundEffectAt(special3Sounds, 0.3f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                    usingTargetingSystem = true;
                    targetingTime = targetingDuration;
                    DirectionEnum direction = owner.transform.localScale.x <= 0f ? DirectionEnum.Left : DirectionEnum.Right;
                    if (targetSystemPrefab != null)
                    {
                        targetSystem = Networking.Networking.Instantiate(targetSystemPrefab,
                            owner.transform.position + Vector3.up * 6.5f, Quaternion.identity, null, false);
                        targetSystem.SetupTargetting(owner, direction);
                    }
                    targettedUnits = new List<BroforceObject>();
                    currentTargetingObjectStreak = new List<TargetableObject>();
                    currentTargetingUnitStreak = new List<Unit>();
                    targettedTargets = new List<AnimatedIcon>();
                }
                else if (usingTargetingSystem)
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
            usingTargetingSystem = false;
            Sound.GetInstance().PlaySoundEffectAt(attackSounds, 1f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
            Sound.GetInstance().PlaySoundEffectAt(special4Sounds, 0.3f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
            if (heartbeatSound != null)
            {
                heartbeatSound.Stop();
            }
            EffectsController.CreateMuzzleFlashBigEffect(X + owner.Direction * 14f, Y + 8f, -25f, owner.Direction * 80f, 0f);
            if (owner.player != null)
            {
                owner.player.hud.specialFrameTime = 0f;
            }
            owner.xIBlast -= owner.Direction * recoilForce;
            if (specialSniperBulletPrefab != null)
            {
                remB = ProjectileController.SpawnProjectileOverNetwork(specialSniperBulletPrefab, owner,
                    X + 6f * owner.transform.localScale.x, Y + 11f,
                    owner.transform.localScale.x * remoteProjectileSpeed, 0f, true, PlayerNum, true, false, 0f);
                var guided = remB.GetComponent<BulletGuidedRobrocop>();
                if (guided != null)
                {
                    guided.targetUnits = new List<BroforceObject>(targettedUnits);
                    guided.targetIcons = new List<AnimatedIcon>(targettedTargets);
                }
            }
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
            if (owner.up)
                targetSystem.TravelDirection = DirectionEnum.Up;
            else if (owner.down)
                targetSystem.TravelDirection = DirectionEnum.Down;
            else if (owner.left)
                targetSystem.TravelDirection = DirectionEnum.Left;
            else if (owner.right)
                targetSystem.TravelDirection = DirectionEnum.Right;
            else
                targetSystem.TravelDirection = DirectionEnum.None;

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

        public override bool HandleRunFiring()
        {
            if (usingTargetingSystem && owner.fire)
            {
                FireSpecial();
                owner.fireDelay = 0.3f;
                return false;
            }
            return true;
        }

        public override bool HandleRunGun()
        {
            if (usingTargetingSystem)
            {
                hero.SetGunSprite(hero.GunFrame, 0);
                return false;
            }
            return true;
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
                    Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 0.3f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
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
                        Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 0.3f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                        CreateTargetOnObject(nextTarget);
                    }
                }
            }
        }

        public override void HandleLateUpdate()
        {
            if (usingTargetingSystem)
            {
                hero.Sprite.SetLowerLeftPixel(0f, hero.SpritePixelHeight * 10);
            }
        }

        public override bool HandleDeath()
        {
            if (usingTargetingSystem)
            {
                FireSpecial();
            }
            return true;
        }
    }
}

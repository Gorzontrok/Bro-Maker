using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    [SpecialPreset("Predabro")]
    public class PredabroSpecial : SpecialAbility
    {
        private enum SpecialState
        {
            WaitForTarget,
            AquireTarget,
            LockOn,
            ClearLockOn,
            finish
        }

        public float stealthModeDuration = 20f;
        public float scanRange = 250f;
        public float maxPulseTime = 3.3f;
        public float emptyPulseTime = 1.3f;
        public float fireCoolDownDuration = 0.5f;
        public float lockOnSpeed = 6f;
        public float bulletSpeed = 700f;
        public float cloakingSoundVolume = 0.3f;
        public float cannonSoundVolume = 1f;

        public AudioClip selfDestructSound;
        public Material[] hudSpecialCountDownMaterials;

        [JsonIgnore]
        private bool scanning;
        [JsonIgnore]
        private float stealthModeTime;
        [JsonIgnore]
        private int pulsesSent;
        [JsonIgnore]
        private float pulseTimer;
        [JsonIgnore]
        private float fireCoolDown;
        [JsonIgnore]
        private float lockOn;
        [JsonIgnore]
        private SpecialState state;
        [JsonIgnore]
        private bool atLeastOneUnitTargetedBySpecial;
        [JsonIgnore]
        private List<BroforceObject> targetedEntities = new List<BroforceObject>();
        [JsonIgnore]
        private List<BroforceObject> entitiesFiredAt = new List<BroforceObject>();
        [JsonIgnore]
        private BroforceObject lockingOntoUnit;
        [JsonIgnore]
        private Projectile lastShoulderBullet;
        [JsonIgnore]
        private float targettingDistance;
        [JsonIgnore]
        private float targettingDistancePrev;
        [JsonIgnore]
        private float cannonAimAngle;
        [JsonIgnore]
        private float prevAngle;
        [JsonIgnore]
        private float distanceTarget;
        [JsonIgnore]
        private float angleOfTarget;
        [JsonIgnore]
        private PredabroTarget prevTarget;
        [JsonIgnore]
        private float glitchTimer;
        [JsonIgnore]
        private int glitchFrame;
        [JsonIgnore]
        private float glitchFrameRate = 0.2f;
        [JsonIgnore]
        private int warningBeepCount;
        [JsonIgnore]
        private float selfDestructTimer;
        [JsonIgnore]
        private float selfDestructTimerInterval = 1f;
        [JsonIgnore]
        private float pitchShift;
        [JsonIgnore]
        private bool notConsideredDead = true;
        [JsonIgnore]
        private bool selfDestructed;

        // Prefab references
        [JsonIgnore]
        private Material stealthMaterial;
        [JsonIgnore]
        private Material stealthGunMaterial;
        [JsonIgnore]
        private Material normalMaterial;
        [JsonIgnore]
        private Material normalGunMaterial;
        [JsonIgnore]
        private Projectile shoulderCannonBullet;
        [JsonIgnore]
        private PredabroTarget targetPrefab;
        [JsonIgnore]
        private AudioClip cloakingSound;
        [JsonIgnore]
        private AudioClip lazerCannonSound;
        [JsonIgnore]
        private Transform shoulderCannonSprite;
        [JsonIgnore]
        private ReplicateSprite outlineSprite;
        [JsonIgnore]
        private SpriteSM searchRing;
        [JsonIgnore]
        private SpriteSM[] laserSightLazers;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            normalMaterial = owner.GetComponent<Renderer>().sharedMaterial;
            normalGunMaterial = owner.gunSprite.GetComponent<Renderer>().sharedMaterial;

            var predabro = owner as Predabro;
            var prefab = HeroController.GetHeroPrefab(HeroType.Predabro) as Predabro;
            var source = predabro ?? prefab;
            if (source != null)
            {
                stealthMaterial = source.stealthMaterial;
                stealthGunMaterial = source.stealthGunMaterial;
                shoulderCannonBullet = source.shoulderCannonBullet;
                targetPrefab = source.targetPrefab;
                cloakingSound = source.cloakingSound;
                lazerCannonSound = source.lazerCannonSound;
                selfDestructSound = source.selfDestructSoundSound;
                hudSpecialCountDownMaterials = source.hudSpecialCountDownMaterials;
            }

            if (predabro != null)
            {
                shoulderCannonSprite = predabro.shoulderCannonSprite;
                outlineSprite = predabro.outlineSprite;
                searchRing = predabro.searchRing;
                laserSightLazers = predabro.laserSightLazers;
            }
            else if (prefab != null)
            {
                // Instantiate child objects from prefab for non-Predabro owners
                if (prefab.shoulderCannonSprite != null)
                {
                    shoulderCannonSprite = Object.Instantiate(prefab.shoulderCannonSprite, owner.transform);
                    shoulderCannonSprite.localPosition = prefab.shoulderCannonSprite.localPosition;
                }
                if (prefab.shoulderCannonBaseSprite != null)
                {
                    var baseSprite = Object.Instantiate(prefab.shoulderCannonBaseSprite, owner.transform);
                    baseSprite.transform.localPosition = prefab.shoulderCannonBaseSprite.transform.localPosition;
                }
                if (prefab.outlineSprite != null)
                {
                    outlineSprite = Object.Instantiate(prefab.outlineSprite, owner.transform);
                    outlineSprite.transform.localPosition = prefab.outlineSprite.transform.localPosition;
                    outlineSprite.hide = true;
                }
                if (prefab.searchRing != null)
                {
                    searchRing = Object.Instantiate(prefab.searchRing, owner.transform);
                    searchRing.transform.localPosition = prefab.searchRing.transform.localPosition;
                    searchRing.gameObject.SetActive(false);
                }
                if (prefab.laserSightLazers != null && prefab.laserSightLazers.Length > 0)
                {
                    laserSightLazers = new SpriteSM[prefab.laserSightLazers.Length];
                    for (int i = 0; i < prefab.laserSightLazers.Length; i++)
                    {
                        if (prefab.laserSightLazers[i] != null && shoulderCannonSprite != null)
                        {
                            laserSightLazers[i] = Object.Instantiate(prefab.laserSightLazers[i], shoulderCannonSprite);
                            laserSightLazers[i].transform.localPosition = prefab.laserSightLazers[i].transform.localPosition;
                        }
                    }
                }
            }
        }

        public override void UseSpecial()
        {
            if (owner.health <= 0 || owner.SpecialAmmo <= 0)
            {
                return;
            }
            if (owner.IsMine)
            {
                TurnOnStealthMode();
            }
        }

        private void TurnOnStealthMode()
        {
            if (scanning)
            {
                return;
            }
            owner.SpecialAmmo--;
            Map.ForgetPlayer(PlayerNum, true, false);
            stealthModeTime = stealthModeDuration;
            PlayCloakingSound();
            if (stealthMaterial != null)
            {
                owner.GetComponent<Renderer>().sharedMaterial = stealthMaterial;
            }
            if (stealthGunMaterial != null)
            {
                owner.gunSprite.GetComponent<Renderer>().sharedMaterial = stealthGunMaterial;
            }
            owner.gameObject.layer = LayerMask.NameToLayer("Displacement");
            owner.gunSprite.gameObject.layer = LayerMask.NameToLayer("Displacement");
            if (outlineSprite != null)
            {
                outlineSprite.hide = false;
            }
            if (searchRing != null)
            {
                searchRing.gameObject.SetActive(true);
            }
            hero.DeactivateGun();
            pulsesSent = 0;
            scanning = true;
            BeginScan();
        }

        private void BeginScan()
        {
            entitiesFiredAt.Clear();
            pulseTimer = 0f;
            state = SpecialState.WaitForTarget;
            fireCoolDown = 0f;
            atLeastOneUnitTargetedBySpecial = false;
        }

        private void StopUsingSpecial()
        {
            owner.frame = 0;
            hero.UsingSpecial = false;
            hero.ActivateGun();
            hero.ChangeFrame();
            if (normalMaterial != null)
            {
                owner.GetComponent<Renderer>().sharedMaterial = normalMaterial;
            }
            if (normalGunMaterial != null)
            {
                owner.gunSprite.GetComponent<Renderer>().sharedMaterial = normalGunMaterial;
            }
            owner.gameObject.layer = LayerMask.NameToLayer("Units");
            owner.gunSprite.gameObject.layer = LayerMask.NameToLayer("Units");
            if (outlineSprite != null)
            {
                outlineSprite.hide = true;
            }
            if (searchRing != null)
            {
                searchRing.gameObject.SetActive(false);
            }
            stealthModeTime = 0f;
            scanning = false;
            EndStealthGlitch();
            if (!atLeastOneUnitTargetedBySpecial)
            {
                owner.SpecialAmmo++;
            }
        }

        public override void Update()
        {
            if (pulsesSent >= 1 && scanning)
            {
                StopUsingSpecial();
            }
            RunShoulderCannon();
        }

        private void RunShoulderCannon()
        {
            BroforceObject closestTarget = null;
            if (scanning)
            {
                closestTarget = GetClosestInSightUnit();
                RunStealthGlitch();
            }
            fireCoolDown -= Time.deltaTime;
            switch (state)
            {
                case SpecialState.AquireTarget:
                    if (closestTarget != null)
                    {
                        lockingOntoUnit = closestTarget;
                        targettingDistancePrev = targettingDistance;
                        prevAngle = cannonAimAngle;
                        lockOn = 0f;
                        state = SpecialState.LockOn;
                        fireCoolDown = fireCoolDownDuration;
                    }
                    else
                    {
                        state = SpecialState.ClearLockOn;
                    }
                    break;
                case SpecialState.LockOn:
                    if (lockingOntoUnit && lockingOntoUnit.predabroTarget == null)
                    {
                        lockingOntoUnit = null;
                    }
                    if (lockingOntoUnit && shoulderCannonSprite != null)
                    {
                        Vector3 targetPos = lockingOntoUnit.predabroTarget.transform.position;
                        if (!entitiesFiredAt.Contains(lockingOntoUnit))
                        {
                            Vector3 toTarget = targetPos - (shoulderCannonSprite.position + shoulderCannonSprite.right * 4f);
                            toTarget.z = 0f;
                            distanceTarget = toTarget.magnitude - 4f;
                            toTarget.Normalize();
                            angleOfTarget = Vector3.Angle(Vector3.up, toTarget) - 90f;
                            if (lockOn >= 1f)
                            {
                                lastShoulderBullet = FireShoulderCannonAtUnit(lockingOntoUnit);
                                entitiesFiredAt.Add(lockingOntoUnit);
                            }
                        }
                    }
                    if (fireCoolDown <= 0f || (lockOn >= 1f && (lastShoulderBullet == null || lockingOntoUnit == null)))
                    {
                        state = SpecialState.ClearLockOn;
                    }
                    break;
                case SpecialState.ClearLockOn:
                    distanceTarget = 0f;
                    lockOn = 0f;
                    targettingDistancePrev = targettingDistance;
                    prevAngle = cannonAimAngle;
                    state = scanning ? SpecialState.WaitForTarget : SpecialState.finish;
                    break;
                case SpecialState.finish:
                    angleOfTarget = 0f;
                    distanceTarget = 0f;
                    break;
                case SpecialState.WaitForTarget:
                    if (closestTarget != null)
                    {
                        if (targettingDistance <= 0.02f)
                        {
                            angleOfTarget = 0f;
                        }
                        state = SpecialState.AquireTarget;
                    }
                    distanceTarget = 0f;
                    break;
            }
            lockOn += Time.deltaTime * lockOnSpeed;
            lockOn = Mathf.Clamp01(lockOn);
            targettingDistance = Mathf.Lerp(targettingDistancePrev, distanceTarget, lockOn);
            cannonAimAngle = Mathf.Lerp(prevAngle, angleOfTarget, lockOn);
            if (shoulderCannonSprite != null)
            {
                shoulderCannonSprite.eulerAngles = new Vector3(0f, 0f, -cannonAimAngle);
            }
            if (laserSightLazers != null)
            {
                foreach (SpriteSM laser in laserSightLazers)
                {
                    laser.width = targettingDistance;
                    laser.SetPixelDimensions((int)targettingDistance, (int)laser.pixelDimensions.y);
                    laser.RefreshVertices();
                }
            }
        }

        private Projectile FireShoulderCannonAtUnit(BroforceObject unit)
        {
            if (shoulderCannonSprite == null || shoulderCannonBullet == null)
            {
                return null;
            }
            Vector3 origin = shoulderCannonSprite.position + shoulderCannonSprite.right * 4f;
            Vector3 direction = unit.predabroTarget.transform.position - origin;
            direction.Normalize();
            direction *= bulletSpeed;
            SortOfFollow.Shake(0.15f);
            EffectsController.CreateMuzzleFlashRoundEffectBlue(origin.x, origin.y, -25f, direction.x * 0.15f, direction.y * 0.15f, owner.transform);
            Projectile projectile = ProjectileController.SpawnProjectileLocally(shoulderCannonBullet, owner, origin.x, origin.y, direction.x, direction.y, PlayerNum, false, -8f);
            Sound.GetInstance().PlaySoundEffectAt(lazerCannonSound, cannonSoundVolume, owner.transform.position, Random.Range(0.9f, 0.1f), true, false, false, 0f);
            return projectile;
        }

        private BroforceObject GetClosestInSightUnit()
        {
            pulseTimer += Time.deltaTime * 0.9f;
            bool continuePulse = true;
            if (pulseTimer > maxPulseTime)
            {
                continuePulse = false;
            }
            else if (pulseTimer > emptyPulseTime && targetedEntities.Count == 0)
            {
                continuePulse = false;
            }
            if (!continuePulse)
            {
                if (pulsesSent < 1)
                {
                    PlayCloakingSound();
                }
                pulsesSent++;
                pulseTimer = 0f;
                entitiesFiredAt.Clear();
                targetedEntities.Clear();
            }
            float pulseRange = scanRange * Mathf.Min(pulseTimer, 1f);
            if (searchRing != null)
            {
                if (pulseTimer > 1f)
                {
                    searchRing.transform.localScale = Vector3.zero;
                }
                else
                {
                    searchRing.transform.localScale = Vector3.one * (pulseRange * 2f / searchRing.width);
                    searchRing.SetAlpha((1f - pulseTimer) * 4f);
                }
            }

            targetedEntities = new List<BroforceObject>();
            List<Projectile> projectiles = Map.GetProjectilesInRange(PlayerNum, (int)pulseRange, X, Y);
            for (int i = 0; i < projectiles.Count; i++)
            {
                if (projectiles[i] is Rocket && projectiles[i].playerNum < 0)
                {
                    targetedEntities.Add(projectiles[i]);
                }
            }
            List<Unit> units = Map.GetUnitsInRange((int)pulseRange, X, Y, false);
            for (int j = units.Count - 1; j >= 0; j--)
            {
                if (!units[j].gameObject.activeInHierarchy || units[j] is RescueBro || units[j] is Villager || units[j] is MookCaptainCutscene)
                {
                    units.RemoveAt(j);
                }
                else if (!units[j].IsEnemy)
                {
                    units.RemoveAt(j);
                }
            }
            for (int k = 0; k < units.Count; k++)
            {
                targetedEntities.Add(units[k]);
            }
            List<TargetableObject> targetables = Map.GeTargetableObjectsInRange(PlayerNum, pulseRange, X, Y);
            for (int l = 0; l < targetables.Count; l++)
            {
                targetedEntities.Add(targetables[l]);
            }
            List<Grenade> grenades = Map.GetGrenadesInRange(PlayerNum, pulseRange, X, Y);
            for (int m = 0; m < grenades.Count; m++)
            {
                targetedEntities.Add(grenades[m]);
            }
            if (targetedEntities.Count > 0)
            {
                atLeastOneUnitTargetedBySpecial = true;
            }

            Vector3 cannonPos = shoulderCannonSprite != null ? shoulderCannonSprite.position : owner.transform.position;
            BroforceObject closest = null;
            float closestDist = 50000f;
            Camera mainCam = CameraController.MainCam;
            for (int n = targetedEntities.Count - 1; n >= 0; n--)
            {
                bool remove = false;
                BroforceObject entity = targetedEntities[n];
                if (entity == null)
                {
                    remove = true;
                }
                else
                {
                    if (entitiesFiredAt.Contains(entity))
                    {
                        remove = true;
                    }
                    Vector3 entityPos = targetedEntities[n].transform.position;
                    if (targetedEntities[n] is Unit)
                    {
                        entityPos.y += 10f;
                    }
                    Vector3 viewport = mainCam.WorldToViewportPoint(entityPos);
                    if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f)
                    {
                        remove = true;
                    }
                    float dx = X - entity.X;
                    int side = (int)Mathf.Sign(dx);
                    if (side == (int)Direction && !owner.GetFieldValue<bool>("wallClimbing") && !owner.GetFieldValue<bool>("wallDrag"))
                    {
                        remove = true;
                    }
                    if (!remove && shoulderCannonSprite != null)
                    {
                        Vector3 entityWorldPos = entity.transform.position;
                        if (entity is Unit)
                        {
                            entityWorldPos += Vector3.up * 10f;
                        }
                        Vector3 toEntity = entityWorldPos - cannonPos;
                        float dist = toEntity.magnitude;
                        RaycastHit hit;
                        Physics.SphereCast(shoulderCannonSprite.position, 3f, toEntity.normalized, out hit, dist, Map.groundLayer);
                        float threshold = (entity is TargetableObject) ? 32f : 5f;
                        if (hit.collider != null && hit.distance < dist - threshold)
                        {
                            remove = true;
                        }
                        else if (closestDist > dist)
                        {
                            closest = entity;
                            closestDist = dist;
                        }
                    }
                }
                if (remove)
                {
                    targetedEntities.RemoveAt(n);
                }
            }
            foreach (BroforceObject entity in targetedEntities)
            {
                if (entity.predabroTarget == null)
                {
                    CreateTargetMarker(entity);
                }
                entity.predabroTarget.RefillCoolDown();
            }
            return closest;
        }

        private PredabroTarget CreateTargetMarker(BroforceObject unit)
        {
            if (targetPrefab == null)
            {
                return null;
            }
            prevTarget = EffectsController.InstantiateEffect(targetPrefab) as PredabroTarget;
            if (prevTarget != null)
            {
                prevTarget.Setup(unit, owner as Predabro);
            }
            return prevTarget;
        }

        private void RunStealthGlitch()
        {
            glitchTimer += Time.deltaTime;
            if (glitchTimer > glitchFrameRate)
            {
                glitchTimer = 0f - Random.value * glitchFrameRate;
                glitchFrame += Random.Range(1, 3);
                glitchFrame %= 8;
                float num = 0.25f + (float)glitchFrame / 8f * 0.5f;
                Color color = new Color(num, num, num, 1f);
                hero.Sprite.meshRender.material.SetColor("_TintColor", color);
                owner.gunSprite.meshRender.material.SetColor("_TintColor", color);
            }
        }

        private void EndStealthGlitch()
        {
            hero.Sprite.meshRender.material.SetColor("_TintColor", Color.gray);
            owner.gunSprite.meshRender.material.SetColor("_TintColor", Color.gray);
        }

        private void PlayCloakingSound()
        {
            if (cloakingSound != null)
            {
                Sound.GetInstance().PlaySoundEffectAt(cloakingSound, cloakingSoundVolume, owner.transform.position, Random.Range(0.8f, 1.2f), true, false, false, 0f);
            }
        }

        public override void HandleLateUpdate()
        {
            RunSelfDestructTimer();
        }

        private void RunSelfDestructTimer()
        {
            if (owner.actionState == ActionState.Dead && !selfDestructed)
            {
                if (!owner.wasFire && owner.fire && warningBeepCount > 1)
                {
                    SelfDestruct();
                }
                selfDestructTimer += Time.deltaTime;
                if (selfDestructTimer > selfDestructTimerInterval)
                {
                    warningBeepCount++;
                    Sound.GetInstance().PlaySoundEffectAt(selfDestructSound, 0.5f, owner.transform.position, 1f + pitchShift, true, false, false, 0f);
                    pitchShift += 0.1f;
                    selfDestructTimer = 0f;
                    selfDestructTimerInterval -= 0.2f;
                    selfDestructTimerInterval = Mathf.Max(selfDestructTimerInterval, 0.3f);
                    Map.AttractMooks(owner.X, owner.Y, 200f, 100f);
                    EffectsController.CreateRedWarningDiamondHuge(owner.X, owner.Y + 4f, owner.transform);
                    SetHudSymbols();
                    if (warningBeepCount > 5)
                    {
                        notConsideredDead = false;
                    }
                    if (warningBeepCount > 6 && !selfDestructed)
                    {
                        SelfDestruct();
                    }
                }
            }
            else
            {
                warningBeepCount = 0;
            }
        }

        private void SetHudSymbols()
        {
            if (warningBeepCount < 2)
            {
                owner.SpecialAmmo = 3;
            }
            else if (warningBeepCount < 4)
            {
                owner.SpecialAmmo = 2;
            }
            else
            {
                owner.SpecialAmmo = 1;
            }
            if (owner.player != null && owner.player.hud != null && hudSpecialCountDownMaterials != null)
            {
                foreach (SpriteSM spriteSM in owner.player.hud.grenadeIcons)
                {
                    spriteSM.meshRender.material = hudSpecialCountDownMaterials.RandomElement<Material>();
                }
            }
        }

        private void SelfDestruct()
        {
            if (!selfDestructed)
            {
                selfDestructed = true;
                int num = 100;
                MapController.DamageGround(owner, 25, DamageType.Explosion, (float)num, owner.X, owner.Y, null, false);
                Map.ExplodeUnits(owner, 25, DamageType.Explosion, (float)num * 1.5f, (float)num * 1.33f, owner.X, owner.Y, (float)num, 400f, owner.playerNum, false, true, true);
                owner.CallMethod("Gib", DamageType.Explosion, 0f, 0f);
                EffectsController.CreateExplosionRangePop(owner.X, owner.Y, -1f, (float)(num * 2));
                EffectsController.CreateHugeExplosion(owner.X, owner.Y, (float)num * 0.25f, (float)num * 0.25f, 120f, 1f, 24f, 1f, 0.5f, 2, 140, 250f, (float)num * 1.5f, 0.3f, 0.2f);
            }
        }

        public override bool HandleIsAlive(ref bool result)
        {
            if (notConsideredDead)
            {
                result = true;
                return false;
            }
            return true;
        }

        public override bool HandleDeath()
        {
            StopUsingSpecial();
            return true;
        }

        public override bool HandleGib(DamageType damageType, float xI, float yI)
        {
            StopUsingSpecial();
            return true;
        }

        public override bool HandleIsInStealthMode()
        {
            if (stealthModeTime > 0f)
            {
                return false;
            }
            return true;
        }

        public override bool HandleAlertNearbyMooks()
        {
            return stealthModeTime <= 0f;
        }

        public override void HandleAfterRecallBro()
        {
            StopUsingSpecial();
        }

        public override bool HandleAttachToHeli()
        {
            if (searchRing != null)
            {
                searchRing.gameObject.SetActive(false);
            }
            return true;
        }

        public override void Cleanup()
        {
            if (!(owner is Predabro))
            {
                if (shoulderCannonSprite != null) Object.Destroy(shoulderCannonSprite.gameObject);
                if (outlineSprite != null) Object.Destroy(outlineSprite.gameObject);
                if (searchRing != null) Object.Destroy(searchRing.gameObject);
            }
        }
    }
}

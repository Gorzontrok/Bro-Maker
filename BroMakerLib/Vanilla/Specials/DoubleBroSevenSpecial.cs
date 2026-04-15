using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Loaders;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Specials
{
    /// <summary>Controls which gadget type is selected when `SpecialAmmo` falls outside the 1–5 gadget range.</summary>
    public enum SpecialOverflowMode
    {
        /// <summary>Wraps ammo into the 1–5 range using modulo arithmetic.</summary>
        Wrap,
        /// <summary>Clamps to the highest gadget type (5).</summary>
        Clamp,
        /// <summary>Parses `overflowSpecialType` by name; falls back to TearGas if parsing fails.</summary>
        Fallback,
        /// <summary>Passes the raw ammo value through unchanged; may resolve to `None` if out of range.</summary>
        Skip
    }

    /// <summary>Double Bro Seven's rotating gadget special with balaclava, jetpack, laser, martini, and tear gas modes.</summary>
    [SpecialPreset("DoubleBroSeven")]
    public class DoubleBroSevenSpecial : SpecialAbility
    {
        protected override HeroType SourceBroType => HeroType.DoubleBroSeven;

        private const int MaxGadgetType = 5;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;
            if (special3Sounds == null) special3Sounds = sourceBro.soundHolder.special3Sounds.CloneArray();
            if (special2Sounds == null) special2Sounds = sourceBro.soundHolder.special2Sounds.CloneArray();
            if (attack3Sounds == null) attack3Sounds = sourceBro.soundHolder.attack3Sounds.CloneArray();
            if (attack2Sounds == null) attack2Sounds = sourceBro.soundHolder.attack2Sounds.CloneArray();
            if (attack4Sounds == null) attack4Sounds = sourceBro.soundHolder.attack4Sounds.CloneArray();
        }
        public int martiniRow = 7;
        public int jetpackRow = 8;
        public int laserRow = 8;
        public int balaclavaRow = 8;
        public int balaclavaRemoveRow = 8;
        public int jetpackSpriteColumn = 14;
        public int balaclavaColumn = 26;
        /// <summary>How to resolve the gadget type when `SpecialAmmo` is outside the 1–5 range.</summary>
        public SpecialOverflowMode overflowMode = SpecialOverflowMode.Wrap;
        /// <summary>Gadget name used when `overflowMode` is `Fallback` and parsing fails.</summary>
        public string overflowSpecialType = "TearGas";
        /// <summary>Duration in seconds of the balaclava stealth mode.</summary>
        public float balaclavaTimeDuration = 5f;
        /// <summary>Duration in seconds of the jetpack fuel after activation.</summary>
        public float jetPackFuelDuration = 0.8f;
        public float startLaserAngle = 265f;
        public float endLaserAngle = 120f;
        /// <summary>Angular step in degrees per raycast iteration while sweeping the laser.</summary>
        public float laserStepM = 0.33f;
        public float minSquareLaserDist = 5f;
        public float laserAngleSpeed = 120f;
        public float laserAngleAcceleration = 80f;
        public float laserVolume = 0.25f;
        /// <summary>Name of the martini glass grenade prefab.</summary>
        public string martiniGrenadeName = "MartiniGlass";
        /// <summary>Name of the tear gas grenade prefab.</summary>
        public string tearGasGrenadeName = "TearGas";

        /// <summary>Sounds played when activating the jetpack gadget.</summary>
        public AudioClip[] special3Sounds;
        /// <summary>Sounds played when activating the laser wristwatch gadget.</summary>
        public AudioClip[] special2Sounds;
        /// <summary>Sounds played when the jetpack fires its liftoff blast.</summary>
        public AudioClip[] attack3Sounds;
        /// <summary>Sounds played during the martini-drinking animation.</summary>
        public AudioClip[] attack2Sounds;
        /// <summary>Sounds played when equipping and removing the balaclava.</summary>
        public AudioClip[] attack4Sounds;
        [JsonIgnore]
        private DoubleBroSevenSpecialType currentSpecialType;
        [JsonIgnore]
        private int usingSpecialFrame;
        [JsonIgnore]
        private float balaclavaTime;
        [JsonIgnore]
        private float jetPackFuel;
        [JsonIgnore]
        private float laserAngle;
        [JsonIgnore]
        private float startAngleSpeed;
        [JsonIgnore]
        private float laserWidth;
        [JsonIgnore]
        private Vector3 lastLaserHitPos;
        [JsonIgnore]
        private Collider lastLaserCollider;
        [JsonIgnore]
        private int colliderLaserDamage;
        [JsonIgnore]
        private int martinisDrunk;
        [JsonIgnore]
        private bool jetpackActivated;
        [JsonIgnore]
        private float originalSpeed;
        [JsonIgnore]
        private LayerMask unitsLayer;

        [JsonIgnore]
        private Grenade martiniGlass;
        [JsonIgnore]
        private Grenade tearGasGrenade;
        [JsonIgnore]
        private FlameWallExplosion liftOffBlastFlameWall;
        [JsonIgnore]
        private Material balaclavaMaterial;
        [JsonIgnore]
        private Material normalMaterial;
        [JsonIgnore]
        private LineRenderer laserLine;
        [JsonIgnore]
        private AudioSource laserAudio;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            originalSpeed = owner.speed;
            unitsLayer = 1 << LayerMask.NameToLayer("Units");
            startAngleSpeed = laserAngleSpeed;

            martiniGlass = LoadBroforceObjects.GetGrenadeFromName(martiniGrenadeName);
            tearGasGrenade = LoadBroforceObjects.GetGrenadeFromName(tearGasGrenadeName);

            var dbs = owner as DoubleBroSeven;
            if (dbs == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.DoubleBroSeven);
                dbs = prefab as DoubleBroSeven;
            }
            if (dbs != null)
            {
                liftOffBlastFlameWall = dbs.liftOffBlastFlameWall;
                balaclavaMaterial = dbs.balaclavaMaterial;
                laserVolume = dbs.laserVolume;
                startLaserAngle = dbs.startLaserAngle;
                endLaserAngle = dbs.endLaserAngle;
                laserStepM = dbs.GetFieldValue<float>("laserStepM");
                minSquareLaserDist = dbs.minSquareLaserDist;
                laserAngleSpeed = dbs.laserAngleSpeed;
                laserAngleAcceleration = dbs.laserAngleAcceleration;
                startAngleSpeed = laserAngleSpeed;
            }

            var ownerDbs = owner as DoubleBroSeven;
            if (ownerDbs != null)
            {
                laserLine = ownerDbs.laserLine;
                laserAudio = ownerDbs.laserAudio;
            }
            else
            {
                laserLine = owner.gameObject.AddComponent<LineRenderer>();
#pragma warning disable 618
                laserLine.SetVertexCount(2);
#pragma warning restore 618
                laserLine.startWidth = 0f;
                laserLine.endWidth = 0f;
                laserLine.enabled = false;
                if (dbs != null && dbs.laserLine != null)
                {
                    laserLine.material = dbs.laserLine.material;
                    laserLine.startColor = dbs.laserLine.startColor;
                    laserLine.endColor = dbs.laserLine.endColor;
                }
                laserAudio = owner.gameObject.AddComponent<AudioSource>();
                laserAudio.rolloffMode = AudioRolloffMode.Linear;
                laserAudio.minDistance = 350f;
                laserAudio.maxDistance = 500f;
                laserAudio.spatialBlend = 1f;
                laserAudio.volume = 0f;
                laserAudio.dopplerLevel = 0f;
                laserAudio.loop = true;
                laserAudio.playOnAwake = false;
                if (dbs != null && dbs.laserAudio != null && dbs.laserAudio.clip != null)
                {
                    laserAudio.clip = dbs.laserAudio.clip;
                }
            }
        }

        private DoubleBroSevenSpecialType ResolveSpecialType(int ammo)
        {
            if (ammo >= 1 && ammo <= MaxGadgetType)
                return (DoubleBroSevenSpecialType)ammo;

            switch (overflowMode)
            {
                case SpecialOverflowMode.Wrap:
                    return (DoubleBroSevenSpecialType)(((ammo - 1) % MaxGadgetType) + 1);
                case SpecialOverflowMode.Clamp:
                    return (DoubleBroSevenSpecialType)MaxGadgetType;
                case SpecialOverflowMode.Fallback:
                    DoubleBroSevenSpecialType parsed;
                    try { parsed = (DoubleBroSevenSpecialType)System.Enum.Parse(typeof(DoubleBroSevenSpecialType), overflowSpecialType, true); }
                    catch { parsed = DoubleBroSevenSpecialType.TearGas; }
                    return parsed;
                case SpecialOverflowMode.Skip:
                default:
                    return (DoubleBroSevenSpecialType)ammo;
            }
        }

        public override void PressSpecial()
        {
            if (normalMaterial == null)
            {
                normalMaterial = owner.GetComponent<Renderer>().sharedMaterial;
            }
            if (!owner.hasBeenCoverInAcid && !hero.DoingMelee && !hero.UsingSpecial)
            {
                if (owner.SpecialAmmo > 0)
                {
                    hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                    SetSpecialType(ResolveSpecialType(owner.SpecialAmmo));
                    if (hero.UsingSpecial)
                    {
                        if (currentSpecialType == DoubleBroSevenSpecialType.Jetpack)
                        {
                            jetPackFuel = jetPackFuelDuration;
                            Sound.GetInstance().PlaySoundEffectAt(special3Sounds, 0.25f, owner.transform.position, 1f, true, false, false, 0f);
                        }
                        else if (currentSpecialType == DoubleBroSevenSpecialType.LaserWristWatch)
                        {
                            laserAngleSpeed = startAngleSpeed;
                            laserAngle = startLaserAngle;
                            lastLaserHitPos = -Vector3.one;
                            Sound.GetInstance().PlaySoundEffectAt(special2Sounds, 0.5f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                        }
                        else if (currentSpecialType == DoubleBroSevenSpecialType.Balaclava)
                        {
                            Sound.GetInstance().PlaySoundEffectAt(attack4Sounds[0], 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                            balaclavaTime = balaclavaTimeDuration;
                        }
                    }
                }
                else
                {
                    HeroController.FlashSpecialAmmo(PlayerNum);
                }
            }
        }

        private void SetSpecialType(DoubleBroSevenSpecialType type)
        {
            hero.UsingSpecial = true;
            owner.frame = 0;
            usingSpecialFrame = 0;
            owner.counter = 0f;
            currentSpecialType = type;
            hero.ChangeFrame();
        }

        public override void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                switch (currentSpecialType)
                {
                    case DoubleBroSevenSpecialType.Balaclava:
                        Map.ForgetPlayer(PlayerNum, true, false);
                        if (balaclavaMaterial != null)
                        {
                            owner.GetComponent<Renderer>().sharedMaterial = balaclavaMaterial;
                        }
                        break;
                    case DoubleBroSevenSpecialType.LaserWristWatch:
                        break;
                    case DoubleBroSevenSpecialType.Jetpack:
                        Sound.GetInstance().PlaySoundEffectAt(attack3Sounds, 0.5f, owner.transform.position, 0.9f + owner.pitchShiftAmount, true, false, false, 0f);
                        jetpackActivated = true;
                        if (owner.IsMine)
                        {
                            owner.yI = 250f;
                            if (liftOffBlastFlameWall != null)
                            {
                                FlameWallExplosion flame = Networking.Networking.Instantiate<FlameWallExplosion>(liftOffBlastFlameWall,
                                    new Vector3(X - owner.transform.localScale.x * 5f, Y + 9f, 0f), Quaternion.identity, false);
                                flame.Setup(PlayerNum, owner, DirectionEnum.Any);
                            }
                        }
                        break;
                    case DoubleBroSevenSpecialType.Martini:
                        Sound.GetInstance().PlaySoundEffectAt(throwSounds, 0.4f, owner.transform.position, 1f, true, false, false, 0f);
                        if (owner.IsMine && martiniGlass != null)
                        {
                            ProjectileController.SpawnGrenadeOverNetwork(martiniGlass, owner,
                                X + Mathf.Sign(owner.transform.localScale.x) * 6f, Y + 10f,
                                0.001f, 0.011f,
                                Mathf.Sign(owner.transform.localScale.x) * 100f, 110f,
                                PlayerNum, 1f);
                        }
                        break;
                    case DoubleBroSevenSpecialType.TearGas:
                        Sound.GetInstance().PlaySoundEffectAt(throwSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                        if (owner.IsMine && tearGasGrenade != null)
                        {
                            ProjectileController.SpawnGrenadeOverNetwork(tearGasGrenade, owner,
                                X + Mathf.Sign(owner.transform.localScale.x) * 6f, Y + 10f,
                                0.001f, 0.011f,
                                Mathf.Sign(owner.transform.localScale.x) * 200f, 150f,
                                PlayerNum, 1f);
                        }
                        break;
                    case DoubleBroSevenSpecialType.BalaclavaRemoval:
                        StopUsingSpecialInternal();
                        break;
                }
                owner.SpecialAmmo--;
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override void AnimateSpecial()
        {
            bool isWalking = Y < owner.groundHeight + 0.5f && owner.yI < 10f;
            switch (currentSpecialType)
            {
                case DoubleBroSevenSpecialType.Martini:
                    hero.SetSpriteOffset(0f, 0f);
                    hero.DeactivateGun();
                    hero.FrameRate = frameRate;
                    int martiniCol = Mathf.Clamp(usingSpecialFrame, 0, 10);
                    hero.Sprite.SetLowerLeftPixel(martiniCol * hero.SpritePixelWidth, hero.SpritePixelHeight * martiniRow);
                    if (usingSpecialFrame < 10 && isWalking) owner.speed = 0f;
                    else owner.speed = originalSpeed;
                    if (usingSpecialFrame == 5)
                    {
                        hero.FrameRate = 0.25f;
                        Sound.GetInstance().PlaySoundEffectAt(attack2Sounds, 0.35f, owner.transform.position, 1f, true, false, false, 0f);
                        martinisDrunk++;
                    }
                    if (usingSpecialFrame == 10) UseSpecial();
                    if (usingSpecialFrame >= 11) StopUsingSpecialInternal();
                    usingSpecialFrame++;
                    break;

                case DoubleBroSevenSpecialType.Jetpack:
                    hero.SetSpriteOffset(0f, 0f);
                    hero.DeactivateGun();
                    hero.FrameRate = frameRate;
                    int jetCol = jetpackSpriteColumn + Mathf.Clamp(usingSpecialFrame, 0, 6);
                    hero.Sprite.SetLowerLeftPixel(jetCol * hero.SpritePixelWidth, hero.SpritePixelHeight * jetpackRow);
                    if (usingSpecialFrame < 5 && isWalking) owner.speed = 0f;
                    else owner.speed = originalSpeed;
                    if (usingSpecialFrame == 5 && !jetpackActivated) UseSpecial();
                    usingSpecialFrame++;
                    break;

                case DoubleBroSevenSpecialType.LaserWristWatch:
                    hero.SetSpriteOffset(0f, 0f);
                    hero.DeactivateGun();
                    hero.FrameRate = frameRate;
                    int laserCol = Mathf.Clamp(usingSpecialFrame, 0, 12);
                    hero.Sprite.SetLowerLeftPixel(laserCol * hero.SpritePixelWidth, hero.SpritePixelHeight * laserRow);
                    if (isWalking) owner.speed = 0f;
                    if (usingSpecialFrame == 9) UseSpecial();
                    usingSpecialFrame++;
                    break;

                case DoubleBroSevenSpecialType.Balaclava:
                    hero.SetSpriteOffset(0f, 0f);
                    hero.DeactivateGun();
                    hero.FrameRate = frameRate;
                    int balaCol = balaclavaColumn + Mathf.Clamp(usingSpecialFrame, 0, 5);
                    hero.Sprite.SetLowerLeftPixel(balaCol * hero.SpritePixelWidth, hero.SpritePixelHeight * balaclavaRow);
                    if (isWalking) owner.speed = 0f;
                    if (usingSpecialFrame == 5)
                    {
                        owner.speed = originalSpeed;
                        UseSpecial();
                        hero.UsingSpecial = false;
                        usingSpecialFrame = 0;
                    }
                    usingSpecialFrame++;
                    break;

                case DoubleBroSevenSpecialType.BalaclavaRemoval:
                    hero.SetSpriteOffset(0f, 0f);
                    hero.DeactivateGun();
                    hero.FrameRate = frameRate;
                    int removeCol = balaclavaColumn + Mathf.Clamp(5 - usingSpecialFrame, 0, 5);
                    hero.Sprite.SetLowerLeftPixel(removeCol * hero.SpritePixelWidth, hero.SpritePixelHeight * balaclavaRemoveRow);
                    if (isWalking) owner.speed = 0f;
                    if (usingSpecialFrame == 5) StopUsingSpecialInternal();
                    usingSpecialFrame++;
                    break;

                default:
                    base.AnimateSpecial();
                    break;
            }
        }

        private void StopUsingSpecialInternal()
        {
            owner.frame = 0;
            usingSpecialFrame = 0;
            hero.UsingSpecial = false;
            currentSpecialType = DoubleBroSevenSpecialType.None;
            hero.ActivateGun();
            hero.ChangeFrame();
            owner.speed = originalSpeed;
            colliderLaserDamage = 0;
            lastLaserCollider = null;
            balaclavaTime = 0f;
            jetpackActivated = false;
            if (owner.GetComponent<Renderer>().sharedMaterial != normalMaterial && normalMaterial != null)
            {
                owner.GetComponent<Renderer>().sharedMaterial = normalMaterial;
            }
        }

        public override void Update()
        {
            if (owner.health <= 0) return;
            RunSpecial();
        }

        private void RunSpecial()
        {
            if (currentSpecialType != DoubleBroSevenSpecialType.LaserWristWatch && laserWidth > 0f)
            {
                laserAudio.pitch = Mathf.Lerp(laserAudio.pitch, 0.3f, hero.DeltaTime * 8f);
                laserAudio.volume = laserWidth / 4f * laserVolume;
                laserWidth -= hero.DeltaTime * 24f;
                laserLine.startWidth = laserWidth;
                laserLine.endWidth = laserWidth;
                if (laserWidth <= 0f)
                {
                    laserLine.enabled = false;
                    laserAudio.Stop();
                }
            }
            if (currentSpecialType == DoubleBroSevenSpecialType.None) return;
            switch (currentSpecialType)
            {
                case DoubleBroSevenSpecialType.Balaclava:
                    balaclavaTime -= hero.DeltaTime;
                    if (balaclavaTime <= 0f)
                    {
                        hero.UsingSpecial = true;
                        currentSpecialType = DoubleBroSevenSpecialType.BalaclavaRemoval;
                        owner.frame = 0;
                        usingSpecialFrame = 0;
                        owner.counter = 0f;
                        hero.ChangeFrame();
                        Sound.GetInstance().PlaySoundEffectAt(attack4Sounds[1], 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                    }
                    break;
                case DoubleBroSevenSpecialType.LaserWristWatch:
                    if (owner.frame > 9)
                    {
                        if (!laserAudio.isPlaying) laserAudio.Play();
                        laserAudio.pitch = Mathf.Lerp(1f, 2f, laserAngleSpeed / startAngleSpeed);
                        laserAudio.volume = laserVolume;
                        laserAngleSpeed += laserAngleAcceleration * hero.DeltaTime;
                        if (laserAngle > endLaserAngle)
                        {
                            StepLaser(laserAngle - laserAngleSpeed * hero.DeltaTime);
                        }
                        if (laserAngle <= endLaserAngle)
                        {
                            DamageLastLaserCollider();
                            StopUsingSpecialInternal();
                        }
                        laserLine.enabled = true;
                        laserWidth = 4f;
                        laserLine.startWidth = laserWidth;
                        laserLine.endWidth = laserWidth;
                    }
                    break;
                case DoubleBroSevenSpecialType.Jetpack:
                    jetPackFuel -= hero.DeltaTime;
                    if (jetPackFuel <= 0f)
                    {
                        StopUsingSpecialInternal();
                    }
                    else if (owner.frame >= 5)
                    {
                        owner.yI += 200f * hero.DeltaTime;
                        owner.yI *= 1f - hero.DeltaTime * 3f;
                        EffectsController.CreatePlumeParticle(X - owner.transform.localScale.x * 5f, Y + 5f, 7f, 4f, 0f, -20f, 0.6f, 1.3f);
                        EffectsController.CreatePlumeParticle(X + owner.transform.localScale.x * 4f, Y + 5f, 7f, 4f, 0f, -20f, 0.6f, 1.3f);
                    }
                    break;
            }
        }

        private void StepLaser(float targetAngle)
        {
            if (targetAngle >= laserAngle) return;
            Vector3 origin = new Vector3(owner.transform.position.x + owner.transform.localScale.x * 4f, owner.transform.position.y + 11f, -4f);
            bool hitSomething = false;
            Vector3 hitPoint = lastLaserHitPos;
            while (laserAngle > targetAngle)
            {
                laserAngle -= laserStepM;
                Vector3 dir = global::Math.Point3OnCircle(laserAngle / 180f * Mathf.PI, 300f);
                dir.x = Mathf.Abs(dir.x) * owner.transform.localScale.x;
                float range = 350f;
                RaycastHit hit;
                if (Physics.Raycast(origin, dir, out hit, range, hero.GroundLayer))
                {
                    if (hit.distance < range) range = hit.distance;
                    if (range < 15f)
                    {
                        if (laserAngle > startLaserAngle - 45f)
                        {
                            laserAngle -= 4f;
                            targetAngle = Mathf.Clamp(targetAngle - 4f, -1000f, laserAngle - laserStepM);
                            continue;
                        }
                        lastLaserCollider = hit.collider;
                        colliderLaserDamage = 5;
                        DamageLastLaserCollider();
                        lastLaserCollider = null;
                    }
                }
                if (Physics.Raycast(origin, dir, out hit, range, unitsLayer) || Physics.Raycast(origin, dir, out hit, 350f, hero.GroundLayer))
                {
                    hitSomething = true;
                    hitPoint = hit.point;
                    if ((lastLaserHitPos - hit.point).sqrMagnitude > minSquareLaserDist)
                    {
                        lastLaserHitPos = hit.point;
                        EffectsController.CreateLaserParticle(lastLaserHitPos.x, lastLaserHitPos.y, hit.collider.gameObject);
                        if (Random.value > 0.5f)
                        {
                            EffectsController.CreateSparkShower(lastLaserHitPos.x + hit.normal.x * 2f, lastLaserHitPos.y + hit.normal.y * 2f,
                                1 + Random.Range(0, 3), 2f, 70f, hit.normal.x * 45f, hit.normal.y * 45f, 0.5f, 0f);
                        }
                        if (hit.collider != lastLaserCollider || colliderLaserDamage > 5)
                        {
                            DamageLastLaserCollider();
                            colliderLaserDamage = 0;
                            lastLaserCollider = hit.collider;
                        }
                        else
                        {
                            colliderLaserDamage += 2;
                        }
                    }
                }
                else
                {
                    DamageLastLaserCollider();
                    lastLaserCollider = null;
                    colliderLaserDamage = 0;
                    hitSomething = false;
                }
            }
            laserLine.SetPosition(0, origin);
            if (hitSomething)
            {
                laserLine.SetPosition(1, hitPoint);
            }
            else
            {
                Vector3 endDir = global::Math.Point3OnCircle(laserAngle / 180f * Mathf.PI, 300f);
                endDir.x = Mathf.Abs(endDir.x) * owner.transform.localScale.x;
                laserLine.SetPosition(1, origin + endDir);
            }
        }

        private void DamageLastLaserCollider()
        {
            if (lastLaserCollider != null && owner.IsMine && SortOfFollow.IsItSortOfVisible(lastLaserCollider.transform.position, 48f, 48f))
            {
                Unit unit = lastLaserCollider.GetComponent<Unit>();
                if (unit != null)
                {
                    unit.Damage(colliderLaserDamage, DamageType.Fire, 0f, 0f, (int)owner.transform.localScale.x, owner, lastLaserHitPos.x, lastLaserHitPos.y);
                }
                else
                {
                    lastLaserCollider.SendMessage("Damage", new DamageObject(colliderLaserDamage, DamageType.Bullet, 0f, 0f, lastLaserHitPos.x, lastLaserHitPos.y, owner));
                }
            }
        }

        public override bool HandleApplyFallingGravity()
        {
            if (hero.UsingSpecial && currentSpecialType == DoubleBroSevenSpecialType.Jetpack && owner.frame >= 5)
            {
                return false;
            }
            return true;
        }

        public override void HandleAfterCheckInput()
        {
            if (hero.UsingSpecial && currentSpecialType == DoubleBroSevenSpecialType.LaserWristWatch)
            {
                owner.left = false;
                owner.right = false;
                owner.up = false;
                owner.down = false;
                owner.fire = false;
                owner.buttonJump = false;
            }
        }

        public override bool HandleIsInStealthMode()
        {
            if (balaclavaTime > 0f)
            {
                return false;
            }
            return true;
        }

        public override bool HandleAlertNearbyMooks()
        {
            return balaclavaTime <= 0f;
        }

        public override bool HandleDeath()
        {
            if (laserLine != null) laserLine.enabled = false;
            if (laserAudio != null && laserAudio.isPlaying) laserAudio.Stop();
            return true;
        }

        public override bool HandleGib(DamageType damageType, float xI, float yI)
        {
            if (laserLine != null) laserLine.enabled = false;
            return true;
        }

        public override void Cleanup()
        {
            if (!(owner is DoubleBroSeven))
            {
                if (laserLine != null) Object.Destroy(laserLine);
                if (laserAudio != null) Object.Destroy(laserAudio);
            }
        }
    }
}

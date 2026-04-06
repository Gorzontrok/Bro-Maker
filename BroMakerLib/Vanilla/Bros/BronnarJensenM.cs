// Auto-generated from RambroM.cs — do not edit manually
using System;
using System.Collections.Generic;
using System.Reflection;
using BroMakerLib.Abilities;
using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using BroMakerLib.Infos;
using RocketLib;
using BroMakerLib.Loggers;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BronnarJensen", HeroType.BronnarJensen)]
    public class BronnarJensenM : BronnarJensen, ICustomHero
    {
        [Syncronize] public CustomBroInfo Info { get; set; }
        [Syncronize] public BroBase Character { get; set; }
        [JsonIgnore] public MuscleTempleFlexEffect FlexEffect { get; set; }
        [JsonIgnore] public int CurrentVariant { get; set; }
        [JsonIgnore] public Vector2 CurrentGunSpriteOffset { get; set; }
        [JsonIgnore] public List<Material> CurrentSpecialMaterials { get; set; }
        [JsonIgnore] public Vector2 CurrentSpecialMaterialOffset { get; set; }
        [JsonIgnore] public float CurrentSpecialMaterialSpacing { get; set; }
        [JsonIgnore] public Material CurrentFirstAvatar { get; set; }

        [JsonIgnore] public SpecialAbility specialAbility;
        [JsonIgnore] public MeleeAbility meleeAbility;

        protected bool blockGesturesDuringMelee = true;

        #region BroBase Methods

        protected override void Awake()
        {
            try
            {
                this.StandardBeforeAwake();

                specialAbility = AbilityFactory.CreateSpecial(Info.special, this);
                meleeAbility = AbilityFactory.CreateMelee(Info.melee, this);
                if (meleeAbility != null)
                {
                    meleeType = meleeAbility.meleeType;
                }

                base.Awake();
                this.StandardAfterAwake();
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
                enabled = false;
            }
        }

        protected override void Start()
        {
            try
            {
                this.StandardBeforeStart();
                base.Start();
                this.StandardAfterStart();
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
                enabled = false;
            }
        }

        protected override void Update()
        {
            if (specialAbility != null && !specialAbility.HandleUpdate())
            {
                specialAbility.Update();
                meleeAbility?.Update();
                return;
            }
            base.Update();
            specialAbility?.Update();
            meleeAbility?.Update();
        }

        void ICustomHero.PrefabSetup()
        {
            FixNullVariableLocal();
        }

        protected void CopySerializedValues(TestVanDammeAnim prefab)
        {
            Type type = prefab.GetType();
            while (type != null && type != typeof(MonoBehaviour))
            {
                foreach (FieldInfo field in type.GetFields(
                             BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType.IsValueType || field.FieldType == typeof(string))
                    {
                        field.SetValue(this, field.GetValue(prefab));
                    }
                }

                type = type.BaseType;
            }
        }

        protected virtual void FixNullVariableLocal()
        {
            var bro = HeroController.GetHeroPrefab(HeroType.BronnarJensen).As<BronnarJensen>();
            if (bro == null) return;
            CopySerializedValues(bro);
            primaryGrenade = bro.primaryGrenade;
            remoteControlVehiclePrefab = bro.remoteControlVehiclePrefab;
        }

        public override void SetGestureAnimation(GestureElement.Gestures gesture)
        {
            if (blockGesturesDuringMelee && doingMelee)
            {
                return;
            }

            base.SetGestureAnimation(gesture);
        }

        protected override void SetGunPosition(float xOffset, float yOffset)
        {
            gunSprite.transform.localPosition = new Vector3(xOffset + CurrentGunSpriteOffset.x, yOffset + CurrentGunSpriteOffset.y, -.001f);
        }

        protected override void CheckForTraps(ref float yIT)
        {
            if (specialAbility != null && !specialAbility.HandleCheckForTraps()) return;

            var num = Y + yIT;
            if (num <= groundHeight + 1f)
            {
                num = groundHeight + 1f;
            }

            if (Map.isEditing || invulnerable)
            {
                return;
            }

            if (!IsEnemy && !IsMine)
            {
                return;
            }

            var nearestAcid = Map.GetNearestAcid(X, Y + 8f, 2f);
            if (nearestAcid != null && nearestAcid.fullness > 0.2f)
            {
                CoverInAcid();
            }

            if (impaledByTransform == null && IsHero && ((yI > 50f && (canTouchRightWalls || canTouchLeftWalls || WallClimbing) && (Time.time - lastJumpTime > 0.2f || Y > groundHeight + 17f)) || yI < -120f) && IsSurroundedByBarbedWire())
            {
                EffectsController.CreateBloodParticles(bloodColor, X, Y + 10f, -5f, 1, 4f, 4f, 50f, xI * 0.8f, 70f);
                if (yI < 0f)
                {
                    yI *= 0.2f;
                }
                else
                {
                    yI *= 0.45f;
                }

                barbedWireWithin.ForceBloody();
                barbedWireWithin.PlayCutSound();
            }

            RaycastHit raycastHit;
            if (impaledByTransform == null && Physics.Raycast(new Vector3(X, num, 0f), Vector3.down, out raycastHit, 25f, groundLayer))
            {
                var component = raycastHit.collider.GetComponent<Block>();
                if (component != null)
                {
                    if (raycastHit.distance < 10f && (IsMine || IsEnemy))
                    {
                        component.CheckForMine();
                    }

                    if (component.spikes != null && CanBeImpaledByGroundSpikes())
                    {
                        if (component.spikes.EvaluateImpalent(this))
                        {
                            var num2 = playerNum >= 0 ? component.spikes.spikeTrapHarmlessHeight : component.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit.point.y > groundHeight - 1f && raycastHit.distance >= num2 && raycastHit.distance < component.spikes.spikeTrapHeight && Y > component.Y + component.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
                                component.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component.spikes.IsBarbedWire(this) && component.spikes.collumn == component.collumn && Y < raycastHit.point.y + 12f && Mathf.Abs(yI) < 50f && Mathf.Abs(xI + xIBlast) > GetSpeed - 2f && (int)Mathf.Sign(xI + xIBlast) == (!left ? 0 : -1) + (!right ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(bloodColor, X, Y + 10f, -5f, 1, 4f, 4f, 50f, xI * 0.8f, 70f);
                            xIBlast -= xI * 0.4f;
                            xI = 0f;
                            component.spikes.ForceBloody();
                            component.spikes.PlayCutSound();
                        }
                    }
                }
            }

            RaycastHit raycastHit2;
            if (impaledByTransform == null && Physics.Raycast(new Vector3(X - 3f, num, 0f), Vector3.down, out raycastHit2, 25f, groundLayer))
            {
                var component2 = raycastHit2.collider.GetComponent<Block>();
                if (component2 != null)
                {
                    if (raycastHit2.distance < 10f && (IsMine || IsEnemy))
                    {
                        component2.CheckForMine();
                    }

                    if (component2.spikes != null && CanBeImpaledByGroundSpikes())
                    {
                        if (component2.spikes.EvaluateImpalent(this))
                        {
                            var num3 = playerNum >= 0 ? component2.spikes.spikeTrapHarmlessHeight : component2.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit2.point.y > groundHeight - 1f && raycastHit2.distance >= num3 && raycastHit2.distance < component2.spikes.spikeTrapHeight && Y > component2.Y + component2.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
                                component2.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component2.spikes.IsBarbedWire(this) && component2.spikes.collumn == component2.collumn && Y < raycastHit2.point.y + 12f && Mathf.Abs(yI) < 50f && Mathf.Abs(xI + xIBlast) > GetSpeed - 2f && (int)Mathf.Sign(xI + xIBlast) == (!left ? 0 : -1) + (!right ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(bloodColor, X, Y + 10f, -5f, 1, 4f, 4f, 50f, xI * 0.8f, 70f);
                            xIBlast -= xI * 0.4f;
                            xI = 0f;
                            component2.spikes.ForceBloody();
                            component2.spikes.PlayCutSound();
                        }
                    }
                }
            }

            RaycastHit raycastHit3;
            if (impaledByTransform == null && Physics.Raycast(new Vector3(X + 3f, num, 0f), Vector3.down, out raycastHit3, 25f, groundLayer))
            {
                var component3 = raycastHit3.collider.GetComponent<Block>();
                if (component3 != null)
                {
                    if (raycastHit3.distance < 10f && (IsMine || IsEnemy))
                    {
                        component3.CheckForMine();
                    }

                    if (component3.spikes != null && CanBeImpaledByGroundSpikes())
                    {
                        if (component3.spikes.EvaluateImpalent(this))
                        {
                            var num4 = playerNum >= 0 ? component3.spikes.spikeTrapHarmlessHeight : component3.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit3.point.y > groundHeight - 1f && raycastHit3.distance >= num4 && raycastHit3.distance < component3.spikes.spikeTrapHeight && Y > component3.Y + component3.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
                                component3.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component3.spikes.IsBarbedWire(this) && component3.spikes.collumn == component3.collumn && Y < raycastHit3.point.y + 12f && Mathf.Abs(yI) < 50f && Mathf.Abs(xI + xIBlast) > GetSpeed - 2f && (int)Mathf.Sign(xI + xIBlast) == (!left ? 0 : -1) + (!right ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(bloodColor, X, Y + 10f, -5f, 1, 4f, 4f, 50f, xI * 0.8f, 70f);
                            xIBlast -= xI * 0.4f;
                            xI = 0f;
                            component3.spikes.ForceBloody();
                            component3.spikes.PlayCutSound();
                        }
                    }
                }
            }
        }

        protected override void TriggerFlexEvent()
        {
            if (player.HasFlexPower(PickupType.FlexAlluring))
            {
                Map.AttractMooks(X, Y, 96f, 30f);
            }

            if (player.HasFlexPower(PickupType.FlexGoldenLight))
            {
                if (FlexEffect == null)
                {
                    FlexEffect = Traverse.Create(this as BroBase).GetFieldValue("flexEffect") as MuscleTempleFlexEffect;
                }

                if (FlexEffect != null)
                {
                    FlexEffect.PlaySoundEffect();
                }

                if (IsMine)
                {
                    var num = 8 + Random.Range(0, 5);
                    for (var i = 0; i < num; i++)
                    {
                        var angle = -1.88495576f + 1.2f / (float)(num - 1) * 3.14159274f * (float)i;
                        var vector = Math.Point2OnCircle(angle, 1f);
                        ProjectileController.SpawnProjectileLocally(ProjectileController.instance.goldenLightProjectile, this, X, Y + 12f, vector.x * 400f, vector.y * 400f, true, 15, false, true, -15f);
                    }
                }
            }
            else if (player.HasFlexPower(PickupType.FlexInvulnerability) && FlexEffect != null)
            {
                FlexEffect.PlaySoundEffect();
            }
        }

        #endregion

        #region ICustomHero Ability Accessors

        SpecialAbility ICustomHero.SpecialAbility => specialAbility;
        MeleeAbility ICustomHero.MeleeAbility => meleeAbility;

        // Shared field accessors
        SpriteSM ICustomHero.Sprite => sprite;
        int ICustomHero.SpritePixelWidth => spritePixelWidth;
        int ICustomHero.SpritePixelHeight => spritePixelHeight;
        bool ICustomHero.Ducking => ducking;
        float ICustomHero.DeltaTime => t;
        Sound ICustomHero.Sound => sound;
        LayerMask ICustomHero.GroundLayer => groundLayer;
        bool ICustomHero.WallDrag => wallDrag;

        float ICustomHero.FrameRate
        {
            get => frameRate;
            set => frameRate = value;
        }

        int ICustomHero.GunFrame
        {
            get => gunFrame;
            set => gunFrame = value;
        }

        float ICustomHero.InvulnerableTime
        {
            get => invulnerableTime;
            set => invulnerableTime = value;
        }

        float ICustomHero.JumpTime
        {
            set => jumpTime = value;
        }

        // Special ability state
        bool ICustomHero.UsingSpecial
        {
            get => usingSpecial;
            set => usingSpecial = value;
        }

        bool ICustomHero.UsingPockettedSpecial
        {
            get => usingPockettedSpecial;
            set => usingPockettedSpecial = value;
        }

        int ICustomHero.PressSpecialFacingDirection
        {
            get => pressSpecialFacingDirection;
            set => pressSpecialFacingDirection = value;
        }

        // Melee ability state
        bool ICustomHero.DoingMelee
        {
            get => doingMelee;
            set => doingMelee = value;
        }

        bool ICustomHero.MeleeHasHit
        {
            get => meleeHasHit;
            set => meleeHasHit = value;
        }

        bool ICustomHero.MeleeFollowUp
        {
            get => meleeFollowUp;
            set => meleeFollowUp = value;
        }

        bool ICustomHero.StandingMelee => standingMelee;
        bool ICustomHero.JumpingMelee => jumpingMelee;
        bool ICustomHero.DashingMelee => dashingMelee;

        Unit ICustomHero.MeleeChosenUnit
        {
            get => meleeChosenUnit;
            set => meleeChosenUnit = value;
        }

        // Shared method accessors
        void ICustomHero.SetSpriteOffset(float x, float y) => SetSpriteOffset(x, y);
        void ICustomHero.DeactivateGun() => DeactivateGun();
        void ICustomHero.ActivateGun() => ActivateGun();
        void ICustomHero.ChangeFrame() => ChangeFrame();
        void ICustomHero.SetGunSprite(int spriteFrame, int spriteRow) => SetGunSprite(spriteFrame, spriteRow);
        void ICustomHero.CreateFaderTrailInstance() => CreateFaderTrailInstance();
        void ICustomHero.SetInvulnerable(float time, bool dvOverride, bool dvNetwork) => SetInvulnerable(time, dvOverride, dvNetwork);

        // Special ability methods
        void ICustomHero.TriggerBroSpecialEvent() => TriggerBroSpecialEvent();
        void ICustomHero.PlayAttackSound() => PlayAttackSound();
        void ICustomHero.PlayAttackSound(float v) => PlayAttackSound(v);

        // Melee ability methods
        void ICustomHero.AnimateMeleeCommon() => AnimateMeleeCommon();
        void ICustomHero.CancelMelee() => CancelMelee();
        void ICustomHero.SetMeleeType() => SetMeleeType();
        bool ICustomHero.TryMeleeTerrain(int offset, int damage) => TryMeleeTerrain(offset, damage);
        void ICustomHero.KickDoors(float range) => KickDoors(range);
        void ICustomHero.TriggerBroMeleeEvent() => TriggerBroMeleeEvent();
        void ICustomHero.ResetMeleeValues() => ResetMeleeValues();
        void ICustomHero.StartMeleeCommon() => StartMeleeCommon();

        #endregion

        #region Ability Forwarding

        protected override void PressSpecial()
        {
            if (specialAbility != null)
            {
                specialAbility.PressSpecial();
                return;
            }
            base.PressSpecial();
        }

        protected override void AnimateSpecial()
        {
            if (specialAbility != null)
            {
                specialAbility.AnimateSpecial();
                return;
            }
            base.AnimateSpecial();
        }

        protected override void UseSpecial()
        {
            if (specialAbility != null)
            {
                specialAbility.UseSpecial();
                return;
            }
            base.UseSpecial();
        }

        protected override void ReleaseSpecial()
        {
            if (specialAbility != null && !specialAbility.HandleReleaseSpecial())
            {
                return;
            }
            base.ReleaseSpecial();
        }

        protected override bool MustIgnoreHighFiveMeleePress()
        {
            specialAbility?.HandleMustIgnoreHighFiveMeleePress();
            return base.MustIgnoreHighFiveMeleePress();
        }

        protected override void CalculateMovement()
        {
            if (specialAbility != null)
            {
                float xI = this.xI;
                float yI = this.yI;
                if (!specialAbility.HandleCalculateMovement(ref xI, ref yI))
                {
                    this.xI = xI;
                    this.yI = yI;
                    return;
                }
                this.xI = xI;
                this.yI = yI;
            }
            base.CalculateMovement();
            specialAbility?.HandleAfterCalculateMovement();
        }

        public override void Damage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if (specialAbility != null && !specialAbility.HandleDamage(damage, damageType, xI, yI, direction, damageSender, hitX, hitY))
            {
                return;
            }
            if (meleeAbility != null && !meleeAbility.HandleDamage(damage, damageType, xI, yI, direction, damageSender, hitX, hitY))
            {
                return;
            }
            base.Damage(damage, damageType, xI, yI, direction, damageSender, hitX, hitY);
        }

        public override void Death(float xI, float yI, DamageObject damage)
        {
            if (specialAbility != null && !specialAbility.HandleDeath())
            {
                return;
            }
            if (meleeAbility != null && !meleeAbility.HandleDeath())
            {
                return;
            }
            base.Death(xI, yI, damage);
            specialAbility?.HandleAfterDeath();
            meleeAbility?.HandleAfterDeath();
        }

        protected override bool CanReduceLives()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleCanReduceLives(ref result))
                {
                    return result;
                }
            }
            return base.CanReduceLives();
        }

        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if (specialAbility != null && !specialAbility.HandleFireWeapon())
            {
                return;
            }
            base.FireWeapon(x, y, xSpeed, ySpeed);
            specialAbility?.HandleAfterFireWeapon(x, y, xSpeed, ySpeed);
        }

        protected override void Jump(bool wallJump)
        {
            if (specialAbility != null && !specialAbility.HandleJump(wallJump))
            {
                return;
            }
            base.Jump(wallJump);
        }

        protected override void RunMovement()
        {
            if (specialAbility != null && !specialAbility.HandleRunMovement())
            {
                return;
            }
            base.RunMovement();
        }

        protected override void ApplyNormalGravity()
        {
            if (specialAbility != null && !specialAbility.HandleApplyNormalGravity())
            {
                return;
            }
            base.ApplyNormalGravity();
        }

        protected override void StartFiring()
        {
            if (specialAbility != null && !specialAbility.HandleStartFiring())
            {
                return;
            }
            if (meleeAbility != null && !meleeAbility.HandleStartFiring())
            {
                return;
            }
            base.StartFiring();
        }

        protected override void StartMelee()
        {
            if (specialAbility != null && !specialAbility.HandleStartMelee())
            {
                return;
            }
            base.StartMelee();
        }

        protected override void RunIndependentMeleeFrames()
        {
            if (meleeAbility != null)
            {
                return;
            }
            base.RunIndependentMeleeFrames();
        }

        protected override void StartKnifeMelee()
        {
            if (meleeAbility != null) { meleeAbility.StartMelee(); return; }
            base.StartKnifeMelee();
        }

        protected override void AnimateKnifeMelee()
        {
            if (meleeAbility != null) { meleeAbility.AnimateMelee(); return; }
            base.AnimateKnifeMelee();
        }

        protected override void RunKnifeMeleeMovement()
        {
            if (meleeAbility != null) { meleeAbility.RunMeleeMovement(); return; }
            base.RunKnifeMeleeMovement();
        }

        protected override void StartPunch()
        {
            if (meleeAbility != null) { meleeAbility.StartMelee(); return; }
            base.StartPunch();
        }

        protected override void AnimatePunch()
        {
            if (meleeAbility != null) { meleeAbility.AnimateMelee(); return; }
            base.AnimatePunch();
        }

        protected override void RunPunchMovement()
        {
            if (meleeAbility != null) { meleeAbility.RunMeleeMovement(); return; }
            base.RunPunchMovement();
        }

        protected override void RunJetPackPunchMovement()
        {
            if (meleeAbility != null) { meleeAbility.RunMeleeMovement(); return; }
            base.RunJetPackPunchMovement();
        }

        protected override void StartCustomMelee()
        {
            if (meleeAbility != null) { meleeAbility.StartMelee(); return; }
            base.StartCustomMelee();
        }

        protected override void AnimateCustomMelee()
        {
            if (meleeAbility != null) { meleeAbility.AnimateMelee(); return; }
            base.AnimateCustomMelee();
        }

        protected override void RunCustomMeleeMovement()
        {
            if (meleeAbility != null) { meleeAbility.RunMeleeMovement(); return; }
            base.RunCustomMeleeMovement();
        }

        protected override void CancelMelee()
        {
            if (meleeAbility != null)
            {
                meleeAbility.CancelMelee();
            }
            base.CancelMelee();
        }

        /// <summary>
        /// Assigns a special ability at runtime. Calls <see cref="SpecialAbility.Initialize"/> on the new ability.
        /// </summary>
        /// <param name="ability">The ability to assign, or null to clear.</param>
        public void SetSpecialAbility(SpecialAbility ability)
        {
            specialAbility?.Cleanup();
            specialAbility = ability;
            ability?.Initialize(this);
        }

        /// <summary>
        /// Assigns a melee ability at runtime. Calls <see cref="MeleeAbility.Initialize"/> on the new ability
        /// and sets <c>meleeType</c> to the ability's declared <see cref="MeleeAbility.meleeType"/>.
        /// </summary>
        /// <param name="ability">The ability to assign, or null to clear.</param>
        public void SetMeleeAbility(MeleeAbility ability)
        {
            meleeAbility?.Cleanup();
            meleeAbility = ability;
            ability?.Initialize(this);
            if (ability != null)
            {
                meleeType = ability.meleeType;
            }
        }

        protected override void ChangeFrame()
        {
            if (specialAbility != null && !specialAbility.HandleChangeFrame())
            {
                return;
            }
            base.ChangeFrame();
            specialAbility?.HandleAfterChangeFrame();
        }

        protected override void IncreaseFrame()
        {
            base.IncreaseFrame();
            specialAbility?.HandleAfterIncreaseFrame();
        }

        protected override void RunGun()
        {
            if (specialAbility != null && !specialAbility.HandleRunGun())
            {
                return;
            }
            base.RunGun();
        }

        protected override void RunFiring()
        {
            if (specialAbility != null && !specialAbility.HandleRunFiring())
            {
                return;
            }
            base.RunFiring();
        }

        protected override void Land()
        {
            if (specialAbility != null && !specialAbility.HandleLand())
            {
                return;
            }
            if (meleeAbility != null && !meleeAbility.HandleLand())
            {
                return;
            }
            base.Land();
            specialAbility?.HandleAfterLand();
            meleeAbility?.HandleAfterLand();
        }

        protected override void RunAvatarFiring()
        {
            if (specialAbility != null && !specialAbility.HandleRunAvatarFiring())
            {
                return;
            }
            base.RunAvatarFiring();
        }

        protected override bool IsOverLadder(float xOffset, ref float ladderXPos)
        {
            if (specialAbility != null && !specialAbility.HandleIsOverLadder())
            {
                return false;
            }
            return base.IsOverLadder(xOffset, ref ladderXPos);
        }

        protected override bool WallDrag
        {
            get => base.WallDrag;
            set
            {
                if (specialAbility != null && !specialAbility.HandleWallDrag(value))
                {
                    return;
                }
                if (meleeAbility != null && !meleeAbility.HandleWallDrag(value))
                {
                    return;
                }
                base.WallDrag = value;
            }
        }

        protected override void AnimateActualJumpingFrames()
        {
            if (specialAbility != null && !specialAbility.HandleAnimateActualJumpingFrames())
            {
                return;
            }
            base.AnimateActualJumpingFrames();
        }

        protected override void AnimateActualNewRunningFrames()
        {
            base.AnimateActualNewRunningFrames();
            specialAbility?.HandleAfterAnimateNewRunningFrames();
        }

        protected override bool ConstrainToFloor(ref float yIT)
        {
            if (specialAbility != null && !specialAbility.HandleConstrainToFloor())
            {
                return false;
            }
            return base.ConstrainToFloor(ref yIT);
        }

        protected override bool ConstrainToCeiling(ref float yIT)
        {
            if (specialAbility != null && !specialAbility.HandleConstrainToCeiling())
            {
                return false;
            }
            return base.ConstrainToCeiling(ref yIT);
        }

        protected override bool ConstrainToWalls(ref float yIT, ref float xIT)
        {
            if (specialAbility != null && !specialAbility.HandleConstrainToWalls())
            {
                return false;
            }
            return base.ConstrainToWalls(ref yIT, ref xIT);
        }

        public override Vector3 GetFollowPosition()
        {
            if (specialAbility != null)
            {
                Vector3 result = Vector3.zero;
                if (!specialAbility.HandleGetFollowPosition(ref result))
                {
                    return result;
                }
            }
            return base.GetFollowPosition();
        }

        public override bool IsInStealthMode()
        {
            if (specialAbility != null && !specialAbility.HandleIsInStealthMode())
            {
                return true;
            }
            if (meleeAbility != null && !meleeAbility.HandleIsInStealthMode())
            {
                return true;
            }
            return base.IsInStealthMode();
        }

        protected override void AlertNearbyMooks()
        {
            if (specialAbility != null && !specialAbility.HandleAlertNearbyMooks())
            {
                return;
            }
            if (meleeAbility != null && !meleeAbility.HandleAlertNearbyMooks())
            {
                return;
            }
            base.AlertNearbyMooks();
        }

        protected override void Gib(DamageType damageType, float xI, float yI)
        {
            if (specialAbility != null && !specialAbility.HandleGib(damageType, xI, yI))
            {
                return;
            }
            base.Gib(damageType, xI, yI);
        }

        public override void RecallBro()
        {
            if (specialAbility != null && !specialAbility.HandleRecallBro())
            {
                return;
            }
            base.RecallBro();
            specialAbility?.HandleAfterRecallBro();
        }

        public override void AttachToHeli()
        {
            if (specialAbility != null && !specialAbility.HandleAttachToHeli())
            {
                return;
            }
            base.AttachToHeli();
        }

        protected override void HitCeiling(RaycastHit ceilingHit)
        {
            if (specialAbility != null && !specialAbility.HandleHitCeiling())
            {
                return;
            }
            if (meleeAbility != null && !meleeAbility.HandleHitCeiling())
            {
                return;
            }
            base.HitCeiling(ceilingHit);
            specialAbility?.HandleAfterHitCeiling();
            meleeAbility?.HandleAfterHitCeiling();
        }

        protected override void HitLeftWall()
        {
            if (specialAbility != null && !specialAbility.HandleHitLeftWall())
            {
                return;
            }
            base.HitLeftWall();
            specialAbility?.HandleAfterHitLeftWall();
        }

        protected override void HitRightWall()
        {
            if (specialAbility != null && !specialAbility.HandleHitRightWall())
            {
                return;
            }
            base.HitRightWall();
            specialAbility?.HandleAfterHitRightWall();
        }

        protected override void ClampWallDragYI(ref float yIT)
        {
            if (specialAbility != null && !specialAbility.HandleClampWallDragYI(ref yIT))
            {
                return;
            }
            base.ClampWallDragYI(ref yIT);
        }

        protected override void RunHanging()
        {
            if (specialAbility != null && !specialAbility.HandleRunHanging())
            {
                return;
            }
            base.RunHanging();
        }

        protected override bool CanCheckClimbAlongCeiling()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleCanCheckClimbAlongCeiling(ref result))
                {
                    return result;
                }
            }
            return base.CanCheckClimbAlongCeiling();
        }

        protected override void CheckClimbAlongCeiling()
        {
            if (specialAbility != null && !specialAbility.HandleCheckClimbAlongCeiling())
            {
                return;
            }
            base.CheckClimbAlongCeiling();
        }

        protected override void CheckInput()
        {
            base.CheckInput();
            specialAbility?.HandleAfterCheckInput();
        }

        protected override void AirDashDown()
        {
            if (specialAbility != null && !specialAbility.HandleAirDashDown())
            {
                return;
            }
            base.AirDashDown();
        }

        protected override void RunDownwardDash()
        {
            if (specialAbility != null && !specialAbility.HandleRunDownwardDash())
            {
                return;
            }
            base.RunDownwardDash();
            specialAbility?.HandleAfterRunDownwardDash();
        }

        public override bool IsAlive()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleIsAlive(ref result))
                {
                    return result;
                }
            }
            return base.IsAlive();
        }

        public override bool Revive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
            if (specialAbility != null && !specialAbility.HandleRevive(playerNum, isUnderPlayerControl, reviveSource))
            {
                return false;
            }
            bool result = base.Revive(playerNum, isUnderPlayerControl, reviveSource);
            specialAbility?.HandleAfterRevive(playerNum, isUnderPlayerControl, reviveSource);
            return result;
        }

        public override void UseSteroids()
        {
            if (specialAbility != null && !specialAbility.HandleUseSteroids())
            {
                return;
            }
            base.UseSteroids();
            specialAbility?.HandleAfterUseSteroids();
        }

        protected override void CheckNotifyDeathType()
        {
            if (specialAbility != null && !specialAbility.HandleCheckNotifyDeathType())
            {
                return;
            }
            base.CheckNotifyDeathType();
        }

        protected override void ApplyFallingGravity()
        {
            if (specialAbility != null && !specialAbility.HandleApplyFallingGravity())
            {
                return;
            }
            if (meleeAbility != null && !meleeAbility.HandleApplyFallingGravity())
            {
                return;
            }
            base.ApplyFallingGravity();
        }

        protected override void SetDeltaTime()
        {
            if (specialAbility != null && !specialAbility.HandleSetDeltaTime())
            {
                return;
            }
            base.SetDeltaTime();
        }

        public override bool CanInseminate(float xI, float yI)
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleCanInseminate(ref result))
                {
                    return result;
                }
            }
            if (meleeAbility != null)
            {
                bool result = false;
                if (!meleeAbility.HandleCanInseminate(ref result))
                {
                    return result;
                }
            }
            return base.CanInseminate(xI, yI);
        }

        public override void StartPilotingUnit(Unit pilottedUnit)
        {
            if (specialAbility != null && !specialAbility.HandleStartPilotingUnit())
            {
                return;
            }
            base.StartPilotingUnit(pilottedUnit);
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            specialAbility?.HandleLateUpdate();
        }

        protected override void AddSpeedLeft()
        {
            base.AddSpeedLeft();
            specialAbility?.HandleAfterAddSpeedLeft();
        }

        protected override void AddSpeedRight()
        {
            base.AddSpeedRight();
            specialAbility?.HandleAfterAddSpeedRight();
        }

        protected virtual bool CanBeImpaledByGroundSpikes()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleCanBeImpaledByGroundSpikes(ref result))
                {
                    return result;
                }
            }
            return !invulnerable && !wallDrag;
        }

        #endregion
    }
}

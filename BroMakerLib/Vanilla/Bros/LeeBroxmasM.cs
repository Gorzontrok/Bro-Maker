// Auto-generated from RambroM.cs — do not edit manually
using System;
using System.Collections.Generic;
using System.Reflection;
using BroMakerLib.Abilities;
using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using HarmonyLib;
using Newtonsoft.Json;
using RocketLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("LeeBroxmas", HeroType.LeeBroxmas)]
    public class LeeBroxmasM : LeeBroxmas, ICustomHero, IAbilityOwner
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

        #region ICustomHero Setup
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
            var bro = HeroController.GetHeroPrefab(HeroType.LeeBroxmas).As<LeeBroxmas>();
            if (bro == null) return;
            CopySerializedValues(bro);
            if (faderSpritePrefab == null) faderSpritePrefab = bro.faderSpritePrefab;
            if (disarmedGunMaterial == null) disarmedGunMaterial = bro.disarmedGunMaterial;
            if (parachute == null) parachute = bro.parachute;
            macheteSprayProjectile = bro.macheteSprayProjectile;
        }
        #endregion

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
            bool runBase = true;
            if (specialAbility != null && !specialAbility.HandleUpdate()) runBase = false;
            if (meleeAbility != null && !meleeAbility.HandleUpdate()) runBase = false;
            if (runBase) base.Update();
            specialAbility?.Update();
            meleeAbility?.Update();
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
            base.SetGunPosition(xOffset + CurrentGunSpriteOffset.x, yOffset + CurrentGunSpriteOffset.y);
        }

        protected override void CheckForTraps(ref float yIT)
        {
            if (specialAbility != null && !specialAbility.HandleCheckForTraps()) return;
            if (meleeAbility != null && !meleeAbility.HandleCheckForTraps()) return;

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
                        var angle = -1.88495576f + (1.2f / (float)(num - 1) * 3.14159274f * (float)i);
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

        #region IAbilityOwner
        SpecialAbility IAbilityOwner.SpecialAbility => specialAbility;
        MeleeAbility IAbilityOwner.MeleeAbility => meleeAbility;

        SpriteSM IAbilityOwner.Sprite => sprite;
        int IAbilityOwner.SpritePixelWidth => spritePixelWidth;
        int IAbilityOwner.SpritePixelHeight => spritePixelHeight;
        bool IAbilityOwner.Ducking => ducking;
        float IAbilityOwner.DeltaTime => t;
        Sound IAbilityOwner.Sound => sound;
        LayerMask IAbilityOwner.GroundLayer => groundLayer;
        bool IAbilityOwner.WallDrag => wallDrag;
        bool IAbilityOwner.IsInQuicksand => isInQuicksand;
        float IAbilityOwner.HalfWidth => halfWidth;
        float IAbilityOwner.CeilingHeight => ceilingHeight;
        LayerMask IAbilityOwner.FragileLayer => fragileLayer;
        bool IAbilityOwner.HighFive => highFive;

        float IAbilityOwner.FrameRate { get => frameRate; set => frameRate = value; }
        int IAbilityOwner.GunFrame { get => gunFrame; set => gunFrame = value; }
        float IAbilityOwner.InvulnerableTime { get => invulnerableTime; set => invulnerableTime = value; }
        float IAbilityOwner.JumpTime { set => jumpTime = value; }
        float IAbilityOwner.DeathTime { get => deathTime; set => deathTime = value; }
        DeathType IAbilityOwner.CurrentDeathType { get => deathType; set => deathType = value; }
        Mook IAbilityOwner.NearbyMook { get => nearbyMook; set => nearbyMook = value; }
        bool IAbilityOwner.HasPlayedMissSound { get => hasPlayedMissSound; set => hasPlayedMissSound = value; }

        bool IAbilityOwner.UsingSpecial { get => usingSpecial; set => usingSpecial = value; }
        bool IAbilityOwner.UsingPockettedSpecial { get => usingPockettedSpecial; set => usingPockettedSpecial = value; }
        int IAbilityOwner.PressSpecialFacingDirection { get => pressSpecialFacingDirection; set => pressSpecialFacingDirection = value; }

        bool IAbilityOwner.DoingMelee { get => doingMelee; set => doingMelee = value; }
        bool IAbilityOwner.MeleeHasHit { get => meleeHasHit; set => meleeHasHit = value; }
        bool IAbilityOwner.MeleeFollowUp { get => meleeFollowUp; set => meleeFollowUp = value; }
        bool IAbilityOwner.StandingMelee => standingMelee;
        bool IAbilityOwner.JumpingMelee { get => jumpingMelee; set => jumpingMelee = value; }
        bool IAbilityOwner.DashingMelee { get => dashingMelee; set => dashingMelee = value; }
        Unit IAbilityOwner.MeleeChosenUnit { get => meleeChosenUnit; set => meleeChosenUnit = value; }
        int IAbilityOwner.RollingFrames { set => rollingFrames = value; }
        float IAbilityOwner.ShowHighFiveAfterMeleeTimer { set => showHighFiveAfterMeleeTimer = value; }
        bool IAbilityOwner.HasJumpedForKick { get => hasJumpedForKick; set => hasJumpedForKick = value; }
        bool IAbilityOwner.SplitKick { get => splitkick; set => splitkick = value; }
        float IAbilityOwner.HangGrace { set => hangGrace = value; }
        bool IAbilityOwner.CancelMeleeOnChangeDirection { set => cancelMeleeOnChangeDirection = value; }
        bool IAbilityOwner.PerformedMeleeAttack { set => performedMeleeAttack = value; }
        DirectionEnum IAbilityOwner.AirdashDirection { get => airdashDirection; set => airdashDirection = value; }

        void IAbilityOwner.SetSpriteOffset(float x, float y) => SetSpriteOffset(x, y);
        void IAbilityOwner.DeactivateGun() => DeactivateGun();
        void IAbilityOwner.ActivateGun() => ActivateGun();
        void IAbilityOwner.ChangeFrame() => ChangeFrame();
        void IAbilityOwner.SetGunSprite(int spriteFrame, int spriteRow) => SetGunSprite(spriteFrame, spriteRow);
        void IAbilityOwner.CreateFaderTrailInstance() => CreateFaderTrailInstance();
        void IAbilityOwner.SetInvulnerable(float time, bool dvOverride, bool dvNetwork) => SetInvulnerable(time, dvOverride, dvNetwork);
        void IAbilityOwner.ApplyFallingGravity() => ApplyFallingGravity();
        void IAbilityOwner.Jump(bool wallJump) => Jump(wallJump);
        void IAbilityOwner.AnimateJumping() => AnimateJumping();

        void IAbilityOwner.TriggerBroSpecialEvent() => TriggerBroSpecialEvent();
        void IAbilityOwner.PlayAttackSound() => PlayAttackSound();
        void IAbilityOwner.PlayAttackSound(float v) => PlayAttackSound(v);
        void IAbilityOwner.StopAirDashing() => StopAirDashing();
        void IAbilityOwner.StopHanging() => StopHanging();
        void IAbilityOwner.StartHanging() => StartHanging();

        void IAbilityOwner.AnimateMeleeCommon() => AnimateMeleeCommon();
        void IAbilityOwner.CancelMelee() => CancelMelee();
        void IAbilityOwner.SetMeleeType() => SetMeleeType();
        bool IAbilityOwner.TryMeleeTerrain(int offset, int damage)
        {
            if (meleeAbility != null) return meleeAbility.HandleTryMeleeTerrain(offset, damage);
            return TryMeleeTerrain(offset, damage);
        }
        void IAbilityOwner.KickDoors(float range) => KickDoors(range);
        void IAbilityOwner.TriggerBroMeleeEvent() => TriggerBroMeleeEvent();
        void IAbilityOwner.ResetMeleeValues() => ResetMeleeValues();
        void IAbilityOwner.StartMeleeCommon() => StartMeleeCommon();
        void IAbilityOwner.ThrowBackMook(Mook mook) => ThrowBackMook(mook);
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
            if (specialAbility != null && !specialAbility.HandleReleaseSpecial()) return;
            if (meleeAbility != null && !meleeAbility.HandleReleaseSpecial()) return;
            base.ReleaseSpecial();
        }

        protected override bool MustIgnoreHighFiveMeleePress()
        {
            specialAbility?.HandleMustIgnoreHighFiveMeleePress();
            meleeAbility?.HandleMustIgnoreHighFiveMeleePress();
            return base.MustIgnoreHighFiveMeleePress();
        }

        protected override void CalculateMovement()
        {
            float xI = this.xI;
            float yI = this.yI;
            if (specialAbility != null && !specialAbility.HandleCalculateMovement(ref xI, ref yI))
            {
                this.xI = xI;
                this.yI = yI;
                return;
            }
            if (meleeAbility != null && !meleeAbility.HandleCalculateMovement(ref xI, ref yI))
            {
                this.xI = xI;
                this.yI = yI;
                return;
            }
            this.xI = xI;
            this.yI = yI;
            base.CalculateMovement();
            specialAbility?.HandleAfterCalculateMovement();
            meleeAbility?.HandleAfterCalculateMovement();
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
                if (!specialAbility.HandleCanReduceLives(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                bool result = false;
                if (!meleeAbility.HandleCanReduceLives(ref result)) return result;
            }
            return base.CanReduceLives();
        }

        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if (specialAbility != null && !specialAbility.HandleFireWeapon()) return;
            if (meleeAbility != null && !meleeAbility.HandleFireWeapon()) return;
            base.FireWeapon(x, y, xSpeed, ySpeed);
            specialAbility?.HandleAfterFireWeapon(x, y, xSpeed, ySpeed);
            meleeAbility?.HandleAfterFireWeapon(x, y, xSpeed, ySpeed);
        }

        protected override void Jump(bool wallJump)
        {
            if (specialAbility != null && !specialAbility.HandleJump(wallJump)) return;
            if (meleeAbility != null && !meleeAbility.HandleJump(wallJump)) return;
            base.Jump(wallJump);
        }

        protected override void RunMovement()
        {
            if (specialAbility != null && !specialAbility.HandleRunMovement()) return;
            if (meleeAbility != null && !meleeAbility.HandleRunMovement()) return;
            base.RunMovement();
        }

        protected override void ApplyNormalGravity()
        {
            if (specialAbility != null && !specialAbility.HandleApplyNormalGravity()) return;
            if (meleeAbility != null && !meleeAbility.HandleApplyNormalGravity()) return;
            base.ApplyNormalGravity();
        }

        protected override void StartFiring()
        {
            if (specialAbility != null && !specialAbility.HandleStartFiring()) return;
            if (meleeAbility != null && !meleeAbility.HandleStartFiring()) return;
            base.StartFiring();
        }

        protected override void StartMelee()
        {
            if (specialAbility != null && !specialAbility.HandleStartMelee()) return;
            if (meleeAbility != null && !meleeAbility.HandleStartMelee()) return;
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
            if (meleeAbility != null)
            {
                meleeAbility.StartMelee();
                return;
            }
            base.StartKnifeMelee();
        }

        protected override void AnimateKnifeMelee()
        {
            if (meleeAbility != null)
            {
                meleeAbility.AnimateMelee();
                return;
            }
            base.AnimateKnifeMelee();
        }

        protected override void RunKnifeMeleeMovement()
        {
            if (meleeAbility != null)
            {
                meleeAbility.RunMeleeMovement();
                return;
            }
            base.RunKnifeMeleeMovement();
        }

        protected override void StartPunch()
        {
            if (meleeAbility != null)
            {
                meleeAbility.StartMelee();
                return;
            }
            base.StartPunch();
        }

        protected override void AnimatePunch()
        {
            if (meleeAbility != null)
            {
                meleeAbility.AnimateMelee();
                return;
            }
            base.AnimatePunch();
        }

        protected override void RunPunchMovement()
        {
            if (meleeAbility != null)
            {
                meleeAbility.RunMeleeMovement();
                return;
            }
            base.RunPunchMovement();
        }

        protected override void RunJetPackPunchMovement()
        {
            if (meleeAbility != null)
            {
                meleeAbility.RunMeleeMovement();
                return;
            }
            base.RunJetPackPunchMovement();
        }

        protected override void StartCustomMelee()
        {
            if (meleeAbility != null)
            {
                meleeAbility.StartMelee();
                return;
            }
            base.StartCustomMelee();
        }

        protected override void AnimateCustomMelee()
        {
            if (meleeAbility != null)
            {
                meleeAbility.AnimateMelee();
                return;
            }
            base.AnimateCustomMelee();
        }

        protected override void RunCustomMeleeMovement()
        {
            if (meleeAbility != null)
            {
                meleeAbility.RunMeleeMovement();
                return;
            }
            base.RunCustomMeleeMovement();
        }

        protected override void CancelMelee()
        {
            base.CancelMelee();
            if (meleeAbility != null)
            {
                meleeAbility.CancelMelee();
            }
        }

        /// <summary>
        /// Assigns a special ability at runtime. Calls <see cref="SpecialAbility.Initialize" /> on the new ability.
        /// </summary>
        /// <param name="ability">The ability to assign, or null to clear.</param>
        public void SetSpecialAbility(SpecialAbility ability)
        {
            specialAbility?.Cleanup();
            specialAbility = ability;
            ability?.Initialize(this);
        }

        /// <summary>
        /// Assigns a melee ability at runtime. Calls <see cref="MeleeAbility.Initialize" /> on the new ability
        /// and sets <c>meleeType</c> to the ability's declared <see cref="MeleeAbility.meleeType" />.
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
            if (specialAbility != null && !specialAbility.HandleChangeFrame()) return;
            if (meleeAbility != null && !meleeAbility.HandleChangeFrame()) return;
            base.ChangeFrame();
            specialAbility?.HandleAfterChangeFrame();
            meleeAbility?.HandleAfterChangeFrame();
        }

        protected override void IncreaseFrame()
        {
            base.IncreaseFrame();
            specialAbility?.HandleAfterIncreaseFrame();
            meleeAbility?.HandleAfterIncreaseFrame();
        }

        protected override void RunGun()
        {
            if (specialAbility != null && !specialAbility.HandleRunGun()) return;
            if (meleeAbility != null && !meleeAbility.HandleRunGun()) return;
            base.RunGun();
        }

        protected override void RunFiring()
        {
            if (specialAbility != null && !specialAbility.HandleRunFiring()) return;
            if (meleeAbility != null && !meleeAbility.HandleRunFiring()) return;
            base.RunFiring();
        }

        protected override void Land()
        {
            if (specialAbility != null && !specialAbility.HandleLand()) return;
            if (meleeAbility != null && !meleeAbility.HandleLand()) return;
            base.Land();
            specialAbility?.HandleAfterLand();
            meleeAbility?.HandleAfterLand();
        }

        protected override void RunAvatarFiring()
        {
            if (specialAbility != null && !specialAbility.HandleRunAvatarFiring()) return;
            if (meleeAbility != null && !meleeAbility.HandleRunAvatarFiring()) return;
            base.RunAvatarFiring();
        }

        protected override bool IsOverLadder(float xOffset, ref float ladderXPos)
        {
            if (specialAbility != null && !specialAbility.HandleIsOverLadder()) return false;
            if (meleeAbility != null && !meleeAbility.HandleIsOverLadder()) return false;
            return base.IsOverLadder(xOffset, ref ladderXPos);
        }

        protected override bool WallDrag
        {
            get => base.WallDrag;
            set
            {
                if (specialAbility != null && !specialAbility.HandleWallDrag(value)) return;
                if (meleeAbility != null && !meleeAbility.HandleWallDrag(value)) return;
                base.WallDrag = value;
            }
        }

        protected override void AnimateActualJumpingFrames()
        {
            if (specialAbility != null && !specialAbility.HandleAnimateActualJumpingFrames()) return;
            if (meleeAbility != null && !meleeAbility.HandleAnimateActualJumpingFrames()) return;
            base.AnimateActualJumpingFrames();
        }

        protected override void AnimateActualNewRunningFrames()
        {
            base.AnimateActualNewRunningFrames();
            specialAbility?.HandleAfterAnimateNewRunningFrames();
            meleeAbility?.HandleAfterAnimateNewRunningFrames();
        }

        protected override bool ConstrainToFloor(ref float yIT)
        {
            if (specialAbility != null && !specialAbility.HandleConstrainToFloor()) return false;
            if (meleeAbility != null && !meleeAbility.HandleConstrainToFloor()) return false;
            return base.ConstrainToFloor(ref yIT);
        }

        protected override bool ConstrainToCeiling(ref float yIT)
        {
            if (specialAbility != null && !specialAbility.HandleConstrainToCeiling()) return false;
            if (meleeAbility != null && !meleeAbility.HandleConstrainToCeiling()) return false;
            return base.ConstrainToCeiling(ref yIT);
        }

        protected override bool ConstrainToWalls(ref float yIT, ref float xIT)
        {
            if (specialAbility != null && !specialAbility.HandleConstrainToWalls()) return false;
            if (meleeAbility != null && !meleeAbility.HandleConstrainToWalls()) return false;
            return base.ConstrainToWalls(ref yIT, ref xIT);
        }

        public override Vector3 GetFollowPosition()
        {
            if (specialAbility != null)
            {
                Vector3 result = Vector3.zero;
                if (!specialAbility.HandleGetFollowPosition(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                Vector3 result = Vector3.zero;
                if (!meleeAbility.HandleGetFollowPosition(ref result)) return result;
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
            if (specialAbility != null && !specialAbility.HandleGib(damageType, xI, yI)) return;
            if (meleeAbility != null && !meleeAbility.HandleGib(damageType, xI, yI)) return;
            base.Gib(damageType, xI, yI);
        }

        public override void RecallBro()
        {
            if (specialAbility != null && !specialAbility.HandleRecallBro()) return;
            if (meleeAbility != null && !meleeAbility.HandleRecallBro()) return;
            base.RecallBro();
            specialAbility?.HandleAfterRecallBro();
            meleeAbility?.HandleAfterRecallBro();
        }

        public override void AttachToHeli()
        {
            if (specialAbility != null && !specialAbility.HandleAttachToHeli()) return;
            if (meleeAbility != null && !meleeAbility.HandleAttachToHeli()) return;
            base.AttachToHeli();
        }

        protected override void HitCeiling(RaycastHit ceilingHit)
        {
            if (specialAbility != null && !specialAbility.HandleHitCeiling()) return;
            if (meleeAbility != null && !meleeAbility.HandleHitCeiling()) return;
            base.HitCeiling(ceilingHit);
            specialAbility?.HandleAfterHitCeiling();
            meleeAbility?.HandleAfterHitCeiling();
        }

        protected override void HitLeftWall()
        {
            if (specialAbility != null && !specialAbility.HandleHitLeftWall()) return;
            if (meleeAbility != null && !meleeAbility.HandleHitLeftWall()) return;
            base.HitLeftWall();
            specialAbility?.HandleAfterHitLeftWall();
            meleeAbility?.HandleAfterHitLeftWall();
        }

        protected override void HitRightWall()
        {
            if (specialAbility != null && !specialAbility.HandleHitRightWall()) return;
            if (meleeAbility != null && !meleeAbility.HandleHitRightWall()) return;
            base.HitRightWall();
            specialAbility?.HandleAfterHitRightWall();
            meleeAbility?.HandleAfterHitRightWall();
        }

        protected override void ClampWallDragYI(ref float yIT)
        {
            if (specialAbility != null && !specialAbility.HandleClampWallDragYI(ref yIT)) return;
            if (meleeAbility != null && !meleeAbility.HandleClampWallDragYI(ref yIT)) return;
            base.ClampWallDragYI(ref yIT);
        }

        protected override void RunHanging()
        {
            if (specialAbility != null && !specialAbility.HandleRunHanging()) return;
            if (meleeAbility != null && !meleeAbility.HandleRunHanging()) return;
            base.RunHanging();
        }

        protected override bool CanCheckClimbAlongCeiling()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleCanCheckClimbAlongCeiling(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                bool result = false;
                if (!meleeAbility.HandleCanCheckClimbAlongCeiling(ref result)) return result;
            }
            return base.CanCheckClimbAlongCeiling();
        }

        protected override void CheckClimbAlongCeiling()
        {
            if (specialAbility != null && !specialAbility.HandleCheckClimbAlongCeiling()) return;
            if (meleeAbility != null && !meleeAbility.HandleCheckClimbAlongCeiling()) return;
            base.CheckClimbAlongCeiling();
        }

        protected override void CheckInput()
        {
            base.CheckInput();
            specialAbility?.HandleAfterCheckInput();
            meleeAbility?.HandleAfterCheckInput();
        }

        protected override void AirDashDown()
        {
            if (specialAbility != null && !specialAbility.HandleAirDashDown()) return;
            if (meleeAbility != null && !meleeAbility.HandleAirDashDown()) return;
            base.AirDashDown();
        }

        protected override void RunDownwardDash()
        {
            if (specialAbility != null && !specialAbility.HandleRunDownwardDash()) return;
            if (meleeAbility != null && !meleeAbility.HandleRunDownwardDash()) return;
            base.RunDownwardDash();
            specialAbility?.HandleAfterRunDownwardDash();
            meleeAbility?.HandleAfterRunDownwardDash();
        }

        protected override void AnimateIdle()
        {
            if (specialAbility != null && !specialAbility.HandleAnimateIdle()) return;
            if (meleeAbility != null && !meleeAbility.HandleAnimateIdle()) return;
            base.AnimateIdle();
        }

        public override LayerMask GetGroundLayer()
        {
            if (specialAbility != null)
            {
                int result = 0;
                if (!specialAbility.HandleGetGroundLayer(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                int result = 0;
                if (!meleeAbility.HandleGetGroundLayer(ref result)) return result;
            }
            return base.GetGroundLayer();
        }

        public override bool IsAlive()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleIsAlive(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                bool result = false;
                if (!meleeAbility.HandleIsAlive(ref result)) return result;
            }
            return base.IsAlive();
        }

        public override bool Revive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
            if (specialAbility != null && !specialAbility.HandleRevive(playerNum, isUnderPlayerControl, reviveSource)) return false;
            if (meleeAbility != null && !meleeAbility.HandleRevive(playerNum, isUnderPlayerControl, reviveSource)) return false;
            bool result = base.Revive(playerNum, isUnderPlayerControl, reviveSource);
            specialAbility?.HandleAfterRevive(playerNum, isUnderPlayerControl, reviveSource);
            meleeAbility?.HandleAfterRevive(playerNum, isUnderPlayerControl, reviveSource);
            return result;
        }

        public override void UseSteroids()
        {
            if (specialAbility != null && !specialAbility.HandleUseSteroids()) return;
            if (meleeAbility != null && !meleeAbility.HandleUseSteroids()) return;
            base.UseSteroids();
            specialAbility?.HandleAfterUseSteroids();
            meleeAbility?.HandleAfterUseSteroids();
        }

        protected override void CheckNotifyDeathType()
        {
            if (specialAbility != null && !specialAbility.HandleCheckNotifyDeathType()) return;
            if (meleeAbility != null && !meleeAbility.HandleCheckNotifyDeathType()) return;
            base.CheckNotifyDeathType();
        }

        protected override void ApplyFallingGravity()
        {
            if (specialAbility != null && !specialAbility.HandleApplyFallingGravity()) return;
            if (meleeAbility != null && !meleeAbility.HandleApplyFallingGravity()) return;
            base.ApplyFallingGravity();
        }

        protected override void SetDeltaTime()
        {
            if (specialAbility != null && !specialAbility.HandleSetDeltaTime()) return;
            if (meleeAbility != null && !meleeAbility.HandleSetDeltaTime()) return;
            base.SetDeltaTime();
        }

        public override bool CanInseminate(float xI, float yI)
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleCanInseminate(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                bool result = false;
                if (!meleeAbility.HandleCanInseminate(ref result)) return result;
            }
            return base.CanInseminate(xI, yI);
        }

        protected override bool CanStartNewMelee()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleCanStartNewMelee(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                bool result = false;
                if (!meleeAbility.HandleCanStartNewMelee(ref result)) return result;
            }
            return base.CanStartNewMelee();
        }

        protected override bool IsLockedInMelee()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleIsLockedInMelee(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                bool result = false;
                if (!meleeAbility.HandleIsLockedInMelee(ref result)) return result;
            }
            return base.IsLockedInMelee();
        }

        public override void StartPilotingUnit(Unit pilottedUnit)
        {
            if (specialAbility != null && !specialAbility.HandleStartPilotingUnit()) return;
            if (meleeAbility != null && !meleeAbility.HandleStartPilotingUnit()) return;
            base.StartPilotingUnit(pilottedUnit);
        }

        public override void DischargePilotingUnit(float newX, float newY, float xI, float yI, bool stunPilot)
        {
            base.DischargePilotingUnit(newX, newY, xI, yI, stunPilot);
            specialAbility?.HandleAfterDischargePilotingUnit();
            meleeAbility?.HandleAfterDischargePilotingUnit();
        }

        public override void DestroyUnit()
        {
            specialAbility?.HandleDestroyUnit();
            meleeAbility?.HandleDestroyUnit();
            base.DestroyUnit();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            specialAbility?.HandleLateUpdate();
            meleeAbility?.HandleLateUpdate();
        }

        protected override void AddSpeedLeft()
        {
            base.AddSpeedLeft();
            specialAbility?.HandleAfterAddSpeedLeft();
            meleeAbility?.HandleAfterAddSpeedLeft();
        }

        protected override void AddSpeedRight()
        {
            base.AddSpeedRight();
            specialAbility?.HandleAfterAddSpeedRight();
            meleeAbility?.HandleAfterAddSpeedRight();
        }

        public override void Knock(DamageType damageType, float xI, float yI, bool forceTumble)
        {
            if (specialAbility != null && !specialAbility.HandleKnock(damageType, xI, yI, forceTumble)) return;
            if (meleeAbility != null && !meleeAbility.HandleKnock(damageType, xI, yI, forceTumble)) return;
            base.Knock(damageType, xI, yI, forceTumble);
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            specialAbility?.HandleAfterFixedUpdate();
            meleeAbility?.HandleAfterFixedUpdate();
        }

        protected override void CopyInput(TestVanDammeAnim zombie, ref float zombieDelay, ref bool up, ref bool down, ref bool left, ref bool right, ref bool fire, ref bool buttonJump, ref bool special, ref bool highFive)
        {
            base.CopyInput(zombie, ref zombieDelay, ref up, ref down, ref left, ref right, ref fire, ref buttonJump, ref special, ref highFive);
            specialAbility?.HandleAfterCopyInput(zombie, ref zombieDelay, ref up, ref down, ref left, ref right, ref fire, ref buttonJump);
            meleeAbility?.HandleAfterCopyInput(zombie, ref zombieDelay, ref up, ref down, ref left, ref right, ref fire, ref buttonJump);
        }

        protected virtual bool CanBeImpaledByGroundSpikes()
        {
            if (specialAbility != null)
            {
                bool result = false;
                if (!specialAbility.HandleCanBeImpaledByGroundSpikes(ref result)) return result;
            }
            if (meleeAbility != null)
            {
                bool result = false;
                if (!meleeAbility.HandleCanBeImpaledByGroundSpikes(ref result)) return result;
            }
            return !invulnerable && !wallDrag;
        }
        #endregion
    }
}

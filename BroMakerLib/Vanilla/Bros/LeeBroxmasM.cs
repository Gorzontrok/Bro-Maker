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
        [JsonIgnore] public List<PassiveAbility> passives = new List<PassiveAbility>();
        [JsonIgnore] private AbilityBase[] abilities = new AbilityBase[2];

        protected bool blockGesturesDuringMelee = true;

        private void RebuildAbilitiesArray()
        {
            int n = 2 + passives.Count;
            if (abilities == null || abilities.Length != n)
            {
                abilities = new AbilityBase[n];
            }
            abilities[0] = specialAbility;
            abilities[1] = meleeAbility;
            for (int i = 0; i < passives.Count; i++)
            {
                abilities[2 + i] = passives[i];
            }
        }

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

                var awakePrefab = HeroController.GetHeroPrefab(HeroType.LeeBroxmas).As<LeeBroxmas>();
                if (awakePrefab != null)
                {
                    useNewHighFivingFrames = awakePrefab.useNewHighFivingFrames;
                    hasNewAirFlexFrames = awakePrefab.hasNewAirFlexFrames;
                }

                specialAbility = AbilityFactory.CreateSpecial(Info.special, this);
                meleeAbility = AbilityFactory.CreateMelee(Info.melee, this);
                passives = AbilityFactory.CreatePassives(Info.passives, this);
                if (meleeAbility != null)
                {
                    meleeType = meleeAbility.meleeType;
                }
                RebuildAbilitiesArray();

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
            foreach (var a in abilities)
                if (a != null && !a.HandleUpdate()) runBase = false;
            if (runBase) base.Update();
            foreach (var a in abilities)
                a?.Update();
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
            foreach (var a in abilities)
                if (a != null && !a.HandleCheckForTraps()) return;

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
        List<PassiveAbility> IAbilityOwner.Passives => passives;

        T IAbilityOwner.GetPassive<T>()
        {
            for (int i = 0; i < passives.Count; i++)
            {
                if (passives[i] is T t) return t;
            }
            return null;
        }

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
        /// <summary>Assigns a special ability at runtime. Calls `Initialize` on the new ability.</summary>
        public void SetSpecialAbility(SpecialAbility ability)
        {
            specialAbility?.Cleanup();
            specialAbility = ability;
            ability?.Initialize(this);
            RebuildAbilitiesArray();
        }

        /// <summary>Assigns a melee ability at runtime and updates `meleeType` to match.</summary>
        public void SetMeleeAbility(MeleeAbility ability)
        {
            meleeAbility?.Cleanup();
            meleeAbility = ability;
            ability?.Initialize(this);
            if (ability != null)
            {
                meleeType = ability.meleeType;
            }
            RebuildAbilitiesArray();
        }

        /// <summary>Attaches a passive ability at runtime. Initializes it and skips it if `IsRedundant`.
        /// `ConflictsWithPreset` enforcement is not checked on this path.</summary>
        public void AddPassive(PassiveAbility ability)
        {
            if (ability == null) return;
            ability.Initialize(this);
            if (ability.IsRedundant)
            {
                ability.Cleanup();
                return;
            }
            passives.Add(ability);
            RebuildAbilitiesArray();
        }

        /// <summary>Removes and cleans up all attached passives of type <typeparamref name="T" />.</summary>
        public void RemovePassive<T>() where T : PassiveAbility
        {
            for (int i = passives.Count - 1; i >= 0; i--)
            {
                if (passives[i] is T)
                {
                    passives[i].Cleanup();
                    passives.RemoveAt(i);
                }
            }
            RebuildAbilitiesArray();
        }

        /// <summary>Cleans up and removes every attached passive.</summary>
        public void ClearPassives()
        {
            foreach (var p in passives) p?.Cleanup();
            passives.Clear();
            RebuildAbilitiesArray();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            foreach (var a in abilities)
                a?.HandleLateUpdate();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            foreach (var a in abilities)
                a?.HandleAfterFixedUpdate();
        }

        protected override void SetDeltaTime()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleSetDeltaTime()) return;
            base.SetDeltaTime();
        }

        protected override void CheckInput()
        {
            base.CheckInput();
            foreach (var a in abilities)
                a?.HandleAfterCheckInput();
        }

        protected override void CopyInput(TestVanDammeAnim zombie, ref float zombieDelay, ref bool up, ref bool down, ref bool left, ref bool right, ref bool fire, ref bool buttonJump, ref bool special, ref bool highFive)
        {
            base.CopyInput(zombie, ref zombieDelay, ref up, ref down, ref left, ref right, ref fire, ref buttonJump, ref special, ref highFive);
            foreach (var a in abilities)
                a?.HandleAfterCopyInput(zombie, ref zombieDelay, ref up, ref down, ref left, ref right, ref fire, ref buttonJump);
        }

        protected override bool MustIgnoreHighFiveMeleePress()
        {
            foreach (var a in abilities)
                a?.HandleMustIgnoreHighFiveMeleePress();
            return base.MustIgnoreHighFiveMeleePress();
        }

        protected override void PressSpecial()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandlePressSpecial()) return;
            if (specialAbility != null)
            {
                specialAbility.PressSpecial();
                return;
            }
            base.PressSpecial();
        }

        protected override void ReleaseSpecial()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleReleaseSpecial()) return;
            base.ReleaseSpecial();
        }

        protected override void PressHighFiveMelee(bool forceHighFive = false)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandlePressHighFiveMelee()) return;
            base.PressHighFiveMelee(forceHighFive);
        }

        protected override void PressDashButton()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandlePressDashButton()) return;
            base.PressDashButton();
        }

        protected override void CalculateMovement()
        {
            float xI = this.xI;
            float yI = this.yI;
            foreach (var a in abilities)
            {
                if (a != null && !a.HandleCalculateMovement(ref xI, ref yI))
                {
                    this.xI = xI;
                    this.yI = yI;
                    return;
                }
            }
            this.xI = xI;
            this.yI = yI;
            base.CalculateMovement();
            foreach (var a in abilities)
                a?.HandleAfterCalculateMovement();
        }

        protected override void RunMovement()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunMovement()) return;
            base.RunMovement();
        }

        protected override void Jump(bool wallJump)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleJump(wallJump)) return;
            base.Jump(wallJump);
        }

        protected override void AirJump()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAirJump()) return;
            base.AirJump();
        }

        protected override void ApplyNormalGravity()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleApplyNormalGravity()) return;
            base.ApplyNormalGravity();
        }

        protected override void ApplyFallingGravity()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleApplyFallingGravity()) return;
            base.ApplyFallingGravity();
        }

        protected override void RunFalling()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunFalling()) return;
            base.RunFalling();
        }

        protected override void AddSpeedLeft()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAddSpeedLeft()) return;
            base.AddSpeedLeft();
            foreach (var a in abilities)
                a?.HandleAfterAddSpeedLeft();
        }

        protected override void AddSpeedRight()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAddSpeedRight()) return;
            base.AddSpeedRight();
            foreach (var a in abilities)
                a?.HandleAfterAddSpeedRight();
        }

        public override void Knock(DamageType damageType, float xI, float yI, bool forceTumble)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleKnock(damageType, xI, yI, forceTumble)) return;
            base.Knock(damageType, xI, yI, forceTumble);
        }

        protected override bool ConstrainToFloor(ref float yIT)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleConstrainToFloor()) return false;
            return base.ConstrainToFloor(ref yIT);
        }

        protected override bool ConstrainToCeiling(ref float yIT)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleConstrainToCeiling()) return false;
            return base.ConstrainToCeiling(ref yIT);
        }

        protected override bool ConstrainToWalls(ref float yIT, ref float xIT)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleConstrainToWalls()) return false;
            return base.ConstrainToWalls(ref yIT, ref xIT);
        }

        protected override void ClampWallDragYI(ref float yIT)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleClampWallDragYI(ref yIT)) return;
            base.ClampWallDragYI(ref yIT);
        }

        protected override void HitCeiling(RaycastHit ceilingHit)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleHitCeiling()) return;
            base.HitCeiling(ceilingHit);
            foreach (var a in abilities)
                a?.HandleAfterHitCeiling();
        }

        protected override void HitLeftWall()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleHitLeftWall()) return;
            base.HitLeftWall();
            foreach (var a in abilities)
                a?.HandleAfterHitLeftWall();
        }

        protected override void HitRightWall()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleHitRightWall()) return;
            base.HitRightWall();
            foreach (var a in abilities)
                a?.HandleAfterHitRightWall();
        }

        protected override void Land()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleLand()) return;
            base.Land();
            foreach (var a in abilities)
                a?.HandleAfterLand();
        }

        protected override bool WallDrag
        {
            get => base.WallDrag;
            set
            {
                foreach (var a in abilities)
                    if (a != null && !a.HandleWallDrag(value)) return;
                base.WallDrag = value;
            }
        }

        public override LayerMask GetGroundLayer()
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                int result = 0;
                if (!a.HandleGetGroundLayer(ref result)) return result;
            }
            return base.GetGroundLayer();
        }

        protected override void CheckForQuicksand()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleCheckForQuicksand()) return;
            base.CheckForQuicksand();
        }

        protected override bool IsOverLadder(float xOffset, ref float ladderXPos)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleIsOverLadder()) return false;
            return base.IsOverLadder(xOffset, ref ladderXPos);
        }

        protected override void RunHanging()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunHanging()) return;
            base.RunHanging();
        }

        protected override bool CanCheckClimbAlongCeiling()
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                bool result = false;
                if (!a.HandleCanCheckClimbAlongCeiling(ref result)) return result;
            }
            return base.CanCheckClimbAlongCeiling();
        }

        protected override void CheckClimbAlongCeiling()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleCheckClimbAlongCeiling()) return;
            base.CheckClimbAlongCeiling();
        }

        protected override void LedgeGrapple(bool left, bool right, float radius, float heightOpenOffset)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleLedgeGrapple(left, right, radius, heightOpenOffset)) return;
            base.LedgeGrapple(left, right, radius, heightOpenOffset);
        }

        protected override void AirDashDown()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAirDashDown()) return;
            base.AirDashDown();
        }

        protected override void AirDashUp()
        {
            base.AirDashUp();
            foreach (var a in abilities)
                a?.HandleAfterAirDashUp();
        }

        protected override void RunDownwardDash()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunDownwardDash()) return;
            base.RunDownwardDash();
            foreach (var a in abilities)
                a?.HandleAfterRunDownwardDash();
        }

        protected override void RunLeftAirDash()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunLeftAirDash()) return;
            base.RunLeftAirDash();
            foreach (var a in abilities)
                a?.HandleAfterRunLeftAirDash();
        }

        protected override void RunRightAirDash()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunRightAirDash()) return;
            base.RunRightAirDash();
            foreach (var a in abilities)
                a?.HandleAfterRunRightAirDash();
        }

        protected override void RunUpwardDash()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunUpwardDash()) return;
            base.RunUpwardDash();
            foreach (var a in abilities)
                a?.HandleAfterRunUpwardDash();
        }

        protected override void AnimateAirdash()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAnimateAirdash()) return;
            base.AnimateAirdash();
        }

        protected override void PlayAidDashSound()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandlePlayAidDashSound()) return;
            base.PlayAidDashSound();
        }

        protected override void PlayAirDashChargeUpSound()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandlePlayAirDashChargeUpSound()) return;
            base.PlayAirDashChargeUpSound();
        }

        protected override void StartFiring()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleStartFiring()) return;
            base.StartFiring();
        }

        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleFireWeapon()) return;
            base.FireWeapon(x, y, xSpeed, ySpeed);
            foreach (var a in abilities)
                a?.HandleAfterFireWeapon(x, y, xSpeed, ySpeed);
        }

        protected override void RunGun()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunGun()) return;
            base.RunGun();
        }

        protected override void RunFiring()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunFiring()) return;
            base.RunFiring();
        }

        protected override void RunAvatarFiring()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRunAvatarFiring()) return;
            base.RunAvatarFiring();
        }

        protected override void ActivateGun()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleActivateGun()) return;
            base.ActivateGun();
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

        protected override void StartMelee()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleStartMelee()) return;
            base.StartMelee();
        }

        protected override bool CanStartNewMelee()
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                bool result = false;
                if (!a.HandleCanStartNewMelee(ref result)) return result;
            }
            return base.CanStartNewMelee();
        }

        protected override bool IsLockedInMelee()
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                bool result = false;
                if (!a.HandleIsLockedInMelee(ref result)) return result;
            }
            return base.IsLockedInMelee();
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

        protected override void ChangeFrame()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleChangeFrame()) return;
            base.ChangeFrame();
            foreach (var a in abilities)
                a?.HandleAfterChangeFrame();
        }

        protected override void IncreaseFrame()
        {
            base.IncreaseFrame();
            foreach (var a in abilities)
                a?.HandleAfterIncreaseFrame();
        }

        protected override void AnimateActualJumpingFrames()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAnimateActualJumpingFrames()) return;
            base.AnimateActualJumpingFrames();
        }

        protected override void AnimateActualJumpingDuckingFrames()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAnimateActualJumpingDuckingFrames()) return;
            base.AnimateActualJumpingDuckingFrames();
        }

        protected override void AnimateIdle()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAnimateIdle()) return;
            base.AnimateIdle();
        }

        protected override void AnimateActualNewRunningFrames()
        {
            base.AnimateActualNewRunningFrames();
            foreach (var a in abilities)
                a?.HandleAfterAnimateNewRunningFrames();
        }

        protected override void AnimateWallAnticipation()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAnimateWallAnticipation()) return;
            base.AnimateWallAnticipation();
        }

        protected override void AnimateWallClimb()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAnimateWallClimb()) return;
            base.AnimateWallClimb();
        }

        public override void Damage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleDamage(damage, damageType, xI, yI, direction, damageSender, hitX, hitY)) return;
            base.Damage(damage, damageType, xI, yI, direction, damageSender, hitX, hitY);
        }

        public override void Death(float xI, float yI, DamageObject damage)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleDeath()) return;
            base.Death(xI, yI, damage);
            foreach (var a in abilities)
                a?.HandleAfterDeath();
        }

        protected override void Gib(DamageType damageType, float xI, float yI)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleGib(damageType, xI, yI)) return;
            base.Gib(damageType, xI, yI);
        }

        protected override bool CanReduceLives()
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                bool result = false;
                if (!a.HandleCanReduceLives(ref result)) return result;
            }
            return base.CanReduceLives();
        }

        public override bool IsAlive()
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                bool result = false;
                if (!a.HandleIsAlive(ref result)) return result;
            }
            return base.IsAlive();
        }

        public override bool Revive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRevive(playerNum, isUnderPlayerControl, reviveSource)) return false;
            bool result = base.Revive(playerNum, isUnderPlayerControl, reviveSource);
            foreach (var a in abilities)
                a?.HandleAfterRevive(playerNum, isUnderPlayerControl, reviveSource);
            return result;
        }

        protected override void CheckNotifyDeathType()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleCheckNotifyDeathType()) return;
            base.CheckNotifyDeathType();
        }

        protected virtual bool CanBeImpaledByGroundSpikes()
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                bool result = false;
                if (!a.HandleCanBeImpaledByGroundSpikes(ref result)) return result;
            }
            return !invulnerable && !wallDrag;
        }

        public override void UseSteroids()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleUseSteroids()) return;
            base.UseSteroids();
            foreach (var a in abilities)
                a?.HandleAfterUseSteroids();
        }

        public override bool IsInStealthMode()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleIsInStealthMode()) return true;
            return base.IsInStealthMode();
        }

        protected override void AlertNearbyMooks()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAlertNearbyMooks()) return;
            base.AlertNearbyMooks();
        }

        public override bool CanInseminate(float xI, float yI)
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                bool result = false;
                if (!a.HandleCanInseminate(ref result)) return result;
            }
            return base.CanInseminate(xI, yI);
        }

        public override Vector3 GetFollowPosition()
        {
            foreach (var a in abilities)
            {
                if (a == null) continue;
                Vector3 result = Vector3.zero;
                if (!a.HandleGetFollowPosition(ref result)) return result;
            }
            return base.GetFollowPosition();
        }

        public override void RecallBro()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleRecallBro()) return;
            base.RecallBro();
            foreach (var a in abilities)
                a?.HandleAfterRecallBro();
        }

        public override void AttachToHeli()
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleAttachToHeli()) return;
            base.AttachToHeli();
        }

        public override void StartPilotingUnit(Unit pilottedUnit)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleStartPilotingUnit()) return;
            base.StartPilotingUnit(pilottedUnit);
        }

        public override void DischargePilotingUnit(float newX, float newY, float xI, float yI, bool stunPilot)
        {
            base.DischargePilotingUnit(newX, newY, xI, yI, stunPilot);
            foreach (var a in abilities)
                a?.HandleAfterDischargePilotingUnit();
        }

        public override void DestroyUnit()
        {
            foreach (var a in abilities)
                a?.HandleDestroyUnit();
            base.DestroyUnit();
        }

        protected override void ThrowBackMook(Mook mook)
        {
            foreach (var a in abilities)
                if (a != null && !a.HandleThrowBackMook(mook)) return;
            base.ThrowBackMook(mook);
        }
        #endregion
    }
}

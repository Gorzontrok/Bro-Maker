// Auto-generated from RambroM.cs, do not edit manually

using System;
using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BroshipTrooper", HeroType.Rambro)]
    public class BroshipTrooperM : BroshipTrooper, ICustomHero, IAbilityOwner
    {
        [Syncronize] public CustomBroInfo Info { get; set; }
        [Syncronize] public BroBase Character { get; set; }
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
        [JsonIgnore] private OwnerSpeedState speedState;

        protected bool blockGesturesDuringMelee = true;

        private bool meleeButtonPressed;
        private bool abilityMeleeOwned;
        private bool hostIndependentMeleeAnimation;

        private bool fireLoopRefused;
        private bool fireHeldAtLastFireLoop;

        private bool abilityUsingSpecial;

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
            foreach (BroBase vanilla in GetComponents<BroBase>())
            {
                if (vanilla != this)
                {
                    LoadHero.CopySerializedFields(vanilla, this);
                }
            }

            hoverSinSpeed = 12f;
            hoverSinStart = 20f;
            hoverSinForce = 0.08f;
            defaultHoverForce = 0.09f;
            hoverDuration = 2.2f;
            liftOffBlastFlameWall = (HeroController.GetHeroPrefab(HeroType.DoubleBroSeven) as DoubleBroSeven).liftOffBlastFlameWall;
            specialGrenade = HeroController.GetHeroPrefab(HeroType.TheBrocketeer).specialGrenade;
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
                this.LogError($"Couldn't set up '{Info?.name}' when it spawned", ex);
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
                this.LogError($"Couldn't finish setting up '{Info?.name}' when it spawned", ex);
                enabled = false;
            }
        }

        protected override void Update()
        {
            bool runBase = true;
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleUpdate())
                {
                    runBase = false;
                }
            }
            if (runBase)
            {
                base.Update();
            }
            EndFinishedAbilityMelee();
            foreach (AbilityBase a in abilities)
            {
                a?.Update();
            }
        }

        public override void SetGestureAnimation(GestureElement.Gestures gesture)
        {
            if (blockGesturesDuringMelee && doingMelee)
            {
                return;
            }

            base.SetGestureAnimation(gesture);
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
                if (passives[i] is T t)
                {
                    return t;
                }
            }
            return null;
        }

        OwnerSpeedState IAbilityOwner.SpeedState => speedState ?? (speedState = new OwnerSpeedState(this));

        SpriteSM IAbilityOwner.Sprite => sprite;
        int IAbilityOwner.SpritePixelWidth => spritePixelWidth;
        int IAbilityOwner.SpritePixelHeight => spritePixelHeight;
        int IAbilityOwner.GunSpritePixelWidth => gunSpritePixelWidth;
        int IAbilityOwner.GunSpritePixelHeight => gunSpritePixelHeight;
        bool IAbilityOwner.Ducking => ducking;
        float IAbilityOwner.DeltaTime { get => t; set => t = value; }
        float IAbilityOwner.LastDeltaTime { set => lastT = value; }
        Sound IAbilityOwner.Sound => sound;
        SoundHolderVoice IAbilityOwner.SoundHolderVoice => SoundHolderVoice;
        LayerMask IAbilityOwner.GroundLayer => groundLayer;
        bool IAbilityOwner.WallDrag => wallDrag;
        bool IAbilityOwner.IsInQuicksand => isInQuicksand;
        float IAbilityOwner.HalfWidth => halfWidth;
        float IAbilityOwner.CeilingHeight { get => ceilingHeight; set => ceilingHeight = value; }
        LayerMask IAbilityOwner.FragileLayer => fragileLayer;
        bool IAbilityOwner.HighFive => highFive;

        float IAbilityOwner.FrameRate { get => frameRate; set => frameRate = value; }
        int IAbilityOwner.GunFrame { get => gunFrame; set => gunFrame = value; }
        float IAbilityOwner.InvulnerableTime { get => invulnerableTime; set => invulnerableTime = value; }
        float IAbilityOwner.StunTime => stunTime;
        float IAbilityOwner.JumpTime { set => jumpTime = value; }
        float IAbilityOwner.DeathTime { get => deathTime; set => deathTime = value; }
        DeathType IAbilityOwner.CurrentDeathType { get => deathType; set => deathType = value; }
        Mook IAbilityOwner.NearbyMook { get => nearbyMook; set => nearbyMook = value; }
        bool IAbilityOwner.HasPlayedMissSound { get => hasPlayedMissSound; set => hasPlayedMissSound = value; }

        bool IAbilityOwner.UsingSpecial
        {
            get => SpecialAbility.SpecialStateActive ? abilityUsingSpecial : usingSpecial;
            set
            {
                if (value && !((IAbilityOwner)this).UsingSpecial)
                {
                    specialAbility?.ResetTriggerFrames();
                }
                if (SpecialAbility.SpecialStateActive)
                {
                    abilityUsingSpecial = value;
                }
                else
                {
                    usingSpecial = value;
                }
            }
        }

        bool IAbilityOwner.HostUsingSpecial => usingSpecial;
        bool IAbilityOwner.UsingPockettedSpecial { get => usingPockettedSpecial; set => usingPockettedSpecial = value; }
        int IAbilityOwner.PressSpecialFacingDirection { get => pressSpecialFacingDirection; set => pressSpecialFacingDirection = value; }

        bool IAbilityOwner.DoingMelee { get => doingMelee; set => doingMelee = value; }
        bool IAbilityOwner.MeleeFromButton => meleeButtonPressed;
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

        bool IAbilityOwner.CanAirdash { get => canAirdash; set => canAirdash = value; }
        float IAbilityOwner.AirdashTime { get => airdashTime; set => airdashTime = value; }
        float IAbilityOwner.AirDashDelay => airDashDelay;
        bool IAbilityOwner.WasHighFive { get => wasHighFive; set => wasHighFive = value; }
        bool IAbilityOwner.WasDashButton => wasdashButton;
        bool IAbilityOwner.HoldingHighFive => holdingHighFive;
        bool IAbilityOwner.AirdashUpAvailable => airdashUpAvailable;
        float IAbilityOwner.DefaultAirdashDelay { get => defaultAirdashDelay; set => defaultAirdashDelay = value; }

        float IAbilityOwner.PressedJumpInAirSoJumpIfTouchGroundGrace => pressedJumpInAirSoJumpIfTouchGroundGrace;
        bool IAbilityOwner.ChimneyFlip { get => chimneyFlip; set => chimneyFlip = value; }
        bool IAbilityOwner.ChimneyFlipConstrained => chimneyFlipConstrained;
        float IAbilityOwner.AvatarAngryTime { get => avatarAngryTime; set => avatarAngryTime = value; }
        bool IAbilityOwner.ControllingProjectile { get => controllingProjectile; set => controllingProjectile = value; }
        bool IAbilityOwner.WallClimbing => wallClimbing;
        float IAbilityOwner.LastJumpTime { get => lastJumpTime; set => lastJumpTime = value; }
        float IAbilityOwner.LastButtonJumpTime { get => lastButtonJumpTime; set => lastButtonJumpTime = value; }
        float IAbilityOwner.HighFiveBoostTime { get => highFiveBoostTime; set => highFiveBoostTime = value; }
        bool IAbilityOwner.HighFiveBoostActive { set => highFiveBoost = value; }
        float IAbilityOwner.AvatarGunFireTime { get => avatarGunFireTime; set => avatarGunFireTime = value; }
        int IAbilityOwner.SpecialAmmoField { get => _specialAmmo; set => _specialAmmo = value; }
        MeleeType IAbilityOwner.CurrentMeleeType { get => currentMeleeType; set => currentMeleeType = value; }
        float IAbilityOwner.OriginalMaxFallSpeed => originalMaxFallSpeed;
        float IAbilityOwner.FireCounter { get => fireCounter; set => fireCounter = value; }

        void IAbilityOwner.SetSpriteOffset(float x, float y) => SetSpriteOffset(x, y);
        void IAbilityOwner.DeactivateGun() => DeactivateGun();
        void IAbilityOwner.ActivateGun() => ActivateGun();
        void IAbilityOwner.ChangeFrame() => ChangeFrame();
        void IAbilityOwner.SetGunSprite(int spriteFrame, int spriteRow) => SetGunSprite(spriteFrame, spriteRow);
        void IAbilityOwner.SetDeltaTime() => SetDeltaTime();

        void IAbilityOwner.CreateFaderTrailInstance()
        {
            if (faderSpritePrefab == null)
            {
                return;
            }
            // The Brode draws trails only while usingSpecial is set, so an ability's request sets it for the call.
            bool hostUsingSpecial = usingSpecial;
            usingSpecial = true;
            try
            {
                CreateFaderTrailInstance();
            }
            finally
            {
                usingSpecial = hostUsingSpecial;
            }
        }

        void IAbilityOwner.SetInvulnerable(float time, bool dvOverride, bool dvNetwork) => SetInvulnerable(time, dvOverride, dvNetwork);
        void IAbilityOwner.ApplyFallingGravity() => ApplyFallingGravity();
        void IAbilityOwner.Jump(bool wallJump) => Jump(wallJump);
        void IAbilityOwner.AnimateJumping() => AnimateJumping();

        void IAbilityOwner.TriggerBroSpecialEvent() => TriggerBroSpecialEvent();
        void IAbilityOwner.PlayAttackSound() => PlayAttackSound();
        void IAbilityOwner.PlayAttackSound(float v) => PlayAttackSound(v);
        void IAbilityOwner.UseFire() => UseFire();
        void IAbilityOwner.FireFlashAvatar() => FireFlashAvatar();
        void IAbilityOwner.StopAirDashing() => StopAirDashing();
        void IAbilityOwner.StopHanging() => StopHanging();
        void IAbilityOwner.StartHanging() => StartHanging();

        void IAbilityOwner.AnimateMeleeCommon() => AnimateMeleeCommon();
        void IAbilityOwner.CancelMelee() => CancelMelee();
        void IAbilityOwner.SetMeleeType() => SetMeleeType();

        bool IAbilityOwner.TryMeleeTerrain(int offset, int damage)
        {
            if (meleeAbility != null)
            {
                return meleeAbility.TryMeleeTerrain(offset, damage);
            }
            return TryMeleeTerrain(offset, damage);
        }

        void IAbilityOwner.KickDoors(float range) => KickDoors(range);
        void IAbilityOwner.TriggerBroMeleeEvent() => TriggerBroMeleeEvent();
        void IAbilityOwner.ResetMeleeValues() => ResetMeleeValues();
        void IAbilityOwner.StartMeleeCommon() => StartMeleeCommon();
        void IAbilityOwner.ThrowBackMook(Mook mook) => ThrowBackMook(mook);
        void IAbilityOwner.ClaimMelee() => ClaimAbilityMelee();
        #endregion

        #region Ability Forwarding
        public void SetSpecialAbility(SpecialAbility ability)
        {
            specialAbility?.Detach();
            abilityUsingSpecial = false;
            specialAbility = ability;
            ability?.Initialize(this);
            RebuildAbilitiesArray();
        }

        public void SetMeleeAbility(MeleeAbility ability)
        {
            if (abilityMeleeOwned && doingMelee)
            {
                CancelMelee();
            }
            EndAbilityMelee();
            meleeAbility?.Detach();
            meleeAbility = ability;
            ability?.Initialize(this);
            if (ability != null)
            {
                meleeType = ability.meleeType;
            }
            RebuildAbilitiesArray();
        }

        public void AddPassive(PassiveAbility ability)
        {
            if (ability == null)
            {
                return;
            }
            ability.Initialize(this);
            if (ability.IsRedundant)
            {
                ability.Detach();
                return;
            }
            passives.Add(ability);
            RebuildAbilitiesArray();
        }

        public void RemovePassive<T>() where T : PassiveAbility
        {
            for (int i = passives.Count - 1; i >= 0; i--)
            {
                if (passives[i] is T)
                {
                    passives[i].Detach();
                    passives.RemoveAt(i);
                }
            }
            RebuildAbilitiesArray();
        }

        public void ClearPassives()
        {
            foreach (PassiveAbility p in passives)
            {
                p?.Detach();
            }
            passives.Clear();
            RebuildAbilitiesArray();
        }

        protected override void LateUpdate()
        {
            base.LateUpdate();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleLateUpdate();
            }
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterFixedUpdate();
            }
        }

        protected override void SetDeltaTime()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleSetDeltaTime())
                {
                    return;
                }
            }
            base.SetDeltaTime();
        }

        protected override void SetHighFiveBoostDeltaTime()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleSetHighFiveBoostDeltaTime())
                {
                    return;
                }
            }
            base.SetHighFiveBoostDeltaTime();
        }

        protected override void OnDestroy()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleOnDestroy())
                {
                    return;
                }
            }
            base.OnDestroy();
        }

        protected override void CheckInput()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleCheckInput())
                {
                    return;
                }
            }
            base.CheckInput();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterCheckInput();
            }
        }

        protected override void CopyInput(TestVanDammeAnim zombie, ref float zombieDelay, ref bool up, ref bool down, ref bool left, ref bool right, ref bool fire, ref bool buttonJump, ref bool special, ref bool highFive)
        {
            base.CopyInput(zombie, ref zombieDelay, ref up, ref down, ref left, ref right, ref fire, ref buttonJump, ref special, ref highFive);
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterCopyInput(zombie, ref zombieDelay, ref up, ref down, ref left, ref right, ref fire, ref buttonJump);
            }
        }

        protected override bool MustIgnoreHighFiveMeleePress()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleMustIgnoreHighFiveMeleePress(ref result))
                {
                    return result;
                }
            }
            return base.MustIgnoreHighFiveMeleePress();
        }

        protected override void PressSpecial()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandlePressSpecial())
                {
                    return;
                }
            }
            if (specialAbility != null)
            {
                if (SpecialAbility.SpecialStateActive && usingSpecial)
                {
                    return;
                }
                specialAbility.PressSpecial();
                return;
            }
            base.PressSpecial();
        }

        protected override void ReleaseSpecial()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleReleaseSpecial())
                {
                    return;
                }
            }
            base.ReleaseSpecial();
        }

        protected override void PressHighFiveMelee(bool forceHighFive = false)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandlePressHighFiveMelee(ref forceHighFive))
                {
                    return;
                }
            }
            bool wasMeleeButtonPressed = meleeButtonPressed;
            meleeButtonPressed = true;
            try
            {
                base.PressHighFiveMelee(forceHighFive);
            }
            finally
            {
                meleeButtonPressed = wasMeleeButtonPressed;
            }
        }

        protected override void PressDashButton()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandlePressDashButton())
                {
                    return;
                }
            }
            base.PressDashButton();
        }

        protected override void CalculateMovement()
        {
            speedState?.Sync();
            float xI = this.xI;
            float yI = this.yI;
            foreach (AbilityBase a in abilities)
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
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterCalculateMovement();
            }
        }

        protected override void RunMovement()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunMovement())
                {
                    return;
                }
            }
            base.RunMovement();
        }

        protected override void Jump(bool wallJump)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleJump(wallJump))
                {
                    return;
                }
            }
            base.Jump(wallJump);
        }

        protected override void AirJump()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAirJump())
                {
                    return;
                }
            }
            base.AirJump();
        }

        protected override void ApplyNormalGravity()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleApplyNormalGravity())
                {
                    return;
                }
            }
            base.ApplyNormalGravity();
        }

        protected override void ApplyFallingGravity()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleApplyFallingGravity())
                {
                    return;
                }
            }
            base.ApplyFallingGravity();
        }

        protected override void RunFalling()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunFalling())
                {
                    return;
                }
            }
            base.RunFalling();
        }

        protected override void AddSpeedLeft()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAddSpeedLeft())
                {
                    return;
                }
            }
            base.AddSpeedLeft();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterAddSpeedLeft();
            }
        }

        protected override void AddSpeedRight()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAddSpeedRight())
                {
                    return;
                }
            }
            base.AddSpeedRight();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterAddSpeedRight();
            }
        }

        public override void Knock(DamageType damageType, float xI, float yI, bool forceTumble)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleKnock(damageType, xI, yI, forceTumble))
                {
                    return;
                }
            }
            base.Knock(damageType, xI, yI, forceTumble);
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterKnock(damageType, xI, yI, forceTumble);
            }
        }

        protected override bool ConstrainToFloor(ref float yIT)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleConstrainToFloor())
                {
                    return false;
                }
            }
            return base.ConstrainToFloor(ref yIT);
        }

        protected override bool ConstrainToCeiling(ref float yIT)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleConstrainToCeiling())
                {
                    return false;
                }
            }
            return base.ConstrainToCeiling(ref yIT);
        }

        protected override bool ConstrainToWalls(ref float yIT, ref float xIT)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleConstrainToWalls())
                {
                    return false;
                }
            }
            return base.ConstrainToWalls(ref yIT, ref xIT);
        }

        protected override void ClampWallDragYI(ref float yIT)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleClampWallDragYI(ref yIT))
                {
                    return;
                }
            }
            base.ClampWallDragYI(ref yIT);
        }

        protected override void HitCeiling(RaycastHit ceilingHit)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleHitCeiling())
                {
                    return;
                }
            }
            base.HitCeiling(ceilingHit);
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterHitCeiling();
            }
        }

        protected override void HitLeftWall()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleHitLeftWall())
                {
                    return;
                }
            }
            base.HitLeftWall();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterHitLeftWall();
            }
        }

        protected override void HitRightWall()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleHitRightWall())
                {
                    return;
                }
            }
            base.HitRightWall();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterHitRightWall();
            }
        }

        protected override void Land()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleLand())
                {
                    return;
                }
            }
            base.Land();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterLand();
            }
        }

        protected override void RollOnLand()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRollOnLand())
                {
                    return;
                }
            }
            base.RollOnLand();
        }

        protected override bool WallDrag
        {
            get => base.WallDrag;
            set
            {
                foreach (AbilityBase a in abilities)
                {
                    if (a != null && !a.HandleWallDrag(value))
                    {
                        return;
                    }
                }
                base.WallDrag = value;
            }
        }

        public override LayerMask GetGroundLayer()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                int result = 0;
                if (!a.HandleGetGroundLayer(ref result))
                {
                    return result;
                }
            }
            return base.GetGroundLayer();
        }

        protected override void CheckForQuicksand()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleCheckForQuicksand())
                {
                    return;
                }
            }
            base.CheckForQuicksand();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterCheckForQuicksand();
            }
        }

        protected override bool IsOverLadder(float xOffset, ref float ladderXPos)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleIsOverLadder())
                {
                    return false;
                }
            }
            return base.IsOverLadder(xOffset, ref ladderXPos);
        }

        protected override void RunHanging()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunHanging())
                {
                    return;
                }
            }
            base.RunHanging();
        }

        protected override bool CanCheckClimbAlongCeiling()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleCanCheckClimbAlongCeiling(ref result))
                {
                    return result;
                }
            }
            // Seth Brondle can't hang from a ceiling during a special, the ability's included.
            bool hostUsingSpecial = usingSpecial;
            usingSpecial = hostUsingSpecial || (SpecialAbility.SpecialStateActive && abilityUsingSpecial);
            try
            {
                return base.CanCheckClimbAlongCeiling();
            }
            finally
            {
                usingSpecial = hostUsingSpecial;
            }
        }

        protected override void CheckClimbAlongCeiling()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleCheckClimbAlongCeiling())
                {
                    return;
                }
            }
            base.CheckClimbAlongCeiling();
        }

        protected override void LedgeGrapple(bool left, bool right, float radius, float heightOpenOffset)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleLedgeGrapple(left, right, radius, heightOpenOffset))
                {
                    return;
                }
            }
            base.LedgeGrapple(left, right, radius, heightOpenOffset);
        }

        protected override void AirDashDown()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAirDashDown())
                {
                    return;
                }
            }
            base.AirDashDown();
        }

        protected override bool CanAirDash(DirectionEnum direction)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleCanAirDash(direction, ref result))
                {
                    return result;
                }
            }
            return base.CanAirDash(direction);
        }

        protected override void AirDashUp()
        {
            base.AirDashUp();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterAirDashUp();
            }
        }

        protected override void AirDashLeft()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAirDashLeft())
                {
                    return;
                }
            }
            base.AirDashLeft();
        }

        protected override void AirDashRight()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAirDashRight())
                {
                    return;
                }
            }
            base.AirDashRight();
        }

        protected override void RunDownwardDash()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunDownwardDash())
                {
                    return;
                }
            }
            base.RunDownwardDash();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterRunDownwardDash();
            }
        }

        protected override void RunLeftAirDash()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunLeftAirDash())
                {
                    return;
                }
            }
            base.RunLeftAirDash();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterRunLeftAirDash();
            }
        }

        protected override void RunRightAirDash()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunRightAirDash())
                {
                    return;
                }
            }
            base.RunRightAirDash();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterRunRightAirDash();
            }
        }

        protected override void RunUpwardDash()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunUpwardDash())
                {
                    return;
                }
            }
            base.RunUpwardDash();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterRunUpwardDash();
            }
        }

        protected override void AnimateAirdash()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAnimateAirdash())
                {
                    return;
                }
            }
            base.AnimateAirdash();
        }

        protected override void PlayAidDashSound()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandlePlayAidDashSound())
                {
                    return;
                }
            }
            base.PlayAidDashSound();
        }

        protected override void PlayAirDashChargeUpSound()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandlePlayAirDashChargeUpSound())
                {
                    return;
                }
            }
            base.PlayAirDashChargeUpSound();
        }

        protected override void StartFiring()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleStartFiring())
                {
                    return;
                }
            }
            base.StartFiring();
        }

        protected override void UseFire()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleUseFire())
                {
                    return;
                }
            }
            base.UseFire();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterUseFire();
            }
        }

        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleFireWeapon())
                {
                    return;
                }
            }
            base.FireWeapon(x, y, xSpeed, ySpeed);
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterFireWeapon(x, y, xSpeed, ySpeed);
            }
        }

        protected override void RunGun()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunGun())
                {
                    return;
                }
            }
            base.RunGun();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterRunGun();
            }
        }

        protected override void RunFiring()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunFiring())
                {
                    fireLoopRefused = true;
                    return;
                }
            }
            // Hosts that act only on a `fire`/`wasFire` edge would otherwise miss a press or release made while the loop was refused.
            bool replayEdge = fireLoopRefused && fire != fireHeldAtLastFireLoop && wasFire == fire;
            fireLoopRefused = false;
            if (replayEdge)
            {
                wasFire = !wasFire;
            }
            base.RunFiring();
            if (replayEdge)
            {
                wasFire = !wasFire;
            }
            fireHeldAtLastFireLoop = fire;
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterRunFiring();
            }
        }

        protected override void RunAvatarFiring()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunAvatarFiring())
                {
                    return;
                }
            }
            base.RunAvatarFiring();
        }

        protected override void ActivateGun()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleActivateGun())
                {
                    return;
                }
            }
            base.ActivateGun();
        }

        protected override void SetGunPosition(float xOffset, float yOffset)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleSetGunPosition(ref xOffset, ref yOffset))
                {
                    return;
                }
            }
            base.SetGunPosition(xOffset + CurrentGunSpriteOffset.x, yOffset + CurrentGunSpriteOffset.y);
        }

        protected override void AnimateSpecial()
        {
            if (specialAbility != null && (abilityUsingSpecial || !SpecialAbility.SpecialStateActive))
            {
                specialAbility.RunAnimateSpecial();
                return;
            }
            base.AnimateSpecial();
        }

        // With the ability state split off, only the bro's own code reaches here, for its own special.
        protected override void UseSpecial()
        {
            if (specialAbility != null)
            {
                if (!SpecialAbility.SpecialStateActive)
                {
                    specialAbility.UseSpecial();
                }
                return;
            }
            base.UseSpecial();
        }

        protected override void StartMelee()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleStartMelee())
                {
                    return;
                }
            }
            EndFinishedAbilityMelee();
            if (meleeAbility != null && meleeButtonPressed)
            {
                RaycastHit hitInfo;
                bool onAnimal = (Physics.Raycast(new Vector3(X, Y + 5f, 0f), Vector3.down, out hitInfo, 16f, platformLayer) || Physics.Raycast(new Vector3(X + 4f, Y + 5f, 0f), Vector3.down, out hitInfo, 16f, platformLayer) || Physics.Raycast(new Vector3(X - 4f, Y + 5f, 0f), Vector3.down, out hitInfo, 16f, platformLayer)) && hitInfo.collider.GetComponentInParent<Animal>() != null;
                if (meleeAbility.HandlesMeleeType(onAnimal ? MeleeType.Knife : meleeType, true))
                {
                    // The owner's own StartMelee is skipped for a melee the ability takes, so BroBase.StartMelee's work is done here.
                    counter = 0f;
                    currentMeleeType = onAnimal ? MeleeType.Knife : meleeAbility.meleeType;
                    switch (currentMeleeType)
                    {
                        case MeleeType.Knife:
                            StartKnifeMelee();
                            break;
                        case MeleeType.Punch:
                        case MeleeType.JetpackPunch:
                            StartPunch();
                            break;
                        default:
                            StartCustomMelee();
                            break;
                    }
                    return;
                }
            }
            base.StartMelee();
        }

        private bool StartAbilityMelee()
        {
            if (meleeAbility == null || !meleeAbility.HandlesMeleeType(currentMeleeType, meleeButtonPressed))
            {
                if (abilityMeleeOwned && doingMelee)
                {
                    CancelRunningMelee();
                }
                EndAbilityMelee();
                return false;
            }
            ClaimAbilityMelee();
            meleeAbility.StartMelee();
            return true;
        }

        private void ClaimAbilityMelee()
        {
            if (meleeAbility == null || (abilityMeleeOwned && doingMelee))
            {
                return;
            }
            if (doingMelee)
            {
                CancelRunningMelee();
            }
            EndAbilityMelee();
            // Frames of an ability melee come through ChangeFrame; a preset that animates independently sets the flag itself.
            hostIndependentMeleeAnimation = canDoIndependentMeleeAnimation;
            canDoIndependentMeleeAnimation = false;
            abilityMeleeOwned = true;
        }

        // A start that moves a running melee to the other owner ends it first, keeping the target the press just chose.
        private void CancelRunningMelee()
        {
            Unit chosenUnit = meleeChosenUnit;
            CancelMelee();
            meleeChosenUnit = chosenUnit;
        }

        private void EndAbilityMelee()
        {
            if (!abilityMeleeOwned)
            {
                return;
            }
            abilityMeleeOwned = false;
            canDoIndependentMeleeAnimation = hostIndependentMeleeAnimation;
        }

        private void EndFinishedAbilityMelee()
        {
            if (abilityMeleeOwned && !doingMelee)
            {
                EndAbilityMelee();
            }
        }

        // StartMelee reroutes currentMeleeType to Knife on an Animal, so a non-knife ability yields to the vanilla knife for that swing.
        protected override void StartKnifeMelee()
        {
            if (currentMeleeType == (meleeAbility != null ? meleeAbility.meleeType : meleeType))
            {
                foreach (AbilityBase a in abilities)
                {
                    if (a != null && !a.HandleStartMeleeAttack())
                    {
                        return;
                    }
                }
            }
            if (StartAbilityMelee())
            {
                return;
            }
            base.StartKnifeMelee();
        }

        protected override void StartPunch()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleStartMeleeAttack())
                {
                    return;
                }
            }
            if (StartAbilityMelee())
            {
                return;
            }
            base.StartPunch();
        }

        protected override void StartCustomMelee()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleStartMeleeAttack())
                {
                    return;
                }
            }
            if (StartAbilityMelee())
            {
                return;
            }
            base.StartCustomMelee();
        }

        protected override void AnimateMelee()
        {
            if (abilityMeleeOwned)
            {
                meleeAbility.AnimateMelee();
                return;
            }
            base.AnimateMelee();
        }

        protected override void RunMelee()
        {
            if (abilityMeleeOwned)
            {
                meleeAbility.RunMeleeMovement();
                return;
            }
            base.RunMelee();
        }

        protected override void SetMeleeType()
        {
            if (!abilityMeleeOwned)
            {
                base.SetMeleeType();
                return;
            }
            if (!useNewKnifingFrames)
            {
                standingMelee = true;
                jumpingMelee = false;
                dashingMelee = false;
            }
            else if (actionState == ActionState.Jumping || Y > groundHeight + 1f)
            {
                standingMelee = false;
                jumpingMelee = true;
                dashingMelee = false;
            }
            else if (right || left)
            {
                standingMelee = false;
                jumpingMelee = false;
                dashingMelee = true;
            }
            else if (actionState == ActionState.ClimbingLadder)
            {
                standingMelee = false;
                jumpingMelee = true;
                dashingMelee = false;
            }
            else
            {
                standingMelee = true;
                jumpingMelee = false;
                dashingMelee = false;
            }
        }

        protected override void CancelMelee()
        {
            base.CancelMelee();
            if (meleeAbility != null)
            {
                meleeAbility.CancelMelee();
            }
            EndFinishedAbilityMelee();
        }

        protected override bool CanStartNewMelee()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleCanStartNewMelee(ref result))
                {
                    return result;
                }
            }
            if (abilityMeleeOwned)
            {
                return !doingMelee || frame > 3;
            }
            return base.CanStartNewMelee();
        }

        protected override bool IsLockedInMelee()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleIsLockedInMelee(ref result))
                {
                    return result;
                }
            }
            if (abilityMeleeOwned)
            {
                return doingMelee && !canDoIndependentMeleeAnimation;
            }
            return base.IsLockedInMelee();
        }

        protected override void RunIndependentMeleeFrames()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRunIndependentMeleeFrames())
                {
                    return;
                }
            }
            base.RunIndependentMeleeFrames();
        }

        protected override void ResetMeleeValues()
        {
            base.ResetMeleeValues();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterResetMeleeValues();
            }
        }

        protected override void ChangeFrame()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleChangeFrame())
                {
                    return;
                }
            }
            base.ChangeFrame();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterChangeFrame();
            }
        }

        protected override void IncreaseFrame()
        {
            base.IncreaseFrame();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterIncreaseFrame();
            }
        }

        protected override void AnimateJumping()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAnimateJumping())
                {
                    return;
                }
            }
            base.AnimateJumping();
        }

        protected override void AnimateActualJumpingFrames()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAnimateActualJumpingFrames())
                {
                    return;
                }
            }
            base.AnimateActualJumpingFrames();
        }

        protected override void AnimateActualJumpingDuckingFrames()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAnimateActualJumpingDuckingFrames())
                {
                    return;
                }
            }
            base.AnimateActualJumpingDuckingFrames();
        }

        protected override void AnimateIdle()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAnimateIdle())
                {
                    return;
                }
            }
            base.AnimateIdle();
        }

        public override void AnimateActualIdleFrames()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAnimateActualIdleFrames())
                {
                    return;
                }
            }
            base.AnimateActualIdleFrames();
        }

        protected override void AnimateActualNewRunningFrames()
        {
            base.AnimateActualNewRunningFrames();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterAnimateNewRunningFrames();
            }
        }

        protected override void AnimateWallAnticipation()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAnimateWallAnticipation())
                {
                    return;
                }
            }
            base.AnimateWallAnticipation();
        }

        protected override void AnimateWallClimb()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAnimateWallClimb())
                {
                    return;
                }
            }
            base.AnimateWallClimb();
        }

        public override void Damage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleDamage(damage, damageType, xI, yI, direction, damageSender, hitX, hitY))
                {
                    return;
                }
            }
            base.Damage(damage, damageType, xI, yI, direction, damageSender, hitX, hitY);
        }

        public override void Stop()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleStop())
                {
                    return;
                }
            }
            base.Stop();
        }

        public override void Death(float xI, float yI, DamageObject damage)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleDeath(xI, yI, damage))
                {
                    return;
                }
            }
            base.Death(xI, yI, damage);
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterDeath(xI, yI, damage);
            }
        }

        protected override void Gib(DamageType damageType, float xI, float yI)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleGib(damageType, xI, yI))
                {
                    return;
                }
            }
            base.Gib(damageType, xI, yI);
        }

        protected override bool CanReduceLives()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleCanReduceLives(ref result))
                {
                    return result;
                }
            }
            return base.CanReduceLives();
        }

        protected override void ReduceLives(bool destroyed)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleReduceLives(destroyed))
                {
                    return;
                }
            }
            base.ReduceLives(destroyed);
        }

        public override bool IsAlive()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleIsAlive(ref result))
                {
                    return result;
                }
            }
            return base.IsAlive();
        }

        public override bool Revive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRevive(playerNum, isUnderPlayerControl, reviveSource))
                {
                    return false;
                }
            }
            bool result = base.Revive(playerNum, isUnderPlayerControl, reviveSource);
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterRevive(playerNum, isUnderPlayerControl, reviveSource, result);
            }
            return result;
        }

        protected override void CheckNotifyDeathType()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleCheckNotifyDeathType())
                {
                    return;
                }
            }
            base.CheckNotifyDeathType();
        }

        protected override void CheckForTraps(ref float yIT)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleCheckForTraps())
                {
                    return;
                }
            }

            TestVanDammeAnim self = this;
            if (usingSpecial && (self is TheBrolander || self is BronanTheBrobarian))
            {
                return;
            }

            float num = Y + yIT;
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

            DoodadAcidPool nearestAcid = Map.GetNearestAcid(X, Y + 8f, 2f);
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
                Block component = raycastHit.collider.GetComponent<Block>();
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
                            float num2 = playerNum >= 0 ? component.spikes.spikeTrapHarmlessHeight : component.spikes.spikeTrapHarmlessHeight * 0.5f;
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
                Block component2 = raycastHit2.collider.GetComponent<Block>();
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
                            float num3 = playerNum >= 0 ? component2.spikes.spikeTrapHarmlessHeight : component2.spikes.spikeTrapHarmlessHeight * 0.5f;
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
                Block component3 = raycastHit3.collider.GetComponent<Block>();
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
                            float num4 = playerNum >= 0 ? component3.spikes.spikeTrapHarmlessHeight : component3.spikes.spikeTrapHarmlessHeight * 0.5f;
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

        protected override bool CanBeImpaledByGroundSpikes()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleCanBeImpaledByGroundSpikes(ref result))
                {
                    return result;
                }
            }
            return base.CanBeImpaledByGroundSpikes();
        }

        public override void UseSteroids()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleUseSteroids())
                {
                    return;
                }
            }
            base.UseSteroids();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterUseSteroids();
            }
        }

        protected override void PerformanceEnhancedGrunt()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandlePerformanceEnhancedGrunt())
                {
                    return;
                }
            }
            base.PerformanceEnhancedGrunt();
        }

        public override void ResetSpecialAmmo()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleResetSpecialAmmo())
                {
                    return;
                }
            }
            base.ResetSpecialAmmo();
        }

        public override bool IsAmmoFull()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleIsAmmoFull(ref result))
                {
                    return result;
                }
            }
            return base.IsAmmoFull();
        }

        public override bool IsInStealthMode()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleIsInStealthMode(ref result))
                {
                    return result;
                }
            }
            return base.IsInStealthMode();
        }

        protected override void AlertNearbyMooks()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAlertNearbyMooks())
                {
                    return;
                }
            }
            base.AlertNearbyMooks();
        }

        public override bool CanInseminate(float xI, float yI)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                bool result = false;
                if (!a.HandleCanInseminate(ref result))
                {
                    return result;
                }
            }
            return base.CanInseminate(xI, yI);
        }

        public override Vector3 GetFollowPosition()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                Vector3 result = Vector3.zero;
                if (!a.HandleGetFollowPosition(ref result))
                {
                    return result;
                }
            }
            return base.GetFollowPosition();
        }

        public override void RecallBro()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleRecallBro())
                {
                    return;
                }
            }
            base.RecallBro();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterRecallBro();
            }
        }

        public override void AttachToHeli()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleAttachToHeli())
                {
                    return;
                }
            }
            base.AttachToHeli();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterAttachToHeli();
            }
        }

        public override void StartPilotingUnit(Unit pilottedUnit)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleStartPilotingUnit())
                {
                    return;
                }
            }
            base.StartPilotingUnit(pilottedUnit);
        }

        public override void DischargePilotingUnit(float newX, float newY, float xI, float yI, bool stunPilot)
        {
            base.DischargePilotingUnit(newX, newY, xI, yI, stunPilot);
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterDischargePilotingUnit(newX, newY, xI, yI, stunPilot);
            }
        }

        public override void DestroyUnit()
        {
            base.DestroyUnit();
            foreach (AbilityBase a in abilities)
            {
                a?.HandleAfterDestroyUnit();
            }
        }

        protected override void ThrowBackMook(Mook mook)
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleThrowBackMook(mook))
                {
                    return;
                }
            }
            base.ThrowBackMook(mook);
        }

        protected override void FindNearbyMook()
        {
            foreach (AbilityBase a in abilities)
            {
                if (a != null && !a.HandleFindNearbyMook())
                {
                    return;
                }
            }
            base.FindNearbyMook();
        }

        protected override void SetupThrownMookVelocity(out float XI, out float YI)
        {
            float xI, yI;
            base.SetupThrownMookVelocity(out xI, out yI);
            foreach (AbilityBase a in abilities)
            {
                if (a == null)
                {
                    continue;
                }
                if (!a.HandleSetupThrownMookVelocity(ref xI, ref yI))
                {
                    break;
                }
            }
            XI = xI;
            YI = yI;
        }
        #endregion
    }
}

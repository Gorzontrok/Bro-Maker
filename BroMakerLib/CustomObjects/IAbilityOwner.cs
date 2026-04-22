using System.Collections.Generic;
using BroMakerLib.Abilities;
using UnityEngine;

namespace BroMakerLib.CustomObjects
{
    /// <summary>Interface for bros that support the ability system.</summary>
    public interface IAbilityOwner
    {
        SpecialAbility SpecialAbility { get; }
        MeleeAbility MeleeAbility { get; }

        /// <summary>All passive abilities attached to this bro, in JSON-declared order. Do not mutate the returned list directly; use the bro's `AddPassive`/`RemovePassive`/`ClearPassives` helpers.</summary>
        List<PassiveAbility> Passives { get; }

        /// <summary>Returns the first attached passive of type <typeparamref name="T" />, or null if none.</summary>
        T GetPassive<T>() where T : PassiveAbility;

        #region Field Accessors
        SpriteSM Sprite { get; }
        int SpritePixelWidth { get; }
        int SpritePixelHeight { get; }
        bool Ducking { get; }
        float DeltaTime { get; set; }
        Sound Sound { get; }
        float FrameRate { get; set; }
        float InvulnerableTime { get; set; }
        int GunFrame { get; set; }
        LayerMask GroundLayer { get; }
        bool WallDrag { get; }
        float JumpTime { set; }
        bool IsInQuicksand { get; }
        float HalfWidth { get; }
        float CeilingHeight { get; set; }
        LayerMask FragileLayer { get; }
        float DeathTime { get; set; }
        DeathType CurrentDeathType { get; set; }
        bool HighFive { get; }
        Mook NearbyMook { get; set; }
        bool HasPlayedMissSound { get; set; }

        bool UsingSpecial { get; set; }
        bool UsingPockettedSpecial { get; set; }
        int PressSpecialFacingDirection { get; set; }

        bool DoingMelee { get; set; }
        bool MeleeHasHit { get; set; }
        bool MeleeFollowUp { get; set; }
        bool StandingMelee { get; }
        bool JumpingMelee { get; set; }
        bool DashingMelee { get; set; }
        Unit MeleeChosenUnit { get; set; }
        int RollingFrames { set; }
        float ShowHighFiveAfterMeleeTimer { set; }
        bool HasJumpedForKick { get; set; }
        bool SplitKick { get; set; }
        float HangGrace { set; }
        bool CancelMeleeOnChangeDirection { set; }
        bool PerformedMeleeAttack { set; }
        DirectionEnum AirdashDirection { get; set; }

        bool CanAirdash { get; set; }
        float AirdashTime { get; set; }
        float AirDashDelay { get; }
        bool WasHighFive { get; set; }
        bool WasDashButton { get; }
        bool HoldingHighFive { get; }
        bool AirdashUpAvailable { get; }
        float DefaultAirdashDelay { get; set; }

        float PressedJumpInAirSoJumpIfTouchGroundGrace { get; }
        bool ChimneyFlip { get; set; }
        float AvatarAngryTime { get; set; }
        bool ControllingProjectile { get; set; }
        bool WallClimbing { get; }
        float LastJumpTime { get; set; }
        float AvatarGunFireTime { get; set; }
        int SpecialAmmoField { get; set; }
        BroBase.MeleeType CurrentMeleeType { get; set; }
        float OriginalMaxFallSpeed { get; }
        #endregion

        #region Method Accessors
        void SetSpriteOffset(float x, float y);
        void DeactivateGun();
        void ActivateGun();
        void ChangeFrame();
        void SetGunSprite(int spriteFrame, int spriteRow);
        void CreateFaderTrailInstance();
        void SetInvulnerable(float time, bool dvOverride, bool dvNetwork);
        void ApplyFallingGravity();
        void Jump(bool wallJump);
        void AnimateJumping();

        void TriggerBroSpecialEvent();
        void PlayAttackSound();
        void PlayAttackSound(float v);
        void StopAirDashing();
        void StopHanging();
        void StartHanging();

        void AnimateMeleeCommon();
        void CancelMelee();
        void SetMeleeType();
        bool TryMeleeTerrain(int offset, int damage);
        void KickDoors(float range);
        void TriggerBroMeleeEvent();
        void ResetMeleeValues();
        void StartMeleeCommon();
        void ThrowBackMook(Mook mook);
        #endregion
    }
}

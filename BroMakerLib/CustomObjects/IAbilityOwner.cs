using System.Collections.Generic;
using BroMakerLib.Abilities;
using UnityEngine;

namespace BroMakerLib.CustomObjects
{
    /// <summary>
    /// Interface for bros that host the ability system.
    /// </summary>
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
        float DeltaTime { get; }
        Sound Sound { get; }
        float FrameRate { get; set; }
        float InvulnerableTime { get; set; }
        int GunFrame { get; set; }
        LayerMask GroundLayer { get; }
        bool WallDrag { get; }
        float JumpTime { set; }
        bool IsInQuicksand { get; }
        float HalfWidth { get; }
        float CeilingHeight { get; }
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

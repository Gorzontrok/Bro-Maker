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

        /// <summary>All passive abilities attached to this bro, in JSON-declared order. Mutate it only through this interface's `AddPassive`, `RemovePassive` and `ClearPassives`.</summary>
        List<PassiveAbility> Passives { get; }

        /// <summary>Returns the first attached passive of type `T`, or null if none.</summary>
        T GetPassive<T>() where T : PassiveAbility;

        /// <summary>Replaces the attached special ability, detaching the old one and initializing the new one, or removes it when `ability` is null.</summary>
        void SetSpecialAbility(SpecialAbility ability);

        /// <summary>Replaces the attached melee ability, ending a melee the old one is running, detaching it, initializing the new one and setting the bro's melee type to the new one's, or removes it when `ability` is null, leaving the bro's melee type as it is.</summary>
        void SetMeleeAbility(MeleeAbility ability);

        /// <summary>Initializes `ability` and attaches it after the attached passives, skipping one the bro already has natively, with none of the duplicate and conflict checks a JSON `passives` list gets.</summary>
        void AddPassive(PassiveAbility ability);

        /// <summary>Detaches every attached passive of type `T`. A hook or `Update` call in progress can still reach a detached passive, after its `Cleanup`.</summary>
        void RemovePassive<T>() where T : PassiveAbility;

        /// <summary>Detaches every attached passive. A hook or `Update` call in progress can still reach them, after their `Cleanup`.</summary>
        void ClearPassives();

        /// <summary>The bro's speed as changed by its abilities' speed helpers; the bro calls its `Sync` once per frame, before computing its movement.</summary>
        OwnerSpeedState SpeedState { get; }

        #region Field Accessors
        SpriteSM Sprite { get; }
        int SpritePixelWidth { get; }
        int SpritePixelHeight { get; }
        int GunSpritePixelWidth { get; }
        int GunSpritePixelHeight { get; }
        bool Ducking { get; }
        float DeltaTime { get; set; }

        /// <summary>The bro's `lastT`, its delta time from before the current frame's.</summary>
        float LastDeltaTime { set; }

        Sound Sound { get; }

        /// <summary>The bro's active voice sound holder, honoring any per-bro override.</summary>
        SoundHolderVoice SoundHolderVoice { get; }

        float FrameRate { get; set; }
        float InvulnerableTime { get; set; }
        float StunTime { get; }
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

        /// <summary>True while the attached special ability's special runs. The game's shared animation, movement, input and state code treats it as the bro's special, and ends it wherever that code ends the bro's special. The bro's own code neither sees nor changes it, with one exception: Seth Brondle can't hang from a ceiling during it. When BroMaker couldn't adjust the game's special handling, it is the bro's own `usingSpecial`.</summary>
        bool UsingSpecial { get; set; }

        /// <summary>True while the bro's own special, started by its own code, runs. When BroMaker couldn't adjust the game's special handling, it is the bro's own `usingSpecial`, which the attached special ability also sets.</summary>
        bool HostUsingSpecial { get; }

        bool UsingPockettedSpecial { get; set; }
        int PressSpecialFacingDirection { get; set; }

        bool DoingMelee { get; set; }

        /// <summary>True while the melee button's press is being handled, so a melee started now came from the button rather than from the bro's own code.</summary>
        bool MeleeFromButton { get; }

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
        bool ChimneyFlipConstrained { get; }
        float AvatarAngryTime { get; set; }
        bool ControllingProjectile { get; set; }
        bool WallClimbing { get; }
        float LastJumpTime { get; set; }
        float LastButtonJumpTime { get; set; }
        float HighFiveBoostTime { get; set; }
        bool HighFiveBoostActive { set; }
        float AvatarGunFireTime { get; set; }
        int SpecialAmmoField { get; set; }
        BroBase.MeleeType CurrentMeleeType { get; set; }
        float OriginalMaxFallSpeed { get; }
        float FireCounter { get; set; }
        #endregion

        #region Method Accessors
        void SetSpriteOffset(float x, float y);
        void DeactivateGun();
        void ActivateGun();
        void ChangeFrame();
        void SetGunSprite(int spriteFrame, int spriteRow);

        /// <summary>Runs the bro's `SetDeltaTime`, through its attached abilities' `HandleSetDeltaTime`.</summary>
        void SetDeltaTime();

        /// <summary>Spawns one after-image from the owner's `faderSpritePrefab`, or does nothing when the owner has none.</summary>
        void CreateFaderTrailInstance();

        void SetInvulnerable(float time, bool dvOverride, bool dvNetwork);
        void ApplyFallingGravity();
        void Jump(bool wallJump);
        void AnimateJumping();

        void TriggerBroSpecialEvent();
        void PlayAttackSound();
        void PlayAttackSound(float v);
        void UseFire();
        void FireFlashAvatar();
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

        /// <summary>Marks a melee the melee ability starts itself as the ability's, so the owner animates and moves it through the ability; call it before starting the melee.</summary>
        void ClaimMelee();
        #endregion
    }
}

using BroMakerLib.CustomObjects;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>
    /// Shared base class for <see cref="SpecialAbility" /> and <see cref="MeleeAbility" />.
    /// </summary>
    public abstract class AbilityBase
    {
        [JsonIgnore]
        public TestVanDammeAnim owner;

        [JsonIgnore]
        protected IAbilityOwner hero;

        [JsonIgnore]
        protected int PlayerNum => owner.playerNum;

        /// <summary>Facing direction: 1 = right, -1 = left.</summary>
        [JsonIgnore]
        protected float Direction => Mathf.Sign(owner.transform.localScale.x);

        [JsonIgnore]
        protected float X => owner.X;

        [JsonIgnore]
        protected float Y => owner.Y;

        [JsonIgnore]
        protected Sound sound => hero.Sound;

        [JsonIgnore]
        protected SoundHolder soundHolder => owner.soundHolder;

        /// <summary>Called once when the bro spawns.</summary>
        /// <param name="owner">The bro instance that owns this ability.</param>
        public virtual void Initialize(TestVanDammeAnim owner)
        {
            this.owner = owner;
            this.hero = owner as IAbilityOwner;
        }

        public virtual void Update()
        {
        }

        /// <summary>Called before this ability is replaced by another.</summary>
        public virtual void Cleanup()
        {
        }

        #region Hooks
        /// <returns>Return false to skip base.Update() entirely.</returns>
        public virtual bool HandleUpdate()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckForTraps()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleReleaseSpecial()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleMustIgnoreHighFiveMeleePress()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            return true;
        }

        public virtual void HandleAfterCalculateMovement()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDeath()
        {
            return true;
        }

        public virtual void HandleAfterDeath()
        {
        }

        /// <returns>True to run original, false to force the provided result.</returns>
        public virtual bool HandleCanReduceLives(ref bool result)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleFireWeapon()
        {
            return true;
        }

        public virtual void HandleAfterFireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleJump(bool wallJump)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunMovement()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyNormalGravity()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartFiring()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartMelee()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleChangeFrame()
        {
            return true;
        }

        public virtual void HandleAfterChangeFrame()
        {
        }

        public virtual void HandleAfterIncreaseFrame()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunGun()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunFiring()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleLand()
        {
            return true;
        }

        public virtual void HandleAfterLand()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunAvatarFiring()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleIsOverLadder()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleWallDrag(bool value)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateActualJumpingFrames()
        {
            return true;
        }

        public virtual void HandleAfterAnimateNewRunningFrames()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToFloor()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToCeiling()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToWalls()
        {
            return true;
        }

        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleGetFollowPosition(ref Vector3 result)
        {
            return true;
        }

        /// <returns>True to run original, false to force stealth mode active.</returns>
        public virtual bool HandleIsInStealthMode()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAlertNearbyMooks()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleGib(DamageType damageType, float xI, float yI)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRecallBro()
        {
            return true;
        }

        public virtual void HandleAfterRecallBro()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAttachToHeli()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitCeiling()
        {
            return true;
        }

        public virtual void HandleAfterHitCeiling()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitLeftWall()
        {
            return true;
        }

        public virtual void HandleAfterHitLeftWall()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitRightWall()
        {
            return true;
        }

        public virtual void HandleAfterHitRightWall()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleClampWallDragYI(ref float yIT)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunHanging()
        {
            return true;
        }

        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleCanCheckClimbAlongCeiling(ref bool result)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckClimbAlongCeiling()
        {
            return true;
        }

        public virtual void HandleAfterCheckInput()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAirDashDown()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunDownwardDash()
        {
            return true;
        }

        public virtual void HandleAfterRunDownwardDash()
        {
        }

        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleIsAlive(ref bool result)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
            return true;
        }

        public virtual void HandleAfterRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleUseSteroids()
        {
            return true;
        }

        public virtual void HandleAfterUseSteroids()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckNotifyDeathType()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyFallingGravity()
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleSetDeltaTime()
        {
            return true;
        }

        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleCanInseminate(ref bool result)
        {
            return true;
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartPilotingUnit()
        {
            return true;
        }

        public virtual void HandleAfterDischargePilotingUnit()
        {
        }

        public virtual void HandleDestroyUnit()
        {
        }

        public virtual void HandleLateUpdate()
        {
        }

        public virtual void HandleAfterAddSpeedLeft()
        {
        }

        public virtual void HandleAfterAddSpeedRight()
        {
        }

        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateIdle()
        {
            return true;
        }

        /// <summary>Override to change which ground layers the bro collides with.</summary>
        /// <returns>True to use base result, false to use the modified result.</returns>
        public virtual bool HandleGetGroundLayer(ref int result)
        {
            return true;
        }

        /// <returns>True to run original, false to force the provided result.</returns>
        public virtual bool HandleCanBeImpaledByGroundSpikes(ref bool result)
        {
            return true;
        }

        /// <summary>Override to gate when the player can re-trigger a melee mid-melee.</summary>
        /// <param name="result">Modify to override the default.</param>
        /// <returns>True to use base result, false to use the modified result.</returns>
        public virtual bool HandleCanStartNewMelee(ref bool result)
        {
            return true;
        }

        /// <summary>Override to control whether the player is movement-locked during a melee.</summary>
        /// <param name="result">Modify to override the default.</param>
        /// <returns>True to use base result, false to use the modified result.</returns>
        public virtual bool HandleIsLockedInMelee(ref bool result)
        {
            return true;
        }
        #endregion
    }
}

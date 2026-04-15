using BroMakerLib.CustomObjects;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>Shared base class for `SpecialAbility` and `PassiveAbility` and `MeleeAbility`.</summary>
    public abstract class AbilityBase
    {
        /// <summary>Sprite sheet row for the primary animation.</summary>
        public int animationRow = 1;

        /// <summary>Starting column for the primary animation.</summary>
        public int animationColumn = 25;

        /// <summary>Number of frames in the primary animation.</summary>
        public int animationFrameCount = 7;

        /// <summary>Seconds per animation frame.</summary>
        public float frameRate = 0.025f;

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


        /// <summary>Called each frame during the bro's Update loop.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleUpdate() => true;

        /// <summary>Called each frame during LateUpdate.</summary>
        public virtual void HandleLateUpdate() { }

        /// <summary>Called after FixedUpdate each physics tick.</summary>
        public virtual void HandleAfterFixedUpdate() { }

        /// <summary>Called when the bro's per-frame delta time is being computed.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleSetDeltaTime() => true;


        /// <summary>Called after the bro reads and processes its input state.</summary>
        public virtual void HandleAfterCheckInput() { }

        /// <summary>Called after input is copied to a zombie co-op bro.</summary>
        public virtual void HandleAfterCopyInput(TestVanDammeAnim zombie, ref float zombieDelay, ref bool up, ref bool down, ref bool left, ref bool right, ref bool fire, ref bool buttonJump) { }

        /// <summary>Called when checking whether to suppress melee input for a high-five.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleMustIgnoreHighFiveMeleePress() => true;

        /// <summary>Called when the player presses the special button.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePressSpecial() => true;

        /// <summary>Called when the player releases the special button.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleReleaseSpecial() => true;

        /// <summary>Called when the player presses the melee/high-five button.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePressHighFiveMelee() => true;

        /// <summary>Called when the player presses the dash button.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePressDashButton() => true;


        /// <summary>Called each frame to compute the bro's velocity. Write to `xI`/`yI` to override; pair with `HandleAfterCalculateMovement`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCalculateMovement(ref float xI, ref float yI) => true;

        /// <summary>Called after velocity is computed. Pair with `HandleCalculateMovement`.</summary>
        public virtual void HandleAfterCalculateMovement() { }

        /// <summary>Called each frame to apply horizontal movement.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunMovement() => true;

        /// <summary>Called when the bro jumps. `wallJump` is true for wall jumps.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleJump(bool wallJump) => true;

        /// <summary>Called when the bro performs an air jump (double jump).</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAirJump() => true;

        /// <summary>Called each frame to apply normal (grounded) gravity.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyNormalGravity() => true;

        /// <summary>Called each frame to apply falling gravity.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyFallingGravity() => true;

        /// <summary>Called each frame while the bro is airborne.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunFalling() => true;

        /// <summary>Called to accelerate the bro leftward. Pair with `HandleAfterAddSpeedLeft`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAddSpeedLeft() => true;

        /// <summary>Called after leftward acceleration is applied. Pair with `HandleAddSpeedLeft`.</summary>
        public virtual void HandleAfterAddSpeedLeft() { }

        /// <summary>Called to accelerate the bro rightward. Pair with `HandleAfterAddSpeedRight`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAddSpeedRight() => true;

        /// <summary>Called after rightward acceleration is applied. Pair with `HandleAddSpeedRight`.</summary>
        public virtual void HandleAfterAddSpeedRight() { }

        /// <summary>Called when the bro is knocked back by damage.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleKnock(DamageType damageType, float xI, float yI, bool forceTumble) => true;


        /// <summary>Called each frame to prevent the bro from passing through the floor.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToFloor() => true;

        /// <summary>Called each frame to prevent the bro from passing through the ceiling.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToCeiling() => true;

        /// <summary>Called each frame to prevent the bro from passing through walls.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToWalls() => true;

        /// <summary>Called to clamp vertical speed while wall-dragging. Write to `yIT` to override the clamped value.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleClampWallDragYI(ref float yIT) => true;

        /// <summary>Called when the bro's head hits the ceiling. Pair with `HandleAfterHitCeiling`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitCeiling() => true;

        /// <summary>Called after ceiling-hit logic runs. Pair with `HandleHitCeiling`.</summary>
        public virtual void HandleAfterHitCeiling() { }

        /// <summary>Called when the bro hits the left wall. Pair with `HandleAfterHitLeftWall`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitLeftWall() => true;

        /// <summary>Called after left-wall-hit logic runs. Pair with `HandleHitLeftWall`.</summary>
        public virtual void HandleAfterHitLeftWall() { }

        /// <summary>Called when the bro hits the right wall. Pair with `HandleAfterHitRightWall`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitRightWall() => true;

        /// <summary>Called after right-wall-hit logic runs. Pair with `HandleHitRightWall`.</summary>
        public virtual void HandleAfterHitRightWall() { }

        /// <summary>Called when the bro lands on the ground. Pair with `HandleAfterLand`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleLand() => true;

        /// <summary>Called after landing logic runs. Pair with `HandleLand`.</summary>
        public virtual void HandleAfterLand() { }

        /// <summary>Called each frame to handle wall-drag state. `value` is the requested drag state.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleWallDrag(bool value) => true;

        /// <summary>Called when determining which physics layer to treat as ground. Write to `result` and return false to override.</summary>
        /// <returns>True to use base result, false to use the modified `result`.</returns>
        public virtual bool HandleGetGroundLayer(ref int result) => true;

        /// <summary>Called each frame to check whether the bro is standing in quicksand.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckForQuicksand() => true;


        /// <summary>Called to check whether the bro is positioned over a ladder.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleIsOverLadder() => true;

        /// <summary>Called each frame while the bro is hanging on a ledge or ladder.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunHanging() => true;

        /// <summary>Called to determine whether ceiling-climb is possible. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleCanCheckClimbAlongCeiling(ref bool result) => true;

        /// <summary>Called each frame to run ceiling-climb movement.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckClimbAlongCeiling() => true;

        /// <summary>Called when the bro attempts to grab a ledge.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleLedgeGrapple(bool left, bool right, float radius, float heightOpenOffset) => true;


        /// <summary>Called when the bro initiates a downward air dash. Pair with `HandleAfterAirDashUp` for the upward variant.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAirDashDown() => true;

        /// <summary>Called after an upward air dash resolves. Pair with `HandleAirDashDown` for the downward variant.</summary>
        public virtual void HandleAfterAirDashUp() { }

        /// <summary>Called each frame while a downward dash is active. Pair with `HandleAfterRunDownwardDash`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunDownwardDash() => true;

        /// <summary>Called after downward-dash movement is applied. Pair with `HandleRunDownwardDash`.</summary>
        public virtual void HandleAfterRunDownwardDash() { }

        /// <summary>Called each frame while a leftward air dash is active. Pair with `HandleAfterRunLeftAirDash`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunLeftAirDash() => true;

        /// <summary>Called after leftward-dash movement is applied. Pair with `HandleRunLeftAirDash`.</summary>
        public virtual void HandleAfterRunLeftAirDash() { }

        /// <summary>Called each frame while a rightward air dash is active. Pair with `HandleAfterRunRightAirDash`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunRightAirDash() => true;

        /// <summary>Called after rightward-dash movement is applied. Pair with `HandleRunRightAirDash`.</summary>
        public virtual void HandleAfterRunRightAirDash() { }

        /// <summary>Called each frame while an upward air dash is active. Pair with `HandleAfterRunUpwardDash`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunUpwardDash() => true;

        /// <summary>Called after upward-dash movement is applied. Pair with `HandleRunUpwardDash`.</summary>
        public virtual void HandleAfterRunUpwardDash() { }

        /// <summary>Called each frame to update the airdash animation sprite.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateAirdash() => true;

        /// <summary>Called to play the airdash launch sound.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePlayAidDashSound() => true;

        /// <summary>Called to play the airdash charge-up sound (before release).</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePlayAirDashChargeUpSound() => true;


        /// <summary>Called when the bro begins firing its weapon.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartFiring() => true;

        /// <summary>Called to spawn a projectile each shot. Pair with `HandleAfterFireWeapon`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleFireWeapon() => true;

        /// <summary>Called after a projectile is fired. Pair with `HandleFireWeapon`.</summary>
        public virtual void HandleAfterFireWeapon(float x, float y, float xSpeed, float ySpeed) { }

        /// <summary>Called each frame to run gun-bob and weapon animation.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunGun() => true;

        /// <summary>Called each frame while the bro is in a firing state.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunFiring() => true;

        /// <summary>Called each frame to run avatar (grenade-throw stance) firing logic.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunAvatarFiring() => true;

        /// <summary>Called when the gun is re-enabled after a special or melee animation.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleActivateGun() => true;


        /// <summary>Called when the player presses the melee button and a melee is about to start.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartMelee() => true;

        /// <summary>Called to check whether a new melee can begin. Write to `result` and return false to override.</summary>
        /// <returns>True to use base result, false to use the modified `result`.</returns>
        public virtual bool HandleCanStartNewMelee(ref bool result) => true;

        /// <summary>Called to check whether the bro is locked into a melee animation. Write to `result` and return false to override.</summary>
        /// <returns>True to use base result, false to use the modified `result`.</returns>
        public virtual bool HandleIsLockedInMelee(ref bool result) => true;


        /// <summary>Called each frame to advance the sprite animation. Pair with `HandleAfterChangeFrame`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleChangeFrame() => true;

        /// <summary>Called after the frame counter advances. Pair with `HandleChangeFrame`.</summary>
        public virtual void HandleAfterChangeFrame() { }

        /// <summary>Called after the frame index is incremented within an animation cycle.</summary>
        public virtual void HandleAfterIncreaseFrame() { }

        /// <summary>Called to set the sprite for jumping animation frames.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateActualJumpingFrames() => true;

        /// <summary>Called to set the sprite for jump-ducking animation frames.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateActualJumpingDuckingFrames() => true;

        /// <summary>Called to set the sprite for the idle animation.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateIdle() => true;

        /// <summary>Called after the running-cycle frame is updated.</summary>
        public virtual void HandleAfterAnimateNewRunningFrames() { }

        /// <summary>Called to set the sprite for wall-anticipation animation.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateWallAnticipation() => true;

        /// <summary>Called to set the sprite for wall-climb animation.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateWallClimb() => true;


        /// <summary>Called when the bro takes damage.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY) => true;

        /// <summary>Called when the bro dies. Pair with `HandleAfterDeath`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDeath() => true;

        /// <summary>Called after death logic runs. Pair with `HandleDeath`.</summary>
        public virtual void HandleAfterDeath() { }

        /// <summary>Called when the bro is gibbed (exploded into pieces).</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleGib(DamageType damageType, float xI, float yI) => true;

        /// <summary>Called to check whether this death counts against the player's lives. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to force the provided `result`.</returns>
        public virtual bool HandleCanReduceLives(ref bool result) => true;

        /// <summary>Called to check whether the bro is still alive. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleIsAlive(ref bool result) => true;

        /// <summary>Called when the bro is revived by a teammate. Pair with `HandleAfterRevive`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource) => true;

        /// <summary>Called after revive logic runs. Pair with `HandleRevive`.</summary>
        public virtual void HandleAfterRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource) { }

        /// <summary>Called to broadcast the death type to HUD and scoring systems.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckNotifyDeathType() => true;

        /// <summary>Called to check whether the bro is touching any level traps.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckForTraps() => true;

        /// <summary>Called to check whether ground spikes can impale this bro. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to force the provided `result`.</returns>
        public virtual bool HandleCanBeImpaledByGroundSpikes(ref bool result) => true;

        /// <summary>Called when the bro activates steroid mode. Pair with `HandleAfterUseSteroids`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleUseSteroids() => true;

        /// <summary>Called after steroid activation logic runs. Pair with `HandleUseSteroids`.</summary>
        public virtual void HandleAfterUseSteroids() { }


        /// <summary>Called to check whether the bro is currently in stealth mode.</summary>
        /// <returns>True to run original, false to force stealth mode active.</returns>
        public virtual bool HandleIsInStealthMode() => true;

        /// <summary>Called to alert nearby enemies to the bro's presence.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAlertNearbyMooks() => true;

        /// <summary>Called to check whether this bro can inseminate facehugger-type enemies. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleCanInseminate(ref bool result) => true;

        /// <summary>Called to get the world position that AI companions follow. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleGetFollowPosition(ref Vector3 result) => true;

        /// <summary>Called when the bro is recalled to its owner (companion recall mechanic). Pair with `HandleAfterRecallBro`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRecallBro() => true;

        /// <summary>Called after recall logic runs. Pair with `HandleRecallBro`.</summary>
        public virtual void HandleAfterRecallBro() { }

        /// <summary>Called when the bro attaches to a helicopter.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAttachToHeli() => true;

        /// <summary>Called when the bro begins piloting a vehicle unit.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartPilotingUnit() => true;

        /// <summary>Called after the bro exits a piloted vehicle.</summary>
        public virtual void HandleAfterDischargePilotingUnit() { }

        /// <summary>Called when the bro's GameObject is about to be destroyed.</summary>
        public virtual void HandleDestroyUnit() { }

        /// <summary>Called when the bro throws back a nearby mook during a high-five-gated melee. Return false to skip base behavior.</summary>
        public virtual bool HandleThrowBackMook(Mook mook) { return true; }
        #endregion
    }
}

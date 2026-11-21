using BroMakerLib.CustomObjects;
using BroMakerLib.Loggers;
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

        [JsonIgnore] public BroBase owner;

        [JsonIgnore] protected IAbilityOwner hero;

        [JsonIgnore] protected int PlayerNum => owner.playerNum;

        /// <summary>Facing direction: 1 = right, -1 = left.</summary>
        [JsonIgnore]
        protected float Direction => Mathf.Sign(owner.transform.localScale.x);

        [JsonIgnore] protected float X => owner.X;

        [JsonIgnore] protected float Y => owner.Y;

        [JsonIgnore] protected Sound sound => hero.Sound;

        /// <summary>Called once when the bro spawns.</summary>
        public virtual void Initialize(BroBase owner)
        {
            this.owner = owner;
            hero = owner as IAbilityOwner;
            if (hero == null)
            {
                string kind = this is SpecialAbility ? "Special" : this is MeleeAbility ? "Melee" : this is PassiveAbility ? "Passive" : "Ability";
                string preset = ((CustomObjectPresetAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(CustomObjectPresetAttribute)))?.name ?? GetType().Name;
                string bro = (owner as ICustomHero)?.Info?.name ?? owner.name;
                BMLogger.Error($"{kind} preset '{preset}' won't work on '{bro}' because that bro doesn't support abilities. Use a vanilla preset as its characterPreset");
            }
        }

        /// <summary>Called every frame, after the owner's own `Update` step.</summary>
        public virtual void Update()
        {
        }

        /// <summary>Called when this ability is detached from the bro: replaced, removed, or rejected after `Initialize`, including one whose `Initialize` returned early.</summary>
        public virtual void Cleanup()
        {
        }

        [JsonIgnore] private OwnerSpeedState ownSpeedState;

        private OwnerSpeedState SpeedState => hero != null ? hero.SpeedState : ownSpeedState ?? (ownSpeedState = new OwnerSpeedState(owner));

        /// <summary>Starts this ability's hold on the owner's `speed` with a multiplier of 1, unless it already holds one, so `RestoreOwnerSpeed` later returns the owner to its unchanged speed times the other holds.</summary>
        protected void SaveOwnerSpeed()
        {
            SpeedState.Save(this);
        }

        /// <summary>Sets this ability's speed multiplier, starting its hold if it has none, and sets the owner's `speed` to its unchanged speed times the multipliers of every holding ability.</summary>
        protected void ChangeOwnerSpeed(float multiplier)
        {
            SpeedState.Change(this, multiplier);
        }

        /// <summary>Ends this ability's hold and sets the owner's `speed` to its unchanged speed times the multipliers of the remaining holds; does nothing when this ability holds none.</summary>
        protected void RestoreOwnerSpeed()
        {
            SpeedState.Restore(this);
        }

        /// <summary>The owner's `speed` without the multipliers abilities hold; a `speed` written by anything else becomes the unchanged speed.</summary>
        protected float GetUnchangedOwnerSpeed()
        {
            return SpeedState.UnchangedSpeed;
        }

        internal void Detach()
        {
            Cleanup();
            if (owner != null)
            {
                RestoreOwnerSpeed();
            }
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

        /// <summary>Called instead of `HandleSetDeltaTime` while a high-five or time boost is running. On a Time Bro host `HandleSetDeltaTime` runs as well.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleSetHighFiveBoostDeltaTime() => true;

        /// <summary>Called when the bro's GameObject is being destroyed, before the base teardown; returning false skips that teardown, including the bro's removal from the game's bro list.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleOnDestroy() => true;


        /// <summary>Called before the bro reads and processes its input state.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckInput() => true;

        /// <summary>Called after the bro reads and processes its input state.</summary>
        public virtual void HandleAfterCheckInput() { }

        /// <summary>Called after this bro's input is copied to a zombie it revived, which the zombie does each frame.</summary>
        public virtual void HandleAfterCopyInput(TestVanDammeAnim zombie, ref float zombieDelay, ref bool up, ref bool down, ref bool left, ref bool right, ref bool fire, ref bool buttonJump) { }

        /// <summary>
        /// Called when the melee button is pressed, or held long enough to raise a high-five, to check whether the bro ignores it.
        /// Write into `result` and return false to override the wrapper's return value.
        /// </summary>
        public virtual bool HandleMustIgnoreHighFiveMeleePress(ref bool result) => true;

        /// <summary>Called when the player presses the special button, unless the press goes to a pocketed special.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePressSpecial() => true;

        /// <summary>Called when the player releases the special button.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleReleaseSpecial() => true;

        /// <summary>Called when the player presses the melee/high-five button, or holds it long enough to raise a high-five with `forceHighFive` true. Write to `forceHighFive` to change what the base method receives.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePressHighFiveMelee(ref bool forceHighFive) => true;

        /// <summary>Called each frame the dash button is held, on the client that controls the bro. `hero.WasDashButton` is false on the first of those frames.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePressDashButton() => true;


        /// <summary>Called each frame to compute the bro's velocity. Write to `xI`/`yI` to override; pair with `HandleAfterCalculateMovement`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCalculateMovement(ref float xI, ref float yI) => true;

        /// <summary>Called after velocity is computed. Pair with `HandleCalculateMovement`.</summary>
        public virtual void HandleAfterCalculateMovement() { }

        /// <summary>Called each frame to run the bro's movement step: its gravity, its current movement state, and collision with ceilings, walls and floors.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunMovement() => true;

        /// <summary>Called when the bro jumps. `wallJump` is true for wall jumps.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleJump(bool wallJump) => true;

        /// <summary>Called when a new jump press finds no ground jump, wall jump or jetpack to use, whether or not the bro can air jump.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAirJump() => true;

        /// <summary>Called each frame the game applies its default falling gravity, which it does with no parachute open and outside quicksand.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyNormalGravity() => true;

        /// <summary>Called each frame to apply falling gravity.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyFallingGravity() => true;

        /// <summary>Called each frame the bro falls with `yI` below -50, alive or dead, except during a melee, air dash, ladder climb, hang or zipline ride.</summary>
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

        /// <summary>Called when the bro is knocked back by an explosion, a hit or another push. Pair with `HandleAfterKnock`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleKnock(DamageType damageType, float xI, float yI, bool forceTumble) => true;

        /// <summary>Called after the knockback is applied. Pair with `HandleKnock`.</summary>
        public virtual void HandleAfterKnock(DamageType damageType, float xI, float yI, bool forceTumble) { }


        /// <summary>Called each frame the bro isn't moving upward, to stop it passing through the floor.</summary>
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

        /// <summary>Called each frame the bro's movement would carry its head into a ceiling, up to twice in one frame. Pair with `HandleAfterHitCeiling`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitCeiling() => true;

        /// <summary>Called after ceiling-hit logic runs. Pair with `HandleHitCeiling`.</summary>
        public virtual void HandleAfterHitCeiling() { }

        /// <summary>Called each frame the bro's movement would carry it into a wall on its left. Pair with `HandleAfterHitLeftWall`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitLeftWall() => true;

        /// <summary>Called after left-wall-hit logic runs. Pair with `HandleHitLeftWall`.</summary>
        public virtual void HandleAfterHitLeftWall() { }

        /// <summary>Called each frame the bro's movement would carry it into a wall on its right. Pair with `HandleAfterHitRightWall`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitRightWall() => true;

        /// <summary>Called after right-wall-hit logic runs. Pair with `HandleHitRightWall`.</summary>
        public virtual void HandleAfterHitRightWall() { }

        /// <summary>Called when the bro lands on the ground outside quicksand, and each frame it lies on the ground dead. Pair with `HandleAfterLand`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleLand() => true;

        /// <summary>Called after landing logic runs. Pair with `HandleLand`.</summary>
        public virtual void HandleAfterLand() { }

        /// <summary>Called when a hard landing or a down-dash would start the landing roll.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRollOnLand() => true;

        /// <summary>Called each frame to handle wall-drag state. `value` is the requested drag state.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleWallDrag(bool value) => true;

        /// <summary>Called when determining which physics layer to treat as ground. Write to `result` and return false to override.</summary>
        /// <returns>True to use base result, false to use the modified `result`.</returns>
        public virtual bool HandleGetGroundLayer(ref int result) => true;

        /// <summary>Called twice each frame, in the movement step and in `LateUpdate`, to check whether the bro is in quicksand.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckForQuicksand() => true;

        /// <summary>Called after the quicksand check runs. Pair with `HandleCheckForQuicksand`.</summary>
        public virtual void HandleAfterCheckForQuicksand() { }


        /// <summary>Called to check whether the bro is positioned over a ladder.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleIsOverLadder() => true;

        /// <summary>Called each frame while the bro hangs from a ceiling.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunHanging() => true;

        /// <summary>Called to determine whether ceiling-climb is possible. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleCanCheckClimbAlongCeiling(ref bool result) => true;

        /// <summary>Called each frame to check whether a jumping bro that has just moved past a wall's edge grabs the ceiling above to hang from it.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckClimbAlongCeiling() => true;

        /// <summary>Called each frame a jumping bro holds toward a wall whose top is below its head, unless down is held, to lift it onto the ledge.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleLedgeGrapple(bool left, bool right, float radius, float heightOpenOffset) => true;


        /// <summary>Called when the bro initiates a downward air dash. Pair with `HandleAfterAirDashUp` for the upward variant.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAirDashDown() => true;

        /// <summary>Called to check whether the bro may air dash in a given direction. Write to `result` and return false to override.</summary>
        /// <returns>True to use base result, false to use the modified `result`.</returns>
        public virtual bool HandleCanAirDash(DirectionEnum direction, ref bool result) => true;

        /// <summary>Called after the bro starts an upward air dash. Pair with `HandleAirDashDown` for the downward variant.</summary>
        public virtual void HandleAfterAirDashUp() { }

        /// <summary>Called when the bro initiates a leftward air dash. Pair with `HandleAirDashRight` for the rightward variant.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAirDashLeft() => true;

        /// <summary>Called when the bro initiates a rightward air dash. Pair with `HandleAirDashLeft` for the leftward variant.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAirDashRight() => true;

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

        /// <summary>Called to set the sprite for the airdash animation.</summary>
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

        /// <summary>Called when the bro uses its weapon, before the shot is fired. Pair with `HandleAfterUseFire`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleUseFire() => true;

        /// <summary>Called after the weapon is used. Pair with `HandleUseFire`.</summary>
        public virtual void HandleAfterUseFire() { }

        /// <summary>Called to spawn a projectile each shot. Pair with `HandleAfterFireWeapon`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleFireWeapon() => true;

        /// <summary>Called after a projectile is fired. Pair with `HandleFireWeapon`.</summary>
        public virtual void HandleAfterFireWeapon(float x, float y, float xSpeed, float ySpeed) { }

        /// <summary>Called each frame to advance the gun sprite's firing animation. Pair with `HandleAfterRunGun`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunGun() => true;

        /// <summary>Called after the gun sprite's firing animation advances. Pair with `HandleRunGun`.</summary>
        public virtual void HandleAfterRunGun() { }

        /// <summary>Called each frame before the bro's own fire loop. Pair with `HandleAfterRunFiring`. When the loop runs again after a skip, it sees a press or release if `fire` changed since it last ran.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunFiring() => true;

        /// <summary>Called after the firing state runs. Pair with `HandleRunFiring`.</summary>
        public virtual void HandleAfterRunFiring() { }

        /// <summary>Called each frame to update the HUD avatar's face for firing.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunAvatarFiring() => true;

        /// <summary>Called each time the gun sprite is shown, which most of the bro's animations do on every animation frame.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleActivateGun() => true;

        /// <summary>Called when the gun sprite is positioned. Write to `xOffset`/`yOffset` to change the offsets the base method receives.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleSetGunPosition(ref float xOffset, ref float yOffset) => true;


        /// <summary>Called when a melee is about to start, from the melee button or from the bro's own code as Xebro's attack does, and `hero.MeleeFromButton` tells which.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartMelee() => true;

        /// <summary>Called after the melee type is resolved, when the bro's own melee attack is about to start. Not called for the knife swing vanilla reroutes to while standing on an animal.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartMeleeAttack() => true;

        /// <summary>Called to check whether a new melee can begin. Write to `result` and return false to override.</summary>
        /// <returns>True to use base result, false to use the modified `result`.</returns>
        public virtual bool HandleCanStartNewMelee(ref bool result) => true;

        /// <summary>Called to check whether the bro is locked into a melee animation. Write to `result` and return false to override.</summary>
        /// <returns>True to use base result, false to use the modified `result`.</returns>
        public virtual bool HandleIsLockedInMelee(ref bool result) => true;

        /// <summary>Called each frame while the bro's `canDoIndependentMeleeAnimation` is set, whether or not a melee is running.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunIndependentMeleeFrames() => true;

        /// <summary>Called after melee state is reset. Use for ability-specific state that should track vanilla's ResetMeleeValues contract.</summary>
        public virtual void HandleAfterResetMeleeValues() { }


        /// <summary>Called on each animation tick, and whenever the game refreshes the bro's sprite between ticks, to set the sprite for the current frame. Pair with `HandleAfterChangeFrame`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleChangeFrame() => true;

        /// <summary>Called after the sprite is set for the current frame. Pair with `HandleChangeFrame`.</summary>
        public virtual void HandleAfterChangeFrame() { }

        /// <summary>Called after the frame index is incremented within an animation cycle.</summary>
        public virtual void HandleAfterIncreaseFrame() { }

        /// <summary>Called to set the sprite for the jumping animation, before the ducking/zipline/ledge branches are chosen.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateJumping() => true;

        /// <summary>Called to set the sprite for jumping animation frames.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateActualJumpingFrames() => true;

        /// <summary>Called to set the sprite for jump-ducking animation frames.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateActualJumpingDuckingFrames() => true;

        /// <summary>Called on each animation frame of a bro in the idle state, before the game picks the standing melee, special, high-five, throw, roll, gesture or dance animation or the idle frames, so skipping it skips whichever was due. `HandleAnimateActualIdleFrames` replaces only the idle frames.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateIdle() => true;

        /// <summary>Called to set the sprite for standing idle frames, which the idle animation reaches when the bro isn't ducking and no melee, special, gesture, roll or dance animation is active, and which a bro holding still on a ladder can also show.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateActualIdleFrames() => true;

        /// <summary>Called after the running-cycle frame is updated.</summary>
        public virtual void HandleAfterAnimateNewRunningFrames() { }

        /// <summary>Called to set the sprite for wall-anticipation animation.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateWallAnticipation() => true;

        /// <summary>Called to set the sprite for wall-climb animation. The base method also gives the climb its upward kicks, so skipping it slows the climb to a crawl.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateWallClimb() => true;


        /// <summary>Called when the bro takes damage.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY) => true;

        /// <summary>Called when the bro is stopped: interrupted mid-action by a stun, freeze or blind, or ending its own special.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStop() => true;

        /// <summary>Called when the bro dies. Pair with `HandleAfterDeath`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDeath(float xI, float yI, DamageObject damage) => true;

        /// <summary>Called after death logic runs. Pair with `HandleDeath`.</summary>
        public virtual void HandleAfterDeath(float xI, float yI, DamageObject damage) { }

        /// <summary>Called when the bro is gibbed (exploded into pieces).</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleGib(DamageType damageType, float xI, float yI) => true;

        /// <summary>Called to check whether this death counts against the player's lives. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to force the provided `result`.</returns>
        public virtual bool HandleCanReduceLives(ref bool result) => true;

        /// <summary>Called when the bro dies or is gibbed and `CanReduceLives` allows a life to be lost, which can happen twice for one death and on clients that don't control the bro. Only the first call on the client that controls the bro can cost the player a life.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleReduceLives(bool destroyed) => true;

        /// <summary>Called to check whether the bro is still alive. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleIsAlive(ref bool result) => true;

        /// <summary>Called when a revive reaches this bro while it is dead, the bro's own revive included, before the game decides whether it comes back to life. Pair with `HandleAfterRevive`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource) => true;

        /// <summary>Called after revive logic runs. `result` is what the base method returned. Pair with `HandleRevive`.</summary>
        public virtual void HandleAfterRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource, bool result) { }

        /// <summary>Called each frame after the bro dies until its death type is recorded in the level statistics.</summary>
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

        /// <summary>Called 15 times a second while the bro's steroids last, and every fourth call panics nearby enemies.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandlePerformanceEnhancedGrunt() => true;

        /// <summary>Called when an ammo pickup or a pocketed-special use refills the bro's special ammo.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleResetSpecialAmmo() => true;

        /// <summary>Called to check whether the bro's special ammo is full, which makes an ammo pickup refuse to be collected. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleIsAmmoFull(ref bool result) => true;


        /// <summary>
        /// Called to check whether the bro is currently in stealth mode.
        /// Write into `result` and return false to override the wrapper's return value.
        /// </summary>
        public virtual bool HandleIsInStealthMode(ref bool result) => true;

        /// <summary>Called to alert nearby enemies to the bro's presence.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAlertNearbyMooks() => true;

        /// <summary>Called to check whether this bro can be inseminated by a facehugger. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleCanInseminate(ref bool result) => true;

        /// <summary>Called to get the world position the camera follows for this bro. Write to `result` and return false to override.</summary>
        /// <returns>True to run original, false to use the provided `result`.</returns>
        public virtual bool HandleGetFollowPosition(ref Vector3 result) => true;

        /// <summary>Called when the bro is recalled, as the game does to a player's living bro when it spawns the next one. Pair with `HandleAfterRecallBro`.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRecallBro() => true;

        /// <summary>Called after recall logic runs. Pair with `HandleRecallBro`.</summary>
        public virtual void HandleAfterRecallBro() { }

        /// <summary>Called when the bro attaches to a helicopter.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAttachToHeli() => true;

        /// <summary>Called after the bro has attached to a helicopter.</summary>
        public virtual void HandleAfterAttachToHeli() { }

        /// <summary>Called when the bro begins piloting a vehicle unit.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartPilotingUnit() => true;

        /// <summary>Called after the bro exits a piloted vehicle, with the discharge parameters.</summary>
        public virtual void HandleAfterDischargePilotingUnit(float x, float y, float xI, float yI, bool stunPilot) { }

        /// <summary>Called when the bro's GameObject is about to be destroyed.</summary>
        public virtual void HandleAfterDestroyUnit() { }

        /// <summary>Called when the bro throws back a nearby mook during a high-five-gated melee. Return false to skip base behavior.</summary>
        public virtual bool HandleThrowBackMook(Mook mook) { return true; }

        /// <summary>Called each time a melee button press is handled, to pick the nearby mook a melee can grab and throw.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleFindNearbyMook() => true;

        /// <summary>Called after the base velocity for a thrown mook is computed. Write to `xI`/`yI` to change it; return false to stop later abilities from adjusting it further.</summary>
        /// <returns>True to let the remaining abilities adjust the velocity, false to stop.</returns>
        public virtual bool HandleSetupThrownMookVelocity(ref float xI, ref float yI) => true;
        #endregion
    }
}

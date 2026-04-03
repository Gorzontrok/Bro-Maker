using BroMakerLib.CustomObjects;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    public abstract class SpecialAbility
    {
        [JsonIgnore]
        public TestVanDammeAnim owner;

        [JsonIgnore]
        protected ICustomHero hero;

        [JsonIgnore]
        protected int PlayerNum => owner.playerNum;

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

        public int animationRow = 5;
        public int animationColumn = 17;
        public int animationFrameCount = 8;
        public int triggerFrame = 4;
        public float frameRate = 0.0334f;
        public float spawnOffsetX = 0f;
        public float spawnOffsetY = 0f;
        public bool instantUse = false;
        public bool blockMovement = false;
        public bool deactivateGun = true;

        public AudioClip[] throwSounds;
        public AudioClip[] attackSounds;
        public AudioClip[] specialAttackSounds;

        [JsonIgnore]
        public bool IsActive { get; protected set; }

        public virtual void Initialize(TestVanDammeAnim owner)
        {
            this.owner = owner;
            this.hero = owner as ICustomHero;
        }

        public virtual void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.DoingMelee)
            {
                return;
            }

            if (instantUse)
            {
                UseSpecial();
                return;
            }

            hero.UsingSpecial = true;
            owner.frame = 0;
            hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
        }

        public virtual void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);

            if (deactivateGun)
            {
                hero.DeactivateGun();
            }

            hero.FrameRate = frameRate;

            int column = animationColumn + Mathf.Clamp(owner.frame, 0, animationFrameCount - 1);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, animationRow * hero.SpritePixelHeight);

            if (owner.frame == triggerFrame)
            {
                UseSpecial();
            }

            if (owner.frame >= animationFrameCount - 1)
            {
                owner.frame = 0;
                hero.UsingSpecial = false;
                hero.UsingPockettedSpecial = false;
                hero.ActivateGun();
                hero.ChangeFrame();
            }
        }

        public virtual void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                hero.TriggerBroSpecialEvent();
                ActivateSpecial();
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }

        public virtual void ActivateSpecial()
        {
        }

        /// <summary>Called when the bro dies.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDeath()
        {
            return true;
        }

        /// <summary>Called after Death has run.</summary>
        public virtual void HandleAfterDeath()
        {
        }

        /// <summary>Called during CanReduceLives check.</summary>
        /// <returns>True to run original, false to force the provided result.</returns>
        public virtual bool HandleCanReduceLives(ref bool result)
        {
            return true;
        }

        /// <summary>Called before the bro's base Update. Return false to skip base.Update() entirely.</summary>
        public virtual bool HandleUpdate()
        {
            return true;
        }

        public virtual void Update()
        {
        }

        /// <summary>Called during movement processing.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            if (blockMovement && hero.UsingSpecial)
            {
                xI = 0f;
                yI = 0f;
                return false;
            }
            return true;
        }

        /// <summary>Called after CalculateMovement has run.</summary>
        public virtual void HandleAfterCalculateMovement()
        {
        }

        /// <summary>Called when the bro takes damage.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            return true;
        }

        /// <summary>Called during firing logic.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleFireWeapon()
        {
            return true;
        }

        /// <summary>Called after FireWeapon has run.</summary>
        public virtual void HandleAfterFireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
        }

        /// <summary>Called after AddSpeedLeft has run.</summary>
        public virtual void HandleAfterAddSpeedLeft()
        {
        }

        /// <summary>Called after AddSpeedRight has run.</summary>
        public virtual void HandleAfterAddSpeedRight()
        {
        }

        /// <summary>Called during CanBeImpaledByGroundSpikes check.</summary>
        /// <returns>True to run original, false to force the provided result.</returns>
        public virtual bool HandleCanBeImpaledByGroundSpikes(ref bool result)
        {
            return true;
        }

        /// <summary>Called when the bro jumps.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleJump(bool wallJump)
        {
            return true;
        }

        /// <summary>Called during RunMovement.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunMovement()
        {
            return true;
        }

        /// <summary>Called during gravity application.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyNormalGravity()
        {
            return true;
        }

        /// <summary>Called when the bro starts firing.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartFiring()
        {
            return true;
        }

        /// <summary>Called when the bro starts a melee attack.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartMelee()
        {
            return true;
        }

        /// <summary>Called during ChangeFrame.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleChangeFrame()
        {
            return true;
        }

        /// <summary>Called during RunGun.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunGun()
        {
            return true;
        }

        /// <summary>Called during RunFiring.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunFiring()
        {
            return true;
        }

        /// <summary>Called during RunAvatarFiring.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunAvatarFiring()
        {
            return true;
        }

        /// <summary>Called during floor constraint check.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToFloor()
        {
            return true;
        }

        /// <summary>Called during ceiling constraint check.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToCeiling()
        {
            return true;
        }

        /// <summary>Called during wall constraint check.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleConstrainToWalls()
        {
            return true;
        }

        /// <summary>Called during IsOverLadder check.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleIsOverLadder()
        {
            return true;
        }

        /// <summary>Called when WallDrag is being set.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleWallDrag(bool value)
        {
            return true;
        }

        /// <summary>Called during jumping frame animation.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAnimateActualJumpingFrames()
        {
            return true;
        }

        /// <summary>Called before Land.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleLand()
        {
            return true;
        }

        /// <summary>Called after Land has run.</summary>
        public virtual void HandleAfterLand()
        {
        }

        /// <summary>Called after ChangeFrame has run.</summary>
        public virtual void HandleAfterChangeFrame()
        {
        }

        /// <summary>Called after IncreaseFrame has run (once per animation frame advance).</summary>
        public virtual void HandleAfterIncreaseFrame()
        {
        }

        /// <summary>Called when the special button is released.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleReleaseSpecial()
        {
            return true;
        }

        /// <summary>Called during MustIgnoreHighFiveMeleePress to allow side effects before the base check runs.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleMustIgnoreHighFiveMeleePress()
        {
            return true;
        }

        /// <summary>Called after AnimateActualNewRunningFrames has run.</summary>
        public virtual void HandleAfterAnimateNewRunningFrames()
        {
        }

        /// <summary>Called when the camera queries the bro's follow position.</summary>
        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleGetFollowPosition(ref Vector3 result)
        {
            return true;
        }

        /// <summary>Called during IsInStealthMode check.</summary>
        /// <returns>True to run original, false to force stealth mode active.</returns>
        public virtual bool HandleIsInStealthMode()
        {
            return true;
        }

        /// <summary>Called during AlertNearbyMooks.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAlertNearbyMooks()
        {
            return true;
        }

        /// <summary>Called when the bro is gibbed.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleGib(DamageType damageType, float xI, float yI)
        {
            return true;
        }

        /// <summary>Called when the bro is recalled (e.g., level extraction).</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRecallBro()
        {
            return true;
        }

        /// <summary>Called after RecallBro has run.</summary>
        public virtual void HandleAfterRecallBro()
        {
        }

        /// <summary>Called when the bro attaches to the extraction helicopter.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAttachToHeli()
        {
            return true;
        }

        /// <summary>Called when the bro hits a ceiling.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitCeiling()
        {
            return true;
        }

        /// <summary>Called after HitCeiling has run.</summary>
        public virtual void HandleAfterHitCeiling()
        {
        }

        /// <summary>Called when the bro hits a left wall.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitLeftWall()
        {
            return true;
        }

        /// <summary>Called after HitLeftWall has run.</summary>
        public virtual void HandleAfterHitLeftWall()
        {
        }

        /// <summary>Called when the bro hits a right wall.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleHitRightWall()
        {
            return true;
        }

        /// <summary>Called after HitRightWall has run.</summary>
        public virtual void HandleAfterHitRightWall()
        {
        }

        /// <summary>Called during wall drag Y velocity clamping.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleClampWallDragYI(ref float yIT)
        {
            return true;
        }

        /// <summary>Called during RunHanging.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunHanging()
        {
            return true;
        }

        /// <summary>Called during CanCheckClimbAlongCeiling.</summary>
        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleCanCheckClimbAlongCeiling(ref bool result)
        {
            return true;
        }

        /// <summary>Called during CheckClimbAlongCeiling.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckClimbAlongCeiling()
        {
            return true;
        }

        /// <summary>Called after CheckInput has run.</summary>
        public virtual void HandleAfterCheckInput()
        {
        }

        /// <summary>Called during AirDashDown.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAirDashDown()
        {
            return true;
        }

        /// <summary>Called during RunDownwardDash.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRunDownwardDash()
        {
            return true;
        }

        /// <summary>Called after RunDownwardDash has run.</summary>
        public virtual void HandleAfterRunDownwardDash()
        {
        }

        /// <summary>Called during IsAlive check.</summary>
        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleIsAlive(ref bool result)
        {
            return true;
        }

        /// <summary>Called during Revive.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
            return true;
        }

        /// <summary>Called after Revive has run.</summary>
        public virtual void HandleAfterRevive(int playerNum, bool isUnderPlayerControl, TestVanDammeAnim reviveSource)
        {
        }

        /// <summary>Called during UseSteroids.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleUseSteroids()
        {
            return true;
        }

        /// <summary>Called after UseSteroids has run.</summary>
        public virtual void HandleAfterUseSteroids()
        {
        }

        /// <summary>Called during CheckNotifyDeathType.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckNotifyDeathType()
        {
            return true;
        }

        /// <summary>Called during ApplyFallingGravity.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyFallingGravity()
        {
            return true;
        }

        /// <summary>Called during SetDeltaTime.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleSetDeltaTime()
        {
            return true;
        }

        /// <summary>Called during CanInseminate check.</summary>
        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleCanInseminate(ref bool result)
        {
            return true;
        }

        /// <summary>Called when the bro starts piloting a unit.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartPilotingUnit()
        {
            return true;
        }

        /// <summary>Called during LateUpdate.</summary>
        public virtual void HandleLateUpdate()
        {
        }

        /// <summary>Called during CheckForTraps.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleCheckForTraps()
        {
            return true;
        }

        /// <summary>Called before this ability is replaced by another. Override to destroy any
        /// components or child objects created during Initialize.</summary>
        public virtual void Cleanup()
        {
        }
    }
}

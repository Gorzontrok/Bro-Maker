using System;
using BroMakerLib.CustomObjects;
using BroMakerLib.Loggers;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>Base class for melee abilities, whose public fields form the JSON parameter schema.</summary>
    public abstract class MeleeAbility : AbilityBase
    {
        /// <summary>Vanilla bro to source melee sounds from.</summary>
        protected virtual HeroType SourceBroType => HeroType.Rambro;

        /// <summary>If true, `StartMelee` starts the melee the way the game starts a custom melee rather than a knife melee.</summary>
        [JsonIgnore]
        public bool customStart = false;

        /// <summary>If true, `RunMeleeMovement` moves the bro with punch movement rather than knife movement.</summary>
        [JsonIgnore]
        public bool punchMovement = false;

        /// <summary>Sprite sheet row for jumping/dashing melee.</summary>
        public int jumpingAnimationRow = 6;

        /// <summary>Starting column for jumping/dashing melee.</summary>
        public int jumpingAnimationColumn = 17;

        /// <summary>Which animation frame triggers the hit detection.</summary>
        public int hitFrame = 3;

        /// <summary>Frame after which re-pressing melee restarts the animation.</summary>
        public int restartFrame = 3;

        /// <summary>Frame at which the melee animation ends and `CancelMelee` is called.</summary>
        public int endFrame = 6;

        /// <summary>Extra time subtracted from the frame counter on `hitFrame`, creating a brief pause on the attack frame.</summary>
        public float hitFrameCounterDelay = 0.066f;

        /// <summary>Whether to continue calling `PerformMeleeAttack` on frames after `hitFrame` if the attack hasn't hit yet.</summary>
        public bool attackOnFollowUpFrames = true;

        /// <summary>Damage dealt to units on hit.</summary>
        public int damage = 4;

        /// <summary>`DamageType` enum name controlling the unit-hit damage type.</summary>
        public string damageType = "Knifed";

        [JsonIgnore] protected DamageType parsedDamageType = DamageType.Knifed;

        /// <summary>Horizontal knockback force applied to hit units.</summary>
        public float knockbackX = 200f;

        /// <summary>Vertical knockback force applied to hit units.</summary>
        public float knockbackY = 500f;

        /// <summary>Horizontal range of the hit detection box.</summary>
        public float hitRangeX = 14f;

        /// <summary>Vertical range of the hit detection box.</summary>
        public float hitRangeY = 24f;

        /// <summary>X offset from owner position for the hit detection center.</summary>
        public float hitOffsetX = 8f;

        /// <summary>Y offset from owner position for the hit detection center.</summary>
        public float hitOffsetY = 8f;

        /// <summary>Whether melee damages terrain and doors.</summary>
        public bool hitTerrain = true;

        /// <summary>Damage dealt to terrain blocks on hit.</summary>
        public int terrainDamage = 2;

        /// <summary>Controls BroBase dispatch routing and TryMeleeTerrain sounds.</summary>
        [JsonIgnore]
        public BroBase.MeleeType meleeType = BroBase.MeleeType.Knife;

        /// <summary>When true, a `customStart` start sets `DashingMelee = true` and pushes `xI` toward the target.</summary>
        [JsonIgnore]
        public bool forceDashingOnCustomStart = false;

        /// <summary>Sounds played when the melee strike connects with a unit.</summary>
        public AudioClip[] meleeHitSounds;

        /// <summary>Sounds played when the melee swing misses.</summary>
        public AudioClip[] missSounds;

        /// <summary>Sounds played when melee hits terrain (knife-type melees).</summary>
        public AudioClip[] meleeHitTerrainSounds;

        /// <summary>Alternate hit sounds used by non-knife melees and terrain hits.</summary>
        public AudioClip[] alternateMeleeHitSounds;

        /// <summary>Called once when the bro spawns. Parses `damageType`, then calls `CacheSoundsFromPrefab`.</summary>
        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            if (string.IsNullOrEmpty(damageType))
            {
                parsedDamageType = DamageType.Knifed;
            }
            else
            {
                try
                {
                    parsedDamageType = (DamageType)Enum.Parse(typeof(DamageType), damageType, true);
                }
                catch
                {
                    BMLogger.Warning($"Unknown damageType '{damageType}' in the 'melee' ability config, using 'Knifed' instead", bro: (owner as ICustomHero)?.Info?.name);
                    parsedDamageType = DamageType.Knifed;
                }
            }
            CacheSoundsFromPrefab();
        }

        /// <summary>Loads melee sound clips from the `SourceBroType` prefab's soundHolder, null-guarding each array so JSON overrides aren't stomped; call `base.CacheSoundsFromPrefab()` first.</summary>
        protected virtual void CacheSoundsFromPrefab()
        {
            TestVanDammeAnim sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null)
            {
                return;
            }

            if (meleeHitSounds == null)
            {
                meleeHitSounds = sourceBro.soundHolder.meleeHitSound.CloneArray();
            }
            if (missSounds == null)
            {
                missSounds = sourceBro.soundHolder.missSounds.CloneArray();
            }
            if (meleeHitTerrainSounds == null)
            {
                meleeHitTerrainSounds = sourceBro.soundHolder.meleeHitTerrainSound.CloneArray();
            }
            if (alternateMeleeHitSounds == null)
            {
                alternateMeleeHitSounds = sourceBro.soundHolder.alternateMeleeHitSound.CloneArray();
            }
        }

        /// <summary>Whether this ability runs a melee of the given type, started by the melee button or by the owner's own code. Return false to leave that melee to the owner. By default it takes every melee-button melee except a knife swing the owner falls back to when this ability is not a knife melee.</summary>
        public virtual bool HandlesMeleeType(BroBase.MeleeType currentMeleeType, bool fromMeleeButton)
        {
            return fromMeleeButton && (currentMeleeType != BroBase.MeleeType.Knife || meleeType == BroBase.MeleeType.Knife);
        }

        /// <summary>Runs the base game's search for the mook a melee press can throw back, so the melee finds one on every owner; return true to leave the search to the owner.</summary>
        public override bool HandleFindNearbyMook()
        {
            FindNearbyMook();
            return false;
        }

        /// <summary>Sets `hero.NearbyMook` as the base game's melee press does: the mook on the owner's back, else the mook found just around the owner.</summary>
        protected void FindNearbyMook()
        {
            if (owner.skinnedMookOnMyBack != null)
            {
                hero.NearbyMook = owner.skinnedMookOnMyBack;
            }
            else
            {
                hero.NearbyMook = Map.GetNearbyMook(30f, 12f, X - (owner.transform.localScale.x * 12f), Y, (int)owner.transform.localScale.x);
            }
        }

        /// <summary>Called when the player presses melee. Behavior depends on `customStart`.</summary>
        public virtual void StartMelee()
        {
            if (!customStart)
            {
                hero.ShowHighFiveAfterMeleeTimer = 0f;
                hero.JumpTime = 0f;
                hero.DeactivateGun();
                hero.SetMeleeType();
                hero.MeleeHasHit = false;
                if (!hero.DoingMelee || owner.frame > restartFrame)
                {
                    owner.frame = 0;
                    owner.counter = -0.05f;
                    AnimateMelee();
                }
                else if (hero.DoingMelee)
                {
                    hero.MeleeFollowUp = true;
                }
                hero.DoingMelee = true;
            }
            else
            {
                if (!hero.DoingMelee || owner.frame > restartFrame)
                {
                    if (hero.DoingMelee)
                    {
                        owner.frame = 1;
                    }
                    else
                    {
                        owner.frame = 0;
                    }
                    owner.counter = -0.05f;
                    AnimateMelee();
                }
                if (forceDashingOnCustomStart && !hero.JumpingMelee)
                {
                    hero.DashingMelee = true;
                    owner.xI = (float)owner.Direction * owner.speed;
                }
                hero.StartMeleeCommon();
            }
        }

        /// <summary>Called each frame during the melee animation.</summary>
        public virtual void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            hero.FrameRate = frameRate;
            int col = animationColumn + Mathf.Clamp(owner.frame, 0, animationFrameCount - 1);
            int row = animationRow;
            if (!hero.StandingMelee)
            {
                if (hero.JumpingMelee)
                {
                    col = jumpingAnimationColumn + Mathf.Clamp(owner.frame, 0, animationFrameCount - 1);
                    row = jumpingAnimationRow;
                }
                else if (hero.DashingMelee)
                {
                    col = jumpingAnimationColumn + Mathf.Clamp(owner.frame, 0, animationFrameCount - 1);
                    row = jumpingAnimationRow;
                    if (owner.frame == 4)
                    {
                        owner.counter -= 0.0334f;
                    }
                    else if (owner.frame == 5)
                    {
                        owner.counter -= 0.0334f;
                    }
                }
            }
            hero.Sprite.SetLowerLeftPixel((float)(col * hero.SpritePixelWidth), (float)(row * hero.SpritePixelHeight));
            if (owner.frame == hitFrame)
            {
                owner.counter -= hitFrameCounterDelay;
                PerformMeleeAttack(true, true);
            }
            else if (attackOnFollowUpFrames && owner.frame > hitFrame && !hero.MeleeHasHit)
            {
                PerformMeleeAttack(false, false);
            }
            if (owner.frame >= endFrame)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        /// <summary>Called each frame to handle movement. Behavior depends on `punchMovement`.</summary>
        public virtual void RunMeleeMovement()
        {
            if (!punchMovement)
            {
                RunKnifeMeleeMovement();
            }
            else
            {
                RunPunchMeleeMovement();
            }
        }

        /// <summary>Standard knife melee movement. Matches `TestVanDammeAnim.RunKnifeMeleeMovement`.</summary>
        protected virtual void RunKnifeMeleeMovement()
        {
            if (!owner.useNewKnifingFrames)
            {
                if (owner.Y > owner.groundHeight + 1f)
                {
                    hero.ApplyFallingGravity();
                }
            }
            else if (hero.JumpingMelee)
            {
                hero.ApplyFallingGravity();
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame <= 1)
                {
                    owner.xI = 0f;
                    owner.yI = 0f;
                }
                else if (owner.frame <= 3)
                {
                    if (hero.MeleeChosenUnit == null)
                    {
                        if (!hero.IsInQuicksand)
                        {
                            owner.xI = owner.speed * 1f * owner.transform.localScale.x;
                        }
                        owner.yI = 0f;
                    }
                    else if (!hero.IsInQuicksand)
                    {
                        owner.xI = (owner.speed * 0.5f * owner.transform.localScale.x) + ((hero.MeleeChosenUnit.X - owner.X) * 6f);
                    }
                }
                else if (owner.frame <= 5)
                {
                    if (!hero.IsInQuicksand)
                    {
                        owner.xI = owner.speed * 0.3f * owner.transform.localScale.x;
                    }
                    hero.ApplyFallingGravity();
                }
                else
                {
                    hero.ApplyFallingGravity();
                }
            }
            else if (owner.Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }

        /// <summary>Punch-style melee movement. Matches `BroBase.RunPunchMovement`.</summary>
        protected virtual void RunPunchMeleeMovement()
        {
            hero.ApplyFallingGravity();
            if (hero.JumpingMelee)
            {
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame < 2)
                {
                    owner.xI = 0f;
                    owner.yI = 0f;
                }
                else if (owner.frame <= 4)
                {
                    if (hero.MeleeChosenUnit != null)
                    {
                        float targetOffset = 8f;
                        if (meleeType == BroBase.MeleeType.Disembowel)
                        {
                            targetOffset = 14f;
                        }
                        float dist = hero.MeleeChosenUnit.X - (Direction * targetOffset) - X;
                        if (!hero.IsInQuicksand)
                        {
                            owner.xI = dist / 0.1f;
                        }
                        if (!hero.IsInQuicksand)
                        {
                            owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                        }
                    }
                    else
                    {
                        if (!hero.IsInQuicksand)
                        {
                            owner.xI = owner.speed * Direction;
                        }
                        owner.yI = 0f;
                    }
                }
                else if (owner.frame <= 7)
                {
                    owner.xI = 0f;
                }
            }
            else if (owner.Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }

        /// <summary>Called by `AnimateMelee` on `hitFrame` and the follow-up frames to damage units, doodads, doors and terrain.</summary>
        protected virtual void PerformMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + (float)(Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(24f);
            if (Map.HitClosestUnit(owner, PlayerNum, damage, parsedDamageType, hitRangeX, hitRangeY, X + (owner.transform.localScale.x * hitOffsetX), Y + hitOffsetY, owner.transform.localScale.x * knockbackX, knockbackY, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(meleeHitSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (hitTerrain && shouldTryHitTerrain && TryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
            hero.TriggerBroMeleeEvent();
        }

        /// <summary>
        /// Called after every base `CancelMelee`: when a melee ends normally or is interrupted, when another melee takes over, and when the game cancels with no melee running, as it does on death.
        /// It must be safe to run more than once and with none of this ability's melee active.
        /// Don't call `hero.CancelMelee()` from it, since that calls this method again.
        /// A melee the game ends without `CancelMelee`, as a stun, freeze, blind, acid cover or grenade throw-back does, never reaches it.
        /// </summary>
        public virtual void CancelMelee()
        {
        }

        /// <summary>
        /// Performs terrain damage and plays the ability's cached sounds instead of the owner's soundHolder.
        /// Returns true if terrain was hit.
        /// </summary>
        public virtual bool TryMeleeTerrain(int offset, int meleeDamage)
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(new Vector3(X - (Direction * 4f), Y + 4f, 0f), new Vector3(Direction, 0f, 0f), out raycastHit, (float)(16 + offset), hero.GroundLayer))
            {
                return false;
            }
            Cage cage = raycastHit.collider.GetComponent<Cage>();
            if (cage == null && raycastHit.collider.transform.parent != null)
            {
                cage = raycastHit.collider.transform.parent.GetComponent<Cage>();
            }
            if (cage != null)
            {
                MapController.Damage_Networked(owner, raycastHit.collider.gameObject, cage.health, DamageType.Melee, 0f, 40f, raycastHit.point.x, raycastHit.point.y);
                return true;
            }
            MapController.Damage_Networked(owner, raycastHit.collider.gameObject, meleeDamage, DamageType.Melee, 0f, 40f, raycastHit.point.x, raycastHit.point.y);
            if (hero.CurrentMeleeType == BroBase.MeleeType.Knife)
            {
                sound.PlaySoundEffectAt(meleeHitTerrainSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            else
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            EffectsController.CreateProjectilePopWhiteEffect(X + (owner.width * owner.transform.localScale.x), Y + owner.height + 4f);
            return true;
        }
    }
}

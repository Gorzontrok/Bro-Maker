using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>
    /// Controls how <see cref="MeleeAbility.StartMelee" /> initializes the melee.
    /// </summary>
    public enum MeleeStartType
    {
        /// <summary>Standard knife start. Matches <c>BroBase.StartKnifeMelee</c>.</summary>
        Knife,

        /// <summary>Custom start. Matches <c>BroBase.StartCustomMelee</c>.</summary>
        Custom
    }

    /// <summary>
    /// Controls how <see cref="MeleeAbility.RunMeleeMovement" /> handles movement during the melee.
    /// </summary>
    public enum MeleeMoveType
    {
        /// <summary>Standard knife movement. Matches <c>TestVanDammeAnim.RunKnifeMeleeMovement</c>.</summary>
        Knife,

        /// <summary>Punch movement. Matches <c>BroBase.RunPunchMovement</c>.</summary>
        Punch
    }

    /// <summary>Base class for all melee abilities. Public fields are the JSON parameter schema.</summary>
    public abstract class MeleeAbility : AbilityBase
    {
        /// <summary>Vanilla bro to source melee sounds from. Override in subclasses. Defaults to <c>HeroType.Rambro</c>.</summary>
        protected virtual HeroType SourceBroType => HeroType.Rambro;

        /// <summary>Controls how <see cref="StartMelee" /> initializes the melee. Defaults to <see cref="MeleeStartType.Knife" />.</summary>
        [JsonIgnore]
        public MeleeStartType startType = MeleeStartType.Knife;

        /// <summary>Controls how <see cref="RunMeleeMovement" /> handles movement. Defaults to <see cref="MeleeMoveType.Knife" />.</summary>
        [JsonIgnore]
        public MeleeMoveType moveType = MeleeMoveType.Knife;

        /// <summary>Sprite sheet row for standing melee animation. Defaults to 1 (standard knife row).</summary>
        public int animationRow = 1;

        /// <summary>Starting column for standing melee animation. Defaults to 25 (standard knife column).</summary>
        public int animationColumn = 25;

        /// <summary>Sprite sheet row for jumping/dashing melee. Defaults to 6 (standard knife jumping row).</summary>
        public int jumpingAnimationRow = 6;

        /// <summary>Starting column for jumping/dashing melee. Defaults to 17 (standard knife jumping column).</summary>
        public int jumpingAnimationColumn = 17;

        /// <summary>Number of frames in the melee animation. Defaults to 7.</summary>
        public int animationFrameCount = 7;

        /// <summary>Seconds per animation frame. Defaults to 0.025.</summary>
        public float frameRate = 0.025f;

        /// <summary>Which animation frame triggers the hit detection. Defaults to 3.</summary>
        public int hitFrame = 3;

        /// <summary>Frame at which a combo restart is allowed during <see cref="MeleeStartType.Knife" /> start.
        /// If the player re-presses melee after this frame, the animation restarts. Defaults to 3.</summary>
        public int restartFrame = 3;

        /// <summary>Frame at which the melee animation ends and <see cref="IAbilityOwner.CancelMelee" /> is called.
        /// Defaults to 6.</summary>
        public int endFrame = 6;

        /// <summary>Extra time subtracted from the frame counter on <see cref="hitFrame" />, creating a brief
        /// pause on the attack frame. Defaults to 0.066.</summary>
        public float hitFrameCounterDelay = 0.066f;

        /// <summary>Whether to continue calling <see cref="PerformMeleeAttack" /> on frames after
        /// <see cref="hitFrame" /> if the attack hasn't hit yet. Defaults to true.</summary>
        public bool attackOnFollowUpFrames = true;

        /// <summary>Damage dealt to units on hit. Defaults to 4 (standard knife damage).</summary>
        public int damage = 4;

        /// <summary>DamageType enum name (e.g., "Knifed", "Crush", "Electrocution"). Parsed at initialization.</summary>
        public string damageType = "Knifed";

        /// <summary>Horizontal knockback force applied to hit units. Defaults to 200.</summary>
        public float knockbackX = 200f;

        /// <summary>Vertical knockback force applied to hit units. Defaults to 500.</summary>
        public float knockbackY = 500f;

        /// <summary>Horizontal range of the hit detection box. Defaults to 14.</summary>
        public float hitRangeX = 14f;

        /// <summary>Vertical range of the hit detection box. Defaults to 24.</summary>
        public float hitRangeY = 24f;

        /// <summary>X offset from owner position for the hit detection center. Defaults to 8.</summary>
        public float hitOffsetX = 8f;

        /// <summary>Y offset from owner position for the hit detection center. Defaults to 8.</summary>
        public float hitOffsetY = 8f;

        /// <summary>Whether melee damages terrain and doors. Defaults to true.</summary>
        public bool hitTerrain = true;

        /// <summary>Damage dealt to terrain blocks on hit. Defaults to 2.</summary>
        public int terrainDamage = 2;

        /// <summary>Controls BroBase dispatch routing and TryMeleeTerrain sounds.</summary>
        [JsonIgnore]
        public BroBase.MeleeType meleeType = BroBase.MeleeType.Knife;

        public AudioClip[] meleeHitSounds;
        public AudioClip[] missSounds;
        /// <summary>Sounds played when melee hits terrain (knife-type melees).</summary>
        public AudioClip[] meleeHitTerrainSounds;
        /// <summary>Alternate hit sounds used by non-knife melees and terrain hits.</summary>
        public AudioClip[] alternateMeleeHitSounds;

        /// <param name="owner">The bro instance that owns this ability.</param>
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            CacheSoundsFromPrefab();
        }

        /// <summary>Loads melee sound clips from the <see cref="SourceBroType" /> prefab's soundHolder.
        /// Each array is null-guarded so JSON overrides aren't stomped. Override in subclasses to
        /// load additional bro-specific sounds (call <c>base.CacheSoundsFromPrefab()</c> first).</summary>
        protected virtual void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (meleeHitSounds == null) meleeHitSounds = sourceBro.soundHolder.meleeHitSound.CloneArray();
            if (missSounds == null) missSounds = sourceBro.soundHolder.missSounds.CloneArray();
            if (meleeHitTerrainSounds == null) meleeHitTerrainSounds = sourceBro.soundHolder.meleeHitTerrainSound.CloneArray();
            if (alternateMeleeHitSounds == null) alternateMeleeHitSounds = sourceBro.soundHolder.alternateMeleeHitSound.CloneArray();
        }

        /// <summary>Called when the player presses melee. Behavior depends on <see cref="startType" />.</summary>
        public virtual void StartMelee()
        {
            if (startType == MeleeStartType.Knife)
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
                if (!hero.DoingMelee || owner.frame > 10)
                {
                    if (hero.DoingMelee)
                    {
                        owner.frame = restartFrame;
                    }
                    else
                    {
                        owner.frame = 0;
                    }
                    owner.counter = -0.05f;
                    AnimateMelee();
                }
                if (!hero.JumpingMelee)
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

        /// <summary>Called each frame to handle movement. Behavior depends on <see cref="moveType" />.</summary>
        public virtual void RunMeleeMovement()
        {
            if (moveType == MeleeMoveType.Knife)
                RunKnifeMeleeMovement();
            else
                RunPunchMeleeMovement();
        }

        /// <summary>Standard knife melee movement. Matches <c>TestVanDammeAnim.RunKnifeMeleeMovement</c>.</summary>
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
                        owner.xI = owner.speed * 0.5f * owner.transform.localScale.x + (hero.MeleeChosenUnit.X - owner.X) * 6f;
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

        /// <summary>Punch-style melee movement. Matches <c>BroBase.RunPunchMovement</c>.</summary>
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
                        float dist = hero.MeleeChosenUnit.X - Direction * targetOffset - X;
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

        /// <summary>Override for custom hit detection (e.g., multi-hit, special effects).</summary>
        protected virtual void PerformMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + (float)(Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(24f);
            if (Map.HitClosestUnit(owner, PlayerNum, damage, DamageType.Knifed, hitRangeX, hitRangeY, X + owner.transform.localScale.x * hitOffsetX, Y + hitOffsetY, owner.transform.localScale.x * knockbackX, knockbackY, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(meleeHitSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
            hero.TriggerBroMeleeEvent();
        }

        /// <summary>Called when the melee is interrupted (e.g., taking damage, falling).
        /// IMPORTANT: Do NOT call <c>hero.CancelMelee()</c> from this method — the wrapper
        /// already calls both this callback and <c>base.CancelMelee()</c> in sequence.</summary>
        public virtual void CancelMelee()
        {
        }

        /// <summary>Performs terrain damage and plays the ability's
        /// cached sounds instead of the owner's soundHolder. Returns true if terrain was hit.</summary>
        public virtual bool HandleTryMeleeTerrain(int offset, int meleeDamage)
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(new Vector3(X - Direction * 4f, Y + 4f, 0f), new Vector3(Direction, 0f, 0f), out raycastHit, (float)(16 + offset), hero.GroundLayer))
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
            if (meleeType == BroBase.MeleeType.Knife)
            {
                sound.PlaySoundEffectAt(meleeHitTerrainSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            else
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 4f);
            return true;
        }
    }
}

using BroMakerLib.CustomObjects;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>
    /// Base class for all melee abilities. Subclasses implement specific vanilla melees
    /// (knife slash, punch, roundhouse kick, etc.) and are registered via <see cref="Attributes.MeleePresetAttribute" />.
    /// Not a MonoBehaviour — instantiated as a plain C# object and configured via JSON deserialization.
    /// Public fields are the JSON parameter schema; override defaults in subclass constructors for bro-specific values.
    /// </summary>
    public abstract class MeleeAbility
    {
        /// <summary>The bro that owns this ability. Set by <see cref="Initialize" />.</summary>
        [JsonIgnore]
        public TestVanDammeAnim owner;

        /// <summary>The <see cref="ICustomHero" /> interface on the owner bro. Provides access to protected
        /// fields and methods without reflection.</summary>
        [JsonIgnore]
        protected ICustomHero hero;

        /// <summary>Owner's player number.</summary>
        [JsonIgnore]
        protected int PlayerNum => owner.playerNum;

        /// <summary>Owner's facing direction: 1 = right, -1 = left.</summary>
        [JsonIgnore]
        protected float Direction => Mathf.Sign(owner.transform.localScale.x);

        /// <summary>Owner's X world position.</summary>
        [JsonIgnore]
        protected float X => owner.X;

        /// <summary>Owner's Y world position.</summary>
        [JsonIgnore]
        protected float Y => owner.Y;

        /// <summary>The Sound singleton for playing audio.</summary>
        [JsonIgnore]
        protected Sound sound => hero.Sound;

        /// <summary>Owner's SoundHolder containing AudioClip arrays for SFX (meleeHitSound, attackSounds, etc.).</summary>
        [JsonIgnore]
        protected SoundHolder soundHolder => owner.soundHolder;

        /// <summary>Sprite sheet row for standing melee animation.</summary>
        public int animationRow = 1;

        /// <summary>Starting column for standing melee animation.</summary>
        public int animationColumn = 25;

        /// <summary>Sprite sheet row for jumping/dashing melee. -1 = use <see cref="animationRow" />.</summary>
        public int jumpingAnimationRow = -1;

        /// <summary>Starting column for jumping/dashing melee. -1 = use <see cref="animationColumn" />.</summary>
        public int jumpingAnimationColumn = -1;

        /// <summary>Number of frames in the melee animation.</summary>
        public int animationFrameCount = 7;

        /// <summary>Seconds per animation frame.</summary>
        public float frameRate = 0.025f;

        /// <summary>Which animation frame triggers the hit detection.</summary>
        public int hitFrame = 3;

        /// <summary>Damage dealt to units on hit.</summary>
        public int damage = 3;

        /// <summary>DamageType enum name (e.g., "Knifed", "Crush", "Electrocution"). Parsed at initialization.</summary>
        public string damageType = "Knifed";

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

        // Common melee sound clips — loaded from source bro's prefab in subclass Initialize
        public AudioClip[] meleeHitSounds;
        public AudioClip[] missSounds;
        public AudioClip[] meleeHitTerrainSounds;
        public AudioClip[] alternateMeleeHitSounds;

        /// <summary>Called once when the bro spawns. Sets <see cref="owner" /> and caches the <see cref="hero" /> reference.</summary>
        /// <param name="owner">The bro instance that owns this ability.</param>
        public virtual void Initialize(TestVanDammeAnim owner)
        {
            this.owner = owner;
            this.hero = owner as ICustomHero;
        }

        /// <summary>Called when the player presses melee. Initializes state and sets animation parameters.</summary>
        public virtual void StartMelee()
        {
        }

        /// <summary>Called each frame during the melee animation. Controls sprite frames and performs hit detection on <see cref="hitFrame" />.</summary>
        public virtual void AnimateMelee()
        {
        }

        /// <summary>Called each frame to handle movement during the melee animation (e.g., dash forward, lunge).</summary>
        public virtual void RunMeleeMovement()
        {
        }

        /// <summary>Called when the melee is interrupted (e.g., taking damage, falling).
        /// IMPORTANT: Do NOT call <c>hero.CancelMelee()</c> from this method — the wrapper
        /// already calls both this callback and <c>base.CancelMelee()</c> in sequence.</summary>
        public virtual void CancelMelee()
        {
        }

        /// <summary>Called every frame. Use for timers and other per-frame logic.</summary>
        public virtual void Update()
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

        /// <summary>Called when the bro takes damage.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            return true;
        }

        /// <summary>Called during IsInStealthMode check.</summary>
        /// <returns>True to run original, false to force stealth mode active.</returns>
        public virtual bool HandleIsInStealthMode()
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

        /// <summary>Called during CanInseminate check.</summary>
        /// <returns>True to run original, false to use the provided result.</returns>
        public virtual bool HandleCanInseminate(ref bool result)
        {
            return true;
        }

        /// <summary>Called during ApplyFallingGravity.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleApplyFallingGravity()
        {
            return true;
        }

        /// <summary>Called during AlertNearbyMooks.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleAlertNearbyMooks()
        {
            return true;
        }

        /// <summary>Called when WallDrag is being set.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleWallDrag(bool value)
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

        /// <summary>Called when the bro starts firing.</summary>
        /// <returns>True to run original, false to skip.</returns>
        public virtual bool HandleStartFiring()
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

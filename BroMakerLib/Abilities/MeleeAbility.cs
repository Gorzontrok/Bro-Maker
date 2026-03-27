using HarmonyLib;
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

        [JsonIgnore] protected Sound _sound;

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

        /// <summary>The Sound singleton for playing audio. Cached from owner's protected field.</summary>
        [JsonIgnore]
        protected Sound sound => _sound;

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

        /// <summary>Called once when the bro spawns. Sets <see cref="owner" /> and caches protected field references.</summary>
        public virtual void Initialize(TestVanDammeAnim owner)
        {
            this.owner = owner;
            _sound = Traverse.Create(owner).Field<Sound>("sound").Value;
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

        /// <summary>Called when the melee is interrupted (e.g., taking damage, falling).</summary>
        public virtual void CancelMelee()
        {
        }

        /// <summary>Called every frame. Use for timers and other per-frame logic.</summary>
        public virtual void Update()
        {
        }
    }
}
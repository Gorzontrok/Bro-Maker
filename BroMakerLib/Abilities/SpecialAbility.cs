using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>
    /// Base class for all special abilities. Subclasses implement specific vanilla specials
    /// (grenade throws, dashes, mode toggles, etc.) and are registered via <see cref="Attributes.SpecialPresetAttribute" />.
    /// Not a MonoBehaviour — instantiated as a plain C# object and configured via JSON deserialization.
    /// Public fields are the JSON parameter schema; override defaults in subclass constructors for bro-specific values.
    /// </summary>
    public abstract class SpecialAbility
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

        /// <summary>Owner's SoundHolder containing AudioClip arrays for SFX (specialSounds, throwSounds, etc.).</summary>
        [JsonIgnore]
        protected SoundHolder soundHolder => owner.soundHolder;

        /// <summary>Sprite sheet row for the special animation.</summary>
        public int animationRow = 1;

        /// <summary>Starting column on the sprite sheet.</summary>
        public int animationColumn = 16;

        /// <summary>Number of frames in the animation.</summary>
        public int animationFrameCount = 5;

        /// <summary>Which frame triggers the special effect (calls <see cref="UseSpecial" />).</summary>
        public int triggerFrame = 2;

        /// <summary>Seconds per animation frame.</summary>
        public float frameRate = 0.0334f;

        /// <summary>When true, <see cref="PressSpecial" /> calls <see cref="UseSpecial" /> directly, skipping the animation.</summary>
        public bool instantUse = false;

        /// <summary>When true, the bro cannot move during the special animation.</summary>
        public bool blockMovement = true;

        /// <summary>When true, hides the gun sprite during the special animation.</summary>
        public bool deactivateGun = true;

        /// <summary>
        /// Whether this ability is currently active (e.g., mid-dash, in stealth mode).
        /// While active, CustomHero calls modifier hooks each frame.
        /// </summary>
        [JsonIgnore]
        public bool IsActive { get; protected set; }

        /// <summary>Called once when the bro spawns. Sets <see cref="owner" /> and caches protected field references.</summary>
        public virtual void Initialize(TestVanDammeAnim owner)
        {
            this.owner = owner;
            _sound = Traverse.Create(owner).Field<Sound>("sound").Value;
        }

        /// <summary>Called when the player presses the special button. Consumes ammo and begins the special sequence.</summary>
        public virtual void PressSpecial()
        {
        }

        /// <summary>Called each frame during the special animation. Controls sprite frames and triggers <see cref="UseSpecial" /> at <see cref="triggerFrame" />.</summary>
        public virtual void AnimateSpecial()
        {
        }

        /// <summary>Called at the trigger frame to execute the special effect (spawn projectile, activate mode, etc.).</summary>
        public virtual void UseSpecial()
        {
        }

        /// <summary>Called every frame. Use for timers, projectile tracking, and other per-frame logic.</summary>
        public virtual void Update()
        {
        }

        /// <summary>
        /// Called during the bro's movement processing while <see cref="IsActive" /> is true.
        /// Return value follows Harmony prefix convention: true = run original movement, false = skip it.
        /// </summary>
        public virtual bool HandleMovement(ref float xI, ref float yI)
        {
            return true;
        }

        /// <summary>
        /// Called when the bro takes damage while <see cref="IsActive" /> is true.
        /// Return value follows Harmony prefix convention: true = run original damage handling, false = skip it.
        /// </summary>
        public virtual bool HandleDamage(DamageObject damage)
        {
            return true;
        }

        /// <summary>
        /// Called during the bro's firing logic while <see cref="IsActive" /> is true.
        /// Return value follows Harmony prefix convention: true = run original firing, false = skip it.
        /// </summary>
        public virtual bool HandleFiring()
        {
            return true;
        }
    }
}
using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>Base class for all special abilities. Public fields are the JSON parameter schema.</summary>
    public abstract class SpecialAbility : AbilityBase
    {
        /// <summary>Vanilla bro to source special sounds from.</summary>
        protected virtual HeroType SourceBroType => HeroType.Rambro;

        /// <summary>Which animation frame calls `UseSpecial`.</summary>
        public int triggerFrame = 4;

        protected SpecialAbility()
        {
            animationRow = 5;
            animationColumn = 17;
            animationFrameCount = 8;
            frameRate = 0.0334f;
        }

        /// <summary>Horizontal offset from the owner's position used when spawning the special (projectile, grenade, etc.).</summary>
        public float spawnOffsetX = 0f;

        /// <summary>Vertical offset from the owner's position used when spawning the special.</summary>
        public float spawnOffsetY = 0f;

        /// <summary>If true, `UseSpecial` fires immediately on press without running the animation.</summary>
        public bool instantUse = false;

        /// <summary>If true, xI and yI are zeroed while the special is active via `HandleCalculateMovement`.</summary>
        public bool blockMovement = false;

        /// <summary>If true, `DeactivateGun` is called during the animation.</summary>
        public bool deactivateGun = true;

        /// <summary>Sounds played when throwing a projectile or grenade special.</summary>
        public AudioClip[] throwSounds;

        /// <summary>Generic attack sounds (hit feedback, impact cues).</summary>
        public AudioClip[] attackSounds;

        /// <summary>Sounds played for the special's main attack effect.</summary>
        public AudioClip[] specialAttackSounds;

        /// <summary>Called once when the bro spawns. Also calls `CacheSoundsFromPrefab`.</summary>
        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            CacheSoundsFromPrefab();
        }

        /// <summary>Loads special sound clips from the `SourceBroType` prefab's soundHolder.
        /// Each array is null-guarded so JSON overrides aren't stomped. Override in subclasses to
        /// load additional bro-specific sounds (call `base.CacheSoundsFromPrefab()` first).</summary>
        protected virtual void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (throwSounds == null) throwSounds = sourceBro.soundHolder.throwSounds.CloneArray();
            if (attackSounds == null) attackSounds = sourceBro.soundHolder.attackSounds.CloneArray();
            if (specialAttackSounds == null) specialAttackSounds = sourceBro.soundHolder.specialAttackSounds.CloneArray();
        }

        /// <summary>Called when the player presses the special button. Starts the animation or, if `instantUse` is true, calls `UseSpecial` directly.</summary>
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

        /// <summary>Called each frame while the special is animating. Advances the sprite, calls `UseSpecial` on the trigger frame, and ends the animation.</summary>
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

        /// <summary>Called when the special fires. Deducts ammo and calls `ActivateSpecial`, or flashes the HUD if out of ammo.</summary>
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

        /// <summary>Override to implement the special's effect (spawn projectile, apply buff, etc.). Called by `UseSpecial` after ammo is deducted.</summary>
        public virtual void ActivateSpecial()
        {
        }

        public override bool HandleCalculateMovement(ref float xI, ref float yI)
        {
            if (blockMovement && hero.UsingSpecial)
            {
                xI = 0f;
                yI = 0f;
                return false;
            }
            return true;
        }
    }
}

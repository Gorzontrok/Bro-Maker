using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    public abstract class SpecialAbility : AbilityBase
    {
        /// <summary>Vanilla bro to source special sounds from. Override in subclasses. Defaults to <c>HeroType.Rambro</c>.</summary>
        protected virtual HeroType SourceBroType => HeroType.Rambro;

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

        /// <summary>Called once when the bro spawns. Also calls <see cref="CacheSoundsFromPrefab" />.</summary>
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            CacheSoundsFromPrefab();
        }

        /// <summary>Loads special sound clips from the <see cref="SourceBroType" /> prefab's soundHolder.
        /// Each array is null-guarded so JSON overrides aren't stomped. Override in subclasses to
        /// load additional bro-specific sounds (call <c>base.CacheSoundsFromPrefab()</c> first).</summary>
        protected virtual void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (throwSounds == null) throwSounds = sourceBro.soundHolder.throwSounds.CloneArray();
            if (attackSounds == null) attackSounds = sourceBro.soundHolder.attackSounds.CloneArray();
            if (specialAttackSounds == null) specialAttackSounds = sourceBro.soundHolder.specialAttackSounds.CloneArray();
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

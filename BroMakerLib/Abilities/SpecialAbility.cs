using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Abilities
{
    /// <summary>Base class for special abilities, whose public fields form the JSON parameter schema.</summary>
    public abstract class SpecialAbility : AbilityBase
    {
        /// <summary>Vanilla bro to source special sounds from.</summary>
        protected virtual HeroType SourceBroType => HeroType.Rambro;

        /// <summary>Animation frame on which `UseSpecial` is called, a frame past the animation's last one never calling it. On presets whose press doesn't redraw the bro, which is most, frame 0 isn't animated, so 0 fires only when something else redraws the bro first.</summary>
        public int triggerFrame = 4;

        protected SpecialAbility()
        {
            animationRow = 5;
            animationColumn = 17;
            animationFrameCount = 8;
            frameRate = 0.0334f;
        }

        /// <summary>If true, `UseSpecial` fires immediately on press without running the animation.</summary>
        public bool instantUse = false;

        /// <summary>If true, xI and yI are zeroed while the special is active via `HandleCalculateMovement`.</summary>
        public bool blockMovement = false;

        /// <summary>If true, `DeactivateGun` is called during the animation.</summary>
        public bool deactivateGun = true;

        /// <summary>Throw sounds, taken from the `SourceBroType` prefab when JSON leaves them unset. Only presets that play this array use them, and most of those play a voiced owner's throw grunt instead unless JSON sets them.</summary>
        public AudioClip[] throwSounds;

        /// <summary>Attack sounds, taken from the `SourceBroType` prefab when JSON leaves them unset.</summary>
        public AudioClip[] attackSounds;

        /// <summary>Special attack sounds, taken from the `SourceBroType` prefab when JSON leaves them unset.</summary>
        public AudioClip[] specialAttackSounds;

        [JsonIgnore] private bool soundsCached;
        [JsonIgnore] private bool throwSoundsOverridden;
        [JsonIgnore] private int stepEndFrame = -1;
        [JsonIgnore] private int replayedThroughFrame = -1;

        /// <summary>Called once when the bro spawns. Also calls `CacheSoundsFromPrefab`.</summary>
        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);
            CacheSoundsFromPrefab();
        }

        /// <summary>Loads special sound clips from the `SourceBroType` prefab's soundHolder, null-guarding each array so JSON overrides aren't stomped; call `base.CacheSoundsFromPrefab()` first.</summary>
        protected virtual void CacheSoundsFromPrefab()
        {
            if (!soundsCached)
            {
                throwSoundsOverridden = throwSounds != null;
                soundsCached = true;
            }

            TestVanDammeAnim sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null)
            {
                return;
            }

            if (throwSounds == null)
            {
                throwSounds = sourceBro.soundHolder.throwSounds.CloneArray();
            }
            if (attackSounds == null)
            {
                attackSounds = sourceBro.soundHolder.attackSounds.CloneArray();
            }
            if (specialAttackSounds == null)
            {
                specialAttackSounds = sourceBro.soundHolder.specialAttackSounds.CloneArray();
            }
        }

        /// <summary>Reproduces `BroBase.PlayThrowLightSound` with the owner's voice grunt, playing `throwSounds` instead when JSON supplies them or the owner has no voice.</summary>
        protected void PlayThrowLightSound(float volume)
        {
            if (throwSoundsOverridden)
            {
                Sound.GetInstance().PlaySoundEffectAt(throwSounds, volume, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
            }
            else if (hero.SoundHolderVoice != null && !owner.BypassNewVoices)
            {
                Sound.GetInstance().PlaySoundEffectAt(hero.SoundHolderVoice.throwLightGrunt, volume * hero.SoundHolderVoice.voiceGruntVolumeM, owner.transform.position, 1f, true, false, false, 0f);
            }
            else
            {
                Sound.GetInstance().PlaySoundEffectAt(throwSounds, volume, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
            }
        }

        /// <summary>Called when the player presses the special button. Starts the animation or, if `instantUse` is true, calls `UseSpecial` directly while the owner's health is above zero.</summary>
        public virtual void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.DoingMelee)
            {
                return;
            }

            if (instantUse)
            {
                // The game reads a dead bro's input too, so its body gets the next bro's presses.
                if (owner.health > 0)
                {
                    UseSpecial();
                }
                return;
            }

            hero.UsingSpecial = true;
            owner.frame = 0;
            ResetTriggerFrames();
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

            if (IsTriggerFrame(triggerFrame))
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

        /// <summary>Runs one step of `AnimateSpecial`, noting when code outside the special rewound the owner's `frame` since the previous step or stepped the animation again without advancing it.</summary>
        internal void RunAnimateSpecial()
        {
            int startFrame = owner.frame;
            IsRepeatedStep = startFrame == stepEndFrame;
            if (startFrame < stepEndFrame && stepEndFrame > replayedThroughFrame)
            {
                replayedThroughFrame = stepEndFrame;
            }
            AnimateSpecial();
            if (owner.frame < startFrame)
            {
                replayedThroughFrame = -1;
            }
            stepEndFrame = owner.frame;
        }

        /// <summary>True while `AnimateSpecial` runs a step that starts on the frame the previous step ended on, as when the game steps the animation again without advancing `frame`.</summary>
        protected bool IsRepeatedStep { get; private set; }

        /// <summary>True when the owner's `frame` is `frame` on a step that isn't a repeated step and isn't replaying frames after the bro's own code rewound it, so each trigger frame fires once per pass; a rewind made inside `AnimateSpecial` itself replays normally.</summary>
        protected bool IsTriggerFrame(int frame)
        {
            return owner.frame == frame && owner.frame > replayedThroughFrame && !IsRepeatedStep;
        }

        /// <summary>Starts a new run for `IsTriggerFrame`; call it where an override of `PressSpecial` restarts an animation that is already running without calling the base method.</summary>
        protected internal void ResetTriggerFrames()
        {
            stepEndFrame = -1;
            replayedThroughFrame = -1;
        }

        /// <summary>Called when the special fires. Deducts ammo and calls `ActivateSpecial`, or flashes the HUD if out of ammo.</summary>
        public virtual void UseSpecial()
        {
            if (owner.SpecialAmmo > 0)
            {
                owner.SpecialAmmo--;
                if (owner.IsMine)
                {
                    hero.TriggerBroSpecialEvent();
                }
                ActivateSpecial();
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
            hero.PressSpecialFacingDirection = 0;
        }

        /// <summary>The special's effect. Called by `UseSpecial` after ammo is deducted.</summary>
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

        /// <summary>True once the special state patches are in place, and false again once a later patch pass leaves one out; while false, an attached special ability keeps its state in the bro's own `usingSpecial`.</summary>
        internal static bool SpecialStateActive { get; set; }

        internal static readonly Dictionary<string, int[]> specialStateExpectedCounts = new Dictionary<string, int[]>();
        internal static List<string> specialStateFailures;
        internal static AccessTools.FieldRef<TestVanDammeAnim, bool> specialStateUsingSpecialRef;

        internal static bool ReadUsingSpecial(TestVanDammeAnim unit, bool hostUsingSpecial)
        {
            if (hostUsingSpecial || !SpecialStateActive)
            {
                return hostUsingSpecial;
            }
            IAbilityOwner owner = unit as IAbilityOwner;
            return owner != null && owner.UsingSpecial;
        }

        internal static void StoreUsingSpecial(TestVanDammeAnim unit, bool value)
        {
            specialStateUsingSpecialRef(unit) = value;
            if (!value && SpecialStateActive)
            {
                IAbilityOwner owner = unit as IAbilityOwner;
                if (owner != null)
                {
                    owner.UsingSpecial = false;
                }
            }
        }
    }
}

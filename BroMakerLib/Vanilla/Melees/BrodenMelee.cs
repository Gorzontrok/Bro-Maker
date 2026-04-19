using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Broden's impaling uppercut melee.</summary>
    [MeleePreset("Broden")]
    public class BrodenMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.Broden;

        public AudioClip[] alternateMeleeMissSounds;

        [JsonIgnore] private bool hasPlayedUppercutSound;

        public BrodenMelee()
        {
            meleeType = BroBase.MeleeType.Custom;
            startType = MeleeStartType.Custom;
            restartFrame = 1;
            animationRow = 8;
            animationColumn = 22;
            frameRate = 0.033f;
            damageType = "Melee";
        }

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();

            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (alternateMeleeMissSounds == null) alternateMeleeMissSounds = sourceBro.soundHolder.alternateMeleeMissSound.CloneArray();
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);

            var sourceBro = HeroController.GetHeroPrefab(HeroType.Broden) as Broden;
            if (sourceBro != null)
            {
                animationRow = sourceBro.upperCutAnimationRow;
            }
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 4)
            {
                hasPlayedUppercutSound = false;
                owner.frame = 1;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else if (owner.frame >= 5)
            {
                hero.MeleeFollowUp = true;
            }
            if (!hero.JumpingMelee)
            {
                hero.DashingMelee = true;
                owner.xI = Direction * owner.speed;
            }
            hero.StartMeleeCommon();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 9);
            int num2 = animationRow;
            hero.FrameRate = frameRate;
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 7)
            {
                owner.counter = -0.066f;
            }
            if (owner.frame >= 5 && owner.frame < 7 && !hero.MeleeHasHit)
            {
                PerformUpperCut(true, true);
            }
            if (owner.frame >= 9)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
            if (owner.frame > 1 && !hero.MeleeHasHit && !hasPlayedUppercutSound)
            {
                sound.PlaySoundEffectAt(alternateMeleeMissSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                hero.HasPlayedMissSound = true;
                hasPlayedUppercutSound = true;
            }
        }

        public override void HandleAfterResetMeleeValues()
        {
            if (owner.frame <= 1)
            {
                hasPlayedUppercutSound = false;
            }
        }

        private void PerformUpperCut(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, owner.X + (float)(owner.Direction * 4), owner.Y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(24f);
            List<Unit> list = new List<Unit>();
            if (Map.HitUnits(owner, owner.playerNum, 8, parsedDamageType, 9f, owner.X + (float)(owner.Direction * 8), owner.Y + 16f, owner.xI + (float)(owner.Direction * 80), owner.yI + 700f, false, true, false, list, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                SortOfFollow.Shake(0.3f);
            }
            else if (TryMeleeTerrain(0, 10))
            {
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && TryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

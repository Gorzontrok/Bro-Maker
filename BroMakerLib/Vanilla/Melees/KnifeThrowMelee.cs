using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Shared base for throwing-knife melee. Extended by: TheBrodeMelee.</summary>
    [MeleePreset("KnifeThrowMelee")]
    public class KnifeThrowMelee : MeleeAbility
    {
        [JsonIgnore]
        protected Projectile throwingKnife;

        [JsonIgnore]
        protected bool knifeThrown;

        public AudioClip[] attackSounds;

        public KnifeThrowMelee()
        {
            meleeType = BroBase.MeleeType.ThrowingKnife;
            startType = MeleeStartType.Custom;
            moveType = MeleeMoveType.Punch;
            restartFrame = 3;
            animationRow = 7;
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);

            var bladePrefab = HeroController.GetHeroPrefab(HeroType.Blade) as Blade;
            if (bladePrefab != null)
            {
                throwingKnife = bladePrefab.throwingKnife;
                if (attackSounds == null) attackSounds = bladePrefab.soundHolder.attackSounds.CloneArray();
            }
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 3)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else
            {
                hero.MeleeFollowUp = true;
            }
            hero.StartMeleeCommon();
            knifeThrown = false;
        }

        public override void AnimateMelee()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.RollingFrames = 0;
            if (owner.frame == 1)
            {
                owner.counter -= 0.0334f;
            }
            if (owner.frame == 6 && hero.MeleeFollowUp)
            {
                owner.counter -= 0.08f;
                owner.frame = 1;
                hero.MeleeFollowUp = false;
                hero.ResetMeleeValues();
            }
            hero.FrameRate = frameRate;
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 6);
            int num2 = animationRow;
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3 && !knifeThrown)
            {
                ThrowKnife();
            }
            if (owner.frame >= 6)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
            if (owner.frame == 2 && hero.NearbyMook != null && hero.NearbyMook.CanBeThrown() && hero.HighFive)
            {
                hero.CancelMelee();
                Mook nearbyMook = hero.NearbyMook;
                hero.ThrowBackMook(nearbyMook);
                hero.NearbyMook = null;
            }
        }

        protected void ThrowKnife()
        {
            knifeThrown = true;
            sound.PlaySoundEffectAt(attackSounds, 0.44f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
            ProjectileController.SpawnProjectileLocally(throwingKnife, owner, X + (float)(16 * (int)Direction), Y + 10f, owner.xI + (float)(250 * (int)Direction), 0f, PlayerNum);
        }
    }
}

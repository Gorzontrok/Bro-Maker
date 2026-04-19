using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>TheBrode's throwing-knife melee.</summary>
    [MeleePreset("TheBrode")]
    public class TheBrodeMelee : KnifeThrowMelee
    {
        public TheBrodeMelee()
        {
            meleeType = BroBase.MeleeType.Custom;
            animationRow = 9;
        }

        public override void Initialize(BroBase owner)
        {
            base.Initialize(owner);

            var sourceBro = HeroController.GetHeroPrefab(HeroType.TheBrode) as TheBrode;
            if (sourceBro != null)
            {
                throwingKnife = sourceBro.throwingKnife;
            }
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
    }
}

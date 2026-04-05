using BroMakerLib.Attributes;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("TheBrode")]
    public class TheBrodeMelee : KnifeThrowMelee
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            TheBrode theBrode = owner as TheBrode;
            if (theBrode == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.TheBrode);
                theBrode = prefab as TheBrode;
            }
            if (theBrode != null)
            {
                throwingKnife = theBrode.throwingKnife;
            }
        }

        public override void AnimateMelee()
        {
            hero.SetSpriteOffset(0f, 0f);
            owner.SetFieldValue("rollingFrames", 0);
            if (owner.frame == 1)
            {
                owner.counter -= 0.0334f;
            }
            if (owner.frame == 6 && hero.MeleeFollowUp)
            {
                owner.counter -= 0.08f;
                owner.frame = 1;
                hero.MeleeFollowUp = false;
            }
            hero.FrameRate = 0.025f;
            int num = 25 + Mathf.Clamp(owner.frame, 0, 6);
            int num2 = 9;
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
            if (owner.frame == 2 && owner.GetFieldValue<Mook>("nearbyMook") != null && owner.GetFieldValue<Mook>("nearbyMook").CanBeThrown() && owner.GetFieldValue<bool>("highFive"))
            {
                hero.CancelMelee();
                Mook nearbyMook = owner.GetFieldValue<Mook>("nearbyMook");
                owner.CallMethod("ThrowBackMook", nearbyMook);
                owner.SetFieldValue("nearbyMook", null);
            }
        }
    }
}

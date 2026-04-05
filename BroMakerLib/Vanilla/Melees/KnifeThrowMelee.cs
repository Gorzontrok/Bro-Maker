using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("KnifeThrowMelee")]
    public class KnifeThrowMelee : MeleeAbility
    {
        [JsonIgnore]
        protected Projectile throwingKnife;

        [JsonIgnore]
        protected bool knifeThrown;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var bladePrefab = HeroController.GetHeroPrefab(HeroType.Blade) as Blade;
            if (bladePrefab != null)
            {
                throwingKnife = bladePrefab.throwingKnife;
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
            int num2 = 7;
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

        public override void RunMeleeMovement()
        {
            owner.CallMethod("ApplyFallingGravity");
            if (hero.JumpingMelee)
            {
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame < 2)
                {
                    owner.xI = 0f;
                    owner.yI = 0f;
                }
                else if (owner.frame <= 4)
                {
                    if (hero.MeleeChosenUnit != null)
                    {
                        float num = 8f;
                        float num2 = hero.MeleeChosenUnit.X - Direction * num - X;
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = num2 / 0.1f;
                        }
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                        }
                    }
                    else
                    {
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = owner.speed * Direction;
                        }
                        owner.yI = 0f;
                    }
                }
                else if (owner.frame <= 7)
                {
                    owner.xI = 0f;
                }
            }
            else if (owner.Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }

        protected void ThrowKnife()
        {
            knifeThrown = true;
            hero.PlayAttackSound(0.44f);
            ProjectileController.SpawnProjectileLocally(throwingKnife, owner, X + (float)(16 * (int)Direction), Y + 10f, owner.xI + (float)(250 * (int)Direction), 0f, PlayerNum);
        }
    }
}

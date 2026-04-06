using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("Broden")]
    public class BrodenMelee : MeleeAbility
    {
        public int upperCutAnimationRow = 7;

        [JsonIgnore] private bool hasPlayedUppercutSound;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            meleeType = BroBase.MeleeType.Custom;

            var broden = owner as Broden;
            if (broden == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.Broden);
                broden = prefab as Broden;
            }
            if (broden != null)
            {
                upperCutAnimationRow = broden.upperCutAnimationRow;
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
                owner.SetFieldValue("dashingMelee", true);
                owner.xI = Direction * owner.speed;
            }
            hero.StartMeleeCommon();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = 22 + Mathf.Clamp(owner.frame, 0, 9);
            int num2 = upperCutAnimationRow;
            hero.FrameRate = 0.033f;
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
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeMissSound, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                owner.SetFieldValue("hasPlayedMissSound", true);
                hasPlayedUppercutSound = true;
            }
        }

        public override void CancelMelee()
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
            if (Map.HitUnits(owner, owner.playerNum, 8, DamageType.Melee, 9f, owner.X + (float)(owner.Direction * 8), owner.Y + 16f, owner.xI + (float)(owner.Direction * 80), owner.yI + 700f, false, true, false, list, false, true))
            {
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                SortOfFollow.Shake(0.3f);
            }
            else if (hero.TryMeleeTerrain(0, 10))
            {
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }

        public override void RunMeleeMovement()
        {
            if (!owner.useNewKnifingFrames)
            {
                if (owner.Y > owner.groundHeight + 1f)
                {
                    owner.CallMethod("ApplyFallingGravity");
                }
            }
            else if (hero.JumpingMelee)
            {
                owner.CallMethod("ApplyFallingGravity");
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame <= 1)
                {
                    owner.xI = 0f;
                    owner.yI = 0f;
                }
                else if (owner.frame <= 3)
                {
                    if (hero.MeleeChosenUnit == null)
                    {
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = owner.speed * 1f * owner.transform.localScale.x;
                        }
                        owner.yI = 0f;
                    }
                    else if (!owner.GetFieldValue<bool>("isInQuicksand"))
                    {
                        owner.xI = owner.speed * 0.5f * owner.transform.localScale.x + (hero.MeleeChosenUnit.X - owner.X) * 6f;
                    }
                }
                else if (owner.frame <= 5)
                {
                    if (!owner.GetFieldValue<bool>("isInQuicksand"))
                    {
                        owner.xI = owner.speed * 0.3f * owner.transform.localScale.x;
                    }
                    owner.CallMethod("ApplyFallingGravity");
                }
                else
                {
                    owner.CallMethod("ApplyFallingGravity");
                }
            }
            else if (owner.Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }
    }
}

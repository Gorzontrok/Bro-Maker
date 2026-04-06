using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("DirtyHarry")]
    public class DirtyHarryMelee : MeleeAbility
    {
        public AudioClip[] alternateMeleeHitSound2;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            meleeType = BroBase.MeleeType.Punch;

            var harry = owner as DirtyHarry;
            if (harry == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.DirtyHarry);
                harry = prefab as DirtyHarry;
            }
            if (harry != null)
            {
                alternateMeleeHitSound2 = harry.soundHolder.alternateMeleeHitSound2;
            }
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 4)
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
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = 25 + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = 9;
            if (owner.frame == 5)
            {
                owner.counter -= 0.0334f;
            }
            if (owner.frame == 2)
            {
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 2 && !hero.MeleeHasHit)
            {
                PerformBaseBallBatHit();
            }
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformBaseBallBatHit()
        {
            float num = 8f;
            Vector3 vector = new Vector3(owner.X + (float)owner.Direction * (num + 7f), owner.Y + 10f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(25f);
            if (Map.HitClosestUnit(owner, owner.playerNum, 4, DamageType.Melee, num, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 250f, 250f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 0.3f, owner.transform.position, 0.6f, true, false, false, 0f);
                sound.PlaySoundEffectAt(alternateMeleeHitSound2, 0.5f, owner.transform.position, 0.6f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(owner.X + (owner.width + 10f) * owner.transform.localScale.x, owner.Y + owner.height + 4f);
            }
            else
            {
                if (!owner.GetFieldValue<bool>("hasPlayedMissSound"))
                {
                    sound.PlaySoundEffectAt(soundHolder.missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
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
                        if (owner.GetFieldValue<BroBase.MeleeType>("meleeType") == BroBase.MeleeType.Disembowel)
                        {
                            num = 14f;
                        }
                        float num2 = hero.MeleeChosenUnit.X - (float)owner.Direction * num - owner.X;
                        owner.xI = num2 / 0.1f;
                        owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                    }
                    else
                    {
                        owner.xI = owner.speed * (float)owner.Direction;
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
    }
}

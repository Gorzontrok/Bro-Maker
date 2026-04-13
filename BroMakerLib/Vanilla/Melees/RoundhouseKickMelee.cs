using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("RoundhouseKickMelee")]
    public class RoundhouseKickMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.BrodellWalker;

        [JsonIgnore]
        private float prePauseXI;

        [JsonIgnore]
        private float prePauseYI;

        public AudioClip[] alternateMeleeMissSounds;

        public RoundhouseKickMelee()
        {
            meleeType = BroBase.MeleeType.ChuckKick;
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (alternateMeleeHitSounds == null) alternateMeleeHitSounds = sourceBro.soundHolder.alternateMeleeHitSound.CloneArray();
            if (alternateMeleeMissSounds == null) alternateMeleeMissSounds = sourceBro.soundHolder.alternateMeleeMissSound.CloneArray();
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 6)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
            }
            if (hero.DoingMelee && owner.frame <= 4)
            {
                hero.MeleeFollowUp = true;
            }
            hero.StartMeleeCommon();
            if (hero.DashingMelee)
            {
                hero.HasJumpedForKick = !owner.IsOnGround();
            }
            else
            {
                hero.HasJumpedForKick = true;
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
            hero.FrameRate = 0.025f;
            if (owner.frame == 2 && hero.NearbyMook != null && hero.NearbyMook.CanBeThrown() && hero.HighFive)
            {
                hero.CancelMelee();
                hero.ThrowBackMook(hero.NearbyMook);
                hero.NearbyMook = null;
            }
            if (owner.frame == 3)
            {
                hero.FrameRate = 0.125f;
                prePauseXI = owner.xI;
                prePauseYI = owner.yI;
                owner.xI = 0f;
            }
            if (owner.frame == 4)
            {
                owner.xI = prePauseXI * 1.5f;
                owner.yI = prePauseYI;
                hero.FrameRate = 0.05f;
            }
            if (owner.frame == 5)
            {
                hero.FrameRate = 0.075f;
            }
            if (hero.JumpingMelee && owner.frame == 6 && !owner.IsOnGround())
            {
                owner.counter -= 0.066f;
            }
            if (hero.JumpingMelee)
            {
                if (owner.frame > 3 && owner.frame < 6 && !hero.MeleeHasHit)
                {
                    PerformRoundHouseKickAttack(true, true);
                }
            }
            else if (owner.frame > 1 && owner.frame < 4 && !hero.MeleeHasHit)
            {
                PerformRoundHouseKickAttack(true, true);
            }
            if (hero.JumpingMelee && owner.frame > 6 && !owner.IsOnGround() && hero.HighFive)
            {
                owner.frame = 6;
            }
            if (owner.frame == 2 && hero.DoingMelee && !hero.HasJumpedForKick)
            {
                if (owner.IsOnGround())
                {
                    owner.yI = 150f;
                }
                hero.HasJumpedForKick = true;
            }
            int num = 24 + owner.frame;
            if ((hero.DashingMelee && !hero.MeleeFollowUp) || hero.JumpingMelee)
            {
                hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(hero.SpritePixelHeight * 9));
            }
            else
            {
                hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(hero.SpritePixelHeight * 10));
            }
            if (owner.frame > 7)
            {
                owner.frame = 7;
                hero.CancelMelee();
            }
        }

        private void PerformRoundHouseKickAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, X + (float)((int)Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(25f);
            hero.MeleeChosenUnit = null;
            int direction = (int)Direction;
            int num = 3;
            int num2 = num + 3;
            float num3 = 350f + owner.xI;
            if (Map.HitUnits(owner, owner, PlayerNum, 5, DamageType.Melee, (float)num, 9f, X + (float)(direction * num2), Y, (float)direction * num3, 460f, false, true, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.7f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            if (!hero.MeleeHasHit && shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
            if (!hero.MeleeHasHit && !hero.HasPlayedMissSound)
            {
                sound.PlaySoundEffectAt(alternateMeleeMissSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                hero.HasPlayedMissSound = true;
            }
        }

        public override void RunMeleeMovement()
        {
            hero.ApplyFallingGravity();
            if (owner.yI < owner.maxFallSpeed)
            {
                owner.yI = owner.maxFallSpeed;
            }
            if (!hero.JumpingMelee)
            {
                if (hero.DashingMelee)
                {
                    if (owner.frame <= 0)
                    {
                        owner.xI = 0f;
                        owner.yI = 0f;
                    }
                    else if (owner.frame <= 3)
                    {
                        if (hero.MeleeChosenUnit == null)
                        {
                            owner.xI = owner.speed * 1f * owner.transform.localScale.x;
                        }
                        else
                        {
                            owner.xI = owner.speed * 0.5f * owner.transform.localScale.x + (hero.MeleeChosenUnit.X - owner.X) * 6f;
                        }
                    }
                }
            }
        }
    }
}

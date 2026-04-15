using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Shared base for split kick melee. Used by: TimeBroVanDamme.</summary>
    [MeleePreset("SplitKickMelee")]
    public class SplitKickMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.TimeBroVanDamme;

        public AudioClip[] alternateMeleeMissSounds;

        public SplitKickMelee()
        {
            meleeType = BroBase.MeleeType.VanDammeKick;
            animationColumn = 24;
            animationRow = 9;
        }

        public int splitKickRow = 10;
        public int jumpingFallingRow = 11;

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (alternateMeleeHitSounds == null) alternateMeleeHitSounds = sourceBro.soundHolder.alternateMeleeHitSound.CloneArray();
            if (alternateMeleeMissSounds == null) alternateMeleeMissSounds = sourceBro.soundHolder.alternateMeleeMissSound.CloneArray();
        }

        public override void StartMelee()
        {
            if (owner.frame > 5)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else if (hero.DoingMelee)
            {
                hero.MeleeFollowUp = true;
            }
            hero.StartMeleeCommon();
            hero.SplitKick = Mathf.Abs(owner.xI) < 30f;
            hero.HasJumpedForKick = !owner.IsOnGround();
        }

        public override void AnimateMelee()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.RollingFrames = 0;
            if (owner.frame == 7 && hero.MeleeFollowUp)
            {
                owner.counter -= 0.08f;
                owner.frame = 1;
                hero.MeleeFollowUp = false;
                hero.ResetMeleeValues();
            }
            hero.FrameRate = frameRate;
            if (owner.frame == 2 && hero.NearbyMook != null && hero.NearbyMook.CanBeThrown() && hero.HighFive)
            {
                hero.CancelMelee();
                hero.ThrowBackMook(hero.NearbyMook);
                hero.NearbyMook = null;
            }
            if (hero.JumpingMelee)
            {
                if (owner.frame == 5 && !owner.IsOnGround())
                {
                    owner.counter -= 0.066f;
                }
                if (hero.HighFive && !owner.IsOnGround() && owner.frame > 6)
                {
                    owner.frame = 6;
                }
            }
            else if (owner.frame == 6)
            {
                owner.counter -= 0.0334f;
            }
            if (owner.frame > 4 && !hero.MeleeHasHit)
            {
                PerformVanDammeKickMelee(true, true);
            }
            if (owner.frame == 2 && !hero.HasJumpedForKick)
            {
                if (owner.IsOnGround())
                {
                    owner.yI = 150f;
                }
                hero.HasJumpedForKick = true;
            }
            int num = animationRow;
            if (hero.JumpingMelee)
            {
                if (owner.yI <= 0f)
                {
                    num = jumpingFallingRow;
                }
                else
                {
                    num = animationRow;
                }
            }
            if (hero.SplitKick)
            {
                num = splitKickRow;
            }
            int num2 = animationColumn + owner.frame;
            hero.Sprite.SetLowerLeftPixel((float)(num2 * hero.SpritePixelWidth), (float)(hero.SpritePixelHeight * num));
            if (owner.frame > 7)
            {
                owner.frame = 7;
                hero.CancelMelee();
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
                    else if (owner.frame <= 5)
                    {
                    }
                }
            }
        }

        private void PerformVanDammeKickMelee(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, X + (float)(owner.Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            List<Unit> list = new List<Unit>();
            hero.KickDoors(24f);
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
            int num = owner.Direction;
            int num2 = 3;
            int num3 = num2 + num2 + 1;
            float num4 = CalculateKickForce();
            float num5 = 250f;
            bool splitkick = hero.SplitKick;
            if (owner.yI < 0f && hero.JumpingMelee && !splitkick)
            {
                num5 = -250f;
            }
            BloodColor bloodColor = BloodColor.None;
            if (Map.HitUnits(owner, owner, PlayerNum, 5, DamageType.Melee, (float)num2, 9f, X + (float)(num * num3), Y, (float)num * num4, num5, false, true, false, false, ref bloodColor, list, false))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            if (splitkick)
            {
                num = -num;
                if (Map.HitUnits(owner, owner, PlayerNum, 5, DamageType.Melee, (float)num2, 9f, X + (float)(num * (num3 + 4)), Y, (float)num * num4, 250f, false, true, false, false, ref bloodColor, list, false))
                {
                    sound.PlaySoundEffectAt(alternateMeleeHitSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
                    hero.MeleeHasHit = true;
                }
            }
            if (hero.MeleeHasHit)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    bool flag2 = false;
                    for (int j = i + 1; j < list.Count; j++)
                    {
                        if (list[i] == list[j])
                        {
                            flag2 = true;
                        }
                    }
                    if (!flag2)
                    {
                        list[i].timesKickedByVanDammeSinceLanding++;
                        if (list[i].timesKickedByVanDammeSinceLanding > 2)
                        {
                            Utility.AchievementManager.AwardAchievement(Utility.Achievement.broadway, PlayerNum);
                        }
                    }
                }
            }
            if (!hero.MeleeHasHit && !hero.HasPlayedMissSound)
            {
                sound.PlaySoundEffectAt(alternateMeleeMissSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                hero.HasPlayedMissSound = true;
            }
        }

        private float CalculateKickForce()
        {
            VanDammeBro vanDammeBro = owner as VanDammeBro;
            if (vanDammeBro != null)
            {
                return vanDammeBro.CallMethod<float>("CalculateKickForce");
            }
            return 350f + Mathf.Abs(owner.xI);
        }
    }
}

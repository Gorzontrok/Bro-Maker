using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("BroniversalSoldier")]
    public class BroniversalSoldierMelee : MeleeAbility
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var broniversalSoldier = owner as BroniversalSoldier;
            if (broniversalSoldier == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BroniversalSoldier);
                broniversalSoldier = prefab as BroniversalSoldier;
            }
            if (broniversalSoldier != null)
            {
                meleeHitSounds = broniversalSoldier.soundHolder.meleeHitSound;
                missSounds = broniversalSoldier.soundHolder.missSounds;
                meleeHitTerrainSounds = broniversalSoldier.soundHolder.meleeHitTerrainSound;
                alternateMeleeHitSounds = broniversalSoldier.soundHolder.alternateMeleeHitSound;
            }
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
            owner.SetFieldValue("splitkick", Mathf.Abs(owner.xI) < 30f);
            owner.SetFieldValue("hasJumpedForKick", !owner.IsOnGround());
        }

        public override void AnimateMelee()
        {
            hero.FrameRate = 0.05f;
            hero.SetSpriteOffset(0f, 0f);
            owner.SetFieldValue("rollingFrames", 0);
            if (owner.frame == 7 && hero.MeleeFollowUp)
            {
                owner.counter -= 0.08f;
                owner.frame = 1;
                hero.MeleeFollowUp = false;
                hero.ResetMeleeValues();
            }
            hero.FrameRate = 0.025f;
            if (owner.frame == 2 && owner.GetFieldValue<Mook>("nearbyMook") != null && owner.GetFieldValue<Mook>("nearbyMook").CanBeThrown() && owner.GetFieldValue<bool>("highFive"))
            {
                hero.CancelMelee();
                owner.CallMethod("ThrowBackMook", owner.GetFieldValue<Mook>("nearbyMook"));
                owner.SetFieldValue("nearbyMook", (Mook)null);
            }
            if (hero.JumpingMelee)
            {
                if (owner.frame == 5 && !owner.IsOnGround())
                {
                    owner.counter -= 0.066f;
                }
                if (owner.GetFieldValue<bool>("highFive") && !owner.IsOnGround() && owner.frame > 6)
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
            if (owner.frame == 2 && !owner.GetFieldValue<bool>("hasJumpedForKick"))
            {
                if (owner.IsOnGround())
                {
                    owner.yI = 150f;
                }
                owner.SetFieldValue("hasJumpedForKick", true);
            }
            int num = 9;
            if (hero.JumpingMelee)
            {
                if (owner.yI <= 0f)
                {
                    num = 11;
                }
                else
                {
                    num = 9;
                }
            }
            if (owner.GetFieldValue<bool>("splitkick"))
            {
                num = 10;
            }
            int num2 = 24 + owner.frame;
            hero.Sprite.SetLowerLeftPixel((float)(num2 * hero.SpritePixelWidth), (float)(hero.SpritePixelHeight * num));
            if (owner.frame > 7)
            {
                owner.frame = 7;
                hero.CancelMelee();
            }
        }

        public override void RunMeleeMovement()
        {
            owner.CallMethod("ApplyFallingGravity");
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
            if (shouldTryHitTerrain && TryVanDammeKickHitTerrain())
            {
                hero.MeleeHasHit = true;
            }
            int num = owner.Direction;
            int num2 = 3;
            int num3 = num2 + num2 + 1;
            float num4 = CalculateKickForce();
            float num5 = 250f;
            bool splitkick = owner.GetFieldValue<bool>("splitkick");
            if (owner.yI < 0f && hero.JumpingMelee && !splitkick)
            {
                num5 = -250f;
            }
            BloodColor bloodColor = BloodColor.None;
            if (Map.HitUnits(owner, owner, PlayerNum, 5, DamageType.Melee, (float)num2, 9f, X + (float)(num * num3), Y, (float)num * num4, num5, false, true, false, false, ref bloodColor, list, false))
            {
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            if (splitkick)
            {
                num = -num;
                if (Map.HitUnits(owner, owner, PlayerNum, 5, DamageType.Melee, (float)num2, 9f, X + (float)(num * (num3 + 4)), Y, (float)num * num4, 250f, false, true, false, false, ref bloodColor, list, false))
                {
                    sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 1f, owner.transform.position, 1f, true, false, false, 0f);
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
            if (!hero.MeleeHasHit && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
            {
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeMissSound, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
        }

        private float CalculateKickForce()
        {
            float baseForce = 350f + Mathf.Abs(owner.xI);
            bool serumFrenzy = owner.GetFieldValue<bool>("serumFrenzy");
            return baseForce * (serumFrenzy ? 1.8f : 1f);
        }

        private bool TryVanDammeKickHitTerrain()
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(new Vector3(X - owner.transform.localScale.x * 4f, Y + 4f, 0f), new Vector3(owner.transform.localScale.x, 0f, 0f), out raycastHit, 16f, hero.GroundLayer))
            {
                return false;
            }
            Cage cage = raycastHit.collider.GetComponent<Cage>();
            if (cage == null && raycastHit.collider.transform.parent != null)
            {
                cage = raycastHit.collider.transform.parent.GetComponent<Cage>();
            }
            if (cage != null)
            {
                MapController.Damage_Networked(owner, raycastHit.collider.gameObject, cage.health, DamageType.Melee, 0f, 40f, raycastHit.point.x, raycastHit.point.y);
                return true;
            }
            MapController.Damage_Networked(owner, raycastHit.collider.gameObject, terrainDamage, DamageType.Melee, 0f, 40f, raycastHit.point.x, raycastHit.point.y);
            sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 4f);
            return true;
        }
    }
}

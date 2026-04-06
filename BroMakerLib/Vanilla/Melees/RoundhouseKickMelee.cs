using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("RoundhouseKickMelee")]
    public class RoundhouseKickMelee : MeleeAbility
    {
        [JsonIgnore]
        private float prePauseXI;

        [JsonIgnore]
        private float prePauseYI;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            meleeType = BroBase.MeleeType.ChuckKick;
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
                owner.SetFieldValue("hasJumpedForKick", !owner.IsOnGround());
            }
            else
            {
                owner.SetFieldValue("hasJumpedForKick", true);
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
            hero.FrameRate = 0.025f;
            if (owner.frame == 2 && owner.GetFieldValue<Mook>("nearbyMook") != null && owner.GetFieldValue<Mook>("nearbyMook").CanBeThrown() && owner.GetFieldValue<bool>("highFive"))
            {
                hero.CancelMelee();
                owner.CallMethod("ThrowBackMook", owner.GetFieldValue<Mook>("nearbyMook"));
                owner.SetFieldValue("nearbyMook", null);
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
            if (hero.JumpingMelee && owner.frame > 6 && !owner.IsOnGround() && owner.GetFieldValue<bool>("highFive"))
            {
                owner.frame = 6;
            }
            if (owner.frame == 2 && hero.DoingMelee && !owner.GetFieldValue<bool>("hasJumpedForKick"))
            {
                if (owner.IsOnGround())
                {
                    owner.yI = 150f;
                }
                owner.SetFieldValue("hasJumpedForKick", true);
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
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 0.7f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            if (!hero.MeleeHasHit && shouldTryHitTerrain && TryRoundhouseHitTerrain())
            {
                hero.MeleeHasHit = true;
            }
            if (!hero.MeleeHasHit && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
            {
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeMissSound, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
        }

        private bool TryRoundhouseHitTerrain()
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
            MapController.Damage_Networked(owner, raycastHit.collider.gameObject, 2, DamageType.Melee, 0f, 40f, raycastHit.point.x, raycastHit.point.y);
            sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 4f);
            return true;
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
                }
            }
        }
    }
}

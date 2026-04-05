using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("CherryBroling")]
    public class CherryBrolingMelee : MeleeAbility
    {
        [JsonIgnore]
        private AudioClip[] alternateMeleeHitSounds2;
        [JsonIgnore]
        private AudioClip[] alternateMeleeMissSounds;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var cherryBroling = owner as CherryBroling;
            if (cherryBroling == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.CherryBroling);
                cherryBroling = prefab as CherryBroling;
            }
            if (cherryBroling != null)
            {
                alternateMeleeHitSounds2 = cherryBroling.soundHolder.alternateMeleeHitSound;
                alternateMeleeMissSounds = cherryBroling.soundHolder.alternateMeleeMissSound;
            }
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 10)
            {
                if (hero.DoingMelee)
                {
                    owner.frame = 3;
                }
                else
                {
                    owner.frame = 0;
                }
                owner.counter = -0.05f;
                AnimateMelee();
            }
            if (!hero.JumpingMelee)
            {
                owner.SetFieldValue("dashingMelee", true);
                owner.xI = (float)owner.Direction * owner.speed;
            }
            owner.SetFieldValue("lerpToMeleeTargetPos", 0f);
            hero.DoingMelee = true;
            hero.MeleeHasHit = false;
            owner.SetFieldValue("performedMeleeAttack", false);
            owner.SetFieldValue("hasPlayedMissSound", false);
            owner.SetFieldValue("showHighFiveAfterMeleeTimer", 0f);
            hero.DeactivateGun();
            hero.SetMeleeType();
            owner.SetFieldValue("meleeStartPos", owner.transform.position);
            AnimateMelee();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = 18 + Mathf.Clamp(owner.frame, 0, 11);
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(9 * hero.SpritePixelHeight));
            if (owner.frame > 3 && !hero.MeleeHasHit)
            {
                hero.KickDoors(24f);
            }
            if (owner.frame >= 9 && owner.frame <= 13 && !hero.MeleeHasHit)
            {
                PerformKnifeKickMeleeAttack(true, true);
                hero.FrameRate = 0.0167f;
            }
            if (owner.frame == 9 && !owner.IsOnGround())
            {
                owner.frame = 9;
            }
            if (owner.frame > 13)
            {
                owner.frame = 13;
                if (owner.IsOnGround())
                {
                    hero.CancelMelee();
                }
            }
        }

        public override void RunMeleeMovement()
        {
            if (hero.JumpingMelee)
            {
                owner.CallMethod("ApplyFallingGravity");
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (owner.frame <= 4)
            {
                owner.xI = 0f;
                owner.yI = 0f;
            }
            else if (owner.frame <= 7)
            {
                if (hero.MeleeChosenUnit == null)
                {
                    owner.xI = owner.speed * 1.5f * owner.transform.localScale.x;
                    owner.yI = 50f;
                }
                else
                {
                    owner.xI = owner.speed * 0.5f * owner.transform.localScale.x + (hero.MeleeChosenUnit.X - owner.X) * 6f;
                }
            }
            else if (owner.frame <= 13)
            {
                owner.xI = owner.speed * 0.8f * owner.transform.localScale.x;
                owner.CallMethod("ApplyFallingGravity");
            }
            else
            {
                owner.CallMethod("ApplyFallingGravity");
            }
        }

        private void PerformKnifeKickMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + (float)(owner.Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            Unit unit = Map.HitClosestUnit(owner, PlayerNum, 8, DamageType.Knifed, 10f, 24f, X + owner.transform.localScale.x * 8f, Y + 8f + (float)((owner.yI >= -60f) ? 0 : (-6)), owner.transform.localScale.x * 200f, 500f, true, false, owner.IsMine, false, true);
            if (unit)
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds2, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                hero.CancelMelee();
                owner.SetFieldValue("somersaulting", true);
                owner.SetFieldValue("somersaultFrame", 0);
                owner.actionState = ActionState.Jumping;
                owner.yI = 400f;
                hero.InvulnerableTime = 0.2f;
                owner.CallMethod("AnimateJumping");
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(alternateMeleeMissSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

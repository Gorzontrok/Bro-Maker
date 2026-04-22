using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Cherry Broling's flip-kick melee.</summary>
    [MeleePreset("CherryBroling")]
    public class CherryBrolingMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.CherryBroling;

        public AudioClip[] alternateMeleeHitSounds2;
        public AudioClip[] alternateMeleeMissSounds;

        /// <summary>Whether a successful kick triggers the airborne somersault animation.</summary>
        public bool enableSomersault = true;
        /// <summary>Sprite sheet row for the somersault animation.</summary>
        public int somersaultAnimationRow = 8;
        /// <summary>Starting column of the somersault animation.</summary>
        public int somersaultAnimationColumn = 0;
        /// <summary>Number of somersault animation frames played.</summary>
        public int somersaultFrameCount = 11;
        /// <summary>Seconds per somersault frame.</summary>
        public float somersaultFrameRate = 0.04f;

        [JsonIgnore]
        private bool somersaulting;
        [JsonIgnore]
        private int somersaultFrame;

        public CherryBrolingMelee()
        {
            meleeType = BroBase.MeleeType.FlipKick;
            startType = MeleeStartType.Custom;
            restartFrame = 3;
            animationRow = 9;
            animationColumn = 18;
            frameRate = 0.0167f;
            damageType = "Knifed";
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (alternateMeleeHitSounds2 == null) alternateMeleeHitSounds2 = sourceBro.soundHolder.alternateMeleeHitSound.CloneArray();
            if (alternateMeleeMissSounds == null) alternateMeleeMissSounds = sourceBro.soundHolder.alternateMeleeMissSound.CloneArray();
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
                hero.DashingMelee = true;
                owner.xI = (float)owner.Direction * owner.speed;
            }
            owner.SetFieldValue("lerpToMeleeTargetPos", 0f);
            hero.DoingMelee = true;
            hero.MeleeHasHit = false;
            hero.PerformedMeleeAttack = false;
            hero.HasPlayedMissSound = false;
            hero.ShowHighFiveAfterMeleeTimer = 0f;
            hero.DeactivateGun();
            hero.SetMeleeType();
            owner.SetFieldValue("meleeStartPos", owner.transform.position);
            AnimateMelee();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 11);
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(animationRow * hero.SpritePixelHeight));
            if (owner.frame > 3 && !hero.MeleeHasHit)
            {
                hero.KickDoors(24f);
            }
            if (owner.frame >= 9 && owner.frame <= 13 && !hero.MeleeHasHit)
            {
                PerformKnifeKickMeleeAttack(true, true);
                hero.FrameRate = frameRate;
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
                hero.ApplyFallingGravity();
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
                hero.ApplyFallingGravity();
            }
            else
            {
                hero.ApplyFallingGravity();
            }
        }

        private void PerformKnifeKickMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + (float)(owner.Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            Unit unit = Map.HitClosestUnit(owner, PlayerNum, 8, parsedDamageType, 10f, 24f, X + owner.transform.localScale.x * 8f, Y + 8f + (float)((owner.yI >= -60f) ? 0 : (-6)), owner.transform.localScale.x * 200f, 500f, true, false, owner.IsMine, false, true);
            if (unit)
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds2, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                hero.CancelMelee();
                if (enableSomersault)
                {
                    somersaulting = true;
                    somersaultFrame = 0;
                }
                owner.actionState = ActionState.Jumping;
                owner.yI = 400f;
                hero.InvulnerableTime = 0.2f;
                hero.AnimateJumping();
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(alternateMeleeMissSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && TryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }

        public override bool HandleUpdate()
        {
            if (somersaulting && (owner.actionState != ActionState.Jumping || hero.WallDrag))
            {
                somersaulting = false;
                somersaultFrame = 0;
            }
            return true;
        }

        public override void HandleAfterLand()
        {
            somersaulting = false;
            somersaultFrame = 0;
        }

        public override bool HandleAnimateActualJumpingFrames()
        {
            if (!somersaulting)
            {
                return true;
            }
            hero.DeactivateGun();
            int col = somersaultAnimationColumn + somersaultFrame;
            hero.Sprite.SetLowerLeftPixel((float)(col * hero.SpritePixelWidth), (float)(somersaultAnimationRow * hero.SpritePixelHeight));
            somersaultFrame++;
            if (somersaultFrame >= somersaultFrameCount)
            {
                somersaulting = false;
            }
            hero.FrameRate = somersaultFrameRate;
            return false;
        }

        public override bool HandleRunFiring()
        {
            return !somersaulting;
        }
    }
}

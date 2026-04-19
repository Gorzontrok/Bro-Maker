using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Shared base for punch melee. Used by: McBrover, BroMax, DoubleBroSeven, BurtBrommer, TheBrolander.</summary>
    [MeleePreset("Punch")]
    public class PunchMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.MadMaxBrotansky;

        /// <summary>Forward offset from the bro center to the punch hit origin.</summary>
        public float punchOffset = 6f;
        public int normalDamage = 4;
        public int jetpackDamage = 10;
        public float normalKnockbackX = 250f;
        public float jetpackKnockbackX = 600f;
        public int normalTerrainDamage = 2;
        public int jetpackTerrainDamage = 10;
        /// <summary>Door kick range for normal punch.</summary>
        public float normalKickRange = 25f;
        /// <summary>Door kick range for jetpack punch.</summary>
        public float jetpackKickRange = 50f;
        /// <summary>Y velocity added on jetpack-punch hit or terrain hit.</summary>
        public float jetpackHitYBoost = 80f;
        /// <summary>Horizontal blast impulse applied on jetpack-punch hit or terrain hit.</summary>
        public float jetpackHitXBlast = -90f;
        /// <summary>Volume for alternate melee hit sound.</summary>
        public float hitSoundVolume = 0.5f;
        /// <summary>Volume for miss sound.</summary>
        public float missSoundVolume = 0.15f;

        public PunchMelee()
        {
            meleeType = BroBase.MeleeType.Punch;
            moveType = MeleeMoveType.Punch;
            restartFrame = 0;
            knockbackY = 250f;
            animationRow = 9;
            animationFrameCount = 9;
            damageType = "Melee";
        }

        public override void StartMelee()
        {
            bool canStartNew = !hero.DoingMelee || owner.frame > 4;
            if (canStartNew)
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
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            if (owner.frame == 2)
            {
                Mook nearbyMook = hero.NearbyMook;
                if (nearbyMook != null && nearbyMook.CanBeThrown() && hero.HighFive)
                {
                    hero.CancelMelee();
                    hero.ThrowBackMook(nearbyMook);
                    hero.NearbyMook = null;
                    return;
                }
            }
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = animationRow;
            if (owner.frame == 5)
            {
                owner.counter -= 0.0334f;
                owner.counter -= 0.0334f;
                owner.counter -= 0.0334f;
            }
            if (owner.frame == 3)
            {
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3 && !hero.MeleeHasHit)
            {
                PerformPunchAttack(true, true);
            }
            if (hero.CurrentMeleeType == BroBase.MeleeType.JetpackPunch && owner.frame >= 4 && owner.frame <= 5 && !hero.MeleeHasHit)
            {
                PerformPunchAttack(true, true);
            }
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        protected virtual void PerformPunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool isJetpackPunch = hero.CurrentMeleeType == BroBase.MeleeType.JetpackPunch;
            Vector3 vector = new Vector3(owner.X + (float)owner.Direction * (punchOffset + 7f), owner.Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(isJetpackPunch ? jetpackKickRange : normalKickRange);
            int damage = isJetpackPunch ? jetpackDamage : normalDamage;
            float kbX = isJetpackPunch ? jetpackKnockbackX : normalKnockbackX;
            if (Map.HitClosestUnit(owner, owner.playerNum, damage, parsedDamageType, punchOffset, punchOffset * 2f, vector.x, vector.y, owner.transform.localScale.x * kbX, knockbackY, true, false, owner.IsMine, false, true))
            {
                if (isJetpackPunch)
                {
                    SortOfFollow.Shake(0.3f, 1.5f);
                }
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, hitSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(owner.X + (owner.width + 4f) * owner.transform.localScale.x, owner.Y + owner.height + 4f);
                if (isJetpackPunch)
                {
                    owner.yI += jetpackHitYBoost;
                    owner.xIBlast = jetpackHitXBlast * owner.transform.localScale.x;
                }
            }
            else
            {
                if (playMissSound && !hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, missSoundVolume, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            hero.MeleeChosenUnit = null;
            int tdmg = isJetpackPunch ? jetpackTerrainDamage : normalTerrainDamage;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && TryMeleeTerrain(0, tdmg))
            {
                hero.MeleeHasHit = true;
                if (isJetpackPunch)
                {
                    owner.yI += jetpackHitYBoost;
                    owner.xIBlast = jetpackHitXBlast * owner.transform.localScale.x;
                }
            }
            hero.TriggerBroMeleeEvent();
        }
    }
}

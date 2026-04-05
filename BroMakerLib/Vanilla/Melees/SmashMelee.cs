using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using Rogueforce;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>
    /// Shared base for the dual-mode smash melee used by BronanTheBrobarian, Brominator, and TankBro.
    /// Ground mode plays an uppercut punch animation; aerial mode performs a targeted dive smash.
    /// Subclasses set per-bro defaults via public configurable fields and virtual hook methods.
    /// </summary>
    public abstract class SmashMelee : MeleeAbility
    {
        // Uppercut hit parameters
        public int upperCutDamage = 4;
        public float upperCutXI = 10f;
        public float upperCutYI = 750f;

        // Smash blast parameters (MakeSmashBlast)
        public float smashExplodeRange = 64f;
        public float smashExplodeHeight = 20f;
        public float smashExplodeXI = 300f;
        public float smashExplodeYI = 240f;
        public float smashGroundDamageRange = 25f;
        public float smashGroundWaveSize = 80f;

        // Runtime state — not serialized
        [JsonIgnore]
        private bool smashing;

        [JsonIgnore]
        private float smashingTime;

        /// <summary>The sprite sheet row used by the current punch animation. Set by <see cref="SetPunchAnimationRow"/> each time StartMelee fires.</summary>
        [JsonIgnore]
        protected int currentPunchAnimationRow = 9;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
        }

        public override void StartMelee()
        {
            SetPunchAnimationRow();

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
            OnAfterStartMeleeCommon();

            smashing = hero.JumpingMelee && ShouldSmash();
            if (smashing)
            {
                smashingTime = Time.time;
                owner.actionState = ActionState.Jumping;
                float searchWidth = 24f;
                float searchHeight = 128f;
                float searchX = X + searchWidth * Direction - 6f;
                float searchY = Y - searchHeight;
                List<Unit> unitsInRange = Map.GetUnitsInRange(searchWidth, searchHeight, searchX, searchY, true);
                hero.MeleeChosenUnit = null;
                foreach (Unit unit in unitsInRange)
                {
                    float dx = Mathf.Abs(unit.X - X);
                    float dy = Mathf.Abs(unit.Y - Y);
                    RaycastHit raycastHit;
                    if (dx * 1.5f < dy && unit.Y < Y && Physics.Raycast(owner.transform.position, unit.transform.position, out raycastHit, dy + 16f, Map.groundLayer))
                    {
                        if (raycastHit.distance > Mathf.Abs(dy) - 20f)
                        {
                            hero.MeleeChosenUnit = unit;
                        }
                        break;
                    }
                }
                OnAfterSmashSetup();
            }
        }

        /// <summary>Called after the smash target search in StartMelee. Override to add post-setup logic (e.g., drift velocity when no target found).</summary>
        protected virtual void OnAfterSmashSetup()
        {
        }

        /// <summary>Called at the start of StartMelee to choose the punch animation row for this melee. Sets <see cref="currentPunchAnimationRow"/>.</summary>
        protected virtual void SetPunchAnimationRow()
        {
        }

        /// <summary>Called after StartMeleeCommon inside StartMelee. Override to play sounds or set state flags.</summary>
        protected virtual void OnAfterStartMeleeCommon()
        {
        }

        /// <summary>Returns whether aerial smash mode should activate. Override to restrict smash (e.g., disallow from ladder).</summary>
        protected virtual bool ShouldSmash()
        {
            return true;
        }

        public override void AnimateMelee()
        {
            if (!smashing)
            {
                AnimateBronanPunch();
                return;
            }
            hero.AnimateMeleeCommon();
            int col = 24 + Mathf.Clamp(owner.frame, 0, 8);
            int row = 11;
            if (owner.frame == 4)
            {
                owner.counter -= 0.3f;
            }
            if (owner.GetFieldValue<bool>("highFive") && owner.frame > 4 && !owner.IsOnGround())
            {
                owner.frame = 4;
            }
            hero.Sprite.SetLowerLeftPixel((float)(col * hero.SpritePixelWidth), (float)(row * hero.SpritePixelHeight));
            if (owner.frame >= 7)
            {
                hero.CancelMelee();
            }
        }

        protected virtual void AnimateBronanPunch()
        {
            hero.AnimateMeleeCommon();
            int col = 24 + Mathf.Clamp(owner.frame, 0, 8);
            int row = currentPunchAnimationRow;
            hero.Sprite.SetLowerLeftPixel((float)(col * hero.SpritePixelWidth), (float)(row * hero.SpritePixelHeight));
            if (owner.frame == 4)
            {
                owner.counter = -0.066f;
            }
            AnimatePunchHitDetection();
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
            AnimatePunchMissSound();
        }

        /// <summary>Called inside AnimateBronanPunch to run hit detection. Override to change which frames fire the uppercut.</summary>
        protected virtual void AnimatePunchHitDetection()
        {
            if (owner.frame >= 3 && owner.frame < 6 && !hero.MeleeHasHit)
            {
                PerformUpperCut(true, true);
            }
        }

        /// <summary>Called at the end of AnimateBronanPunch for miss sound logic. Default is no-op; override per bro.</summary>
        protected virtual void AnimatePunchMissSound()
        {
        }

        public override void RunMeleeMovement()
        {
            if (!smashing)
            {
                owner.CallMethod("RunPunchMovement");
                return;
            }
            if (!owner.IsOnGround())
            {
                owner.CallMethod("ApplyFallingGravity");
            }
            if (owner.frame > 2 && !owner.IsOnGround())
            {
                owner.maxFallSpeed = owner.GetFieldValue<float>("originalMaxFallSpeed") * 1.3f;
                owner.yI -= 1100f * hero.DeltaTime;
            }
            if (owner.yI < owner.maxFallSpeed)
            {
                owner.yI = owner.maxFallSpeed;
            }
            if (hero.MeleeChosenUnit == null)
            {
                owner.xI *= 1f - hero.DeltaTime * 6f;
            }
            else
            {
                float dx = hero.MeleeChosenUnit.X - X;
                owner.xI = dx / 0.1f;
                owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
            }
            SmashPerformAttackCheck();
            if (owner.IsOnGround())
            {
                if (owner.frame < 5)
                {
                    owner.frame = 5;
                    AnimateMelee();
                    owner.counter = 0f;
                }
                if (Time.time - smashingTime > 0.36f)
                {
                    hero.SetInvulnerable(0.2f, true, false);
                    MakeSmashBlast(X, Y, true);
                    owner.counter = -0.2f;
                    owner.SetFieldValue("stunTime", 0.06f);
                    smashingTime = Time.time;
                }
            }
        }

        /// <summary>Called each frame while smashing to decide when to fire PerformSmashAttack. Override to add additional guards (e.g., must not be grounded).</summary>
        protected virtual void SmashPerformAttackCheck()
        {
            if (owner.frame == 4 && !hero.MeleeHasHit)
            {
                PerformSmashAttack();
            }
        }

        protected void PerformUpperCut(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + Direction * 4f, Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(24f);
            List<Unit> list = new List<Unit>();
            if (Map.HitUnits(owner, PlayerNum, upperCutDamage, DamageType.Melee, 6f, X + Direction * 8f, Y + 8f, owner.xI + Direction * upperCutXI, owner.yI + upperCutYI, false, false, false, list, false, true))
            {
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                OnUpperCutMissSound();
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                OnUpperCutTerrainHit();
                hero.MeleeHasHit = true;
            }
        }

        /// <summary>Called when the uppercut misses all units and playMissSound is true. Override to play a miss sound.</summary>
        protected virtual void OnUpperCutMissSound()
        {
        }

        /// <summary>Called after TryMeleeTerrain succeeds in the uppercut. Override to play a terrain-hit sound.</summary>
        protected virtual void OnUpperCutTerrainHit()
        {
        }

        protected void PerformSmashAttack()
        {
            if (!owner.GetFieldValue<bool>("hasPlayedMissSound"))
            {
                sound.PlaySoundEffectAt(soundHolder.alternateMeleeMissSound, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
            float range = 8f;
            Vector3 center = new Vector3(X + Direction * range, Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, center.x, center.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            if (Map.HitClosestUnit(owner, PlayerNum, 10, DamageType.Crush, range, range * 2f, center.x, center.y, owner.transform.localScale.x * 20f, 50f, true, false, owner.IsMine, true, true))
            {
                if (!hero.MeleeHasHit)
                {
                    sound.PlaySoundEffectAt(soundHolder.alternateMeleeHitSound, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.MeleeHasHit = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
            hero.TriggerBroMeleeEvent();
        }

        protected virtual void MakeSmashBlast(float xPoint, float yPoint, bool groundWave)
        {
            Map.ExplodeUnits(owner, 10, DamageType.Crush, smashExplodeRange, smashExplodeHeight, xPoint, yPoint, smashExplodeXI, smashExplodeYI, PlayerNum, false, false, true);
            MapController.DamageGround(owner, ValueOrchestrator.GetModifiedDamage(15, PlayerNum), DamageType.Explosion, smashGroundDamageRange, xPoint, yPoint, null, false);
            EffectsController.CreateWhiteFlashPop(xPoint, yPoint);
            owner.PlaySpecial3Sound(0.25f);
            if (groundWave)
            {
                EffectsController.CreateGroundWave(xPoint, yPoint + 1f, smashGroundWaveSize);
                Map.ShakeTrees(X, Y, 64f, 32f, 64f);
            }
            Map.DisturbWildLife(X, Y, 48f, PlayerNum);
        }

        public override void CancelMelee()
        {
            smashing = false;
        }
    }

    [MeleePreset("BronanTheBrobarian")]
    public class BronanMelee : SmashMelee
    {
        [JsonIgnore]
        private int punchCount;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var bronan = owner as BronanTheBrobarian;
            if (bronan == null)
            {
                bronan = HeroController.GetHeroPrefab(HeroType.BronanTheBrobarian) as BronanTheBrobarian;
            }
            if (bronan != null)
            {
                soundHolder.alternateMeleeHitSound = bronan.soundHolder.alternateMeleeHitSound;
                soundHolder.missSounds = bronan.soundHolder.missSounds;
                soundHolder.alternateMeleeMissSound = bronan.soundHolder.alternateMeleeMissSound;
            }
        }

        protected override void SetPunchAnimationRow()
        {
            currentPunchAnimationRow = (punchCount % 2 == 0) ? 9 : 10;
        }

        protected override void OnAfterStartMeleeCommon()
        {
            sound.PlaySoundEffectAt(soundHolder.missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            owner.SetFieldValue("hasPlayedMissSound", true);
        }

        protected override void AnimatePunchMissSound()
        {
            if (!hero.MeleeHasHit && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
            {
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
        }
    }
}

using System.Collections.Generic;
using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using Rogueforce;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Abstract shared base for smash melee. Extended by: BrominatorMelee, TankBroMelee, BronanMelee.</summary>
    public abstract class SmashMelee : MeleeAbility
    {
        /// <summary>Damage dealt by the ground uppercut.</summary>
        public int upperCutDamage = 4;
        /// <summary>Horizontal impulse applied to the target on uppercut hit.</summary>
        public float upperCutXI = 10f;
        /// <summary>Vertical impulse applied to the target on uppercut hit.</summary>
        public float upperCutYI = 750f;

        /// <summary>Horizontal radius of the explosion triggered on landing the aerial smash.</summary>
        public float smashExplodeRange = 64f;
        /// <summary>Vertical radius of the explosion triggered on landing the aerial smash.</summary>
        public float smashExplodeHeight = 20f;
        /// <summary>Horizontal impulse of the smash landing explosion.</summary>
        public float smashExplodeXI = 300f;
        /// <summary>Vertical impulse of the smash landing explosion.</summary>
        public float smashExplodeYI = 240f;
        /// <summary>Radius of the ground damage ring on smash landing.</summary>
        public float smashGroundDamageRange = 25f;
        /// <summary>Width of the ground-wave visual effect on smash landing.</summary>
        public float smashGroundWaveSize = 80f;

        /// <summary>Sprite sheet row used for the aerial smash animation.</summary>
        public int smashAnimationRow = 11;

        public AudioClip[] alternateMeleeMissSounds;
        public AudioClip[] alternateMeleeHitSounds2;
        public AudioClip[] special3Sounds;

        [JsonIgnore]
        private bool smashing;

        [JsonIgnore]
        private float smashingTime;

        [JsonIgnore]
        protected int currentPunchAnimationRow = 9;

        public SmashMelee()
        {
            meleeType = BroBase.MeleeType.Smash;
            animationColumn = 24;
            damageType = "Crush";
        }

        public override void Initialize(BroBase owner)
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

        /// <summary>Called at the start of StartMelee to choose the punch animation row for this melee. Sets `currentPunchAnimationRow`.</summary>
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
            int col = animationColumn + Mathf.Clamp(owner.frame, 0, 8);
            int row = smashAnimationRow;
            if (owner.frame == 4)
            {
                owner.counter -= 0.3f;
            }
            if (hero.HighFive && owner.frame > 4 && !owner.IsOnGround())
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
            int col = animationColumn + Mathf.Clamp(owner.frame, 0, 8);
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
                RunPunchMovement();
                return;
            }
            if (!owner.IsOnGround())
            {
                hero.ApplyFallingGravity();
            }
            if (owner.frame > 2 && !owner.IsOnGround())
            {
                owner.maxFallSpeed = hero.OriginalMaxFallSpeed * 1.3f;
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
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                OnUpperCutMissSound();
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && TryMeleeTerrain(0, 2))
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
            if (!hero.HasPlayedMissSound)
            {
                sound.PlaySoundEffectAt(alternateMeleeMissSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                hero.HasPlayedMissSound = true;
            }
            float range = 8f;
            Vector3 center = new Vector3(X + Direction * range, Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, center.x, center.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            if (Map.HitClosestUnit(owner, PlayerNum, 10, parsedDamageType, range, range * 2f, center.x, center.y, owner.transform.localScale.x * 20f, 50f, true, false, owner.IsMine, true, true))
            {
                if (!hero.MeleeHasHit)
                {
                    sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.MeleeHasHit = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && TryMeleeTerrain(0, 2))
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
            sound.PlaySoundEffectAt(special3Sounds, 0.25f, owner.transform.position, 1f, true, false, false, 0f);
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

        private void RunPunchMovement()
        {
            hero.ApplyFallingGravity();
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
                        float num2 = hero.MeleeChosenUnit.X - (float)owner.Direction * 8f - X;
                        if (!hero.IsInQuicksand)
                        {
                            owner.xI = num2 / 0.1f;
                            owner.xI = Mathf.Clamp(owner.xI, -owner.speed * 1.7f, owner.speed * 1.7f);
                        }
                    }
                    else
                    {
                        if (!hero.IsInQuicksand)
                        {
                            owner.xI = owner.speed * (float)owner.Direction;
                        }
                        owner.yI = 0f;
                    }
                }
                else if (owner.frame <= 7)
                {
                    owner.xI = 0f;
                }
            }
            else if (Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }
    }

    /// <summary>Bronan the Brobarian's smash melee.</summary>
    [MeleePreset("BronanTheBrobarian")]
    public class BronanMelee : SmashMelee
    {
        protected override HeroType SourceBroType => HeroType.BronanTheBrobarian;
        [JsonIgnore]
#pragma warning disable CS0649 // Never assigned, intentional
        private int punchCount;
#pragma warning restore CS0649

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro != null)
            {
                if (alternateMeleeMissSounds == null) alternateMeleeMissSounds = sourceBro.soundHolder.alternateMeleeMissSound.CloneArray();
                if (special3Sounds == null) special3Sounds = sourceBro.soundHolder.special3Sounds.CloneArray();
            }
        }

        protected override void SetPunchAnimationRow()
        {
            currentPunchAnimationRow = (punchCount % 2 == 0) ? 9 : 10;
        }

        protected override void OnAfterStartMeleeCommon()
        {
            sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            hero.HasPlayedMissSound = true;
        }

        protected override void AnimatePunchMissSound()
        {
            if (!hero.MeleeHasHit && !hero.HasPlayedMissSound)
            {
                hero.HasPlayedMissSound = true;
            }
        }
    }
}

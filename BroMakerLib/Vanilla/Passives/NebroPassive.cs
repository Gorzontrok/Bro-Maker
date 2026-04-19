using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Passives
{
    /// <summary>Nebro's airdash.</summary>
    [PassivePreset("MrAnderbro")]
    [ConflictsWithPreset("BroLee")]
    public class NebroPassive : AirdashPassive
    {
        protected override HeroType SourceBroType => HeroType.Nebro;

        protected override bool IsOwnerRedundant(BroBase owner) => owner is Nebro;

        public NebroPassive()
        {
            airdashMaxTime = 0.3f;
            animationRow = 8;
            frameRate = 0.066f;
        }

        /// <summary>Damage dealt per tick during a horizontal airdash.</summary>
        public int horizontalDashDamage = 1;
        /// <summary>Damage dealt per tick during an upward or downward airdash.</summary>
        public int verticalDashDamage = 3;
        /// <summary>Hit radius of the per-tick dash damage, in world units.</summary>
        public float dashDamageRange = 9f;
        /// <summary>Minimum seconds between fader-trail instances spawned during a dash.</summary>
        public float airdashFadeRate = 0.05f;
        /// <summary>Seconds of invulnerability granted after a downward-dash slam landing.</summary>
        public float downSlamInvulnerabilityTime = 0.36f;
        /// <summary>Disable for bros whose sprite sheet doesn't match Nebro's stamp layout.</summary>
        public bool enableStampAnimation = true;
        public int stampAnimationStartColumn = 6;
        public int stampAnimationEndFrame = 7;
        /// <summary>How long the landing-stamp animation plays before returning to idle, in seconds.</summary>
        public float stampDuration = 0.4f;
        /// <summary>Damage dealt by the explosion on dash impact.</summary>
        public int blastDamage = 25;
        /// <summary>Horizontal radius of the dash-impact explosion, in world units.</summary>
        public float blastXRange = 64f;
        /// <summary>Vertical radius of the dash-impact explosion, in world units.</summary>
        public float blastYRange = 20f;
        /// <summary>Horizontal impulse applied to units hit by the dash-impact explosion.</summary>
        public float blastXI = 300f;
        /// <summary>Vertical impulse applied to units hit by the dash-impact explosion.</summary>
        public float blastYI = 240f;
        /// <summary>Radius for ground damage at the dash-impact point, in world units.</summary>
        public float blastGroundRange = 25f;
        /// <summary>Damage applied to terrain at the dash-impact point.</summary>
        public int blastGroundDamage = 15;
        /// <summary>Sounds played when an airdash begins.</summary>
        public AudioClip[] specialAttackSounds;
        /// <summary>Sounds played during the airdash charge-up.</summary>
        public AudioClip[] panicSounds;

        [JsonIgnore] private float specialAttackDashCounter;
        [JsonIgnore] private float airdashFadeCounter;
        [JsonIgnore] private float stampDelay;

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as Nebro;
            if (sourceBro == null) return;
            // CreateFaderTrailInstance reads owner.faderSpritePrefab; Rambro's is null.
            if (owner.faderSpritePrefab == null) owner.faderSpritePrefab = sourceBro.faderSpritePrefab;
            if (specialAttackSounds == null) specialAttackSounds = sourceBro.soundHolder.specialAttackSounds.CloneArray();
            if (panicSounds == null) panicSounds = sourceBro.soundHolder.panic.CloneArray();
        }

        public override bool HandlePressDashButton()
        {
            if (!hero.CanAirdash) return true;
            bool wasdash = hero.WasDashButton;
            if (wasdash) return true;

            if (owner.up && CanAirDash(DirectionEnum.Up)) { Airdash(true); }
            else if (owner.right && CanAirDash(DirectionEnum.Right)) { Airdash(true); }
            else if (owner.left && CanAirDash(DirectionEnum.Left)) { Airdash(true); }
            else if (owner.down && CanAirDash(DirectionEnum.Down)) { Airdash(true); }
            return true;
        }

        public override bool HandleJump(bool wallJump)
        {
            if (hero.HoldingHighFive && hero.AirdashUpAvailable)
            {
                owner.up = true;
                Airdash(true);
                return false;
            }
            return true;
        }

        public override void HandleAfterRunLeftAirDash()
        {
            TickDashDamage(horizontalDashDamage, 0f, owner.xI * 0.3f, 500f + Random.value * 200f);
            TickFaderTrail();
        }

        public override void HandleAfterRunRightAirDash()
        {
            TickDashDamage(horizontalDashDamage, 0f, owner.xI * 0.3f, 500f + Random.value * 200f);
            TickFaderTrail();
        }

        public override bool HandleRunUpwardDash()
        {
            if (specialAttackDashCounter > 0f)
            {
                specialAttackDashCounter -= 0.0333f;
                Map.HitUnits(owner, owner, PlayerNum, verticalDashDamage, DamageType.Crush, dashDamageRange,
                    X, Y + 8f, 0f, 200f, true, true);
            }
            return true;
        }

        public override void HandleAfterRunUpwardDash()
        {
            airdashFadeCounter += Time.deltaTime;
            if (airdashFadeCounter > airdashFadeRate)
            {
                airdashFadeCounter -= airdashFadeRate;
                hero.CreateFaderTrailInstance();
            }
        }

        public override bool HandlePlayAidDashSound()
        {
            if (specialAttackSounds == null || specialAttackSounds.Length == 0) return true;
            Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
            return false;
        }

        public override bool HandlePlayAirDashChargeUpSound()
        {
            if (panicSounds == null || panicSounds.Length == 0) return true;
            Sound.GetInstance().PlaySoundEffectAt(panicSounds, 0.6f, owner.transform.position, 1f, true, false, false, 0f);
            return false;
        }

        public override void HandleAfterRunDownwardDash()
        {
            specialAttackDashCounter += hero.DeltaTime;
            if (specialAttackDashCounter > 0f)
            {
                specialAttackDashCounter -= 0.0333f;
                Map.HitUnits(owner, owner, PlayerNum, verticalDashDamage, DamageType.Crush, dashDamageRange,
                    X, Y - 5f, 0f, 200f, true, true);
            }
            if (owner.yI < -100f)
            {
                airdashFadeCounter += Time.deltaTime;
                if (airdashFadeCounter > airdashFadeRate)
                {
                    airdashFadeCounter -= airdashFadeRate;
                    hero.CreateFaderTrailInstance();
                }
                if (Y < owner.groundHeight + 30f)
                {
                    hero.SetInvulnerable(0.35f, false, false);
                }
            }
        }

        public override bool HandleHitRightWall()
        {
            float airdashTime = hero.AirdashTime;
            DirectionEnum airdashDirection = hero.AirdashDirection;
            if (airdashTime > 0f && airdashDirection == DirectionEnum.Right)
            {
                MakeDashBlast(X + 7f, Y + 5f, true);
                owner.xIBlast = -100f;
                owner.yI += 50f;
                hero.AirdashDirection = DirectionEnum.Any;
                hero.AirdashTime = 0f;
                return false;
            }
            return true;
        }

        public override bool HandleHitLeftWall()
        {
            float airdashTime = hero.AirdashTime;
            DirectionEnum airdashDirection = hero.AirdashDirection;
            if (airdashTime > 0f && airdashDirection == DirectionEnum.Left)
            {
                MakeDashBlast(X - 7f, Y + 5f, true);
                owner.xIBlast = 100f;
                owner.yI += 50f;
                hero.AirdashDirection = DirectionEnum.Any;
                hero.AirdashTime = 0f;
                return false;
            }
            return true;
        }

        public override bool HandleHitCeiling()
        {
            float airdashTime = hero.AirdashTime;
            DirectionEnum airdashDirection = hero.AirdashDirection;
            if (airdashTime > 0f && airdashDirection == DirectionEnum.Up)
            {
                MakeDashBlast(X, Y + owner.headHeight + 5f, false);
                hero.AirdashTime = 0f;
            }
            return true;
        }

        public override bool HandleLand()
        {
            owner.CallMethod("SetAirdashAvailable");

            float airdashTime = hero.AirdashTime;
            DirectionEnum airdashDirection = hero.AirdashDirection;
            if (airdashTime > 0f && airdashDirection == DirectionEnum.Down)
            {
                MakeDashBlast(X, owner.groundHeight, true);
                float groundHeightGround = owner.CallMethod<float>("GetGroundHeightGround");
                if (Mathf.Abs(groundHeightGround - Y) < 24f)
                {
                    owner.Y = groundHeightGround;
                    owner.groundHeight = groundHeightGround;
                }
                if (enableStampAnimation)
                {
                    stampDelay = stampDuration;
                    owner.frame = -3;
                }
                hero.SetInvulnerable(downSlamInvulnerabilityTime, false, false);
            }
            return true;
        }

        public override bool HandleAnimateIdle()
        {
            if (!enableStampAnimation || stampDelay <= 0f) return true;
            hero.DeactivateGun();
            hero.FrameRate = frameRate;
            hero.Sprite.SetLowerLeftPixel(
                (float)((stampAnimationStartColumn + Mathf.Clamp(owner.frame, 0, 3)) * hero.SpritePixelWidth),
                (float)(hero.SpritePixelHeight * animationRow));
            if (owner.frame >= stampAnimationEndFrame)
            {
                stampDelay = 0f;
            }
            return false;
        }

        public override void Update()
        {
            if (stampDelay > 0f) stampDelay -= hero.DeltaTime;
        }

        private void TickDashDamage(int damage, float yOffset, float xImpulse, float yImpulse)
        {
            specialAttackDashCounter += hero.DeltaTime;
            if (specialAttackDashCounter > 0f)
            {
                specialAttackDashCounter -= 0.0333f;
                Map.HitUnits(owner, owner, PlayerNum, damage, DamageType.Crush, dashDamageRange,
                    X, Y + yOffset, xImpulse, yImpulse, true, true);
            }
        }

        private void TickFaderTrail()
        {
            if (hero.AirDashDelay > 0f) return;
            airdashFadeCounter += Time.deltaTime;
            if (airdashFadeCounter > airdashFadeRate)
            {
                airdashFadeCounter -= airdashFadeRate;
                hero.CreateFaderTrailInstance();
            }
        }

        private void MakeDashBlast(float xPoint, float yPoint, bool groundWave)
        {
            Map.ExplodeUnits(owner, blastDamage, DamageType.Crush, blastXRange, blastYRange,
                xPoint, yPoint, blastXI, blastYI, PlayerNum, false, false, true);
            MapController.DamageGround(owner, blastGroundDamage, DamageType.Explosion, blastGroundRange, xPoint, yPoint, null, false);
            EffectsController.CreateWhiteFlashPop(xPoint, yPoint);
            if (groundWave)
            {
                EffectsController.CreateGroundWave(xPoint, yPoint + 1f, 80f);
                Map.ShakeTrees(X, Y, 64f, 32f, 64f);
            }
            Map.DisturbWildLife(X, Y, 48f, PlayerNum);
        }
    }
}

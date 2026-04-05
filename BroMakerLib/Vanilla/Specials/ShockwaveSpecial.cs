using BroMakerLib.Abilities;
using Newtonsoft.Json;
using UnityEngine;
using RocketLib.Extensions;

namespace BroMakerLib.Vanilla.Specials
{
    public class ShockwaveSpecial : SpecialAbility
    {
        public bool lethal = true;
        public AudioClip[] special3Sounds;
        public float shockwaveRange = 144f;
        public float groundDamageRange = 16f;
        public int groundDamage = 20;
        public float jumpForceNormal = 210f;
        public float jumpForceDashing = 280f;
        public float blastForceNormal = 90f;
        public float blastForceDashing = 200f;
        public float pressSpecialSoundVolume = 0.5f;
        public float useSpecialSoundVolume = 0.4f;
        public int useSpecialAttackFrame = 2;
        public float stampDelay = 0f;
        public float slamGravityMultiplier = 0.5f;

        [JsonIgnore]
        protected bool readyForBlast;
        [JsonIgnore]
        protected bool setupBlastReadiness;
        [JsonIgnore]
        protected float specialAttackDirection;
        [JsonIgnore]
        protected float currentStampDelay;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            var prefab = HeroController.GetHeroPrefab(HeroType.BronanTheBrobarian);
            var sourceBro = prefab.GetComponent<TestVanDammeAnim>();
            if (special3Sounds == null)
            {
                special3Sounds = sourceBro.soundHolder.special3Sounds;
            }
            if (specialAttackSounds == null)
            {
                specialAttackSounds = sourceBro.soundHolder.specialAttackSounds;
            }
            if (owner.faderSpritePrefab == null)
            {
                owner.faderSpritePrefab = sourceBro.faderSpritePrefab;
            }
        }

        public override void PressSpecial()
        {
            if (owner.hasBeenCoverInAcid || hero.UsingSpecial || owner.health <= 0 || hero.DoingMelee)
            {
                return;
            }
            if (owner.SpecialAmmo > 0)
            {
                MapController.DamageGround(owner, groundDamage, DamageType.Normal, groundDamageRange, X, Y, null, false);
                EffectsController.CreateGroundWave(X, Y, 2f);
                if (!hero.UsingSpecial)
                {
                    specialAttackDirection = owner.transform.localScale.x;
                    Sound.GetInstance().PlaySoundEffectAt(specialAttackSounds, pressSpecialSoundVolume, owner.transform.position, 1f + owner.pitchShiftAmount);
                }
                hero.UsingSpecial = true;
                owner.frame = 0;
                hero.PressSpecialFacingDirection = (int)owner.transform.localScale.x;
                if (owner.actionState == ActionState.ClimbingLadder)
                {
                    owner.actionState = ActionState.Idle;
                }
                if (owner.dashing)
                {
                    owner.yI = jumpForceDashing;
                    owner.xIBlast += owner.transform.localScale.x * blastForceDashing;
                }
                else
                {
                    owner.yI = jumpForceNormal;
                    owner.xIBlast += owner.transform.localScale.x * blastForceNormal;
                }
                readyForBlast = false;
                setupBlastReadiness = false;
                hero.ChangeFrame();
            }
            else
            {
                HeroController.FlashSpecialAmmo(PlayerNum);
                hero.ActivateGun();
            }
        }

        public override void AnimateSpecial()
        {
            hero.SetSpriteOffset(0f, 0f);
            hero.DeactivateGun();
            hero.FrameRate = 0.0334f;
            int column = Mathf.Clamp(owner.frame, 0, 8);
            hero.Sprite.SetLowerLeftPixel(column * hero.SpritePixelWidth, hero.SpritePixelHeight * 8);
            if (owner.frame == 0)
            {
                hero.FrameRate = 0.18f;
            }
            else
            {
                hero.FrameRate = 0.034f;
                hero.CreateFaderTrailInstance();
            }
            if (owner.frame == useSpecialAttackFrame)
            {
                if (!setupBlastReadiness)
                {
                    readyForBlast = true;
                    setupBlastReadiness = true;
                }
                if (readyForBlast)
                {
                    owner.frame -= 2;
                }
                else
                {
                    owner.counter = -0.06f;
                }
            }
            if (owner.frame == 8)
            {
                owner.counter -= 0.15f;
            }
            if (owner.frame >= 10)
            {
                hero.GunFrame = 0;
                owner.frame = 0;
                hero.ActivateGun();
                hero.UsingSpecial = false;
                hero.ChangeFrame();
                currentStampDelay = 0f;
            }
        }

        public override void HandleAfterLand()
        {
            if (readyForBlast)
            {
                readyForBlast = false;
                owner.frame = useSpecialAttackFrame + 1;
                hero.ChangeFrame();
                UseSpecial();
                currentStampDelay = stampDelay;
            }
        }

        public override void UseSpecial()
        {
            ExplosionGroundWave wave = EffectsController.CreateHugeShockWave(
                X + owner.transform.localScale.x * -12f, Y + owner.headHeight, shockwaveRange, lethal);
            FullScreenFlashEffect.FlashHot(1f, owner.transform.position);
            wave.playerNum = PlayerNum;
            wave.avoidObject = owner;
            wave.origins = owner;
            owner.SpecialAmmo--;
            HeroController.SetSpecialAmmo(PlayerNum, owner.SpecialAmmo);
            Sound.GetInstance().PlaySoundEffectAt(special3Sounds, useSpecialSoundVolume, owner.transform.position, 1f + owner.pitchShiftAmount);
            if (owner.transform.localScale.x > 0f)
            {
                wave.leftWave = false;
            }
            else
            {
                wave.rightWave = false;
            }
            owner.xI = 0f;
            owner.xIBlast = 0f;
            owner.yI = 50f;
        }

        public override bool HandleApplyFallingGravity()
        {
            if (owner.GetFieldValue<bool>("isInQuicksand"))
            {
                return true;
            }
            if (hero.UsingSpecial && !readyForBlast)
            {
                owner.yI -= 1100f * hero.DeltaTime * slamGravityMultiplier;
                return false;
            }
            return true;
        }

        public override void HandleAfterCheckInput()
        {
            if (currentStampDelay > 0f)
            {
                owner.fire = false;
                owner.special = false;
                owner.down = false;
                owner.left = false;
                owner.right = false;
                owner.SetFieldValue("highFive", false);
            }
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            return !hero.UsingSpecial;
        }

        public override bool HandleIsOverLadder()
        {
            return !hero.UsingSpecial;
        }

        public override bool HandleDeath()
        {
            readyForBlast = false;
            hero.UsingSpecial = false;
            return true;
        }

        public override void HandleAfterCalculateMovement()
        {
            if (hero.UsingSpecial)
            {
                owner.canWallClimb = false;
                owner.xI *= 1f - hero.DeltaTime * 12f;
            }
            else
            {
                owner.canWallClimb = true;
            }
        }

        public override void Update()
        {
            if (currentStampDelay > 0f)
            {
                currentStampDelay -= hero.DeltaTime;
            }
        }
    }
}

using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("TheBrofessional")]
    public class TheBrofessionalMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.TheBrofessional;
        [JsonIgnore] private bool teleporting;
        [JsonIgnore] private bool teleported;
        [JsonIgnore] private float _lastTeleportTime;
        [JsonIgnore] private int teleportFrame;
        [JsonIgnore] private int teleportDirection;
        [JsonIgnore] private int teleportHoldFrames;
        [JsonIgnore] private bool pressedDownAtStartOfTeleport;
        [JsonIgnore] private bool pressedUpAtStartOfTeleport;
        [JsonIgnore] private Unit teleportTargetEnemy;
        [JsonIgnore] private float meleeDelay;

        public int maxTeleportHoldFrames = 30;

        public AudioClip[] attack2Sounds;
        public AudioClip[] attack3Sounds;

        public TheBrofessionalMelee()
        {
            meleeType = BroBase.MeleeType.TeleportStab;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var sourceBro = HeroController.GetHeroPrefab(SourceBroType) as TheBrofessional;
            if (sourceBro != null)
            {
                maxTeleportHoldFrames = sourceBro.maxTeleportHoldFrames;
            }
        }

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro != null)
            {
                if (attack2Sounds == null) attack2Sounds = sourceBro.soundHolder.attack2Sounds.CloneArray();
                if (attack3Sounds == null) attack3Sounds = sourceBro.soundHolder.attack3Sounds.CloneArray();
            }
        }

        public override void Update()
        {
            meleeDelay -= hero.DeltaTime;
        }

        public override void StartMelee()
        {
            if (owner.skinnedMookOnMyBack != null)
            {
                hero.CancelMelee();
                hero.ThrowBackMook(owner.skinnedMookOnMyBack);
                hero.NearbyMook = null;
                return;
            }
            if (meleeDelay > 0f && !owner.IsOnGround())
            {
                return;
            }
            if (teleporting || (hero.DoingMelee && !hero.MeleeHasHit))
            {
                return;
            }
            if (!hero.DoingMelee)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
                owner.xI = 0f;
                owner.yI = 0f;
            }
            owner.dashing = false;
            hero.StartMeleeCommon();
            if (meleeDelay <= 0f || owner.IsOnGround())
            {
                teleportTargetEnemy = null;
                teleportHoldFrames = 0;
                teleportFrame = 0;
                pressedDownAtStartOfTeleport = owner.down;
                pressedUpAtStartOfTeleport = owner.up;
                CalculateTeleportTarget();
                if (teleportTargetEnemy == null || Mathf.Abs(teleportTargetEnemy.X - owner.X) > 12f || Mathf.Abs(teleportTargetEnemy.Y - owner.Y) > 12f)
                {
                    sound.PlaySoundEffectAt(attack2Sounds, 0.14f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
                    teleporting = true;
                    teleported = false;
                    teleportDirection = (int)owner.transform.localScale.x;
                    owner.invulnerable = true;
                    owner.xI = 0f;
                    EffectsController.CreateNinjaSmokeEffect(owner.X, owner.Y, 0f, 0f);
                    hero.ChangeFrame();
                    owner.counter = 0f;
                }
            }
        }

        public override void AnimateMelee()
        {
            if (teleporting)
            {
                teleportFrame++;
            }

            if (teleporting && !teleported)
            {
                hero.DeactivateGun();
                hero.SetSpriteOffset(0f, 0f);
                hero.RollingFrames = 0;
                hero.FrameRate = 0.025f;
                if (teleportFrame == 2)
                {
                    owner.invulnerable = true;
                }
                if (hero.HighFive && teleportFrame == 9)
                {
                    teleportHoldFrames++;
                    if (teleportHoldFrames < maxTeleportHoldFrames)
                    {
                        teleportFrame--;
                        owner.xI = 0f;
                        owner.yI = 0f;
                        if (teleportHoldFrames < 15)
                        {
                            pressedUpAtStartOfTeleport = owner.up;
                            pressedDownAtStartOfTeleport = owner.down;
                        }
                    }
                }
                if (teleportFrame == 9)
                {
                    owner.xI = 0f;
                    owner.xIBlast = 0f;
                    owner.yI = 0f;
                    hero.JumpTime = 0f;
                    owner.xIBlast = 0f;
                    if (teleportTargetEnemy != null && Mathf.Abs(teleportTargetEnemy.Y - owner.Y) < 20f && !CanTeleportTo(teleportTargetEnemy.X, 4))
                    {
                        teleportTargetEnemy = null;
                    }
                    if (teleportTargetEnemy == null)
                    {
                        meleeDelay = 0.5f;
                    }
                    Teleport();
                    if (teleportTargetEnemy != null)
                    {
                        owner.SetFieldValue("holdStillTime", 0.2f);
                    }
                    teleportFrame = 1;
                    AnimateMelee();
                }
                else
                {
                    int num = 23 + Mathf.Clamp(teleportFrame, 0, 8);
                    hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(7 * hero.SpritePixelHeight));
                }
            }
            else if (teleporting && teleported)
            {
                hero.DeactivateGun();
                hero.SetSpriteOffset(0f, 0f);
                hero.RollingFrames = 0;
                hero.FrameRate = 0.033f;
                if (teleportFrame == 5)
                {
                    if (teleportTargetEnemy != null)
                    {
                        owner.ForceFaceDirection(-teleportDirection);
                        owner.right = false;
                        owner.left = false;
                        hero.MeleeChosenUnit = null;
                        hero.StartMeleeCommon();
                        hero.MeleeFollowUp = false;
                        hero.DashingMelee = false;
                        owner.SetFieldValue("standingMelee", true);
                        owner.SetFieldValue("holdStillTime", 0.24f);
                        owner.frame = 2;
                        owner.xI = (float)(-(float)teleportDirection * 50);
                        AnimateKnifeMelee();
                        owner.counter = 0f;
                        hero.FrameRate = 0.033f;
                    }
                    else
                    {
                        hero.DoingMelee = false;
                        owner.SetFieldValue("wasHighFive", true);
                    }
                    teleporting = false;
                    owner.invulnerable = false;
                    teleportTargetEnemy = null;
                }
                else
                {
                    int num2 = 28 - Mathf.Clamp(teleportFrame, 0, 6);
                    hero.Sprite.SetLowerLeftPixel((float)(num2 * hero.SpritePixelWidth), (float)(7 * hero.SpritePixelHeight));
                }
            }
            else
            {
                AnimateKnifeMelee();
            }
        }

        private void AnimateKnifeMelee()
        {
            hero.AnimateMeleeCommon();
            int num = 21 + Mathf.Clamp(owner.frame, 0, 10);
            int num2 = 1;
            if (owner.frame < 4)
            {
                hero.FrameRate = 0.025f;
            }
            else
            {
                hero.FrameRate = 0.033f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3)
            {
                owner.counter -= 0.045f;
            }
            int num3 = 4;
            if (owner.actionState == ActionState.Jumping)
            {
                num3 = 1;
            }
            if (owner.frame == num3)
            {
                PerformKnifeMeleeAttack(true, true);
            }
            else if (owner.frame > num3 && !hero.MeleeHasHit)
            {
                PerformKnifeMeleeAttack(false, false);
            }
            if (owner.frame == 8)
            {
                owner.counter -= 0.033f;
            }
            if (owner.frame >= 10)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformKnifeMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, owner.X + (float)(owner.Direction * 4), owner.Y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(24f);
            if (Map.HitClosestUnit(owner, owner.playerNum, 8, DamageType.SilencedBullet, 24f, 24f, owner.X + owner.transform.localScale.x * 8f, owner.Y + 8f, owner.transform.localScale.x * 200f, 0f, false, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(meleeHitSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && hero.TryMeleeTerrain(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }

        public override void RunMeleeMovement()
        {
            if (!teleporting || teleported)
            {
                hero.ApplyFallingGravity();
            }
            if (owner.yI < owner.maxFallSpeed)
            {
                owner.yI = owner.maxFallSpeed;
            }
            owner.xI *= 1f - hero.DeltaTime * 25f;
        }

        public override void CancelMelee()
        {
            teleporting = false;
            owner.invulnerable = false;
        }

        public override void HandleAfterDeath()
        {
            teleporting = false;
        }

        public override bool HandleDamage(int damage, DamageType damageType, float xI, float yI, int direction, MonoBehaviour damageSender, float hitX, float hitY)
        {
            if ((damageType == DamageType.Melee || damageType == DamageType.Knifed) && IsAttacking())
            {
                return false;
            }
            if (Time.time - _lastTeleportTime < 0.25f)
            {
                return false;
            }
            return true;
        }

        public override bool HandleIsInStealthMode()
        {
            if (teleporting)
            {
                return false;
            }
            return true;
        }

        public override void HandleAfterLand()
        {
            if (meleeDelay > 0f)
            {
                meleeDelay = 0f;
            }
        }

        public override bool HandleCanInseminate(ref bool result)
        {
            if (IsAttacking())
            {
                result = false;
                return false;
            }
            return true;
        }

        public override bool HandleApplyFallingGravity()
        {
            if (teleporting && !teleported)
            {
                return false;
            }
            return true;
        }

        private bool IsAttacking()
        {
            return teleporting || (hero.DoingMelee && owner.frame < 6);
        }

        private void CalculateTeleportTarget()
        {
            if (pressedUpAtStartOfTeleport)
            {
                teleportTargetEnemy = Map.GetNearbyMookVertical(40f, 96f, owner.X, owner.Y, 1, false);
            }
            else if (pressedDownAtStartOfTeleport)
            {
                teleportTargetEnemy = Map.GetNearbyMookVertical(40f, 96f, owner.X, owner.Y, -1, false);
            }
            int num;
            if (owner.left)
            {
                num = -1;
            }
            else if (owner.right)
            {
                num = 1;
            }
            else
            {
                num = (int)owner.transform.localScale.x;
            }
            if (teleportTargetEnemy == null && !pressedUpAtStartOfTeleport && !pressedDownAtStartOfTeleport)
            {
                teleportTargetEnemy = Map.GetNearbyMook(160f, 12f, owner.X, owner.Y, num, false);
            }
            if (teleportTargetEnemy == null && !pressedUpAtStartOfTeleport && !pressedDownAtStartOfTeleport)
            {
                teleportTargetEnemy = Map.GetNearbyMook(15f, 12f, owner.X, owner.Y, -num, false);
            }
            if (teleportTargetEnemy == null && owner.actionState == ActionState.Jumping)
            {
                if (owner.yI > 20f)
                {
                    teleportTargetEnemy = Map.GetNearbyMook(100f, 24f, owner.X, owner.Y + 12f, num, false);
                }
                else if (owner.yI < -20f)
                {
                    teleportTargetEnemy = Map.GetNearbyMook(100f, 24f, owner.X, owner.Y - 12f, num, false);
                }
                else
                {
                    teleportTargetEnemy = Map.GetNearbyMook(100f, 24f, owner.X, owner.Y, num, false);
                }
            }
        }

        private bool CanTeleportTo(float xTarget, int maxSolidBlocks)
        {
            int collumn = Map.GetCollumn(xTarget);
            int num = Map.GetCollumn(owner.X);
            int num2 = 0;
            int row = owner.row;
            if (collumn < num)
            {
                int num3 = 0;
                while (num > collumn && num3 < 40)
                {
                    num--;
                    if (Map.IsBlockSolid(num, row))
                    {
                        num2++;
                    }
                    if (num2 >= maxSolidBlocks)
                    {
                        return false;
                    }
                    num3++;
                }
            }
            else
            {
                if (collumn <= num)
                {
                    return true;
                }
                int num4 = 0;
                while (num < collumn && num4 < 40)
                {
                    num++;
                    if (Map.IsBlockSolid(num, row))
                    {
                        num2++;
                    }
                    if (num2 >= maxSolidBlocks)
                    {
                        return false;
                    }
                    num4++;
                }
            }
            return true;
        }

        private void Teleport()
        {
            float headHeight = owner.headHeight;
            float halfWidth = hero.HalfWidth;
            LayerMask groundLayer = hero.GroundLayer;

            if (teleportTargetEnemy != null)
            {
                RaycastHit raycastHit;
                if ((pressedDownAtStartOfTeleport || pressedUpAtStartOfTeleport) && Mathf.Abs(teleportTargetEnemy.Y - owner.Y) > 8f)
                {
                    teleportDirection = (int)Mathf.Sign(teleportTargetEnemy.Y - owner.Y);
                    if (Map.IsBlockSolid(teleportTargetEnemy.Y + (float)(teleportDirection * 13), teleportTargetEnemy.Y))
                    {
                        teleportDirection *= -1;
                    }
                    if (Map.IsBlockSolid(teleportTargetEnemy.Y + (float)(teleportDirection * 13), teleportTargetEnemy.Y))
                    {
                        teleportDirection = 0;
                    }
                }
                else if (Physics.Raycast(new Vector3(owner.X, owner.Y + headHeight - 2f, 0f), new Vector3((float)teleportDirection, 0f, 0f), out raycastHit, Mathf.Abs(owner.X - teleportTargetEnemy.X) + halfWidth + 15f, groundLayer))
                {
                    if (!Map.IsBlockSolid(teleportTargetEnemy.X - (float)(teleportDirection * 15), owner.Y + 2f))
                    {
                        teleportDirection *= -1;
                    }
                    else if (!Map.IsBlockSolid(teleportTargetEnemy.X + (float)(teleportDirection * 15), owner.Y + 2f))
                    {
                        teleportDirection = 0;
                    }
                }
                else if (!Physics.Raycast(new Vector3(teleportTargetEnemy.X + (float)(teleportDirection * 15), teleportTargetEnemy.Y + 2f, 0f), new Vector3(0f, -1f, 0f), out raycastHit, 8f, groundLayer))
                {
                    if (Physics.Raycast(new Vector3(teleportTargetEnemy.X - (float)(teleportDirection * 15), teleportTargetEnemy.Y + 2f, 0f), new Vector3(0f, -1f, 0f), out raycastHit, 8f, groundLayer))
                    {
                        teleportDirection *= -1;
                    }
                    else
                    {
                        teleportDirection = 0;
                    }
                }
                owner.SetXY(teleportTargetEnemy.X + (float)(teleportDirection * 15), teleportTargetEnemy.Y);
                owner.xI = (float)(-(float)teleportDirection * 50);
                if (teleportDirection != 0)
                {
                    owner.ForceFaceDirection(-teleportDirection);
                }
            }
            else
            {
                RaycastHit raycastHit2;
                if (pressedUpAtStartOfTeleport && !Map.IsBlockSolid(owner.X, owner.Y + 36f))
                {
                    owner.Y += 36f;
                    owner.yI = 125f;
                }
                else if (pressedUpAtStartOfTeleport && !Map.IsBlockSolid(owner.X, owner.Y + 48f))
                {
                    owner.Y += 48f;
                    owner.yI = 125f;
                }
                else if (pressedUpAtStartOfTeleport && !Map.IsBlockSolid(owner.X, owner.Y + 64f))
                {
                    owner.Y += 64f;
                    owner.yI = 160f;
                }
                else if (pressedUpAtStartOfTeleport && !Map.IsBlockSolid(owner.X, owner.Y + 72f))
                {
                    owner.Y += 72f;
                    owner.yI = 160f;
                }
                else if (pressedDownAtStartOfTeleport && !Map.IsBlockSolid(owner.X, owner.Y - 32f))
                {
                    owner.Y -= 32f;
                }
                else if (pressedDownAtStartOfTeleport && !Map.IsBlockSolid(owner.X, owner.Y - 48f))
                {
                    owner.Y -= 48f;
                }
                else if (pressedDownAtStartOfTeleport && !Map.IsBlockSolid(owner.X, owner.Y - 63f))
                {
                    owner.Y -= 63f;
                }
                else if (Physics.Raycast(new Vector3(owner.X, owner.Y + headHeight - 2f, 0f), new Vector3((float)teleportDirection, 0f, 0f), out raycastHit2, 32f + halfWidth, groundLayer) || Physics.Raycast(new Vector3(owner.X, owner.Y + 2f, 0f), new Vector3((float)teleportDirection, 0f, 0f), out raycastHit2, 32f + halfWidth, groundLayer))
                {
                    if (!Map.IsBlockSolid(owner.X + (float)(teleportDirection * 48), owner.Y + 2f))
                    {
                        owner.X += (float)(teleportDirection * 48);
                    }
                    else if (!Map.IsBlockSolid(owner.X + (float)(teleportDirection * 32), owner.Y + 2f))
                    {
                        owner.X += (float)(teleportDirection * 32);
                    }
                    else
                    {
                        owner.X = raycastHit2.point.x - (float)teleportDirection * halfWidth;
                    }
                }
                else
                {
                    owner.X += (float)(teleportDirection * 32);
                }
                owner.xI = 0f;
            }
            teleported = true;
            _lastTeleportTime = Time.time;
            owner.dashing = false;
            sound.PlaySoundEffectAt(attack3Sounds, 0.14f, owner.transform.position, 0.9f + owner.pitchShiftAmount, true, false, false, 0f);
        }
    }
}

using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("IndianaBrones")]
    public class IndianaBronesMelee : MeleeAbility
    {
        public float meleeFrameRate = 0.03334f;
        public float wallHitVolume = 0.2f;

        public AudioClip[] wallHitSounds;

        [JsonIgnore] private int meleeFrame;
        [JsonIgnore] private float meleeCounter;
        [JsonIgnore] private bool wallHasHit;
        [JsonIgnore] private bool wallHasHitForward;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var indiana = owner as IndianaBrones;
            if (indiana == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.IndianaBrones);
                indiana = prefab as IndianaBrones;
            }
            if (indiana != null)
            {
                meleeHitSounds = indiana.soundHolder.meleeHitSound;
                missSounds = indiana.soundHolder.missSounds;
                meleeHitTerrainSounds = indiana.soundHolder.meleeHitTerrainSound;
                wallHitSounds = indiana.soundHolder.special3Sounds;
            }
        }

        public override void StartMelee()
        {
            owner.SetFieldValue("cancelMeleeOnChangeDirection", true);
            var indianaCheck = owner as IndianaBrones;
            bool whippingAnimation = indianaCheck != null && indianaCheck.GetFieldValue<bool>("whippingAnimation");
            if (owner.GetFieldValue<bool>("wallClimbing") || whippingAnimation)
            {
                return;
            }
            owner.SetFieldValue("showHighFiveAfterMeleeTimer", 0f);
            hero.SetMeleeType();
            hero.MeleeHasHit = false;
            if (!hero.DoingMelee || meleeFrame > 3)
            {
                hero.DoingMelee = true;
                meleeFrame = 0;
                meleeCounter = -0.033f;
                wallHasHit = false;
                wallHasHitForward = false;
                hero.ActivateGun();
                owner.gunSprite.transform.localPosition = new Vector3(0f, 0f, -0.001f);
                owner.gunSprite.transform.localScale = new Vector3(1f, 1f, 1f);
                var indiana = owner as IndianaBrones;
                if (indiana != null)
                {
                    indiana.SetFieldValue("currentMaterial", indiana.materialArmless);
                    owner.GetComponent<Renderer>().sharedMaterial = indiana.materialArmless;
                }
                owner.canDoIndependentMeleeAnimation = true;
                AnimateMelee();
            }
            else
            {
                hero.MeleeFollowUp = true;
                hero.DoingMelee = true;
            }
        }

        public override void Update()
        {
            var indiana = owner as IndianaBrones;
            bool whipping = indiana != null && indiana.GetFieldValue<bool>("whippingAnimation");
            if (owner.canDoIndependentMeleeAnimation && hero.DoingMelee && !whipping)
            {
                hero.ActivateGun();
                meleeCounter += hero.DeltaTime;
                if (meleeCounter >= meleeFrameRate)
                {
                    meleeCounter -= meleeFrameRate;
                    meleeFrame++;
                    AnimateMelee();
                }
            }
        }

        public override void AnimateMelee()
        {
            hero.SetSpriteOffset(0f, 0f);
            owner.SetFieldValue("rollingFrames", 0);
            if (meleeFrame == 1)
            {
                meleeCounter -= 0.06667f;
            }
            if (meleeFrame <= 2)
            {
                wallHasHit = false;
            }
            if (meleeFrame >= 5 && hero.MeleeFollowUp)
            {
                meleeCounter -= 0.033f;
                meleeFrame = 0;
                hero.MeleeFollowUp = false;
                wallHasHit = false;
                wallHasHitForward = false;
                hero.ResetMeleeValues();
            }
            if (meleeFrame <= 2 && hero.MeleeFollowUp)
            {
                hero.MeleeFollowUp = false;
            }
            hero.FrameRate = 0.0333f;
            if (meleeFrame == 3)
            {
                if (Map.HitClosestUnit(owner, PlayerNum, 4, DamageType.Blade, 14f, 24f, X + owner.transform.localScale.x * 8f, Y + 8f, owner.transform.localScale.x * 170f, 300f, true, true, owner.IsMine, false, true))
                {
                    sound.PlaySoundEffectAt(meleeHitSounds, 0.6f, owner.transform.position, 1f, true, false, false, 0f);
                    hero.MeleeHasHit = true;
                    if (!wallHasHit && MapController.DamageGround(owner, 8, DamageType.Crush, 8f, X + owner.transform.localScale.x * 11f, Y + 11f, null, false))
                    {
                        PlayWallSound();
                        wallHasHit = true;
                        wallHasHitForward = true;
                        EffectsController.CreateSparkShower(X + owner.transform.localScale.x * 13f, Y + 7f, 8, 5f, 150f, owner.transform.localScale.x * -110f, 100f, 0.4f, 0f);
                        EffectsController.CreateProjectilePopEffect(X + owner.transform.localScale.x * 11f, Y + 7f);
                    }
                }
                else if (!wallHasHit && MapController.DamageGround(owner, 8, DamageType.Crush, 8f, X + owner.transform.localScale.x * 16f, Y + 11f, null, false))
                {
                    PlayWallSound();
                    wallHasHit = true;
                    wallHasHitForward = true;
                    EffectsController.CreateSparkShower(X + owner.transform.localScale.x * 14f, Y + 7f, 8, 5f, 150f, owner.transform.localScale.x * -110f, 100f, 0.4f, 0f);
                    EffectsController.CreateProjectilePopEffect(X + owner.transform.localScale.x * 14f, Y + 7f);
                }
                else if (!wallHasHit && MapController.DamageGround(owner, 8, DamageType.Crush, 8f, X + owner.transform.localScale.x * 15f, Y + 4f, null, false))
                {
                    PlayWallSound();
                    wallHasHit = true;
                    EffectsController.CreateSparkShower(X + owner.transform.localScale.x * 11f, Y + 2f, 8, 5f, 150f, 0f, 150f, 0.4f, 0f);
                    EffectsController.CreateProjectilePopEffect(X + owner.transform.localScale.x * 11f, Y + 2f);
                }
                else if (!wallHasHit && MapController.DamageGround(owner, 8, DamageType.Crush, 8f, X + owner.transform.localScale.x * 9f, Y + 4f, null, false))
                {
                    PlayWallSound();
                    wallHasHit = true;
                    EffectsController.CreateSparkShower(X + owner.transform.localScale.x * 11f, Y + 2f, 8, 5f, 150f, 0f, 150f, 0.4f, 0f);
                    EffectsController.CreateProjectilePopEffect(X + owner.transform.localScale.x * 11f, Y + 2f);
                }
                else
                {
                    sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.MeleeChosenUnit = null;
                if (TryMeleeTerrainInline(0, 2))
                {
                    hero.MeleeHasHit = true;
                }
            }
            int num = 17 + Mathf.Clamp(meleeFrame, 0, 6);
            hero.SetGunSprite(num, wallHasHitForward ? 1 : 0);
            if (meleeFrame == 4)
            {
                meleeCounter -= 0.0334f;
            }
            if (meleeFrame >= 6)
            {
                meleeFrame = 0;
                hero.CancelMelee();
                return;
            }
            Mook nearbyMook = owner.GetFieldValue<Mook>("nearbyMook");
            if (meleeFrame == 2 && nearbyMook != null && nearbyMook.CanBeThrown() && owner.GetFieldValue<bool>("highFive"))
            {
                hero.CancelMelee();
                owner.CallMethod("ThrowBackMook", nearbyMook);
                Transform parentedToTransform = nearbyMook.GetParentedToTransform();
                if (parentedToTransform != null && parentedToTransform.name.IndexOf("BOSS", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Utility.AchievementManager.AwardAchievement(Utility.Achievement.noticket, PlayerNum);
                }
                owner.SetFieldValue("nearbyMook", null);
            }
            else if (meleeFrame == 2)
            {
                owner.PlaySpecialAttackSound(0.2f);
            }
        }

        public override void CancelMelee()
        {
            owner.canDoIndependentMeleeAnimation = false;
        }

        public override void RunMeleeMovement()
        {
            if (!owner.useNewKnifingFrames)
            {
                if (owner.Y > owner.groundHeight + 1f)
                {
                    owner.CallMethod("ApplyFallingGravity");
                }
            }
            else if (hero.JumpingMelee)
            {
                owner.CallMethod("ApplyFallingGravity");
                if (owner.yI < owner.maxFallSpeed)
                {
                    owner.yI = owner.maxFallSpeed;
                }
            }
            else if (hero.DashingMelee)
            {
                if (owner.frame <= 1)
                {
                    owner.xI = 0f;
                    owner.yI = 0f;
                }
                else if (owner.frame <= 3)
                {
                    if (hero.MeleeChosenUnit == null)
                    {
                        if (!owner.GetFieldValue<bool>("isInQuicksand"))
                        {
                            owner.xI = owner.speed * 1f * owner.transform.localScale.x;
                        }
                        owner.yI = 0f;
                    }
                    else if (!owner.GetFieldValue<bool>("isInQuicksand"))
                    {
                        owner.xI = owner.speed * 0.5f * owner.transform.localScale.x + (hero.MeleeChosenUnit.X - owner.X) * 6f;
                    }
                }
                else if (owner.frame <= 5)
                {
                    if (!owner.GetFieldValue<bool>("isInQuicksand"))
                    {
                        owner.xI = owner.speed * 0.3f * owner.transform.localScale.x;
                    }
                    owner.CallMethod("ApplyFallingGravity");
                }
                else
                {
                    owner.CallMethod("ApplyFallingGravity");
                }
            }
            else if (owner.Y > owner.groundHeight + 1f)
            {
                hero.CancelMelee();
            }
        }

        private void PlayWallSound()
        {
            if (sound == null) return;
            sound.PlaySoundEffectAt(wallHitSounds, wallHitVolume, owner.transform.position, 1f, true, false, false, 0f);
        }

        private bool TryMeleeTerrainInline(int offset, int meleeDamage)
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(new Vector3(X - owner.transform.localScale.x * 4f, Y + 4f, 0f), new Vector3(owner.transform.localScale.x, 0f, 0f), out raycastHit, (float)(16 + offset), hero.GroundLayer))
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
            MapController.Damage_Networked(owner, raycastHit.collider.gameObject, meleeDamage, DamageType.Melee, 0f, 40f, raycastHit.point.x, raycastHit.point.y);
            sound.PlaySoundEffectAt(meleeHitTerrainSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 4f);
            return true;
        }
    }
}

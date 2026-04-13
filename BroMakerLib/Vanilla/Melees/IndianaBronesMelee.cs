using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("IndianaBrones")]
    public class IndianaBronesMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.IndianaBrones;

        public float meleeFrameRate = 0.03334f;
        public float wallHitVolume = 0.2f;

        public AudioClip[] wallHitSounds;
        public AudioClip[] specialAttackSounds;

        [JsonIgnore] private int meleeFrame;
        [JsonIgnore] private float meleeCounter;
        [JsonIgnore] private bool wallHasHit;
        [JsonIgnore] private bool wallHasHitForward;

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (meleeHitSounds == null) meleeHitSounds = sourceBro.soundHolder.meleeHitSound.CloneArray();
            if (missSounds == null) missSounds = sourceBro.soundHolder.missSounds.CloneArray();
            if (meleeHitTerrainSounds == null) meleeHitTerrainSounds = sourceBro.soundHolder.meleeHitTerrainSound.CloneArray();
            if (wallHitSounds == null) wallHitSounds = sourceBro.soundHolder.special3Sounds.CloneArray();
            if (specialAttackSounds == null) specialAttackSounds = sourceBro.soundHolder.specialAttackSounds.CloneArray();
        }

        public override void StartMelee()
        {
            hero.CancelMeleeOnChangeDirection = true;
            var indianaCheck = owner as IndianaBrones;
            bool whippingAnimation = indianaCheck != null && indianaCheck.GetFieldValue<bool>("whippingAnimation");
            if (owner.GetFieldValue<bool>("wallClimbing") || whippingAnimation)
            {
                return;
            }
            hero.ShowHighFiveAfterMeleeTimer = 0f;
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
            hero.RollingFrames = 0;
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
                if (HandleTryMeleeTerrain(0, terrainDamage))
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
            Mook nearbyMook = hero.NearbyMook;
            if (meleeFrame == 2 && nearbyMook != null && nearbyMook.CanBeThrown() && hero.HighFive)
            {
                hero.CancelMelee();
                hero.ThrowBackMook(nearbyMook);
                Transform parentedToTransform = nearbyMook.GetParentedToTransform();
                if (parentedToTransform != null && parentedToTransform.name.IndexOf("BOSS", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    Utility.AchievementManager.AwardAchievement(Utility.Achievement.noticket, PlayerNum);
                }
                hero.NearbyMook = null;
            }
            else if (meleeFrame == 2)
            {
                sound.PlaySoundEffectAt(specialAttackSounds, 0.2f, owner.transform.position, 1f + owner.pitchShiftAmount, true, false, false, 0f);
            }
        }

        public override void CancelMelee()
        {
            owner.canDoIndependentMeleeAnimation = false;
        }

        private void PlayWallSound()
        {
            if (sound == null) return;
            sound.PlaySoundEffectAt(wallHitSounds, wallHitVolume, owner.transform.position, 1f, true, false, false, 0f);
        }
    }
}

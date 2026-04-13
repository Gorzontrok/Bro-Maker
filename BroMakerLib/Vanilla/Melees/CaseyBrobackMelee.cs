using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("CaseyBroback")]
    public class CaseyBrobackMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.CaseyBroback;

        [JsonIgnore]
        private int meleeDirection = 1;

        public CaseyBrobackMelee()
        {
            meleeType = BroBase.MeleeType.Knife;
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (meleeHitSounds == null) meleeHitSounds = sourceBro.soundHolder.meleeHitSound.CloneArray();
            if (missSounds == null) missSounds = sourceBro.soundHolder.missSounds.CloneArray();
            if (meleeHitTerrainSounds == null) meleeHitTerrainSounds = sourceBro.soundHolder.meleeHitTerrainSound.CloneArray();
        }

        public override void StartMelee()
        {
            hero.ShowHighFiveAfterMeleeTimer = 0f;
            hero.JumpTime = 0f;
            hero.DeactivateGun();
            hero.SetMeleeType();
            hero.MeleeHasHit = false;
            if (!hero.DoingMelee || owner.frame > 3)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else if (hero.DoingMelee)
            {
                hero.MeleeFollowUp = true;
            }
            hero.DoingMelee = true;
            owner.SetFieldValue("frameRate", 0.025f);
            owner.frame = 1;
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = 25 + Mathf.Clamp(owner.frame, 0, 6);
            int num2 = 1;
            if (!hero.StandingMelee)
            {
                if (hero.JumpingMelee)
                {
                    num = 17 + Mathf.Clamp(owner.frame, 0, 6);
                    num2 = 6;
                }
                else if (hero.DashingMelee)
                {
                    num = 17 + Mathf.Clamp(owner.frame, 0, 6);
                    num2 = 6;
                    if (owner.frame == 4)
                    {
                        owner.counter -= 0.0334f;
                    }
                    else if (owner.frame == 5)
                    {
                        owner.counter -= 0.0334f;
                    }
                }
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3)
            {
                owner.counter -= 0.066f;
                PerformBatMeleeAttack(true, true);
            }
            else if (owner.frame > 3 && !hero.MeleeHasHit)
            {
                PerformBatMeleeAttack(false, false);
            }
            if (owner.frame >= 6)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformBatMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            Unit unit = Map.HitClosestUnit(owner, PlayerNum, 0, DamageType.Melee, 14f, 24f, X + owner.transform.localScale.x * 8f, Y + 8f, owner.transform.localScale.x * 150f * (float)meleeDirection, 750f, true, false, owner.IsMine, false, true);
            if (unit != null)
            {
                Mook mook = unit as Mook;
                if (mook != null)
                {
                    mook.PlayFallSound(0.3f);
                }
                sound.PlaySoundEffectAt(meleeHitSounds, 1f, owner.transform.position, 1.5f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
            meleeDirection *= -1;
        }
    }
}

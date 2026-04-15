using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Shared base for pistol-whip melee. Used by: BroHard, BoondockBro.</summary>
    [MeleePreset("PistolWhipMelee")]
    public class PistolWhipMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.BroHard;

        public PistolWhipMelee()
        {
            meleeType = BroBase.MeleeType.PistolWhip;
            animationColumn = 27;
            animationRow = 9;
            animationFrameCount = 7;
            endFrame = 4;
            restartFrame = 4;
        }

        protected override void CacheSoundsFromPrefab()
        {
            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (alternateMeleeHitSounds == null) alternateMeleeHitSounds = sourceBro.soundHolder.alternateMeleeHitSound.CloneArray();
            if (missSounds == null) missSounds = sourceBro.soundHolder.missSounds.CloneArray();
            if (meleeHitTerrainSounds == null) meleeHitTerrainSounds = sourceBro.soundHolder.alternateMeleeHitSound.CloneArray();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 6);
            int num2 = animationRow;
            hero.FrameRate = frameRate;
            if (owner.frame >= 2 && owner.frame <= 4)
            {
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3 && !hero.MeleeHasHit)
            {
                PerformPistolWhip(true, true);
            }
            if (owner.frame >= 4)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformPistolWhip(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 8f;
            Vector3 vector = new Vector3(X + Direction * (num + 3f), Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(26f);
            if (Map.HitClosestUnit(owner, PlayerNum, 4, DamageType.Melee, num, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 150f, 0f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.8f, owner.transform.position, Random.Range(0.9f, 1.1f), true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
            if (!hero.MeleeHasHit)
            {
                if (!hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
        }
    }
}

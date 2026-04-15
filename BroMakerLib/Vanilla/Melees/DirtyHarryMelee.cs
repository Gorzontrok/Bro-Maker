using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Dirty Harry's baseball bat melee.</summary>
    [MeleePreset("DirtyHarry")]
    public class DirtyHarryMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.DirtyHarry;

        public AudioClip[] alternateMeleeHitSound2;

        public DirtyHarryMelee()
        {
            meleeType = BroBase.MeleeType.Punch;
            startType = MeleeStartType.Custom;
            moveType = MeleeMoveType.Punch;
            restartFrame = 0;
            animationRow = 9;
        }

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();

            var sourceBro = HeroController.GetHeroPrefab(SourceBroType);
            if (sourceBro == null) return;

            if (alternateMeleeHitSound2 == null) alternateMeleeHitSound2 = sourceBro.soundHolder.alternateMeleeHitSound2.CloneArray();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = animationRow;
            if (owner.frame == 5)
            {
                owner.counter -= 0.0334f;
            }
            if (owner.frame == 2)
            {
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 2 && !hero.MeleeHasHit)
            {
                PerformBaseBallBatHit();
            }
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformBaseBallBatHit()
        {
            float num = 8f;
            Vector3 vector = new Vector3(owner.X + (float)owner.Direction * (num + 7f), owner.Y + 10f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, owner.playerNum, out flag, null);
            hero.KickDoors(25f);
            if (Map.HitClosestUnit(owner, owner.playerNum, 4, DamageType.Melee, num, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 250f, 250f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.3f, owner.transform.position, 0.6f, true, false, false, 0f);
                sound.PlaySoundEffectAt(alternateMeleeHitSound2, 0.5f, owner.transform.position, 0.6f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(owner.X + (owner.width + 10f) * owner.transform.localScale.x, owner.Y + owner.height + 4f);
            }
            else
            {
                if (!hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>B.A. Broracus's headbutt melee.</summary>
    [MeleePreset("BaBroracus")]
    public class BaBroracusMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.BaBroracus;

        public BaBroracusMelee()
        {
            meleeType = BroBase.MeleeType.Custom;
            startType = MeleeStartType.Custom;
            restartFrame = 1;
            animationColumn = 25;
            animationRow = 9;
            animationFrameCount = 9;
            hitFrame = 4;
            endFrame = 7;
            jumpingAnimationRow = 10;
            damageType = "Melee";
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 12)
            {
                owner.frame = 1;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            if (hero.DoingMelee && owner.frame < 12)
            {
                hero.MeleeFollowUp = true;
            }
            if (!hero.JumpingMelee)
            {
                hero.DashingMelee = true;
                owner.xI = (float)owner.Direction * owner.speed;
            }
            hero.StartMeleeCommon();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = animationColumn + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = animationRow;
            if (hero.JumpingMelee)
            {
                num2 = jumpingAnimationRow;
            }
            if (owner.frame == 4)
            {
                owner.counter -= 0.0334f;
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 4 && !hero.MeleeHasHit)
            {
                PerformPunchAttack(true, true);
            }
            if (owner.frame >= 7)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        public override bool TryMeleeTerrain(int offset, int meleeDamage)
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(new Vector3(X - Direction * 4f, Y + 8f, 0f), new Vector3(Direction, 0f, 0f), out raycastHit, (float)(16 + offset), hero.GroundLayer))
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
            MapController.Damage_Networked(owner, raycastHit.collider.gameObject, 5, DamageType.Melee, 0f, 40f, raycastHit.point.x, raycastHit.point.y);
            sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 4f);
            return true;
        }

        private void PerformPunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 6f;
            Vector3 vector = new Vector3(X + Direction * (num + 1f), Y + 8f + 4f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(25f);
            if (Map.HitClosestUnit(owner, PlayerNum, 6, parsedDamageType, num + 13f, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 250f, 250f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 8f);
            }
            else
            {
                if (playMissSound && !hero.HasPlayedMissSound)
                {
                    sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                hero.HasPlayedMissSound = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && TryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
        }
    }
}

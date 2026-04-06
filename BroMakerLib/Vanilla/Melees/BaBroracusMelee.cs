using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("BaBroracus")]
    public class BaBroracusMelee : MeleeAbility
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            meleeType = BroBase.MeleeType.Custom;

            var baBroracus = owner as BaBroracus;
            if (baBroracus == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.BaBroracus);
                baBroracus = prefab as BaBroracus;
            }
            if (baBroracus != null)
            {
                meleeHitSounds = baBroracus.soundHolder.meleeHitSound;
                missSounds = baBroracus.soundHolder.missSounds;
                alternateMeleeHitSounds = baBroracus.soundHolder.alternateMeleeHitSound;
                meleeHitTerrainSounds = baBroracus.soundHolder.meleeHitTerrainSound;
            }
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
                owner.SetFieldValue("dashingMelee", true);
                owner.xI = (float)owner.Direction * owner.speed;
            }
            hero.StartMeleeCommon();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = 25 + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = 9;
            if (hero.JumpingMelee)
            {
                num2 = 10;
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

        private void PerformPunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 6f;
            Vector3 vector = new Vector3(X + Direction * (num + 1f), Y + 8f + 4f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(25f);
            if (Map.HitClosestUnit(owner, PlayerNum, 6, DamageType.Melee, num + 13f, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 250f, 250f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 8f);
            }
            else
            {
                if (playMissSound && !owner.GetFieldValue<bool>("hasPlayedMissSound"))
                {
                    sound.PlaySoundEffectAt(missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && TryMeleeTerrainCustom(0, 2))
            {
                hero.MeleeHasHit = true;
            }
        }

        private bool TryMeleeTerrainCustom(int offset, int meleeDamage)
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(new Vector3(X - owner.transform.localScale.x * 4f, Y + 8f, 0f), new Vector3(owner.transform.localScale.x, 0f, 0f), out raycastHit, (float)(16 + offset), hero.GroundLayer))
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
            if (owner.GetFieldValue<BroBase.MeleeType>("currentMeleeType") == BroBase.MeleeType.Knife)
            {
                sound.PlaySoundEffectAt(meleeHitTerrainSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            else
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 8f);
            return true;
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
                else if (owner.frame <= 4)
                {
                    if (hero.MeleeChosenUnit == null)
                    {
                        owner.xI = owner.speed * 1f * owner.transform.localScale.x;
                        owner.yI = 0f;
                    }
                    else
                    {
                        owner.xI = owner.speed * 0.5f * owner.transform.localScale.x + (hero.MeleeChosenUnit.X - owner.X) * 6f;
                    }
                }
                else if (owner.frame <= 5)
                {
                    owner.xI = owner.speed * 0.3f * owner.transform.localScale.x;
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
    }
}

using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("PistolWhipMelee")]
    public class PistolWhipMelee : MeleeAbility
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);
            meleeHitTerrainSounds = owner.soundHolder.alternateMeleeHitSound;
        }

        public override void StartMelee()
        {
            if (!hero.DoingMelee || owner.frame > 4)
            {
                owner.frame = 0;
                owner.counter = -0.05f;
                AnimateMelee();
            }
            else
            {
                hero.MeleeFollowUp = true;
            }
            hero.StartMeleeCommon();
        }

        public override void AnimateMelee()
        {
            hero.AnimateMeleeCommon();
            int num = 27 + Mathf.Clamp(owner.frame, 0, 6);
            int num2 = 9;
            hero.FrameRate = 0.025f;
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

        private void PerformPistolWhip(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 8f;
            Vector3 vector = new Vector3(X + Direction * (num + 3f), Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(26f);
            if (Map.HitClosestUnit(owner, PlayerNum, 4, DamageType.Melee, num, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 150f, 0f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(owner.soundHolder.alternateMeleeHitSound, 0.8f, owner.transform.position, Random.Range(0.9f, 1.1f), true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            hero.MeleeChosenUnit = null;
            if (!hero.MeleeHasHit && shouldTryHitTerrain && TryPistolWhipHitTerrain())
            {
                hero.MeleeHasHit = true;
            }
            if (!hero.MeleeHasHit)
            {
                if (!owner.GetFieldValue<bool>("hasPlayedMissSound"))
                {
                    sound.PlaySoundEffectAt(owner.soundHolder.missSounds, 0.15f, owner.transform.position, 1f, true, false, false, 0f);
                }
                owner.SetFieldValue("hasPlayedMissSound", true);
            }
        }

        private bool TryPistolWhipHitTerrain()
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(new Vector3(X - owner.transform.localScale.x * 4f, Y + 4f, 0f), new Vector3(owner.transform.localScale.x, 0f, 0f), out raycastHit, 16f, hero.GroundLayer))
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
            MapController.Damage_Networked(owner, raycastHit.collider.gameObject, terrainDamage, DamageType.Melee, 0f, 40f, raycastHit.point.x, raycastHit.point.y);
            sound.PlaySoundEffectAt(meleeHitTerrainSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            EffectsController.CreateProjectilePopWhiteEffect(X + owner.width * owner.transform.localScale.x, Y + owner.height + 4f);
            return true;
        }
    }
}

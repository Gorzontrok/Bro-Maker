using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("Broffy")]
    public class BroffyMelee : MeleeAbility
    {
        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            Broffy broffy = owner as Broffy;
            if (broffy == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.Broffy);
                broffy = prefab as Broffy;
            }
            if (broffy != null)
            {
                meleeHitSounds = broffy.soundHolder.meleeHitSound;
                missSounds = broffy.soundHolder.missSounds;
                meleeHitTerrainSounds = broffy.soundHolder.meleeHitTerrainSound;
            }
        }

        public override void StartMelee()
        {
            owner.SetFieldValue("showHighFiveAfterMeleeTimer", 0f);
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
                    num = 17 + Mathf.Clamp(owner.frame, 0, 9);
                    num2 = 6;
                }
                else if (hero.DashingMelee)
                {
                    num = 17 + Mathf.Clamp(owner.frame, 0, 9);
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
            int hitFrame = (!hero.DashingMelee) ? 3 : 5;
            if (owner.frame == hitFrame)
            {
                owner.counter -= 0.066f;
                PerformKnifeMeleeAttack(true, true);
            }
            else if (owner.frame > hitFrame && !hero.MeleeHasHit)
            {
                PerformKnifeMeleeAttack(false, false);
            }
            int frameCount = (!hero.DashingMelee) ? 6 : 9;
            if (owner.frame >= frameCount)
            {
                owner.frame = 0;
                hero.CancelMelee();
            }
        }

        private void PerformKnifeMeleeAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            DamageType damageType = (!hero.DashingMelee) ? DamageType.SilencedBullet : DamageType.Melee;
            bool flag;
            Map.DamageDoodads(3, DamageType.Knifed, X + (float)(Direction * 4), Y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(24f);
            bool applyLaunch = false;
            float launchX = 0f;
            float launchY = 0f;
            if (hero.DashingMelee)
            {
                applyLaunch = true;
                launchX = (float)(Direction * 300);
                launchY = 240f;
            }
            if (Map.HitClosestUnit(owner, PlayerNum, 4, damageType, 14f, 24f, X + owner.transform.localScale.x * 8f, Y + 8f, launchX, launchY, applyLaunch, true, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(meleeHitSounds, 1f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
            }
            else if (playMissSound)
            {
                sound.PlaySoundEffectAt(missSounds, 0.3f, owner.transform.position, 1f, true, false, false, 0f);
            }
            hero.MeleeChosenUnit = null;
            if (shouldTryHitTerrain && TryKnifeHitTerrain())
            {
                hero.MeleeHasHit = true;
            }
        }

        private bool TryKnifeHitTerrain()
        {
            RaycastHit raycastHit;
            if (!Physics.Raycast(new Vector3(X - owner.transform.localScale.x * 4f, Y + 4f, 0f), new Vector3(owner.transform.localScale.x, 0f, 0f), out raycastHit, 16f, hero.GroundLayer))
                return false;
            Cage cage = raycastHit.collider.GetComponent<Cage>();
            if (cage == null && raycastHit.collider.transform.parent != null)
                cage = raycastHit.collider.transform.parent.GetComponent<Cage>();
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
                if (owner.frame <= 3)
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
    }
}

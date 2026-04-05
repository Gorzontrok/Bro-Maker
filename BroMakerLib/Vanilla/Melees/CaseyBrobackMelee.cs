using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using Newtonsoft.Json;
using RocketLib.Extensions;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("CaseyBroback")]
    public class CaseyBrobackMelee : MeleeAbility
    {
        [JsonIgnore]
        private int meleeDirection = 1;

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var caseyBroback = owner as CaseyBroback;
            if (caseyBroback == null)
            {
                var prefab = HeroController.GetHeroPrefab(HeroType.CaseyBroback);
                caseyBroback = prefab as CaseyBroback;
            }
            if (caseyBroback != null)
            {
                meleeHitSounds = caseyBroback.soundHolder.meleeHitSound;
                missSounds = caseyBroback.soundHolder.missSounds;
                meleeHitTerrainSounds = caseyBroback.soundHolder.meleeHitTerrainSound;
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
            if (shouldTryHitTerrain && TryKnifeHitTerrain())
            {
                hero.MeleeHasHit = true;
            }
            meleeDirection *= -1;
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
    }
}

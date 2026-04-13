using BroMakerLib.Abilities;
using BroMakerLib.Attributes;
using BroMakerLib.Extensions;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    [MeleePreset("BroGummer")]
    public class BroGummerMelee : MeleeAbility
    {
        protected override HeroType SourceBroType => HeroType.BroGummer;

        [JsonIgnore] private float sachelPackCooldown;
        [JsonIgnore] private Projectile sachelPackProjectile;
        [JsonIgnore] private RaycastHit raycastHit;

        public BroGummerMelee()
        {
            meleeType = BroBase.MeleeType.Punch;
            startType = MeleeStartType.Custom;
            moveType = MeleeMoveType.Punch;
            restartFrame = 0;
        }

        public override void Initialize(TestVanDammeAnim owner)
        {
            base.Initialize(owner);

            var sourceBro = HeroController.GetHeroPrefab(HeroType.BroGummer) as BroGummer;
            if (sourceBro != null)
            {
                sachelPackProjectile = sourceBro.sachelPackProjectile;
            }
        }

        public override void Update()
        {
            sachelPackCooldown -= hero.DeltaTime;
        }

        public override void AnimateMelee()
        {
            if (owner.frame == 2)
            {
                Mook nearbyMook = hero.NearbyMook;
                if (nearbyMook != null && nearbyMook.CanBeThrown() && hero.HighFive)
                {
                    hero.CancelMelee();
                    if (!(owner is BroGummer) && sachelPackProjectile != null && owner.IsMine)
                    {
                        Projectile projectile = ProjectileController.SpawnProjectileOverNetwork(sachelPackProjectile, owner,
                            nearbyMook.X, nearbyMook.Y + 10f,
                            owner.transform.localScale.x * 100f + owner.xI * 0.7f, owner.yI,
                            false, PlayerNum, false, false, 0f);
                        SachelPack sachelPack = projectile as SachelPack;
                        if (sachelPack != null)
                        {
                            sachelPack.TryStickToUnit(nearbyMook, true);
                        }
                    }
                    hero.ThrowBackMook(nearbyMook);
                    hero.NearbyMook = null;
                    return;
                }
            }
            hero.AnimateMeleeCommon();
            int num = 25 + Mathf.Clamp(owner.frame, 0, 8);
            int num2 = 9;
            if (owner.frame == 5)
            {
                owner.counter -= 0.0334f;
                owner.counter -= 0.0334f;
                owner.counter -= 0.0334f;
            }
            if (owner.frame == 3)
            {
                owner.counter -= 0.0334f;
            }
            hero.Sprite.SetLowerLeftPixel((float)(num * hero.SpritePixelWidth), (float)(num2 * hero.SpritePixelHeight));
            if (owner.frame == 3 && !hero.MeleeHasHit)
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
            if (sachelPackCooldown > 0f)
            {
                PerformBasePunchAttack(shouldTryHitTerrain, playMissSound);
                return;
            }
            Unit unit = Map.GeLivingtUnit(owner.playerNum, 8f, 8f, owner.X + (float)(owner.Direction * 6), owner.Y + 6f);
            if (unit != null)
            {
                ProjectileController.SpawnProjectileOverNetwork(sachelPackProjectile, owner, unit.X, unit.Y + 6f, 0f, 0f, false, owner.playerNum, false, false, 0f);
                sachelPackCooldown = 0.5f;
            }
            else if (owner.Direction < 0 && Physics.Raycast(new Vector3(owner.X + 6f, owner.Y + 10f, 0f), Vector3.left, out raycastHit, 16f, hero.GroundLayer | hero.FragileLayer))
            {
                ProjectileController.SpawnProjectileOverNetwork(sachelPackProjectile, owner, owner.X - 6f, owner.Y + 10f, -10f, 10f, false, owner.playerNum, false, false, 0f);
                sachelPackCooldown = 0.5f;
            }
            else if (owner.Direction > 0 && Physics.Raycast(new Vector3(owner.X - 6f, owner.Y + 10f, 0f), Vector3.right, out raycastHit, 12f, hero.GroundLayer | hero.FragileLayer))
            {
                ProjectileController.SpawnProjectileOverNetwork(sachelPackProjectile, owner, owner.X + 6f, owner.Y + 10f, 10f, 10f, false, owner.playerNum, false, false, 0f);
                sachelPackCooldown = 0.5f;
            }
            else
            {
                PerformBasePunchAttack(shouldTryHitTerrain, playMissSound);
            }
        }

        private void PerformBasePunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            float num = 6f;
            Vector3 vector = new Vector3(X + (float)owner.Direction * (num + 7f), Y + 8f, 0f);
            bool flag;
            Map.DamageDoodads(3, DamageType.Melee, vector.x, vector.y, 0f, 0f, 6f, PlayerNum, out flag, null);
            hero.KickDoors(25);
            if (Map.HitClosestUnit(owner, PlayerNum, 4, DamageType.Melee, num, num * 2f, vector.x, vector.y, owner.transform.localScale.x * 250f, 250f, true, false, owner.IsMine, false, true))
            {
                sound.PlaySoundEffectAt(alternateMeleeHitSounds, 0.5f, owner.transform.position, 1f, true, false, false, 0f);
                hero.MeleeHasHit = true;
                EffectsController.CreateProjectilePopWhiteEffect(X + (owner.width + 4f) * owner.transform.localScale.x, Y + owner.height + 4f);
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
            if (!hero.MeleeHasHit && shouldTryHitTerrain && HandleTryMeleeTerrain(0, terrainDamage))
            {
                hero.MeleeHasHit = true;
            }
            hero.TriggerBroMeleeEvent();
        }
    }
}

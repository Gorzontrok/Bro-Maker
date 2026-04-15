using BroMakerLib.Attributes;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Melees
{
    /// <summary>Burt Brommer's satchel-charge punch melee.</summary>
    [MeleePreset("BurtBrommer")]
    public class BroGummerMelee : PunchMelee
    {
        protected override HeroType SourceBroType => HeroType.BroGummer;

        [JsonIgnore] private float sachelPackCooldown;
        [JsonIgnore] private Projectile sachelPackProjectile;
        [JsonIgnore] private RaycastHit raycastHit;

        protected override void CacheSoundsFromPrefab()
        {
            base.CacheSoundsFromPrefab();

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

        protected override void PerformPunchAttack(bool shouldTryHitTerrain, bool playMissSound)
        {
            if (sachelPackCooldown > 0f)
            {
                base.PerformPunchAttack(shouldTryHitTerrain, playMissSound);
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
                base.PerformPunchAttack(shouldTryHitTerrain, playMissSound);
            }
        }

        public override bool HandleThrowBackMook(Mook mook)
        {
            if (owner.IsMine)
            {
                SachelPack sachelPack = ProjectileController.SpawnProjectileOverNetwork(
                    sachelPackProjectile, owner, mook.X, mook.Y + 10f,
                    owner.transform.localScale.x * 100f + owner.xI * 0.7f, owner.yI,
                    false, PlayerNum, false, false, 0f) as SachelPack;
                if (sachelPack != null)
                    sachelPack.TryStickToUnit(mook, true);
            }
            return true;
        }
    }
}

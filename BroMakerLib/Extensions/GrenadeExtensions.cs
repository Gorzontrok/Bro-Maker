using BroMakerLib.Loaders;
using BroMakerLib.Stats;
using BroMakerLib.Stats.Grenades;
using System;
using System.Collections.Generic;
using System.Linq;
using RocketLib;

namespace BroMakerLib
{
    public static class GrenadeExtensions
    {
        public static void LoadStats(this Grenade grenade, GrenadeStats stats)
        {
            // Normal Grenade
            grenade.damage = stats.damage;
            grenade.range = stats.range;
            grenade.blastForce = stats.blastForce;
            grenade.damageType = stats.damageType;
            grenade.trailType = stats.trailType;
            grenade.useAngularFriction = stats.useAngularFriction;
            grenade.angularFrictionM = stats.angularFrictionM;
            grenade.minVelocityBounceSound = stats.minVelocityBounceSound;
            grenade.maxVelocityBounceVolume = stats.maxVelocityBounceVolume;
            grenade.largeWarning = stats.largeWarning;
            grenade.weight = stats.weight;
            grenade.bounceOffEnemies = stats.bounceOffEnemies;
            grenade.bounceOffEnemiesMultiple = stats.bounceOffEnemiesMultiple;
            grenade.SetFieldValue("bounceYOffset", 2f);
            grenade.shootable = stats.shootable;
            grenade.friendlyFire = stats.friendlyFire;
            grenade.dontMakeEffects = stats.dontMakeEffects;
            grenade.hugeExplosion = stats.hugeExplosion;
            grenade.hugeExplosionLowPassM = stats.hugeExplosionLowPassM;

            if ((grenade is GrenadeAirstrike) && (stats is AirstrikeStats))
                (grenade as GrenadeAirstrike).LoadStats(stats as AirstrikeStats);

            else if ((grenade is GrenadeCluster) && (stats is GrenadeClusterStats))
                (grenade as GrenadeCluster).LoadStats(stats as GrenadeClusterStats);

            else if ((grenade is GrenadeSticky) && (stats is StickyGrenadeStats))
                (grenade as GrenadeSticky).LoadStats(stats as StickyGrenadeStats);

            else if ((grenade is GrenadeTearGas) && (stats is TeargasStats))
                (grenade as GrenadeTearGas).LoadStats(stats as TeargasStats);

            else if ((grenade is GrenaderAlienPheromones) && (stats is AlienPheromoneGrenadeStats))
                (grenade as GrenaderAlienPheromones).LoadStats(stats as AlienPheromoneGrenadeStats);
        }

        public static void LoadStats(this GrenadeAirstrike grenade, AirstrikeStats stats)
        {
            if(stats.GetProjectile() != null)
            {
                grenade.airstrikeProjectile = stats.GetProjectile();
            }
        }

        public static void LoadStats(this GrenadeCluster grenade, GrenadeClusterStats stats)
        {
            grenade.startRadianAngle = stats.startRadianAngle;
            grenade.wavesOfClusters = stats.wavesOfClusters;
            grenade.waveDelay = stats.waveDelay;
            grenade.SetFieldValue("forceM", stats.forceM);
            grenade.smallGrenadePrefab = LoadGrenade.FromName(stats.smallGrenadeName);
        }

        public static void LoadStats(this GrenadeSticky grenade, StickyGrenadeStats stats)
        {
            grenade.stickyToUnits = stats.stickyToUnits;
            grenade.stickGrenadeSwarmIndex = stats.stickGrenadeSwarmIndex;
        }

        public static void LoadStats(this GrenadeTearGas grenade, TeargasStats stats)
        {
            grenade.bounceOffWallsAndCeilings = stats.bounceOffWallsAndCeilings;
            grenade.bounceLikeRegularGrenade = stats.bounceLikeRegularGrenade;
        }

        public static void LoadStats(this GrenaderAlienPheromones grenade, AlienPheromoneGrenadeStats stats)
        {
            (grenade as GrenadeSticky).LoadStats(stats as StickyGrenadeStats);

            grenade.pheromoneRate = stats.pheromoneRate;
            grenade.pheromoneXRange = stats.pheromoneRange.x;
            grenade.pheromoneYRange = stats.pheromoneRange.y;
            grenade.pheromoneYOffset = stats.pheromoneYOffset;
            grenade.smokeRateAirborn = stats.smokeRateAirborn;
            grenade.smokeRateStuck = stats.smokeRateStuck;
            grenade.smokeZ = stats.smokeZ;
            grenade.SetFieldValue("pheromoneDelay", stats.pheromoneDelay);
        }

    }
}

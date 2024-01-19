using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Infos
{
    public class CustomProjectileInfo : CustomBroforceObjectInfo
    {
        public static CustomProjectileInfo RambroProjectile
        {
            get
            {
                return new CustomProjectileInfo("bf_Rambro", true);
            }
        }

        public readonly bool isVanillaProjectile = false;

        protected new string _defaultName = "PROJECTILE";

        protected static readonly Dictionary<string, string> projectileVanilla = new Dictionary<string, string>()
        {
            { "Default", "networkobjects:Bullet" },
            { "Bullet", "networkobjects:Bullet" },
            { "Rambro", "networkobjects:Bullet Rambo" },
            { "Rocket", "networkobjects:Rocket" },
            { "DrunkRocket", "networkobjects:Rocket Drunk" },
            { "ShotgunAdjusted", "networkobjects:Bullet Shotgun Adjusted" },
            { "ThrowingKnife", "networkobjects:ThrowingKnife" },
            { "ShockWave", "networkobjects:Bullet ShockWave Brade" },
            { "Turkey", "networkobjects:Sachel Pack Turkey" },
            { "SachelPack", "networkobjects:Sachel Pack" },
            { "NoisyCricket", "networkobjects:NoisyCricket" },
            { "BroboCop", "networkobjects:Bullet BroboCop" },
            { "RocketRemote", "networkobjects:Rocket Remote" },
            { "BulletSeeking", "networkobjects:Bullet Dredd" },
            { "IndianaBrones", "networkobjects:Bullet Indy" },
            { "Shotgun", "networkobjects:Bullet Shotgun Ash" },
            { "Machete", "networkobjects:Brochete Machete" },
            { "MacheteSpray", "networkobjects:Brochete Machete Spray" },
            { "Plasma", "networkobjects:PlasmaBullet" },
            { "TimeBro", "networkobjects:Bullet Time Bro" },
            { "BroniversalSoldier", "networkobjects:Bullet Broniversal Soldier" },
            { "KnifeSpray", "networkobjects:Broxmas Knife Spray" },
            { "GrenadeSticky", "networkobjects:Grenade Jensen" },
            { "Sniper", "networkobjects:Bullet DirtyBrory" },
            { "ShotgunFlame", "networkobjects:DragonBreath Flame Bullet" },
            { "Boomerang", "networkobjects:Boomerang" },
            { "Silence", "networkobjects:Bullet Double Bro Seven" },
            { "PredabroCanon", "networkobjects:Bullet Predabro" },
            { "SpearCharged", "networkobjects:Predabro Spear Charged" },
            { "Spear", "networkobjects:Predabro Spear" },
            { "Vomit", "networkobjects:FlyVomit" },
            { "Sword", "networkobjects:Broveheart Sword" },
            { "RocketBig", "networkobjects:Rocket Tankbro" },
            { "Arrow", "networkobjects:Arrow" },
            { "Stake", "networkobjects:Broffy Stake" },
            { "SachelPackSmall", "networkobjects:Sachel Pack Small" },
            { "DemolitionBomb", "networkobjects:DemolitionBroBomb" },
            { "Huge", "networkobjects:Bullet Huge Armoured" },
            { "MookMiniGun", "networkobjects:Bullet MookMiniGun" },
            { "RocketHuge", "networkobjects:Rocket Tankbro BIG" },
            { "ShellHeavy", "networkobjects:Shell Heavy Tankbro" },
            { "BabyDog", "networkobjects:ZHellDogEgg" },
            { "SlimeVomit", "networkobjects:Slime Vomit Drop" },
            { "Airstrike", "networkobjects:Rocket Launch Airstrike" },
            { "RocketSeeking", "networkobjects:Rocket Mook Bazooka" },
            { "RocketSeekingBig", "networkobjects:Rocket Tank Big" },
            { "FireBall", "networkobjects:Fireball Satan Flames" },
            { "FireBallBombardment", "networkobjects:Fireball Bombardment" },
            { "WarlockPortalGuided", "networkobjects:WarlockPortalProjectileGuided" },
            { "ShellMedium", "networkobjects:Shell Medium" },
            { "ShellHeavyEvil", "networkobjects:Shell Heavy" },
            { "FlameThrower1", "sharedassets:FlameThrower Bullet 1" },
            { "FlameThrower2", "sharedassets:FlameThrower Bullet 2" },
            { "FlameThrower3", "sharedassets:FlameThrower Bullet 3" },
        };


        public CustomProjectileInfo() : base() { }
        public CustomProjectileInfo(string name) : base(name) { }

        private CustomProjectileInfo(string name, bool isVanillaProjectile) : base(name)
        {
            this.isVanillaProjectile = isVanillaProjectile;
        }

        //public ProjectileStats stats;

        public static Projectile GetProjectileFromName(string name)
        {
            try
            {
                string resourceName = string.Empty;
                if (name.Contains(":"))
                    resourceName = name;
                else
                    resourceName = projectileVanilla[name];
                var go = InstantiationController.GetPrefabFromResourceName(resourceName);
                if (go != null)
                    return go.GetComponent<Projectile>();
            }
            catch (Exception e)
            {
                BMLogger.Log(e.ToString(), UnityEngine.LogType.Warning);
            }
            return InstantiationController.GetPrefabFromResourceName(projectileVanilla["Default"]).GetComponent<Projectile>();
        }
    }
}

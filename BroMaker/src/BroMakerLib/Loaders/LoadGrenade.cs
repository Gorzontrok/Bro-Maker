using BroMakerLib;
using BroMakerLib.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Loaders
{
    public static class LoadGrenade
    {
        #region consts
        public const string AIRSTRIKE = "airstrike";
        public const string ALIEN_PHEROMONE = "alien_pheromone";
        public const string FLASHBANG = "flashbang";
        public const string FREEZE = "freeze";
        public const string GRENADE_CLUSTER = "grenade_cluster";
        public const string HOLOGRAM = "hologram";
        public const string HOLY_WATER = "holy_water";
        public const string INCENDIARY = "incendiary";
        public const string MARTINI = "martini";
        public const string MECH_DROP = "mech_drop";
        public const string MOLOTOV = "molotov";
        public const string STICKY = "sticky";
        public const string TANK_DROP = "tank_drop";
        public const string TEARGAS = "teargas";
        #endregion

        public static Grenade GetGrenade(string grenadeName, GrenadeStats stats = null)
        {
            if(string.IsNullOrEmpty(grenadeName))
            {
                return HeroController.GetHeroPrefab(HeroType.Rambro).specialGrenade;
            }

            Grenade result = null;
            switch (grenadeName.ToLower())
            {
                case AIRSTRIKE:
                    result = HeroController.GetHeroPrefab(HeroType.BrodellWalker).specialGrenade; break;
                case ALIEN_PHEROMONE:
                    result = ProjectileController.GetAlienPheromoneGrenadePrefab(); break;
                case FLASHBANG:
                    result = HeroController.GetHeroPrefab(HeroType.BroHard).specialGrenade; break;
                case FREEZE:
                    result = HeroController.GetHeroPrefab(HeroType.DemolitionBro).specialGrenade; break;
                case GRENADE_CLUSTER:
                    result = HeroController.GetHeroPrefab(HeroType.ColJamesBroddock).specialGrenade; break;
                case HOLOGRAM:
                    result = HeroController.GetHeroPrefab(HeroType.SnakeBroSkin).specialGrenade; break;
                case HOLY_WATER:
                    result = HeroController.GetHeroPrefab(HeroType.Broffy).specialGrenade; break;
                case INCENDIARY:
                    result = HeroController.GetHeroPrefab(HeroType.BaBroracus).specialGrenade; break;
                case MARTINI:
                    result = (HeroController.GetHeroPrefab(HeroType.DoubleBroSeven) as DoubleBroSeven).martiniGlass; break;
                case MECH_DROP:
                    result = ProjectileController.GetMechDropGrenadePrefab(); break;
                case MOLOTOV:
                    result = HeroController.GetHeroPrefab(HeroType.DirtyHarry).specialGrenade; break;
                case STICKY:
                    result = HeroController.GetHeroPrefab(HeroType.BroneyRoss).specialGrenade; break;
                case TANK_DROP:
                    result = HeroController.GetHeroPrefab(HeroType.TankBro).specialGrenade; break;
                case TEARGAS:
                    result = (HeroController.GetHeroPrefab(HeroType.DoubleBroSeven) as DoubleBroSeven).tearGasGrenade; break;
                default:
                    // Try load custom grenade
                    break;
            }
            if (result == null)
            {
                result = HeroController.GetHeroPrefab(HeroType.Rambro).specialGrenade;
            }
            result.LoadStats(stats);
            return result;
        }
    }
}

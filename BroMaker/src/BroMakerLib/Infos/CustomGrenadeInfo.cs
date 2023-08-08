using BroMakerLib.Stats;
using BroMakerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BroMakerLib.Loggers;

namespace BroMakerLib.Infos
{
    public class CustomGrenadeInfo : CustomBroforceObjectInfo
    {
        [CantBeNull]
        public GrenadeStats stats = new GrenadeStats();

        protected new string _defaultName = "GRENADE";

        protected static readonly Dictionary<string, string> grenadesVanilla = new Dictionary<string, string>()
        {
            { "Grenade", "networkobjects:Grenade" },
            { "Default", "networkobjects:Grenade" },
            { "Martini", "networkobjects:Martini Grenade" },
            { "TearGas", "networkobjects:GrenadeTearGas" },
            { "FlameWave", "networkobjects:GrenadeFlameWave" },
            { "AirStrike", "networkobjects:Grenade Airstrike" },
            { "FlashBang", "networkobjects:FlashBang" },
            { "Hologram", "networkobjects:GrenadeHologram" },
            { "Cluster", "networkobjects:GrenadeCluster" },
            { "Sticky", "networkobjects:Grenade Sticky" },
            { "GrenadeTollBroad", "networkobjects:GrenadeTollBroad" },
            { "SummonTank", "networkobjects:Grenade Summon Tank" },
            { "Molotove", "networkobjects:Grenade Molotove" },
            { "HolyWater", "networkobjects:GrenadeHolyWater" },
            { "Freeze", "networkobjects:DemolitionBroFreezeBomb" },
            { "MechDrop", "networkobjects:Grenade MechDrop" },
            { "AlienPheromones", "networkobjects:Grenade Alien Pheromones" },
            { "EvilSmall", "networkobjects:GrenadeEvilSmall Long fuse" },
            { "EvilBig", "networkobjects:GrenadeEvilBig" },
            { "EvilBigShortLife", "networkobjects:GrenadeEvilBig Short Life" },
        };

        public CustomGrenadeInfo() : base() { }
        public CustomGrenadeInfo(string name) : base(name) {}

        public override void Initialize()
        {
            base.Initialize();
            if(stats == null)
                stats = new GrenadeStats();
        }
        public override string SerializeJSON()
        {
            return SerializeJSON(DirectoriesManager.WeaponsDirectory);
        }

        public static Grenade GetGrenadeFromName(string name)
        {
            Grenade result = null;
            try
            {
                var go = InstantiationController.GetPrefabFromResourceName(grenadesVanilla[name]);
                if(go != null)
                    return go.GetComponent<Grenade>();
            }
            catch(Exception e)
            {
                BMLogger.Debug(e);
            }
            return InstantiationController.GetPrefabFromResourceName(grenadesVanilla["Default"]).GetComponent<Grenade>();
        }
    }
}

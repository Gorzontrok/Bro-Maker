using BroMakerLib.Attributes;
using BroMakerLib.CustomObjects;
using BroMakerLib.CustomObjects.Grenades;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace BroMakerLib.Loaders
{
    public static class LoadGrenade
    {
        public static readonly Dictionary<string, string> GrenadesVanilla = new Dictionary<string, string>()
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

        public static CustomGrenadeInfo currentInfo;

        public static Grenade FromName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return InstantiationController.GetPrefabFromResourceName(GrenadesVanilla["Default"]).GetComponent<Grenade>();
            }
            try
            {
                string resourceName = string.Empty;
                if (name.Contains(":"))
                    resourceName = name;
                else
                    resourceName = GrenadesVanilla[name];
                var go = InstantiationController.GetPrefabFromResourceName(resourceName);
                if (go != null)
                    return go.GetComponent<Grenade>();
            }
            catch (Exception e)
            {
                BMLogger.Log($"Error with loading {name}\n{e}", LogType.Warning);
            }
            return InstantiationController.GetPrefabFromResourceName(GrenadesVanilla["Default"]).GetComponent<Grenade>();
        }

        public static Grenade WithInfo(CustomGrenadeInfo info)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            Type preset = PresetManager.GetGrenadePreset(info.Preset);
            if (preset == null)
                throw new Exception($"Preset {info.Preset} not founded");
            if (!typeof(CustomGrenade).IsAssignableFrom(preset))
                throw new Exception($"Preset {info.Preset} don't inherted from 'CustomGrenade' class.");

            BMLogger.Debug("LoadGrenade.WithInfo: Start Creation Process.");

            currentInfo = info;
            Grenade result = null;

            string basedOn = GetBaseGrenadeNameOfPreset(preset);
            if (!GrenadesVanilla.ContainsKey(basedOn))
            {
                BMLogger.Warning($"Vanilla grenade {basedOn} not founded. Using Default grenade instead.");
                basedOn = GrenadesVanilla.First().Key;
            }
            else
            {
                basedOn = GrenadesVanilla[basedOn];
            }

            Grenade originalGrenade = FromName(basedOn);
            if (originalGrenade == null)
                throw new Exception("Failed to get original grenade");

            return null;
        }

        private static GameObject CreateOriginal(Type type)
        {
            //GameObject prefab = GetPrefab("networkobjects:Rambo");
            GameObject prefab = null;
            GameObject inst = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);

            inst.AddComponent(type);
            inst.name = LoadHero.GAMEOBJECT_PREFIX + currentInfo.name;
            if (inst == null)
                throw new Exception("CreateOriginal: Instantiate has failed is null");
            BMLogger.Debug("CreateOriginal: Has Instantiate Hero.");
            inst.SetActive(false);

            AddObjectToPrefabList(inst);
            BMLogger.Debug("CreateOriginal: inst added to list");

            return inst;
        }

        private static string GetBaseGrenadeNameOfPreset(Type type)
        {
            var attributes = type.GetCustomAttributes(typeof(GrenadePresetAttribute), true);
            if (attributes.IsNotNullOrEmpty())
            {
                return attributes[0].As<GrenadePresetAttribute>().basedOn;
            }
            throw new NotImplementedException($"Type {type} as no attribute of {nameof(GrenadePresetAttribute)}");
        }

        private static void AddObjectToPrefabList(GameObject obj)
        { }
    }
}

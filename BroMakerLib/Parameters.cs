using BroMakerLib.Attributes;
using BroMakerLib.Loggers;
using System;
using UnityEngine;

namespace BroMakerLib
{
    public static class Parameters
    {
        [Parameter]
        public static void Halo(object obj, bool value)
        {
            if(value && obj as TestVanDammeAnim)
            {
                var character = obj as TestVanDammeAnim;

                var halo = HeroController.GetHeroPrefab(HeroType.Broffy).halo;
                character.halo = UnityEngine.Object.Instantiate(halo, halo.transform.localPosition, Quaternion.identity);
                character.halo.transform.parent = character.transform;
            }
        }

        [Parameter]
        public static void HaloPreset(object obj, string value)
        {
            if (value.IsNullOrEmpty())
            {
                BMLogger.Warning("HaloPreset is empty or null");
                return;
            }

            var character = obj as TestVanDammeAnim;
            if (!character)
            {
                BMLogger.Warning("HaloPreset parameter works only with characters.");
                return;
            }

            if (character.halo == null)
            {
                BMLogger.Warning("Halo is null, can't apply preset.");
                return;
            }

            if (PresetManager.customObjectsPreset.TryGetValue(value, out Type haloType))
            {
                character.halo.gameObject.AddComponent(haloType);
            }
            else
                BMLogger.Log($"Halo preset {value} not founded.", LogType.Warning);
        }

        [Parameter]
        public static void TestParameter(object obj, string value)
        {
            BMLogger.Log("TestParameter: " + value);
        }
    }
}

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
            var character = obj as TestVanDammeAnim;
            if (!character)
            {
                BMLogger.Warning("HaloPreset parameter works only with characters.");
                return;
            }

            if (value)
            {
                var halo = HeroController.GetHeroPrefab(HeroType.Broffy).halo;
                character.halo = UnityEngine.Object.Instantiate(halo, halo.transform.localPosition, Quaternion.identity);
                character.halo.transform.parent = character.transform;
            }
        }

        [Parameter]
        public static void HaloPreset(object obj, string value)
        {
            if (value != null || value.IsNullOrEmpty())
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
        public static void TestParameter(object obj, object value)
        {
            BMLogger.Log("TestParameter: " + value);
        }

        [Parameter]
        public static void BetterAnimation(object obj, bool value)
        {
            var character = obj as TestVanDammeAnim;
            if (!character)
            {
                BMLogger.Warning($"{nameof(BetterAnimation)} parameter works only with characters.");
                return;
            }

            if (value)
            {
                character.doRollOnLand = true;
                character.useDashFrames = true;
                character.useNewFrames = true;
                character.useNewKnifingFrames = true;
                character.useNewLedgeGrappleFrames = true;
                character.useNewThrowingFrames = true;
                character.useNewHighFivingFrames = true;
                character.SetFieldValue("hasNewAirFlexFrames", true);
                character.useNewKnifeClimbingFrames = true;
                character.useDuckingFrames = true;
                character.useNewDuckingFrames = true;
            }
        }

        [Parameter]
        public static void JetPackSprite(object obj, bool value)
        {
            var broBase = obj as BroBase;
            if (!broBase)
            {
                BMLogger.Warning($"{nameof(JetPackSprite)} parameter works only with Bros.");
                return;
            }

            var jetPackSprite = HeroController.GetHeroPrefab(HeroType.Rambro).As<BroBase>().jetPackSprite;
            broBase.jetPackSprite = UnityEngine.Object.Instantiate(jetPackSprite, jetPackSprite.transform.localPosition, Quaternion.identity);
            broBase.jetPackSprite.transform.parent = broBase.transform;
        }
    }
}

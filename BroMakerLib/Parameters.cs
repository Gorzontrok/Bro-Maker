using BroMakerLib.Attributes;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Loggers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        [Parameter]
        public static void SpecialIcons(object obj, object value)
        {
            CustomHero hero = obj as CustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(SpecialIcons)} parameter works only with Bros.");
                return;
            }
            if (hero.specialMaterials == null)
                hero.specialMaterials = new System.Collections.Generic.List<Material>();

            if (hero.specialMaterials.Count > 0)
                return;

            if (value is string)
            {
                string iconFile = value as string;
                Material specialMat = ResourcesController.GetMaterial(hero.info.path, iconFile);
                hero.specialMaterials.Add(specialMat);
            }
            else if (value is JArray)
            {
                JArray iconFiles = value as JArray;
                for (int i = 0; i < iconFiles.Count; ++i)
                {
                    Material specialMat = ResourcesController.GetMaterial(hero.info.path, iconFiles[i].ToObject<string>());
                    hero.specialMaterials.Add(specialMat);
                }
            }
            else if (value is string[])
            {
                string[] iconFiles = value as string[];
                for (int i = 0; i < iconFiles.Length; ++i)
                {
                    Material specialMat = ResourcesController.GetMaterial(hero.info.path, iconFiles[i]);
                    hero.specialMaterials.Add(specialMat);
                }
            }
            else
            {
                BMLogger.Warning($"SpecialIcons value is type of {value.GetType()} it must be a String or a String Array");
            }
        }

        [Parameter]
        public static void Avatar(object obj, string value)
        {
            CustomHero hero = obj as CustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(Avatar)} parameter works only with Bros.");
                return;
            }

            hero.firstAvatar = ResourcesController.GetMaterial(hero.info.path, value as string);
        }

        [Parameter]
        public static void GunSpriteOffset(object obj, object value)
        {
            CustomHero hero = obj as CustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(GunSpriteOffset)} parameter works only with Bros.");
                return;
            }

            try
            {
                if (value is JObject)
                {
                    JToken xToken = value.As<JObject>().GetValue("x");
                    JToken yToken = value.As<JObject>().GetValue("y");


                    float x = Convert.ToSingle(xToken.ToObject<object>());
                    float y = Convert.ToSingle(yToken.ToObject<object>());
                    hero.gunSpriteOffset = new Vector2(x, y);
                    hero.info.gunSpriteOffset = new Vector2(x, y);
                }
                else
                {
                    BMLogger.Error("Can't load GunSpriteOffset value. It should be { \"x\": 0, \"y\": 0 } (0 is replacable)");
                }
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
            }
        }
    }
}

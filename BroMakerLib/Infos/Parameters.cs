using System;
using System.Collections.Generic;
using BroMakerLib.Attributes;
using BroMakerLib.CustomObjects;
using BroMakerLib.Loggers;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Local

namespace BroMakerLib.Infos
{
    internal static class Parameters
    {
        private static T Target<T>(object obj, string parameter) where T : class
        {
            T target = obj as T;
            if (target == null)
            {
                BMLogger.Warning($"{parameter} only works on bros");
            }
            return target;
        }

        [Parameter]
        private static void Halo(object obj, bool value)
        {
            TestVanDammeAnim character = Target<TestVanDammeAnim>(obj, nameof(Halo));
            if (character == null)
            {
                return;
            }

            if (value)
            {
                BroHalo halo = HeroController.GetHeroPrefab(HeroType.Broffy).halo;
                character.halo = Object.Instantiate(halo, halo.transform.localPosition, Quaternion.identity);
                character.halo.transform.SetParent(character.transform, false);
            }
        }

        [Parameter]
        private static void HaloPreset(object obj, string value)
        {
            string broName = (obj as ICustomHero)?.Info?.name;
            if (value.IsNullOrEmpty())
            {
                BMLogger.Warning("HaloPreset is empty", bro: broName);
                return;
            }

            TestVanDammeAnim character = Target<TestVanDammeAnim>(obj, nameof(HaloPreset));
            if (character == null)
            {
                return;
            }

            if (character.halo == null)
            {
                BMLogger.Warning("HaloPreset needs Halo set to true", bro: broName);
                return;
            }

            Type haloType = PresetManager.GetCustomObjectPreset(value);
            if (haloType != null)
            {
                character.halo.gameObject.AddComponent(haloType);
            }
            else
            {
                BMLogger.Warning($"Halo preset '{value}' was not found", bro: broName);
            }
        }

        [Parameter]
        private static void BetterAnimation(object obj, bool value)
        {
            TestVanDammeAnim character = Target<TestVanDammeAnim>(obj, nameof(BetterAnimation));
            if (character == null)
            {
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
        private static void JetPackSprite(object obj, bool value)
        {
            BroBase broBase = Target<BroBase>(obj, nameof(JetPackSprite));
            if (broBase == null)
            {
                return;
            }

            if (value && (broBase.jetPackSprite == null || !broBase.jetPackSprite.transform.IsChildOf(broBase.transform)))
            {
                SpriteSM jetPackSprite = HeroController.GetHeroPrefab(HeroType.Rambro).As<BroBase>().jetPackSprite;
                broBase.jetPackSprite = Object.Instantiate(jetPackSprite, jetPackSprite.transform.localPosition, Quaternion.identity);
                broBase.jetPackSprite.transform.SetParent(broBase.transform, false);
            }
        }

        [Parameter]
        private static void SpecialIcons(object obj, object value)
        {
            ICustomHero hero = Target<ICustomHero>(obj, nameof(SpecialIcons));
            if (hero == null)
            {
                return;
            }

            // Prevent multiple calls from overwriting data
            // Check if we have more than the default empty list, or if the default list has content
            if (hero.Info.SpecialMaterials.Count > 1 || (hero.Info.SpecialMaterials.Count == 1 && hero.Info.SpecialMaterials[0].Count > 0))
            {
                return;
            }

            // Clear existing special materials first
            hero.Info.SpecialMaterials.Clear();

            if (value is string s)
            {
                // Single string - shared across all variants
                Material specialMat = ResourcesController.GetMaterial(hero.Info.path, s);
                hero.Info.SpecialMaterials.Add(new List<Material> { specialMat });
            }
            else if (value is JArray array)
            {
                // Check if it's an array of arrays or array of strings
                if (array.Count > 0)
                {
                    if (array[0] is JArray)
                    {
                        // Array of arrays - each variant has multiple icons
                        foreach (JToken variantArray in array)
                        {
                            List<Material> variantMaterials = new List<Material>();
                            if (variantArray is JArray icons)
                            {
                                foreach (JToken icon in icons)
                                {
                                    string iconFile = icon.ToObject<string>();
                                    Material specialMat = ResourcesController.GetMaterial(hero.Info.path, iconFile);
                                    variantMaterials.Add(specialMat);
                                }
                            }

                            hero.Info.SpecialMaterials.Add(variantMaterials);
                        }
                    }
                    else
                    {
                        // Array of strings - one variant with multiple icons (different icon per special ammo slot)
                        List<Material> variantMaterials = new List<Material>();
                        foreach (JToken item in array)
                        {
                            string iconFile = item.ToObject<string>();
                            Material specialMat = ResourcesController.GetMaterial(hero.Info.path, iconFile);
                            variantMaterials.Add(specialMat);
                        }

                        hero.Info.SpecialMaterials.Add(variantMaterials);
                    }
                }
            }
            else if (value is string[] iconFiles)
            {
                // Array of strings - one variant with multiple icons
                List<Material> variantMaterials = new List<Material>();
                foreach (string t in iconFiles)
                {
                    Material specialMat = ResourcesController.GetMaterial(hero.Info.path, t);
                    variantMaterials.Add(specialMat);
                }

                hero.Info.SpecialMaterials.Add(variantMaterials);
            }
            else
            {
                BMLogger.Warning(
                    $"SpecialIcons can't be {BroMakerUtilities.DescribeJsonValue(value.GetType()) ?? "that kind of value"}. It must be a file name, a list of file names, or a list of lists of file names",
                    bro: hero.Info?.name);
            }
        }

        [Parameter]
        private static void SpecialIconOffset(object obj, object value)
        {
            ICustomHero hero = Target<ICustomHero>(obj, nameof(SpecialIconOffset));
            if (hero == null)
            {
                return;
            }

            try
            {
                // Handle array of offsets
                if (value is JArray array)
                {
                    // Prevent multiple calls from overwriting data
                    if (hero.Info.SpecialMaterialOffset.Count > 1 || (hero.Info.SpecialMaterialOffset.Count == 1 && hero.Info.SpecialMaterialOffset[0] != Vector2.zero))
                    {
                        return;
                    }

                    hero.Info.SpecialMaterialOffset.Clear();
                    foreach (JToken item in array)
                    {
                        if (item is JObject jObj)
                        {
                            float x = Convert.ToSingle(jObj.GetValue("x").ToObject<object>());
                            float y = Convert.ToSingle(jObj.GetValue("y").ToObject<object>());
                            hero.Info.SpecialMaterialOffset.Add(new Vector2(x, y));
                        }
                    }
                }
                // Handle single offset (backwards compatibility)
                else if (value is JObject)
                {
                    // Only process if not already set as array
                    if (hero.Info.SpecialMaterialOffset.Count <= 1)
                    {
                        JToken xToken = value.As<JObject>().GetValue("x");
                        JToken yToken = value.As<JObject>().GetValue("y");

                        float x = Convert.ToSingle(xToken.ToObject<object>());
                        float y = Convert.ToSingle(yToken.ToObject<object>());

                        if (hero.Info.SpecialMaterialOffset.Count == 0)
                        {
                            hero.Info.SpecialMaterialOffset.Add(new Vector2(x, y));
                        }
                        else if (hero.Info.SpecialMaterialOffset[0] == Vector2.zero)
                        {
                            hero.Info.SpecialMaterialOffset[0] = new Vector2(x, y);
                        }
                    }
                }
                else
                {
                    BMLogger.Error("Can't load SpecialIconOffset value. It should be { \"x\": 0, \"y\": 0 } or an array of such objects", bro: hero.Info?.name);
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Can't read SpecialIconOffset for '{hero.Info?.name}'", ex, bro: hero.Info?.name);
            }
        }

        [Parameter]
        private static void SpecialIconSpacing(object obj, object value)
        {
            ICustomHero hero = Target<ICustomHero>(obj, nameof(SpecialIconSpacing));
            if (hero == null)
            {
                return;
            }

            try
            {
                // Handle array of spacings
                if (value is JArray array)
                {
                    // Prevent multiple calls from overwriting data
                    if (hero.Info.SpecialMaterialSpacing.Count > 1 || (hero.Info.SpecialMaterialSpacing.Count == 1 && hero.Info.SpecialMaterialSpacing[0] != 0f))
                    {
                        return;
                    }

                    hero.Info.SpecialMaterialSpacing.Clear();
                    foreach (JToken item in array)
                    {
                        hero.Info.SpecialMaterialSpacing.Add(Convert.ToSingle(item));
                    }
                }
                // Handle single spacing (backwards compatibility)
                else
                {
                    // Only process if not already set as array
                    if (hero.Info.SpecialMaterialSpacing.Count <= 1)
                    {
                        if (hero.Info.SpecialMaterialSpacing.Count == 0)
                        {
                            hero.Info.SpecialMaterialSpacing.Add(Convert.ToSingle(value));
                        }
                        else if (hero.Info.SpecialMaterialSpacing[0] == 0f)
                        {
                            hero.Info.SpecialMaterialSpacing[0] = Convert.ToSingle(value);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Can't read SpecialIconSpacing for '{hero.Info?.name}'", ex, bro: hero.Info?.name);
            }
        }

        [Parameter]
        private static void Avatar(object obj, object value)
        {
            ICustomHero hero = Target<ICustomHero>(obj, nameof(Avatar));
            if (hero == null)
            {
                return;
            }

            // Handle array of avatars
            if (value is JArray array)
            {
                // Prevent multiple calls from overwriting data
                if (hero.Info.FirstAvatar.Count > 1 || (hero.Info.FirstAvatar.Count == 1 && hero.Info.FirstAvatar[0] != null))
                {
                    return;
                }

                hero.Info.FirstAvatar.Clear();
                foreach (JToken item in array)
                {
                    string avatarPath = item.ToObject<string>();
                    hero.Info.FirstAvatar.Add(ResourcesController.GetMaterial(hero.Info.path, avatarPath));
                }
            }
            else if (value is string[] stringArray)
            {
                // Prevent multiple calls from overwriting data
                if (hero.Info.FirstAvatar.Count > 1 || (hero.Info.FirstAvatar.Count == 1 && hero.Info.FirstAvatar[0] != null))
                {
                    return;
                }

                hero.Info.FirstAvatar.Clear();
                foreach (string avatarPath in stringArray)
                {
                    hero.Info.FirstAvatar.Add(ResourcesController.GetMaterial(hero.Info.path, avatarPath));
                }
            }
            // Handle single avatar (backwards compatibility)
            else if (value is string s)
            {
                // Only process if not already set as array
                if (hero.Info.FirstAvatar.Count <= 1)
                {
                    if (hero.Info.FirstAvatar.Count == 0)
                    {
                        hero.Info.FirstAvatar.Add(ResourcesController.GetMaterial(hero.Info.path, s));
                    }
                    else if (hero.Info.FirstAvatar[0] == null)
                    {
                        hero.Info.FirstAvatar[0] = ResourcesController.GetMaterial(hero.Info.path, s);
                    }
                }
            }
        }

        [Parameter]
        private static void GunSpriteOffset(object obj, object value)
        {
            ICustomHero hero = Target<ICustomHero>(obj, nameof(GunSpriteOffset));
            if (hero == null)
            {
                return;
            }

            try
            {
                // Handle array of offsets
                if (value is JArray array)
                {
                    // Prevent multiple calls from overwriting data
                    if (hero.Info.GunSpriteOffset.Count > 1 || (hero.Info.GunSpriteOffset.Count == 1 && hero.Info.GunSpriteOffset[0] != Vector2.zero))
                    {
                        return;
                    }

                    hero.Info.GunSpriteOffset.Clear();
                    foreach (JToken item in array)
                    {
                        if (item is JObject jObj)
                        {
                            float x = Convert.ToSingle(jObj.GetValue("x").ToObject<object>());
                            float y = Convert.ToSingle(jObj.GetValue("y").ToObject<object>());
                            hero.Info.GunSpriteOffset.Add(new Vector2(x, y));
                        }
                    }
                }
                // Handle single offset (backwards compatibility)
                else if (value is JObject)
                {
                    // Only process if not already set as array
                    if (hero.Info.GunSpriteOffset.Count <= 1)
                    {
                        JToken xToken = value.As<JObject>().GetValue("x");
                        JToken yToken = value.As<JObject>().GetValue("y");

                        float x = Convert.ToSingle(xToken.ToObject<object>());
                        float y = Convert.ToSingle(yToken.ToObject<object>());

                        if (hero.Info.GunSpriteOffset.Count == 0)
                        {
                            hero.Info.GunSpriteOffset.Add(new Vector2(x, y));
                        }
                        else if (hero.Info.GunSpriteOffset[0] == Vector2.zero)
                        {
                            hero.Info.GunSpriteOffset[0] = new Vector2(x, y);
                        }
                    }
                }
                else
                {
                    BMLogger.Error("Can't load GunSpriteOffset value. It should be { \"x\": 0, \"y\": 0 } or an array of such objects", bro: hero.Info?.name);
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Can't read GunSpriteOffset for '{hero.Info?.name}'", ex, bro: hero.Info?.name);
            }
        }

        [Parameter]
        private static void Sprite(object obj, object value)
        {
            ICustomHero hero = Target<ICustomHero>(obj, nameof(Sprite));
            if (hero == null)
            {
                return;
            }

            if (hero.Info.SpritePath.Count != 0)
            {
                return;
            }

            // Handle array of sprites
            if (value is JArray array)
            {
                hero.Info.SpritePath.Clear();
                foreach (JToken item in array)
                {
                    hero.Info.SpritePath.Add(item.ToObject<string>());
                }
            }
            else if (value is string[] stringArray)
            {
                hero.Info.SpritePath.Clear();
                hero.Info.SpritePath.AddRange(stringArray);
            }
            // Handle single sprite (backwards compatibility)
            else if (value is string s)
            {
                if (hero.Info.SpritePath.Count == 0)
                {
                    hero.Info.SpritePath.Add(s);
                }
                else if (hero.Info.SpritePath.Count == 1)
                {
                    hero.Info.SpritePath[0] = s;
                }
                // If count > 1, it's already been set as an array, don't override
            }
        }

        [Parameter]
        private static void GunSprite(object obj, object value)
        {
            ICustomHero hero = Target<ICustomHero>(obj, nameof(GunSprite));
            if (hero == null)
            {
                return;
            }

            if (hero.Info.GunSpritePath.Count != 0)
            {
                return;
            }

            // Handle array of gun sprites
            if (value is JArray array)
            {
                hero.Info.GunSpritePath.Clear();
                foreach (JToken item in array)
                {
                    hero.Info.GunSpritePath.Add(item.ToObject<string>());
                }
            }
            else if (value is string[] stringArray)
            {
                hero.Info.GunSpritePath.Clear();
                hero.Info.GunSpritePath.AddRange(stringArray);
            }
            // Handle single gun sprite
            else if (value is string s)
            {
                if (hero.Info.GunSpritePath.Count == 0)
                {
                    hero.Info.GunSpritePath.Add(s);
                }
                else if (hero.Info.GunSpritePath.Count == 1)
                {
                    hero.Info.GunSpritePath[0] = s;
                }
            }
        }
    }
}

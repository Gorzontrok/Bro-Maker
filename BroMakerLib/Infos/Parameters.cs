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
        [Parameter]
        private static void Halo(object obj, bool value)
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
                character.halo = Object.Instantiate(halo, halo.transform.localPosition, Quaternion.identity);
                character.halo.transform.parent = character.transform;
            }
        }

        [Parameter]
        private static void HaloPreset(object obj, string value)
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

            if (PresetManager.customObjectsPreset.TryGetValue(value, out var haloType))
            {
                character.halo.gameObject.AddComponent(haloType);
            }
            else
            {
                BMLogger.Log($"Halo preset {value} not founded.", LogType.Warning);
            }
        }

        [Parameter]
        private static void BetterAnimation(object obj, bool value)
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
        private static void JetPackSprite(object obj, bool value)
        {
            var broBase = obj as BroBase;
            if (!broBase)
            {
                BMLogger.Warning($"{nameof(JetPackSprite)} parameter works only with Bros.");
                return;
            }

            if (value)
            {
                var jetPackSprite = HeroController.GetHeroPrefab(HeroType.Rambro).As<BroBase>().jetPackSprite;
                broBase.jetPackSprite = Object.Instantiate(jetPackSprite, jetPackSprite.transform.localPosition, Quaternion.identity);
                broBase.jetPackSprite.transform.parent = broBase.transform;
            }
        }

        [Parameter]
        private static void SpecialIcons(object obj, object value)
        {
            var hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(SpecialIcons)} parameter works only with Bros.");
                return;
            }

            // Prevent multiple calls from overwriting data
            // Check if we have more than the default empty list, or if the default list has content
            if (hero.Info.SpecialMaterials.Count > 1 ||
                (hero.Info.SpecialMaterials.Count == 1 && hero.Info.SpecialMaterials[0].Count > 0))
            {
                return;
            }

            // Clear existing special materials first
            hero.Info.SpecialMaterials.Clear();

            if (value is string s)
            {
                // Single string - shared across all variants
                var specialMat = ResourcesController.GetMaterial(hero.Info.path, s);
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
                        foreach (var variantArray in array)
                        {
                            var variantMaterials = new List<Material>();
                            if (variantArray is JArray icons)
                            {
                                foreach (var icon in icons)
                                {
                                    var iconFile = icon.ToObject<string>();
                                    var specialMat = ResourcesController.GetMaterial(hero.Info.path, iconFile);
                                    variantMaterials.Add(specialMat);
                                }
                            }

                            hero.Info.SpecialMaterials.Add(variantMaterials);
                        }
                    }
                    else
                    {
                        // Array of strings - one variant with multiple icons (different icon per special ammo slot)
                        var variantMaterials = new List<Material>();
                        foreach (var item in array)
                        {
                            var iconFile = item.ToObject<string>();
                            var specialMat = ResourcesController.GetMaterial(hero.Info.path, iconFile);
                            variantMaterials.Add(specialMat);
                        }

                        hero.Info.SpecialMaterials.Add(variantMaterials);
                    }
                }
            }
            else if (value is string[] iconFiles)
            {
                // Array of strings - one variant with multiple icons
                var variantMaterials = new List<Material>();
                foreach (var t in iconFiles)
                {
                    var specialMat = ResourcesController.GetMaterial(hero.Info.path, t);
                    variantMaterials.Add(specialMat);
                }

                hero.Info.SpecialMaterials.Add(variantMaterials);
            }
            else
            {
                BMLogger.Warning($"SpecialIcons value is type of {value.GetType()} it must be a String, String Array, or Array of String Arrays");
            }
        }

        [Parameter]
        private static void SpecialIconOffset(object obj, object value)
        {
            var hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(SpecialIconOffset)} parameter works only with Bros.");
                return;
            }

            try
            {
                // Handle array of offsets
                if (value is JArray array)
                {
                    // Prevent multiple calls from overwriting data
                    if (hero.Info.SpecialMaterialOffset.Count > 1 ||
                        (hero.Info.SpecialMaterialOffset.Count == 1 && hero.Info.SpecialMaterialOffset[0] != Vector2.zero))
                    {
                        return;
                    }

                    hero.Info.SpecialMaterialOffset.Clear();
                    foreach (var item in array)
                    {
                        if (item is JObject jObj)
                        {
                            var x = Convert.ToSingle(jObj.GetValue("x").ToObject<object>());
                            var y = Convert.ToSingle(jObj.GetValue("y").ToObject<object>());
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
                        var xToken = value.As<JObject>().GetValue("x");
                        var yToken = value.As<JObject>().GetValue("y");

                        var x = Convert.ToSingle(xToken.ToObject<object>());
                        var y = Convert.ToSingle(yToken.ToObject<object>());

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
                    BMLogger.Error("Can't load SpecialIconOffset value. It should be { \"x\": 0, \"y\": 0 } or an array of such objects");
                }
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
            }
        }

        [Parameter]
        private static void SpecialIconSpacing(object obj, object value)
        {
            var hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(SpecialIconSpacing)} parameter works only with Bros.");
                return;
            }

            try
            {
                // Handle array of spacings
                if (value is JArray array)
                {
                    // Prevent multiple calls from overwriting data
                    if (hero.Info.SpecialMaterialSpacing.Count > 1 ||
                        (hero.Info.SpecialMaterialSpacing.Count == 1 && hero.Info.SpecialMaterialSpacing[0] != 0f))
                    {
                        return;
                    }

                    hero.Info.SpecialMaterialSpacing.Clear();
                    foreach (var item in array)
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
                BMLogger.ExceptionLog(ex);
            }
        }

        [Parameter]
        private static void Avatar(object obj, object value)
        {
            var hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(Avatar)} parameter works only with Bros.");
                return;
            }

            // Handle array of avatars
            if (value is JArray array)
            {
                // Prevent multiple calls from overwriting data
                if (hero.Info.FirstAvatar.Count > 1 ||
                    (hero.Info.FirstAvatar.Count == 1 && hero.Info.FirstAvatar[0] != null))
                {
                    return;
                }

                hero.Info.FirstAvatar.Clear();
                foreach (var item in array)
                {
                    var avatarPath = item.ToObject<string>();
                    hero.Info.FirstAvatar.Add(ResourcesController.GetMaterial(hero.Info.path, avatarPath));
                }
            }
            else if (value is string[] stringArray)
            {
                // Prevent multiple calls from overwriting data
                if (hero.Info.FirstAvatar.Count > 1 ||
                    (hero.Info.FirstAvatar.Count == 1 && hero.Info.FirstAvatar[0] != null))
                {
                    return;
                }

                hero.Info.FirstAvatar.Clear();
                foreach (var avatarPath in stringArray)
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
            var hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(GunSpriteOffset)} parameter works only with Bros.");
                return;
            }

            try
            {
                // Handle array of offsets
                if (value is JArray array)
                {
                    // Prevent multiple calls from overwriting data
                    if (hero.Info.GunSpriteOffset.Count > 1 ||
                        (hero.Info.GunSpriteOffset.Count == 1 && hero.Info.GunSpriteOffset[0] != Vector2.zero))
                    {
                        return;
                    }

                    hero.Info.GunSpriteOffset.Clear();
                    foreach (var item in array)
                    {
                        if (item is JObject jObj)
                        {
                            var x = Convert.ToSingle(jObj.GetValue("x").ToObject<object>());
                            var y = Convert.ToSingle(jObj.GetValue("y").ToObject<object>());
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
                        var xToken = value.As<JObject>().GetValue("x");
                        var yToken = value.As<JObject>().GetValue("y");

                        var x = Convert.ToSingle(xToken.ToObject<object>());
                        var y = Convert.ToSingle(yToken.ToObject<object>());

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
                    BMLogger.Error("Can't load GunSpriteOffset value. It should be { \"x\": 0, \"y\": 0 } or an array of such objects");
                }
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
            }
        }

        [Parameter]
        private static void Sprite(object obj, object value)
        {
            var hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(Sprite)} parameter works only with Bros.");
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
                foreach (var item in array)
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
            var hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(GunSprite)} parameter works only with Bros.");
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
                foreach (var item in array)
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
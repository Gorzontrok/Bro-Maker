using BroMakerLib.Attributes;
using BroMakerLib.CustomObjects;
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
            ICustomHero hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(SpecialIcons)} parameter works only with Bros.");
                return;
            }
            
            // Prevent multiple calls from overwriting data
            // Check if we have more than the default empty list, or if the default list has content
            if (hero.info.SpecialMaterials.Count > 1 || 
                (hero.info.SpecialMaterials.Count == 1 && hero.info.SpecialMaterials[0].Count > 0))
                return;
                
            // Clear existing special materials first
            hero.info.SpecialMaterials.Clear();

            if (value is string)
            {
                // Single string - shared across all variants
                string iconFile = value as string;
                Material specialMat = ResourcesController.GetMaterial(hero.info.path, iconFile);
                hero.info.SpecialMaterials.Add(new List<Material> { specialMat });
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
                                    string iconFile = icon.ToObject<string>();
                                    Material specialMat = ResourcesController.GetMaterial(hero.info.path, iconFile);
                                    variantMaterials.Add(specialMat);
                                }
                            }
                            hero.info.SpecialMaterials.Add(variantMaterials);
                        }
                    }
                    else
                    {
                        // Array of strings - one icon per variant
                        foreach (var item in array)
                        {
                            string iconFile = item.ToObject<string>();
                            Material specialMat = ResourcesController.GetMaterial(hero.info.path, iconFile);
                            hero.info.SpecialMaterials.Add(new List<Material> { specialMat });
                        }
                    }
                }
            }
            else if (value is string[])
            {
                // Array of strings - one icon per variant
                string[] iconFiles = value as string[];
                for (int i = 0; i < iconFiles.Length; ++i)
                {
                    Material specialMat = ResourcesController.GetMaterial(hero.info.path, iconFiles[i]);
                    hero.info.SpecialMaterials.Add(new List<Material> { specialMat });
                }
            }
            else
            {
                BMLogger.Warning($"SpecialIcons value is type of {value.GetType()} it must be a String, String Array, or Array of String Arrays");
            }
        }

        [Parameter]
        public static void SpecialIconOffset(object obj, object value)
        {
            ICustomHero hero = obj as ICustomHero;
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
                    if (hero.info.SpecialMaterialOffset.Count > 1 || 
                        (hero.info.SpecialMaterialOffset.Count == 1 && hero.info.SpecialMaterialOffset[0] != Vector2.zero))
                        return;
                        
                    hero.info.SpecialMaterialOffset.Clear();
                    foreach (var item in array)
                    {
                        if (item is JObject jObj)
                        {
                            float x = Convert.ToSingle(jObj.GetValue("x").ToObject<object>());
                            float y = Convert.ToSingle(jObj.GetValue("y").ToObject<object>());
                            hero.info.SpecialMaterialOffset.Add(new Vector2(x, y));
                        }
                    }
                }
                // Handle single offset (backwards compatibility)
                else if (value is JObject)
                {
                    // Only process if not already set as array
                    if (hero.info.SpecialMaterialOffset.Count <= 1)
                    {
                        JToken xToken = value.As<JObject>().GetValue("x");
                        JToken yToken = value.As<JObject>().GetValue("y");

                        float x = Convert.ToSingle(xToken.ToObject<object>());
                        float y = Convert.ToSingle(yToken.ToObject<object>());
                        
                        if (hero.info.SpecialMaterialOffset.Count == 0)
                            hero.info.SpecialMaterialOffset.Add(new Vector2(x, y));
                        else if (hero.info.SpecialMaterialOffset[0] == Vector2.zero)
                            hero.info.SpecialMaterialOffset[0] = new Vector2(x, y);
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
        public static void SpecialIconSpacing(object obj, object value)
        {
            ICustomHero hero = obj as ICustomHero;
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
                    if (hero.info.SpecialMaterialSpacing.Count > 1 || 
                        (hero.info.SpecialMaterialSpacing.Count == 1 && hero.info.SpecialMaterialSpacing[0] != 0f))
                        return;
                        
                    hero.info.SpecialMaterialSpacing.Clear();
                    foreach (var item in array)
                    {
                        hero.info.SpecialMaterialSpacing.Add(Convert.ToSingle(item));
                    }
                }
                // Handle single spacing (backwards compatibility)
                else
                {
                    // Only process if not already set as array
                    if (hero.info.SpecialMaterialSpacing.Count <= 1)
                    {
                        if (hero.info.SpecialMaterialSpacing.Count == 0)
                            hero.info.SpecialMaterialSpacing.Add(Convert.ToSingle(value));
                        else if (hero.info.SpecialMaterialSpacing[0] == 0f)
                            hero.info.SpecialMaterialSpacing[0] = Convert.ToSingle(value);
                    }
                }
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
            }
        }

        [Parameter]
        public static void Avatar(object obj, object value)
        {
            ICustomHero hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(Avatar)} parameter works only with Bros.");
                return;
            }

            // Handle array of avatars
            if (value is JArray array)
            {
                // Prevent multiple calls from overwriting data
                if (hero.info.FirstAvatar.Count > 1 || 
                    (hero.info.FirstAvatar.Count == 1 && hero.info.FirstAvatar[0] != null))
                    return;
                    
                hero.info.FirstAvatar.Clear();
                foreach (var item in array)
                {
                    string avatarPath = item.ToObject<string>();
                    hero.info.FirstAvatar.Add(ResourcesController.GetMaterial(hero.info.path, avatarPath));
                }
            }
            else if (value is string[] stringArray)
            {
                // Prevent multiple calls from overwriting data
                if (hero.info.FirstAvatar.Count > 1 || 
                    (hero.info.FirstAvatar.Count == 1 && hero.info.FirstAvatar[0] != null))
                    return;
                    
                hero.info.FirstAvatar.Clear();
                foreach (var avatarPath in stringArray)
                {
                    hero.info.FirstAvatar.Add(ResourcesController.GetMaterial(hero.info.path, avatarPath));
                }
            }
            // Handle single avatar (backwards compatibility)
            else if (value is string)
            {
                // Only process if not already set as array
                if (hero.info.FirstAvatar.Count <= 1)
                {
                    if (hero.info.FirstAvatar.Count == 0)
                        hero.info.FirstAvatar.Add(ResourcesController.GetMaterial(hero.info.path, value as string));
                    else if (hero.info.FirstAvatar[0] == null)
                        hero.info.FirstAvatar[0] = ResourcesController.GetMaterial(hero.info.path, value as string);
                }
            }
        }

        [Parameter]
        public static void GunSpriteOffset(object obj, object value)
        {
            ICustomHero hero = obj as ICustomHero;
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
                    if (hero.info.GunSpriteOffset.Count > 1 || 
                        (hero.info.GunSpriteOffset.Count == 1 && hero.info.GunSpriteOffset[0] != Vector2.zero))
                        return;
                        
                    hero.info.GunSpriteOffset.Clear();
                    foreach (var item in array)
                    {
                        if (item is JObject jObj)
                        {
                            float x = Convert.ToSingle(jObj.GetValue("x").ToObject<object>());
                            float y = Convert.ToSingle(jObj.GetValue("y").ToObject<object>());
                            hero.info.GunSpriteOffset.Add(new Vector2(x, y));
                        }
                    }
                }
                // Handle single offset (backwards compatibility)
                else if (value is JObject)
                {
                    // Only process if not already set as array
                    if (hero.info.GunSpriteOffset.Count <= 1)
                    {
                        JToken xToken = value.As<JObject>().GetValue("x");
                        JToken yToken = value.As<JObject>().GetValue("y");

                        float x = Convert.ToSingle(xToken.ToObject<object>());
                        float y = Convert.ToSingle(yToken.ToObject<object>());
                        
                        if (hero.info.GunSpriteOffset.Count == 0)
                            hero.info.GunSpriteOffset.Add(new Vector2(x, y));
                        else if (hero.info.GunSpriteOffset[0] == Vector2.zero)
                            hero.info.GunSpriteOffset[0] = new Vector2(x, y);
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
        public static void Sprite(object obj, object value)
        {
            ICustomHero hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(Sprite)} parameter works only with Bros.");
                return;
            }

            if ( hero.info.SpritePath.Count != 0 )
            {
                return;
            }

            // Handle array of sprites
            if (value is JArray array)
            {
                hero.info.SpritePath.Clear();
                foreach (var item in array)
                {
                    hero.info.SpritePath.Add(item.ToObject<string>());
                }
            }
            else if (value is string[] stringArray)
            {
                hero.info.SpritePath.Clear();
                hero.info.SpritePath.AddRange(stringArray);
            }
            // Handle single sprite (backwards compatibility)
            else if (value is string)
            {
                if (hero.info.SpritePath.Count == 0)
                {
                    hero.info.SpritePath.Add(value as string);
                }
                else if (hero.info.SpritePath.Count == 1)
                {
                    hero.info.SpritePath[0] = value as string;
                }
                // If count > 1, it's already been set as an array, don't override
            }
        }

        [Parameter]
        public static void GunSprite(object obj, object value)
        {
            ICustomHero hero = obj as ICustomHero;
            if (hero == null)
            {
                BMLogger.Warning($"{nameof(GunSprite)} parameter works only with Bros.");
                return;
            }

            if ( hero.info.GunSpritePath.Count != 0 )
            {
                return;
            }

            // Handle array of gun sprites
            if (value is JArray array)
            {
                hero.info.GunSpritePath.Clear();
                foreach (var item in array)
                {
                    hero.info.GunSpritePath.Add(item.ToObject<string>());
                }
            }
            else if (value is string[] stringArray)
            {
                hero.info.GunSpritePath.Clear();
                hero.info.GunSpritePath.AddRange(stringArray);
            }
            // Handle single gun sprite
            else if (value is string)
            {
                if (hero.info.GunSpritePath.Count == 0)
                {
                    hero.info.GunSpritePath.Add(value as string);
                }
                else if (hero.info.GunSpritePath.Count == 1)
                {
                    hero.info.GunSpritePath[0] = value as string;
                }
            }
        }
    }
}

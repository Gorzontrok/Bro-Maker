using System;
using System.Collections.Generic;
using System.Reflection;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using BroMakerLib.Triggers;
using BroMakerLib.Unlocks;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BroMakerLib
{
    public static class BroMaker
    {
        internal static Harmony harmony;

        private static bool _hasInit;

        public static void Initialize()
        {
            if (!_hasInit)
            {
                BMLogger.Log("Initialization of BroMaker");

                harmony = new Harmony("BroMakerLib");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Settings.Load();
                DirectoriesManager.Initialize();

                BroMakerStorage.Initialize();
                PresetManager.Initialize();

                // Apply all harmony patches if any bros have overridden the method
                if (harmony != null)
                {
                    BroMaker.ApplyBroPatches(harmony);
                }

                RocketLib.CustomTriggers.CustomTriggerManager.RegisterCustomTrigger(typeof(ForceBroAction), typeof(ForceBroActionInfo), "BroMaker - Force Custom Bro", "Custom Bros", 10);
                RocketLib.CustomTriggers.CustomTriggerManager.RegisterCustomTrigger(typeof(HeroUnlockCutsceneAction), typeof(HeroUnlockCutsceneActionInfo), "BroMaker - Play Custom Bro Cutscene", "Custom Bros", 9);

                _hasInit = true;

                // Initialize Unlock Manager
                BroUnlockManager.Initialize();

                // Preload all bro assets
                PreloadBroAssets();

                BroSpawnManager.CheckForDeletedBros();

                // Register Custom Bros menu with MainMenu
                try
                {
                    RocketLib.Menus.Core.MenuRegistry.RegisterAction(
                        displayText: "CUSTOM BROS",
                        onSelect: (menu) => BroMakerLib.Menus.CustomBrosGridMenu.Show(menu),
                        targetMenu: RocketLib.Menus.Core.TargetMenu.MainMenu,
                        position: RocketLib.Menus.Core.PositionMode.After,
                        positionReference: "START"
                    );
                }
                catch (Exception ex)
                {
                    Main.Log("Error while registering Custom Bros menu.\n" + ex);
                }

                BMLogger.Log("Finish Initialization");
            }
        }

        public static void ApplyBroPatches(Harmony harmony)
        {
            GameObject heroHolder = new GameObject();
            heroHolder.SetActive(false);

            foreach (KeyValuePair<string, Type> kvp in PresetManager.heroesPreset)
            {
                if (typeof(CustomHero).IsAssignableFrom(kvp.Value))
                {
                    CustomHero bro = heroHolder.AddComponent(kvp.Value) as CustomHero;
                    bro.HarmonyPatches(harmony);
                    bro.RegisterCustomTriggers();
                }
            }
        }

        public static void PreloadBroAssets()
        {
            // Preload all assets listed in the JSON file
            for (int i = 0; i < BroMakerStorage.Bros.Count; ++i)
            {
                CustomBroInfo info = BroMakerStorage.Bros[i].GetInfo();
                List<string> spritePaths = new List<string>();
                List<string> soundPaths = new List<string>();

                // Handle Sprite parameter
                if (info.parameters.ContainsKey("Sprite"))
                {
                    if (info.parameters["Sprite"] is string sprite)
                    {
                        spritePaths.Add(sprite);
                    }
                    else if (info.parameters["Sprite"] is JArray spriteArray)
                    {
                        foreach (var item in spriteArray)
                        {
                            string spritePath = item.ToObject<string>();
                            if (!string.IsNullOrEmpty(spritePath))
                                spritePaths.Add(spritePath);
                        }
                    }
                }

                // Handle GunSprite parameter
                if (info.parameters.ContainsKey("GunSprite"))
                {
                    if (info.parameters["GunSprite"] is string gunSprite)
                    {
                        spritePaths.Add(gunSprite);
                    }
                    else if (info.parameters["GunSprite"] is JArray gunSpriteArray)
                    {
                        foreach (var item in gunSpriteArray)
                        {
                            string gunSpritePath = item.ToObject<string>();
                            if (!string.IsNullOrEmpty(gunSpritePath))
                                spritePaths.Add(gunSpritePath);
                        }
                    }
                }

                if (info.parameters.ContainsKey("SpecialIcons"))
                {
                    if (info.parameters["SpecialIcons"] is string path)
                    {
                        spritePaths.Add(path);
                    }
                    else if (info.parameters["SpecialIcons"] is JArray paths)
                    {
                        foreach (var item in paths)
                        {
                            if (item is JArray variantIcons)
                            {
                                // Array of arrays - each variant has multiple icons
                                foreach (var icon in variantIcons)
                                {
                                    string iconPath = icon.ToObject<string>();
                                    if (!string.IsNullOrEmpty(iconPath))
                                        spritePaths.Add(iconPath);
                                }
                            }
                            else
                            {
                                // Simple array - one icon per variant
                                string iconPath = item.ToObject<string>();
                                if (!string.IsNullOrEmpty(iconPath))
                                    spritePaths.Add(iconPath);
                            }
                        }
                    }
                }
                if (info.parameters.ContainsKey("Avatar"))
                {
                    if (info.parameters["Avatar"] is string avatar)
                    {
                        spritePaths.Add(avatar);
                    }
                    else if (info.parameters["Avatar"] is JArray avatarArray)
                    {
                        foreach (var item in avatarArray)
                        {
                            string avatarPath = item.ToObject<string>();
                            if (!string.IsNullOrEmpty(avatarPath))
                                spritePaths.Add(avatarPath);
                        }
                    }
                }

                // Handle SpecialMaterials parameter
                if (info.parameters.ContainsKey("SpecialMaterials"))
                {
                    if (info.parameters["SpecialMaterials"] is JArray materialsArray)
                    {
                        foreach (var item in materialsArray)
                        {
                            if (item is JArray variantMaterials)
                            {
                                foreach (var material in variantMaterials)
                                {
                                    string materialPath = material.ToObject<string>();
                                    if (!string.IsNullOrEmpty(materialPath))
                                        spritePaths.Add(materialPath);
                                }
                            }
                            else
                            {
                                string materialPath = item.ToObject<string>();
                                if (!string.IsNullOrEmpty(materialPath))
                                    spritePaths.Add(materialPath);
                            }
                        }
                    }
                }

                // Handle FirstAvatar parameter
                if (info.parameters.ContainsKey("FirstAvatar"))
                {
                    if (info.parameters["FirstAvatar"] is string firstAvatar)
                    {
                        spritePaths.Add(firstAvatar);
                    }
                    else if (info.parameters["FirstAvatar"] is JArray firstAvatarArray)
                    {
                        foreach (var item in firstAvatarArray)
                        {
                            string firstAvatarPath = item.ToObject<string>();
                            if (!string.IsNullOrEmpty(firstAvatarPath))
                                spritePaths.Add(firstAvatarPath);
                        }
                    }
                }

                foreach (var cutscene in info.Cutscene)
                {
                    if (cutscene.spritePath != string.Empty)
                    {
                        spritePaths.Add(cutscene.spritePath);
                    }
                    if (cutscene.barkPath != string.Empty)
                    {
                        soundPaths.Add(cutscene.barkPath);
                    }
                    if (cutscene.fanfarePath != string.Empty)
                    {
                        soundPaths.Add(cutscene.fanfarePath);
                    }
                }

                CustomHero.PreloadSprites(info.path, spritePaths);
                CustomHero.PreloadSounds(info.path, soundPaths);
            }

            // Allow bro developers to preload assets
            GameObject heroHolder = new GameObject();
            heroHolder.SetActive(false);
            foreach (KeyValuePair<string, Type> kvp in PresetManager.heroesPreset)
            {
                if (typeof(CustomHero).IsAssignableFrom(kvp.Value))
                {
                    try
                    {
                        CustomHero bro = heroHolder.AddComponent(kvp.Value) as CustomHero;
                        try
                        {
                            if (BroMakerStorage.GetStoredHeroByCustomHeroType(kvp.Value, out StoredHero storedHero))
                            {
                                bro.AssignDirectoryPaths(storedHero.GetInfo().path);
                                bro.PreloadAssets();
                            }
                        }
                        catch { }
                    }
                    catch (Exception ex)
                    {
                        BMLogger.ExceptionLog($"Exception occurred while preloading {kvp.Key}'s assets:", ex);
                    }
                }
            }
        }
    }
}

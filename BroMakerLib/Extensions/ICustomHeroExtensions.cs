using System;
using BroMakerLib.CustomObjects;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using HarmonyLib;
using Networking;
using RocketLib;
using UnityEngine;

namespace BroMakerLib.Extensions
{
    public static class ICustomHeroExtensions
    {
        private static readonly AccessTools.FieldRef<SpriteBase, Texture> spriteTexture = AccessTools.FieldRefAccess<SpriteBase, Texture>("texture");
        private static readonly AccessTools.FieldRef<SpriteBase, Mesh> spriteMesh = AccessTools.FieldRefAccess<SpriteBase, Mesh>("mesh");

        /// <summary>
        /// Before base.Awake()
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="character"></param>
        [AllowedRPC]
        public static void AssignNullVariables(this ICustomHero hero, BroBase character)
        {
            if (hero.Character == null)
            {
                throw new NullReferenceException("AssignNullVariables: ICustomHero.character is null");
            }

            BroBase heroCharacter = hero.Character;
            BMLogger.Debug($"AssignNullVariables: {heroCharacter.GetType()}: Start Assigning null variables from {character.GetType()}.");

            // Global
            heroCharacter.SetFieldValue("sprite", heroCharacter.Sprite());
            heroCharacter.gunSprite = character.gunSprite;
            heroCharacter.soundHolder = heroCharacter.soundHolder ?? character.soundHolder;
            heroCharacter.soundHolderFootSteps = heroCharacter.soundHolderFootSteps ?? character.soundHolderFootSteps;
            heroCharacter.soundHolderVoice = heroCharacter.soundHolderVoice ?? character.soundHolderVoice;
            heroCharacter.gibs = character.gibs;
            heroCharacter.player1Bubble = character.player1Bubble;
            heroCharacter.player2Bubble = character.player2Bubble;
            heroCharacter.player3Bubble = character.player3Bubble;
            heroCharacter.player4Bubble = character.player4Bubble;
            heroCharacter.blood = character.blood;
            heroCharacter.heroTrailPrefab = character.heroTrailPrefab;
            if (heroCharacter.faderSpritePrefab == null)
            {
                heroCharacter.faderSpritePrefab = character.faderSpritePrefab;
            }
            heroCharacter.high5Bubble = character.high5Bubble;
            heroCharacter.projectile = character.projectile;
            heroCharacter.jetPackSprite = character.jetPackSprite;
            if (heroCharacter.specialGrenade == null)
            {
                heroCharacter.specialGrenade = LoadBroforceObjects.GetGrenadeFromName(string.Empty);
            }
            heroCharacter.specialGrenade.playerNum = heroCharacter.playerNum;
            heroCharacter.heroType = character.heroType;
            heroCharacter.canUnFreeze = character.canUnFreeze;
            heroCharacter.SetFieldValue("wallDragAudio", character.GetFieldValue<AudioSource>("wallDragAudio"));
            heroCharacter.parachute = heroCharacter.GetComponentInChildren<Parachute>();
            heroCharacter.SetOwner(character.Owner);

            BMLogger.Debug($"AssignNullVariables: {heroCharacter.GetType()}: Has finish assigning null variables.");
        }

        /// <summary>
        /// Before Awake
        /// </summary>
        [AllowedRPC]
        public static void FixOtherComponentValues(this ICustomHero hero)
        {
            if (hero.Character == null)
            {
                throw new NullReferenceException("FixOtherComponentValues: ICustomHero.character is null");
            }

            BroBase character = hero.Character;
            if (character.parachute != null)
            {
                character.parachute.tvd = character;
                BMLogger.Debug("FixOtherComponentValues: Has set 'tvd' of 'Parachute' component.");
            }
            else
            {
                BMLogger.Debug("FixOtherComponentValues: Parachute Not Found");
            }
            character.RemoveComponent<WavyGrassEffector>();
            BMLogger.Debug($"FixOtherComponentValues: Removed 'WavyGrassEffector' component.");
            character.RemoveComponent<PathAgent>();
            BMLogger.Debug("FixOtherComponentValues: Removed 'PathAgent' component.");
        }

        /// <summary>
        /// Before Awake
        /// </summary>
        public static BroBase GetTheOtherBroBaseComponent(this ICustomHero hero)
        {
            BroBase character = hero.Character;
            BroBase result = null;

            BroBase[] bros = character.GetComponents<BroBase>();
            BMLogger.Debug($"GetTheOtherBroBaseComponent: {character.GetType()}: Has got BroBase components.");
            if (bros.Length > 2)
            {
                throw new Exception($"GetTheOtherBroBaseComponent: {bros.Length} components found: {bros.ValuesAsString()}");
            }
            foreach (BroBase c in bros)
            {
                BMLogger.Debug($"GetTheOtherBroBaseComponent: Find component of {c.GetType()}.");
                if (c != character)
                {
                    result = c;
                }
            }
            BMLogger.Debug($"GetTheOtherBroBaseComponent: {character.GetType()}: Has loop throught the Brobase components.");
            return result;
        }

        /// <summary>
        /// Before Awake
        /// </summary>
        /// <param name="hero"></param>
        /// <exception cref="NullReferenceException"></exception>
        [AllowedRPC]
        public static void SetupCustomHero(this ICustomHero hero)
        {
            BroBase otherBroComponent = hero.GetTheOtherBroBaseComponent();
            if (otherBroComponent != null)
            {
                hero.AssignNullVariables(otherBroComponent);
                Type type = otherBroComponent.GetType();
                UnityEngine.Object.DestroyImmediate(otherBroComponent);
                BMLogger.Debug($"SetupCustomHero: Has destroy {type} component.");
                hero.Info.ReadParameters(hero.Character);
                BMLogger.Debug("SetupCustomHero: Has read parameters");
            }
            else
            {
                BMLogger.Debug("SetupCustomHero: No other bro component founded");
            }

            hero.FixOtherComponentValues();
            hero.RemoveDuplicateGlowLight();

            hero.Character.maxHealth = 1;
            CustomBroInfo info = hero.Info;
            if (!info.beforeAwake.ContainsKey("specialGrenade.playerNum"))
            {
                info.beforeAwake.Add("specialGrenade.playerNum", LoadHero.playerNum);
            }
            if (!info.beforeAwake.ContainsKey("health"))
            {
                info.beforeAwake.Add("health", 1);
            }

            if (Settings.instance.maxHealthAtOne)
            {
                info.afterStart["health"] = 1;

                info.afterStart["maxHealth"] = 1;
            }

            // Determine variant count from all variant properties
            DetermineVariantCount(hero.Info);

            // Validate all variant lists
            ValidateVariantLists(hero.Info);

            // Set canCeilingHang to true by default 
            if (!info.beforeAwake.ContainsKey("canCeilingHang"))
            {
                info.beforeAwake.Add("canCeilingHang", true);
            }

            // Setup vanilla bro
            if (!(hero is CustomHero))
            {
                // For vanilla bros, use random selection
                hero.CurrentVariant = UnityEngine.Random.Range(0, hero.Info.VariantCount);

                // Cache current variant parameters
                hero.CurrentGunSpriteOffset = BroMakerUtilities.GetVariantValue(hero.Info.GunSpriteOffset, hero.CurrentVariant);
                hero.CurrentSpecialMaterials = BroMakerUtilities.GetVariantValue(hero.Info.SpecialMaterials, hero.CurrentVariant);
                hero.CurrentSpecialMaterialOffset = BroMakerUtilities.GetVariantValue(hero.Info.SpecialMaterialOffset, hero.CurrentVariant);
                hero.CurrentSpecialMaterialSpacing = BroMakerUtilities.GetVariantValue(hero.Info.SpecialMaterialSpacing, hero.CurrentVariant);
                hero.CurrentFirstAvatar = BroMakerUtilities.GetVariantValue(hero.Info.FirstAvatar, hero.CurrentVariant);

                hero.Character.specialGrenade.playerNum = LoadHero.playerNum;
            }
        }

        [AllowedRPC]
        public static void RemoveDuplicateGlowLight(this ICustomHero hero)
        {
            Transform transform = hero.Character.transform;
            int childCount = transform.childCount;
            int glowLightFounded = 0;
            GameObject lastGlowLight = null;

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (child.gameObject.name.Contains("Glow Light"))
                {
                    glowLightFounded++;
                    lastGlowLight = child.gameObject;
                }
            }
            if (glowLightFounded > 1 && lastGlowLight != null)
            {
                UnityEngine.Object.Destroy(lastGlowLight);
                BMLogger.Debug("GlowLight Destroyed");
            }
        }

        private static void DetermineVariantCount(CustomBroInfo info)
        {
            int maxCount = 1;

            // Check all variant property lists
            if (info.SpritePath?.Count > maxCount)
            {
                maxCount = info.SpritePath.Count;
            }
            if (info.GunSpritePath?.Count > maxCount)
            {
                maxCount = info.GunSpritePath.Count;
            }
            if (info.GunSpriteOffset?.Count > maxCount)
            {
                maxCount = info.GunSpriteOffset.Count;
            }
            if (info.SpecialMaterials?.Count > maxCount)
            {
                maxCount = info.SpecialMaterials.Count;
            }
            if (info.SpecialMaterialOffset?.Count > maxCount)
            {
                maxCount = info.SpecialMaterialOffset.Count;
            }
            if (info.SpecialMaterialSpacing?.Count > maxCount)
            {
                maxCount = info.SpecialMaterialSpacing.Count;
            }
            if (info.FirstAvatar?.Count > maxCount)
            {
                maxCount = info.FirstAvatar.Count;
            }
            if (info.Cutscene?.Count > maxCount)
            {
                maxCount = info.Cutscene.Count;
            }

            info.VariantCount = maxCount;
        }

        private static void ValidateVariantLists(CustomBroInfo info)
        {
            // Each list must have either 1 item or exactly VariantCount items
            ValidateList(info.SpritePath, "Sprite", info.VariantCount);
            ValidateList(info.GunSpritePath, "GunSprite", info.VariantCount);
            ValidateList(info.GunSpriteOffset, "GunSpriteOffset", info.VariantCount);
            ValidateList(info.SpecialMaterials, "SpecialIcons", info.VariantCount);
            ValidateList(info.SpecialMaterialOffset, "SpecialIconOffset", info.VariantCount);
            ValidateList(info.SpecialMaterialSpacing, "SpecialIconSpacing", info.VariantCount);
            ValidateList(info.FirstAvatar, "Avatar", info.VariantCount);
            ValidateList(info.Cutscene, "cutscene", info.VariantCount);
        }

        private static void ValidateList<T>(System.Collections.Generic.List<T> list, string jsonKey, int expectedCount)
        {
            if (list != null && list.Count > 1 && list.Count != expectedCount)
            {
                throw new InvalidOperationException(
                    $"{jsonKey} lists {list.Count} variants but another setting lists {expectedCount}. " +
                    $"Every setting with variants needs either 1 entry, shared by every variant, or exactly {expectedCount}, one per variant.");
            }
        }

        /// <summary>
        /// Loads and sets the sprite and gunsprite material for the given bro.
        /// </summary>
        /// <param name="bro">Bro to load sprites for</param>
        public static void SetSprites(this ICustomHero bro)
        {
            // Get sprite paths for current variant
            string spritePath = BroMakerUtilities.GetVariantValue(bro.Info.SpritePath, bro.CurrentVariant);
            string gunSpritePath = BroMakerUtilities.GetVariantValue(bro.Info.GunSpritePath, bro.CurrentVariant);

            BroBase character = bro.Character;

            // Set main sprite
            if (!string.IsNullOrEmpty(spritePath))
            {
                Material material = ResourcesController.GetMaterial(bro.Info.path, spritePath);
                character.GetComponent<SpriteSM>().SetMaterialAndRefresh(material);
                character.SetFieldValue("defaultMaterial", material);
            }

            // Set gun sprite
            if (!string.IsNullOrEmpty(gunSpritePath))
            {
                Material gunMaterial = ResourcesController.GetMaterial(bro.Info.path, gunSpritePath);
                character.gunSprite.SetMaterialAndRefresh(gunMaterial);
                character.SetFieldValue("gunMaterial", gunMaterial);
            }

            if (character is CustomHero customHero)
            {
                customHero.SetupAdditionalSprites();
            }
        }

        /// <summary>
        /// Assigns a material to the sprite's renderer and refreshes the texture size it maps pixel coordinates against.
        /// Not for HUD special icons, whose fixed 16x16 rect it would crop.
        /// </summary>
        public static void SetMaterialAndRefresh(this SpriteSM sprite, Material material)
        {
            if (sprite == null)
            {
                return;
            }

            Renderer renderer = sprite.meshRender != null ? sprite.meshRender : sprite.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            sprite.RefreshTexture();
        }

        /// <summary>
        /// Re-reads the sprite's texture size from its current material and re-pushes its UVs, skipping the UV recompute when the size is unchanged.
        /// Call after changing the texture of a material the sprite already uses.
        /// </summary>
        public static void RefreshTexture(this SpriteSM sprite)
        {
            if (sprite == null)
            {
                return;
            }

            Texture previousTexture = spriteTexture(sprite);
            sprite.RecalcTexture();
            Texture currentTexture = spriteTexture(sprite);
            if (previousTexture != null && currentTexture != null && previousTexture.width == currentTexture.width && previousTexture.height == currentTexture.height)
            {
                return;
            }

            // Before the sprite awakes there is no mesh to push UVs into, and its own Awake rebuilds them from the new material.
            if (spriteMesh(sprite) == null)
            {
                return;
            }

            sprite.CalcUVs();
            sprite.SetBleedCompensation();
        }

        /// <summary>
        /// Called before base.Awake() for vanilla bros
        /// </summary>
        /// <param name="hero">The hero instance to setup</param>
        /// <param name="fixNullVariableLocal">Optional action to fix null variables specific to the hero type</param>
        public static void StandardBeforeAwake(this ICustomHero hero, Action fixNullVariableLocal = null)
        {
            hero.Character = hero as BroBase;
            hero.Info = LoadHero.currentInfo;
            fixNullVariableLocal?.Invoke();
            hero.SetupCustomHero();
            hero.Info.BeforeAwake(hero);
        }

        /// <summary>
        /// Called after base.Awake() for vanilla bros
        /// </summary>
        /// <param name="hero">The hero instance to cleanup</param>
        public static void StandardAfterAwake(this ICustomHero hero)
        {
            hero.SetSprites();
            hero.Info.AfterAwake(hero);
        }

        /// <summary>
        /// Called before base.Start() for vanilla bros
        /// </summary>
        /// <param name="hero">The hero instance to setup</param>
        public static void StandardBeforeStart(this ICustomHero hero)
        {
            hero.Info.BeforeStart(hero);
        }

        /// <summary>
        /// Called after base.Start() for vanilla bros
        /// </summary>
        /// <param name="hero">The hero instance to cleanup</param>
        public static void StandardAfterStart(this ICustomHero hero)
        {
            hero.Info.AfterStart(hero);
        }

        /// <summary>Log a message prefixed with the bro's name and mod.</summary>
        public static void Log(this ICustomHero hero, string message)
        {
            BMLogger.Log(FormatHeroPrefix(hero) + " " + message);
        }

        /// <summary>Logs a debug message prefixed with the bro's name and mod, shown only when BroMaker's debug logs setting is on.</summary>
        public static void LogDebug(this ICustomHero hero, string message)
        {
            if (!BMLogger.DebugEnabled)
            {
                return;
            }
            BMLogger.Debug(FormatHeroPrefix(hero) + " " + message);
        }

        /// <summary>Log a warning that is shown in the Issues panel and as a notification.</summary>
        public static void LogWarning(this ICustomHero hero, string message, Exception ex = null)
        {
            BMLogger.Warning(message, ex, HeroModName(hero), HeroBroName(hero));
        }

        /// <summary>Log an error that is shown in the Issues panel and as a notification.</summary>
        public static void LogError(this ICustomHero hero, string message, Exception ex = null)
        {
            BMLogger.Error(message, ex, HeroModName(hero), HeroBroName(hero));
        }

        private static string HeroModName(ICustomHero hero)
        {
            return hero != null && hero.Info != null ? hero.Info.Mod?.Name : null;
        }

        private static string HeroBroName(ICustomHero hero)
        {
            return hero != null && hero.Info != null ? hero.Info.name : null;
        }

        private static string FormatHeroPrefix(ICustomHero hero)
        {
            return "[" + BMLogger.FormatSource(HeroModName(hero), HeroBroName(hero)) + "]";
        }
    }
}

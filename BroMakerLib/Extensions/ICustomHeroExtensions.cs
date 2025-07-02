using BroMakerLib.CustomObjects;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using RocketLib;
using System;
using System.IO;
using UnityEngine;
using Networking;
using Net = Networking.Networking;

namespace BroMakerLib
{
    public static class ICustomHeroExtensions
    {
        /// <summary>
        /// Before base.Awake()
        /// </summary>
        /// <param name="hero"></param>
        /// <param name="character"></param>
        [AllowedRPC]
        public static void AssignNullVariables(this ICustomHero hero, BroBase character)
        {
            if (hero.character == null)
                throw new NullReferenceException("AssignNullVariables: ICustomHero.character is null");

            var heroCharacter = hero.character;
            BMLogger.Debug($"AssignNullVariables: {heroCharacter.GetType()}: Start Assigning null variables from {character.GetType()}.");

            // Global
            heroCharacter.SetFieldValue("sprite", heroCharacter.Sprite());
            heroCharacter.gunSprite = character.gunSprite;
            heroCharacter.soundHolder = heroCharacter.soundHolder ?? character.soundHolder;
            heroCharacter.soundHolderFootSteps = heroCharacter.soundHolderFootSteps ?? character.soundHolderFootSteps;
            heroCharacter.soundHolderVoice = heroCharacter.soundHolderVoice ??character.soundHolderVoice;
            heroCharacter.parachute = character.parachute;
            heroCharacter.gibs = character.gibs;
            heroCharacter.player1Bubble = character.player1Bubble;
            heroCharacter.player2Bubble = character.player2Bubble;
            heroCharacter.player3Bubble = character.player3Bubble;
            heroCharacter.player4Bubble = character.player4Bubble;
            heroCharacter.blood = character.blood;
            heroCharacter.heroTrailPrefab = character.heroTrailPrefab;
            heroCharacter.high5Bubble = character.high5Bubble;
            heroCharacter.projectile = character.projectile;
            heroCharacter.specialGrenade = LoadBroforceObjects.GetGrenadeFromName(string.Empty);
            heroCharacter.specialGrenade.playerNum = heroCharacter.playerNum;
            heroCharacter.heroType = character.heroType;
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
            if (hero.character == null)
                throw new NullReferenceException("FixOtherComponentValues: ICustomHero.character is null");

            var character = hero.character;
            if (character.parachute != null)
            {
                character.parachute.tvd = character;
                BMLogger.Debug("FixOtherComponentValues: Has set 'tvd' of 'Parachute' component.");
            }
            else
                BMLogger.Debug("FixOtherComponentValues: Parachute Not Found");
            character.RemoveComponent<WavyGrassEffector>();
            BMLogger.Debug($"FixOtherComponentValues: Removed 'WavyGrassEffector' component.");
            character.gameObject.RemoveComponent<InvulnerabilityFlash>();
            character.RemoveComponent<PathAgent>();
            BMLogger.Debug("FixOtherComponentValues: Removed 'PathAgent' component.");
        }

        /// <summary>
        /// Before Awake
        /// </summary>
        public static BroBase GetTheOtherBroBaseComponent(this ICustomHero hero)
        {
            var character = hero.character;
            BroBase result = null;

            BroBase[] bros = character.GetComponents<BroBase>();
            BMLogger.Debug($"GetTheOtherBroBaseComponent: {character.GetType()}: Has got BroBase components.");
            if (bros.Length > 2)
                throw new Exception($"GetTheOtherBroBaseComponent: {bros.Length} components found: {bros.ValuesAsString()}");
            foreach (var c in bros)
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
            var otherBroComponent = hero.GetTheOtherBroBaseComponent();
            if (otherBroComponent != null)
            {
                hero.AssignNullVariables(otherBroComponent);
                //Net.RPC(PID.TargetAll, new RpcSignature<BroBase>(hero.AssignNullVariables), otherBroComponent);
                var type = otherBroComponent.GetType();
                UnityEngine.Object.Destroy(otherBroComponent);
                //Net.RPC(PID.TargetAll, new RpcSignature<UnityEngine.Object>(UnityEngine.Object.Destroy), otherBroComponent);
                BMLogger.Debug($"SetupCustomHero: Has destroy {type} component.");
                hero.info.ReadParameters(hero.character);
                //Net.RPC(PID.TargetAll, new RpcSignature<object>(hero.info.ReadParameters), hero.character);
                BMLogger.Debug("SetupCustomHero: Has read parameters");
            }
            else
                BMLogger.Debug("SetupCustomHero: No other bro component founded");

            hero.FixOtherComponentValues();
            //Net.RPC(PID.TargetAll, new RpcSignature(hero.FixOtherComponentValues));
            hero.RemoveDuplicateGlowLight();
            //Net.RPC(PID.TargetAll, new RpcSignature(hero.RemoveDuplicateGlowLight));

            EffectsController.CreateHeroIndicator(hero.character);
            hero.character.maxHealth = 1;
            var info = hero.info;
            if (!info.beforeAwake.ContainsKey("specialGrenade.playerNum"))
                info.beforeAwake.Add("specialGrenade.playerNum", LoadHero.playerNum);
            if(!info.beforeAwake.ContainsKey("health"))
                info.beforeAwake.Add("health", 1);

            if(Settings.instance.maxHealthAtOne)
            {
                if (info.afterStart.ContainsKey("health"))
                    info.afterStart["health"] = 1;
                else
                    info.afterStart.Add("health", 1);

                if (info.afterStart.ContainsKey("maxHealth"))
                    info.afterStart["maxHealth"] = 1;
                else
                    info.afterStart.Add("maxHealth", 1);
            }

            // Migrate legacy sprite/gunSprite properties
            MigrateLegacySprites(hero);
            
            // Determine variant count from all variant properties
            DetermineVariantCount(hero.info);
            
            // Validate all variant lists
            ValidateVariantLists(hero.info);
            
            // Select variant
            if (hero is CustomObjects.Bros.CustomHero customHero)
            {
                customHero.CurrentVariant = customHero.GetVariant();
                // Set the cached gun sprite offset
                customHero.CurrentGunSpriteOffset = BroMakerUtilities.GetVariantValue(hero.info.GunSpriteOffset, customHero.CurrentVariant);
            }
            else
            {
                // For vanilla bros, use random selection
                hero.CurrentVariant = UnityEngine.Random.Range(0, hero.info.VariantCount);
            }

            // Set canCeilingHang to true by default 
            if ( !info.beforeAwake.ContainsKey("canCeilingHang") )
            {
                info.beforeAwake.Add("canCeilingHang", true);
            }

            hero.character.specialGrenade.playerNum = LoadHero.playerNum;
        }

        [AllowedRPC]
        public static void RemoveDuplicateGlowLight(this ICustomHero hero)
        {
            var transform = hero.character.transform;
            int childCount = transform.childCount;
            int glowLightFounded = 0;
            GameObject lastGlowLight = null;

            for (int i = 0; i < childCount; i++)
            {
                var child = transform.GetChild(i);
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
        
        private static void MigrateLegacySprites(ICustomHero hero)
        {
            var info = hero.info;
            // Check beforeAwake, afterAwake, beforeStart, afterStart for sprite/gunSprite
            if (info.beforeStart.ContainsKey("sprite"))
            {
                if (info.SpritePath.Count == 0)
                    info.SpritePath.Add(info.beforeStart["sprite"] as string);
                info.beforeStart.Remove("sprite");
                
                if (info.beforeStart.ContainsKey("gunSprite"))
                {
                    if (info.GunSpritePath.Count == 0)
                        info.GunSpritePath.Add(info.beforeStart["gunSprite"] as string);
                    info.beforeStart.Remove("gunSprite");
                }
            }
            else if (info.afterStart.ContainsKey("sprite"))
            {
                if (info.SpritePath.Count == 0)
                    info.SpritePath.Add(info.afterStart["sprite"] as string);
                info.afterStart.Remove("sprite");
                
                if (info.afterStart.ContainsKey("gunSprite"))
                {
                    if (info.GunSpritePath.Count == 0)
                        info.GunSpritePath.Add(info.afterStart["gunSprite"] as string);
                    info.afterStart.Remove("gunSprite");
                }
            }
            else if (info.beforeAwake.ContainsKey("sprite"))
            {
                if (info.SpritePath.Count == 0)
                    info.SpritePath.Add(info.beforeAwake["sprite"] as string);
                info.beforeAwake.Remove("sprite");
                
                if (info.beforeAwake.ContainsKey("gunSprite"))
                {
                    if (info.GunSpritePath.Count == 0)
                        info.GunSpritePath.Add(info.beforeAwake["gunSprite"] as string);
                    info.beforeAwake.Remove("gunSprite");
                }
            }
            else if (info.afterAwake.ContainsKey("sprite"))
            {
                if (info.SpritePath.Count == 0)
                    info.SpritePath.Add(info.afterAwake["sprite"] as string);
                info.afterAwake.Remove("sprite");
                
                if (info.afterAwake.ContainsKey("gunSprite"))
                {
                    if (info.GunSpritePath.Count == 0)
                        info.GunSpritePath.Add(info.afterAwake["gunSprite"] as string);
                    info.afterAwake.Remove("gunSprite");
                }
            }
        }
        
        private static void DetermineVariantCount(Infos.CustomBroInfo info)
        {
            int maxCount = 1;
            
            // Check all variant property lists
            if (info.SpritePath?.Count > maxCount) maxCount = info.SpritePath.Count;
            if (info.GunSpritePath?.Count > maxCount) maxCount = info.GunSpritePath.Count;
            if (info.GunSpriteOffset?.Count > maxCount) maxCount = info.GunSpriteOffset.Count;
            if (info.SpecialMaterials?.Count > maxCount) maxCount = info.SpecialMaterials.Count;
            if (info.SpecialMaterialOffset?.Count > maxCount) maxCount = info.SpecialMaterialOffset.Count;
            if (info.SpecialMaterialSpacing?.Count > maxCount) maxCount = info.SpecialMaterialSpacing.Count;
            if (info.FirstAvatar?.Count > maxCount) maxCount = info.FirstAvatar.Count;
            if (info.Cutscene?.Count > maxCount) maxCount = info.Cutscene.Count;
            
            info.VariantCount = maxCount;
        }
        
        private static void ValidateVariantLists(Infos.CustomBroInfo info)
        {
            // Each list must have either 1 item or exactly VariantCount items
            ValidateList(info.SpritePath, "SpritePath", info.VariantCount);
            ValidateList(info.GunSpritePath, "GunSpritePath", info.VariantCount);
            ValidateList(info.GunSpriteOffset, "GunSpriteOffset", info.VariantCount);
            ValidateList(info.SpecialMaterials, "SpecialMaterials", info.VariantCount);
            ValidateList(info.SpecialMaterialOffset, "SpecialMaterialOffset", info.VariantCount);
            ValidateList(info.SpecialMaterialSpacing, "SpecialMaterialSpacing", info.VariantCount);
            ValidateList(info.FirstAvatar, "FirstAvatar", info.VariantCount);
            ValidateList(info.Cutscene, "Cutscene", info.VariantCount);
        }
        
        private static void ValidateList<T>(System.Collections.Generic.List<T> list, string propertyName, int expectedCount)
        {
            if (list != null && list.Count > 1 && list.Count != expectedCount)
            {
                throw new InvalidOperationException(
                    $"{propertyName} has {list.Count} items but VariantCount is {expectedCount}. " +
                    $"Lists must have either 1 item (shared across variants) or exactly {expectedCount} items (one per variant).");
            }
        }
        
        public static void SetSprites(this ICustomHero hero)
        {
            // Get sprite paths for current variant
            string spritePath = BroMakerUtilities.GetVariantValue(hero.info.SpritePath, hero.CurrentVariant);
            string gunSpritePath = BroMakerUtilities.GetVariantValue(hero.info.GunSpritePath, hero.CurrentVariant);
            
            BroBase character = hero.character;
            
            // Set main sprite
            if (!string.IsNullOrEmpty(spritePath))
            {
                character.Sprite().SetTexture(ResourcesController.GetTexture(hero.info.path, spritePath));
            }
            
            // Set gun sprite
            if (!string.IsNullOrEmpty(gunSpritePath))
            {
                character.gunSprite.SetTexture(ResourcesController.GetTexture(hero.info.path, gunSpritePath));
            }
        }
    }
}

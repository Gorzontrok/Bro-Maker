using BroMakerLib.CustomObjects;
using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using Networking;
using RocketLib;
using System;
using UnityEngine;
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
                var type = otherBroComponent.GetType();
                UnityEngine.Object.Destroy(otherBroComponent);
                BMLogger.Debug($"SetupCustomHero: Has destroy {type} component.");
                hero.info.ReadParameters(hero.character);
                BMLogger.Debug("SetupCustomHero: Has read parameters");
            }
            else
                BMLogger.Debug("SetupCustomHero: No other bro component founded");

            hero.FixOtherComponentValues();
            hero.RemoveDuplicateGlowLight();

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
            
            // Determine variant count from all variant properties
            DetermineVariantCount(hero.info);
            
            // Validate all variant lists
            ValidateVariantLists(hero.info);
            
            // Select variant
            if (hero is CustomObjects.Bros.CustomHero customHero)
            {
                customHero.CurrentVariant = customHero.GetVariant();
            }
            else
            {
                // For vanilla bros, use random selection
                hero.CurrentVariant = UnityEngine.Random.Range(0, hero.info.VariantCount);
            }

            // Cache current variant parameters
            hero.CurrentGunSpriteOffset = BroMakerUtilities.GetVariantValue( hero.info.GunSpriteOffset, hero.CurrentVariant );
            hero.CurrentSpecialMaterials = BroMakerUtilities.GetVariantValue( hero.info.SpecialMaterials, hero.CurrentVariant );
            hero.CurrentSpecialMaterialOffset = BroMakerUtilities.GetVariantValue( hero.info.SpecialMaterialOffset, hero.CurrentVariant );
            hero.CurrentSpecialMaterialSpacing = BroMakerUtilities.GetVariantValue( hero.info.SpecialMaterialSpacing, hero.CurrentVariant );
            hero.CurrentFirstAvatar = BroMakerUtilities.GetVariantValue( hero.info.FirstAvatar, hero.CurrentVariant );

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
        
        /// <summary>
        /// Loads and sets the sprite and gunsprite material for the given bro.
        /// </summary>
        /// <param name="bro">Bro to load sprites for</param>
        public static void SetSprites(this ICustomHero bro)
        {
            // Get sprite paths for current variant
            string spritePath = BroMakerUtilities.GetVariantValue(bro.info.SpritePath, bro.CurrentVariant);
            string gunSpritePath = BroMakerUtilities.GetVariantValue(bro.info.GunSpritePath, bro.CurrentVariant);
            
            BroBase character = bro.character;
            
            // Set main sprite
            if (!string.IsNullOrEmpty(spritePath))
            {
                Material material = ResourcesController.GetMaterial( bro.info.path, spritePath );
                character.material = material;
                character.SetFieldValue( "defaultMaterial", material );
            }
            
            // Set gun sprite
            if (!string.IsNullOrEmpty(gunSpritePath))
            {
                Material gunMaterial = ResourcesController.GetMaterial( bro.info.path, gunSpritePath );
                character.gunSprite.meshRender.sharedMaterial = gunMaterial;
                character.SetFieldValue( "gunMaterial", gunMaterial );
            }

            if ( character is CustomHero customHero )
            {
                customHero.SetupAdditionalSprites();
            }
        }

        /// <summary>
        /// Called before base.Awake() for vanilla bros
        /// </summary>
        /// <param name="hero">The hero instance to setup</param>
        /// <param name="fixNullVariableLocal">Optional action to fix null variables specific to the hero type</param>
        public static void StandardBeforeAwake(this ICustomHero hero, Action fixNullVariableLocal = null)
        {
            hero.character = hero as BroBase;
            hero.info = LoadHero.currentInfo;
            fixNullVariableLocal?.Invoke();
            hero.SetupCustomHero();
            hero.info.BeforeAwake(hero);
        }

        /// <summary>
        /// Called after base.Awake() for vanilla bros
        /// </summary>
        /// <param name="hero">The hero instance to cleanup</param>
        public static void StandardAfterAwake(this ICustomHero hero)
        {
            hero.SetSprites();
            hero.info.AfterAwake(hero);
        }

        /// <summary>
        /// Called before base.Start() for vanilla bros
        /// </summary>
        /// <param name="hero">The hero instance to setup</param>
        public static void StandardBeforeStart(this ICustomHero hero)
        {
            hero.info.BeforeStart(hero);
        }

        /// <summary>
        /// Called after base.Start() for vanilla bros
        /// </summary>
        /// <param name="hero">The hero instance to cleanup</param>
        public static void StandardAfterStart(this ICustomHero hero)
        {
            hero.info.AfterStart(hero);
        }
    }
}

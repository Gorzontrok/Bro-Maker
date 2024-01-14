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
            heroCharacter.soundHolder = character.soundHolder;
            heroCharacter.soundHolderFootSteps = character.soundHolderFootSteps;
            heroCharacter.soundHolderVoice = character.soundHolderVoice;
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
            heroCharacter.specialGrenade = LoadGrenade.GetGrenade(string.Empty);
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
                character.parachute.Set_tvd(character);
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

            if (info.beforeStart.ContainsKey("sprite"))
            {
                info.spritePath = Path.Combine( info.path, info.beforeStart["sprite"] as string );
                info.gunSpritePath = Path.Combine( info.path, info.beforeStart["gunSprite"] as string );
            }
            else if (info.afterStart.ContainsKey("sprite"))
            {
                info.spritePath = Path.Combine(info.path, info.afterStart["sprite"] as string );
                info.gunSpritePath = Path.Combine(info.path, info.afterStart["gunSprite"] as string);
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
    }
}

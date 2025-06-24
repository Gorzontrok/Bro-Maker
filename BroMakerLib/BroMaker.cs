using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using HarmonyLib;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace BroMakerLib
{
    public static class BroMaker
    {
        internal static Harmony harmony;

        private static bool _hasInit;

        public static void Initialize()
        {
            if(!_hasInit)
            {
                BMLogger.Log("Initialization of BroMaker");

                harmony = new Harmony("BroMakerLib");
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                Settings.Load();
                DirectoriesManager.Initialize();

                BroMakerStorage.Initialize();
                PresetManager.Initialize();

                _hasInit = true;
                BMLogger.Log("Finish Initialization");
            }
        }

        public static void ApplyBroPatches( Harmony harmony )
        {
            GameObject heroHolder = new GameObject();
            heroHolder.SetActive(false);

            foreach (KeyValuePair<string, Type> kvp in PresetManager.heroesPreset)
            {
                if (typeof(CustomHero).IsAssignableFrom(kvp.Value))
                {
                    CustomHero bro = heroHolder.AddComponent(kvp.Value) as CustomHero;
                    bro.HarmonyPatches(harmony);
                }
            }
        }

        public static void PreloadBroAssets()
        {
            // Preload all assets listed in the Json file
            for ( int i = 0; i < BroMakerStorage.Bros.Length; ++i )
            {
                CustomBroInfo info = BroMakerStorage.Bros[i].GetInfo();
                List<string> spritePaths = new List<string>();
                List<string> soundPaths = new List<string>();

                if (info.beforeStart.ContainsKey("sprite"))
                {
                    spritePaths.Add(info.beforeStart["sprite"] as string);
                    spritePaths.Add(info.beforeStart["gunSprite"] as string);
                }
                else if (info.afterStart.ContainsKey("sprite"))
                {
                    spritePaths.Add(info.afterStart["sprite"] as string);
                    spritePaths.Add(info.afterStart["gunSprite"] as string);
                }
                else if (info.beforeAwake.ContainsKey("sprite"))
                {
                    spritePaths.Add(info.beforeAwake["sprite"] as string);
                    spritePaths.Add(info.beforeAwake["gunSprite"] as string);
                }
                else if (info.afterAwake.ContainsKey("sprite"))
                {
                    spritePaths.Add(info.afterAwake["sprite"] as string);
                    spritePaths.Add(info.afterAwake["gunSprite"] as string);
                }

                if ( info.parameters.ContainsKey( "SpecialIcons" ) )
                {
                    if ( info.parameters["SpecialIcons"] is string path )
                    {   
                        spritePaths.Add( path );
                    }
                    else if ( info.parameters["SpecialIcons"] is JArray paths )
                    {
                        for ( int j = 0; j < paths.Count; ++j )
                        {
                            spritePaths.Add( paths[j].ToObject<string>() );
                        }
                    }
                }
                if ( info.parameters.ContainsKey("Avatar") )
                {
                    spritePaths.Add(info.parameters["Avatar"] as string);
                }

                if ( info.cutscene.spritePath != string.Empty )
                {
                    spritePaths.Add(info.cutscene.spritePath);
                }
                if ( info.cutscene.barkPath != string.Empty )
                {
                    soundPaths.Add(info.cutscene.barkPath);
                }
                if ( info.cutscene.fanfarePath != string.Empty )
                {
                    soundPaths.Add(info.cutscene.fanfarePath);
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
                    CustomHero bro = heroHolder.AddComponent(kvp.Value) as CustomHero;
                    bro.PreloadAssets();
                }
            }
        }

        public static void ReloadFiles()
        {
            BroMakerStorage.Initialize();
        }
    }
}

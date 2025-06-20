﻿using BroMakerLib.CustomObjects.Bros;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.ModManager;
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

        public static Shader Shader1
        {
            get
            {
                return HeroController.GetAvatarMaterial(HeroType.Rambro).shader;
            }
        }

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

                ModLoader.Initialize();
                MakerObjectStorage.Initialize();
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
            for ( int i = 0; i < MakerObjectStorage.Bros.Length; ++i )
            {
                CustomBroInfo info = MakerObjectStorage.Bros[i].GetInfo();
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
            MakerObjectStorage.Initialize();
        }

        /// <summary>
        /// Create a material from a file with a specific shader
        /// </summary>
        /// <param name="ImagePath"></param>
        /// <returns></returns>
        public static Material CreateMaterialFromFile(string ImagePath)
        {
            return CreateMaterialFromFile(ImagePath, Shader1);
        }

        /// <summary>
        /// Create a material from a file with a specific shader
        /// </summary>
        /// <param name="ImagePath"></param>
        /// <param name="shader"></param>
        /// <returns></returns>
        public static Material CreateMaterialFromFile(string ImagePath, Shader shader)
        {
            Material mat = new Material(shader);

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(ImagePath));
            tex.filterMode = FilterMode.Point;
            tex.anisoLevel = 1;
            tex.mipMapBias = 0;
            tex.wrapMode = TextureWrapMode.Clamp;

            mat.mainTexture = tex;

            return mat;
        }


        internal static Type GetBroType(HeroType heroType)
        {
            switch(heroType)
            {
                case HeroType.AshBrolliams: return typeof(AshBrolliams);
                case HeroType.BaBroracus: return typeof(BaBroracus);
                case HeroType.Blade: return typeof(Blade);
                //case HeroType.BoondockBros: return typeof(BoondockBro);
                case HeroType.Brobocop: return typeof(Brobocop);
                case HeroType.Broc: return typeof(BrocSnipes);
                case HeroType.Brochete: return typeof(Brochete);
                case HeroType.BrodellWalker: return typeof(BrodellWalker);
                case HeroType.Broden: return typeof(Broden);
                case HeroType.BroDredd: return typeof(BroDredd);
                case HeroType.BroHard: return typeof(BroHard);
                case HeroType.BroLee: return typeof(BroLee);
                case HeroType.BroMax: return typeof(BroMax);
                case HeroType.Brominator: return typeof(Brominator);
                case HeroType.Brommando: return typeof(Brommando);
                case HeroType.BronanTheBrobarian: return typeof(BronanTheBrobarian);
                case HeroType.BrondleFly: return typeof(BrondleFly);
                case HeroType.BroneyRoss: return typeof(BroneyRoss);
                case HeroType.BroniversalSoldier: return typeof(BroniversalSoldier);
                case HeroType.BronnarJensen: return typeof(BronnarJensen);
                case HeroType.Brononymous: return typeof(Brononymous);
                case HeroType.BroveHeart: return typeof(BroveHeart);
                case HeroType.CherryBroling: return typeof(CherryBroling);
                case HeroType.ColJamesBroddock: return typeof(ColJamesBrodock);
                case HeroType.DirtyHarry: return typeof(DirtyHarry);
                case HeroType.DoubleBroSeven: return typeof(DoubleBroSeven);
                case HeroType.EllenRipbro: return typeof(EllenRipbro);
                case HeroType.HaleTheBro: return typeof(BroCeasar);
                case HeroType.IndianaBrones: return typeof(IndianaBrones);
                case HeroType.LeeBroxmas: return typeof(LeeBroxmas);
                case HeroType.McBrover: return typeof(McBrover);
                case HeroType.Nebro: return typeof(Nebro);
                case HeroType.Predabro: return typeof(Predabro);
                case HeroType.Rambro: return typeof(Rambro);
                case HeroType.SnakeBroSkin: return typeof(SnakeBroskin);
                case HeroType.TimeBroVanDamme: return typeof(TimeBroVanDamme);
                case HeroType.TollBroad: return typeof(TollBroad);
                case HeroType.TankBro: return typeof(TankBro);
                case HeroType.TheBrocketeer: return typeof(TheBrocketeer);
                case HeroType.TheBrode: return typeof(TheBrode);
                case HeroType.TheBrofessional: return typeof(TheBrofessional);
                case HeroType.TheBrolander: return typeof(TheBrolander);
                case HeroType.TrentBroser: return typeof(TrentBroser);
            }
            return null;
        }
        internal static HeroType GetBroHeroType(string BroName)
        {
            switch (BroName)
            {
                case "AshBrolliams": return HeroType.AshBrolliams;
                case "BaBroracus": return HeroType.BaBroracus;
                case "Brade": return HeroType.Blade;
                case "BoondockBros": return HeroType.BoondockBros;
                case "Brobocop": return HeroType.Brobocop;
                case "Broc": return HeroType.Broc;
                case "Brochete": return HeroType.Brochete;
                case "BrodellWalker": return HeroType.BrodellWalker;
                case "Broden": return HeroType.Broden;
                case "BroDredd": return HeroType.BroDredd;
                case "BroHard": return HeroType.BroHard;
                case "BroLee": return HeroType.BroLee;
                case "BroMax": return HeroType.BroMax;
                case "Brominator": return HeroType.Brominator;
                case "Brommando": return HeroType.Brommando;
                case "BronanTheBrobarian": return HeroType.BronanTheBrobarian;
                case "BrondleFly": return HeroType.BrondleFly;
                case "BroneyRoss": return HeroType.BroneyRoss;
                case "BroniversalSoldier": return HeroType.BroniversalSoldier;
                case "BronnarJensen": return HeroType.BronnarJensen;
                case "BroInBlack": return HeroType.Brononymous;
                case "BroHeart": return HeroType.BroveHeart;
                case "CherryBroling": return HeroType.CherryBroling;
                case "ColJamesBroddock": return HeroType.ColJamesBroddock;
                case "DirtyHarry": return HeroType.DirtyHarry;
                case "DoubleBroSeven": return HeroType.DoubleBroSeven;
                case "EllenRipbro": return HeroType.EllenRipbro;
                case "BroCaesar": return HeroType.HaleTheBro;
                case "IndianaBrones": return HeroType.IndianaBrones;
                case "LeeBroxmas": return HeroType.LeeBroxmas;
                case "McBrover": return HeroType.McBrover;
                case "Nebro": return HeroType.Nebro;
                case "Predabro": return HeroType.Predabro;
                case "Rambro": return HeroType.Rambro;
                case "SnakeBroSkin": return HeroType.SnakeBroSkin;
                case "TimeBro": return HeroType.TimeBroVanDamme;
                case "TollBroad": return HeroType.TollBroad;
                case "TankBro": return HeroType.TankBro;
                case "TheBrocketeer": return HeroType.TheBrocketeer;
                case "TheBrode": return HeroType.TheBrode;
                case "TheBrofessional": return HeroType.TheBrofessional;
                case "TheBrolander": return HeroType.TheBrolander;
                case "TrentBroser": return HeroType.TrentBroser;
            }
            return HeroType.Rambro;
        }
    }
}

using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using BroMakerLoadMod;
using System.IO;

namespace BroMakerLib
{
    /// <summary>
    ///
    /// </summary>
    public static class BroMaker
    {
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
        internal static Texture2D CreateTexture(string ImagePath)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(ImagePath));
            tex.filterMode = FilterMode.Point;
            tex.anisoLevel = 1;
            tex.mipMapBias = 0;
            tex.wrapMode = TextureWrapMode.Repeat;

            return tex;
        }

        /// <summary>
        /// Create a material from a file.
        /// </summary>
        /// <param name="ImagePath"></param>
        /// <returns></returns>
        public static Material CreateMaterialFromFile(string ImagePath)
        {
            return CreateMaterialFromFile(ImagePath, Shader.Find("Unlit/Depth Cutout With ColouredImage"));
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
            tex.wrapMode = TextureWrapMode.Repeat;

            mat.mainTexture = tex;

            return mat;
        }
    }
}

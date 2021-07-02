using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using System.Reflection;
using System.IO;

namespace BroMaker
{
    public static class BroMaker
    {
        private static Dictionary<HeroType, HeroController.HeroDefinition> heroDefinition = Traverse.Create(HeroController.Instance).Field("_heroData").GetValue() as Dictionary<HeroType, HeroController.HeroDefinition>;

        private static bool switchToBase = false;
        private static HeroType heroBase = HeroType.Rambro;

        private static void Logging(object str)
        {
            BroMakerLoadMod.Main.Log(str);
        }

        public static void SpawnHero(HeroType broBase, SpriteSM sprite, string SpriteCharacterPath, string SpriteGunPath = null, string SpriteProjectilePath = null, string SpriteArmlessPath = null)
        {
            try
            {
                switchToBase = true;
                heroBase = broBase;
                /*if(SpriteArmlessPath != null)
                    MakeSprite(sprite, ConvertToTexture2D(SpriteCharacterPath), ConvertToTexture2D(SpriteGunPath), ConvertToTexture2D(SpriteProjectilePath), ConvertToTexture2D(SpriteArmlessPath));
                else MakeSprite(sprite, ConvertToTexture2D(SpriteCharacterPath), ConvertToTexture2D(SpriteGunPath), ConvertToTexture2D(SpriteProjectilePath));*/
            }
            catch (Exception ex)
            {
                Logging(ex);
            }

        }

        public static Texture2D LoadCharacterSprite(string SpriteImage, SpriteSM sprite)
        {
            // Load the image
            var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            try
            {
                tex.LoadImage(File.ReadAllBytes(SpriteImage));
                tex.wrapMode = TextureWrapMode.Clamp;

                Texture orig = sprite.meshRender.sharedMaterial.GetTexture("_MainTex");

                tex.anisoLevel = orig.anisoLevel;
                tex.filterMode = orig.filterMode;
                tex.mipMapBias = orig.mipMapBias;
                tex.wrapMode = orig.wrapMode;

            }
            catch (Exception ex)
            {
                Logging(ex);
            }
            return tex;

        }

        public static Texture2D LoadGunSprite(string SpriteImage, TestVanDammeAnim newbro)
        {
            var texGun = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            try
            {
                // Load the image
                texGun.LoadImage(File.ReadAllBytes(SpriteImage));
                texGun.wrapMode = TextureWrapMode.Clamp;

                Texture origGun = newbro.gunSprite.GetComponent<Renderer>().sharedMaterial.GetTexture("_MainTex");
                
                texGun.anisoLevel = origGun.anisoLevel;
                texGun.filterMode = origGun.filterMode;
                texGun.mipMapBias = origGun.mipMapBias;
                texGun.wrapMode = origGun.wrapMode;
            }
            catch (Exception ex)
            {
                Logging(ex);
            }

            return texGun;
        }

        public static Texture2D LoadCharacterArmlessSprite(string SpriteImage, SpriteSM sprite)
        {
            var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            try
            {
                tex.LoadImage(File.ReadAllBytes(SpriteImage));
                tex.wrapMode = TextureWrapMode.Clamp;

                Texture orig = sprite.meshRender.sharedMaterial.GetTexture("_MainTex");

                tex.anisoLevel = orig.anisoLevel;
                tex.filterMode = orig.filterMode;
                tex.mipMapBias = orig.mipMapBias;
                tex.wrapMode = orig.wrapMode;
            }
            catch (Exception ex)
            {
                Logging(ex);
            }

            return tex;
        }


    }
}

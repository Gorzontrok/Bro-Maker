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
        private static void Logging(object str)
        {
            BroMakerLoadMod.Main.Log(str);
        }


        public static Texture2D CreateCharacterSprite(string SpriteImage, SpriteSM sprite)
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

        public static Texture2D CreateGunSprite(string SpriteImage, Texture origGun)
        {
            var texGun = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            try
            {
                // Load the image
                texGun.LoadImage(File.ReadAllBytes(SpriteImage));
                texGun.wrapMode = TextureWrapMode.Clamp;

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

        public static Texture2D CreateAmmoSprite(string SpriteImage, MeshRenderer meshRenderer)
        {
            var tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            try
            {
                tex.LoadImage(File.ReadAllBytes(SpriteImage));
                tex.wrapMode = TextureWrapMode.Clamp;

                Texture orig = meshRenderer.sharedMaterial.mainTexture;

                tex.anisoLevel = orig.anisoLevel;
                tex.filterMode = orig.filterMode;
                tex.mipMapBias = orig.mipMapBias;
                tex.wrapMode = orig.wrapMode;
            }
            catch(Exception ex)
            {
                Logging(ex);
            }
            return tex;
        }

        public static SpriteSM CreateAvatar(string filePath, ref PlayerHUD PHUD) //Create avatar sprite
        {
            if (!File.Exists(filePath)) throw new Exception("The specified file does not exist !\n");
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(File.ReadAllBytes(filePath));
            tex.wrapMode = TextureWrapMode.Clamp;

            SpriteSM sprite = PHUD.avatar;

            Texture orig = sprite.meshRender.sharedMaterial.GetTexture("_MainTex");

            tex.anisoLevel = orig.anisoLevel;
            tex.filterMode = orig.filterMode;
            tex.mipMapBias = orig.mipMapBias;
            tex.wrapMode = orig.wrapMode;

            sprite.meshRender.sharedMaterial.SetTexture("_MainTex", tex);

            return sprite;
        }
    }
}

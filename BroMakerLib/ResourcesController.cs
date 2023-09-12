using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using BroMakerLib.Loggers;
using TFBGames.Systems;
using RocketLib;
using Mono.Cecil;

namespace BroMakerLib
{
    public static class ResourcesController
    {
        /// <summary>
        /// Particles/Alpha Blended
        /// </summary>
        public static Shader Particle_AlphaBlend
        {
            get
            {
                return Shader.Find("Particles/Alpha Blended");
            }
        }

        /// <summary>
        /// Unlit/Depth Cutout With ColouredImage
        /// </summary>
        public static Shader Unlit_DepthCutout
        {
            get
            {
                return Shader.Find("Unlit/Depth Cutout With ColouredImage");
            }
        }

        /// <summary>
        /// Particle/Additive
        /// </summary>
        public static Shader Particle
        {
            get
            {
                return Shader.Find("Particle/Additive");
            }
        }


        private static string AssetsFolder
        {
            get
            {
                return Path.Combine(Directory.GetCurrentDirectory(), "assets");
            }
        }

        public static string resourceFolder = "BroMaker.Assets.";

        private static Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private static Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();

        [Obsolete("Use 'ResourcesController.GetMaterial' instead.")]
        public static Material GetMaterialResource(string resourceName, Shader shader)
        {
            Material result;
            if (materials.ContainsKey(resourceName))
            {
                return materials[resourceName];
            }
            else
            {
                result = CreateMaterial(resourceName, shader);
                if (result != null)
                {
                    materials.Add(resourceName, result);
                }
            }
            return result;
        }

        public static Material GetMaterial(string filePath)
        {
            Material result = null;
            if (materials.ContainsKey(filePath))
            {
                return materials[filePath];
            }

            if (filePath.Contains(":"))
            {
                result = LoadAssetSync<Material>(filePath);
            }
            else
            {
                result = CreateMaterial(filePath, Unlit_DepthCutout);
            }

            if (result != null)
            {
                materials.Add(filePath, result);
            }
            return result;
        }

        public static Material CreateMaterial(string filePath, Shader shader)
        {
            var tex = CreateTexture(filePath);
            if (tex != null)
            {
                var mat = new Material(shader);
                mat.mainTexture = tex;
                return mat;
            }
            return null;
        }
        public static Material CreateMaterial(string filePath, Material source)
        {
            var tex = CreateTexture(filePath);
            if (tex != null)
            {
                var mat = new Material(source);
                mat.mainTexture = tex;
                return mat;
            }
            return null;
        }

        public static Texture2D GetTexture(string path, string fileName)
        {
            Texture2D tex = null;
            textures.TryGetValue(fileName, out tex);
            if (tex != null)
                return tex;

            var filePath = Path.Combine(path, fileName);
            if (File.Exists(filePath))
            {
                tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
                tex.LoadImage(File.ReadAllBytes(filePath));
                tex.filterMode = FilterMode.Point;
                textures.Add(fileName, tex);
            }

            if (fileName.Contains(":"))
            {
                try
                {
                    tex = LoadAssetSync<Texture2D>(fileName);
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog(ex);
                }
            }
            else
                tex = CreateTexture(path, fileName);

            if (tex != null)
                textures.Add(fileName, tex);
            return tex;
        }

        public static Texture2D CreateTexture(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return CreateTexture(File.ReadAllBytes(filePath));
        }

        public static Texture2D CreateTexture(string path, string fileName)
        {
            if (fileName.Contains(":"))
            {
                BMLogger.Warning($"The argument '{nameof(fileName)}' contains ':' which means it's a asset path. Use 'ResourcesController.GetTexture' method instead.");
                return LoadAssetSync<Texture2D>(fileName);
            }
            return CreateTexture(Path.Combine(path, fileName));
        }

        public static Texture2D CreateTexture(byte[] imageBytes)
        {
            if (imageBytes.IsNullOrEmpty())
                throw new ArgumentException("Is null or empty", nameof(imageBytes));

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(imageBytes);
            tex.filterMode = FilterMode.Point;
            tex.anisoLevel = 1;
            tex.mipMapBias = 0;
            tex.wrapMode = TextureWrapMode.Repeat;
            return tex;
        }

        [Obsolete("It get the resource from 'BroMakerLib.dll'. Return value is null.")]
        public static byte[] ExtractResource(string filename)
        {
            return null;
        }

        public static T LoadAssetSync<T>(string name) where T : UnityEngine.Object
        {
            return GameSystems.ResourceManager.LoadAssetSync<T>(name);
        }
    }
}



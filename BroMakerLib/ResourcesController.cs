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
        private static Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

        /// <summary>
        /// Creates a Material using the shader Unlit_DepthCutout.
        /// Loads Material from cache if created previously.
        /// </summary>
        /// <param name="path">Path to an image or asset</param>
        /// /// <param name="fileName">Name of an image or asset file</param>
        /// <returns></returns>
        public static Material GetMaterial(string path, string fileName)
        {
            return GetMaterial(Path.Combine(path, fileName));
        }

        /// <summary>
        /// Creates a Material using the shader Unlit_DepthCutout.
        /// Loads Material from cache if created previously.
        /// </summary>
        /// <param name="filePath">Path to an image or asset file</param>
        /// <returns></returns>
        public static Material GetMaterial(string filePath)
        {
            Material result = null;
            if (materials.ContainsKey(filePath))
            {
                return materials[filePath];
            }

            if ( File.Exists(filePath) )
            {
                result = CreateMaterial(filePath, Unlit_DepthCutout);
            }
            else if (filePath.Contains(":"))
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

        /// <summary>
        /// Creates a Material from an array of bytes using the shader Unlit_DepthCutout.
        /// The Material is not cached, use GetMaterial if caching is desired.
        /// </summary>
        /// <param name="imageBytes">Byte array to load image from</param>
        /// <returns></returns>
        public static Material CreateMaterial(byte[] imageBytes)
        {
            Material result = null;

            var tex = CreateTexture(imageBytes);
            if (tex != null)
            {
                var mat = new Material(Unlit_DepthCutout);
                mat.mainTexture = tex;
                return mat;
            }

            return result;
        }

        /// <summary>
        /// Creates a Material using the specified image and shader.
        /// The Material is not cached, use GetMaterial if caching is desired.
        /// </summary>
        /// <param name="filePath">Path to an image file</param>
        /// <param name="shader">Shader to use</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a Material using the specified image and Material as a source.
        /// The Material is not cached, use GetMaterial if caching is desired.
        /// </summary>
        /// <param name="filePath">Path to an image file</param>
        /// <param name="source">Source Material to copy</param>
        /// <returns></returns>
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

        /// <summary>
        /// Creates a Texture2D from an image or asset file.
        /// Loads Texture2D from cache if created previously.
        /// </summary>
        /// <param name="path">Path to an image or asset</param>
        /// /// <param name="fileName">Name of an image or asset file</param>
        /// <returns></returns>
        public static Texture2D GetTexture(string path, string fileName)
        {
            return GetTexture(Path.Combine(path, fileName));
        }

        /// <summary>
        /// Creates a Texture2D from an image or asset file.
        /// Loads Texture2D from cache if created previously.
        /// </summary>
        /// <param name="filePath">Path to an image or asset file</param>
        /// <returns></returns>
        public static Texture2D GetTexture(string filePath)
        {
            Texture2D tex = null;
            textures.TryGetValue(filePath, out tex);
            if (tex != null)
                return tex;

            if (File.Exists(filePath))
            {
                tex = CreateTexture(filePath);
            }
            else if (filePath.Contains(":"))
            {
                try
                {
                    tex = LoadAssetSync<Texture2D>(filePath);
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog(ex);
                }
            }
            else
                tex = CreateTexture(filePath);

            if (tex != null)
                textures.Add(filePath, tex);
            return tex;
        }

        /// <summary>
        /// Creates a Texture2D from an image or asset file.
        /// The Texture2D is not cached, use GetTexture if caching is desired.
        /// </summary>
        /// <param name="filePath">Path to an image file</param>
        /// <returns></returns>
        public static Texture2D CreateTexture(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File not found", filePath);

            return CreateTexture(File.ReadAllBytes(filePath));
        }

        /// <summary>
        /// Creates a Texture2D from a byte array.
        /// The Texture2D is not cached, use GetTexture if caching is desired.
        /// </summary>
        /// <param name="imageBytes">Byte array to load image from</param>
        /// <returns></returns>
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

            // Textures always load as ARGB32 when loading from a png, so this is necessary to convert it to RGBA32
            Texture2D convertedTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            convertedTexture.filterMode = FilterMode.Point;
            convertedTexture.anisoLevel = 1;
            convertedTexture.mipMapBias = 0;
            convertedTexture.wrapMode = TextureWrapMode.Repeat;
            Graphics.ConvertTexture(tex, convertedTexture);
            return convertedTexture;
        }

        /// <summary>
        /// Creates an AudioClip from an audio file.
        /// Loads AudioClip from cache if created previously. Note that the same cached AudioClip can't be played several times simultaneously.
        /// </summary>
        /// <param name="path">Path to an audio file</param>
        /// <param name="fileName">Name of an audio file</param>
        /// <returns></returns>
        public static AudioClip GetAudioClip(string path, string fileName)
        {
            // Get full path converts / to \ which is needed because WWW doesn't like /
            return GetAudioClip(Path.GetFullPath(Path.Combine(path, fileName)));
        }

        /// <summary>
        /// Creates an AudioClip from an audio file.
        /// Loads AudioClip from cache if created previously. Note that the same cached AudioClip can't be played several times simultaneously.
        /// </summary>
        /// <param name="filePath">Path to an audio file</param>
        /// <returns></returns>
        public static AudioClip GetAudioClip(string filePath)
        {
            AudioClip result = null;
            if (audioClips.ContainsKey(filePath))
            {
                return audioClips[filePath];
            }

            if (File.Exists(filePath))
            {
                result = CreateAudioClip(filePath);
            }
            else if (filePath.Contains(":"))
            {
                result = LoadAssetSync<AudioClip>(filePath);
            }
            else
            {
                result = CreateAudioClip(filePath);
            }

            if (result != null)
            {
                audioClips.Add(filePath, result);
            }
            return result;
        }

        /// <summary>
        /// Creates an AudioClip from an audio file.
        /// The AudioClip is not cached, use GetAudioClip is caching is desired.
        /// </summary>
        /// <param name="path">Path to an audio file</param>
        /// <param name="fileName">Name of an audio file</param>
        /// <returns></returns>
        public static AudioClip CreateAudioClip(string path, string fileName)
        {
            string filePath = Path.GetFullPath(Path.Combine(path, fileName));
            return CreateAudioClip(filePath);
        }

        /// <summary>
        /// Creates an AudioClip from an audio file.
        /// The AudioClip is not cached, use GetAudioClip is caching is desired.
        /// </summary>
        /// <param name="filePath">Path to an audio file</param>
        /// <returns></returns>
        public static AudioClip CreateAudioClip(string filePath)
        {
            WWW getClip = new WWW("file:////" + filePath);

            while ( !getClip.isDone )
            {
            };


            AudioClip result = getClip.GetAudioClip(false, true);
            result.name = Path.GetFileNameWithoutExtension(filePath);

            return result;
        }

        /// <summary>
        /// Creates a byte array from a file.
        /// </summary>
        /// <param name="filename">Name of a file</param>
        /// <returns></returns>
        public static byte[] ExtractResource(string filename)
        {
            Assembly a = Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null) return null;
                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        /// <summary>
        /// Loads an object from an asset file.
        /// </summary>
        /// <typeparam name="T">Type of the object</typeparam>
        /// <param name="name">Name of the asset file</param>
        /// <returns></returns>
        public static T LoadAssetSync<T>(string name) where T : UnityEngine.Object
        {
            return GameSystems.ResourceManager.LoadAssetSync<T>(name);
        }
    }
}



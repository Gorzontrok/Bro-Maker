using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using TFBGames.Systems;
using UnityEngine;

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

        private static readonly Dictionary<string, Material> materials = new Dictionary<string, Material>();
        private static readonly Dictionary<string, Texture2D> textures = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();

        /// <summary>
        /// Creates a Material using the shader Unlit_DepthCutout.
        /// Loads Material from cache if created previously.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it or the named game asset can't be loaded or decoded.
        /// </summary>
        /// <param name="path">Path to an image or asset</param>
        /// /// <param name="fileName">Name of an image or asset file</param>
        /// <returns></returns>
        public static Material GetMaterial(string path, string fileName)
        {
            return Reporting(path, () => LoadMaterial(path, fileName));
        }

        internal static Material LoadMaterial(string path, string fileName)
        {
            return LoadMaterial(Path.Combine(path, fileName));
        }

        /// <summary>
        /// Creates a Material using the shader Unlit_DepthCutout.
        /// Loads Material from cache if created previously.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it or the named game asset can't be loaded or decoded.
        /// </summary>
        /// <param name="filePath">Path to an image or asset file</param>
        /// <returns></returns>
        public static Material GetMaterial(string filePath)
        {
            return Reporting(filePath, () => LoadMaterial(filePath));
        }

        private static Material LoadMaterial(string filePath)
        {
            Material result = null;
            if (materials.TryGetValue(filePath, out Material cached) && cached)
            {
                return cached;
            }

            if (File.Exists(filePath))
            {
                result = LoadMaterialFile(filePath, Unlit_DepthCutout, null);
            }
            else if (filePath.Contains(":") && filePath[1] != ':') // Second condition is here to make sure it is not a disk path
            {
                try
                {
                    result = LoadAssetSync<Material>(filePath);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to load asset-bundle material '{filePath}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new FileNotFoundException($"Material file not found: '{filePath}'", filePath);
            }

            if (result != null)
            {
                materials[filePath] = result;
            }
            return result;
        }

        /// <summary>
        /// Creates a Material from an array of bytes using the shader Unlit_DepthCutout.
        /// The Material is not cached, use GetMaterial if caching is desired.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `ArgumentException` when the data is empty, `InvalidDataException` when it can't be decoded as an image.
        /// </summary>
        /// <param name="imageBytes">Byte array to load image from</param>
        /// <returns></returns>
        public static Material CreateMaterial(byte[] imageBytes)
        {
            return Reporting(null, () =>
            {
                Texture2D tex = LoadTexture(imageBytes, false);
                Material mat = new Material(Unlit_DepthCutout);
                mat.mainTexture = tex;
                return mat;
            });
        }

        /// <summary>
        /// Creates a Material using the specified image and shader.
        /// The Material is not cached, use GetMaterial if caching is desired.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it can't be read or decoded.
        /// </summary>
        /// <param name="filePath">Path to an image file</param>
        /// <param name="shader">Shader to use</param>
        /// <returns></returns>
        public static Material CreateMaterial(string filePath, Shader shader)
        {
            return Reporting(filePath, () => LoadMaterialFile(filePath, shader, null));
        }

        /// <summary>
        /// Creates a Material using the specified image and Material as a source.
        /// The Material is not cached, use GetMaterial if caching is desired.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it can't be read or decoded.
        /// </summary>
        /// <param name="filePath">Path to an image file</param>
        /// <param name="source">Source Material to copy</param>
        /// <returns></returns>
        public static Material CreateMaterial(string filePath, Material source)
        {
            return Reporting(filePath, () => LoadMaterialFile(filePath, null, source));
        }

        private static Material LoadMaterialFile(string filePath, Shader shader, Material source)
        {
            Texture2D tex = LoadTextureFile(filePath, false);
            Material mat = source != null ? new Material(source) : new Material(shader);
            mat.mainTexture = tex;
            return mat;
        }

        /// <summary>
        /// Creates a Texture2D from an image or asset file.
        /// Loads Texture2D from cache if created previously.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it or the named game asset can't be loaded or decoded.
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
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it or the named game asset can't be loaded or decoded.
        /// </summary>
        /// <param name="filePath">Path to an image or asset file</param>
        /// <returns></returns>
        public static Texture2D GetTexture(string filePath)
        {
            return Reporting(filePath, () => LoadTexture(filePath));
        }

        private static Texture2D LoadTexture(string filePath)
        {
            Texture2D tex = null;
            textures.TryGetValue(filePath, out tex);
            if (tex != null)
                return tex;

            if (File.Exists(filePath))
            {
                tex = LoadTextureFile(filePath, false);
            }
            else if (filePath.Contains(":") && filePath[1] != ':') // Second condition is here to make sure it is not a disk path
            {
                try
                {
                    tex = LoadAssetSync<Texture2D>(filePath);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to load asset-bundle texture '{filePath}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new FileNotFoundException($"Texture file not found: '{filePath}'", filePath);
            }

            if (tex != null)
            {
                textures[filePath] = tex;
            }
            return tex;
        }

        /// <summary>
        /// Creates a Texture2D from an image or asset file.
        /// The Texture2D is not cached, use GetTexture if caching is desired.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it can't be read or decoded.
        /// </summary>
        /// <param name="filePath">Path to an image file</param>
        /// <returns></returns>
        public static Texture2D CreateTexture(string filePath)
        {
            return CreateTexture(filePath, false);
        }

        /// <summary>
        /// Creates a Texture2D from an image file.
        /// The Texture2D is not cached, use GetTexture if caching is desired.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it can't be read or decoded.
        /// </summary>
        /// <param name="filePath">Path to an image file</param>
        /// <param name="readable">Whether the texture's pixels can be read back with GetPixels, which makes loading slower</param>
        public static Texture2D CreateTexture(string filePath, bool readable)
        {
            return Reporting(filePath, () => LoadTextureFile(filePath, readable));
        }

        private static Texture2D LoadTextureFile(string filePath, bool readable)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Texture file not found: '{filePath}'", filePath);
            }

            try
            {
                return LoadTexture(File.ReadAllBytes(filePath), readable);
            }
            catch (Exception ex)
            {
                throw new IOException($"Failed to load texture '{filePath}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a Texture2D from a byte array.
        /// The Texture2D is not cached, use GetTexture if caching is desired.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `ArgumentException` when the data is empty, `InvalidDataException` when it can't be decoded as an image.
        /// </summary>
        /// <param name="imageBytes">Byte array to load image from</param>
        /// <returns></returns>
        public static Texture2D CreateTexture(byte[] imageBytes)
        {
            return CreateTexture(imageBytes, false);
        }

        /// <summary>
        /// Creates a Texture2D from a byte array.
        /// The Texture2D is not cached, use GetTexture if caching is desired.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `ArgumentException` when the data is empty, `InvalidDataException` when it can't be decoded as an image.
        /// </summary>
        /// <param name="imageBytes">Byte array to load image from</param>
        /// <param name="readable">Whether the texture's pixels can be read back with GetPixels, which makes loading slower</param>
        public static Texture2D CreateTexture(byte[] imageBytes, bool readable)
        {
            return Reporting(null, () => LoadTexture(imageBytes, readable));
        }

        private static Texture2D LoadTexture(byte[] imageBytes, bool readable)
        {
            if (imageBytes.IsNullOrEmpty())
            {
                throw new ArgumentException("Image data is null or empty", nameof(imageBytes));
            }

            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!tex.LoadImage(imageBytes))
            {
                UnityEngine.Object.Destroy(tex);
                throw new InvalidDataException($"Failed to decode image bytes ({imageBytes.Length} bytes), which may be corrupt or in an unsupported format");
            }
            tex.filterMode = FilterMode.Point;
            tex.anisoLevel = 1;
            tex.mipMapBias = 0;
            tex.wrapMode = TextureWrapMode.Clamp;

            // Textures always load as ARGB32 when loading from a png, so this is necessary to convert it to RGBA32
            Texture2D convertedTexture = new Texture2D(tex.width, tex.height, TextureFormat.RGBA32, false);
            convertedTexture.filterMode = FilterMode.Point;
            convertedTexture.anisoLevel = 1;
            convertedTexture.mipMapBias = 0;
            convertedTexture.wrapMode = TextureWrapMode.Clamp;
            if (readable)
            {
                convertedTexture.SetPixels32(tex.GetPixels32());
                convertedTexture.Apply();
            }
            else
            {
                // ConvertTexture fills only the GPU copy, so GetPixels on the result returns uninitialized data
                Graphics.ConvertTexture(tex, convertedTexture);
            }
            UnityEngine.Object.Destroy(tex);
            return convertedTexture;
        }

        /// <summary>
        /// Creates an AudioClip from an audio file.
        /// Loads AudioClip from cache if created previously.
        /// IMPORTANT: MP3 files will not load.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it or the named game asset can't be loaded or decoded.
        /// </summary>
        /// <param name="path">Path to the directory that contains the audio file</param>
        /// <param name="fileName">Filename of an audio file, including the extension</param>
        /// <returns></returns>
        public static AudioClip GetAudioClip(string path, string fileName)
        {
            return Reporting(path, () => LoadAudioClip(path, fileName));
        }

        internal static AudioClip LoadAudioClip(string path, string fileName)
        {
            // Get full path converts / to \ which is needed because WWW doesn't like /
            return LoadAudioClip(Path.GetFullPath(Path.Combine(path, fileName)));
        }

        /// <summary>
        /// Creates an AudioClip from an audio file.
        /// Loads AudioClip from cache if created previously.
        /// IMPORTANT: MP3 files will not load.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it or the named game asset can't be loaded or decoded.
        /// </summary>
        /// <param name="filePath">Path to an audio file</param>
        /// <returns></returns>
        public static AudioClip GetAudioClip(string filePath)
        {
            return Reporting(filePath, () => LoadAudioClip(filePath));
        }

        private static AudioClip LoadAudioClip(string filePath)
        {
            AudioClip result = null;
            if (audioClips.TryGetValue(filePath, out AudioClip cached) && cached)
            {
                return cached;
            }

            if (File.Exists(filePath))
            {
                result = LoadAudioClipFile(filePath);
            }
            else if (filePath.Contains(":") && filePath[1] != ':') // Second condition is here to make sure it is not a disk path
            {
                try
                {
                    result = LoadAssetSync<AudioClip>(filePath);
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to load asset-bundle audio clip '{filePath}': {ex.Message}", ex);
                }
            }
            else
            {
                throw new FileNotFoundException($"Audio file not found: '{filePath}'", filePath);
            }

            if (result != null)
            {
                audioClips[filePath] = result;
            }
            return result;
        }

        /// <summary>
        /// Loads an array of audio clips that follow a standard numbering pattern,
        /// such as "file0.wav", "file1.wav", "file2.wav"
        /// The first clip that fails to load is shown in BroMaker's Issues panel and its exception thrown, as GetAudioClip does.
        /// </summary>
        /// <param name="path">Path to the directory that contains the audio files</param>
        /// <param name="fileNameWithouExtension">Filename of the audio files without the extension</param>
        /// <param name="length">Number of files to load</param>
        /// <param name="startingNumber">Number of the first audio clip, defaults to 0</param>
        /// <param name="extension">Extension of the audio files, defaults to .wav</param>
        /// <returns></returns>
        public static AudioClip[] GetAudioClipArray(string path, string fileNameWithouExtension, int length, int startingNumber = 0, string extension = ".wav")
        {
            AudioClip[] clips = new AudioClip[length];
            for (int i = 0 + startingNumber; i < length + startingNumber; ++i)
            {
                clips[i - startingNumber] = GetAudioClip(path, fileNameWithouExtension + i + extension);
            }

            return clips;
        }

        /// <summary>
        /// Creates an AudioClip from an audio file.
        /// The AudioClip is not cached, use GetAudioClip is caching is desired.
        /// IMPORTANT: MP3 files will not load.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it can't be loaded or decoded.
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
        /// IMPORTANT: MP3 files will not load.
        /// A failure is shown in BroMaker's Issues panel and then thrown: `FileNotFoundException` when the file doesn't exist, `IOException` when it can't be loaded or decoded.
        /// </summary>
        /// <param name="filePath">Path to an audio file</param>
        /// <returns></returns>
        public static AudioClip CreateAudioClip(string filePath)
        {
            return Reporting(filePath, () => LoadAudioClipFile(filePath));
        }

        private static AudioClip LoadAudioClipFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Audio file not found: '{filePath}'", filePath);
            }
            WWW getClip = new WWW("file:////" + filePath);

            while (!getClip.isDone)
            {
            }
            ;

            if (getClip.error != null)
            {
                throw new IOException($"Failed to load AudioClip '{filePath}': {getClip.error}");
            }

            AudioClip result = getClip.GetAudioClip(false, false);
            if (result == null)
            {
                throw new IOException($"Failed to convert AudioClip from '{filePath}' (corrupt or unsupported format?)");
            }
            result.name = Path.GetFileNameWithoutExtension(filePath);
            return result;
        }

        private static T Reporting<T>(string path, Func<T> load)
        {
            try
            {
                return load();
            }
            catch (Exception ex)
            {
                BMLogger.Error(ex.Message, ex, BroMakerStorage.TryResolveModFromPath(path));
                throw;
            }
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
                int offset = 0;
                while (offset < ba.Length)
                {
                    int read = resFilestream.Read(ba, offset, ba.Length - offset);
                    if (read == 0)
                    {
                        break;
                    }
                    offset += read;
                }
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



using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BroMakerLib.Loggers;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Projectiles
{
    /// <summary>
    /// CustomSacehlPack has all the default functionality of the SachelPack class, with extra methods to make creating custom projectiles easier
    /// </summary>
    public class CustomSachelPack : SachelPack, ICustomProjectile
    {
        /// <summary>
        /// Override Awake() and set this value to false to prevent the sprite from being loaded automatically
        /// </summary>
        public bool spriteAutoLoad = true;

        /// <summary>
        /// Stores the sprite that was loaded if automatic loading is enabled
        /// </summary>
        public SpriteSM sprite = null;

        /// <summary>
        /// Stores all custom projectile prefabs that have been created to avoid having to recreate them.
        /// </summary>
        public static Dictionary<Type, CustomSachelPack> CustomSachelPackPrefabs = new Dictionary<Type, CustomSachelPack>();

        /// <summary>
        /// Tracks whether the prefab setup has been run or not.
        /// </summary>
        public bool RanSetup = false;

        /// <summary>
        /// Folder that contains your sprite image file. 
        /// Defaults to "projectiles". 
        /// Set this to "" if your sprite is in the same folder as your .dll
        /// </summary>
        public string spriteFolder = "projectiles";

        /// <summary>
        /// Name of your image file that will be automatically loaded.
        /// Defaults to "nameofyourclass.png".
        /// </summary>
        public string spriteFileName = string.Empty;

        /// <summary>
        /// Automatically gets set to the path to the folder your sprite is in.
        /// Don't set this manually.
        /// </summary>
        public string spriteDirectoryPath = string.Empty;

        /// <summary>
        /// Automatically gets set to the path to the folder your projectile's assembly is in.
        /// Don't set this manually.
        /// </summary>
        public string assemblyPath = string.Empty;

        /// <summary>
        /// Folder that contains your projectile's sounds.
        /// Defaults to "sounds". 
        /// Set this to "" if your sounds are in the same folder as your .dll
        /// </summary>
        public string soundFolder = "sounds";

        /// <summary>
        /// Automatically gets set to the path to the folder your projectile's sounds are in.
        /// Assumes they're in a folder called sounds
        /// </summary>
        public string soundPath = string.Empty;

        /// <summary>
        /// Size of one frame of your projectile. 
        /// By default this will be the whole image, but if your projectile has multiple frames, you will want to change this.
        /// </summary>
        public Vector2 spritePixelDimensions = Vector2.zero;

        /// <summary>
        /// Sets the lower left corner of your sprite.
        /// Defaults to (0, heightofyourimage), which will include the whole image.
        /// </summary>
        public Vector2 spriteLowerLeftPixel = Vector2.zero;

        /// <summary>
        /// Controls how wide your sprite appears in-game. This can be set to anything, regardless of the actual image size.
        /// Defaults to the same width as your image file.
        /// </summary>
        public float spriteWidth = -1f;

        /// <summary>
        /// Controls how tall your sprite appears in-game. This can be set to anything, regardless of the actual image size.
        /// Defaults to the same height as your image file.
        /// </summary>
        public float spriteHeight = -1f;

        /// <summary>
        /// Sets the offset of your sprite from the game object.
        /// Defaults to (0, 0, 0).
        /// </summary>
        public Vector3 spriteOffset = Vector3.zero;

        /// <summary>
        /// Sets the sprite color.
        /// Defaults to white.
        /// </summary>
        public Color spriteColor = Color.white;

        /// <summary>
        /// Sets the SoundHolder of the projectile.
        /// Defaults to Rambro's Bullet's SoundHolder.
        /// </summary>
        public SoundHolder defaultSoundHolder = null;

        /// <summary>
        /// You must override this function for automatic sprite loading to work.
        /// If you don't put any code in your Awake() function besides base.Awake(), the sprite loading will attempt to use the defaults to load your sprite.
        /// </summary>
        protected override void Awake()
        {
            string className = this.GetType().Name;
            try
            {
                // Only load sprite if we're creating a prefab and it hasn't been created yet.
                if (this.sprite == null && this.spriteAutoLoad)
                {
                    this.assemblyPath = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
                    this.spriteDirectoryPath = Path.Combine(assemblyPath, spriteFolder);
                    this.soundPath = Path.Combine(assemblyPath, soundFolder);

                    if (spriteFileName == string.Empty)
                    {
                        spriteFileName = className + ".png";
                    }

                    Material mat = ResourcesController.GetMaterial(spriteDirectoryPath, spriteFileName);
                    this.gameObject.GetComponent<MeshRenderer>().material = mat;
                    float imageWidth = mat.mainTexture.width;
                    float imageHeight = mat.mainTexture.height;

                    if (this.spritePixelDimensions == Vector2.zero)
                    {
                        this.spritePixelDimensions = new Vector2(imageWidth, imageHeight);
                    }
                    if (this.spriteLowerLeftPixel == Vector2.zero)
                    {
                        this.spriteLowerLeftPixel = new Vector2(0, imageHeight);
                    }
                    if (this.spriteWidth == -1)
                    {
                        this.spriteWidth = imageWidth;
                    }
                    if (this.spriteHeight == -1)
                    {
                        this.spriteHeight = imageHeight;
                    }

                    this.sprite = this.gameObject.GetComponent<SpriteSM>();
                    this.sprite.pixelDimensions = spritePixelDimensions;
                    this.sprite.lowerLeftPixel = this.spriteLowerLeftPixel;
                    this.sprite.width = this.spriteWidth;
                    this.sprite.height = this.spriteHeight;
                    this.sprite.offset = this.spriteOffset;
                    this.sprite.plane = SpriteBase.SPRITE_PLANE.XY;
                    this.sprite.color = this.spriteColor;
                }

                if (this.soundHolder == null)
                {
                    this.soundHolder = UnityEngine.Object.Instantiate(this.defaultSoundHolder ?? HeroController.GetHeroPrefab(HeroType.Rambro).projectile.soundHolder);
                    this.soundHolder.gameObject.SetActive(false);
                    this.soundHolder.gameObject.name = "SoundHolder " + className;
                    UnityEngine.Object.DontDestroyOnLoad(this.soundHolder);
                }

                base.Awake();


            }
            catch (Exception exception)
            {
                BMLogger.Log(className + " threw an exception in awake: " + exception.ToString());
            }
        }

        /// <summary>
        /// Runs one time after Awake() is called when the prefab is being created.
        /// Allows you to setup and store variables in the prefab to be reused by copies of this projectile.
        /// </summary>
        public virtual void PrefabSetup()
        {
        }

        /// <summary>
        /// Creates a prefab with the default required components for a Projectile.
        /// </summary>
        /// <typeparam name="T">Type of your custom projectile class.</typeparam>
        /// <returns>The prefab of your custom projectile.</returns>
        public static T CreatePrefab<T>() where T : CustomSachelPack
        {
            if (CustomSachelPackPrefabs.TryGetValue(typeof(T), out CustomSachelPack customProjectile) && customProjectile != null)
            {
                return customProjectile as T;
            }
            else
            {
                T prefab = new GameObject(typeof(T).Name, new Type[] { typeof(MeshFilter), typeof(MeshRenderer), typeof(SpriteSM), typeof(T) }).GetComponent<T>();
                prefab.PrefabSetup();
                prefab.RanSetup = true;
                prefab.enabled = false;
                prefab.gameObject.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(prefab.gameObject);
                CustomSachelPackPrefabs.Add(typeof(T), prefab);
                return prefab;
            }
        }

        /// <summary>
        /// Creates a prefab with the default required components for a Projectile along with the additional specified components.
        /// </summary>
        /// <param name="additionalComponents">Additional components to add to the prefab.</param>
        /// <typeparam name="T">Type of your custom projectile class.</typeparam>
        /// <returns>The prefab of your custom projectile.</returns>
        public static T CreatePrefab<T>(List<Type> additionalComponents) where T : CustomSachelPack
        {
            if (CustomSachelPackPrefabs.TryGetValue(typeof(T), out CustomSachelPack customProjectile) && customProjectile != null)
            {
                return customProjectile as T;
            }
            else
            {
                Type[] components = (new List<Type> { typeof(MeshFilter), typeof(MeshRenderer), typeof(SpriteSM), typeof(T) }).Concat(additionalComponents).ToArray();
                T prefab = new GameObject(typeof(T).Name, components).GetComponent<T>();
                prefab.PrefabSetup();
                prefab.RanSetup = true;
                prefab.enabled = false;
                prefab.gameObject.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(prefab.gameObject);
                CustomSachelPackPrefabs.Add(typeof(T), prefab);
                return prefab;
            }
        }

        /// <summary>
        /// Spawns a projectile locally and enables it after spawning.
        /// </summary>
        /// <param name="FiredBy">The MonoBehaviour that fired this projectile.</param>
        /// <param name="x">The X position to spawn the projectile.</param>
        /// <param name="y">The Y position to spawn the projectile.</param>
        /// <param name="xI">The initial X velocity of the projectile.</param>
        /// <param name="yI">The initial Y velocity of the projectile.</param>
        /// <param name="playerNum">The player number who owns this projectile.</param>
        /// <param name="_zOffset">The Z-axis offset for the projectile spawn position.</param>
        /// <returns>The spawned Projectile instance.</returns>
        public virtual Projectile SpawnProjectileLocally(MonoBehaviour FiredBy, float x, float y, float xI, float yI, int playerNum, float _zOffset = 0f)
        {
            Projectile projectile = ProjectileController.SpawnProjectileLocally(this, FiredBy, x, y, xI, yI, playerNum, false, _zOffset);
            projectile.enabled = true;
            return projectile;
        }
    }
}

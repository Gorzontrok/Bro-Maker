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
    /// CustomGrenade has all the default functionality of the Grenade class, with extra methods to make creating custom grenades easier
    /// </summary>
    public class CustomGrenade : Grenade, ICustomProjectile
    {
        /// <summary>
        /// Override Awake() and set this value to false to prevent the sprite from being loaded automatically
        /// </summary>
        public bool spriteAutoLoad = true;

        /// <summary>
        /// Stores the sprite that was loaded if automatic loading is enabled
        /// </summary>
        public SpriteSM storedSprite = null;

        /// <summary>
        /// Stores all custom projectile prefabs that have been created to avoid having to recreate them.
        /// </summary>
        public static Dictionary<Type, CustomGrenade> CustomGrenadePrefabs = new Dictionary<Type, CustomGrenade>();

        /// <summary>
        /// Tracks whether the prefab setup has been run or not.
        /// </summary>
        public bool RanSetup = false;

        /// <summary>
        /// Folder that contains your sprite image file. 
        /// Defaults to "projectiles". 
        /// Set this to "" if your grenades are in the same folder as your .dll
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
        /// Automatically gets set to the path to the folder your grenade's assembly is in.
        /// Don't set this manually.
        /// </summary>
        public string assemblyPath = string.Empty;

        /// <summary>
        /// Folder that contains your grenade's sounds.
        /// Defaults to "sounds". 
        /// Set this to "" if your sounds are in the same folder as your .dll
        /// </summary>
        public string soundFolder = "sounds";

        /// <summary>
        /// Automatically gets set to the path to the folder your grenade's sounds are in.
        /// Assumes they're in a folder called sounds
        /// </summary>
        public string soundPath = string.Empty;

        /// <summary>
        /// Size of one frame of your grenade. 
        /// By default this will be the whole image, but if your grenade has multiple frames, you will want to change this.
        /// </summary>
        public Vector2 spritePixelDimensions = Vector2.zero;

        /// <summary>
        /// Sets the lower left corner of your sprite.
        /// Defaults to (0, heightofyourimage), which will include the whole image.
        /// </summary>
        public Vector2 spriteLowerLeftPixel = Vector2.zero;

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
        /// Sets the SoundHolder of the grenade.
        /// Defaults to Rambro's Grenade's SoundHolder.
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
                if (this.storedSprite == null && this.spriteAutoLoad)
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
                    if (this.spriteWidth == 2)
                    {
                        this.spriteWidth = imageWidth;
                    }
                    if (this.spriteHeight == 2)
                    {
                        this.spriteHeight = imageHeight;
                    }

                    this.storedSprite = this.gameObject.GetComponent<SpriteSM>();
                    this.storedSprite.pixelDimensions = spritePixelDimensions;
                    this.storedSprite.lowerLeftPixel = this.spriteLowerLeftPixel;
                    this.storedSprite.width = this.spriteWidth;
                    this.storedSprite.height = this.spriteHeight;
                    this.storedSprite.offset = this.spriteOffset;
                    this.storedSprite.plane = SpriteBase.SPRITE_PLANE.XY;
                    this.storedSprite.color = this.spriteColor;
                }

                this.sprite = this.storedSprite;

                if (this.soundHolder == null)
                {
                    this.soundHolder = UnityEngine.Object.Instantiate(this.defaultSoundHolder ?? HeroController.GetHeroPrefab(HeroType.Rambro).specialGrenade.soundHolder);
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
        /// Creates a prefab with the default required components for a Grenade.
        /// </summary>
        /// <typeparam name="T">Type of your custom grenade class.</typeparam>
        /// <returns>The prefab of your custom grenade.</returns>
        public static T CreatePrefab<T>() where T : CustomGrenade
        {
            if (CustomGrenadePrefabs.TryGetValue(typeof(T), out CustomGrenade customGrenade) && customGrenade != null)
            {
                return customGrenade as T;
            }
            else
            {
                T prefab = new GameObject(typeof(T).Name, new Type[] { typeof(MeshFilter), typeof(MeshRenderer), typeof(SpriteSM), typeof(T) }).GetComponent<T>();
                prefab.PrefabSetup();
                prefab.RanSetup = true;
                prefab.enabled = false;
                prefab.gameObject.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(prefab.gameObject);
                CustomGrenadePrefabs.Add(typeof(T), prefab);
                return prefab;
            }
        }

        /// <summary>
        /// Creates a prefab with the default required components for a Grenade along with the additional specified components.
        /// </summary>
        /// <param name="additionalComponents">Additional components to add to the prefab.</param>
        /// <typeparam name="T">Type of your custom grenade class.</typeparam>
        /// <returns>The prefab of your custom grenade.</returns>
        public static T CreatePrefab<T>(List<Type> additionalComponents) where T : CustomGrenade
        {
            if (CustomGrenadePrefabs.TryGetValue(typeof(T), out CustomGrenade customGrenade) && customGrenade != null)
            {
                return customGrenade as T;
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
                CustomGrenadePrefabs.Add(typeof(T), prefab);
                return prefab;
            }
        }

        /// <summary>
        /// Spawns a grenade locally and enables it after spawning.
        /// </summary>
        /// <param name="firedBy">The MonoBehaviour that threw this grenade.</param>
        /// <param name="x">The X position to spawn the grenade.</param>
        /// <param name="y">The Y position to spawn the grenade.</param>
        /// <param name="radius">The explosion radius of the grenade (passed to SetupGrenade but actual usage depends on grenade type).</param>
        /// <param name="force">The explosion force of the grenade (passed to SetupGrenade but actual usage depends on grenade type).</param>
        /// <param name="xI">The initial X velocity of the grenade.</param>
        /// <param name="yI">The initial Y velocity of the grenade.</param>
        /// <param name="playerNum">The player number who owns this grenade.</param>
        /// <param name="seed">Random seed for deterministic grenade behavior. Defaults to UnityEngine.Random.Range( 0, 10000 )</param>
        /// <returns>The spawned Grenade instance.</returns>
        public virtual Grenade SpawnGrenadeLocally(MonoBehaviour firedBy, float x, float y, float radius, float force, float xI, float yI, int playerNum, int seed = -1)
        {
            if (seed == -1)
            {
                seed = UnityEngine.Random.Range(0, 10000);
            }
            Grenade grenade = ProjectileController.SpawnGrenadeLocally(this, firedBy, x, y, radius, force, xI, yI, playerNum, seed);
            grenade.enabled = true;
            return grenade;
        }
    }
}

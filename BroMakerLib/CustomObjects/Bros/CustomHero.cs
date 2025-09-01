using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Bros
{
    [HeroPreset("CustomHero", HeroType.Rambro)]
    public class CustomHero : BroBase, ICustomHero
    {
        /// <summary>
        /// Different types of SoundHolderVoice that are available in the base game
        /// </summary>
        public enum SoundHolderVoiceTypes
        {
            None,
            MaleDeep,
            MaleLight,
            MaleRough,
            MaleFlyInsect,
            Female,
            ChevBrolios,
            ChevBroliosDrowsy
        }

        [Syncronize]
        public CustomBroInfo Info { get; set; }
        [Syncronize]
        public BroBase Character { get; set; }
        [JsonIgnore]
        public MuscleTempleFlexEffect FlexEffect { get; set; }

        [JsonIgnore]
        public int CurrentVariant { get; set; }
        [JsonIgnore]
        public Vector2 CurrentGunSpriteOffset { get; set; }
        [JsonIgnore]
        public List<Material> CurrentSpecialMaterials { get; set; }
        [JsonIgnore]
        public Vector2 CurrentSpecialMaterialOffset { get; set; }
        [JsonIgnore]
        public float CurrentSpecialMaterialSpacing { get; set; }
        [JsonIgnore]
        public Material CurrentFirstAvatar { get; set; }

        /// <summary>
        /// Set this to control which hero is used for the default soundHolder.
        /// If set to none, the base HeroType of your bro will be used.
        /// </summary>
        public HeroType SoundHolderHeroType = HeroType.None;

        /// <summary>
        /// Set this to control which hero is used for the default SoundHolderVoice. 
        /// If set to none, the SoundHolderHeroType will be used, unless that is set to None,
        /// in which case the base HeroType of your bro will be used.
        /// </summary>
        public SoundHolderVoiceTypes SoundHolderVoiceType = SoundHolderVoiceTypes.None;

        /// <summary>
        /// Contains the path to the directory that contains your custom bro's dll
        /// </summary>
        public string DirectoryPath
        {
            get => _directoryPath;
        }

        /// <summary>
        /// Contains the path to your bro's sound folder (assumes the folder is named "sounds" by default)
        /// </summary>
        public string SoundPath
        {
            get => _soundPath;
        }

        /// <summary>
        /// Contains the path to your bro's projectile folder (assumes the folder is named "projectiles" by default)
        /// </summary>
        public string ProjectilePath
        {
            get => _projectilePath;
        }

        [SerializeField]
        private string _directoryPath;
        [SerializeField]
        private string _soundPath;
        [SerializeField]
        private string _projectilePath;

        #region BroBase Methods
        protected override void Awake()
        {
            Character = this;
            Info = LoadHero.currentInfo;
            try
            {
                EnableSyncing(true, true);

                // Select variant
                this.CurrentVariant = this.GetVariant();

                // Cache current variant parameters
                this.CurrentGunSpriteOffset = BroMakerUtilities.GetVariantValue(this.Info.GunSpriteOffset, this.CurrentVariant);
                this.CurrentSpecialMaterials = BroMakerUtilities.GetVariantValue(this.Info.SpecialMaterials, this.CurrentVariant);
                this.CurrentSpecialMaterialOffset = BroMakerUtilities.GetVariantValue(this.Info.SpecialMaterialOffset, this.CurrentVariant);
                this.CurrentSpecialMaterialSpacing = BroMakerUtilities.GetVariantValue(this.Info.SpecialMaterialSpacing, this.CurrentVariant);
                this.CurrentFirstAvatar = BroMakerUtilities.GetVariantValue(this.Info.FirstAvatar, this.CurrentVariant);

                this.Character.specialGrenade.playerNum = LoadHero.playerNum;

                Info.BeforeAwake(this);
                base.Awake();
                this.SetSprites();
                Info.AfterAwake(this);

                // Make sure parachute isn't null, for some reason the game's default way of handling this doesn't work
                if (this.parachute == null)
                {
                    Parachute parachute = null;
                    for (int i = 0; i < this.transform.childCount; ++i)
                    {
                        if ((parachute = this.transform.GetChild(i).GetComponent<Parachute>()) != null)
                        {
                            this.parachute = parachute;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
                enabled = false;
            }
        }

        protected override void Start()
        {
            try
            {
                Info.BeforeStart(this);
                base.Start();
                Info.AfterStart(this);
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
                enabled = false;
            }
        }

        // This function is overridden to remove the RPC calls, since they don't work currently
        protected override void CheckForTraps(ref float yIT)
        {
            float num = base.Y + yIT;
            if (num <= this.groundHeight + 1f)
            {
                num = this.groundHeight + 1f;
            }
            if (Map.isEditing || this.invulnerable)
            {
                return;
            }
            if (!base.IsEnemy && !base.IsMine)
            {
                return;
            }
            DoodadAcidPool nearestAcid = Map.GetNearestAcid(base.X, base.Y + 8f, 2f);
            if (nearestAcid != null && nearestAcid.fullness > 0.2f)
            {
                this.CoverInAcid();
            }
            if (this.impaledByTransform == null && base.IsHero && ((this.yI > 50f && (this.canTouchRightWalls || this.canTouchLeftWalls || this.WallClimbing) && (Time.time - this.lastJumpTime > 0.2f || base.Y > this.groundHeight + 17f)) || this.yI < -120f) && this.IsSurroundedByBarbedWire())
            {
                EffectsController.CreateBloodParticles(this.bloodColor, base.X, base.Y + 10f, -5f, 1, 4f, 4f, 50f, this.xI * 0.8f, 70f);
                if (this.yI < 0f)
                {
                    this.yI *= 0.2f;
                }
                else
                {
                    this.yI *= 0.45f;
                }
                this.barbedWireWithin.ForceBloody();
                this.barbedWireWithin.PlayCutSound();
            }
            RaycastHit raycastHit;
            if (this.impaledByTransform == null && Physics.Raycast(new Vector3(base.X, num, 0f), Vector3.down, out raycastHit, 25f, this.groundLayer))
            {
                Block component = raycastHit.collider.GetComponent<Block>();
                if (component != null)
                {
                    if (raycastHit.distance < 10f && (base.IsMine || base.IsEnemy))
                    {
                        component.CheckForMine();
                    }
                    if (component.spikes != null && !this.invulnerable && !this.wallDrag)
                    {
                        if (component.spikes.EvaluateImpalent(this))
                        {
                            float num2 = (base.playerNum >= 0) ? component.spikes.spikeTrapHarmlessHeight : (component.spikes.spikeTrapHarmlessHeight * 0.5f);
                            if (this.yI < -150f && raycastHit.point.y > this.groundHeight - 1f && raycastHit.distance >= num2 && raycastHit.distance < component.spikes.spikeTrapHeight && base.Y > component.Y + component.spikes.spikeTrapHeight)
                            {
                                base.Y = Mathf.Clamp(base.Y, this.groundHeight + 2f, this.groundHeight + 3f);
                                yIT = 0f;
                                //Networking.RPC<TestVanDammeAnim>(PID.TargetAll, new RpcSignature<TestVanDammeAnim>(component.spikes.ImpaleUnit), this, false);
                                component.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component.spikes.IsBarbedWire(this) && component.spikes.collumn == component.collumn && base.Y < raycastHit.point.y + 12f && Mathf.Abs(this.yI) < 50f && Mathf.Abs(this.xI + this.xIBlast) > this.GetSpeed - 2f && (int)Mathf.Sign(this.xI + this.xIBlast) == ((!this.left) ? 0 : -1) + ((!this.right) ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(this.bloodColor, base.X, base.Y + 10f, -5f, 1, 4f, 4f, 50f, this.xI * 0.8f, 70f);
                            this.xIBlast -= this.xI * 0.4f;
                            this.xI = 0f;
                            component.spikes.ForceBloody();
                            component.spikes.PlayCutSound();
                        }
                    }
                }
            }
            RaycastHit raycastHit2;
            if (this.impaledByTransform == null && Physics.Raycast(new Vector3(base.X - 3f, num, 0f), Vector3.down, out raycastHit2, 25f, this.groundLayer))
            {
                Block component2 = raycastHit2.collider.GetComponent<Block>();
                if (component2 != null)
                {
                    if (raycastHit2.distance < 10f && (base.IsMine || base.IsEnemy))
                    {
                        component2.CheckForMine();
                    }
                    if (component2.spikes != null && !this.invulnerable && !this.wallDrag)
                    {
                        if (component2.spikes.EvaluateImpalent(this))
                        {
                            float num3 = (base.playerNum >= 0) ? component2.spikes.spikeTrapHarmlessHeight : (component2.spikes.spikeTrapHarmlessHeight * 0.5f);
                            if (this.yI < -150f && raycastHit2.point.y > this.groundHeight - 1f && raycastHit2.distance >= num3 && raycastHit2.distance < component2.spikes.spikeTrapHeight && base.Y > component2.Y + component2.spikes.spikeTrapHeight)
                            {
                                base.Y = Mathf.Clamp(base.Y, this.groundHeight + 2f, this.groundHeight + 3f);
                                yIT = 0f;
                                //Networking.RPC<TestVanDammeAnim>(PID.TargetAll, new RpcSignature<TestVanDammeAnim>(component2.spikes.ImpaleUnit), this, false);
                                component2.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component2.spikes.IsBarbedWire(this) && component2.spikes.collumn == component2.collumn && base.Y < raycastHit2.point.y + 12f && Mathf.Abs(this.yI) < 50f && Mathf.Abs(this.xI + this.xIBlast) > this.GetSpeed - 2f && (int)Mathf.Sign(this.xI + this.xIBlast) == ((!this.left) ? 0 : -1) + ((!this.right) ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(this.bloodColor, base.X, base.Y + 10f, -5f, 1, 4f, 4f, 50f, this.xI * 0.8f, 70f);
                            this.xIBlast -= this.xI * 0.4f;
                            this.xI = 0f;
                            component2.spikes.ForceBloody();
                            component2.spikes.PlayCutSound();
                        }
                    }
                }
            }
            RaycastHit raycastHit3;
            if (this.impaledByTransform == null && Physics.Raycast(new Vector3(base.X + 3f, num, 0f), Vector3.down, out raycastHit3, 25f, this.groundLayer))
            {
                Block component3 = raycastHit3.collider.GetComponent<Block>();
                if (component3 != null)
                {
                    if (raycastHit3.distance < 10f && (base.IsMine || base.IsEnemy))
                    {
                        component3.CheckForMine();
                    }
                    if (component3.spikes != null && !this.invulnerable && !this.wallDrag)
                    {
                        if (component3.spikes.EvaluateImpalent(this))
                        {
                            float num4 = (base.playerNum >= 0) ? component3.spikes.spikeTrapHarmlessHeight : (component3.spikes.spikeTrapHarmlessHeight * 0.5f);
                            if (this.yI < -150f && raycastHit3.point.y > this.groundHeight - 1f && raycastHit3.distance >= num4 && raycastHit3.distance < component3.spikes.spikeTrapHeight && base.Y > component3.Y + component3.spikes.spikeTrapHeight)
                            {
                                base.Y = Mathf.Clamp(base.Y, this.groundHeight + 2f, this.groundHeight + 3f);
                                yIT = 0f;
                                //Networking.RPC<TestVanDammeAnim>(PID.TargetAll, new RpcSignature<TestVanDammeAnim>(component3.spikes.ImpaleUnit), this, false);
                                component3.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component3.spikes.IsBarbedWire(this) && component3.spikes.collumn == component3.collumn && base.Y < raycastHit3.point.y + 12f && Mathf.Abs(this.yI) < 50f && Mathf.Abs(this.xI + this.xIBlast) > this.GetSpeed - 2f && (int)Mathf.Sign(this.xI + this.xIBlast) == ((!this.left) ? 0 : -1) + ((!this.right) ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(this.bloodColor, base.X, base.Y + 10f, -5f, 1, 4f, 4f, 50f, this.xI * 0.8f, 70f);
                            this.xIBlast -= this.xI * 0.4f;
                            this.xI = 0f;
                            component3.spikes.ForceBloody();
                            component3.spikes.PlayCutSound();
                        }
                    }
                }
            }
        }

        protected override void TriggerFlexEvent()
        {
            if (this.player.HasFlexPower(PickupType.FlexAlluring))
            {
                Map.AttractMooks(base.X, base.Y, 96f, 30f);
            }
            if (this.player.HasFlexPower(PickupType.FlexGoldenLight))
            {
                if (FlexEffect == null)
                {
                    this.FlexEffect = Traverse.Create((this as BroBase)).GetFieldValue("flexEffect") as MuscleTempleFlexEffect;
                }
                if (this.FlexEffect != null)
                {
                    this.FlexEffect.PlaySoundEffect();
                }
                if (base.IsMine)
                {
                    int num = 8 + UnityEngine.Random.Range(0, 5);
                    for (int i = 0; i < num; i++)
                    {
                        float angle = -1.88495576f + 1.2f / (float)(num - 1) * 3.14159274f * (float)i;
                        Vector2 vector = global::Math.Point2OnCircle(angle, 1f);
                        ProjectileController.SpawnProjectileLocally(ProjectileController.instance.goldenLightProjectile, this, base.X, base.Y + 12f, vector.x * 400f, vector.y * 400f, true, 15, false, true, -15f);
                    }
                }
            }
            else if (this.player.HasFlexPower(PickupType.FlexInvulnerability) && this.FlexEffect != null)
            {
                this.FlexEffect.PlaySoundEffect();
            }
        }

        protected override void SetGunPosition(float xOffset, float yOffset)
        {
            this.gunSprite.transform.localPosition = new Vector3(xOffset + CurrentGunSpriteOffset.x, yOffset + CurrentGunSpriteOffset.y, -.001f);
        }
        #endregion

        #region Custom Methods
        /// <summary>
        /// Override this method to have UI options displayed for your custom hero underneath their name in the Custom Bros tab
        /// </summary>
        public virtual void UIOptions()
        {

        }

        /// <summary>
        /// Override this method to add custom harmony patches to your custom hero
        /// </summary>
        /// <param name="harmony"></param>
        public virtual void HarmonyPatches(Harmony harmony)
        {
        }

        /// <summary>
        /// Override this method to add your own assets to be preloaded when the game starts, to avoid lag when spawning as custom characters.
        /// </summary>
        public virtual void PreloadAssets()
        {
        }

        /// <summary>
        /// Preloads each sprite in the spritePaths list.
        /// </summary>
		/// <param name="directoryPath">Path to the directory containing the sprites</param>
        /// <param name="spritePaths">Sprites to load</param>
        public static void PreloadSprites(string directoryPath, List<string> spritePaths)
        {
            for (int i = 0; i < spritePaths.Count; ++i)
            {
                ResourcesController.GetMaterial(directoryPath, spritePaths[i]);
            }
        }

        /// <summary>
        /// Preloads each sound in the soundPaths list.
        /// </summary>
		/// <param name="directoryPath">Path to the directory containing the sound files</param>
        /// <param name="soundPaths">Sounds to load</param>
        public static void PreloadSounds(string directoryPath, List<string> soundPaths)
        {
            for (int i = 0; i < soundPaths.Count; ++i)
            {
                ResourcesController.GetAudioClip(directoryPath, soundPaths[i]);
            }
        }

        /// <summary>
        /// This method is called once after the prefab is created and before PrefabSetup has run. You can override this to set SoundHolderHeroType or SoundHolderVoiceType
        /// to control which heros your default soundHolder components are initialized from.
        /// </summary>
        public virtual void BeforePrefabSetup()
        {
        }

        /// <summary>
        /// This method is called once after the prefab is created and after PrefabSetup has run. You can override this to add additional variables to be set up in your prefab.
        /// The soundHolder, SoundHolderVoice, and soundHolderFootSteps will have been set up at this point, as well as directoryPath, so you can load sounds and assign them to
        /// these soundHolders to set up your bro's sounds.
        /// </summary>
        public virtual void AfterPrefabSetup()
        {
        }

        internal void PrefabSetup()
        {
            this.BeforePrefabSetup();
            HeroType baseHeroType = this.SoundHolderHeroType;
            // Use base hero type if SoundHolderHeroType is unassigned
            if (baseHeroType == HeroType.None)
            {
                baseHeroType = LoadHero.GetBaseHeroTypeOfPreset(this.GetType());
            }
            this.soundHolder = UnityEngine.Object.Instantiate(HeroController.GetHeroPrefab(baseHeroType).soundHolder);
            this.soundHolder.gameObject.SetActive(false);
            this.soundHolder.gameObject.name = "SoundHolder " + this.name;
            UnityEngine.Object.DontDestroyOnLoad(this.soundHolder);

            if (this.SoundHolderVoiceType == SoundHolderVoiceTypes.None)
            {
                this.soundHolderVoice = UnityEngine.Object.Instantiate((HeroController.GetHeroPrefab(baseHeroType) as BroBase).soundHolderVoice);
            }
            else
            {
                switch (this.SoundHolderVoiceType)
                {
                    case SoundHolderVoiceTypes.MaleDeep:
                        this.soundHolderVoice = UnityEngine.Object.Instantiate((HeroController.GetHeroPrefab(HeroType.Rambro) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.MaleLight:
                        this.soundHolderVoice = UnityEngine.Object.Instantiate((HeroController.GetHeroPrefab(HeroType.McBrover) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.MaleRough:
                        this.soundHolderVoice = UnityEngine.Object.Instantiate((HeroController.GetHeroPrefab(HeroType.SnakeBroSkin) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.MaleFlyInsect:
                        this.soundHolderVoice = UnityEngine.Object.Instantiate((HeroController.GetHeroPrefab(HeroType.BrondleFly) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.Female:
                        this.soundHolderVoice = UnityEngine.Object.Instantiate((HeroController.GetHeroPrefab(HeroType.EllenRipbro) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.ChevBrolios:
                        this.soundHolderVoice = UnityEngine.Object.Instantiate((HeroController.GetHeroPrefab(HeroType.ChevBrolios) as ChevBrolios).arousedVoiceHolder);
                        break;
                    case SoundHolderVoiceTypes.ChevBroliosDrowsy:
                        this.soundHolderVoice = UnityEngine.Object.Instantiate((HeroController.GetHeroPrefab(HeroType.ChevBrolios) as ChevBrolios).drowsyVoiceHolder);
                        break;
                }
            }
            this.soundHolderVoice.gameObject.SetActive(false);
            this.soundHolderVoice.gameObject.name = "SoundHolderVoice " + this.name;
            UnityEngine.Object.DontDestroyOnLoad(this.soundHolderVoice);

            this.soundHolderFootSteps = UnityEngine.Object.Instantiate(HeroController.GetHeroPrefab(baseHeroType).soundHolderFootSteps);
            this.soundHolderFootSteps.gameObject.SetActive(false);
            this.soundHolderFootSteps.gameObject.name = "SoundHolderFootSteps " + this.name;
            UnityEngine.Object.DontDestroyOnLoad(this.soundHolderFootSteps);

            this.AssignDirectoryPaths(LoadHero.currentInfo.path);
            try
            {
                this.LoadSettings();
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog("Failed to load settings in SetupPrefab: ", ex);
            }

            this.Character = this;
            this.Info = LoadHero.currentInfo;

            // Setup CustomHero from original bro component and destroy it
            this.SetupCustomHero();

            this.AfterPrefabSetup();
        }

        /// <summary>
        /// Sets up the DirectoryPath, SoundPath, and ProjectilePath variables.
        /// You can override this to adjust the paths if your sounds / projectiles folders 
        /// are named something other than "sounds" and "projectiles"
        /// </summary>
        /// <param name="directoryPath">Path to the directory that contains your custom bro's .dll</param>
        public virtual void AssignDirectoryPaths(string directoryPath)
        {
            this._directoryPath = directoryPath;
            this._soundPath = Path.Combine(directoryPath, "sounds");
            this._projectilePath = Path.Combine(directoryPath, "projectiles");
        }

        /// <summary>
        /// Override this to customize variant selection logic
        /// </summary>
        public virtual int GetVariant()
        {
            if (Info.VariantCount <= 1)
            {
                return 0;
            }
            return UnityEngine.Random.Range(0, Info.VariantCount);
        }

        /// <summary>
        /// Override this if you have additional sprites that need to be set up and showing immediately
        /// </summary>
        public virtual void SetupAdditionalSprites()
        {
        }

        /// <summary>
        /// This method switches the current variant and changes all assigned parameters to the new variant's values
        /// </summary>
        /// <param name="variant">Variant to switch to</param>
        public virtual void SwitchVariant(int variant)
        {
            this.CurrentVariant = variant;
            this.CurrentGunSpriteOffset = BroMakerUtilities.GetVariantValue(this.Info.GunSpriteOffset, this.CurrentVariant);
            this.CurrentSpecialMaterials = BroMakerUtilities.GetVariantValue(this.Info.SpecialMaterials, this.CurrentVariant);
            this.CurrentSpecialMaterialOffset = BroMakerUtilities.GetVariantValue(this.Info.SpecialMaterialOffset, this.CurrentVariant);
            this.CurrentSpecialMaterialSpacing = BroMakerUtilities.GetVariantValue(this.Info.SpecialMaterialSpacing, this.CurrentVariant);
            this.CurrentFirstAvatar = BroMakerUtilities.GetVariantValue(this.Info.FirstAvatar, this.CurrentVariant);

            this.SetSprites();
            BroMakerUtilities.SetSpecialMaterials(this.playerNum, this.CurrentSpecialMaterials, this.CurrentSpecialMaterialOffset, this.CurrentSpecialMaterialSpacing);
            HeroController.SetAvatarMaterial(playerNum, this.CurrentFirstAvatar);
        }
        #endregion

        #region Save / Load Settings
        /// <summary>
        /// Marks a static field or property to be saved/loaded as a persistent setting.
        /// Only static members marked with this attribute will be included in settings serialization.
        /// </summary>
        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class SaveableSettingAttribute : Attribute
        {
            /// <summary>
            /// Gets or sets the custom name to use as the JSON key for this setting.
            /// If not specified, the member name will be used.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Initializes a new instance of the SaveableSettingAttribute class.
            /// </summary>
            /// <param name="name">Optional custom name to use as the JSON key. If null, the member name will be used.</param>
            public SaveableSettingAttribute(string name = null)
            {
                Name = name;
            }
        }

        private static readonly Dictionary<Type, bool> _hasSaveableFieldsCache = new Dictionary<Type, bool>();
        private static readonly Dictionary<Type, List<MemberInfo>> _saveableMembersCache = new Dictionary<Type, List<MemberInfo>>();
        private static readonly Dictionary<Type, string> _typeDirectoryCache = new Dictionary<Type, string>();

        /// <summary>
        /// Saves all static fields and properties marked with [SaveableSetting] to a JSON file.
        /// The file will be saved to the instance's directoryPath with the name format: {BroName}settings.json
        /// </summary>
        /// <remarks>
        /// - Only saves non-null values
        /// - Automatically caches the custom bro information for better performance on subsequent calls
        /// - Caches the directory path for use with SaveAll()
        /// - If no saveable fields exist, no file will be created
        /// </remarks>
        public virtual void SaveSettings()
        {
            var type = GetType();

            // Cache the directory path for this type
            _typeDirectoryCache[type] = this.DirectoryPath;

            if (!HasSaveableFields(type))
                return;

            var saveableMembers = GetSaveableMembers(type);
            var data = new Dictionary<string, object>();

            foreach (var member in saveableMembers)
            {
                string key = null;
                object value = null;

                SaveableSettingAttribute attr;
                if (member is FieldInfo field)
                {
                    attr = (SaveableSettingAttribute)field.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                    key = attr?.Name ?? field.Name;
                    value = field.GetValue(null);
                }
                else if (member is PropertyInfo property)
                {
                    attr = (SaveableSettingAttribute)property.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                    key = attr?.Name ?? property.Name;
                    value = property.GetValue(null, null);
                }

                if (value != null && key != null)
                    data[key] = value;
            }

            if (data.Count > 0)
            {
                var fileName = $"{type.Name}settings.json";
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(Path.Combine(DirectoryPath, fileName), json);
            }
        }

        /// <summary>
        /// Loads all static fields and properties marked with [SaveableSetting] from a JSON file.
        /// The file is expected to be in the instance's directoryPath with the name format: {BroName}settings.json
        /// </summary>
        /// <remarks>
        /// - Silently returns if the settings file doesn't exist
        /// - Automatically handles type conversion including enums and complex types
        /// - Logs errors for individual fields that fail to load but continues with other fields
        /// - Caches the directory path for use with SaveAll()
        /// - Uses cached custom bro information for better performance on subsequent calls
        /// </remarks>
        public virtual void LoadSettings()
        {
            var type = GetType();

            // Cache the directory path for this type
            _typeDirectoryCache[type] = this.DirectoryPath;

            if (!HasSaveableFields(type))
                return;

            var fileName = Path.Combine(this.DirectoryPath, $"{type.Name}settings.json");
            if (!File.Exists(fileName))
                return;

            try
            {
                string json = File.ReadAllText(fileName);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (data == null)
                    return;

                var saveableMembers = GetSaveableMembers(type);

                foreach (var member in saveableMembers)
                {
                    SaveableSettingAttribute attr = null;
                    string key = null;

                    if (member is FieldInfo)
                    {
                        FieldInfo field = (FieldInfo)member;
                        attr = (SaveableSettingAttribute)field.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                        key = attr?.Name ?? field.Name;

                        if (data.ContainsKey(key))
                        {
                            try
                            {
                                var value = data[key];
                                var convertedValue = ConvertValue(value, field.FieldType);
                                field.SetValue(null, convertedValue);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading {key}: {ex.Message}");
                            }
                        }
                    }
                    else if (member is PropertyInfo)
                    {
                        var property = (PropertyInfo)member;
                        if (property.CanWrite)
                        {
                            attr = (SaveableSettingAttribute)property.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                            key = attr?.Name ?? property.Name;

                            if (data.ContainsKey(key))
                            {
                                try
                                {
                                    var value = data[key];
                                    var convertedValue = ConvertValue(value, property.PropertyType);
                                    property.SetValue(null, convertedValue, null);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error loading {key}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings from {fileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves settings for a specific custom bro type without requiring an instance.
        /// </summary>
        /// <typeparam name="T">The custom bro type whose settings should be saved</typeparam>
        /// <remarks>
        /// - If the directory path is not cached, retrieves it from BroMakerStorage
        /// - Only saves non-null values
        /// - Creates or overwrites the settings file in the appropriate directory
        /// </remarks>
        public static void SaveSettings<T>() where T : CustomHero
        {
            var type = typeof(T);

            // Get or retrieve the directory path
            string directoryPath;
            if (!_typeDirectoryCache.TryGetValue(type, out directoryPath))
            {
                directoryPath = Storages.BroMakerStorage.GetStoredHeroByCustomHeroType<T>().GetInfo().path;
                if (string.IsNullOrEmpty(directoryPath))
                {
                    Console.WriteLine($"Error: Could not determine directory path for custom bro {type.Name}");
                    return;
                }
                _typeDirectoryCache[type] = directoryPath;
            }

            // Get saveable members
            var saveableMembers = GetSaveableMembers(type);
            if (saveableMembers.Count == 0)
                return;

            var data = new Dictionary<string, object>();

            foreach (var member in saveableMembers)
            {
                string key = null;
                object value = null;

                SaveableSettingAttribute attr;
                if (member is FieldInfo field)
                {
                    attr = (SaveableSettingAttribute)field.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                    key = attr?.Name ?? field.Name;
                    value = field.GetValue(null);
                }
                else if (member is PropertyInfo property)
                {
                    attr = (SaveableSettingAttribute)property.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                    key = attr?.Name ?? property.Name;
                    value = property.GetValue(null, null);
                }

                if (value != null && key != null)
                    data[key] = value;
            }

            if (data.Count > 0)
            {
                var fileName = $"{type.Name}settings.json";
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(Path.Combine(directoryPath, fileName), json);
            }
        }

        /// <summary>
        /// Loads settings for a specific custom bro type without requiring an instance.
        /// </summary>
        /// <typeparam name="T">The custom bro type whose settings should be loaded</typeparam>
        /// <remarks>
        /// - If the directory path is not cached, retrieves it from BroMakerStorage
        /// - Silently returns if the settings file doesn't exist
        /// - Automatically handles type conversion including enums and complex types
        /// - Logs errors for individual fields that fail to load but continues with other fields
        /// </remarks>
        public static void LoadSettings<T>() where T : CustomHero
        {
            var type = typeof(T);

            // Get or retrieve the directory path
            string directoryPath;
            if (!_typeDirectoryCache.TryGetValue(type, out directoryPath))
            {
                directoryPath = Storages.BroMakerStorage.GetStoredHeroByCustomHeroType<T>().GetInfo().path;
                if (string.IsNullOrEmpty(directoryPath))
                {
                    Console.WriteLine($"Error: Could not determine directory path for custom bro {type.Name}");
                    return;
                }
                _typeDirectoryCache[type] = directoryPath;
            }

            var fileName = Path.Combine(directoryPath, $"{type.Name}settings.json");
            if (!File.Exists(fileName))
                return;

            try
            {
                string json = File.ReadAllText(fileName);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (data == null)
                    return;

                var saveableMembers = GetSaveableMembers(type);

                foreach (var member in saveableMembers)
                {
                    SaveableSettingAttribute attr = null;
                    string key = null;

                    if (member is FieldInfo)
                    {
                        FieldInfo field = (FieldInfo)member;
                        attr = (SaveableSettingAttribute)field.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                        key = attr?.Name ?? field.Name;

                        if (data.ContainsKey(key))
                        {
                            try
                            {
                                var value = data[key];
                                var convertedValue = ConvertValue(value, field.FieldType);
                                field.SetValue(null, convertedValue);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error loading {key}: {ex.Message}");
                            }
                        }
                    }
                    else if (member is PropertyInfo)
                    {
                        var property = (PropertyInfo)member;
                        if (property.CanWrite)
                        {
                            attr = (SaveableSettingAttribute)property.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                            key = attr?.Name ?? property.Name;

                            if (data.ContainsKey(key))
                            {
                                try
                                {
                                    var value = data[key];
                                    var convertedValue = ConvertValue(value, property.PropertyType);
                                    property.SetValue(null, convertedValue, null);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Error loading {key}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading settings from {fileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves settings for all custom bros that have been previously loaded or saved.
        /// This static method uses cached custom bro and directory information to save all settings without requiring instances.
        /// </summary>
        /// <remarks>
        /// - Only saves settings for custom bros that have been cached (i.e., SaveSettings or LoadSettings was called at least once)
        /// - Skips custom bros that don't have a cached directory path
        /// - Logs warnings for custom bros without cached directory paths
        /// - Logs success/error messages for each custom bro
        /// - Useful for application shutdown or periodic auto-saves
        /// </remarks>
        public static void SaveAll()
        {
            foreach (var kvp in _saveableMembersCache)
            {
                var type = kvp.Key;
                var saveableMembers = kvp.Value;

                // Skip if we don't have a directory path cached for this custom bro
                if (!_typeDirectoryCache.TryGetValue(type, out string cachedDirectoryPath))
                {
                    Console.WriteLine($"Warning: No directory path cached for custom bro {type.Name}, skipping...");
                    continue;
                }

                // Skip if no saveable members
                if (saveableMembers.Count == 0)
                    continue;

                var data = new Dictionary<string, object>();

                foreach (var member in saveableMembers)
                {
                    string key = null;
                    object value = null;

                    SaveableSettingAttribute attr;
                    if (member is FieldInfo field)
                    {
                        attr = (SaveableSettingAttribute)field.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                        key = attr?.Name ?? field.Name;
                        value = field.GetValue(null);
                    }
                    else if (member is PropertyInfo property)
                    {
                        attr = (SaveableSettingAttribute)property.GetCustomAttributes(typeof(SaveableSettingAttribute), false).FirstOrDefault();
                        key = attr?.Name ?? property.Name;
                        value = property.GetValue(null, null);
                    }

                    if (value != null && key != null)
                        data[key] = value;
                }

                if (data.Count > 0)
                {
                    try
                    {
                        var fileName = $"{type.Name}settings.json";
                        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                        File.WriteAllText(Path.Combine(cachedDirectoryPath, fileName), json);
                        Console.WriteLine($"Saved settings for custom bro {type.Name}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error saving settings for custom bro {type.Name}: {ex.Message}");
                    }
                }
            }
        }

        private bool HasSaveableFields(Type type)
        {
            if (_hasSaveableFieldsCache.TryGetValue(type, out bool hasSaveable))
            {
                return hasSaveable;
            }

            var members = GetSaveableMembers(type);
            hasSaveable = members.Count > 0;

            _hasSaveableFieldsCache[type] = hasSaveable;

            return hasSaveable;
        }

        private static List<MemberInfo> GetSaveableMembers(Type type)
        {
            if (_saveableMembersCache.TryGetValue(type, out var cached))
                return cached;

            var members = new List<MemberInfo>();
            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var fields = type.GetFields(flags)
                .Where(f => f.GetCustomAttributes(typeof(SaveableSettingAttribute), false).Length > 0);

            foreach (var field in fields)
                members.Add(field);

            var properties = type.GetProperties(flags)
                .Where(p => p.GetCustomAttributes(typeof(SaveableSettingAttribute), false).Length > 0);

            foreach (var property in properties)
                members.Add(property);

            _saveableMembersCache[type] = members;

            return members;
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null)
                return null;

            // Handle Newtonsoft.Json's JObject/JArray types
            if (value is JObject || value is JArray || value is JValue)
            {
                string jsonString = value.ToString();
                return JsonConvert.DeserializeObject(jsonString, targetType);
            }

            // Handle simple type conversions
            if (targetType == value.GetType())
                return value;

            if (targetType.IsEnum)
            {
                if (value is string)
                    return Enum.Parse(targetType, (string)value);
                else
                    return Enum.ToObject(targetType, value);
            }

            return Convert.ChangeType(value, targetType);
        }
        #endregion
    }
}

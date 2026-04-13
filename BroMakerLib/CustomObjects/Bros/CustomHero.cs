using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BroMakerLib.Extensions;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityModManagerNet;
using Random = UnityEngine.Random;

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

        [Syncronize] public CustomBroInfo Info { get; set; }
        [Syncronize] public BroBase Character { get; set; }
        [JsonIgnore] public MuscleTempleFlexEffect FlexEffect { get; set; }

        [JsonIgnore] public int CurrentVariant { get; set; }
        [JsonIgnore] public Vector2 CurrentGunSpriteOffset { get; set; }
        [JsonIgnore] public List<Material> CurrentSpecialMaterials { get; set; }
        [JsonIgnore] public Vector2 CurrentSpecialMaterialOffset { get; set; }
        [JsonIgnore] public float CurrentSpecialMaterialSpacing { get; set; }
        [JsonIgnore] public Material CurrentFirstAvatar { get; set; }


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
        public string DirectoryPath => _directoryPath;

        /// <summary>
        /// Contains the path to your bro's sound folder (assumes the folder is named "sounds" by default)
        /// </summary>
        public string SoundPath => _soundPath;

        /// <summary>
        /// Contains the path to your bro's projectile folder (assumes the folder is named "projectiles" by default)
        /// </summary>
        public string ProjectilePath => _projectilePath;

        [SerializeField] private string _directoryPath;
        [SerializeField] private string _soundPath;
        [SerializeField] private string _projectilePath;

        /// <summary>
        /// When enabled, CustomHero tracks death and revival state via <see cref="acceptedDeath" /> and calls
        /// <see cref="OnDeath" /> and <see cref="OnRevived" /> at the appropriate times.
        /// Bros should check <c>if (acceptedDeath) return;</c> after <c>base.Update()</c> to skip their own logic when dead.
        /// </summary>
        protected bool trackDeathRevival = true;

        /// <summary>
        /// When enabled, CustomHero automatically resets _TintColor to Color.gray on the main renderer,
        /// gun sprite, and all materials registered via <see cref="RegisterTintMaterial" /> when invulnerability ends.
        /// </summary>
        protected bool trackInvulnerabilityTint = true;

        /// <summary>
        /// When enabled, CustomHero sets meleeType and currentMeleeType to MeleeType.Custom in Start(),
        /// which causes the base game to call StartCustomMelee() instead of the default melee.
        /// </summary>
        protected bool useCustomMelee = false;

        /// <summary>
        /// When enabled, CustomHero's SetGestureAnimation override blocks gesture animations while doingMelee is true,
        /// preventing flex and other gestures from interrupting melee attacks.
        /// </summary>
        protected bool blockGesturesDuringMelee = true;

        /// <summary>
        /// When enabled, the variant system ensures each active instance of the same bro type gets a different variant
        /// in multiplayer. Registration and unregistration are handled automatically through the lifecycle.
        /// Override <see cref="GetVariant" /> and call <see cref="GetUniqueVariant" /> directly to pass a filtered variant list.
        /// </summary>
        protected bool deduplicateVariants = false;

        /// <summary>
        /// Set to true when the bro has died and is not immediately reviving. Managed automatically when
        /// <see cref="trackDeathRevival" /> is enabled.
        /// Bros should check <c>if (acceptedDeath) return;</c> after <c>base.Update()</c> to skip their own Update logic when dead.
        /// </summary>
        protected bool acceptedDeath = false;

        protected bool wasInvulnerable = false;
        protected List<Material> tintMaterials = new List<Material>();

        private static readonly Dictionary<Type, Dictionary<CustomHero, int>> _activeVariants = new Dictionary<Type, Dictionary<CustomHero, int>>();

        #region BroBase Methods

        protected override void Awake()
        {
            Character = this;
            Info = LoadHero.currentInfo;
            try
            {
                EnableSyncing(true, true);

                // Select variant
                CurrentVariant = GetVariant();
                if (deduplicateVariants)
                {
                    RegisterVariant();
                }

                // Cache current variant parameters
                CurrentGunSpriteOffset = BroMakerUtilities.GetVariantValue(Info.GunSpriteOffset, CurrentVariant);
                CurrentSpecialMaterials = BroMakerUtilities.GetVariantValue(Info.SpecialMaterials, CurrentVariant);
                CurrentSpecialMaterialOffset = BroMakerUtilities.GetVariantValue(Info.SpecialMaterialOffset, CurrentVariant);
                CurrentSpecialMaterialSpacing = BroMakerUtilities.GetVariantValue(Info.SpecialMaterialSpacing, CurrentVariant);
                CurrentFirstAvatar = BroMakerUtilities.GetVariantValue(Info.FirstAvatar, CurrentVariant);

                Character.specialGrenade.playerNum = LoadHero.playerNum;

                Info.BeforeAwake(this);
                base.Awake();
                this.SetSprites();
                Info.AfterAwake(this);

                // Make sure parachute isn't null, for some reason the game's default way of handling this doesn't work
                if (this.parachute == null)
                {
                    Parachute parachute;
                    for (var i = 0; i < transform.childCount; ++i)
                    {
                        if ((parachute = transform.GetChild(i).GetComponent<Parachute>()) != null)
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
                if (useCustomMelee)
                {
                    meleeType = MeleeType.Custom;
                    currentMeleeType = MeleeType.Custom;
                }

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

        protected override void Update()
        {
            base.Update();

            if (trackDeathRevival && acceptedDeath)
            {
                if (health <= 0 && !WillReviveAlready)
                {
                    return;
                }

                acceptedDeath = false;
                OnRevived();
            }

            if (trackInvulnerabilityTint)
            {
                if (invulnerable)
                {
                    wasInvulnerable = true;
                }
                else if (wasInvulnerable)
                {
                    wasInvulnerable = false;
                    ResetTintColors();
                }
            }

            if (trackDeathRevival && actionState == ActionState.Dead && !acceptedDeath && !WillReviveAlready)
            {
                acceptedDeath = true;
                OnDeath();
            }
        }

        protected override void OnDestroy()
        {
            if (deduplicateVariants)
            {
                UnregisterVariant();
            }

            base.OnDestroy();
        }

        public override void SetGestureAnimation(GestureElement.Gestures gesture)
        {
            if (blockGesturesDuringMelee && doingMelee)
            {
                return;
            }

            base.SetGestureAnimation(gesture);
        }

        // This function is overridden to remove the RPC calls, since they don't work currently
        protected override void CheckForTraps(ref float yIT)
        {
            var num = Y + yIT;
            if (num <= groundHeight + 1f)
            {
                num = groundHeight + 1f;
            }

            if (Map.isEditing || invulnerable)
            {
                return;
            }

            if (!IsEnemy && !IsMine)
            {
                return;
            }

            var nearestAcid = Map.GetNearestAcid(X, Y + 8f, 2f);
            if (nearestAcid != null && nearestAcid.fullness > 0.2f)
            {
                CoverInAcid();
            }

            if (impaledByTransform == null && IsHero && ((yI > 50f && (canTouchRightWalls || canTouchLeftWalls || WallClimbing) && (Time.time - lastJumpTime > 0.2f || Y > groundHeight + 17f)) || yI < -120f) && IsSurroundedByBarbedWire())
            {
                EffectsController.CreateBloodParticles(bloodColor, X, Y + 10f, -5f, 1, 4f, 4f, 50f, xI * 0.8f, 70f);
                if (yI < 0f)
                {
                    yI *= 0.2f;
                }
                else
                {
                    yI *= 0.45f;
                }

                barbedWireWithin.ForceBloody();
                barbedWireWithin.PlayCutSound();
            }

            RaycastHit raycastHit;
            if (impaledByTransform == null && Physics.Raycast(new Vector3(X, num, 0f), Vector3.down, out raycastHit, 25f, groundLayer))
            {
                var component = raycastHit.collider.GetComponent<Block>();
                if (component != null)
                {
                    if (raycastHit.distance < 10f && (IsMine || IsEnemy))
                    {
                        component.CheckForMine();
                    }

                    if (component.spikes != null && !invulnerable && !wallDrag)
                    {
                        if (component.spikes.EvaluateImpalent(this))
                        {
                            var num2 = playerNum >= 0 ? component.spikes.spikeTrapHarmlessHeight : component.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit.point.y > groundHeight - 1f && raycastHit.distance >= num2 && raycastHit.distance < component.spikes.spikeTrapHeight && Y > component.Y + component.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
                                //Networking.RPC<TestVanDammeAnim>(PID.TargetAll, new RpcSignature<TestVanDammeAnim>(component.spikes.ImpaleUnit), this, false);
                                component.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component.spikes.IsBarbedWire(this) && component.spikes.collumn == component.collumn && Y < raycastHit.point.y + 12f && Mathf.Abs(yI) < 50f && Mathf.Abs(xI + xIBlast) > GetSpeed - 2f && (int)Mathf.Sign(xI + xIBlast) == (!left ? 0 : -1) + (!right ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(bloodColor, X, Y + 10f, -5f, 1, 4f, 4f, 50f, xI * 0.8f, 70f);
                            xIBlast -= xI * 0.4f;
                            xI = 0f;
                            component.spikes.ForceBloody();
                            component.spikes.PlayCutSound();
                        }
                    }
                }
            }

            RaycastHit raycastHit2;
            if (impaledByTransform == null && Physics.Raycast(new Vector3(X - 3f, num, 0f), Vector3.down, out raycastHit2, 25f, groundLayer))
            {
                var component2 = raycastHit2.collider.GetComponent<Block>();
                if (component2 != null)
                {
                    if (raycastHit2.distance < 10f && (IsMine || IsEnemy))
                    {
                        component2.CheckForMine();
                    }

                    if (component2.spikes != null && !invulnerable && !wallDrag)
                    {
                        if (component2.spikes.EvaluateImpalent(this))
                        {
                            var num3 = playerNum >= 0 ? component2.spikes.spikeTrapHarmlessHeight : component2.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit2.point.y > groundHeight - 1f && raycastHit2.distance >= num3 && raycastHit2.distance < component2.spikes.spikeTrapHeight && Y > component2.Y + component2.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
                                //Networking.RPC<TestVanDammeAnim>(PID.TargetAll, new RpcSignature<TestVanDammeAnim>(component2.spikes.ImpaleUnit), this, false);
                                component2.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component2.spikes.IsBarbedWire(this) && component2.spikes.collumn == component2.collumn && Y < raycastHit2.point.y + 12f && Mathf.Abs(yI) < 50f && Mathf.Abs(xI + xIBlast) > GetSpeed - 2f && (int)Mathf.Sign(xI + xIBlast) == (!left ? 0 : -1) + (!right ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(bloodColor, X, Y + 10f, -5f, 1, 4f, 4f, 50f, xI * 0.8f, 70f);
                            xIBlast -= xI * 0.4f;
                            xI = 0f;
                            component2.spikes.ForceBloody();
                            component2.spikes.PlayCutSound();
                        }
                    }
                }
            }

            RaycastHit raycastHit3;
            if (impaledByTransform == null && Physics.Raycast(new Vector3(X + 3f, num, 0f), Vector3.down, out raycastHit3, 25f, groundLayer))
            {
                var component3 = raycastHit3.collider.GetComponent<Block>();
                if (component3 != null)
                {
                    if (raycastHit3.distance < 10f && (IsMine || IsEnemy))
                    {
                        component3.CheckForMine();
                    }

                    if (component3.spikes != null && !invulnerable && !wallDrag)
                    {
                        if (component3.spikes.EvaluateImpalent(this))
                        {
                            var num4 = playerNum >= 0 ? component3.spikes.spikeTrapHarmlessHeight : component3.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit3.point.y > groundHeight - 1f && raycastHit3.distance >= num4 && raycastHit3.distance < component3.spikes.spikeTrapHeight && Y > component3.Y + component3.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
                                //Networking.RPC<TestVanDammeAnim>(PID.TargetAll, new RpcSignature<TestVanDammeAnim>(component3.spikes.ImpaleUnit), this, false);
                                component3.spikes.ImpaleUnit(this);
                            }
                        }
                        else if (component3.spikes.IsBarbedWire(this) && component3.spikes.collumn == component3.collumn && Y < raycastHit3.point.y + 12f && Mathf.Abs(yI) < 50f && Mathf.Abs(xI + xIBlast) > GetSpeed - 2f && (int)Mathf.Sign(xI + xIBlast) == (!left ? 0 : -1) + (!right ? 0 : 1))
                        {
                            EffectsController.CreateBloodParticles(bloodColor, X, Y + 10f, -5f, 1, 4f, 4f, 50f, xI * 0.8f, 70f);
                            xIBlast -= xI * 0.4f;
                            xI = 0f;
                            component3.spikes.ForceBloody();
                            component3.spikes.PlayCutSound();
                        }
                    }
                }
            }
        }

        protected override void TriggerFlexEvent()
        {
            if (player.HasFlexPower(PickupType.FlexAlluring))
            {
                Map.AttractMooks(X, Y, 96f, 30f);
            }

            if (player.HasFlexPower(PickupType.FlexGoldenLight))
            {
                if (FlexEffect == null)
                {
                    FlexEffect = Traverse.Create(this as BroBase).GetFieldValue("flexEffect") as MuscleTempleFlexEffect;
                }

                if (FlexEffect != null)
                {
                    FlexEffect.PlaySoundEffect();
                }

                if (IsMine)
                {
                    var num = 8 + Random.Range(0, 5);
                    for (var i = 0; i < num; i++)
                    {
                        var angle = -1.88495576f + 1.2f / (float)(num - 1) * 3.14159274f * (float)i;
                        var vector = Math.Point2OnCircle(angle, 1f);
                        ProjectileController.SpawnProjectileLocally(ProjectileController.instance.goldenLightProjectile, this, X, Y + 12f, vector.x * 400f, vector.y * 400f, true, 15, false, true, -15f);
                    }
                }
            }
            else if (player.HasFlexPower(PickupType.FlexInvulnerability) && FlexEffect != null)
            {
                FlexEffect.PlaySoundEffect();
            }
        }

        protected override void SetGunPosition(float xOffset, float yOffset)
        {
            gunSprite.transform.localPosition = new Vector3(xOffset + CurrentGunSpriteOffset.x, yOffset + CurrentGunSpriteOffset.y, -.001f);
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
        /// Override this method to add any custom trigger actions via RocketLib's Custom Trigger system
        /// </summary>
        public virtual void RegisterCustomTriggers()
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
            for (var i = 0; i < spritePaths.Count; ++i)
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
            for (var i = 0; i < soundPaths.Count; ++i)
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

        void ICustomHero.PrefabSetup()
        {
            BeforePrefabSetup();
            var baseHeroType = SoundHolderHeroType;
            // Use base hero type if SoundHolderHeroType is unassigned
            if (baseHeroType == HeroType.None)
            {
                baseHeroType = LoadHero.GetBaseHeroTypeOfPreset(GetType());
            }

            soundHolder = Instantiate(HeroController.GetHeroPrefab(baseHeroType).soundHolder);
            soundHolder.gameObject.SetActive(false);
            soundHolder.gameObject.name = "SoundHolder " + name;
            DontDestroyOnLoad(soundHolder);

            if (SoundHolderVoiceType == SoundHolderVoiceTypes.None)
            {
                soundHolderVoice = Instantiate((HeroController.GetHeroPrefab(baseHeroType) as BroBase).soundHolderVoice);
            }
            else
            {
                switch (SoundHolderVoiceType)
                {
                    case SoundHolderVoiceTypes.MaleDeep:
                        soundHolderVoice = Instantiate((HeroController.GetHeroPrefab(HeroType.Rambro) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.MaleLight:
                        soundHolderVoice = Instantiate((HeroController.GetHeroPrefab(HeroType.McBrover) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.MaleRough:
                        soundHolderVoice = Instantiate((HeroController.GetHeroPrefab(HeroType.SnakeBroSkin) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.MaleFlyInsect:
                        soundHolderVoice = Instantiate((HeroController.GetHeroPrefab(HeroType.BrondleFly) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.Female:
                        soundHolderVoice = Instantiate((HeroController.GetHeroPrefab(HeroType.EllenRipbro) as BroBase).soundHolderVoice);
                        break;
                    case SoundHolderVoiceTypes.ChevBrolios:
                        soundHolderVoice = Instantiate((HeroController.GetHeroPrefab(HeroType.ChevBrolios) as ChevBrolios).arousedVoiceHolder);
                        break;
                    case SoundHolderVoiceTypes.ChevBroliosDrowsy:
                        soundHolderVoice = Instantiate((HeroController.GetHeroPrefab(HeroType.ChevBrolios) as ChevBrolios).drowsyVoiceHolder);
                        break;
                }
            }

            soundHolderVoice.gameObject.SetActive(false);
            soundHolderVoice.gameObject.name = "SoundHolderVoice " + name;
            DontDestroyOnLoad(soundHolderVoice);

            soundHolderFootSteps = Instantiate(HeroController.GetHeroPrefab(baseHeroType).soundHolderFootSteps);
            soundHolderFootSteps.gameObject.SetActive(false);
            soundHolderFootSteps.gameObject.name = "SoundHolderFootSteps " + name;
            DontDestroyOnLoad(soundHolderFootSteps);

            AssignDirectoryPaths(LoadHero.currentInfo.path);
            try
            {
                LoadSettings();
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog("Failed to load settings in SetupPrefab: ", ex);
            }

            Character = this;
            Info = LoadHero.currentInfo;

            // Setup CustomHero from original bro component and destroy it
            this.SetupCustomHero();

            AfterPrefabSetup();
        }

        /// <summary>
        /// Sets up the DirectoryPath, SoundPath, and ProjectilePath variables.
        /// You can override this to adjust the paths if your sounds / projectiles folders
        /// are named something other than "sounds" and "projectiles"
        /// </summary>
        /// <param name="directoryPath">Path to the directory that contains your custom bro's .dll</param>
        public virtual void AssignDirectoryPaths(string directoryPath)
        {
            _directoryPath = directoryPath;
            _soundPath = Path.Combine(directoryPath, "sounds");
            _projectilePath = Path.Combine(directoryPath, "projectiles");
        }

        /// <summary>
        /// Override this to customize variant selection logic
        /// </summary>
        /// <returns>Variant to spawn bro as</returns>
        public virtual int GetVariant()
        {
            if (Info.VariantCount <= 1)
            {
                return 0;
            }

            if (deduplicateVariants)
            {
                return GetUniqueVariant();
            }

            return Random.Range(0, Info.VariantCount);
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
            CurrentVariant = variant;
            CurrentGunSpriteOffset = BroMakerUtilities.GetVariantValue(Info.GunSpriteOffset, CurrentVariant);
            CurrentSpecialMaterials = BroMakerUtilities.GetVariantValue(Info.SpecialMaterials, CurrentVariant);
            CurrentSpecialMaterialOffset = BroMakerUtilities.GetVariantValue(Info.SpecialMaterialOffset, CurrentVariant);
            CurrentSpecialMaterialSpacing = BroMakerUtilities.GetVariantValue(Info.SpecialMaterialSpacing, CurrentVariant);
            CurrentFirstAvatar = BroMakerUtilities.GetVariantValue(Info.FirstAvatar, CurrentVariant);

            this.SetSprites();
            BroMakerUtilities.SetSpecialMaterials(playerNum, CurrentSpecialMaterials, CurrentSpecialMaterialOffset, CurrentSpecialMaterialSpacing);
            HeroController.SetAvatarMaterial(playerNum, CurrentFirstAvatar);
        }

        /// <summary>
        /// Called when the bro dies. Override to perform bro-specific cleanup (stop audio, drop items, reset state).
        /// Requires <see cref="trackDeathRevival" /> to be enabled.
        /// Always call base.OnDeath() when overriding to preserve variant deduplication.
        /// </summary>
        protected virtual void OnDeath()
        {
            if (deduplicateVariants)
            {
                UnregisterVariant();
            }
        }

        /// <summary>
        /// Called when the bro is revived after death. Override to perform bro-specific revival setup (restore ammo, reset
        /// materials).
        /// Requires <see cref="trackDeathRevival" /> to be enabled.
        /// Always call base.OnRevived() when overriding to preserve variant deduplication.
        /// </summary>
        protected virtual void OnRevived()
        {
            if (deduplicateVariants)
            {
                RegisterVariant();
            }
        }

        /// <summary>
        /// Registers a material to be included in automatic tint color resets when invulnerability ends.
        /// Call this for any additional materials beyond the main renderer and gun sprite (which are handled automatically).
        /// Must be called after the material is created, typically in Start() or AfterPrefabSetup().
        /// </summary>
        /// <param name="mat">The material to register for tint tracking</param>
        protected void RegisterTintMaterial(Material mat)
        {
            if (mat != null && !tintMaterials.Contains(mat))
            {
                tintMaterials.Add(mat);
            }
        }

        /// <summary>
        /// Resets the _TintColor on the main renderer, gun sprite, and all registered tint materials to Color.gray.
        /// Called automatically when invulnerability ends if <see cref="trackInvulnerabilityTint" /> is enabled.
        /// Can also be called directly for cases where tint needs to be reset manually (e.g. clearing invulnerability early).
        /// </summary>
        public void ResetTintColors()
        {
            GetComponent<Renderer>().material.SetColor("_TintColor", Color.gray);
            gunSprite.meshRender.material.SetColor("_TintColor", Color.gray);
            foreach (var mat in tintMaterials)
            {
                mat.SetColor("_TintColor", Color.gray);
            }
        }

        /// <summary>
        /// Returns a variant index not currently in use by other active instances of the same bro type.
        /// Falls back to random selection if all variants are taken.
        /// Used automatically by <see cref="GetVariant" /> when <see cref="deduplicateVariants" /> is enabled.
        /// Can be called directly from a <see cref="GetVariant" /> override to pass a filtered list of allowed variants.
        /// </summary>
        /// <param name="allowedVariants">
        /// Optional list of variant indices to choose from. If null, all variants (0 to
        /// VariantCount-1) are considered.
        /// </param>
        /// <returns>A variant index not in use by other active instances, or a random one if all are taken</returns>
        protected int GetUniqueVariant(List<int> allowedVariants = null)
        {
            var broType = GetType();
            List<int> candidates;

            if (allowedVariants != null)
            {
                candidates = new List<int>(allowedVariants);
            }
            else
            {
                candidates = new List<int>();
                for (var i = 0; i < Info.VariantCount; i++)
                {
                    candidates.Add(i);
                }
            }

            if (_activeVariants.ContainsKey(broType))
            {
                var active = _activeVariants[broType];
                foreach (var kvp in active)
                {
                    if (kvp.Key != this)
                    {
                        candidates.Remove(kvp.Value);
                    }
                }
            }

            if (candidates.Count > 0)
            {
                return candidates[Random.Range(0, candidates.Count)];
            }

            if (allowedVariants != null && allowedVariants.Count > 0)
            {
                return allowedVariants[Random.Range(0, allowedVariants.Count)];
            }

            return Random.Range(0, Info.VariantCount);
        }

        private void RegisterVariant()
        {
            var broType = GetType();
            if (!_activeVariants.ContainsKey(broType))
            {
                _activeVariants[broType] = new Dictionary<CustomHero, int>();
            }

            _activeVariants[broType][this] = CurrentVariant;
        }

        private void UnregisterVariant()
        {
            var broType = GetType();
            if (_activeVariants.ContainsKey(broType))
            {
                _activeVariants[broType].Remove(this);
            }
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
        /// Gets the config directory path for a custom bro type.
        /// </summary>
        /// <param name="broType">The custom bro type</param>
        /// <returns>Path to the bro's config directory (e.g., Config/BroMaker/Bros/{BroName}/)</returns>
        public static string GetBroConfigPath(Type broType)
        {
            return Path.Combine(Path.Combine(Path.Combine(UnityModManager.configPath, "BroMaker"), "Bros"), broType.Name);
        }

        /// <summary>
        /// Saves all static fields and properties marked with [SaveableSetting] to a JSON file.
        /// The file will be saved to Config/BroMaker/Bros/{BroName}/ with the name format: {BroName}_settings.json
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
            _typeDirectoryCache[type] = DirectoryPath;

            if (!HasSaveableFields(type))
            {
                return;
            }

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
                {
                    data[key] = value;
                }
            }

            if (data.Count > 0)
            {
                var fileName = $"{type.Name}_settings.json";
                var configPath = GetBroConfigPath(type);
                Directory.CreateDirectory(configPath);
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(Path.Combine(configPath, fileName), json);
            }
        }

        /// <summary>
        /// Loads all static fields and properties marked with [SaveableSetting] from a JSON file.
        /// The file is expected to be in Config/BroMaker/Bros/{BroName}/ with the name format: {BroName}_settings.json
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
            _typeDirectoryCache[type] = DirectoryPath;

            if (!HasSaveableFields(type))
            {
                return;
            }

            var configPath = GetBroConfigPath(type);
            var fileName = Path.Combine(configPath, $"{type.Name}_settings.json");
            if (!File.Exists(fileName))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(fileName);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (data == null)
                {
                    return;
                }

                var saveableMembers = GetSaveableMembers(type);

                foreach (var member in saveableMembers)
                {
                    SaveableSettingAttribute attr;
                    string key;

                    if (member is FieldInfo)
                    {
                        var field = (FieldInfo)member;
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
                                BMLogger.Error($"Error loading {key}: {ex.Message}");
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
                                    BMLogger.Error($"Error loading {key}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Error loading settings from {fileName}: {ex.Message}");
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
                if (!BroMakerStorage.GetStoredHeroByCustomHeroType<T>(out var storedHero))
                {
                    BMLogger.Error($"Error: Could not find stored hero for custom bro {type.Name}");
                    return;
                }

                directoryPath = storedHero.GetInfo().path;
                if (string.IsNullOrEmpty(directoryPath))
                {
                    BMLogger.Error($"Error: Could not determine directory path for custom bro {type.Name}");
                    return;
                }

                _typeDirectoryCache[type] = directoryPath;
            }

            // Get saveable members
            var saveableMembers = GetSaveableMembers(type);
            if (saveableMembers.Count == 0)
            {
                return;
            }

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
                {
                    data[key] = value;
                }
            }

            if (data.Count > 0)
            {
                var fileName = $"{type.Name}_settings.json";
                var configPath = GetBroConfigPath(type);
                Directory.CreateDirectory(configPath);
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(Path.Combine(configPath, fileName), json);
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
                if (!BroMakerStorage.GetStoredHeroByCustomHeroType<T>(out var storedHero))
                {
                    BMLogger.Error($"Error: Could not find stored hero for custom bro {type.Name}");
                    return;
                }

                directoryPath = storedHero.GetInfo().path;
                if (string.IsNullOrEmpty(directoryPath))
                {
                    BMLogger.Error($"Error: Could not determine directory path for custom bro {type.Name}");
                    return;
                }

                _typeDirectoryCache[type] = directoryPath;
            }

            var configPath = GetBroConfigPath(type);
            var fileName = Path.Combine(configPath, $"{type.Name}_settings.json");
            if (!File.Exists(fileName))
            {
                return;
            }

            try
            {
                var json = File.ReadAllText(fileName);
                var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

                if (data == null)
                {
                    return;
                }

                var saveableMembers = GetSaveableMembers(type);

                foreach (var member in saveableMembers)
                {
                    SaveableSettingAttribute attr;
                    string key;

                    if (member is FieldInfo)
                    {
                        var field = (FieldInfo)member;
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
                                BMLogger.Error($"Error loading {key}: {ex.Message}");
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
                                    BMLogger.Error($"Error loading {key}: {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                BMLogger.Error($"Error loading settings from {fileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves settings for all custom bros that have been previously loaded or saved.
        /// This static method uses cached custom bro and directory information to save all settings without requiring
        /// instances.
        /// </summary>
        /// <remarks>
        /// - Only saves settings for custom bros that have been cached (i.e., SaveSettings or LoadSettings was called at least
        /// once)
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
                if (!_typeDirectoryCache.TryGetValue(type, out _))
                {
                    BMLogger.Warning($"Warning: No directory path cached for custom bro {type.Name}, skipping...");
                    continue;
                }

                // Skip if no saveable members
                if (saveableMembers.Count == 0)
                {
                    continue;
                }

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
                    {
                        data[key] = value;
                    }
                }

                if (data.Count > 0)
                {
                    try
                    {
                        var fileName = $"{type.Name}_settings.json";
                        var configPath = GetBroConfigPath(type);
                        Directory.CreateDirectory(configPath);
                        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
                        File.WriteAllText(Path.Combine(configPath, fileName), json);
                    }
                    catch (Exception ex)
                    {
                        BMLogger.Error($"Error saving settings for custom bro {type.Name}: {ex.Message}");
                    }
                }
            }
        }

        private bool HasSaveableFields(Type type)
        {
            if (_hasSaveableFieldsCache.TryGetValue(type, out var hasSaveable))
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
            {
                return cached;
            }

            var members = new List<MemberInfo>();
            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            var fields = type.GetFields(flags)
                .Where(f => f.GetCustomAttributes(typeof(SaveableSettingAttribute), false).Length > 0);

            foreach (var field in fields)
            {
                members.Add(field);
            }

            var properties = type.GetProperties(flags)
                .Where(p => p.GetCustomAttributes(typeof(SaveableSettingAttribute), false).Length > 0);

            foreach (var property in properties)
            {
                members.Add(property);
            }

            _saveableMembersCache[type] = members;

            return members;
        }

        private static object ConvertValue(object value, Type targetType)
        {
            if (value == null)
            {
                return null;
            }

            // Handle Newtonsoft.Json's JObject/JArray types
            if (value is JObject || value is JArray || value is JValue)
            {
                var jsonString = value.ToString();
                return JsonConvert.DeserializeObject(jsonString, targetType);
            }

            // Handle simple type conversions
            if (targetType == value.GetType())
            {
                return value;
            }

            if (targetType.IsEnum)
            {
                if (value is string)
                {
                    return Enum.Parse(targetType, (string)value);
                }
                else
                {
                    return Enum.ToObject(targetType, value);
                }
            }

            return Convert.ChangeType(value, targetType);
        }

        #endregion
    }
}

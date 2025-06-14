using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BroMakerLib.CustomObjects.Bros
{
    [HeroPreset("CustomHero", HeroType.Rambro)]
    public class CustomHero : BroBase, ICustomHero
    {
        [Syncronize]
        public CustomBroInfo info { get; set; }
        [Syncronize]
        public BroBase character { get; set; }

		// This is needed to ensure Unity Serializes the Getter / Setter field
        [field: SerializeField]
        public List<Material> specialMaterials { get; set; } = new List<Material>();
        [field: SerializeField]
        public Vector2 specialMaterialOffset { get; set; } = Vector2.zero;
        [field: SerializeField]
        public float specialMaterialSpacing { get; set; } = 0f;
        [field: SerializeField]
        public Material firstAvatar { get; set; } = null;
        [field: SerializeField]
        public Vector2 gunSpriteOffset { get; set; } = Vector2.zero;
        [field: SerializeField]
        public MuscleTempleFlexEffect flexEffect { get; set; }

        #region BroBase Methods
        protected override void Awake()
        {
            character = this;
            info = LoadHero.currentInfo;
            try
            {
                EnableSyncing(true, true);
                this.SetupCustomHero();

                info.BeforeAwake(this);
                base.Awake();
                info.AfterAwake(this);

				// Somehow it becomes 0, 0 if it's in the parameters
                info.gunSpriteOffset = gunSpriteOffset;

				// Make sure parachute isn't null, for some reason the game's default way of handling this doesn't work
				if ( this.parachute == null )
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
                info.BeforeStart(this);
				if ( character.gameObject.GetComponent<InvulnerabilityFlash>() == null )
				{
                    character.gameObject.AddComponent<InvulnerabilityFlash>().SetCharacter(character);
                }
				else
				{
					character.gameObject.GetComponent<InvulnerabilityFlash>().enabled = true;
                }
				WavyGrassEffector[] wavyGrassEffectors = character.gameObject.GetComponents<WavyGrassEffector>();
				if ( wavyGrassEffectors.Length == 0 )
				{
					character.gameObject.AddComponent<WavyGrassEffector>();
				}
				base.Start();
                info.AfterStart(this);
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
				if (flexEffect == null)
				{
					this.flexEffect = Traverse.Create((this as BroBase)).GetFieldValue("flexEffect") as MuscleTempleFlexEffect;
				}
				if (this.flexEffect != null)
				{
					this.flexEffect.PlaySoundEffect();
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
			else if (this.player.HasFlexPower(PickupType.FlexInvulnerability) && this.flexEffect != null)
			{
				this.flexEffect.PlaySoundEffect();
			}
        }

        protected override void SetGunPosition(float xOffset, float yOffset)
        {
            this.gunSprite.transform.localPosition = new Vector3(xOffset + gunSpriteOffset.x, yOffset + gunSpriteOffset.y, -1f);
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
			for ( int i = 0; i < spritePaths.Count; ++i )
			{
				ResourcesController.GetMaterial(directoryPath, spritePaths[i]);
			}
        }

        /// <summary>
        /// Preloads each sound in the soundPaths list.
        /// </summary>
		/// <param name="directoryPath">Path to the directory containing the sound files</param>
        /// <param name="soundPaths">Sounds to load</param>
        public static void PreloadSounds( string directoryPath, List<string> soundPaths )
		{
			for ( int i = 0; i <  soundPaths.Count; ++i )
			{
				ResourcesController.GetAudioClip(directoryPath, soundPaths[i]);
			}
		}
        #endregion
    }
}

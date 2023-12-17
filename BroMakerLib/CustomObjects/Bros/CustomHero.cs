using System;
using System.Collections;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Loaders;
using BroMakerLib.Stats;
using UnityEngine;
using System.Collections.Generic;
using BroMakerLib.CustomObjects.Components;
using BroMakerLib.Abilities.Weapons;
using BroMakerLib.Abilities;

namespace BroMakerLib.CustomObjects.Bros
{
    [HeroPreset("CustomHero", HeroType.Rambro)]
    public class CustomHero : BroBase, ICustomHero
    {
        [Syncronize]
        public CustomBroInfo info { get; set; }
        [Syncronize]
        public BroBase character { get; set; }
        public CharacterExtended characterExtended { get; set; }

        public CharacterAbility primaryAbility;
        public CharacterAbility specialAbility;
        public CharacterAbility meleeAbility;


        #region Private Variable Becomes
        #region Publics
        #endregion
        #region Protected
        #endregion
        #endregion

        #region New variables
        #endregion

        #region BroBase Methods
        protected override void Awake()
        {
            character = this;
            info = LoadHero.currentInfo;
            try
            {
                EnableSyncing(true, true);
                this.SetupCustomHero();
                characterExtended = gameObject.AddComponent<CharacterExtended>();
                characterExtended.Initialize(info.abilities);

                info.BeforeAwake(this);
                base.Awake();
                info.AfterAwake(this);

                characterExtended.InvokeAbility(nameof(Awake));
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
                base.Start();
                info.AfterStart(this);

                characterExtended.InvokeAbility(nameof(Start));
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
            characterExtended.InvokeAbility(nameof(Update));
        }

        protected override void FireWeapon(float x, float y, float xSpeed, float ySpeed)
        {
            if(primaryAbility != null)
            {
                if (primaryAbility as Weapon != null)
                    primaryAbility.Fire(x, y, xSpeed, ySpeed);
                else
                    primaryAbility.Use();
            }
            else
            {
                base.FireWeapon(x, y, xSpeed, ySpeed);
            }
        }

        protected override void Land()
        {
            base.Land();
            characterExtended.InvokeAbility(nameof(Land));
        }

        protected override void UseSpecial()
        {
            base.UseSpecial();
            if(SpecialAmmo > 0)
                characterExtended.InvokeAbility(nameof(UseSpecial));
        }
        public override void RecallBro()
        {
            base.RecallBro();
            characterExtended.InvokeAbility(nameof(RecallBro));
        }
        protected override void ActivateGun()
        {
            base.ActivateGun();
            characterExtended.InvokeAbility(nameof(ActivateGun));
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
		#endregion

		#region Custom Methods
		/// <summary>
		/// Throw grenade and handle direction if characters crouching
		/// </summary>
		/// <param name="grenade"></param>
		protected virtual void SpawnGrenade(Grenade grenade)
        {
            if (down && IsOnGround() && ducking)
            {
                ProjectileController.SpawnGrenadeOverNetwork(grenade, this, X + Mathf.Sign(transform.localScale.x) * 6f, Y + 3f, 0.001f, 0.011f, Mathf.Sign(transform.localScale.x) * 30f, 70f, base.playerNum);
            }
            else
            {
                ProjectileController.SpawnGrenadeOverNetwork(grenade, this, X + Mathf.Sign(transform.localScale.x) * 8f, Y + 8f, 0.001f, 0.011f, Mathf.Sign(transform.localScale.x) * 200f, 150f, base.playerNum);
            }
        }
        #endregion
    }
}

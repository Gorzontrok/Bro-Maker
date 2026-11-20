// Auto-generated from RambroM.cs, do not edit manually

using System;
using System.Collections.Generic;
using BroMakerLib.CustomObjects;
using BroMakerLib.Extensions;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using Newtonsoft.Json;
using UnityEngine;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("Brominator", HeroType.Brominator)]
    public class BrominatorM : Brominator, ICustomHero
    {
        [Syncronize] public CustomBroInfo Info { get; set; }
        [Syncronize] public BroBase Character { get; set; }
        [JsonIgnore] public int CurrentVariant { get; set; }
        [JsonIgnore] public Vector2 CurrentGunSpriteOffset { get; set; }
        [JsonIgnore] public List<Material> CurrentSpecialMaterials { get; set; }
        [JsonIgnore] public Vector2 CurrentSpecialMaterialOffset { get; set; }
        [JsonIgnore] public float CurrentSpecialMaterialSpacing { get; set; }
        [JsonIgnore] public Material CurrentFirstAvatar { get; set; }

        protected bool blockGesturesDuringMelee = true;

        #region ICustomHero Setup
        void ICustomHero.PrefabSetup()
        {
            foreach (BroBase vanilla in GetComponents<BroBase>())
            {
                if (vanilla != this)
                {
                    LoadHero.CopySerializedFields(vanilla, this);
                }
            }
        }
        #endregion

        #region BroBase Methods
        protected override void Awake()
        {
            try
            {
                this.StandardBeforeAwake();

                if (Info.BetterAnimationDefaulted)
                {
                    Brominator awakePrefab = HeroController.GetHeroPrefab(HeroType.Brominator).As<Brominator>();
                    if (awakePrefab != null)
                    {
                        if (!Info.beforeAwake.ContainsKey("doRollOnLand"))
                        {
                            doRollOnLand = awakePrefab.doRollOnLand;
                        }
                    }
                }

                base.Awake();
                this.StandardAfterAwake();

                Brominator vanillaPrefab = HeroController.GetHeroPrefab(HeroType.Brominator).As<Brominator>();
                if (vanillaPrefab != null && humanBrominator == vanillaPrefab.humanBrominator)
                {
                    humanBrominator = material;
                }
                if (vanillaPrefab != null && humanGunBrominator == vanillaPrefab.humanGunBrominator)
                {
                    humanGunBrominator = gunMaterial;
                }
                if (vanillaPrefab != null && CurrentFirstAvatar != null && brominatorHumanAvatar == vanillaPrefab.brominatorHumanAvatar)
                {
                    brominatorHumanAvatar = CurrentFirstAvatar;
                }
            }
            catch (Exception ex)
            {
                this.LogError($"Couldn't set up '{Info?.name}' when it spawned", ex);
                enabled = false;
            }
        }

        protected override void Start()
        {
            try
            {
                this.StandardBeforeStart();
                base.Start();
                this.StandardAfterStart();
            }
            catch (Exception ex)
            {
                this.LogError($"Couldn't finish setting up '{Info?.name}' when it spawned", ex);
                enabled = false;
            }
        }

        public override void SetGestureAnimation(GestureElement.Gestures gesture)
        {
            if (blockGesturesDuringMelee && doingMelee)
            {
                return;
            }

            base.SetGestureAnimation(gesture);
        }

        protected override void SetGunPosition(float xOffset, float yOffset)
        {
            base.SetGunPosition(xOffset + CurrentGunSpriteOffset.x, yOffset + CurrentGunSpriteOffset.y);
        }

        protected override void CheckForTraps(ref float yIT)
        {
            TestVanDammeAnim self = this;
            if (usingSpecial && (self is TheBrolander || self is BronanTheBrobarian))
            {
                return;
            }

            float num = Y + yIT;
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

            DoodadAcidPool nearestAcid = Map.GetNearestAcid(X, Y + 8f, 2f);
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
                Block component = raycastHit.collider.GetComponent<Block>();
                if (component != null)
                {
                    if (raycastHit.distance < 10f && (IsMine || IsEnemy))
                    {
                        component.CheckForMine();
                    }

                    if (component.spikes != null && CanBeImpaledByGroundSpikes())
                    {
                        if (component.spikes.EvaluateImpalent(this))
                        {
                            float num2 = playerNum >= 0 ? component.spikes.spikeTrapHarmlessHeight : component.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit.point.y > groundHeight - 1f && raycastHit.distance >= num2 && raycastHit.distance < component.spikes.spikeTrapHeight && Y > component.Y + component.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
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
                Block component2 = raycastHit2.collider.GetComponent<Block>();
                if (component2 != null)
                {
                    if (raycastHit2.distance < 10f && (IsMine || IsEnemy))
                    {
                        component2.CheckForMine();
                    }

                    if (component2.spikes != null && CanBeImpaledByGroundSpikes())
                    {
                        if (component2.spikes.EvaluateImpalent(this))
                        {
                            float num3 = playerNum >= 0 ? component2.spikes.spikeTrapHarmlessHeight : component2.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit2.point.y > groundHeight - 1f && raycastHit2.distance >= num3 && raycastHit2.distance < component2.spikes.spikeTrapHeight && Y > component2.Y + component2.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
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
                Block component3 = raycastHit3.collider.GetComponent<Block>();
                if (component3 != null)
                {
                    if (raycastHit3.distance < 10f && (IsMine || IsEnemy))
                    {
                        component3.CheckForMine();
                    }

                    if (component3.spikes != null && CanBeImpaledByGroundSpikes())
                    {
                        if (component3.spikes.EvaluateImpalent(this))
                        {
                            float num4 = playerNum >= 0 ? component3.spikes.spikeTrapHarmlessHeight : component3.spikes.spikeTrapHarmlessHeight * 0.5f;
                            if (yI < -150f && raycastHit3.point.y > groundHeight - 1f && raycastHit3.distance >= num4 && raycastHit3.distance < component3.spikes.spikeTrapHeight && Y > component3.Y + component3.spikes.spikeTrapHeight)
                            {
                                Y = Mathf.Clamp(Y, groundHeight + 2f, groundHeight + 3f);
                                yIT = 0f;
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
        #endregion
    }
}

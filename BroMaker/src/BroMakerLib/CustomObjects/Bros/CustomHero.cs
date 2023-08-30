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

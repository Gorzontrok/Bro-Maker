using System;
using System.Collections;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Loaders;
using BroMakerLib.Stats;
using UnityEngine;
using System.Collections.Generic;

namespace BroMakerLib.CustomObjects.Bros
{
    [HeroPreset("CustomHero", HeroType.Rambro)]
    public class CustomHero : BroBase, ICustomHero
    {
        [Syncronize]
        public CustomBroInfo info { get; set; }
        [Syncronize]
        public BroBase character { get; set; }


        #region Private Variable Becomes
        #region Publics
        #endregion
        #region Protected
        #endregion
        #endregion


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
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
                enabled = false;
            }
        }
    }
}

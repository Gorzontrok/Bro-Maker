using System;
using System.Collections.Generic;
using System.Linq;
using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BroCeasar", HeroType.HaleTheBro)]
    public class BroCeasarM : BroCeasar, ICustomHero
    {
        public CustomBroInfo info { get; set; }
        public BroBase character { get; set; }

        protected override void Awake()
        {
            character = this;
            info = LoadHero.currentInfo;
            try
            {
                FixNullVariableLocal();
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

        protected virtual void FixNullVariableLocal()
        {
            var bro = HeroController.GetHeroPrefab(HeroType.HaleTheBro).As<BroCeasar>();

            bulletShell = bro.bulletShell;
            miniGunSoundSpinning = bro.miniGunSoundSpinning;
            miniGunSoundWindDown = bro.miniGunSoundWindDown;
            miniGunSoundWindup = bro.miniGunSoundWindup;
        }
    }
}

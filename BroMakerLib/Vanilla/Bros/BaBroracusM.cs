using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using System;
using UnityEngine;
using System.Collections.Generic;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BaBroracus", HeroType.BaBroracus)]
    public class BaBroracusM : BaBroracus, ICustomHero
    {
        public CustomBroInfo info { get; set; }
        public BroBase character { get; set; }
        public MuscleTempleFlexEffect flexEffect { get; set; }
        public int CurrentVariant { get; set; }

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
                this.SetSprites();
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
            var bro = HeroController.GetHeroPrefab(HeroType.BaBroracus).As<BaBroracus>();
            flameSource = GetComponent<AudioSource>();
            flameSound = bro.flameSound;
            flameSoundEnd = bro.flameSoundEnd;
            projectile2 = bro.projectile2;
            projectile3 = bro.projectile3;
        }
    }
}

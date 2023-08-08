using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using RocketLib;
using System;
using UnityEngine;
using System.Linq;
using System.Text;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BroniversalSoldier", HeroType.BroniversalSoldier)]
    public class BroniversalSoldierM : BroniversalSoldier, ICustomHero
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
            var bro = HeroController.GetHeroPrefab(HeroType.BroniversalSoldier).As<BroniversalSoldier>();
            bulletShell = bro.bulletShell;
            serumParticles = this.FindChildOfName("SerumParticles").GetComponent<ParticleEmitter>();
            reviveBlastPrefab = bro.reviveBlastPrefab;
            reviveClips = bro.reviveClips;
        }
    }
}

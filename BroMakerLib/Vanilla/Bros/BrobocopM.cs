using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using System;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("Brobocop", HeroType.Brobocop)]
    public class BrobocopM : Brobocop, ICustomHero
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
            var bro = HeroController.GetHeroPrefab(HeroType.Brobocop).As<Brobocop>();

            targetSystemPrefab = bro.targetSystemPrefab;
            chargingSound = bro.chargingSound;
            finishedChargingSound = bro.finishedChargingSound;
            gunMaterialDefault = bro.gunMaterialDefault;
            gunMaterialOneLight = bro.gunMaterialOneLight;
            gunMaterialOtherLight = bro.gunMaterialOtherLight;
            gunMaterialBothLights = bro.gunMaterialBothLights;
        }
    }
}

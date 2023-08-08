using BroMakerLib.CustomObjects;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Vanilla.Bros
{
    [HeroPreset("BoondockBro", HeroType.BoondockBros)]
    public class BoondockBroM : BoondockBro, ICustomHero
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
            isLeadBro = true;
            var bro = HeroController.GetHeroPrefab(HeroType.BoondockBros).As<BoondockBro>();
            billyConnollyPrefab = bro.billyConnollyPrefab;
            secondBroMaterial = bro.secondBroMaterial;
            avatar1 = bro.avatar1;
            avatar2 = bro.avatar2;

            // RuntimeUnityEditor friendly
            gameObject.name.Insert(0, "[LEAD] ");
        }
    }
}

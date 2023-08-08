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
    [HeroPreset("AshBrolliams", HeroType.AshBrolliams)]
    public class AshBrolliamsM : AshBrolliams, ICustomHero
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
            var ash = HeroController.GetHeroPrefab(HeroType.AshBrolliams).As<AshBrolliams>();

            bulletShell = ash.bulletShell;
            chainsawStart = ash.chainsawStart;
            chainsawSpin = ash.chainsawSpin;
            chainsawWindDown = ash.chainsawWindDown;
            bloodyAvatar = ash.bloodyAvatar;
        }
    }
}

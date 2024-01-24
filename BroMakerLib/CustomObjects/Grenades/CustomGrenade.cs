using BroMakerLib.CustomObjects.Components;
using BroMakerLib.Infos;
using BroMakerLib.Loaders;
using BroMakerLib.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.CustomObjects.Grenades
{
    [HeroPreset(nameof(CustomGrenade))]
    public class CustomGrenade : Grenade
    {
        public CustomGrenadeInfo info { get; set; }

        protected override void Awake()
        {
            info = LoadGrenade.currentInfo;
            try
            {
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
    }
}

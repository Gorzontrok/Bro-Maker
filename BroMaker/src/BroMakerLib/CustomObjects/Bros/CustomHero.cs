using System;
using System.Collections;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Loaders;
using BroMakerLib.Stats;
using UnityEngine;
using Net = Networking.Networking;

namespace BroMakerLib.CustomObjects.Bros
{
    [HeroPreset("CustomHero", HeroType.Rambro)]
    public class CustomHero : BroBase, ICustomHero
    {
        [Syncronize]
        public CustomBroInfo info { get; set; }
        [Syncronize]
        public BroBase character { get; set; }


        protected override void Awake()
        {
            character = this;
            info = LoadHero.currentInfo;
            try
            {
                EnableSyncing(true, true);
                //Net.RPC(PID.TargetAll, new RpcSignature(this.SetupCustomHero));
                //Net.RPC(PID.TargetAll, new RpcSignature<object>(info.BeforeAwake), this);
                this.SetupCustomHero();
                info.BeforeAwake(this);
                base.Awake();
                //Net.RPC(PID.TargetAll, new RpcSignature<object>(info.AfterAwake), this);
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
                //Net.RPC(PID.TargetAll, new RpcSignature<object>(info.BeforeStart), this);
                info.BeforeStart(this);
                base.Start();
                info.AfterStart(this);
                //Net.RPC(PID.TargetAll, new RpcSignature<object>(info.AfterStart), this);
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
                enabled = false;
            }
        }

    }
}

using BroMakerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;

namespace BroMakerLib.Infos
{
    public class CustomGrenadeInfo : CustomBroforceObjectInfo
    {
        protected new string _defaultName = "GRENADE";

        public CustomGrenadeInfo() : base() { }
        public CustomGrenadeInfo(string name) : base(name) {}

        public string Preset { get; set; }

        public override void Initialize()
        {
            base.Initialize();
        }
        public override string SerializeJSON()
        {
            return SerializeJSON(DirectoriesManager.WeaponsDirectory);
        }

        public static Grenade TryGetGrenadeFromFile(StoredGrenade storedGrenade)
        {
            return null;
        }


    }
}

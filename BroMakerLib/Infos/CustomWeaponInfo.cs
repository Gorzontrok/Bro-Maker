using BroMakerLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Newtonsoft.Json;

namespace BroMakerLib.Infos
{
    public class CustomWeaponInfo : CustomBroforceObjectInfo
    {
        protected new string _defaultName = "WEAPON";

        public string projectileName = string.Empty;
        [JsonIgnore, EditorIgnore]
        public CustomProjectileInfo projectileInfo;

        public CustomWeaponInfo() : base() { }
        public CustomWeaponInfo(string name) : base(name) { }

        public override void Initialize()
        {
            base.Initialize();

            /*if (string.IsNullOrEmpty(projectileName))
                projectileInfo = CustomProjectileInfo.GetProjectileInfo(projectileName);*/
        }

        public Projectile GetProjectile()
        {
            return null;
        }

        public override string SerializeJSON()
        {
            return SerializeJSON(DirectoriesManager.WeaponsDirectory);
        }
    }
}

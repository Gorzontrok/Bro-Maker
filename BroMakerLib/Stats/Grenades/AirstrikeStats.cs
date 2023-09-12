using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Stats.Grenades
{
    public class AirstrikeStats : GrenadeStats
    {
        public string projectileName = string.Empty;
        /// <summary>
        /// Unimplemented
        /// </summary>
        public int numberOfProjectile = 5;

        public Projectile GetProjectile()
        {
            return null;
        }
    }
}

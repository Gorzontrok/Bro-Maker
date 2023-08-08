using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Stats.Grenades
{
    public class GrenadeClusterStats : GrenadeStats
    {
        public float startRadianAngle = 1.5707964f;
        public int wavesOfClusters = 1;
        public float waveDelay = 0.1f;
        public float forceM = 0.5f;
        // Custom
        public string smallGrenadeName = string.Empty;
    }
}

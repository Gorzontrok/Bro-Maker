using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BroMakerLib.Stats.SortOut
{
    public class AcidStats
    {
        public bool canBeCoveredInAcid = true;
        /// <summary>
        /// Time it takes to melt after receive acid. (It's not in seconds)
        /// </summary>
        public float acidMeltTimer = 1f;
        /// <summary>
        /// During how much time the slime particle will spawn on the ground. (It's not in seconds)
        /// </summary>
        public float acidParticleTimer = 0.1f;
        public float meltDuration = 0.7f;
    }
}

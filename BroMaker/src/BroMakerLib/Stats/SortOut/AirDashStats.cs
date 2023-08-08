using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BroMakerLib.Stats.SortOut
{
    public class AirDashStats
    {
        public bool canAirdash = false;
        public bool airdashDownAvailable = false;
        public bool airdashLeftAvailable = false;
        public bool airdashRightAvailable = false;
        public bool airdashUpAvailable = false;
        public float airdashMaxTime = 0.5f;
        public float dashSpeedM = 1f;
        public float defaultAirdashDelay = 0.15f;
    }
}

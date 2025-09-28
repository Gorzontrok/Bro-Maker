using System;
using System.Collections.Generic;

namespace BroMakerLib.Unlocks
{
    public class BroUnlockProgressData
    {
        public Dictionary<string, BroUnlockState> BroStates { get; set; } = new Dictionary<string, BroUnlockState>();
        public int LastKnownTotalRescues { get; set; }
        public int Version { get; set; } = 1;

        public static BroUnlockProgressData MigrateData(BroUnlockProgressData oldData)
        {
            switch(oldData.Version)
            {
                case 1:
                    return oldData;
                default:
                    return oldData;
            }
        }
    }
}
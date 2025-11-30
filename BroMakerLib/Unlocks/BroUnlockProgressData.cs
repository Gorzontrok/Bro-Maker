using System.Collections.Generic;

namespace BroMakerLib.Unlocks
{
    public class BroUnlockProgressData
    {
        public Dictionary<string, BroUnlockState> BroStates { get; set; } = new Dictionary<string, BroUnlockState>();
        public List<string> PendingUnlocks { get; set; } = new List<string>();
        public int TotalRescues { get; set; } = 0;
        public int Version { get; set; } = 1;

        public static BroUnlockProgressData MigrateData(BroUnlockProgressData oldData)
        {
            switch (oldData.Version)
            {
                case 1:
                    return oldData;
                default:
                    return oldData;
            }
        }
    }
}

using System;

namespace BroMakerLib.Unlocks
{
    public class BroUnlockState
    {
        public string BroName { get; set; }
        public bool IsUnlocked { get; set; }
        public UnlockMethod ConfiguredMethod { get; set; }
        public int TargetRescueCount { get; set; }
        public int OriginalRescueCount { get; set; }
        public string UnlockLevelPath { get; set; }
        public string UnlockLevelName { get; set; }
        public DateTime FirstSeenDate { get; set; }
        public DateTime? UnlockedDate { get; set; }
    }
}

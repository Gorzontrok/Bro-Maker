namespace BroMakerLib.Unlocks
{
    public class BroUnlockConfig
    {
        public UnlockMethod Method { get; set; } = UnlockMethod.AlwaysUnlocked;
        public int RescueCountRequired { get; set; } = 10;
        public string UnlockLevelPath { get; set; }
        public string UnlockLevelName { get; set; }
    }
}

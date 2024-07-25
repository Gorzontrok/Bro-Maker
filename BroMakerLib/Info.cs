using UnityModManagerNet;

namespace BroMakerLib
{
    public static class Info
    {
        public const string NAME = nameof(BroMakerLib);
        public const string AUTHOR = "Gorzontrok";
        public const string VERSION = "2.4.0";

        public static System.Version ParsedVersion
        {
            get
            {
                if (_parsedVersion == null)
                    _parsedVersion = UnityModManager.ParseVersion(VERSION);
                return _parsedVersion;
            }
        }
        private static System.Version _parsedVersion;

        // Used for when BroMaker makes breaking changes that will require Bro mods to be updated
        public const string MINIMUMVERSION = "2.4.0";

        public static System.Version ParsedMinimumVersion
        {
            get
            {
                if (_parsedMinimumVersion == null)
                    _parsedMinimumVersion = UnityModManager.ParseVersion(MINIMUMVERSION);
                return _parsedMinimumVersion;
            }
        }
        private static System.Version _parsedMinimumVersion;
    }
}

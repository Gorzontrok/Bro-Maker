using UnityModManagerNet;

namespace BroMakerLib
{
    public static class Info
    {
        public const string NAME = nameof(BroMakerLib);
        public const string AUTHOR = "Gorzontrok";
        public const string VERSION = "2.5.0";
        // Used to show a warning that if bros are below this version they may experience bugs
        public const string SUGGESTEDMINIMUMVERSION = "2.4.0";
        // Used for when BroMaker makes breaking changes that will require Bro mods to be updated
        public const string MINIMUMVERSION = "2.3.0";

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

        public static System.Version ParsedSuggestedMinimumVersion
        {
            get
            {
                if (_parsedSuggestedMinimumVersion == null)
                    _parsedSuggestedMinimumVersion = UnityModManager.ParseVersion(SUGGESTEDMINIMUMVERSION);
                return _parsedSuggestedMinimumVersion;
            }
        }
        private static System.Version _parsedSuggestedMinimumVersion;

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

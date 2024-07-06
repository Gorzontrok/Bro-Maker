using UnityModManagerNet;

namespace BroMakerLib
{
    public static class Info
    {
        public const string NAME = nameof(BroMakerLib);
        public const string AUTHOR = "Gorzontrok";
        public const string VERSION = "2.3.4";

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
    }
}

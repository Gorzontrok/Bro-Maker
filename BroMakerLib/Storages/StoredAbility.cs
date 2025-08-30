using System.IO;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;

namespace BroMakerLib.Storages
{
#pragma warning disable 659
    public struct StoredAbility : IStoredObject
    {
        public bool IsEmpty
        {
            get
            {
                return name.IsNullOrEmpty() && path.IsNullOrEmpty();
            }
        }

        public string name { get; set; }
        public string path { get; set; }

        public AbilityInfo info;

        public StoredAbility(string path)
        {
            this.path = path;
            name = GetInfo(path).name;
            info = null;
        }

        public StoredAbility(AbilityInfo aInfo)
        {
            info = aInfo;
            name = aInfo.name;
            this.path = aInfo.path;
        }

        public static AbilityInfo GetInfo(string path)
        {
            AbilityInfo info = null;

            BMLogger.Debug($"Start Deserialization of '{path}'");
            string extension = Path.GetExtension(path).ToLower();
            if (extension == ".json")
            {
                info = AbilityInfo.DeserializeJSON<AbilityInfo>(path);
                info.path = Path.GetDirectoryName(path);
            }
            BMLogger.Debug("End Deserialization");
            return info;
        }

        public AbilityInfo GetInfo()
        {
            if (info != null)
                return info;
            return GetInfo(this.path);
        }

        public override bool Equals(object obj)
        {
            return obj is StoredAbility && ((StoredAbility)obj).name == this.name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}

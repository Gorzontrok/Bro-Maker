using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using System.IO;

namespace BroMakerLib.Storages
{
#pragma warning disable 659
    public struct StoredGrenade : IStoredObject
    {
        public string name { get; set; }
        public string path { get; set; }

        public CustomGrenadeInfo info;
        public BroMakerMod mod;

        public StoredGrenade(string path, BroMakerMod mod)
        {
            this.path = path;
            name = Path.GetFileNameWithoutExtension(this.path);
            this.mod = mod;
            info = null;
        }
        public StoredGrenade(CustomGrenadeInfo gInfo, BroMakerMod mod)
        {
            this.mod = mod;
            info = gInfo;
            this.path = info.path;
            name = info.name;
        }

        public CustomGrenadeInfo GetInfo()
        {
            if (this.info != null)
                return this.info;

            CustomGrenadeInfo info = null;

            BMLogger.Debug($"Start Deserialization of '{path}'");
            string extension = Path.GetExtension(path).ToLower();
            if (extension == ".json")
            {
                info = CustomGrenadeInfo.DeserializeJSON<CustomGrenadeInfo>(path);
                info.path = Path.GetDirectoryName(path);
            }
            BMLogger.Debug("End Deserialization");
            return info;
        }

        public override bool Equals(object obj)
        {
            return obj is StoredGrenade && ((StoredGrenade)obj).name == this.name;
        }

        public override string ToString()
        {
            return name;
        }
    }
}

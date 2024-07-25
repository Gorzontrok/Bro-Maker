using BroMakerLib.Loggers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using BroMakerLib.Infos;
using BroMakerLib.Storages;

namespace BroMakerLib
{
    public class BroMakerMod
    {
        public string Name = string.Empty;
        public string Version = "0.0.0";
        public string BroMakerVersion = "0.0.0";
        public string Author = string.Empty;
        public string ErrorMessage = string.Empty;
        // The custom objects

        public string[] Assemblies = new string[0];
        public object[] CustomBros = new object[0];
        public object[] Abilities = new object[0];
        public object[] Grenades = new object[0];

        // Not in JSON file
        [JsonIgnore]
        public string Path { get; protected set; }
        [JsonIgnore]
        public string[] BrosNames { get; protected set; }
        [JsonIgnore]
        public StoredHero[] StoredHeroes {  get; set; }
        [JsonIgnore]
        public StoredAbility[] StoredAbilities {  get; set; }
        [JsonIgnore]
        public StoredGrenade[] StoredGrenades{  get; set; }

        public static BroMakerMod TryLoad(string path)
        {
            BroMakerMod mod = JsonConvert.DeserializeObject<BroMakerMod>(File.ReadAllText(path));

            if (mod != null)
            {
                mod.Path = Directory.GetParent(path).ToString();

            }
            return mod;
        }

        public void Initialize()
        {
            if (Name.IsNullOrEmpty())
            {
                Name = Path.Substring(Directory.GetCurrentDirectory().Length - 1, Path.Length);
            }
            CustomBros = CheckFiles<CustomBroInfo>(CustomBros);
            Abilities = CheckFiles<AbilityInfo>(Abilities);

            BrosNames = GetNames<CustomBroforceObjectInfo>(CustomBros);
        }


        public virtual string SerializeJSON(string folderPath, string fileName)
        {
            Name = fileName;
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var settings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            File.WriteAllText(System.IO.Path.Combine(folderPath, fileName + ".mod.json"), json);
            return json;
        }

        private object[] CheckFiles<T>(object[] objects)
            where T : CustomBroforceObjectInfo
        {
            if (objects.IsNullOrEmpty())
                return new object[0];

            List<object> temp = new List<object>();
            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                if (obj is string && obj.As<string>().IsNotNullOrEmpty())
                {
                    var path = System.IO.Path.Combine(Path, obj.As<string>());
                    if (File.Exists(path))
                    {
                        temp.Add(obj);
                    }
                    else
                    {
                        BMLogger.Warning($"Can't find '{obj}' at '{path}'");
                    }
                }
                else if (obj is JObject && obj.As<JObject>().Count > 0)
                {
                    T info = obj.As<JObject>().ToObject<T>();
                    if (info != null)
                    {
                        temp.Add(info);
                        continue;
                    }
                }
            }
            return temp.ToArray();
        }

        private string[] GetNames<T>(object[] objects)
            where T : CustomBroforceObjectInfo
        {
            if (objects.IsNullOrEmpty())
                return new string[0];

            List<string> temp = new List<string>();
            foreach (var obj in objects)
            {
                if (obj == null)
                    continue;

                if (obj is string && obj.As<string>().IsNotNullOrEmpty())
                {
                    var path = System.IO.Path.Combine(Path, obj.As<string>());
                    if (File.Exists(path))
                    {
                        T info = CustomBroforceObjectInfo.DeserializeJSON<T>(path);
                        temp.Add(info.name);
                    }
                    else
                    {
                        BMLogger.Warning($"Can't find '{obj}' at '{path}'");
                    }
                }
                else if (obj is T)
                {
                    temp.Add(obj.As<T>().name);
                }
            }
            return temp.ToArray();
        }
    }
}

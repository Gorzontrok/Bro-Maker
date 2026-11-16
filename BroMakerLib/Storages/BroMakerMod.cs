using System;
using System.Collections.Generic;
using System.IO;
using BroMakerLib.Infos;
using BroMakerLib.Loggers;
using BroMakerLib.Storages;
using HarmonyLib;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BroMakerLib
{
    public class BroMakerMod
    {
        public string Name = string.Empty;
        public string Version = "0.0.0";
        public string BroMakerVersion = "0.0.0";
        public string Author = string.Empty;
        public string ErrorMessage = string.Empty;
        public bool HasHarmonyPatch = false;
        // The custom objects

        public string[] Assemblies = new string[0];
        public object[] CustomBros = new object[0];
        public object[] Abilities = new object[0];
        public object[] Grenades = new object[0];

        // Not in JSON file
        [JsonIgnore] public string Path { get; protected set; }
        [JsonIgnore] public string[] BrosNames { get; protected set; }
        [JsonIgnore] public StoredHero[] StoredHeroes { get; set; }
        [JsonIgnore] public StoredAbility[] StoredAbilities { get; set; }
        [JsonIgnore] public Harmony Harmony = null;

        public static BroMakerMod TryLoad(string path)
        {
            BroMakerMod mod = JsonConvert.DeserializeObject<BroMakerMod>(File.ReadAllText(path));

            if (mod != null)
            {
                mod.Path = Directory.GetParent(path).ToString();
                if (mod.Name.IsNullOrEmpty())
                {
                    mod.Name = new DirectoryInfo(mod.Path).Name;
                }
            }
            return mod;
        }

        public void Initialize()
        {
            using (BMLogger.Scope(Name, null))
            {
                CustomBros = CheckFiles<CustomBroInfo>(CustomBros);
                Abilities = CheckFiles<AbilityInfo>(Abilities);

                BrosNames = GetNames<CustomBroforceObjectInfo>(CustomBros);
            }
        }


        public virtual string SerializeJSON(string folderPath, string fileName)
        {
            Name = fileName;
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            string json = JsonConvert.SerializeObject(this, Formatting.Indented, settings);
            File.WriteAllText(System.IO.Path.Combine(folderPath, fileName + ".mod.json"), json);
            return json;
        }

        private object[] CheckFiles<T>(object[] objects)
            where T : CustomBroforceObjectInfo
        {
            if (objects.IsNullOrEmpty())
            {
                return new object[0];
            }

            List<object> temp = new List<object>();
            foreach (object obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }

                if (obj is string && obj.As<string>().IsNotNullOrEmpty())
                {
                    string path = System.IO.Path.Combine(Path, obj.As<string>());
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
                    T info;
                    string broName = typeof(T) == typeof(CustomBroInfo) ? (obj.As<JObject>().GetValue("name", StringComparison.OrdinalIgnoreCase) as JValue)?.Value as string : null;
                    using (BMLogger.Scope(null, broName))
                    {
                        info = obj.As<JObject>().ToObject<T>();
                    }
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
            {
                return new string[0];
            }

            List<string> temp = new List<string>();
            foreach (object obj in objects)
            {
                if (obj == null)
                {
                    continue;
                }

                if (obj is string && obj.As<string>().IsNotNullOrEmpty())
                {
                    string path = System.IO.Path.Combine(Path, obj.As<string>());
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

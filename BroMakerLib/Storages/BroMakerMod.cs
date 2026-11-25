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
        public string Author = string.Empty;
        public string ErrorMessage = string.Empty;
        public bool HasHarmonyPatch = false;
        // The custom objects

        public string[] Assemblies = new string[0];
        public object[] CustomBros = new object[0];

        // Not in JSON file
        [JsonIgnore] public string Path { get; protected set; }
        [JsonIgnore] internal string ManifestFileName { get; private set; }
        [JsonIgnore] public string[] BrosNames { get; internal set; } = new string[0];
        [JsonIgnore] public StoredHero[] StoredHeroes { get; internal set; } = new StoredHero[0];
        [JsonIgnore] public Harmony Harmony = null;
        [JsonIgnore] internal string BuiltFor = string.Empty;
        [JsonIgnore] internal List<string> RefusalTokens = new List<string>();
        [JsonIgnore] internal List<string> ScanLog = new List<string>();
        [JsonIgnore] internal string UncheckedReason = null;
        [JsonIgnore] internal Version BroMakerLibReference = null;
        [JsonIgnore] internal Version RocketLibReference = null;
        [JsonIgnore] internal List<KeyValuePair<string, string>> MissingItems = new List<KeyValuePair<string, string>>();
        [JsonIgnore] internal List<string> MissingDetails = new List<string>();
        [JsonIgnore] internal bool CanLoadAnyway = false;
        [JsonIgnore] internal string LoadedAnywayWarning = null;

        public static BroMakerMod TryLoad(string path)
        {
            BroMakerMod mod = JsonConvert.DeserializeObject<BroMakerMod>(File.ReadAllText(path));

            if (mod != null)
            {
                mod.Path = Directory.GetParent(path).ToString();
                mod.ManifestFileName = System.IO.Path.GetFileName(path);
                if (mod.Name.IsNullOrEmpty())
                {
                    mod.Name = new DirectoryInfo(mod.Path).Name;
                }
                if (mod.Assemblies == null)
                {
                    mod.Assemblies = new string[0];
                }
            }

            return mod;
        }

        public void Initialize()
        {
            using (BMLogger.Scope(Name, null))
            {
                CustomBros = CheckFiles<CustomBroInfo>(CustomBros);
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

        private object[] CheckFiles<T>(object[] objects) where T : CustomBroforceObjectInfo
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
                    string broName = (obj.As<JObject>().GetValue("name", StringComparison.OrdinalIgnoreCase) as JValue)?.Value as string;
                    using (BMLogger.Scope(null, broName))
                    {
                        try
                        {
                            info = obj.As<JObject>().ToObject<T>();
                        }
                        catch (Exception e)
                        {
                            BMLogger.Error("Failed to load an inline bro", e);
                            continue;
                        }
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
    }
}

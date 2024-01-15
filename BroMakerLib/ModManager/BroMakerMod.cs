using BroMakerLib.Loggers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Linq;

namespace BroMakerLib
{
    public class BroMakerMod
    {
        public string Name = string.Empty;
        public string Version = "0.0.0";
        public string BroMakerVersion = "0.0.0";
        public string Author = string.Empty;
        // The custom objects

        public string[] Assemblies = new string[0];
        public string[] CustomBros = new string[0];
        public string[] Abilities = new string[0];

        // Not in JSON file
        [JsonIgnore]
        public string Path { get; protected set; }
        [JsonIgnore]
        public string[] BrosNames { get; protected set; }

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
            CustomBros = CheckFiles(CustomBros);
            Abilities = CheckFiles(Abilities);
            BrosNames = CustomBros.Select(str => System.IO.Path.GetFileNameWithoutExtension(str)).ToArray();
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

        private string[] CheckFiles(string[] files)
        {
            if (files.IsNullOrEmpty())
                return new string[0];

            List<string> temp = files.ToList();
            foreach (var file in files)
            {
                if (file.IsNotNullOrEmpty())
                {
                    var path = System.IO.Path.Combine(Path, file);
                    if (!File.Exists(path))
                    {
                        temp.Remove(file);
                        Log($"Can't find '{file}' at '{path}'");
                    }
                }
            }
            return temp.ToArray();
        }


        private void Log(string message)
        {
            BMLogger.Log($"[{Name}] {message}");
        }
    }
}

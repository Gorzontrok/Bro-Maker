using BroMakerLib.Loggers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager.Param;
using UnityEngine;

namespace BroMakerLib
{
    public class BroMakerMod
    {
        [JsonProperty(Required = Required.Always)]
        public string Version;
        public string BroMakerVersion;
        public string Name;
        public string Author;
        public string Repository;
        // The custom objects
        public string[] Assemblies;
        public string[] CustomBros;
        public string[] Abilities;

        // Not in JSON file
        public bool CanBeUpdated { get; protected set; }
        public UnityModManager.Repository.Release Release {  get; protected set; }
        public string Path { get; protected set; }
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
            CustomBros = CheckFiles(CustomBros);
            Abilities = CheckFiles(Abilities);
            BrosNames = CustomBros.Select(str => System.IO.Path.GetFileNameWithoutExtension(str)).ToArray();
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

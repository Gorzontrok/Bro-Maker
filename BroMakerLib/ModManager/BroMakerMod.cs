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
        public string Id;
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

        public void CheckModUpdate()
        {
            if (Version.IsNullOrEmpty() || Repository.IsNullOrEmpty())
            {
                BMLogger.Warning($"Error checking mod update for {Id}. Version or Repository is null or empty.");
                return;
            }

            try
            {
                using (var wc = new WebClient())
                {
                    wc.Encoding = System.Text.Encoding.UTF8;
                    wc.DownloadStringCompleted += (sender, e) => { CheckModUpdate_DownloadStringCompleted(sender, e); };
                    wc.DownloadStringAsync(new Uri(Repository));
                }
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog($"Error checking mod update on '{Repository}' for {Id}.", ex);
            }
        }

        private void CheckModUpdate_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            /* The code stop at the next if with the following Error
             * System.Net.WebException: Error getting response stream (Write: The authentication or decryption has failed.): SendFailure
             * ---> System.IO.IOException: The authentication or decryption has failed.
             * ---> Mono.Security.Protocol.Tls.TlsException: The authentication or decryption has failed.
             */
            if (e.Error != null)
            {
                BMLogger.Error(e.Error.Message);
                return;
            }

            if (!e.Cancelled && !string.IsNullOrEmpty(e.Result))
            {
                try
                {
                    var repository = JsonConvert.DeserializeObject<UnityModManager.Repository>(e.Result);
                    if (repository == null || repository.Releases == null || repository.Releases.Length == 0)
                        return;

                    var release = repository.Releases.FirstOrDefault(x => x.Id == Id);
                    if (release != null && !string.IsNullOrEmpty(release.Version))
                    {
                        Release = release;
                        var ver = UnityModManager.ParseVersion(release.Version);
                        if (UnityModManager.ParseVersion(Version) < ver)
                        {
                            CanBeUpdated = true;
                            BMLogger.Log($"Update is available for {Id}.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    BMLogger.ExceptionLog($"Error checking mod update on '{Repository}' for {Id}.", ex);
                }
            }
        }

        private void Log(string message)
        {
            BMLogger.Log($"[{Name ?? Id}] {message}");
        }
    }
}

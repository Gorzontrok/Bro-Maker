using BroMakerLib.Loggers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Net;
using UnityModManagerNet;

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
        public string[] CustomBros;
        public string[] Abilities;

        // Not in JSON file
        public bool CanBeUpdated { get; protected set; }
        public UnityModManager.Repository.Release Release {  get; protected set; }
        public string Path { get; protected set; }

        public static BroMakerMod TryLoad(string path)
        {
            BroMakerMod mod = JsonConvert.DeserializeObject<BroMakerMod>(File.ReadAllText(path));
            if (mod != null)
                mod.Path = path;
            return mod;
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
    }
}

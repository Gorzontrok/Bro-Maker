using BroMakerLib.Loggers;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using Ionic.Zip;

namespace BroMakerLib.ModManager
{
    internal static class ModUpdater
    {
        public static string TempDirectory { get; private set; }
        public static bool UpdateInProgress { get; private set; }
        public static float DownloadingProgression { get; private set; }

        private static string _tempFilePath;
        private static BroMakerMod _modToUpdate;

        static ModUpdater()
        {
            TempDirectory = Path.Combine(Path.GetTempPath(), "BroMaker");
        }

        public static void CheckModsUpdates()
        {
            var mods = ModLoader.mods;
            if (mods.IsNullOrEmpty())
                return;
            foreach (var mod in mods)
            {
                mod.CheckModUpdate();
            }
        }

        public static void Update(BroMakerMod mod)
        {
            if (UpdateInProgress)
                return;
            try
            {
                UpdateInProgress = true;
                _modToUpdate = mod;

                if (!Directory.Exists(TempDirectory))
                    Directory.CreateDirectory(TempDirectory);

                var release = mod.Release;

                _tempFilePath = Path.Combine(TempDirectory, $"{mod.Id}.zip");
                BMLogger.Log($"Downloading {mod.Id} {release.Version} ...");

                using (var wc = new WebClient())
                {
                    wc.Encoding = Encoding.UTF8;
                    wc.DownloadProgressChanged += ModUpdate_DownloadProgressChanged;
                    wc.DownloadFileCompleted += ModUpdate_DownloadFileCompleted;
                    wc.DownloadFileAsync(new Uri(release.DownloadUrl), _tempFilePath);
                }
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex);
            }
        }

        private static void ModUpdate_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            DownloadingProgression = e.ProgressPercentage;
        }

        private static void ModUpdate_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                BMLogger.Log(e.Error.Message);
                return;
            }
            if (e.Cancelled)
                return;

            try
            {
                using (var zip = ZipFile.Read(_tempFilePath))
                {
                    foreach (var entry in zip.EntriesSorted)
                    {
                        if (entry.IsDirectory)
                        {
                            Directory.CreateDirectory(Path.Combine(_modToUpdate.Path, entry.FileName));
                        }
                        else
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(_modToUpdate.Path, entry.FileName)));
                            using (FileStream fs = new FileStream(Path.Combine(_modToUpdate.Path, entry.FileName), FileMode.Create, FileAccess.Write))
                            {
                                entry.Extract(fs);
                            }
                        }
                    }
                }

               BMLogger.Log($"{_modToUpdate.Id} Updated.");
            }
            catch (Exception ex)
            {
                BMLogger.ExceptionLog(ex.Message);
            }

            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }

            UpdateInProgress = false;
            DownloadingProgression = 0;
        }
    }
}

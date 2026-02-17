using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Windows;
using Gathering_the_Magic.DeckEdit.Data;
using Newtonsoft.Json.Linq;

namespace Gathering_the_Magic.DeckEdit.UI
{
    /// <summary>
    /// Interaktionslogik für UpdateSplash.xaml
    /// </summary>
    public partial class UpdateSplash
    {
        public UpdateSplash(Version _localVersion, ReleaseInfo _latestRelease)
        {
            localVersion = _localVersion;
            latestRelease = _latestRelease;
            InitializeComponent();
            Github.Init();
        }

        private async void updateSplash_Loaded(object _sender, RoutedEventArgs _e)
        {
            try
            {
                oldVersionTextBlock.Text = localVersion == null ? "Not Installed" : $"Installed Version: v{localVersion}";
                newVersionTextBlock.Text = $"Online Version: v{latestRelease.Version}";

                await beginUpdate();

                Close();

                StartupDialog startupDialog = new StartupDialog();
                startupDialog.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().FullName);
                App.Current.Shutdown();
            }
        }

        private Version localVersion;
        private ReleaseInfo latestRelease;
        private async Task beginUpdate()
        {
            ReleaseInfo.ReleaseFile file = latestRelease.Files.First(x => string.Equals(x.Name, "web.zip", StringComparison.InvariantCultureIgnoreCase));
            CancellationToken cancellationToken = new CancellationToken();
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, file.Url);
            req.Headers.Add("Accept", "application/octet-stream");
            HttpResponseMessage response = await Github.Client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead);
            long size = file.Size;
            using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);
            using MemoryStream mem = new MemoryStream();

            titleTextBlock.Text = $"Downloading";
            progressBar.Value = 0;
            progressBar.Maximum = size;
            progressTextBlock.Text = "0 %";
            await download.CopyToAsync(mem, 1024, (long value) =>
            {
                Dispatcher.Invoke(() =>
                {
                    progressBar.Value = value;
                    progressTextBlock.Text = (Convert.ToDouble(value) / size).ToString("0.00 %");
                });
            }, cancellationToken);
            mem.Position = 0;

            titleTextBlock.Text = "Extracting";
            progressBar.Value = 0;
            progressTextBlock.Text = "0 %";
            Directory.Clear(StartUp.WebFolderPath);
            using ZipArchive archive = new ZipArchive(mem, ZipArchiveMode.Read);
            long totalSize = archive.Entries.Sum(x => x.Length);
            long extractedSize = 0;
            progressBar.Maximum = totalSize;
            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                if (entry.FullName.EndsWith("/")) continue;

                string filePath = Path.Combine(StartUp.WebFolderPath, entry.FullName);
                using (Stream fs = File.OpenCreate(filePath))
                using (Stream zs = entry.Open())
                    zs.CopyTo(fs);
                extractedSize += entry.Length;

                progressBar.Value = extractedSize;
                progressTextBlock.Text = (Convert.ToDouble(extractedSize) / totalSize).ToString("0.00 %");
            }

            File.WriteAllText(StartUp.VersionFilePath, latestRelease.Version.ToString());
            File.WriteAllText(StartUp.ReleaseNotesFilePath, latestRelease.Notes);
        }
    }
}
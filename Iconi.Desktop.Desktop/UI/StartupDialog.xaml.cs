using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Gathering_the_Magic.DeckEdit.Data;

namespace Gathering_the_Magic.DeckEdit.UI
{
    /// <summary>
    /// Interaktionslogik für ConfigDialog.xaml
    /// </summary>
    public partial class StartupDialog
    {
        public StartupDialog()
        {
            InitializeComponent();
            Github.Init();
        }

        private Version localVersion;
        private ReleaseInfo latestRelease;

        private async void startupDialog_Loaded(object _sender, RoutedEventArgs _e)
        {
            #region check core
            Version currentCoreVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            ReleaseInfo latestCoreRelease = await Github.GetLatestRelease("Juvinhel", "Iconi.Desktop");
            if (currentCoreVersion < latestCoreRelease.Version)
            {
                MessageBox.Show(
                    $"A new version of the core application is available (v{latestCoreRelease.Version}).\nYou are currently using v{currentCoreVersion}.\n\nPlease update the core application first before using the web application.\n\nDo you want to open the download page now?",
                    "Update Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "https://github.com/Juvinhel/Iconi.Desktop/releases/latest",
                    UseShellExecute = true
                });
                Application.Current.Shutdown();
                return;
            }
            #endregion

            libraryFolderHeader.FolderPath = Config.Current.LibraryFolderPath;

            if (File.Exists(StartUp.VersionFilePath))
                localVersion = Version.Parse(File.ReadAllText(StartUp.VersionFilePath));

            latestRelease = await Github.GetLatestRelease("Juvinhel", "Iconi.Web");

            oldVersionTextBlock.Text = localVersion == null ? "Not Installed" : $"Installed Version: v{localVersion}";
            newVersionTextBlock.Text = $"Online Version: v{latestRelease.Version}";

            if (localVersion != null)
                startAppGrid.Visibility = Visibility.Visible;

            if (localVersion == null)
                startUpdateTextBlock.Text = "Install App";

            if (localVersion == latestRelease.Version)
                startUpdateTextBlock.Text = "Repair App";
        }

        private void startupDialog_Closing(object _sender, RoutedEventArgs _e)
        {
            if (Config.Current.LibraryFolderPath != libraryFolderHeader.FolderPath)
            {
                Config.Current.LibraryFolderPath = libraryFolderHeader.FolderPath;
                Config.Save();
            }
        }

        private void startUpdateHyperLink_Click(object _sender, RoutedEventArgs _e)
        {
            Close();
            UpdateSplash updateSplash = new UpdateSplash(localVersion, latestRelease);
            updateSplash.Show();
        }

        private void startButton_Click(object _sender, RoutedEventArgs _e)
        {
            MainWindow.Current.Start();
            Close();
        }
    }
}

using System.Diagnostics;
using System.Net;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using Celestial;
using Celestial.Components;
using Gathering_the_Magic.DeckEdit.Data;
using Lemon;
using Lemon.Text.Matching;
using Lemon.Threading;
using Microsoft.Web.WebView2.Core;

namespace Gathering_the_Magic.DeckEdit.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            Current = this;
            InitializeComponent();
        }

        static public MainWindow Current { get; private set; }

        private void mainWindow_Loaded(object _sender, RoutedEventArgs _e)
        {
            initializeTitle();

            Delay.Start(10, () =>
            {
                if (!Debugger.IsAttached)
                {
                    StartupDialog startupDialog = new StartupDialog();
                    startupDialog.Show();
                } 
                else
                    MainWindow.Current.Start();
            });
        }

        #region Title
        private TextBlock openFileTextBlock;
        private void initializeTitle()
        {
            DependencyObject titleBar = GetTemplateChild("PART_TitleBar");
            openFileTextBlock = titleBar.FindChild<TextBlock>(x => x.Name == "openFileTextBlock");
        }
        #endregion

        public void Start()
        {
            initWebView();
        }

        private CoreWebView2Environment cwv2Environment;
        private async Task initWebView()
        {
            string cacheFolderPath = Program.MyFolderPath;
            if (cwv2Environment == null)
            {
                CoreWebView2EnvironmentOptions options = new CoreWebView2EnvironmentOptions();
                cwv2Environment = await CoreWebView2Environment.CreateAsync(null, cacheFolderPath, options);
            }
            await webView.EnsureCoreWebView2Async(cwv2Environment);

            webView.CoreWebView2.NewWindowRequested += coreWebView2_NewWindowRequested;

            string page = "";

            if (Debugger.IsAttached)
            {
                webView.CoreWebView2.AddWebResourceRequestedFilter("http://localhost:5424/library/*", CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
                webView.CoreWebView2.WebResourceRequested += coreWebView2_WebResourceRequested;
                webView.Source = new Uri("http://localhost:5424/" + page);
            }
            else
            {
                webView.CoreWebView2.AddWebResourceRequestedFilter("https://web.example/*", CoreWebView2WebResourceContext.All, CoreWebView2WebResourceRequestSourceKinds.All);
                webView.CoreWebView2.WebResourceRequested += coreWebView2_WebResourceRequested;
                webView.Source = new Uri(virtualHost + "/" + page);
            }
        }

        private void coreWebView2_NewWindowRequested(object _sender, CoreWebView2NewWindowRequestedEventArgs _e)
        {
            _e.Handled = true;
            Process.Start(new ProcessStartInfo
            {
                FileName = _e.Uri,
                UseShellExecute = true
            });
        }

        private string virtualHost = "https://web.example";
        private void coreWebView2_WebResourceRequested(object _sender, CoreWebView2WebResourceRequestedEventArgs _e)
        {
            CoreWebView2Deferral deferral = _e.GetDeferral();
            Url url = new Url(_e.Request.Uri);
            Task.Run(() =>
            {
                (Stream file, string mimeType) = resourceRequested(url);

                if (file != null && mimeType != null)
                    Dispatcher.Invoke(() =>
                    {
                        CoreWebView2WebResourceResponse response = webView.CoreWebView2.Environment.CreateWebResourceResponse(file, 200, "OK", "Content-Type: " + mimeType);
                        _e.Response = response;
                    });
                else
                    Dispatcher.Invoke(() =>
                    {
                        CoreWebView2WebResourceResponse response = webView.CoreWebView2.Environment.CreateWebResourceResponse(new MemoryStream(), 404, "NOT FOUND", "Content-Type: application/octet-stream");
                        _e.Response = response;
                    });

                deferral.Complete();
            });
        }

        private (Stream file, string mimeType) resourceRequested(Url _url)
        {
            if (_url.Path.StartsWith("library"))
                return interceptLibrary(_url);

            string decodedPath = WebUtility.UrlDecode(_url.Path);
            string localPath = Path.MakeRooted(Path.Combine(StartUp.WebFolderPath, decodedPath));
            if (!localPath.StartsWith(StartUp.WebFolderPath)) return (null, null);

            if (File.Exists(localPath))
            {
                string extension = Path.GetExtension(localPath);
                MemoryStream mem = new MemoryStream();
                using (FileStream fs = File.OpenRead(localPath))
                    fs.CopyTo(mem);
                mem.Position = 0;
                string mimeType = MimeTypes.MimeTypeMap.GetMimeType(extension.ToLower().TrimStart("."));

                return (mem, mimeType);
            }

            return (null, null);
        }

        private (Stream file, string mimeType) interceptLibrary(Url _url)
        {
            string path = _url.Path;
            string virtualPath = path.Substring("library".Length).TrimStart("/");
            string decodedPath = WebUtility.UrlDecode(virtualPath);

            string localPath = Path.Combine(Config.Current.LibraryFolderPath, decodedPath);
            string fileName = Path.GetFileName(localPath);
            if (string.Equals(fileName, "listing.txt"))
            {
                string listingFolderPath = Path.GetParentDirectory(localPath);
                StringBuilder sb = new StringBuilder();
                Queue<string> folderPaths = new Queue<string>();
                folderPaths.Enqueue(listingFolderPath);
                while (folderPaths.Count > 0)
                {
                    string folderPath = folderPaths.Dequeue();
                    foreach (string subfolderPath in Directory.GetDirectories(folderPath, false))
                    {
                        sb.AppendLine("library/" + Path.MakeRelative(Config.Current.LibraryFolderPath, subfolderPath).Replace("\\", "/") + "/");
                        folderPaths.Enqueue(subfolderPath);
                    }
                    foreach (string filePath in Directory.GetFiles(folderPath, false))
                        sb.AppendLine("library/" + Path.MakeRelative(Config.Current.LibraryFolderPath, filePath).Replace("\\", "/"));
                }

                MemoryStream mem = new MemoryStream();
                using (StreamWriter sw = new StreamWriter(mem, leaveOpen: true))
                    sw.Write(sb.ToString());
                mem.Position = 0;
                return (mem, "text/plain");
            }
            else if (Directory.Exists(localPath))
            {
                string html = createDirectoryListing(localPath);
                MemoryStream mem = new MemoryStream();
                using (StreamWriter sw = new StreamWriter(mem, leaveOpen: true))
                    sw.Write(html);
                mem.Position = 0;

                return (mem, "text/html");
            }
            else
            {
                string extension = Path.GetExtension(localPath);
                MemoryStream mem = new MemoryStream();
                using (FileStream fs = File.OpenRead(localPath))
                    fs.CopyTo(mem);
                mem.Position = 0;
                string mimeType = MimeTypes.MimeTypeMap.GetMimeType(extension.ToLower().TrimStart("."));

                return (mem, mimeType);
            }
        }

        private string createDirectoryListing(string _folderPath)
        {
            Helper.DirectoryListing directoryListing = new Helper.DirectoryListing();
            directoryListing.BaseUrl = "/library";
            directoryListing.RootFolderPath = Config.Current.LibraryFolderPath;
            directoryListing.FolderPath = _folderPath;
            return directoryListing.TransformText();
        }
    }
}
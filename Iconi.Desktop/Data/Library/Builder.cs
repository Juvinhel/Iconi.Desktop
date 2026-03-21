using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using Lemon.Model;
using Lemon.Threading;
using Newtonsoft.Json;
using SVGColorExtractor.Data;

namespace Iconi.Desktop.Data.Library
{
    sealed public class Builder
    {
        public Builder(string _rootFolderPath, bool _update, int _depth, CancellationToken _cancellationToken = default)
        {
            RootFolderPath = _rootFolderPath;
            Update = _update;
            Depth = _depth;
            CancellationToken = _cancellationToken;
        }

        private void initSVGColorExtractor()
        {
            Thread newWindowThread = new Thread(() =>
            {
                svgColorExtractorWindow = new SVGColorExtractor.UI.MainWindow();
                svgColorExtractorWindow.IsReadOnly = true;
                svgColorExtractorWindow.Show();
                svgColorExtractorWindow.Hide();
                System.Windows.Threading.Dispatcher.Run();
            });
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();

            while (svgColorExtractorWindow == null || svgColorExtractorWindow.Dispatcher.Invoke(() => !svgColorExtractorWindow.IsLoaded)) 
                Thread.Sleep(10);
        }

        public string RootFolderPath { get; private set; }
        public bool Update { get; private set; }
        public int Depth { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        private SVGColorExtractor.UI.MainWindow svgColorExtractorWindow;
        public ProgressJob Run()
        {
            return new ProgressJob((p) =>
            {
                string libraryFilePath = Path.Combine(RootFolderPath, "library.json");
                List<Folder> folders = new List<Folder>();
                List<string> filePaths = Directory.GetFiles(RootFolderPath, "svg").ToList();

                if (Update && Lemon.IO.File.Exists(libraryFilePath))
                {
                    string text = Lemon.IO.File.ReadAllText(libraryFilePath);
                    folders = JsonConvert.DeserializeObject<List<Folder>>(text);
                }

                initSVGColorExtractor();
                p.Total = filePaths.Count;
                foreach (string filePath in filePaths)
                {
                    if (CancellationToken.IsCancellationRequested) break;

                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    string extension = Path.GetExtension(filePath);
                    string path = Path.MakeRelative(RootFolderPath, filePath).Trim("\\").Replace("\\", "/");

                    Folder folder = getOrCreateFolder(folders, path, 1);
                    if (folder.Files.Any(x => x.Name == fileName && x.Extension == extension))
                    {
                        ++p.Done;
                        continue;
                    }

                    List<string> tags = parseTags(path).ToList();
                    Chroma chroma = svgColorExtractorWindow.Analyze(filePath).Await();
                    if (chroma != Chroma.Unknown) tags.Add(chroma.ToString().ToLower());
                    tags.Add(extension.ToLower());

                    File file = new File();
                    file.Url = path;
                    file.Name = fileName;
                    file.Extension = extension;
                    file.Tags.AddRange(tags.Distinct());

                    folder.Files.Add(file);
                    ++p.Done;
                }
                svgColorExtractorWindow.Dispatcher.Invoke(() => svgColorExtractorWindow.Close());

                string result = JsonConvert.SerializeObject(folders);
                Lemon.IO.File.WriteAllText(libraryFilePath, result);
            });
        }

        private Folder getOrCreateFolder(IList<Folder> _folders, string _path, int _depth)
        {
            (string current, string remaining) = _path.SplitFirst("/");
            Folder folder = _folders.FirstOrDefault(x => x.Name == current);
            if (folder == null)
            {
                folder = new Folder() { Name = current };
                _folders.Add(folder);
            }

            if (remaining.Contains("/") && (Depth == 0 || _depth < Depth))
                return getOrCreateFolder(folder.Folders, remaining, _depth + 1);
            return folder;
        }

        private IEnumerable<string> parseTags(string _path)
        {
            return _path.SplitLast(".").first.Split("/", "-").Select(x => refineTag(x)).Where(x => !checkExclusion(x)).Distinct();
        }

        private string refineTag(string _tag)
        {
            _tag = _tag.ToLower();
            _tag = _tag.Trim();
            _tag = _tag.Replace("_", " ");
            while (_tag.Contains("  ")) _tag = _tag.Replace("  ", " ");
            return _tag;
        }

        private bool checkExclusion(string _tag)
        {
            if (Regex.IsMatch(_tag, "^[0-9]{5,}$")) return true;
            return false;
        }
    }
}
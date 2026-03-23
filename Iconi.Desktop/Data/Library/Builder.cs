using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageMagick;
using Lemon.Error;
using Lemon.Model;
using Lemon.Text;
using Lemon.Threading;
using Newtonsoft.Json;
using SVGColorExtractor.Data;

namespace Iconi.Desktop.Data.Library
{
    sealed public class Builder
    {
        public Builder(string _rootFolderPath, bool _update, IEnumerable<Regex> _folderExclusions, IEnumerable<Regex> _tagExclusions, int _maxDepth, CancellationToken _cancellationToken = default)
        {
            RootFolderPath = _rootFolderPath;
            Update = _update;
            FolderExclusions = _folderExclusions.ToList().AsReadOnly();
            TagExclusions = _tagExclusions.ToList().AsReadOnly();
            MaxDepth = _maxDepth;
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

        public IReadOnlyList<Regex> FolderExclusions { get; private set; }
        public IReadOnlyList<Regex> TagExclusions { get; private set; } 
        public int MaxDepth { get; private set; }

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
                    try
                    {
                        if (CancellationToken.IsCancellationRequested) break;

                        insertFile(folders, filePath);
                        ++p.Done;
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.Handle(ex, new Dictionary<string, object> { { "Filepath", filePath } });
                    }
                svgColorExtractorWindow.Dispatcher.Invoke(() => svgColorExtractorWindow.Close());

                string result = JsonConvert.SerializeObject(folders);
                Lemon.IO.File.WriteAllText(libraryFilePath, result);
            });
        }

        private void insertFile(IList<Folder> _folders, string _filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(_filePath);
            string extension = Path.GetExtension(_filePath);
            string path = Path.MakeRelative(RootFolderPath, Path.GetParentDirectory(_filePath)).Trim("\\").Replace("\\", "/");
            string url = path + "/" + fileName + "." + extension;
            extension = extension.ToLower();
            path = removePathExclusions(path);

            if (string.IsNullOrEmpty(path)) return; // exclude files in root
            if (extension != "svg") return; // exclude non-svg files

            Folder folder = getOrCreateFolder(_folders, path, 1);
            if (folder.Files.Any(x => x.Name == fileName && x.Extension == extension))
                return; // file already exists, skip

            List<string> tags = parseTags(path + "/" + fileName).ToList();
            Chroma chroma = svgColorExtractorWindow.Analyze(_filePath).Await();
            if (chroma != Chroma.Unknown) tags.Add(chroma.ToString().ToLower());
            tags.Add(extension);

            File file = new File();
            file.Url = url;
            file.Name = fileName;
            file.Extension = extension;
            file.Tags.AddRange(tags.Distinct());

            folder.Files.Add(file);
        }

        private string removePathExclusions(string _path)
        {
            string ret = string.Empty;
            foreach(string part in _path.Split("/"))
            {
                if (FolderExclusions.Any(x => x.IsMatch(part))) continue;
                ret += (ret.Length > 0 ? "/" : "") + part;
            }
            return ret;
        }

        private Folder getOrCreateFolder(IList<Folder> _folders, string _path, int _depth)
        {
            string current; string remaining;
            if (MaxDepth > 0 && _depth >= MaxDepth)
            {
                current = _path;
                remaining = null;
            }
            else
                (current, remaining) = _path.SplitFirst("/");

            Folder folder = _folders.FirstOrDefault(x => x.Name == current);
            if (folder == null)
            {
                folder = new Folder() { Name = current };
                _folders.Add(folder);
            }

            if (!string.IsNullOrEmpty(remaining))
                return getOrCreateFolder(folder.Folders, remaining, _depth + 1);
            return folder;
        }

        private IEnumerable<string> parseTags(string _path)
        {
            return _path.SplitLast(".").first.Split("/", "-").Select(x => refineTag(x)).
                Where(tag => !TagExclusions.Any(exclusion => exclusion.IsMatch(tag)));
        }

        private string refineTag(string _tag)
        {
            _tag = _tag.ToLower();
            _tag = _tag.Trim();
            _tag = _tag.Replace("_", " ");
            while (_tag.Contains("  ")) _tag = _tag.Replace("  ", " ");
            return _tag;
        }
    }
}
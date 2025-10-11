using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemon.Model;
using Newtonsoft.Json;

namespace Gathering_the_Magic.DeckEdit.Data.Library
{
    sealed public class Library
    {
        public ProgressJob Scan(string _rootFolderPath)
        {
            return new ProgressJob((p) =>
            {
                List<string> filePaths = Directory.GetFiles(_rootFolderPath, true, "svg").ToList();
                p.Total = filePaths.Count;
                foreach(string filePath in filePaths)
                {
                    string relativePath = Path.MakeRelative(_rootFolderPath, filePath).Replace("\\", "/").Trim("/");
                    insertFile(relativePath);
                    ++p.Done;
                }
                p.Done = p.Total;
            });
        }

        private void insertFile(string _filePath)
        {
            if (!_filePath.Contains("/")) return; // Files in root folder are ignored.

            string remainingPath = _filePath;
            Folder current = null;
            do
            {
                (string folderName, remainingPath) = remainingPath.SplitFirst("/");
                current = getOrCreateFolder(current, folderName);
            } while (remainingPath.Contains("/"));

            File file = new File()
            {
                Url = _filePath,
                Name = Path.GetFileNameWithoutExtension(_filePath),
                Extension = Path.GetExtension(_filePath).ToLower(),
            };
            file.Tags.AddRangeDistinct(Helper.ParseTags(_filePath));
            current.Files.Add(file);
        }

        private Folder getOrCreateFolder(Folder _parent, string _folderName)
        {
            Folder folder = (_parent?.Folders ?? Folders).FirstOrDefault(f => f.Name == _folderName);
            if (folder == null)
            {
                folder = new Folder() { Name = _folderName, };
                (_parent?.Folders ?? Folders).Add(folder);
            }
            return folder;
        }

        [JsonProperty("folders")]
        public List<Folder> Folders { get; } = new List<Folder>();
    }
}

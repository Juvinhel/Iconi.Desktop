using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gathering_the_Magic.DeckEdit.Helper
{
    public partial class DirectoryListing
    {
        public string BaseUrl { get; set; }
        public string RootFolderPath { get; set; }
        public string FolderPath { get; set; }

        public (string name, string path, long size, DateTime lastWriteTime) Self
        {
            get
            {
                return GetFolder(FolderPath);
            }
        }

        public (string name, string path, long size, DateTime lastWriteTime) Parent
        {
            get
            {
                if (FolderPath == RootFolderPath) return (null, null, 0, new DateTime());

                return GetFolder(Path.GetParentDirectory(FolderPath));
            }
        }

        public IEnumerable<(string name, string path, long size, DateTime lastWriteTime)> Folders
        {
            get
            {
                foreach (string folderPath in Directory.GetDirectories(FolderPath, false))
                    yield return GetFolder(folderPath);
            }
        }

        public IEnumerable<(string name, string path, long size, DateTime lastWriteTime)> Files
        {
            get
            {
                foreach (string filePath in Directory.GetFiles(FolderPath, false))
                    yield return GetFile(filePath);
            }
        }

        public (string name, string path, long size, DateTime lastWriteTime) GetFolder(string _folderPath)
        {
            string name = Path.GetFileName(_folderPath);
            string path = Path.MakeRelative(RootFolderPath, _folderPath);
            string url = BaseUrl + "/" + UrlEncoder.PathEncoder.Encode(path) + "/";
            DateTime lastWriteTime = Directory.GetInfo(_folderPath).LastWriteTime.SetKind(DateTimeKind.Utc);
            return (name, url, 0, lastWriteTime);
        }

        public (string name, string path, long size, DateTime lastWriteTime) GetFile(string _filePath)
        {
            string name = Path.GetFileName(_filePath);
            string path = Path.MakeRelative(RootFolderPath, _filePath);
            string url = BaseUrl +  "/" + UrlEncoder.PathEncoder.Encode(path);
            FileInfo info = File.GetInfo(_filePath);
            long size = info.Length;
            DateTime lastWriteTime = info.LastWriteTime.SetKind(DateTimeKind.Utc);
            return (name, url, size, lastWriteTime);
        }
    }
}
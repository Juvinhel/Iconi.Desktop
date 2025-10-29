using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemon.Model;

namespace Gathering_the_Magic.DeckEdit.Data.Listing
{
    sealed public class Listing
    {
        private List<string> files = new List<string>();
        public IReadOnlyList<string> Files { get { return files.AsReadOnly(); } }

        public ProgressJob Scan(string _rootFolderPath)
        {
            return new ProgressJob((p) =>
            {
                p.Total = 0;
                Queue<string> folderPaths = new Queue<string>();
                folderPaths.Enqueue(_rootFolderPath);
                while (folderPaths.Count > 0)
                {
                    ++p.Total;
                    string folderPath = folderPaths.Dequeue();
                    foreach (string subfolderPath in Directory.GetDirectories(folderPath, false))
                        folderPaths.Enqueue(subfolderPath);
                    foreach (string filePath in Directory.GetFiles(folderPath, false, "svg"))
                        files.Add("library/" + UrlEncoder.PathEncoder.Encode(Path.MakeRelative(_rootFolderPath, filePath)));
                }
                p.Done = p.Total;
            });
        }
    }
}

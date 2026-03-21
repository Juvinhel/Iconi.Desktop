using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Iconi.Desktop.Data.Library
{
    [DebuggerDisplay("{Name,nq}")]
    sealed public class Folder
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("folders")]
        public IList<Folder> Folders { get; private set; } = new List<Folder>();
        [JsonProperty("files")]
        public IList<File> Files { get; private set; } = new List<File>();

        public bool ShouldSerializeFolders()
        {
            if (Folders == null) return false;
            return Folders.Count > 0;
        }

        public bool ShouldSerializeFiles()
        {
            if (Files == null) return false;
            return Files.Count > 0;
        }
    }
}
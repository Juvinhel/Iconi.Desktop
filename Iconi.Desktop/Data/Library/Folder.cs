using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Gathering_the_Magic.DeckEdit.Data.Library
{
    [DebuggerDisplay("{Name,nq}")]
    sealed public class Folder
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("folders")]
        public List<Folder> Folders { get; } = new List<Folder>();
        [JsonProperty("files")]
        public List<File> Files { get; } = new List<File>();
    }
}

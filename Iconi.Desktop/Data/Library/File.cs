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
    sealed public class File
    {
        [JsonProperty("url")]
        public string Url { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("extension")]
        public string Extension { get; set; }
        [JsonProperty("tags")]
        public IList<string> Tags { get; private set; } = new List<string>();
    }
}

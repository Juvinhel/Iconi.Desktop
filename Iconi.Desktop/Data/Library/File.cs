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
    sealed public class File
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("tags")]
        public List<string> Tags { get; } = new List<string>();
    }
}

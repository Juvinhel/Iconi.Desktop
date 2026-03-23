using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemon;
using Newtonsoft.Json;

namespace Iconi.Desktop.Data
{
    sealed public class Config : ConfigBase
    {
        static public readonly string FilePath = Path.Combine(Directory.Current, "Iconi.Desktop.config.user");

        static public void Load()
        {
            Config config = new Config();
            if (File.Exists(FilePath))
                config.Load(FilePath);
            Current = config;
        }

        static public void Save()
        {
            Current?.Save(FilePath);
        }

        static public Config Current { get; private set; }

        public string LibraryFolderPath { get; set; }

        [JsonProperty("library")]
        public LibraryConfig Library { get; set; } = new LibraryConfig();
    }

    sealed public class LibraryConfig
    {
        [JsonProperty("url")]
        public string Url { get; set; } = "library";

        [JsonProperty("folder-exclusions", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public IList<string> FolderExclusions { get; set; } = new List<string>() { "svg" };

        [JsonProperty("tag-exclusions", ObjectCreationHandling = ObjectCreationHandling.Replace)]
        public IList<string> TagExclusions { get; set; } = new List<string>() { "/^[0-9]{5,}$/" };

        [JsonProperty("max-depth")]
        public int MaxDepth { get; set; } = 2;
    }
}

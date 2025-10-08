using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Gathering_the_Magic.DeckEdit.Data
{
    sealed public class ReleaseInfo
    {
        static public ReleaseInfo Parse(string _json)
        {
            JObject releaseInfo = JObject.Parse(_json);
            string tagName = releaseInfo.Property("tag_name").Value.Value<string>();
            string name = releaseInfo.Property("name").Value.Value<string>();
            string body = releaseInfo.Property("body").Value.Value<string>();

            Version version = Version.Parse(tagName.TrimStart("v").SplitFirst(" ").first);
            DateTime created = releaseInfo.Property("created_at").Value.Value<DateTime>().ToLocalTime();
            DateTime updated = releaseInfo.Property("updated_at").Value.Value<DateTime>().ToLocalTime();
            DateTime published = releaseInfo.Property("published_at").Value.Value<DateTime>().ToLocalTime();
            bool prerelease = releaseInfo.Property("prerelease").Value.Value<bool>();   

            List<ReleaseFile> files = new List<ReleaseInfo.ReleaseFile>();
            foreach (JObject asset in releaseInfo.Property("assets").Value.Value<JArray>())
            {
                string fileName = asset.Property("name").Value.Value<string>();
                string fileUrl = asset.Property("url").Value.Value<string>();
                string contentType = asset.Property("content_type").Value.Value<string>();
                long size = asset.Property("size").Value.Value<long>();
                files.Add(new ReleaseFile(fileName, fileUrl, contentType, size));
            }

            return new ReleaseInfo(name, body, version, created, updated, published, prerelease, files);
        }

        public ReleaseInfo(string _title, string _notes, Version _version, DateTime _created, DateTime _updated, DateTime _published, bool _prerelease,  IEnumerable<ReleaseFile> _files)
        {
            Title = _title;
            Notes = _notes;
            Version = _version;
            Files = _files.ToList().AsReadOnly();
            Created = _created;
            Updated = _updated;
            Published = _published;
            Prerelease = _prerelease;
        }

        public string Title { get; private set; }
        public string Notes { get; private set; }
        public Version Version { get; private set; }
        public DateTime Created { get; private set; }
        public DateTime Updated { get; private set; }
        public DateTime Published { get; private set; }
        public bool Prerelease { get; private set; }

        public IReadOnlyList<ReleaseFile> Files { get; private set; }

        sealed public class ReleaseFile
        {
            public ReleaseFile(string _name, string _url, string _contentType, long _size)
            {
                Name = _name;
                Url = _url;
                ContentType = _contentType;
                Size = _size;
            }

            public string Name { get; private set; }
            public string Url { get; private set; }
            public string ContentType { get; private set; }
            public long Size { get; private set; }
        }
    }
}
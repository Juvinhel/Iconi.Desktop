using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Gathering_the_Magic.DeckEdit.Data
{
    static public class Github
    {
        static public void Init()
        {
            if (Client == null)
            {
                Client = new HttpClient();
                Client.DefaultRequestHeaders.Add("User-Agent", "Iconi.Desktop.Updater");
                Client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
            }
        }

        static public HttpClient Client { get; set; }

        static public async Task<ReleaseInfo> GetLatestRelease(string _githubUser, string _githubRepo)
        {
            string url = $"https://api.github.com/repos/{_githubUser}/{_githubRepo}/releases/latest";
            using HttpResponseMessage response = await Client.GetAsync(url);
            string content = await response.Content.ReadAsStringAsync();
            return ReleaseInfo.Parse(content);
        }
    }
}
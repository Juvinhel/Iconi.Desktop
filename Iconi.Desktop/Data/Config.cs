using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lemon;

namespace Gathering_the_Magic.DeckEdit.Data
{
    sealed public class Config : ConfigBase
    {
        static private string configFilePath = Path.Combine(Directory.Current, "Iconi.Desktop.config.user");

        static public void Load()
        {
            Config config = new Config();
            if (File.Exists(configFilePath))
                config.Load(configFilePath);
            Current = config;
        }

        static public void Save()
        {
            Current?.Save(configFilePath);
        }

        static public Config Current { get; private set; }

        public string LibraryFolderPath { get; set; }    
    }
}

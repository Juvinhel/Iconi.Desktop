using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CommandLine;
using Iconi.Desktop.Data;
using Lemon;

namespace Iconi.Desktop
{
    static public class StartUp
    {
        [STAThread]
        static public void Main(string[] _args)
        {
            try
            {
                WebFolderPath = Path.MakeRooted(Path.Combine(Program.MyFolderPath, "web"));
                Directory.Create(WebFolderPath);
                VersionFilePath = Path.Combine(WebFolderPath, "version.txt");
                ReleaseNotesFilePath = Path.Combine(WebFolderPath, "release-notes.txt");
                CacheFolderPath = Path.MakeRooted(Path.Combine(Program.MyFolderPath, "cache"));
                Directory.Create(CacheFolderPath);

                InkScapePath = Program.Find("inkscape", "InkScape");

                Config.Load();

                App app = new App();
                app.InitializeComponent();
                app.Run();

                Config.Save();

                Directory.Clear(CacheFolderPath);
            }
            catch (Exception ex)
            {
                if (Debugger.IsAttached) throw;
                else MessageBox.Show(ex.Message, ex.GetType().Name);
            }
        }

        static public string WebFolderPath { get; private set; }
        static public string VersionFilePath { get; private set; }
        static public string ReleaseNotesFilePath { get; private set; }
        static public string CacheFolderPath { get; private set; }

        static public string InkScapePath { get; private set; } = @"[MEGA]:\Dev Apps\InkScape\inkscape.bat";
    }
}

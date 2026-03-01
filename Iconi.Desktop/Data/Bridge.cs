using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Gathering_the_Magic.DeckEdit.UI;
using Lemon.Error;
using WinCopies.Util.Commands.Primitives;

namespace Gathering_the_Magic.DeckEdit.Data
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    sealed public class Bridge
    {
        public void CopyToClipboard(object[] _fileNames, object[] _datas)
        {
            Directory.Clear(StartUp.CacheFolderPath);

            List<string> fileNames = new List<string>();
            List<string> datas = _datas.Select(x => x.ToString()).ToList();

            for (int i = 0; i < _fileNames.Length; ++i)
            {
                string fileName = _fileNames[i].ToString();
                int count = _fileNames.Take(i + 1).Where(x => string.Equals(x, fileName)).Count();
                (string name, string extension) = fileName.SplitLast(".");
                fileNames.Add(name + (count > 1 ? " (" + count + ")" : string.Empty) + "." + extension);
            }

            StringCollection fileDropList = new StringCollection();
            for (int i = 0; i < fileNames.Count; ++i)
            {
                byte[] data = Convert.FromBase64String(datas[i]);
                string filePath = Path.Combine(StartUp.CacheFolderPath, fileNames[i]);
                using (FileStream fileStream = File.OpenCreate(filePath))
                    fileStream.Write(data);
                fileDropList.Add(filePath);
            }

            Clipboard.SetFileDropList(fileDropList);
        }

        public void OpenInInkScape(object[] _fileNames, object[] _datas)
        {
            if(string.IsNullOrEmpty(StartUp.InkScapePath))
            {
                MessageBox.Show("InkScape not found!", "Error");
                ErrorHandler.Handle(new ErrorMessage("InkScape not found!", "InkScape not found on computer!"));
                return;
            }

            Directory.Clear(StartUp.CacheFolderPath);

            List<string> fileNames = new List<string>();
            List<string> datas = _datas.Select(x => x.ToString()).ToList();

            for (int i = 0; i < _fileNames.Length; ++i)
            {
                string fileName = _fileNames[i].ToString();
                int count = _fileNames.Take(i + 1).Where(x => string.Equals(x, fileName)).Count();
                (string name, string extension) = fileName.SplitLast(".");
                fileNames.Add(name + (count > 1 ? " (" + count + ")" : string.Empty) + "." + extension);
            }

            for (int i = 0; i < fileNames.Count; ++i)
            {
                byte[] data = Convert.FromBase64String(datas[i]);
                string filePath = Path.Combine(StartUp.CacheFolderPath, fileNames[i]);
                using (FileStream fileStream = File.OpenCreate(filePath))
                    fileStream.Write(data);
            }

            CommandLineBuilder builder = new CommandLineBuilder();
            foreach(string fileName in fileNames)
                builder.Add(Path.Combine(StartUp.CacheFolderPath, fileName));

            string command = builder.ToString();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.RedirectStandardError = true;
            startInfo.FileName = StartUp.InkScapePath;
            startInfo.Arguments = command;

            Process.Start(startInfo);
        }
    }
}
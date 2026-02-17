using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Gathering_the_Magic.DeckEdit.UI;

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
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatingBackgroundTasks_Project
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var t = new DirectoryInfo(@"C:\Users\Михаил\Documents\Dropbox\Скриншоты");
            while (true)
            {
                t.Refresh();
                if (DateTime.Now - t.LastWriteTime > new TimeSpan(0, 0, 2))
                    continue;
                var z = t.GetFiles();
                foreach (FileInfo capture in z)
                {
                    var newName = $"{capture.Directory.FullName + $"\\{capture.LastWriteTime.GetDateTimeFormats()[46].Replace('-', '\\').Replace(':', '-').Replace('T', ' ')}.png"}";
                    Directory.CreateDirectory(newName.Remove(newName.LastIndexOf('\\')));
                    try
                    {
                        capture.MoveTo(newName);
                    }
                    catch { }

                }
            }
        }
    }
}

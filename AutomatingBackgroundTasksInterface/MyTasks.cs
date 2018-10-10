using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutomatingBackgroundTasksInterface.Properties;

namespace AutomatingBackgroundTasksInterface
{
    public class MyTasks
    {
        public void CheckFiles(ref bool isActive)
        {
            var source = new DirectoryInfo(Settings.Default.SourcePath);

            while (isActive)
            {
                Thread.Sleep(1000);

                source.Refresh();
                var allFiles = source.GetFiles();
                if (allFiles.Length == 0)
                    continue;
                allFiles = allFiles.Where(e => Settings.Default.Extensions.Contains(e.Extension.ToLower())).ToArray();
                foreach (FileInfo capture in allFiles)
                {
                    var newName = $"{source.FullName + $"\\{capture.LastWriteTime.GetDateTimeFormats()[46].Replace('-', '\\').Replace(':', '-').Replace('T', ' ')}.{capture.Extension.ToLower()}"}";
                    Directory.CreateDirectory(newName.Remove(newName.LastIndexOf('\\')));
                    bool done = false;
                    while(!done)
                        try
                        {
                            capture.MoveTo(newName);
                            done = true;
                        }
                        catch
                        {
                            // ignored
                        }
                }
            }
        }
    }
}

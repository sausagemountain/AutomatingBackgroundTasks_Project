using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace AutomatingBackgroundTasks.Interface
{
    public static class Ext
    {
        public static FileInfo[] RecursiveGetFiles(this DirectoryInfo dir)
        {
            return dir.GetFilesRecursive().ToArray();
        }

        private static IEnumerable<FileInfo> GetFilesRecursive(this DirectoryInfo directory)
        {
            var result = directory.GetFiles().ToList();
            var dirs = directory.GetDirectories();
            foreach (var dir in dirs)
                result.AddRange(dir.GetFilesRecursive());
            return result;
        }
    }

    public class MyTasks
    {
        public static bool IsDestChildOfSource(string source, string destination)
        {
            return destination.Contains(source);
        }
        public static bool IsDestChildOfSource(DirectoryInfo source, DirectoryInfo destination)
        {
            return IsDestChildOfSource(source.FullName, destination.FullName);
        }

        public void CheckFiles(ref bool isActive, int n)
        {
            var source = new DirectoryInfo(Properties.Settings.Default.SourcePaths[n]);
            DirectoryInfo dest = source;
            if (bool.Parse(Properties.Settings.Default.UseDestinations[n]))
                dest = new DirectoryInfo(Properties.Settings.Default.DestinationPaths[n]);

            while (isActive)
            {
                Thread.Sleep(1000);

                source.Refresh();
                FileInfo[] allFiles;
                if (Properties.Settings.Default.Recursive)
                {
                    allFiles = source.RecursiveGetFiles();
                }
                else
                {
                    allFiles = source.GetFiles();
                }
                if (allFiles.Length == 0)
                    continue;
                allFiles = allFiles.Where(e => Properties.Settings.Default.Extensions.Contains(e.Extension.ToLower())).ToArray();
                foreach (FileInfo file in allFiles)
                {
                    var newName = $"{ (bool.Parse(Properties.Settings.Default.UseDestinations[n]) ? dest.FullName : source.FullName) + $"\\{file.LastWriteTime.GetDateTimeFormats()[46].Replace('-', '\\').Replace(':', '-').Replace('T', ' ')}.{file.Extension.ToLower()}"}";
                    Directory.CreateDirectory(newName.Remove(newName.LastIndexOf('\\')));
                    bool done = false;
                    while(!done)
                        try
                        {
                            file.MoveTo(newName);
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

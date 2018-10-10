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

        public void CheckFiles(ref bool isActive)
        {
            var source = new DirectoryInfo(Properties.Settings.Default.SourcePath);
            DirectoryInfo dest = source;
            if (Properties.Settings.Default.UseDestination)
                dest = new DirectoryInfo(Properties.Settings.Default.DestinationPath);

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
                foreach (FileInfo capture in allFiles)
                {
                    var newName = $"{ (Properties.Settings.Default.UseDestination ? dest.FullName : source.FullName) + $"\\{capture.LastWriteTime.GetDateTimeFormats()[46].Replace('-', '\\').Replace(':', '-').Replace('T', ' ')}.{capture.Extension.ToLower()}"}";
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AutomatingBackgroundTasks.Interface
{
    public static partial class Ext
    {
        public static bool IsDestChildOfSource(string source, string destination)
        {
            return destination.Contains(source);
        }

        public static bool IsDestChildOfSource(DirectoryInfo source, DirectoryInfo destination)
        {
            return IsDestChildOfSource(source.FullName, destination.FullName);
        }

        public static Window GetWindowParent(this FrameworkElement el)
        {
            if (el.Parent is Window w)
            {
                return w;
            }

            return (el.Parent as FrameworkElement).GetWindowParent();
        }

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
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;

namespace AutomatingBackgroundTasks.Interface
{
    [Serializable]
    [XmlInclude(typeof(MyTask))]
    public class MyTask
    {
        [XmlIgnore]
        private bool _isMoving = false;

        [XmlAttribute]
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                _isMoving = value;
                if (_isMoving) {
                    var thread = new Thread(CheckFiles);
                    thread.Start();
                }
            }
        }

        [XmlAttribute]
        public string SourcePath { get; set; } = string.Empty;
        [XmlAttribute]
        public string DestinationPath { get; set; } = string.Empty;

        [XmlAttribute]
        public bool UseDestination { get; set; } = false;
        [XmlAttribute]
        public bool Recursive { get; set; } = false;


        [XmlIgnore]
        public ObservableCollection<string> ExtensionCollection = new ObservableCollection<string>();

        [XmlArray]
        public string[] Extensions
        {
            get => ExtensionCollection.ToArray();
            set
            {
                ExtensionCollection.Clear();
                foreach (string s in value)
                {
                    ExtensionCollection.Add(s);
                }
            }
        }

        public void CheckFiles()
        {
            Console.WriteLine("Started!");
            var source = new DirectoryInfo(SourcePath);
            DirectoryInfo dest = source;
            if (UseDestination)
                dest = new DirectoryInfo(DestinationPath);

            while (IsMoving)
            {
                Thread.Sleep(1000);

                source.Refresh();
                FileInfo[] allFiles = null;

                while (allFiles == null)
                    try {
                        allFiles = Recursive ? source.RecursiveGetFiles() : source.GetFiles();
                    }
                    catch { }
                if (allFiles.Length == 0)
                    continue;
                allFiles = allFiles.Where(e => Extensions.Contains(e.Extension.ToLower())).ToArray();
                if (allFiles.Length == 0)
                    continue;

                foreach (FileInfo file in allFiles) {
                    var baseName = file.LastWriteTime.GetDateTimeFormats()[46].Replace('-', '\\').Replace(':', '-').Replace('T', ' ');

                    string newName = Path.GetFullPath(Path.Combine(UseDestination ? dest.FullName : source.FullName, $"{baseName}{file.Extension.ToLower()}"));

                    for(int i = 1;;i++)
                        try {
                            if (!File.Exists(newName)) {
                                Directory.CreateDirectory(Path.GetDirectoryName(newName));
                                file.MoveTo(newName);
                            }
                            else {
                                newName = Path.Combine(Path.GetDirectoryName(newName),
                                    $"{baseName} ({i})" + Path.GetExtension(newName));
                                continue;
                            }
                            break;
                        }
                        catch { }
                }
            }
            Console.WriteLine("Finished!");
        }
    }
}
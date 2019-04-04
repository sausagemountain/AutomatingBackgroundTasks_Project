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
    public enum NamingRules
    {
        None,
        EarliestKnownDate,
        /*
        extension is added to the end automatically
        {0} is name
        {1} is extension
        {2} is creation time
        {3} is last write time
        {4} is last access time
        */
        Custom,
    }

    [Serializable]
    public enum MoveSettings
    {
        Files,
        FilesInSubfolders,
        Folders,
        FilesAndFolders,
    }

    [Serializable]
    [XmlInclude(typeof(MyTask))]
    [XmlInclude(typeof(NamingRules))]
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
                    var thread = new Thread(CheckFiles) {
                        IsBackground = true,
                    };
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
        public NamingRules Rule { get; set; } = NamingRules.None;
        [XmlAttribute]
        public MoveSettings MoveOnly { get; set; } = MoveSettings.Files;
        [XmlAttribute]
        public string CustomName { get; set; } = "{0}";


        [XmlIgnore]
        public string DestinationWrapper
        {
            get => UseDestination? DestinationPath: string.Empty;
            set => DestinationPath = value;
        }
        [XmlIgnore]
        public ObservableCollection<string> PatternCollection = new ObservableCollection<string>();
        [XmlIgnore]
        public string CustomNameWrapper
        {
            get => Rule == NamingRules.Custom? CustomName: string.Empty;
            set => CustomName = value;
        }

        [XmlArray]
        public string[] Patterns
        {
            get => PatternCollection.ToArray();
            set
            {
                PatternCollection.Clear();
                foreach (string s in value)
                {
                    PatternCollection.Add(s);
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
                var allFiles = new List<FileSystemInfo>();

                foreach (var pattern in PatternCollection) {
                    try
                    {
                        switch (MoveOnly)
                        {
                            case MoveSettings.Files:
                                allFiles.AddRange(source.GetFiles(pattern));
                                break;
                            case MoveSettings.FilesInSubfolders:
                                allFiles.AddRange(source.GetFiles(pattern, SearchOption.AllDirectories));
                                break;
                            case MoveSettings.Folders:
                                allFiles.AddRange(source.GetDirectories(pattern));
                                break;
                            case MoveSettings.FilesAndFolders:
                                allFiles.AddRange(source.GetFiles(pattern));
                                allFiles.AddRange(source.GetDirectories(pattern));
                                break;
                        }
                    }
                    catch { }
                }

                if (allFiles.Count == 0)
                    continue;

                allFiles = allFiles.Distinct().ToList();

                if (allFiles.Count == 0)
                    continue;

                foreach (var file in allFiles) {

                    string baseName = file.Name;
                    switch (Rule) {
                        case NamingRules.None:
                            break;
                        case NamingRules.EarliestKnownDate: {
                            baseName = new[] { file.CreationTime, file.LastAccessTime, file.LastWriteTime }.Min().ToString("yyyy\\MM\\dd HH-mm-ss");
                            break;
                        }
                        case NamingRules.Custom:
                            baseName = string.Format(CustomName, file.Name, file.Extension.Trim('.'), file.CreationTime, file.LastWriteTime, file.LastAccessTime);
                            break;
                    }

                    string newName = Path.GetFullPath(Path.Combine(UseDestination ? dest.FullName : source.FullName, $"{baseName}{file.Extension}"));

                    for(int i = 1;;)
                        try {
                            if (!File.Exists(newName)) {
                                Directory.CreateDirectory(Path.GetDirectoryName(newName));
                                file.MoveTo(newName);
                            }
                            else {
                                newName = Path.Combine(Path.GetDirectoryName(newName),
                                    Path.GetFileNameWithoutExtension(baseName) +
                                    $" ({++i})" + 
                                    Path.GetExtension(baseName));
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
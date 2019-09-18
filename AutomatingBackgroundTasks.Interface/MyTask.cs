using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Data;
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
        {5} is earliest of the three dates
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
        public ObservableCollection<string> PatternCollection { get; set; } = new ObservableCollection<string>();

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

            Timer tmr = null;
            tmr = new Timer(o => {
                try {
                    if (!IsMoving) {
                        tmr.Dispose();
                        Console.WriteLine("Finished!");
                    }

                    var source = new DirectoryInfo(SourcePath);
                    if (!source.Exists) {
                        IsMoving = false;
                        return;
                    }   

                    DirectoryInfo dest = source;
                    if (UseDestination)
                        dest = new DirectoryInfo(DestinationPath);
                    var allFiles = new List<FileSystemInfo>();

                    var pc = PatternCollection;
                    if(pc.Count == 0)
                        pc = new ObservableCollection<string>(){"*"};

                    foreach (var pattern in pc)
                    {
                        try {
                            source.Refresh();
                            switch (MoveOnly) {
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
                        catch (Exception e) {
                            Console.WriteLine(e);
                        }
                    }

                    if (allFiles.Count == 0)
                        return;

                    allFiles = allFiles.Distinct().ToList();

                    if (allFiles.Count == 0)
                        return;

                    foreach (var file in allFiles)
                    {
                        if(!file.Exists)
                            continue;

                        string cn = CustomName;
                        if (Rule == NamingRules.EarliestKnownDate)
                            cn = "{5:yyyy}\\{5:MM}\\{5:dd} {5:HH}-{5:mm}-{5:ss}";
                        
                        string ext = file.Extension.Trim('.');
                        var dates = new[] {file.CreationTime, file.LastAccessTime, file.LastWriteTime};
                        string baseName = string.Format(
                            cn,
                            Path.GetFileNameWithoutExtension(file.FullName),
                            ext,
                            dates[0],
                            dates[1],
                            dates[2],
                            dates.Min());

                        string newName = Path.GetFullPath(Path.Combine((UseDestination ? dest.FullName : source.FullName), $"{baseName}.{ext}"));
                        
                        string newDir = Path.GetDirectoryName(newName);
                        baseName = Path.GetFileNameWithoutExtension(newName);

                        for (int i = 0; ;)
                            try {
                                if (!File.Exists(newName)) {
                                    Directory.CreateDirectory(newDir);
                                    file.MoveTo(newName);
                                }
                                else {
                                    newName = Path.Combine(newDir, baseName + $" ({++i})." + ext);
                                    continue;
                                }

                                break;
                            }
                            catch (Exception e) {
                                Console.WriteLine(e);
                                break;
                            }
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }


        [XmlAttribute]
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                _isMoving = value;
                if (_isMoving)
                {
                    var thread = new Thread(CheckFiles)
                    {
                        IsBackground = true,
                    };
                    thread.Start();
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace AutomatingBackgroundTasks.Interface
{
    public static class Setts
    {
    }

    [Serializable]
    [XmlInclude(typeof(MyTask))]
    [XmlInclude(typeof(string))]
    [XmlInclude(typeof(MyTask[]))]
    [XmlInclude(typeof(Preferences))]
    public class Preferences
    {
        public static Preferences Default { get; set; } = Read();

        public int Count => _settings.Count;

        public MyTask this[int i]
        {
            get => _settings[i];
            set => _settings[i] = value;
        }

        public void Save()
        {
            using (var fs = new FileStream(Filename, FileMode.Create))
            {
                var xf = new XmlSerializer(typeof(Preferences));
                xf.Serialize(fs, this);
            }
        }

        protected static string Filename => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Assembly.GetEntryAssembly().Location)), "prefs.xml");
        protected static Preferences Read()
        {
            Preferences res = new Preferences();
            try {
                using (var fs = new FileStream(Filename, FileMode.OpenOrCreate))
                {
                    var xf = new XmlSerializer(typeof(Preferences));
                    res = (Preferences) xf.Deserialize(fs);
                }
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            return res;
        }

        [XmlArray]
        public MyTask[] Settings
        {
            get => _settings.ToArray();
            set => _settings = value.ToList();
        }
        [XmlIgnore]
        private List<MyTask> _settings = new List<MyTask>();

        public bool AlwaysShowTrayIcon { get; set; } = true;
        public bool RunHidden { get; set; } = true;

        public (double, double) LastMainWindowPosition { get; set; } = (10, 10);
        public (double, double) MainWindowSize { get; set; } = (300, 500);
        public bool MainWindowTopmost { get; set; } = false;

        private Preferences() { }
    }
}

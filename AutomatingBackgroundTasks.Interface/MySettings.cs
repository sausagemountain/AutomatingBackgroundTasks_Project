using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
    [XmlInclude(typeof(MySettings))]
    public class MySettings
    {
        public static MySettings Default { get; set; } = MySettings.Read();

        public int Count => _settings.Count;

        public MyTask this[int i]
        {
            get => _settings[i];
            set => _settings[i] = value;
        }

        public void Save()
        {
            using (var fs = new FileStream(".\\settings.xml", FileMode.Create))
            {
                var xf = new XmlSerializer(typeof(MySettings));
                xf.Serialize(fs, this);
            }
        }

        [XmlIgnore]
        public ObservableCollection<string> ExtensionCollection = new ObservableCollection<string>();

        [XmlArray]
        public string[] Extensions
        {
            get => ExtensionCollection.ToArray();
            set
            {
                ExtensionCollection.Clear();
                foreach (string s in value) {
                    ExtensionCollection.Add(s);
                }
            }
        }

        public static MySettings Read()
        {
            MySettings res = new MySettings();
            try {

                using (var fs = new FileStream(".\\settings.xml", FileMode.OpenOrCreate))
                {
                    var xf = new XmlSerializer(typeof(MySettings));
                    res = (MySettings) xf.Deserialize(fs);
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
        public bool MinimizeToTray { get; set; } = true;


        private MySettings() { }
    }
}

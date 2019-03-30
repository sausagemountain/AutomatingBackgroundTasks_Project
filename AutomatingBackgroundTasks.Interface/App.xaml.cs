using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Xml.Serialization;

namespace AutomatingBackgroundTasks.Interface
{
    internal static class NativeMethods
    {
        public const int HWND_BROADCAST = 0xffff;
        public static readonly int WM_SHOWME = RegisterWindowMessage("WM_SHOWME");

        [DllImport("user32")]
        public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32")]
        public static extern int RegisterWindowMessage(string message);
    }

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {


        private static Mutex mutex;

        private void this_Startup(object sender, StartupEventArgs e)
        {
            const string AppName = "Automating Background Tasks";
            mutex = new Mutex(true, AppName, out bool createdNew);

            if (!createdNew)
            {
                NativeMethods.PostMessage(
                    (IntPtr)NativeMethods.HWND_BROADCAST,
                    NativeMethods.WM_SHOWME,
                    IntPtr.Zero,
                    IntPtr.Zero);
                Current.Shutdown();
                return;
            }

            try
            {
                using (var fs = new FileStream("settings.xml", FileMode.Open))
                {
                    var xf = new XmlSerializer(typeof(MySettings));
                    MySettings.Default = (MySettings) xf.Deserialize(fs);
                }
            }
            catch { }
        }
    }
}

using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace AutomatingBackgroundTasks.Interface
{
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
                MessageBox.Show("Already Started!");
                Current.Shutdown();
            }
        }
    }
}

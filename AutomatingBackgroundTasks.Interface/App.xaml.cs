using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace AutomatingBackgroundTasks.Interface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void Activate()
        {
            MainWindow = Windows.OfType<WindowMain>().First();

            if (MainWindow != null)
            {
                MainWindow.Activate();
            }
        }

        private void this_Activated(object sender, EventArgs e)
        {
            Activate();
        }
    }
}

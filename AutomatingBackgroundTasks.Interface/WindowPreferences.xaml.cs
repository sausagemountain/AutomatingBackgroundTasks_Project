using System;
using System.ComponentModel;
using System.Windows;

namespace AutomatingBackgroundTasks.Interface
{
    public partial class WindowPreferences : Window
    {
        public new WindowMain Owner;
        
        public WindowPreferences()
        {
            InitializeComponent();
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            Preferences.Default.Save();
            try {
                Owner.appIcon.Visible = Preferences.Default.AlwaysShowTrayIcon;
            }
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        private void this_Closing(object sender, CancelEventArgs e)
        {
        }
    }
}

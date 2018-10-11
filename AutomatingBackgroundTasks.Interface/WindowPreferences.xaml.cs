using System.ComponentModel;
using System.Windows;

namespace AutomatingBackgroundTasks.Interface
{
    public partial class WindowPreferences : Window
    {
        public WindowPreferences()
        {
            InitializeComponent();
        }

        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void this_Closing(object sender, CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AutomatingBackgroundTasks.Interface
{
    /// <summary>
    /// Interaction logic for WindowEditRule.xaml
    /// </summary>
    public partial class WindowEditRule : Window
    {
        public MyTask Task = null;

        public WindowEditRule(MyTask task)
        {
            InitializeComponent();
            Task = task;
        }

        private void Naming_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Rule = (NamingRules) (sender as ComboBox).SelectedIndex;
            if (Task.Rule == NamingRules.Custom)
                Rules.Visibility = Visibility.Visible;
            else
                Rules.Visibility = Visibility.Collapsed;
        }

        private void Move_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.MoveOnly = (MoveSettings) (sender as ComboBox).SelectedIndex;
        }
    }
}

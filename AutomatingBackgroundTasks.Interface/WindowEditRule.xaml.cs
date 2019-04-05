using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        //public static readonly DependencyProperty TaskProperty = DependencyProperty.Register(
        //    "Task", typeof(MyTask), typeof(WindowEditRule), new PropertyMetadata(default(MyTask)));

        //public MyTask Task
        //{
        //    get => (MyTask)GetValue(TaskProperty);
        //    set => SetValue(TaskProperty, value);
        //}

        public static MyTask Task = null;

        public WindowEditRule(MyTask task)
        {
            Task = task;
            InitializeComponent();
            Move.SelectedItem = Task.MoveOnly;
            Naming.SelectedItem = Task.Rule;

            AddExtItem.Command = new RelayCommand(o => {
                var addExtensionDialog = new WindowAddPattern { Owner = this };
                addExtensionDialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(addExtensionDialog.NewExtension))
                    Task.PatternCollection.Add(addExtensionDialog.NewExtension);
            });
            EditExtItem.Command = new RelayCommand(o => PatternList.SelectedIndex != -1, o => {
                var addExtensionDialog = new WindowAddPattern(Task.PatternCollection[PatternList.SelectedIndex].Clone() as string) { Owner = this };
                addExtensionDialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(addExtensionDialog.NewExtension))
                {
                    Task.PatternCollection.RemoveAt(PatternList.SelectedIndex);
                    Task.PatternCollection.Add(addExtensionDialog.NewExtension);
                }
            });
            RemoveExtItem.Command = new RelayCommand(o => PatternList.SelectedIndex != -1, o => {
                Task.PatternCollection.RemoveAt(PatternList.SelectedIndex);
            });
        }

        private void Naming_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.Rule = (NamingRules) Naming.SelectedItem;
            if (Task.Rule == NamingRules.Custom)
                Rules.Visibility = Visibility.Visible;
            else
                Rules.Visibility = Visibility.Collapsed;
        }

        private void Move_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Task.MoveOnly = (MoveSettings) Move.SelectedItem;
        }

        public static Window NameHelp = null;
        private void Help_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (NameHelp == null)
                NameHelp = new Window()
                {
                    Content = new TextBlock()
                    {
                        Text = @"extension is added to the end automatically
{0} is name
{1} is extension
{2} is creation time
{3} is last write time
{4} is last access time
{5} is earliest of the three dates"
                    },
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Title = "Naming"
                };
            NameHelp.Show();
            NameHelp.Activate();
        }

        private void this_OnClosing(object sender, CancelEventArgs e)
        {
        }
    }
}

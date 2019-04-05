using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Forms.ContextMenu;
using MenuItem = System.Windows.Forms.MenuItem;
using Size = System.Windows.Size;

namespace AutomatingBackgroundTasks.Interface
{
    public partial class WindowMain : Window
    {
        public WindowMain()
        {
            InitializeComponent();

            (Top, Left) = Preferences.Default.LastMainWindowPosition;
            (Height, Width) = Preferences.Default.MainWindowSize;

            Pref.Command = new RelayCommand(o => {
                var preferences = new WindowPreferences { Owner = this };
                preferences.ShowDialog();
                Topmost = Preferences.Default.MainWindowTopmost;
                appIcon.Visible = Preferences.Default.AlwaysShowTrayIcon;
            });

            AddRuleItem.Command = new RelayCommand(true, o => {
                var editRule = new WindowEditRule(new MyTask()) { Owner = this };
                editRule.ShowDialog();
                Tasks.Add(WindowEditRule.Task);
            });
            AddExtItem.Command = new RelayCommand(o =>  ItemsGrid.SelectedIndex != -1, o => {
                var addExtensionDialog = new WindowAddPattern { Owner = this };
                addExtensionDialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(addExtensionDialog.NewExtension))
                    PatternCollection.Add(addExtensionDialog.NewExtension);
            });
            EditRuleItem.Command = new RelayCommand(o => ItemsGrid.SelectedIndex != -1, o => {
                var editRule = new WindowEditRule(Tasks[ItemsGrid.SelectedIndex]) {Owner = this};
                editRule.ShowDialog();
                if (!Tasks.Contains(WindowEditRule.Task))
                    Tasks.Add(WindowEditRule.Task);
            });
            EditExtItem.Command = new RelayCommand(o => PatternList.SelectedIndex != -1 && ItemsGrid.SelectedIndex != -1, o => {
                var addExtensionDialog = new WindowAddPattern(PatternCollection[PatternList.SelectedIndex].Clone() as string) { Owner = this };
                addExtensionDialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(addExtensionDialog.NewExtension)) {
                    PatternCollection.RemoveAt(PatternList.SelectedIndex);
                    PatternCollection.Add(addExtensionDialog.NewExtension);
                }
            });
            RemoveRuleItem.Command = new RelayCommand(o => ItemsGrid.SelectedIndex != -1, o => {
                Tasks[ItemsGrid.SelectedIndex].IsMoving = false;
                Tasks.RemoveAt(ItemsGrid.SelectedIndex);
            });
            RemoveExtItem.Command = new RelayCommand(o => PatternList.SelectedIndex != -1 && ItemsGrid.SelectedIndex != -1, o => {
                PatternCollection.RemoveAt(PatternList.SelectedIndex);
            });
        }
        private void this_Closed(object sender, EventArgs e)
        {
            Preferences.Default.Settings = Tasks.ToArray();
            Preferences.Default.LastMainWindowPosition = (Top, Left);
            Preferences.Default.MainWindowSize = (ActualHeight, ActualWidth);
            Preferences.Default.Save();
            foreach (MyTask task in Tasks)
            {
                task.IsMoving = false;
            }

            if(appIcon != null)
                appIcon.Dispose();
            foreach (Window win in Application.Current.Windows) {
                win.Close();
            }
        }
        private void this_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Minimized:
                    if (Preferences.Default.MinimizeToTray)
                    {
                        appIcon.Visible = true;
                        Hide();
                    }

                    break;
                default:
                    if (Preferences.Default.MinimizeToTray)
                        if (!Preferences.Default.AlwaysShowTrayIcon)
                            appIcon.Visible = false;
                    break;
            }
        }
        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            var popup = new[]
            {
                new MenuItem {Text = "Open"},
                new MenuItem {Text = "Hide"},
                new MenuItem {Text = "Exit"}
            };
            popup[0].Click += delegate
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            };
            popup[1].Click += delegate { WindowState = WindowState.Minimized; };
            popup[2].Click += delegate { Close(); };

            using (var stream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Icon as BitmapSource));
                encoder.Save(stream);
                using (var bitmap = new Bitmap(stream)) {
                    appIcon = new NotifyIcon
                    {
                        ContextMenu = new ContextMenu(popup),
                        Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon()),
                        Visible = Preferences.Default.AlwaysShowTrayIcon
                    };
                }
            }

            appIcon.Click += (o, args) =>
            {
                switch (WindowState)
                {
                    default:
                        popup[1].PerformClick();
                        break;
                    case WindowState.Minimized:
                        popup[0].PerformClick();
                        break;
                }
            };
        }

        public NotifyIcon appIcon;
        public Thread FileChecker;
        public static ObservableCollection<MyTask> Tasks { get; set; } = new ObservableCollection<MyTask>(Preferences.Default.Settings);
        public static ObservableCollection<string> PatternCollection { get; set; } = new ObservableCollection<string>();
        
        private void ItemsGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsGrid.SelectedIndex != -1) {
                PatternCollection = Tasks[ItemsGrid.SelectedIndex].PatternCollection;
            }
            else {
                PatternCollection = new ObservableCollection<string>();
            }

            PatternList.ItemsSource = PatternCollection;
        }
    }
}
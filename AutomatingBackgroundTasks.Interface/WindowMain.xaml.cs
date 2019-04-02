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
            Pref.Command = new RelayCommand(o => {
                var preferences = new WindowPreferences { Owner = this };
                preferences.ShowDialog();
                Topmost = Preferences.Default.MainWindowTopmost;
                appIcon.Visible = Preferences.Default.AlwaysShowTrayIcon;
            });

            AddPathItem.Command = new RelayCommand(true, o => {
                Tasks.Add(new MyTask());
            });
            AddExtItem.Command = new RelayCommand(o =>  ItemsGrid.SelectedIndex != -1, o => {
                var addExtensionDialog = new WindowAddExtension { Owner = this };
                addExtensionDialog.ShowDialog();
                if (!string.IsNullOrWhiteSpace(addExtensionDialog.NewExtension))
                    ExtensionCollection.Add(addExtensionDialog.NewExtension);
            });
            EditExtItem.Command = new RelayCommand(o => ExtensionList.SelectedIndex != -1 && ItemsGrid.SelectedIndex != -1, o => {
                var addExtensionDialog = new WindowAddExtension(ExtensionCollection[ExtensionList.SelectedIndex].Clone() as string) { Owner = this };
                addExtensionDialog.ShowDialog();
                ExtensionCollection.RemoveAt(ExtensionList.SelectedIndex);
                if (!string.IsNullOrWhiteSpace(addExtensionDialog.NewExtension))
                    ExtensionCollection.Add(addExtensionDialog.NewExtension);
            });
            RemovePathItem.Command = new RelayCommand(o => ItemsGrid.SelectedIndex != -1, o => {
                Tasks[ItemsGrid.SelectedIndex].IsMoving = false;
                Tasks.RemoveAt(ItemsGrid.SelectedIndex);
            });
            RemoveExtItem.Command = new RelayCommand(o => ExtensionList.SelectedIndex != -1 && ItemsGrid.SelectedIndex != -1, o => {
                ExtensionCollection.RemoveAt(ExtensionList.SelectedIndex);
            });
        }
        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            //RenderSize = new Size(Preferences.Default.MainWindowSize.Item2, Preferences.Default.MainWindowSize.Item1);
            (Height, Width) = Preferences.Default.MainWindowSize;

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
        }

        public NotifyIcon appIcon;
        public Thread FileChecker;
        public static ObservableCollection<MyTask> Tasks { get; set; } = new ObservableCollection<MyTask>(Preferences.Default.Settings);
        public static ObservableCollection<string> ExtensionCollection { get; set; } = new ObservableCollection<string>();
        
        private void ItemsGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ItemsGrid.SelectedIndex != -1) {
                ExtensionCollection = Tasks[ItemsGrid.SelectedIndex].ExtensionCollection;
            }
            else {
                ExtensionCollection = new ObservableCollection<string>();
            }

            ExtensionList.ItemsSource = ExtensionCollection;
        }
    }
}
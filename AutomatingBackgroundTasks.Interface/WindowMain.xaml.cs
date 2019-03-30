using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using AutomatingBackgroundTasks.Interface.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;
using MessageBox = System.Windows.MessageBox;

namespace AutomatingBackgroundTasks.Interface
{
    public partial class WindowMain : Window
    {
        public WindowMain()
        {
            InitializeComponent();
            AddPathItem.Command = new RelayCommand(true, o => {
                Tasks.Add(new MyTask());
            });
            AddExtItem.Command = new RelayCommand(true, o => {
                var addExtensionDialog = new WindowAddExtension { Owner = this };
                addExtensionDialog.ShowDialog();
                ExtensionCollection.Add(addExtensionDialog.NewExtenison);
            });
            EditExtItem.Command = new RelayCommand(o => ExtensionList.SelectedIndex != -1, o => {
                var addExtensionDialog = new WindowAddExtension(ExtensionCollection[ExtensionList.SelectedIndex].Clone() as string) { Owner = this };
                addExtensionDialog.ShowDialog();
                ExtensionCollection.RemoveAt(ExtensionList.SelectedIndex);
                ExtensionCollection.Add(addExtensionDialog.NewExtenison);
            });
            RemovePathItem.Command = new RelayCommand(o => ItemsGrid.SelectedIndex != -1, o => {
                Tasks[ItemsGrid.SelectedIndex].IsMoving = false;
                Tasks.RemoveAt(ItemsGrid.SelectedIndex);
            });
            RemoveExtItem.Command = new RelayCommand(o => ExtensionList.SelectedIndex != -1, o => {
                ExtensionCollection.RemoveAt(ExtensionList.SelectedIndex);
            });
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
                        Visible = MySettings.Default.AlwaysShowTrayIcon
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
                    if (MySettings.Default.MinimizeToTray)
                    {
                        appIcon.Visible = true;
                        Hide();
                    }

                    break;
                default:
                    if (MySettings.Default.MinimizeToTray)
                        if (!MySettings.Default.AlwaysShowTrayIcon)
                            appIcon.Visible = false;
                    break;
            }
        }
        private void this_Closed(object sender, EventArgs e)
        {
            MySettings.Default.Settings = Tasks.ToArray();
            foreach (MyTask task in Tasks)
            {
                task.IsMoving = false;
            }
            MySettings.Default.Save();

            if(appIcon != null)
                appIcon.Dispose();
        }

        public NotifyIcon appIcon;
        public Thread FileChecker;
        public static ObservableCollection<MyTask> Tasks { get; set; } = new ObservableCollection<MyTask>(MySettings.Default.Settings);
        public static ObservableCollection<string> ExtensionCollection { get; set; } = new ObservableCollection<string>(MySettings.Default.Extensions);
        

        protected void WndProc(ref Message m)
        {
            if (m.Msg == NativeMethods.WM_SHOWME)
            {
                ShowMe();
            }
            //base.WndProc(ref m);
        }
        private void ShowMe()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }
            bool top = Topmost;
            Topmost = true;
            Topmost = top;
        }

        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            var preferences = new WindowPreferences { Owner = this };
            preferences.ShowDialog();
        }
    }
}
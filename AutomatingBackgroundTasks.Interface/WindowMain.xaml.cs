using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
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
        }
        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            var Popup = new[]
            {
                new MenuItem {Text = "Open"},
                new MenuItem {Text = "Hide"},
                new MenuItem {Text = "Exit"}
            };
            Popup[0].Click += delegate
                              {
                                  Show();
                                  WindowState = WindowState.Normal;
                                  Activate();
                              };
            Popup[1].Click += delegate { WindowState = WindowState.Minimized; };
            Popup[2].Click += delegate { Close(); };

            using (var stream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Icon as BitmapSource));
                encoder.Save(stream);
                using (var bitmap = new Bitmap(stream))
                {
                    appIcon = new NotifyIcon
                    {
                        ContextMenu = new ContextMenu(Popup),
                        Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon()),
                        Visible = Settings.Default.AlwaysShowTrayIcon
                    };
                }
            }

            appIcon.DoubleClick += delegate
                                   {
                                       switch (WindowState)
                                       {
                                           default:
                                               Popup[1].PerformClick();
                                               break;
                                           case WindowState.Minimized:
                                               Popup[0].PerformClick();
                                               break;
                                       }
                                   };

            BackgroundProcesses = new MyTasks();

            for (int i = 0; i < Settings.Default.DestinationPaths.Count; i++)
            {
                SortingSettings.Add((Settings.Default.SourcePaths[i], Settings.Default.DestinationPaths[i], Settings.Default.UseDestinations[i], Settings.Default.));
            }
            SortingSettings.CollectionChanged += delegate(object o, NotifyCollectionChangedEventArgs args)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        break;
                    case NotifyCollectionChangedAction.Move:
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        break;
                }
            };
        }
        private void this_StateChanged(object sender, EventArgs e)
        {
            switch (WindowState)
            {
                case WindowState.Minimized:
                    if (Settings.Default.MinimizeToTray)
                    {
                        appIcon.Visible = true;
                        Hide();
                    }

                    break;
                default:
                    if (Settings.Default.MinimizeToTray)
                        if (!Settings.Default.AlwaysShowTrayIcon)
                            appIcon.Visible = false;
                    break;
            }
        }
        private void this_Closed(object sender, EventArgs e)
        {
            shouldRun = false;
            appIcon.Dispose();
        }

        public NotifyIcon appIcon;
        public Thread FileChecker;
        private MyTasks BackgroundProcesses;
        public bool shouldRun;
        public static ObservableCollection<string> ExtensionCollection { get; set; } = new ObservableCollection<string>(Settings.Default.Extensions.Cast<string>());
        public static ObservableCollection<(string, string, bool, bool)> SortingSettings { get; set; } = new ObservableCollection<(string, string, bool, bool)>();

        private void FileCheck_Checked(object sender, RoutedEventArgs e)
        {
            shouldRun = FileCheckBox.IsChecked.GetValueOrDefault();
            if (shouldRun)
            {
                FileChecker = new Thread(() => BackgroundProcesses.CheckFiles(ref shouldRun, 0)) {IsBackground = true};
                FileChecker.Start();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            FileCheckBox.IsChecked = false;
            var addExtensionDialog = new WindowAddExtension {Owner = this};
            addExtensionDialog.ShowDialog();
            ExtensionCollection.Add(addExtensionDialog.NewExtenison);
            Settings.Default.Extensions.Add(addExtensionDialog.NewExtenison);
            Settings.Default.Save();
        }
        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            FileCheckBox.IsChecked = false;
            ExtensionCollection.RemoveAt(ExtensionList.SelectedIndex);
            Settings.Default.Extensions.RemoveAt(ExtensionList.SelectedIndex);
            Settings.Default.Save();
        }

        private void EditSourcePath_Click(object sender, RoutedEventArgs e)
        {
            FileCheckBox.IsChecked = false;
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = Settings.Default.SourcePaths[0],
                Title = "Select Source Folder"
            };

            var result = dialog.ShowDialog();
            switch (result)
            {
                case CommonFileDialogResult.None:
                    break;
                case CommonFileDialogResult.Ok:
                    if (Settings.Default.Recursive)
                        if (MyTasks.IsDestChildOfSource(Settings.Default.SourcePaths[0], Settings.Default.DestinationPaths[0]))
                        {
                            MessageBox.Show("Destination cannot be a child of Source while sorting recursively");
                            break;
                        }

                    Settings.Default.SourcePaths[0] = dialog.FileName;
                    Settings.Default.Save();
                    break;
                case CommonFileDialogResult.Cancel:
                    break;
            }

            dialog.Dispose();
        }
        private void EditDestinationPath_Click(object sender, RoutedEventArgs e)
        {
            FileCheckBox.IsChecked = false;
            var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                InitialDirectory = Settings.Default.DestinationPaths[0],
                Title = "Select Destination Folder"
            };

            var result = dialog.ShowDialog();
            switch (result)
            {
                case CommonFileDialogResult.None:
                    break;
                case CommonFileDialogResult.Ok:
                    if (Settings.Default.Recursive)
                        if (MyTasks.IsDestChildOfSource(Settings.Default.SourcePaths[0], Settings.Default.DestinationPaths[0]))
                        {
                            MessageBox.Show("Destination cannot be a child of Source while sorting recursively");
                            break;
                        }

                    Settings.Default.DestinationPaths[0] = dialog.FileName;
                    Settings.Default.Save();
                    break;
                case CommonFileDialogResult.Cancel:
                    break;
            }

            dialog.Dispose();
        }
        private void SaveSettings(object sender, RoutedEventArgs e)
        {
            FileCheckBox.IsChecked = false;
            if (Settings.Default.Recursive)
                if (MyTasks.IsDestChildOfSource(Settings.Default.SourcePaths[0], Settings.Default.DestinationPaths[0]))
                {
                    MessageBox.Show("Destination cannot be a child of Source while sorting recursively");
                    RecursiveCheckBox.IsChecked = false;
                }

            Settings.Default.Save();
        }

        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            var preferences = new WindowPreferences { Owner = this };
            preferences.ShowDialog();
        }
    }
}
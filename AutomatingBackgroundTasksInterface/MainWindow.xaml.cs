using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutomatingBackgroundTasksInterface.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AutomatingBackgroundTasksInterface
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            {
                
            }
        }

        public System.Windows.Forms.NotifyIcon appIcon;
        public Thread FileChecker;
        private MyTasks BackgroundProcesses;
        public bool shouldRun;

        private void FileCheck_Checked(object sender, RoutedEventArgs e)
        {
            shouldRun = FileCheckBox.IsChecked.GetValueOrDefault();
            if (shouldRun)
            {
                FileChecker = new Thread(() => BackgroundProcesses.CheckFiles(ref shouldRun)) { IsBackground = true };
                FileChecker.Start();
            }
        }

        private void EditPath_Click(object sender, RoutedEventArgs e)
        {
            FileCheckBox.IsChecked = false;
            var dialog = new CommonOpenFileDialog()
            {
                IsFolderPicker = true, 
                InitialDirectory = Settings.Default.SourcePath,
                Title = "Select Folder to Organise",
            };

            var result = dialog.ShowDialog();
            switch (result)
            {
                case CommonFileDialogResult.None:
                    break;
                case CommonFileDialogResult.Ok:
                    Settings.Default.SourcePath = dialog.FileName;
                    break;
                case CommonFileDialogResult.Cancel:
                    break;
            }
            dialog.Dispose();
            Settings.Default.Save();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            var t = new WindowAddExtension();
            t.ShowDialog();
            Settings.Default.Extensions.Add(t.NewExtenison);
            Settings.Default.Save();
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.Extensions.RemoveAt(ExtenisonList.SelectedIndex);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            shouldRun = false;
            appIcon.Dispose();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            var Popup = new[]
            {
                new System.Windows.Forms.MenuItem()
                {
                    Text = "Open",
                },
                new System.Windows.Forms.MenuItem()
                {
                    Text = "Hide",
                },
                new System.Windows.Forms.MenuItem()
                {
                    Text = "Exit",
                },
            };
            Popup[0].Click += delegate { Show(); Activate(); };
            Popup[1].Click += delegate { Hide(); };
            Popup[2].Click += delegate { Close(); };

            using (var stream = new MemoryStream())
            {
                var encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Icon as BitmapSource));
                encoder.Save(stream);
                using (var bitmap = new Bitmap(stream))
                {
                    appIcon = new System.Windows.Forms.NotifyIcon()
                    {
                        ContextMenu = new System.Windows.Forms.ContextMenu(Popup),
                        Icon = System.Drawing.Icon.FromHandle(bitmap.GetHicon()),
                        Visible = true
                    };
                }

                appIcon.DoubleClick += delegate
                                       {
                                           switch (Visibility)
                                           {
                                               case Visibility.Visible:
                                                   Hide();
                                                   break;
                                               case Visibility.Hidden:
                                                   Show();
                                                   Activate();
                                                   break;
                                               case Visibility.Collapsed:
                                                   break;
                                           }
                                       };
            }

            BackgroundProcesses = new MyTasks();


        }
    }
}

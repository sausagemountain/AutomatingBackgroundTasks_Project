using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Messaging;
using System.Runtime.InteropServices;
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
        [STAThread]
        static void Main()
        {
            if (!MessageQueue.Exists(QueueName)) {
                queue = MessageQueue.Create(QueueName);
            }
            else {
                queue = new MessageQueue(QueueName);
            }
            queue.Formatter = new XmlMessageFormatter(new[] { typeof(string) });

            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                App app = new App();
                app.MainWindow = new WindowMain();
                app.Run(app.MainWindow);
                mutex.ReleaseMutex();
            }
            else
            {
                try {
                    queue.Send(AppearMessage, AppName);
                }
                finally {
                }
            }
            queue.Close();
        }

        App() {
            InitializeComponent();
        }

        public bool isRunning = true;

        public void ReceiveQueueMessage()
        {
            queue.Refresh();
            var msg = queue.Receive();
            if (msg == null)
                return;
            msg.Formatter = queue.Formatter;

            if (msg.Label == AppName && (msg.Body as string) == AppearMessage)
            {
                Dispatcher.Invoke(() => {
                    if (MainWindow == null)
                        return;

                    MainWindow.Show();
                    MainWindow.WindowState = WindowState.Normal;
                    MainWindow.Activate();
                }, DispatcherPriority.Background);
            }
        }

        private static readonly string AppName = "Automating Background Tasks";
        public static readonly string QueueName = $@".\private$\{AppName.Replace(" ", "-")}-Queue";
        private static readonly string AppearMessage = "Appear!";
        public static MessageQueue queue = null;

        private static Mutex mutex = new Mutex(true, AppName);

        private void this_Startup(object sender, StartupEventArgs e)
        {
            Preferences.Default.ToString();
            
            messageGetter = new Thread(() => {
                while (isRunning)
                    ReceiveQueueMessage();
            });
            messageGetter.IsBackground = true;
            messageGetter.Start();
        }
        Thread messageGetter;

        private void this_Exit(object sender, ExitEventArgs e)
        {
            isRunning = false;
            queue.Send(null, string.Empty);
        }
    }
}

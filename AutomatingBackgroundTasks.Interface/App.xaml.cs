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
        public void ReceiveQueueMessage()
        {
            _queue.Refresh();
            var msg = _queue.Receive();
            if (msg == null)
                return;
            msg.Formatter = _queue.Formatter;

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

        private static MessageQueue _queue = null;
        private static Mutex _mutex = null;

        bool _isRunning = true;
        bool _createdNew;
        Thread _messageGetter;

        private void this_Startup(object sender, StartupEventArgs e)
        {
            _queue = !MessageQueue.Exists(QueueName) ? MessageQueue.Create(QueueName) : new MessageQueue(QueueName);
            _queue.Formatter = new XmlMessageFormatter(new[] { typeof(string) });

            _mutex = new Mutex(true, AppName, out _createdNew);
            if (!_createdNew) {
                _queue.Send(AppearMessage, AppName);
                Shutdown();
                return;
            }

            Preferences.Default.ToString();
            _messageGetter = new Thread(() => {
                while (_isRunning)
                    ReceiveQueueMessage();
            }) {
                IsBackground = true,
                Priority = ThreadPriority.BelowNormal,
            };
            _messageGetter.IsBackground = true;
            _messageGetter.Start();
        }
        
        private void this_Exit(object sender, ExitEventArgs e)
        {
            _isRunning = false;
            if(_createdNew)
                _queue.Send(null, string.Empty);
            _queue.Close();
        }
    }
}

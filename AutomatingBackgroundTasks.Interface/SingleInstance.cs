using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.ApplicationServices;

namespace AutomatingBackgroundTasks.Interface
{
    public class SingleInstanceManager : WindowsFormsApplicationBase
    {
        private App _application;
        private ReadOnlyCollection<string> _commandLine;

        public SingleInstanceManager()
        {
            IsSingleInstance = true;
        }

        protected override bool OnStartup(StartupEventArgs eventArgs)
        {
            _commandLine = eventArgs.CommandLine;
            _application = new App();
            _application.InitializeComponent();
            _application.Run();
            return false;
        }

        protected override void OnStartupNextInstance(StartupNextInstanceEventArgs eventArgs)
        {
            base.OnStartupNextInstance(eventArgs);
            _commandLine = eventArgs.CommandLine;
            _application.Activate();
        }

        [STAThread]
        public static void Main(string[] args)
        {
            var manager = new SingleInstanceManager();
            manager.Run(args);
        }
    }
}

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

namespace AutomatingBackgroundTasksInterface
{
    public partial class WindowAddExtension : Window
    {
        public WindowAddExtension()
        {
            InitializeComponent();
        }

        public string NewExtenison { get; protected set; } = string.Empty;
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            NewExtenison = Extension.Text;
        }

        static readonly string[] WrongSymbols =
        {
            ".",
            ",",
            "\\",
            "/",
            ":",
            "*",
            "?",
            "\"",
            "<",
            ">",
            "|",
        };

        private void Extension_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            var newText = e.Text;
            foreach (var s in WrongSymbols)
                newText = newText.Replace(s, "");

            Extension.Text = newText;
        }
    }
}

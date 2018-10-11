using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AutomatingBackgroundTasks.Interface
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
            Extension.Text = "";
            Close();
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

        private void this_Closing(object sender, CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            Extension.Focus();
        }
    }
}

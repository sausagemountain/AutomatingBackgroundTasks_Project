using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomatingBackgroundTasks.Interface
{
    public partial class WindowAddExtension : Window
    {
        public WindowAddExtension()
        {
            InitializeComponent();
        }
        public WindowAddExtension(string ext): this()
        {
            Extension.Text = ext;
        }

        public string NewExtension { get; set; } = string.Empty;
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if(!Extension.Text.StartsWith("."))
                NewExtension = ".";
            NewExtension += Extension.Text;
            //Extension.Text = "";
            Close();
        }

        static readonly string[] WrongSymbols =
        {
            " ",
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
            var newText = Extension.Text;
            foreach (var s in WrongSymbols)
                newText = newText.Replace(s, "");

            Extension.Text = newText;
        }

        private void this_Closing(object sender, CancelEventArgs e)
        {
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            Extension.Focus();
        }

        private void Extension_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                Add_Click(null, null);
            }
        }

        private void Extension_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var newText = Extension.Text;
            foreach (var s in WrongSymbols)
                newText = newText.Replace(s, "");
            //newText = newText.Replace("..", ".");

            Extension.Text = newText;
        }
    }
}

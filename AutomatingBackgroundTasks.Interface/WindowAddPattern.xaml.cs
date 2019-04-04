using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomatingBackgroundTasks.Interface
{
    public partial class WindowAddPattern : Window
    {
        public WindowAddPattern()
        {
            InitializeComponent();
        }
        public WindowAddPattern(string ext): this()
        {
            Pattern.Text = ext;
        }

        public string NewExtension { get; set; } = string.Empty;
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            NewExtension = Pattern.Text;
            //Pattern.Text = "";
            Close();
        }

        static readonly string[] WrongSymbols =
        {
            " ",
            ",",
            "\\",
            "/",
            ":",
            "\"",
            "<",
            ">",
            "|",
        };

        private void Extension_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            var newText = Pattern.Text;
            foreach (var s in WrongSymbols)
                newText = newText.Replace(s, "");

            Pattern.Text = newText;
        }

        private void this_Closing(object sender, CancelEventArgs e)
        {
        }

        private void this_Loaded(object sender, RoutedEventArgs e)
        {
            Pattern.Focus();
        }

        private void Extension_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                Add_Click(null, null);
            }
        }

        private void Extension_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var newText = Pattern.Text;
            foreach (var s in WrongSymbols)
                newText = newText.Replace(s, "");
            //newText = newText.Replace("..", ".");

            Pattern.Text = newText;
        }
    }
}

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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace AutomatingBackgroundTasks.Interface
{
    /// <summary>
    /// Interaction logic for PathSelector.xaml
    /// </summary>
    public partial class PathSelector : UserControl
    {
        public PathSelector()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DialogNameProperty = DependencyProperty.Register(
            "DialogName", typeof(string), typeof(PathSelector), new PropertyMetadata(string.Empty));
        
        public string DialogName
        {
            get => (string)GetValue(DialogNameProperty);
            set => SetValue(DialogNameProperty, value);
        }


        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path", typeof(string), typeof(PathSelector), 
            new FrameworkPropertyMetadata(string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, e) => ((PathSelector) o).PathPropertyChanged(e.NewValue as string)));

        public string Path
        {
            get => (string) GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public void PathPropertyChanged(string newValue)
        {
            Thing.Text = newValue;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var diag = new CommonOpenFileDialog(DialogName) {
                IsFolderPicker = true,
                Title = DialogName,
                DefaultDirectory = Path,
                InitialDirectory = Path,
                Multiselect = false,
                ShowHiddenItems = true,
                ShowPlacesList = true,
            };
            diag.ShowDialog();
            Path = diag.FileName;
        }

        private void Thing_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Path = Thing.Text;
        }
    }
}

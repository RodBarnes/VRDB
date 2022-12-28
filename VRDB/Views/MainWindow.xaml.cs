using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VRDB.ViewModels;

namespace VRDB
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainVM vm;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = vm = new MainVM(this);

            // Capture the highlight colors used in the DataGrid
            foreach (DataTrigger trg in ResultsDataGrid.RowStyle.Triggers)
            {
                var setter = (Setter)trg.Setters[0];
                var hex = setter.Value.ToString().Substring(1);
                switch (trg.Value)
                {
                    case Constants.LabelSame:
                        vm.HighlightSame = hex;
                        break;
                    case Constants.LabelMissing:
                        vm.HighlightMissing = hex;
                        break;
                    case Constants.LabelDifferent:
                        vm.HighlightDifferent = hex;
                        break;
                    case Constants.LabelHeader:
                        vm.HighlightHeader = hex;
                        break;
                    default:
                        break;
                }
            }
        }

        #region Event Handlers

        private void DataGrid_LostFocus(object sender, RoutedEventArgs e)
        {
            var dg = sender as DataGrid;
            dg.SelectedValue = null;
        }

        private void SelectTextAll(object sender, RoutedEventArgs e)
        {
            var tb = (sender as TextBox);
            if (tb != null)
            {
                tb.SelectAll();
            }
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {

            var tb = (sender as TextBox);
            if (tb != null)
            {
                if (!tb.IsKeyboardFocusWithin)
                {
                    e.Handled = true;
                    tb.Focus();
                }
            }
        }

        #endregion
    }
}

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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Net.ScottyDoesKnow.DsBackup.ViewModels;

namespace Net.ScottyDoesKnow.DsBackup.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();

            _viewModel = mainWindowViewModel;
            _viewModel.CloseAction = Close;
            DataContext = _viewModel;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (_viewModel.AllowExit)
                return;

            e.Cancel = true;
            _viewModel.Visibility = Visibility.Hidden;
        }
    }
}

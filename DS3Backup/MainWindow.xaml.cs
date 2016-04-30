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

namespace DS3Backup
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;

        public MainWindow(MainWindowViewModel mainWindowViewModel)
        {
            InitializeComponent();

            vm = mainWindowViewModel;
            vm.CloseAction = Close;
            DataContext = vm;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            if (!vm.AllowExit)
            {
                e.Cancel = true;
                vm.Visibility = Visibility.Hidden;
            }
        }
    }
}

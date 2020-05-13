using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DSBackup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var vm = new MainWindowViewModel(e.Args.Contains("-hide"));
                new MainWindow(vm).Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + Environment.NewLine + ex.StackTrace, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown(); // Why is this needed??
            }
        }
    }
}

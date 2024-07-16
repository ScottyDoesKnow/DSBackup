using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Net.ScottyDoesKnow.DsBackup.ViewModels;
using Net.ScottyDoesKnow.DsBackup.Views;

namespace Net.ScottyDoesKnow.DsBackup
{
    public partial class App : Application
    {
        public const string ApplicationName = "Dark Souls Save Backup Tool";

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
                MessageBox.Show($"{ex.Message}{Environment.NewLine}{Environment.NewLine}{ex.StackTrace}", ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
                Current.Shutdown();
            }
        }
    }
}

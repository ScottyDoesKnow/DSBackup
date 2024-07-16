using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Net.ScottyDoesKnow.DsBackup.ViewModels
{
    public class ViewModel : INotifyPropertyChanged
    {
        public class RelayCommand : ICommand
        {
            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            private readonly Action<object> _execute;
            private readonly Predicate<object> _canExecute;

            public RelayCommand(Action<object> execute, Predicate<object> canExecute)
            {
                _execute = execute;
                _canExecute = canExecute;
            }

            public RelayCommand(Action<object> execute)
                : this(execute, _ => true) { }

            public void Execute(object parameters) => _execute(parameters);

            [DebuggerStepThrough]
            public bool CanExecute(object parameters) => _canExecute(parameters);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => OnPropertyChanged(this, name);

        protected void OnPropertyChanged(object sender, string name) => OnPropertyChanged(PropertyChanged, sender, name);

        public static void OnPropertyChanged(PropertyChangedEventHandler eventHandler, object sender, string name)
            => eventHandler?.Invoke(sender, new PropertyChangedEventArgs(name));
    }
}

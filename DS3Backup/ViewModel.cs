using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DS3Backup
{
    public class ViewModel : INotifyPropertyChanged
    {
        public class CommandHandler : ICommand
        {
#pragma warning disable 67
            public event EventHandler CanExecuteChanged;
#pragma warning restore 67

            private Action _action;

            public CommandHandler(Action action, bool canExecute)
            {
                _action = action;
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public void Execute(object parameter)
            {
                _action();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        protected TaskFactory uiFactory;

        public ViewModel()
        {
            uiFactory = new TaskFactory(TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}

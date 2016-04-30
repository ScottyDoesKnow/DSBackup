using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS3Backup
{
    [Serializable]
    public class BackupLocation : ViewModel
    {
        public string Directory
        {
            get { return _Directory; }
            set
            {
                _Directory = value;
                OnPropertyChanged("Directory");
            }
        }
        private string _Directory = string.Empty;

        public DateTime LastBackup
        {
            get { return _LastBackup; }
            set
            {
                _LastBackup = value;
                OnPropertyChanged("LastBackup");
            }
        }
        private DateTime _LastBackup = DateTime.MinValue;

        public bool Accessible
        {
            get { return _Accessible; }
            set
            {
                _Accessible = value;
                OnPropertyChanged("Accessible");
            }
        }
        private bool _Accessible = false;

        public BackupLocation() { }

        public BackupLocation(string directory)
        {
            Directory = directory;
        }
    }
}

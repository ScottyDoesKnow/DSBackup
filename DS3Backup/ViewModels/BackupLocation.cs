using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net.ScottyDoesKnow.DsBackup.ViewModels
{
    [Serializable]
    public class BackupLocation : ViewModel
    {
        public string Directory
        {
            get => _Directory;
            set
            {
                _Directory = value;
                OnPropertyChanged();
            }
        }
        private string _Directory = string.Empty;

        public DateTime LastBackup
        {
            get => _LastBackup;
            set
            {
                _LastBackup = value;
                OnPropertyChanged();
            }
        }
        private DateTime _LastBackup = DateTime.MinValue;

        public string BackupDirectory
        {
            get => _BackupDirectory;
            set
            {
                _BackupDirectory = value;
                OnPropertyChanged();
            }
        }
        private string _BackupDirectory = string.Empty;

        public bool Accessible
        {
            get => _Accessible;
            set
            {
                _Accessible = value;
                OnPropertyChanged();
            }
        }
        private bool _Accessible;

        public BackupLocation() { }

        public BackupLocation(string directory, string backupDirectory)
        {
            Directory = directory;
            BackupDirectory = backupDirectory;
        }

        public BackupLocation(string directory)
            : this(directory, directory) { }
    }
}

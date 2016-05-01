using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace DS3Backup
{
    public class MainWindowViewModel : ViewModel
    {
        private static readonly int SAVE_DIR_LENGTH = 16; // TODO Ugly
        private static readonly string DATETIME_PATTERN = "yyyy-mm-dd-HHMM";
        private static readonly string SEPARATOR = " - ";

        private static readonly int BACKUP_RATE = 15 * 60 * 1000;
        private static readonly int NUM_BACKUPS = 10;

        private static string SETTINGS_DIR = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DS3Backup");
        private static string SETTINGS_PATH = Path.Combine(SETTINGS_DIR, "ds3backup.settings");
        private static readonly string SAVES_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarkSoulsIII");

        #region Commands

        public ICommand AddLocationCommand { get { return _AddLocationCommand ?? (_AddLocationCommand = new CommandHandler(() => AddLocation(), true)); } }
        private ICommand _AddLocationCommand;

        public ICommand RemoveLocationCommand { get { return _RemoveLocationCommand ?? (_RemoveLocationCommand = new CommandHandler(() => RemoveLocation(), true)); } }
        private ICommand _RemoveLocationCommand;

        public ICommand BackupCommand { get { return _BackupCommand ?? (_BackupCommand = new CommandHandler(() => Backup(), true)); } }
        private ICommand _BackupCommand;

        public ICommand ShowWindowCommand { get { return _ShowWindowCommand ?? (_ShowWindowCommand = new CommandHandler(() => Visibility = Visibility.Visible, true)); } }
        private ICommand _ShowWindowCommand;

        public ICommand ExitCommand { get { return _ExitCommand ?? (_ExitCommand = new CommandHandler(() => Exit(), true)); } }
        private ICommand _ExitCommand;

        #endregion

        #region Bindable Properties

        public ObservableCollection<BackupLocation> BackupLocations
        {
            get { return _BackupLocations; }
            set
            {
                _BackupLocations = value;
                OnPropertyChanged("BackupLocations");
            }
        }
        private ObservableCollection<BackupLocation> _BackupLocations = new ObservableCollection<BackupLocation>();

        public BackupLocation SelectedLocation
        {
            get { return _SelectedLocation; }
            set
            {
                _SelectedLocation = value;
                OnPropertyChanged("SelectedLocation");
            }
        }
        private BackupLocation _SelectedLocation = null;

        public Visibility Visibility
        {
            get { return _Visibility; }
            set
            {
                _Visibility = value;
                OnPropertyChanged("Visibility");
            }
        }
        private Visibility _Visibility = Visibility.Visible;

        #endregion

        public bool AllowExit { get; set; }
        public Action CloseAction { get; set; }

        private Timer backupTimer;

        public MainWindowViewModel(bool startHidden)
        {
            LoadSettings();
            backupTimer = new Timer(Backup, null, BACKUP_RATE, BACKUP_RATE);

            if (startHidden)
                Visibility = Visibility.Hidden;
        }

        #region Methods

        private void Backup(object state)
        {
            Backup();
        }

        private void Backup()
        {
            foreach (BackupLocation location in BackupLocations)
                try
                {
                    if (!Directory.Exists(location.Directory))
                    {
                        Directory.CreateDirectory(location.Directory);
                        location.Accessible = true;
                    }

                    if (Directory.Exists(location.Directory))
                        foreach (string saveDirectory in GetSaveDirectories(SAVES_PATH))
                            Backup(location, saveDirectory);

                    SaveSettings();
                }
                catch (Exception ex)
                {
                    location.Accessible = false;
                    MessageBox.Show("Error making backup. The location that caused the error will show as inaccessible. " + ex.Message, "DS3 Backup", MessageBoxButton.OK, MessageBoxImage.Error);
                }
        }

        private void Backup(BackupLocation location, string saveDirectory)
        {
            string dirName = Path.GetFileName(saveDirectory);
            List<string> existingDirs = Directory.GetDirectories(location.Directory).Where(x =>
            {
                string xName = Path.GetFileName(x);
                return xName.StartsWith(dirName) && xName.Length == SAVE_DIR_LENGTH + SEPARATOR.Length + DATETIME_PATTERN.Length;
            }).ToList(); // TODO Ugly
            existingDirs.Sort();

            DateTime? modified = GetModifiedDate(saveDirectory);
            if (!modified.HasValue)
                throw new Exception(); // TODO Ugly

            if (!modified.Value.Equals(location.LastBackup))
            {
                string newDirectory = Path.Combine(location.Directory, dirName + " - " + modified.Value.ToString(DATETIME_PATTERN));
                if (!Directory.Exists(newDirectory))
                {
                    while (existingDirs.Count >= NUM_BACKUPS)
                    {
                        Directory.Delete(existingDirs.First(), true);
                        existingDirs.RemoveAt(0);
                    }

                    DirectoryCopy(saveDirectory, newDirectory, true);
                    location.LastBackup = modified.Value;
                    location.Accessible = true;
                }
            }
        }

        private IEnumerable<string> GetSaveDirectories(string path)
        {
            foreach (string directory in Directory.GetDirectories(path))
            {
                string dirName = Path.GetFileName(directory);
                if (dirName.Length == 16)
                    yield return directory;
            }
        }

        private DateTime? GetModifiedDate(string path)
        {
            var files = Directory.GetFiles(path, "*.sl2");
            if (files.Count() == 1)
                return File.GetLastWriteTime(files.Single());
            else
                return null;
        }

        private void AddLocation()
        {
            var folderBrowser = new VistaFolderBrowserDialog();
            if (folderBrowser.ShowDialog() == true)
            {
                BackupLocations.Add(new BackupLocation(folderBrowser.SelectedPath));
                SaveSettings();
            }
        }

        private void RemoveLocation()
        {
            if (SelectedLocation != null)
            {
                BackupLocations.Remove(SelectedLocation);
                SaveSettings();
            }
        }

        private void SaveSettings()
        {
            if (!Directory.Exists(SETTINGS_DIR))
                Directory.CreateDirectory(SETTINGS_DIR);

            File.WriteAllText(SETTINGS_PATH, XmlHelper.ToXML(BackupLocations.ToList()));
        }

        private void LoadSettings()
        {
            if (!File.Exists(SETTINGS_PATH))
                BackupLocations.Add(new BackupLocation(SAVES_PATH));
            else
                BackupLocations = new ObservableCollection<BackupLocation>(XmlHelper.FromXML<List<BackupLocation>>(File.ReadAllText(SETTINGS_PATH)));
        }

        private void Exit()
        {
            AllowExit = true;
            CloseAction();
        }

        #endregion

        #region DirectoryCopy

        // https://msdn.microsoft.com/en-us/library/bb762914(v=vs.110).aspx
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Net.ScottyDoesKnow.DsBackup.Helpers;
using Ookii.Dialogs.Wpf;

namespace Net.ScottyDoesKnow.DsBackup.ViewModels
{
    public class MainWindowViewModel : ViewModel
    {
        #region Static

        private static readonly int[] SaveDirLengths = { 7, 8, 16, 17 };
        private const string DatetimePattern = "yyyy-MM-dd-HHmm";
        private const string Separator = " - ";

        private const int BackupRate = 15 * 60 * 1000; // 15 minutes
        private const int NumBackups = 10;

        private static readonly string SettingsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DS3Backup");
        private static readonly string SettingsPath = Path.Combine(SettingsDir, "ds3backup.settings");
        private static readonly string SettingsPathBak = SettingsPath + ".bak";

        private static readonly string[] DefaultPaths =
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NBGI", "DarkSouls"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarkSoulsII"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DarkSoulsIII"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NBGI", "DARK SOULS REMASTERED"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EldenRing")
        };

        #endregion

        #region Commands

        public ICommand AddLocationCommand => new RelayCommand(_ => AddLocation());

        public ICommand RemoveLocationCommand => new RelayCommand(_ => RemoveLocation());

        public ICommand BackupCommand => new RelayCommand(_ => Backup());

        public ICommand ShowWindowCommand => new RelayCommand(_ => Visibility = Visibility.Visible);

        public ICommand ExitCommand => new RelayCommand(_ => Exit());

        public ICommand SetBackupDirectoryCommand => new RelayCommand(x => SetBackupDirectory(x as BackupLocation));

        #endregion

        #region Bindable Properties

        public string ApplicationName => App.ApplicationName;

        public ObservableCollection<BackupLocation> BackupLocations { get; } = new ObservableCollection<BackupLocation>();

        public BackupLocation SelectedLocation
        {
            get => _SelectedLocation;
            set
            {
                _SelectedLocation = value;
                OnPropertyChanged();
            }
        }
        private BackupLocation _SelectedLocation;

        public Visibility Visibility
        {
            get => _Visibility;
            set
            {
                _Visibility = value;
                OnPropertyChanged();
            }
        }
        private Visibility _Visibility = Visibility.Visible;

        #endregion

        public bool AllowExit { get; set; }
        public Action CloseAction { get; set; }

        private readonly Timer _backupTimer;

        public MainWindowViewModel(bool startHidden)
        {
            LoadSettings();
            _backupTimer = new Timer(Backup, null, BackupRate, BackupRate);

            if (startHidden)
                Visibility = Visibility.Hidden;
        }

        #region Methods

        private void Backup(object state) => Backup();

        private void Backup()
        {
            foreach (var location in BackupLocations)
                try
                {
                    if (!Directory.Exists(location.BackupDirectory))
                    {
                        Directory.CreateDirectory(location.BackupDirectory);
                        location.Accessible = true;
                    }

                    foreach (var saveDirectory in GetSaveDirectories(location.Directory))
                        Backup(location, saveDirectory);
                }
                catch (Exception ex)
                {
                    location.Accessible = false;
                    MessageBox.Show("Error making backup. The location that caused the error will show as inaccessible." +
                                    $"{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                        App.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
                }

            SaveSettings();
        }

        private static void Backup(BackupLocation location, string saveDir)
        {
            var dirName = Path.GetFileName(saveDir);
            var existingDirs = Directory.GetDirectories(location.BackupDirectory).Where(x =>
            {
                var xName = Path.GetFileName(x);
                return xName.StartsWith(dirName) && SaveDirLengths.Contains(xName.Length - Separator.Length - DatetimePattern.Length);
            }).ToList();
            existingDirs.Sort();

            var modified = GetModifiedDate(saveDir);
            if (!modified.HasValue)
                throw new Exception("Encountered multiple saves in single save directory.");

            var targetDir = Path.Combine(location.BackupDirectory, dirName + " - " + modified.Value.ToString(DatetimePattern));
            if (modified.Value.Equals(location.LastBackup) && Directory.Exists(targetDir))
                return;

            if (!Directory.Exists(targetDir))
            {
                FileHelper.CopyDirectory(saveDir, targetDir, true);
                location.LastBackup = modified.Value;
                location.Accessible = true;

                while (existingDirs.Count >= NumBackups)
                {
                    Directory.Delete(existingDirs.First(), true);
                    existingDirs.RemoveAt(0);
                }
            }
            else
            {
                location.LastBackup = modified.Value;

                var testPath = Path.Combine(location.BackupDirectory, "accessibilityTest.txt");
                try
                {
                    File.WriteAllText(testPath, $"{App.ApplicationName} Accessibility Test");
                    location.Accessible = true;
                }
                catch
                {
                    location.Accessible = false;
                }
                finally
                {
                    try
                    {
                        if (File.Exists(testPath))
                            File.Delete(testPath);
                    }
                    catch { }
                }
            }
        }

        private static IEnumerable<string> GetSaveDirectories(string path)
            => Directory.GetDirectories(path).Where(x => SaveDirLengths.Contains(Path.GetFileName(x).Length));

        private static DateTime? GetModifiedDate(string path)
        {
            var files = Directory.GetFiles(path, "*.sl2");
            if (files.Length == 1)
                return File.GetLastWriteTime(files[0]);

            return null;
        }

        private void AddLocation()
        {
            var folderBrowser = new VistaFolderBrowserDialog();
            if (folderBrowser.ShowDialog() != true)
                return;

            BackupLocations.Add(new BackupLocation(folderBrowser.SelectedPath));
            SaveSettings();
        }

        private void RemoveLocation()
        {
            if (SelectedLocation == null)
                return;

            BackupLocations.Remove(SelectedLocation);
            SaveSettings();
        }

        private void SetBackupDirectory(BackupLocation backupLocation)
        {
            if (backupLocation == null)
                return;

            var folderBrowser = new VistaFolderBrowserDialog();
            if (folderBrowser.ShowDialog() != true)
                return;

            backupLocation.BackupDirectory = folderBrowser.SelectedPath;
            SaveSettings();
        }

        private void SaveSettings()
        {
            try
            {
                if (!Directory.Exists(SettingsDir))
                    Directory.CreateDirectory(SettingsDir);

                if (File.Exists(SettingsPath))
                    File.Copy(SettingsPath, SettingsPathBak, true);

                File.WriteAllText(SettingsPath, XmlHelper.ToXml(BackupLocations.ToList()));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", App.ApplicationName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadSettings()
        {
            if (File.Exists(SettingsPath))
            {
                var backupLocations = XmlHelper.FromXml<List<BackupLocation>>(File.ReadAllText(SettingsPath));
                foreach (var backupLocation in backupLocations)
                {
                    if (string.IsNullOrWhiteSpace(backupLocation.BackupDirectory))
                        backupLocation.BackupDirectory = backupLocation.Directory;

                    BackupLocations.Add(backupLocation);
                }
            }
            else
            {
                foreach (var defaultPath in DefaultPaths)
                    BackupLocations.Add(new BackupLocation(defaultPath));
            }
        }

        private void Exit()
        {
            _backupTimer.Change(Timeout.Infinite, Timeout.Infinite);

            AllowExit = true;
            CloseAction();
        }

        #endregion
    }
}

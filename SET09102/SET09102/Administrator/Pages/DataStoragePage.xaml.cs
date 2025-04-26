using Microsoft.Maui.Controls;
using SET09102.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SET09102.Administrator.Pages;

public partial class DataStoragePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private readonly string _backupDir;
    private ObservableCollection<BackupInfo> _backups = new();

    public DataStoragePage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
        _backupDir = Path.Combine(FileSystem.AppDataDirectory, "EnvironmentalMonitoring", "Backups");
        LoadDatabaseInfo();
        LoadBackups();
    }

    private void LoadDatabaseInfo()
    {
        var dbPath = _databaseService.GetDatabasePath();
        DatabaseLocationLabel.Text = dbPath;
        if (File.Exists(dbPath))
        {
            var fileInfo = new FileInfo(dbPath);
            DatabaseSizeLabel.Text = $"{fileInfo.Length / 1024.0:F2} KB";
        }
        else
        {
            DatabaseSizeLabel.Text = "-";
        }
    }

    private void LoadBackups()
    {
        if (!Directory.Exists(_backupDir))
            Directory.CreateDirectory(_backupDir);
        var files = Directory.GetFiles(_backupDir, "*.db").OrderByDescending(f => f).ToList();
        _backups = new ObservableCollection<BackupInfo>(files.Select(f => new BackupInfo
        {
            Name = Path.GetFileName(f),
            Path = f,
            Date = File.GetLastWriteTime(f)
        }));
        BackupsList.ItemsSource = _backups;
        LastBackupLabel.Text = _backups.FirstOrDefault()?.Date.ToString() ?? "-";
    }

    private async void OnCreateBackupClicked(object sender, EventArgs e)
    {
        try
        {
            var dbPath = _databaseService.GetDatabasePath();
            if (!File.Exists(dbPath))
            {
                await DisplayAlert("Error", "Database file not found.", "OK");
                return;
            }
            if (!Directory.Exists(_backupDir))
                Directory.CreateDirectory(_backupDir);
            var backupName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
            var backupPath = Path.Combine(_backupDir, backupName);
            File.Copy(dbPath, backupPath, true);
            LoadBackups();
            await DisplayAlert("Success", $"Backup created: {backupName}", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to create backup: {ex.Message}", "OK");
        }
    }

    private async void OnRestoreBackupClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string backupName)
        {
            var backupPath = Path.Combine(_backupDir, backupName);
            var dbPath = _databaseService.GetDatabasePath();
            if (!File.Exists(backupPath))
            {
                await DisplayAlert("Error", "Backup file not found.", "OK");
                return;
            }
            try
            {
                File.Copy(backupPath, dbPath, true);
                LoadDatabaseInfo();
                await DisplayAlert("Success", $"Database restored from {backupName}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to restore backup: {ex.Message}", "OK");
            }
        }
    }

    private async void OnDeleteBackupClicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.CommandParameter is string backupName)
        {
            var backupPath = Path.Combine(_backupDir, backupName);
            if (!File.Exists(backupPath))
            {
                await DisplayAlert("Error", "Backup file not found.", "OK");
                return;
            }
            try
            {
                File.Delete(backupPath);
                LoadBackups();
                await DisplayAlert("Success", $"Backup deleted: {backupName}", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete backup: {ex.Message}", "OK");
            }
        }
    }

    private class BackupInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public DateTime Date { get; set; }
    }
}
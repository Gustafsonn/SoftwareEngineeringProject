using Microsoft.Maui.Controls;
using SET09102.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace SET09102.Administrator.Pages;

/// <summary>
/// Manages database storage and backup functionality for the environmental monitoring system.
/// This page allows administrators to create, restore, and delete database backups.
/// </summary>
public partial class DataStoragePage : ContentPage
{
    // Services and fields
    private readonly DatabaseService _databaseService;
    private readonly string _backupDir;
    private ObservableCollection<BackupInfo> _backups = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DataStoragePage"/> class.
    /// Sets up the database service, backup directory, and loads initial data.
    /// </summary>
    public DataStoragePage()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
        // Define backup directory path within app data directory
        _backupDir = Path.Combine(FileSystem.AppDataDirectory, "EnvironmentalMonitoring", "Backups");
        // Load database information and available backups
        LoadDatabaseInfo();
        LoadBackups();
    }

    /// <summary>
    /// Loads and displays database metadata including file location and size.
    /// </summary>
    private void LoadDatabaseInfo()
    {
        // Get database path from service
        var dbPath = _databaseService.GetDatabasePath();
        DatabaseLocationLabel.Text = dbPath;
        
        // Calculate and display database file size if it exists
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

    /// <summary>
    /// Loads the list of available database backups from the backup directory.
    /// Creates the backup directory if it doesn't exist.
    /// </summary>
    private void LoadBackups()
    {
        // Create backup directory if it doesn't exist
        if (!Directory.Exists(_backupDir))
            Directory.CreateDirectory(_backupDir);
            
        // Get list of backup files sorted by most recent first
        var files = Directory.GetFiles(_backupDir, "*.db").OrderByDescending(f => f).ToList();
        
        // Create collection of backup info objects from file paths
        _backups = new ObservableCollection<BackupInfo>(files.Select(f => new BackupInfo
        {
            Name = Path.GetFileName(f),
            Path = f,
            Date = File.GetLastWriteTime(f)
        }));
        
        // Update UI elements
        BackupsList.ItemsSource = _backups;
        LastBackupLabel.Text = _backups.FirstOrDefault()?.Date.ToString() ?? "-";
    }

    /// <summary>
    /// Creates a new database backup with a timestamped filename.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">Event arguments.</param>
    private async void OnCreateBackupClicked(object sender, EventArgs e)
    {
        try
        {
            // Get database path and check if file exists
            var dbPath = _databaseService.GetDatabasePath();
            if (!File.Exists(dbPath))
            {
                await DisplayAlert("Error", "Database file not found.", "OK");
                return;
            }
            
            // Create backup directory if it doesn't exist
            if (!Directory.Exists(_backupDir))
                Directory.CreateDirectory(_backupDir);
                
            // Create timestamped backup filename
            var backupName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.db";
            var backupPath = Path.Combine(_backupDir, backupName);
            
            // Copy current database to backup location
            File.Copy(dbPath, backupPath, true);
            
            // Refresh backup list and notify user
            LoadBackups();
            await DisplayAlert("Success", $"Backup created: {backupName}", "OK");
        }
        catch (Exception ex)
        {
            // Display error message if backup fails
            await DisplayAlert("Error", $"Failed to create backup: {ex.Message}", "OK");
        }
    }

    /// <summary>
    /// Restores the database from a selected backup file.
    /// </summary>
    /// <param name="sender">The source of the event (Button).</param>
    /// <param name="e">Event arguments.</param>
    private async void OnRestoreBackupClicked(object sender, EventArgs e)
    {
        // Verify the sender is a button with a backup name parameter
        if (sender is Button btn && btn.CommandParameter is string backupName)
        {
            var backupPath = Path.Combine(_backupDir, backupName);
            var dbPath = _databaseService.GetDatabasePath();
            
            // Check if backup file exists
            if (!File.Exists(backupPath))
            {
                await DisplayAlert("Error", "Backup file not found.", "OK");
                return;
            }
            
            try
            {
                // Copy backup to database location, overwriting existing database
                File.Copy(backupPath, dbPath, true);
                
                // Refresh database info display
                LoadDatabaseInfo();
                
                // Notify user of successful restoration
                await DisplayAlert("Success", $"Database restored from {backupName}", "OK");
            }
            catch (Exception ex)
            {
                // Display error message if restoration fails
                await DisplayAlert("Error", $"Failed to restore backup: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Deletes a selected backup file.
    /// </summary>
    /// <param name="sender">The source of the event (Button).</param>
    /// <param name="e">Event arguments.</param>
    private async void OnDeleteBackupClicked(object sender, EventArgs e)
    {
        // Verify the sender is a button with a backup name parameter
        if (sender is Button btn && btn.CommandParameter is string backupName)
        {
            var backupPath = Path.Combine(_backupDir, backupName);
            
            // Check if backup file exists
            if (!File.Exists(backupPath))
            {
                await DisplayAlert("Error", "Backup file not found.", "OK");
                return;
            }
            
            try
            {
                // Delete the backup file
                File.Delete(backupPath);
                
                // Refresh backup list
                LoadBackups();
                
                // Notify user of successful deletion
                await DisplayAlert("Success", $"Backup deleted: {backupName}", "OK");
            }
            catch (Exception ex)
            {
                // Display error message if deletion fails
                await DisplayAlert("Error", $"Failed to delete backup: {ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Data class for storing backup file information.
    /// </summary>
    private class BackupInfo
    {
        /// <summary>
        /// Gets or sets the name of the backup file.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Gets or sets the full path to the backup file.
        /// </summary>
        public string Path { get; set; }
        
        /// <summary>
        /// Gets or sets the creation or last modification date of the backup file.
        /// </summary>
        public DateTime Date { get; set; }
    }
}
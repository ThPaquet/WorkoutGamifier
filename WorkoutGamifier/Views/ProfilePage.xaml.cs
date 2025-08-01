using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WorkoutGamifier.Services;
using WorkoutGamifier.Models;
using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Views;

public partial class ProfilePage : ContentPage, INotifyPropertyChanged
{
    private readonly IStatisticsService _statisticsService;
    private readonly IDataSeedingService _dataSeedingService;
    private readonly WorkoutGamifier.Core.Services.IBackupService _backupService;
    private bool _hasPoolStats = false;

    public ProfilePage(IStatisticsService statisticsService, IDataSeedingService dataSeedingService, WorkoutGamifier.Core.Services.IBackupService backupService)
    {
        InitializeComponent();
        _statisticsService = statisticsService;
        _dataSeedingService = dataSeedingService;
        _backupService = backupService;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadAllDataAsync();
    }

    public bool HasPoolStats
    {
        get => _hasPoolStats;
        set
        {
            _hasPoolStats = value;
            OnPropertyChanged();
        }
    }

    private async Task LoadAllDataAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        
        try
        {
            await LoadStatisticsAsync();
            await LoadPoolStatsAsync();
            await LoadRecentActivityAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load profile data: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            var stats = await _statisticsService.GetUserStatisticsAsync();
            
            // Update key statistics cards
            TotalPointsLabel.Text = stats.TotalPointsEarned.ToString();
            CurrentBalanceLabel.Text = stats.CurrentPointBalance.ToString();
            TotalSessionsLabel.Text = stats.TotalSessionsCompleted.ToString();
            TotalWorkoutsLabel.Text = stats.TotalWorkoutsReceived.ToString();
            
            // Update detailed statistics
            AverageSessionLabel.Text = $"{stats.AverageSessionDuration:F1} min";
            TotalActionsLabel.Text = stats.TotalActionsCompleted.ToString();
            TotalActiveTimeLabel.Text = $"{stats.TotalActiveSessionTime} min";
            MostUsedPoolLabel.Text = stats.MostUsedWorkoutPool ?? "None";
            PreferredDifficultyLabel.Text = stats.PreferredDifficulty?.ToString() ?? "None";
            
            // Update last session info
            if (stats.LastSessionDate.HasValue)
            {
                LastSessionLabel.Text = $"Last session: {stats.LastSessionDate.Value:MMM dd, yyyy}";
            }
            else
            {
                LastSessionLabel.Text = "Last session: Never";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load statistics: {ex.Message}", "OK");
        }
    }

    private async Task LoadPoolStatsAsync()
    {
        try
        {
            var poolStats = await _statisticsService.GetWorkoutPoolUsageStatsAsync();
            var poolStatsViewModels = poolStats.Select(ps => new PoolStatsViewModel
            {
                PoolName = ps.Key,
                UsageCount = ps.Value
            }).OrderByDescending(ps => ps.UsageCount).ToList();

            PoolStatsCollectionView.ItemsSource = poolStatsViewModels;
            HasPoolStats = poolStatsViewModels.Any();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load pool statistics: {ex.Message}", "OK");
        }
    }

    private async Task LoadRecentActivityAsync()
    {
        try
        {
            var transactions = await _statisticsService.GetPointTransactionHistoryAsync();
            var recentTransactions = transactions.Take(15).Select(t => new TransactionViewModel
            {
                Description = t.Description,
                Date = t.Date,
                Points = t.Type == TransactionType.Earned ? t.Points : -t.Points,
                PointsColor = t.Type == TransactionType.Earned ? "Green" : "Red"
            }).ToList();

            RecentActivityCollectionView.ItemsSource = recentTransactions;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load recent activity: {ex.Message}", "OK");
        }
    }

    private async void OnViewHistoryClicked(object sender, EventArgs e)
    {
        try
        {
            var transactions = await _statisticsService.GetPointTransactionHistoryAsync();
            var workoutHistory = await _statisticsService.GetWorkoutHistoryAsync();
            
            var historyText = "=== POINT TRANSACTIONS ===\n";
            foreach (var transaction in transactions.Take(50))
            {
                var sign = transaction.Type == TransactionType.Earned ? "+" : "-";
                historyText += $"{transaction.Date:MMM dd, HH:mm} - {transaction.Description} ({sign}{transaction.Points} pts)\n";
            }
            
            historyText += "\n=== WORKOUT HISTORY ===\n";
            foreach (var workout in workoutHistory.Take(20))
            {
                historyText += $"{workout.ReceivedAt:MMM dd, HH:mm} - Workout received (-{workout.PointsSpent} pts)\n";
            }
            
            await DisplayAlert("Full History", historyText, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load history: {ex.Message}", "OK");
        }
    }

    private async void OnExportDataClicked(object sender, EventArgs e)
    {
        try
        {
            var stats = await _statisticsService.GetUserStatisticsAsync();
            var transactions = await _statisticsService.GetPointTransactionHistoryAsync();
            var workoutHistory = await _statisticsService.GetWorkoutHistoryAsync();
            
            var exportData = $"WORKOUT GAMIFIER DATA EXPORT\n";
            exportData += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n";
            
            exportData += $"=== STATISTICS ===\n";
            exportData += $"Total Points Earned: {stats.TotalPointsEarned}\n";
            exportData += $"Total Points Spent: {stats.TotalPointsSpent}\n";
            exportData += $"Current Balance: {stats.CurrentPointBalance}\n";
            exportData += $"Total Sessions: {stats.TotalSessionsCompleted}\n";
            exportData += $"Total Workouts: {stats.TotalWorkoutsReceived}\n";
            exportData += $"Total Actions: {stats.TotalActionsCompleted}\n";
            exportData += $"Average Session: {stats.AverageSessionDuration:F1} min\n";
            exportData += $"Total Active Time: {stats.TotalActiveSessionTime} min\n";
            exportData += $"Most Used Pool: {stats.MostUsedWorkoutPool ?? "None"}\n";
            exportData += $"Preferred Difficulty: {stats.PreferredDifficulty?.ToString() ?? "None"}\n\n";
            
            exportData += $"=== TRANSACTIONS ({transactions.Count}) ===\n";
            foreach (var transaction in transactions)
            {
                var sign = transaction.Type == TransactionType.Earned ? "+" : "-";
                exportData += $"{transaction.Date:yyyy-MM-dd HH:mm:ss} | {transaction.Description} | {sign}{transaction.Points} pts\n";
            }
            
            exportData += $"\n=== WORKOUTS ({workoutHistory.Count}) ===\n";
            foreach (var workout in workoutHistory)
            {
                exportData += $"{workout.ReceivedAt:yyyy-MM-dd HH:mm:ss} | Workout ID: {workout.WorkoutId} | -{workout.PointsSpent} pts\n";
            }
            
            // For now, just show the data in an alert. In a real app, you'd save to file or share
            await DisplayAlert("Export Data", "Data exported successfully! (In a real app, this would be saved to a file)", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to export data: {ex.Message}", "OK");
        }
    }

    private async void OnCreateBackupClicked(object sender, EventArgs e)
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        
        try
        {
            // Export data to JSON
            var jsonData = await _backupService.ExportDataAsync();
            
            // Save to file
            var filePath = await _backupService.GetBackupFilePathAsync();
            await _backupService.SaveBackupToFileAsync(jsonData, filePath);
            
            await DisplayAlert("Backup Created", 
                $"Backup successfully created!\n\nFile saved to:\n{filePath}", 
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Backup Failed", 
                $"Failed to create backup: {ex.Message}", 
                "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnRestoreBackupClicked(object sender, EventArgs e)
    {
        try
        {
            var action = await DisplayActionSheet(
                "Restore Options", 
                "Cancel", 
                null, 
                "Merge with existing data", 
                "Replace all existing data");

            if (action == "Cancel")
                return;

            bool overwriteExisting = action == "Replace all existing data";

            if (overwriteExisting)
            {
                var confirmResult = await DisplayAlert("Confirm Replace", 
                    "This will DELETE ALL your current data and replace it with the backup data. This action cannot be undone!\n\nAre you sure?", 
                    "Yes, Replace All", "Cancel");
                
                if (!confirmResult)
                    return;
            }

            // For now, we'll simulate file picker by asking user to manually select
            // In a real implementation, you'd use a file picker
            await DisplayAlert("File Selection", 
                "Please ensure your backup file is in the Documents/WorkoutGamifier/Backups folder and try the 'Import from File' option instead.", 
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Restore failed: {ex.Message}", "OK");
        }
    }

    private async void OnImportFromFileClicked(object sender, EventArgs e)
    {
        try
        {
            // Get backup folder path
            var backupFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), 
                "WorkoutGamifier", 
                "Backups");

            if (!Directory.Exists(backupFolder))
            {
                await DisplayAlert("No Backups Found", 
                    $"No backup folder found at:\n{backupFolder}\n\nCreate a backup first.", 
                    "OK");
                return;
            }

            var backupFiles = Directory.GetFiles(backupFolder, "*.json")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Take(10)
                .Select(f => Path.GetFileName(f))
                .ToArray();

            if (backupFiles.Length == 0)
            {
                await DisplayAlert("No Backups Found", 
                    "No backup files found in the backup folder.", 
                    "OK");
                return;
            }

            var selectedFile = await DisplayActionSheet(
                "Select Backup File", 
                "Cancel", 
                null, 
                backupFiles);

            if (selectedFile == "Cancel" || string.IsNullOrEmpty(selectedFile))
                return;

            var filePath = Path.Combine(backupFolder, selectedFile);
            await ImportBackupFromFile(filePath);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Import failed: {ex.Message}", "OK");
        }
    }

    private async Task ImportBackupFromFile(string filePath)
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        
        try
        {
            // Load backup data from file
            var jsonData = await _backupService.LoadBackupFromFileAsync(filePath);
            if (string.IsNullOrEmpty(jsonData))
            {
                await DisplayAlert("Error", "Failed to load backup file.", "OK");
                return;
            }

            // Validate backup data
            var validationResult = await _backupService.ValidateBackupDataAsync(jsonData);
            
            if (!validationResult.IsValid)
            {
                var errorMessage = "Backup validation failed:\n\n";
                errorMessage += string.Join("\n", validationResult.Errors.Take(5));
                
                if (validationResult.Errors.Count > 5)
                {
                    errorMessage += $"\n... and {validationResult.Errors.Count - 5} more errors";
                }

                await DisplayAlert("Invalid Backup", errorMessage, "OK");
                return;
            }

            // Show warnings if any
            if (validationResult.Warnings.Any())
            {
                var warningMessage = "Backup has warnings:\n\n";
                warningMessage += string.Join("\n", validationResult.Warnings.Take(3));
                
                if (validationResult.Warnings.Count > 3)
                {
                    warningMessage += $"\n... and {validationResult.Warnings.Count - 3} more warnings";
                }
                
                warningMessage += "\n\nDo you want to continue?";

                var continueResult = await DisplayAlert("Backup Warnings", warningMessage, "Continue", "Cancel");
                if (!continueResult)
                    return;
            }

            // Ask for import mode
            var action = await DisplayActionSheet(
                "Import Mode", 
                "Cancel", 
                null, 
                "Merge with existing data", 
                "Replace all existing data");

            if (action == "Cancel")
                return;

            bool overwriteExisting = action == "Replace all existing data";

            if (overwriteExisting)
            {
                var confirmResult = await DisplayAlert("Confirm Replace", 
                    "This will DELETE ALL your current data and replace it with the backup data. This action cannot be undone!\n\nAre you sure?", 
                    "Yes, Replace All", "Cancel");
                
                if (!confirmResult)
                    return;
            }

            // Import the data
            var importResult = await _backupService.ImportDataAsync(jsonData, overwriteExisting);
            
            if (importResult.Success)
            {
                var successMessage = "Import completed successfully!\n\n";
                successMessage += $"Workouts imported: {importResult.WorkoutsImported}\n";
                successMessage += $"Workout pools imported: {importResult.WorkoutPoolsImported}\n";
                successMessage += $"Actions imported: {importResult.ActionsImported}\n";
                successMessage += $"Sessions imported: {importResult.SessionsImported}";

                if (importResult.Warnings.Any())
                {
                    successMessage += $"\n\nWarnings: {importResult.Warnings.Count}";
                }

                await DisplayAlert("Import Successful", successMessage, "OK");
                await LoadAllDataAsync(); // Refresh the UI
            }
            else
            {
                var errorMessage = $"Import failed: {importResult.Message}\n\n";
                if (importResult.Errors.Any())
                {
                    errorMessage += "Errors:\n";
                    errorMessage += string.Join("\n", importResult.Errors.Take(5));
                    
                    if (importResult.Errors.Count > 5)
                    {
                        errorMessage += $"\n... and {importResult.Errors.Count - 5} more errors";
                    }
                }

                await DisplayAlert("Import Failed", errorMessage, "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Import Error", $"Failed to import backup: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnResetDataClicked(object sender, EventArgs e)
    {
        try
        {
            var result = await DisplayAlert("Confirm Reset", 
                "This will delete ALL your data and reset to defaults. This action cannot be undone!\n\nAre you absolutely sure?", 
                "Yes, Reset Everything", "Cancel");
            
            if (result)
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                
                await _dataSeedingService.ResetToDefaultDataAsync();
                await DisplayAlert("Success", "All data has been reset to defaults!", "OK");
                await LoadAllDataAsync();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to reset data: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class PoolStatsViewModel
{
    public string PoolName { get; set; } = string.Empty;
    public int UsageCount { get; set; }
}

public class TransactionViewModel
{
    public string Description { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int Points { get; set; }
    public string PointsColor { get; set; } = "Black";
}
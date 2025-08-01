namespace WorkoutGamifier.Core.Services;

public interface IBackupService
{
    Task<string> ExportDataAsync();
    Task<BackupValidationResult> ValidateBackupDataAsync(string jsonData);
    Task<ImportResult> ImportDataAsync(string jsonData, bool overwriteExisting = false);
    Task<string> GetBackupFilePathAsync();
    Task SaveBackupToFileAsync(string jsonData, string? customPath = null);
    Task<string?> LoadBackupFromFileAsync(string? customPath = null);
}

public class BackupData
{
    public List<Models.Workout> Workouts { get; set; } = new();
    public List<Models.WorkoutPool> WorkoutPools { get; set; } = new();
    public List<Models.WorkoutPoolWorkout> WorkoutPoolWorkouts { get; set; } = new();
    public List<Models.Action> Actions { get; set; } = new();
    public List<Models.Session> Sessions { get; set; } = new();
    public List<Models.ActionCompletion> ActionCompletions { get; set; } = new();
    public List<Models.WorkoutReceived> WorkoutReceived { get; set; } = new();
    public DateTime ExportedAt { get; set; }
    public string AppVersion { get; set; } = string.Empty;
}

public class BackupValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public BackupData? Data { get; set; }
}

public class ImportResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public int WorkoutsImported { get; set; }
    public int WorkoutPoolsImported { get; set; }
    public int ActionsImported { get; set; }
    public int SessionsImported { get; set; }
}
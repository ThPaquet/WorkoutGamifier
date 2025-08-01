namespace WorkoutGamifier.Models;

public class AppInitializationResult
{
    public bool IsFirstRun { get; set; }
    public bool RequiresMigration { get; set; }
    public bool InitializationSuccessful { get; set; }
    public string? ErrorMessage { get; set; }
    public string CurrentVersion { get; set; } = string.Empty;
    public string? PreviousVersion { get; set; }
}
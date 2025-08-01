using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public interface IAppInitializationService
{
    Task<AppInitializationResult> InitializeAppAsync();
    Task<bool> IsFirstRunAsync();
    Task MarkFirstRunCompleteAsync();
    Task<string> GetCurrentAppVersionAsync();
    Task<string> GetLastRunVersionAsync();
    Task UpdateLastRunVersionAsync();
    Task<bool> RequiresMigrationAsync();
    Task PerformMigrationAsync();
}
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WorkoutGamifier.Data;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public class AppInitializationService : IAppInitializationService
{
    private readonly AppDbContext _context;
    private readonly IDataSeedingService _dataSeedingService;
    private readonly IErrorHandler _errorHandler;
    private const string FirstRunKey = "first_run_completed";
    private const string LastVersionKey = "last_run_version";

    public AppInitializationService(
        AppDbContext context,
        IDataSeedingService dataSeedingService,
        IErrorHandler errorHandler)
    {
        _context = context;
        _dataSeedingService = dataSeedingService;
        _errorHandler = errorHandler;
    }

    public async Task<AppInitializationResult> InitializeAppAsync()
    {
        var result = new AppInitializationResult();
        
        try
        {
            // Get version information
            result.CurrentVersion = await GetCurrentAppVersionAsync();
            result.PreviousVersion = await GetLastRunVersionAsync();
            
            // Check if this is first run
            result.IsFirstRun = await IsFirstRunAsync();
            
            // Check if migration is required
            result.RequiresMigration = await RequiresMigrationAsync();
            
            // Ensure database is created and up to date
            await _context.Database.EnsureCreatedAsync();
            
            // Apply any pending migrations
            if (result.RequiresMigration)
            {
                await PerformMigrationAsync();
            }
            
            // Seed initial data if first run or if no preloaded data exists
            if (result.IsFirstRun || !await _dataSeedingService.IsDataSeededAsync())
            {
                await _dataSeedingService.SeedInitialDataAsync();
            }
            
            // Update version tracking
            await UpdateLastRunVersionAsync();
            
            // Mark first run as complete if it was first run
            if (result.IsFirstRun)
            {
                await MarkFirstRunCompleteAsync();
            }
            
            result.InitializationSuccessful = true;
        }
        catch (Exception ex)
        {
            result.InitializationSuccessful = false;
            result.ErrorMessage = ex.Message;
            await _errorHandler.HandleErrorAsync(ex);
        }
        
        return result;
    }

    public async Task<bool> IsFirstRunAsync()
    {
        try
        {
            var firstRunCompleted = await SecureStorage.GetAsync(FirstRunKey);
            return string.IsNullOrEmpty(firstRunCompleted);
        }
        catch
        {
            // If we can't read from secure storage, assume first run
            return true;
        }
    }

    public async Task MarkFirstRunCompleteAsync()
    {
        try
        {
            await SecureStorage.SetAsync(FirstRunKey, "true");
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(ex);
        }
    }

    public async Task<string> GetCurrentAppVersionAsync()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version?.ToString() ?? "1.0.0.0";
        }
        catch
        {
            return "1.0.0.0";
        }
    }

    public async Task<string> GetLastRunVersionAsync()
    {
        try
        {
            var lastVersion = await SecureStorage.GetAsync(LastVersionKey);
            return lastVersion ?? string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    public async Task UpdateLastRunVersionAsync()
    {
        try
        {
            var currentVersion = await GetCurrentAppVersionAsync();
            await SecureStorage.SetAsync(LastVersionKey, currentVersion);
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(ex);
        }
    }

    public async Task<bool> RequiresMigrationAsync()
    {
        try
        {
            var currentVersion = await GetCurrentAppVersionAsync();
            var lastVersion = await GetLastRunVersionAsync();
            
            // If no previous version recorded, no migration needed (first run)
            if (string.IsNullOrEmpty(lastVersion))
                return false;
            
            // Compare versions - if different, migration might be needed
            if (currentVersion != lastVersion)
            {
                // Check if there are pending migrations
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                return pendingMigrations.Any();
            }
            
            return false;
        }
        catch
        {
            // If we can't determine migration status, assume no migration needed
            return false;
        }
    }

    public async Task PerformMigrationAsync()
    {
        try
        {
            await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            await _errorHandler.HandleErrorAsync(ex);
            throw;
        }
    }
}
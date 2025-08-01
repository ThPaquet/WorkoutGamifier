namespace WorkoutGamifier.Services;

public interface IDataSeedingService
{
    Task SeedInitialDataAsync();
    Task<bool> IsDataSeededAsync();
    Task ResetToDefaultDataAsync();
}
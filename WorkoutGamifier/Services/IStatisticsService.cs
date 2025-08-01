using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public interface IStatisticsService
{
    Task<UserStatistics> GetUserStatisticsAsync();
    Task<List<PointTransaction>> GetPointTransactionHistoryAsync();
    Task<List<WorkoutReceived>> GetWorkoutHistoryAsync();
    Task<List<Session>> GetRecentSessionsAsync(int count = 10);
    Task<Dictionary<string, int>> GetWorkoutPoolUsageStatsAsync();
    Task<Dictionary<DifficultyLevel, int>> GetDifficultyPreferenceStatsAsync();
}
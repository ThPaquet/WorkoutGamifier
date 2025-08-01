using WorkoutGamifier.Models;
using WorkoutGamifier.Repositories;

namespace WorkoutGamifier.Services;

public class StatisticsService : IStatisticsService
{
    private readonly IUnitOfWork _unitOfWork;

    public StatisticsService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<UserStatistics> GetUserStatisticsAsync()
    {
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        var completedSessions = allSessions.Where(s => s.Status == SessionStatus.Completed).ToList();
        
        var allWorkoutReceived = await _unitOfWork.WorkoutReceived.GetAllAsync();
        var allActionCompletions = await _unitOfWork.ActionCompletions.GetAllAsync();

        var totalPointsEarned = completedSessions.Sum(s => s.PointsEarned);
        var totalPointsSpent = completedSessions.Sum(s => s.PointsSpent);
        
        // Calculate active session time
        var totalActiveMinutes = completedSessions
            .Where(s => s.EndTime.HasValue)
            .Sum(s => (int)(s.EndTime!.Value - s.StartTime).TotalMinutes);

        var averageSessionDuration = completedSessions.Count > 0 
            ? (double)totalActiveMinutes / completedSessions.Count 
            : 0;

        // Get most used workout pool
        var poolUsage = await GetWorkoutPoolUsageStatsAsync();
        var mostUsedPool = poolUsage.OrderByDescending(p => p.Value).FirstOrDefault().Key;

        // Get preferred difficulty
        var difficultyStats = await GetDifficultyPreferenceStatsAsync();
        var preferredDifficulty = difficultyStats.OrderByDescending(d => d.Value).FirstOrDefault().Key;

        return new UserStatistics
        {
            TotalPointsEarned = totalPointsEarned,
            TotalPointsSpent = totalPointsSpent,
            CurrentPointBalance = totalPointsEarned - totalPointsSpent,
            TotalSessionsCompleted = completedSessions.Count,
            TotalActiveSessionTime = totalActiveMinutes,
            AverageSessionDuration = averageSessionDuration,
            TotalWorkoutsReceived = allWorkoutReceived.Count,
            TotalActionsCompleted = allActionCompletions.Count,
            LastSessionDate = completedSessions.OrderByDescending(s => s.StartTime).FirstOrDefault()?.StartTime,
            MostUsedWorkoutPool = mostUsedPool,
            PreferredDifficulty = preferredDifficulty
        };
    }

    public async Task<List<PointTransaction>> GetPointTransactionHistoryAsync()
    {
        var transactions = new List<PointTransaction>();

        // Get all action completions (points earned)
        var allCompletions = await _unitOfWork.ActionCompletions.GetAllAsync();
        var allActions = await _unitOfWork.Actions.GetAllAsync();
        
        foreach (var completion in allCompletions)
        {
            var action = allActions.FirstOrDefault(a => a.Id == completion.ActionId);
            transactions.Add(new PointTransaction
            {
                Date = completion.CompletedAt,
                Description = $"Completed: {action?.Description ?? "Unknown Action"}",
                Points = completion.PointsAwarded,
                Type = TransactionType.Earned
            });
        }

        // Get all workout redemptions (points spent)
        var allWorkoutReceived = await _unitOfWork.WorkoutReceived.GetAllAsync();
        var allWorkouts = await _unitOfWork.Workouts.GetAllAsync();

        foreach (var workoutReceived in allWorkoutReceived)
        {
            var workout = allWorkouts.FirstOrDefault(w => w.Id == workoutReceived.WorkoutId);
            transactions.Add(new PointTransaction
            {
                Date = workoutReceived.ReceivedAt,
                Description = $"Received workout: {workout?.Name ?? "Unknown Workout"}",
                Points = workoutReceived.PointsSpent,
                Type = TransactionType.Spent
            });
        }

        return transactions.OrderByDescending(t => t.Date).ToList();
    }

    public async Task<List<WorkoutReceived>> GetWorkoutHistoryAsync()
    {
        var allWorkoutReceived = await _unitOfWork.WorkoutReceived.GetAllAsync();
        return allWorkoutReceived.OrderByDescending(wr => wr.ReceivedAt).ToList();
    }

    public async Task<List<Session>> GetRecentSessionsAsync(int count = 10)
    {
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        return allSessions
            .OrderByDescending(s => s.StartTime)
            .Take(count)
            .ToList();
    }

    public async Task<Dictionary<string, int>> GetWorkoutPoolUsageStatsAsync()
    {
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        var allPools = await _unitOfWork.WorkoutPools.GetAllAsync();

        var poolUsage = new Dictionary<string, int>();

        foreach (var pool in allPools)
        {
            var usageCount = allSessions.Count(s => s.WorkoutPoolId == pool.Id);
            if (usageCount > 0)
            {
                poolUsage[pool.Name] = usageCount;
            }
        }

        return poolUsage;
    }

    public async Task<Dictionary<DifficultyLevel, int>> GetDifficultyPreferenceStatsAsync()
    {
        var allWorkoutReceived = await _unitOfWork.WorkoutReceived.GetAllAsync();
        var allWorkouts = await _unitOfWork.Workouts.GetAllAsync();

        var difficultyStats = new Dictionary<DifficultyLevel, int>();

        foreach (var workoutReceived in allWorkoutReceived)
        {
            var workout = allWorkouts.FirstOrDefault(w => w.Id == workoutReceived.WorkoutId);
            if (workout != null)
            {
                if (difficultyStats.ContainsKey(workout.Difficulty))
                {
                    difficultyStats[workout.Difficulty]++;
                }
                else
                {
                    difficultyStats[workout.Difficulty] = 1;
                }
            }
        }

        return difficultyStats;
    }
}
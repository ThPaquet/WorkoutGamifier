namespace WorkoutGamifier.Models;

public class UserStatistics
{
    public int TotalPointsEarned { get; set; }
    public int TotalPointsSpent { get; set; }
    public int CurrentPointBalance { get; set; }
    public int TotalSessionsCompleted { get; set; }
    public int TotalActiveSessionTime { get; set; } // in minutes
    public double AverageSessionDuration { get; set; } // in minutes
    public int TotalWorkoutsReceived { get; set; }
    public int TotalActionsCompleted { get; set; }
    public DateTime? LastSessionDate { get; set; }
    public string? MostUsedWorkoutPool { get; set; }
    public DifficultyLevel? PreferredDifficulty { get; set; }
}

public class PointTransaction
{
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Points { get; set; }
    public TransactionType Type { get; set; }
}

public enum TransactionType
{
    Earned = 1,
    Spent = 2
}
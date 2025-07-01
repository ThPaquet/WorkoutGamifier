using WorkoutGamifier.Domain.Enums;

namespace WorkoutGamifier.Application.DTOs;

public class SessionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SessionStatus Status { get; set; }
    public int CurrentPoints { get; set; }
    public int TotalPointsEarned { get; set; }
    public int TotalPointsSpent { get; set; }
    public Guid UserId { get; set; }
    public Guid WorkoutPoolId { get; set; }
    public string WorkoutPoolName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateSessionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid WorkoutPoolId { get; set; }
}

public class UpdateSessionDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SessionStatus Status { get; set; }
}

public class SessionStatsDto
{
    public Guid SessionId { get; set; }
    public string SessionName { get; set; } = string.Empty;
    public int CurrentPoints { get; set; }
    public int TotalPointsEarned { get; set; }
    public int TotalPointsSpent { get; set; }
    public int ActionsCompleted { get; set; }
    public int WorkoutsCompleted { get; set; }
    public TimeSpan Duration { get; set; }
}
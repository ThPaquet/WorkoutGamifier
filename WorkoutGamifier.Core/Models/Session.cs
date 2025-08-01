using System.ComponentModel.DataAnnotations;

namespace WorkoutGamifier.Core.Models;

public class Session
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public int WorkoutPoolId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public int PointsEarned { get; set; }

    public int PointsSpent { get; set; }

    public SessionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Computed properties
    public int CurrentPointBalance => PointsEarned - PointsSpent;
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
}

public enum SessionStatus
{
    Active = 1,
    InProgress = 1, // Alias for Active
    Completed = 2,
    Cancelled = 3
}
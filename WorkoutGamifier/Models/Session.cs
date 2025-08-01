using System.ComponentModel.DataAnnotations;

namespace WorkoutGamifier.Models;

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

    // Navigation properties
    public virtual WorkoutPool WorkoutPool { get; set; } = null!;
    public virtual ICollection<ActionCompletion> ActionCompletions { get; set; } = new List<ActionCompletion>();
    public virtual ICollection<WorkoutReceived> WorkoutReceived { get; set; } = new List<WorkoutReceived>();

    // Computed properties
    public int CurrentPointBalance => PointsEarned - PointsSpent;
    public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
}
using WorkoutGamifier.Domain.Enums;

namespace WorkoutGamifier.Domain.Entities;

public class SessionWorkout : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid WorkoutId { get; set; }
    public int PointsCost { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public WorkoutStatus Status { get; set; } = WorkoutStatus.Assigned;
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Session Session { get; set; } = null!;
    public virtual Workout Workout { get; set; } = null!;
}
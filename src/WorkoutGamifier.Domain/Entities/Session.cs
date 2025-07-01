using WorkoutGamifier.Domain.Enums;

namespace WorkoutGamifier.Domain.Entities;

public class Session : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public int CurrentPoints { get; set; } = 0;
    public int TotalPointsEarned { get; set; } = 0;
    public int TotalPointsSpent { get; set; } = 0;
    
    public Guid UserId { get; set; }
    public Guid WorkoutPoolId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual WorkoutPool WorkoutPool { get; set; } = null!;
    public virtual ICollection<SessionAction> SessionActions { get; set; } = new List<SessionAction>();
    public virtual ICollection<SessionWorkout> SessionWorkouts { get; set; } = new List<SessionWorkout>();
}
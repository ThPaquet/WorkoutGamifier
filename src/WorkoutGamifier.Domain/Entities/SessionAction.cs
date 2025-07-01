namespace WorkoutGamifier.Domain.Entities;

public class SessionAction : BaseEntity
{
    public Guid SessionId { get; set; }
    public Guid UserActionId { get; set; }
    public int PointsEarned { get; set; }
    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    
    // Navigation properties
    public virtual Session Session { get; set; } = null!;
    public virtual UserAction UserAction { get; set; } = null!;
}
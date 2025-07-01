namespace WorkoutGamifier.Domain.Entities;

public class UserAction : BaseEntity
{
    public string Description { get; set; } = string.Empty;
    public int PointReward { get; set; }
    public Guid UserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<SessionAction> SessionActions { get; set; } = new List<SessionAction>();
}
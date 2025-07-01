namespace WorkoutGamifier.Domain.Entities;

public class WorkoutPool : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    
    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; set; } = new List<WorkoutPoolWorkout>();
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
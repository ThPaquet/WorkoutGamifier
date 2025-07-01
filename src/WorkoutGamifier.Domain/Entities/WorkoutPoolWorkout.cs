namespace WorkoutGamifier.Domain.Entities;

public class WorkoutPoolWorkout : BaseEntity
{
    public Guid WorkoutPoolId { get; set; }
    public Guid WorkoutId { get; set; }
    
    // Navigation properties
    public virtual WorkoutPool WorkoutPool { get; set; } = null!;
    public virtual Workout Workout { get; set; } = null!;
}
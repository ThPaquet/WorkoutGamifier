namespace WorkoutGamifier.Models;

public class WorkoutPoolWorkout
{
    public int WorkoutPoolId { get; set; }
    public int WorkoutId { get; set; }

    // Navigation properties
    public virtual WorkoutPool WorkoutPool { get; set; } = null!;
    public virtual Workout Workout { get; set; } = null!;
}
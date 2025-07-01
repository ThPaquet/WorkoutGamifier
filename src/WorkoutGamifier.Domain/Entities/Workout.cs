namespace WorkoutGamifier.Domain.Entities;

public class Workout : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; } // 1-10 scale
    public string Instructions { get; set; } = string.Empty;
    
    // Navigation properties
    public virtual ICollection<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; set; } = new List<WorkoutPoolWorkout>();
    public virtual ICollection<SessionWorkout> SessionWorkouts { get; set; } = new List<SessionWorkout>();
}
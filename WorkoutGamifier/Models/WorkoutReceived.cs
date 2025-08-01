namespace WorkoutGamifier.Models;

public class WorkoutReceived
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int WorkoutId { get; set; }

    public DateTime ReceivedAt { get; set; }

    public int PointsSpent { get; set; }

    // Navigation properties
    public virtual Session Session { get; set; } = null!;
    public virtual Workout Workout { get; set; } = null!;
}
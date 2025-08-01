namespace WorkoutGamifier.Models;

public class ActionCompletion
{
    public int Id { get; set; }

    public int SessionId { get; set; }

    public int ActionId { get; set; }

    public DateTime CompletedAt { get; set; }

    public int PointsAwarded { get; set; }

    // Navigation properties
    public virtual Session Session { get; set; } = null!;
    public virtual Action Action { get; set; } = null!;
}
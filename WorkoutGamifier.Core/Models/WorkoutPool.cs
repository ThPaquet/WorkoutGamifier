using System.ComponentModel.DataAnnotations;

namespace WorkoutGamifier.Core.Models;

public class WorkoutPool
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; set; } = new List<WorkoutPoolWorkout>();
}

public class WorkoutPoolWorkout
{
    public int WorkoutPoolId { get; set; }
    public int WorkoutId { get; set; }

    // Navigation properties
    public virtual WorkoutPool WorkoutPool { get; set; } = null!;
    public virtual Workout Workout { get; set; } = null!;
}

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
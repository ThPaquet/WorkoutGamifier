using System.ComponentModel.DataAnnotations;

namespace WorkoutGamifier.Models;

public class Workout
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(2000)]
    public string? Instructions { get; set; }

    [Range(1, 300)]
    public int DurationMinutes { get; set; }

    public DifficultyLevel Difficulty { get; set; }

    public bool IsPreloaded { get; set; }

    public bool IsHidden { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public virtual ICollection<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; set; } = new List<WorkoutPoolWorkout>();
    public virtual ICollection<WorkoutReceived> WorkoutReceived { get; set; } = new List<WorkoutReceived>();
}
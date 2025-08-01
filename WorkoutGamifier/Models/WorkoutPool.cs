using System.ComponentModel.DataAnnotations;

namespace WorkoutGamifier.Models;

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
    public virtual ICollection<Session> Sessions { get; set; } = new List<Session>();
}
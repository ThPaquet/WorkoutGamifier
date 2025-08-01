using System.ComponentModel.DataAnnotations;

namespace WorkoutGamifier.Core.Models;

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

    [Range(1, 480)]
    public int DurationMinutes { get; set; }

    public DifficultyLevel Difficulty { get; set; }

    public bool IsPreloaded { get; set; }

    public bool IsHidden { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

public enum DifficultyLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}
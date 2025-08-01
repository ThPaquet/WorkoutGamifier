using System.ComponentModel.DataAnnotations;

namespace WorkoutGamifier.Core.Models;

public class Action
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Range(1, 1000)]
    public int PointValue { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
namespace WorkoutGamifier.Application.DTOs;

public class WorkoutDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string Instructions { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateWorkoutDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string Instructions { get; set; } = string.Empty;
}

public class UpdateWorkoutDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public string Category { get; set; } = string.Empty;
    public int Difficulty { get; set; }
    public string Instructions { get; set; } = string.Empty;
}
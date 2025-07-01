namespace WorkoutGamifier.Application.DTOs;

public class WorkoutPoolDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public List<WorkoutDto> Workouts { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateWorkoutPoolDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> WorkoutIds { get; set; } = new();
}

public class UpdateWorkoutPoolDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<Guid> WorkoutIds { get; set; } = new();
}

public class WorkoutPoolSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int WorkoutCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
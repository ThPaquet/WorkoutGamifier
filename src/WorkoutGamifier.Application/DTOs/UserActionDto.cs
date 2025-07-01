namespace WorkoutGamifier.Application.DTOs;

public class UserActionDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PointReward { get; set; }
    public Guid UserId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateUserActionDto
{
    public string Description { get; set; } = string.Empty;
    public int PointReward { get; set; }
}

public class UpdateUserActionDto
{
    public string Description { get; set; } = string.Empty;
    public int PointReward { get; set; }
}

public class CompleteActionDto
{
    public Guid UserActionId { get; set; }
    public string? Notes { get; set; }
}
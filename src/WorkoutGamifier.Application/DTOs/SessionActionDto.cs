namespace WorkoutGamifier.Application.DTOs;

public class SessionActionDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid UserActionId { get; set; }
    public string ActionDescription { get; set; } = string.Empty;
    public int PointsEarned { get; set; }
    public DateTime CompletedAt { get; set; }
    public string? Notes { get; set; }
}
using WorkoutGamifier.Domain.Enums;

namespace WorkoutGamifier.Application.DTOs;

public class SessionWorkoutDto
{
    public Guid Id { get; set; }
    public Guid SessionId { get; set; }
    public Guid WorkoutId { get; set; }
    public string WorkoutName { get; set; } = string.Empty;
    public int PointsCost { get; set; }
    public DateTime AssignedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public WorkoutStatus Status { get; set; }
    public string? Notes { get; set; }
}
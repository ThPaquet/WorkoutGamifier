using WorkoutGamifier.Application.DTOs;

namespace WorkoutGamifier.Application.Services;

public interface ISessionService
{
    Task<SessionDto> CreateSessionAsync(CreateSessionDto createSessionDto, Guid userId, CancellationToken cancellationToken = default);
    Task<SessionDto> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<IEnumerable<SessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<SessionDto> UpdateSessionAsync(Guid sessionId, UpdateSessionDto updateSessionDto, CancellationToken cancellationToken = default);
    Task<SessionStatsDto> GetSessionStatsAsync(Guid sessionId, CancellationToken cancellationToken = default);
    Task<SessionDto> CompleteActionAsync(Guid sessionId, CompleteActionDto completeActionDto, CancellationToken cancellationToken = default);
    Task<WorkoutDto> GetRandomWorkoutAsync(Guid sessionId, int pointsCost, CancellationToken cancellationToken = default);
    Task<SessionDto> CompleteWorkoutAsync(Guid sessionId, Guid workoutId, string? notes = null, CancellationToken cancellationToken = default);
    Task<SessionDto> EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default);
}
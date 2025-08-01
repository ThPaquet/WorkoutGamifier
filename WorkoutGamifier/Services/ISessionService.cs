using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public interface ISessionService
{
    Task<Session> StartSessionAsync(string name, int workoutPoolId, string? description = null);
    Task<Session> CreateSessionAsync(Session session);
    Task<Session?> GetActiveSessionAsync();
    Task<Session?> GetSessionByIdAsync(int sessionId);
    Task<Session> UpdateSessionAsync(Session session);
    Task<Session> EndSessionAsync(int sessionId);
    Task<List<Session>> GetAllSessionsAsync();
    Task<List<Session>> GetSessionHistoryAsync();
    Task<ActionCompletion> CompleteActionAsync(int sessionId, int actionId);
    Task<ActionCompletion> AddCompletionAsync(ActionCompletion completion);
    Task<List<ActionCompletion>> GetSessionCompletionsAsync(int sessionId);
    Task<WorkoutReceived> SpendPointsForWorkoutAsync(int sessionId, int pointCost);
    Task<bool> HasActiveSessionAsync();
}
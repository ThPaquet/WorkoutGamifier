using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;

namespace WorkoutGamifier.Core.Services;

public interface ISessionService
{
    Task<Session> StartSessionAsync(string name, int workoutPoolId, string? description = null);
    Task<Session?> GetActiveSessionAsync();
    Task<Session?> GetSessionByIdAsync(int sessionId);
    Task<Session> EndSessionAsync(int sessionId);
    Task<List<Session>> GetAllSessionsAsync();
    Task<ActionCompletion> CompleteActionAsync(int sessionId, int actionId);
    Task<WorkoutReceived> SpendPointsForWorkoutAsync(int sessionId, int pointCost);
    Task<bool> HasActiveSessionAsync();
}

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TestDbContext _context;
    private readonly WorkoutSelector _workoutSelector;

    public SessionService(IUnitOfWork unitOfWork, TestDbContext context, WorkoutSelector workoutSelector)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _workoutSelector = workoutSelector;
    }

    public async Task<Session> StartSessionAsync(string name, int workoutPoolId, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Session name is required.");

        // Check if workout pool exists and has workouts
        var workoutPool = await _unitOfWork.WorkoutPools.GetByIdAsync(workoutPoolId);
        if (workoutPool == null)
            throw new InvalidOperationException($"Workout pool with ID {workoutPoolId} not found.");

        var workoutsInPool = await _context.WorkoutPoolWorkouts
            .Where(wpw => wpw.WorkoutPoolId == workoutPoolId)
            .Include(wpw => wpw.Workout)
            .Select(wpw => wpw.Workout)
            .ToListAsync();

        if (!workoutsInPool.Any())
            throw new InvalidOperationException("Cannot start session with an empty workout pool.");

        // Check if there's already an active session
        var activeSession = await GetActiveSessionAsync();
        if (activeSession != null)
            throw new InvalidOperationException("Cannot start a new session while another session is active.");

        var session = new Session
        {
            Name = name,
            Description = description,
            WorkoutPoolId = workoutPoolId,
            StartTime = DateTime.UtcNow,
            Status = SessionStatus.Active,
            PointsEarned = 0,
            PointsSpent = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdSession = await _unitOfWork.Sessions.CreateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return createdSession;
    }

    public async Task<Session?> GetActiveSessionAsync()
    {
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        return allSessions.FirstOrDefault(s => s.Status == SessionStatus.Active);
    }

    public async Task<Session?> GetSessionByIdAsync(int sessionId)
    {
        return await _unitOfWork.Sessions.GetByIdAsync(sessionId);
    }

    public async Task<Session> EndSessionAsync(int sessionId)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Session with ID {sessionId} not found.");

        if (session.Status != SessionStatus.Active)
            throw new InvalidOperationException("Cannot end a session that is not active.");

        session.EndTime = DateTime.UtcNow;
        session.Status = SessionStatus.Completed;
        session.UpdatedAt = DateTime.UtcNow;

        var updatedSession = await _unitOfWork.Sessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return updatedSession;
    }

    public async Task<List<Session>> GetAllSessionsAsync()
    {
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        return allSessions.OrderByDescending(s => s.StartTime).ToList();
    }

    public async Task<ActionCompletion> CompleteActionAsync(int sessionId, int actionId)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Session with ID {sessionId} not found.");

        if (session.Status != SessionStatus.Active)
            throw new InvalidOperationException("Cannot complete actions in an inactive session.");

        var action = await _unitOfWork.Actions.GetByIdAsync(actionId);
        if (action == null)
            throw new InvalidOperationException($"Action with ID {actionId} not found.");

        var actionCompletion = new ActionCompletion
        {
            SessionId = sessionId,
            ActionId = actionId,
            CompletedAt = DateTime.UtcNow,
            PointsAwarded = action.PointValue
        };

        var createdCompletion = await _unitOfWork.ActionCompletions.CreateAsync(actionCompletion);

        // Update session points
        session.PointsEarned += action.PointValue;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Sessions.UpdateAsync(session);

        await _unitOfWork.SaveChangesAsync();

        return createdCompletion;
    }

    public async Task<WorkoutReceived> SpendPointsForWorkoutAsync(int sessionId, int pointCost)
    {
        var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
        if (session == null)
            throw new InvalidOperationException($"Session with ID {sessionId} not found.");

        if (session.Status != SessionStatus.Active)
            throw new InvalidOperationException("Cannot spend points in an inactive session.");

        var currentBalance = session.PointsEarned - session.PointsSpent;
        if (currentBalance < pointCost)
            throw new InvalidOperationException($"Insufficient points. Current balance: {currentBalance}, Required: {pointCost}");

        // Get random workout from session's pool
        var workoutsInPool = await _context.WorkoutPoolWorkouts
            .Where(wpw => wpw.WorkoutPoolId == session.WorkoutPoolId)
            .Include(wpw => wpw.Workout)
            .Select(wpw => wpw.Workout)
            .ToListAsync();

        var randomWorkout = _workoutSelector.SelectRandomWorkout(workoutsInPool);
        if (randomWorkout == null)
            throw new InvalidOperationException("No workouts available in the selected pool.");

        var workoutReceived = new WorkoutReceived
        {
            SessionId = sessionId,
            WorkoutId = randomWorkout.Id,
            ReceivedAt = DateTime.UtcNow,
            PointsSpent = pointCost
        };

        var createdWorkoutReceived = await _unitOfWork.WorkoutReceived.CreateAsync(workoutReceived);

        // Update session points
        session.PointsSpent += pointCost;
        session.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Sessions.UpdateAsync(session);

        await _unitOfWork.SaveChangesAsync();

        return createdWorkoutReceived;
    }

    public async Task<bool> HasActiveSessionAsync()
    {
        var activeSession = await GetActiveSessionAsync();
        return activeSession != null;
    }
}
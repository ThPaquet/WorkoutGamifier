using WorkoutGamifier.Models;
using WorkoutGamifier.Repositories;

namespace WorkoutGamifier.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkoutPoolService _workoutPoolService;

    public SessionService(IUnitOfWork unitOfWork, IWorkoutPoolService workoutPoolService)
    {
        _unitOfWork = unitOfWork;
        _workoutPoolService = workoutPoolService;
    }

    public async Task<Session> StartSessionAsync(string name, int workoutPoolId, string? description = null)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Session name is required.");
        }

        // Check if workout pool exists
        var workoutPool = await _unitOfWork.WorkoutPools.GetByIdAsync(workoutPoolId);
        if (workoutPool == null)
        {
            throw new InvalidOperationException($"Workout pool with ID {workoutPoolId} not found.");
        }

        // Check if there's already an active session
        var activeSession = await GetActiveSessionAsync();
        if (activeSession != null)
        {
            throw new InvalidOperationException("Cannot start a new session while another session is active. Please end the current session first.");
        }

        // Validate that the workout pool has workouts
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(workoutPoolId);
        if (!workoutsInPool.Any())
        {
            throw new InvalidOperationException("Cannot start session with an empty workout pool. Please add workouts to the pool first.");
        }

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
        {
            throw new InvalidOperationException($"Session with ID {sessionId} not found.");
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException("Cannot end a session that is not active.");
        }

        session.EndTime = DateTime.UtcNow;
        session.Status = SessionStatus.Completed;

        var updatedSession = await _unitOfWork.Sessions.UpdateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return updatedSession;
    }

    public async Task<List<Session>> GetSessionHistoryAsync()
    {
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        return allSessions
            .OrderByDescending(s => s.StartTime)
            .ToList();
    }

    public async Task<ActionCompletion> CompleteActionAsync(int sessionId, int actionId)
    {
        // Validate session exists and is active
        var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Session with ID {sessionId} not found.");
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException("Cannot complete actions in an inactive session.");
        }

        // Validate action exists
        var action = await _unitOfWork.Actions.GetByIdAsync(actionId);
        if (action == null)
        {
            throw new InvalidOperationException($"Action with ID {actionId} not found.");
        }

        // Create action completion
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
        await _unitOfWork.Sessions.UpdateAsync(session);

        await _unitOfWork.SaveChangesAsync();

        return createdCompletion;
    }

    public async Task<WorkoutReceived> SpendPointsForWorkoutAsync(int sessionId, int pointCost)
    {
        // Validate session exists and is active
        var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Session with ID {sessionId} not found.");
        }

        if (session.Status != SessionStatus.Active)
        {
            throw new InvalidOperationException("Cannot spend points in an inactive session.");
        }

        // Check if user has sufficient points
        var currentBalance = session.PointsEarned - session.PointsSpent;
        if (currentBalance < pointCost)
        {
            throw new InvalidOperationException($"Insufficient points. Current balance: {currentBalance}, Required: {pointCost}");
        }

        // Get random workout from session's pool
        var randomWorkout = await _workoutPoolService.GetRandomWorkoutFromPoolAsync(session.WorkoutPoolId);
        if (randomWorkout == null)
        {
            throw new InvalidOperationException("No workouts available in the selected pool.");
        }

        // Create workout received record
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
        await _unitOfWork.Sessions.UpdateAsync(session);

        await _unitOfWork.SaveChangesAsync();

        return createdWorkoutReceived;
    }

    public async Task<bool> HasActiveSessionAsync()
    {
        var activeSession = await GetActiveSessionAsync();
        return activeSession != null;
    }

    public async Task<Session> CreateSessionAsync(Session session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        // Check if there's already an active session
        var activeSession = await GetActiveSessionAsync();
        if (activeSession != null)
        {
            throw new InvalidOperationException("Cannot create a new session while another session is active. Please end the current session first.");
        }

        // Validate that the workout pool exists and has workouts
        var workoutPool = await _unitOfWork.WorkoutPools.GetByIdAsync(session.WorkoutPoolId);
        if (workoutPool == null)
        {
            throw new InvalidOperationException($"Workout pool with ID {session.WorkoutPoolId} not found.");
        }

        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(session.WorkoutPoolId);
        if (!workoutsInPool.Any())
        {
            throw new InvalidOperationException("Cannot create session with an empty workout pool. Please add workouts to the pool first.");
        }

        session.CreatedAt = DateTime.UtcNow;
        session.UpdatedAt = DateTime.UtcNow;
        session.Status = SessionStatus.InProgress;

        var createdSession = await _unitOfWork.Sessions.CreateAsync(session);
        await _unitOfWork.SaveChangesAsync();

        return createdSession;
    }

    public async Task<Session> UpdateSessionAsync(Session session)
    {
        if (session == null)
        {
            throw new ArgumentNullException(nameof(session));
        }

        var existingSession = await _unitOfWork.Sessions.GetByIdAsync(session.Id);
        if (existingSession == null)
        {
            throw new InvalidOperationException($"Session with ID {session.Id} not found.");
        }

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

    public async Task<ActionCompletion> AddCompletionAsync(ActionCompletion completion)
    {
        if (completion == null)
        {
            throw new ArgumentNullException(nameof(completion));
        }

        // Validate session exists and is active
        var session = await _unitOfWork.Sessions.GetByIdAsync(completion.SessionId);
        if (session == null)
        {
            throw new InvalidOperationException($"Session with ID {completion.SessionId} not found.");
        }

        if (session.Status != SessionStatus.InProgress)
        {
            throw new InvalidOperationException("Cannot add completions to an inactive session.");
        }

        var createdCompletion = await _unitOfWork.ActionCompletions.CreateAsync(completion);
        await _unitOfWork.SaveChangesAsync();

        return createdCompletion;
    }

    public async Task<List<ActionCompletion>> GetSessionCompletionsAsync(int sessionId)
    {
        var allCompletions = await _unitOfWork.ActionCompletions.GetAllAsync();
        return allCompletions
            .Where(ac => ac.SessionId == sessionId)
            .OrderByDescending(ac => ac.CompletedAt)
            .ToList();
    }
}
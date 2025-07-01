using AutoMapper;
using Microsoft.Extensions.Logging;
using WorkoutGamifier.Application.DTOs;
using WorkoutGamifier.Domain.Entities;
using WorkoutGamifier.Domain.Enums;
using WorkoutGamifier.Domain.Interfaces;

namespace WorkoutGamifier.Application.Services;

public class SessionService : ISessionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SessionService> _logger;
    private readonly Random _random;

    public SessionService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SessionService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _random = new Random();
    }

    public async Task<SessionDto> CreateSessionAsync(CreateSessionDto createSessionDto, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            if (createSessionDto == null)
                throw new ArgumentNullException(nameof(createSessionDto));

            // Validate workout pool exists and belongs to user
            var workoutPool = await _unitOfWork.WorkoutPools.GetByIdAsync(createSessionDto.WorkoutPoolId, cancellationToken);
            if (workoutPool == null)
                throw new ArgumentException("Workout pool not found");

            if (workoutPool.UserId != userId)
                throw new UnauthorizedAccessException("Workout pool does not belong to the user");

            var session = new Session
            {
                Name = createSessionDto.Name,
                Description = createSessionDto.Description,
                StartTime = DateTime.UtcNow,
                UserId = userId,
                WorkoutPoolId = createSessionDto.WorkoutPoolId,
                Status = SessionStatus.Active
            };

            var createdSession = await _unitOfWork.Sessions.AddAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Session created successfully with ID: {SessionId}", createdSession.Id);

            var sessionDto = _mapper.Map<SessionDto>(createdSession);
            sessionDto.WorkoutPoolName = workoutPool.Name;
            return sessionDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user {UserId}", userId);
            throw;
        }
    }

    public async Task<SessionDto> GetSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                throw new ArgumentException("Session not found");

            var workoutPool = await _unitOfWork.WorkoutPools.GetByIdAsync(session.WorkoutPoolId, cancellationToken);
            
            var sessionDto = _mapper.Map<SessionDto>(session);
            sessionDto.WorkoutPoolName = workoutPool?.Name ?? string.Empty;
            
            return sessionDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<IEnumerable<SessionDto>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessions = await _unitOfWork.Sessions.FindAsync(s => s.UserId == userId, cancellationToken);
            var sessionDtos = new List<SessionDto>();

            foreach (var session in sessions)
            {
                var workoutPool = await _unitOfWork.WorkoutPools.GetByIdAsync(session.WorkoutPoolId, cancellationToken);
                var sessionDto = _mapper.Map<SessionDto>(session);
                sessionDto.WorkoutPoolName = workoutPool?.Name ?? string.Empty;
                sessionDtos.Add(sessionDto);
            }

            return sessionDtos.OrderByDescending(s => s.StartTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving sessions for user {UserId}", userId);
            throw;
        }
    }

    public async Task<SessionDto> UpdateSessionAsync(Guid sessionId, UpdateSessionDto updateSessionDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                throw new ArgumentException("Session not found");

            session.Name = updateSessionDto.Name;
            session.Description = updateSessionDto.Description;
            session.Status = updateSessionDto.Status;
            session.UpdatedAt = DateTime.UtcNow;

            if (updateSessionDto.Status == SessionStatus.Completed && !session.EndTime.HasValue)
            {
                session.EndTime = DateTime.UtcNow;
            }

            await _unitOfWork.Sessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return await GetSessionAsync(sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<SessionStatsDto> GetSessionStatsAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                throw new ArgumentException("Session not found");

            var sessionActions = await _unitOfWork.SessionActions.FindAsync(sa => sa.SessionId == sessionId, cancellationToken);
            var sessionWorkouts = await _unitOfWork.SessionWorkouts.FindAsync(sw => sw.SessionId == sessionId, cancellationToken);

            var duration = session.EndTime.HasValue 
                ? session.EndTime.Value - session.StartTime 
                : DateTime.UtcNow - session.StartTime;

            return new SessionStatsDto
            {
                SessionId = session.Id,
                SessionName = session.Name,
                CurrentPoints = session.CurrentPoints,
                TotalPointsEarned = session.TotalPointsEarned,
                TotalPointsSpent = session.TotalPointsSpent,
                ActionsCompleted = sessionActions.Count(),
                WorkoutsCompleted = sessionWorkouts.Count(sw => sw.Status == WorkoutStatus.Completed),
                Duration = duration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session stats for {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<SessionDto> CompleteActionAsync(Guid sessionId, CompleteActionDto completeActionDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                throw new ArgumentException("Session not found");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException("Session is not active");

            var userAction = await _unitOfWork.UserActions.GetByIdAsync(completeActionDto.UserActionId, cancellationToken);
            if (userAction == null)
                throw new ArgumentException("User action not found");

            if (userAction.UserId != session.UserId)
                throw new UnauthorizedAccessException("User action does not belong to the session user");

            var sessionAction = new SessionAction
            {
                SessionId = sessionId,
                UserActionId = completeActionDto.UserActionId,
                PointsEarned = userAction.PointReward,
                CompletedAt = DateTime.UtcNow,
                Notes = completeActionDto.Notes
            };

            await _unitOfWork.SessionActions.AddAsync(sessionAction, cancellationToken);

            // Update session points
            session.CurrentPoints += userAction.PointReward;
            session.TotalPointsEarned += userAction.PointReward;
            session.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Sessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Action completed for session {SessionId}, earned {Points} points", sessionId, userAction.PointReward);

            return await GetSessionAsync(sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing action for session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<WorkoutDto> GetRandomWorkoutAsync(Guid sessionId, int pointsCost, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                throw new ArgumentException("Session not found");

            if (session.Status != SessionStatus.Active)
                throw new InvalidOperationException("Session is not active");

            if (session.CurrentPoints < pointsCost)
                throw new InvalidOperationException("Insufficient points");

            // Get workouts from the session's workout pool
            var workoutPoolWorkouts = await _unitOfWork.WorkoutPoolWorkouts.FindAsync(
                wpw => wpw.WorkoutPoolId == session.WorkoutPoolId, cancellationToken);

            if (!workoutPoolWorkouts.Any())
                throw new InvalidOperationException("No workouts available in the workout pool");

            // Get a random workout
            var workoutIds = workoutPoolWorkouts.Select(wpw => wpw.WorkoutId).ToList();
            var randomWorkoutId = workoutIds[_random.Next(workoutIds.Count)];
            
            var workout = await _unitOfWork.Workouts.GetByIdAsync(randomWorkoutId, cancellationToken);
            if (workout == null)
                throw new InvalidOperationException("Selected workout not found");

            // Create session workout record
            var sessionWorkout = new SessionWorkout
            {
                SessionId = sessionId,
                WorkoutId = workout.Id,
                PointsCost = pointsCost,
                AssignedAt = DateTime.UtcNow,
                Status = WorkoutStatus.Assigned
            };

            await _unitOfWork.SessionWorkouts.AddAsync(sessionWorkout, cancellationToken);

            // Deduct points from session
            session.CurrentPoints -= pointsCost;
            session.TotalPointsSpent += pointsCost;
            session.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Sessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Random workout assigned to session {SessionId}, cost {Points} points", sessionId, pointsCost);

            return _mapper.Map<WorkoutDto>(workout);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random workout for session {SessionId}", sessionId);
            throw;
        }
    }

    public async Task<SessionDto> CompleteWorkoutAsync(Guid sessionId, Guid workoutId, string? notes = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var sessionWorkout = await _unitOfWork.SessionWorkouts.FindAsync(
                sw => sw.SessionId == sessionId && sw.WorkoutId == workoutId && 
                      (sw.Status == WorkoutStatus.Assigned || sw.Status == WorkoutStatus.InProgress), 
                cancellationToken);

            var sessionWorkoutEntity = sessionWorkout.FirstOrDefault();
            if (sessionWorkoutEntity == null)
                throw new ArgumentException("Session workout not found or already completed");

            sessionWorkoutEntity.Status = WorkoutStatus.Completed;
            sessionWorkoutEntity.CompletedAt = DateTime.UtcNow;
            sessionWorkoutEntity.Notes = notes;

            await _unitOfWork.SessionWorkouts.UpdateAsync(sessionWorkoutEntity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Workout {WorkoutId} completed for session {SessionId}", workoutId, sessionId);

            return await GetSessionAsync(sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing workout {WorkoutId} for session {SessionId}", workoutId, sessionId);
            throw;
        }
    }

    public async Task<SessionDto> EndSessionAsync(Guid sessionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var session = await _unitOfWork.Sessions.GetByIdAsync(sessionId, cancellationToken);
            if (session == null)
                throw new ArgumentException("Session not found");

            session.Status = SessionStatus.Completed;
            session.EndTime = DateTime.UtcNow;
            session.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Sessions.UpdateAsync(session, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Session {SessionId} ended", sessionId);

            return await GetSessionAsync(sessionId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session {SessionId}", sessionId);
            throw;
        }
    }
}
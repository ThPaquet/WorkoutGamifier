using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutGamifier.Application.DTOs;
using WorkoutGamifier.Application.Services;

namespace WorkoutGamifier.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SessionsController : ControllerBase
{
    private readonly ISessionService _sessionService;
    private readonly ILogger<SessionsController> _logger;

    public SessionsController(ISessionService sessionService, ILogger<SessionsController> logger)
    {
        _sessionService = sessionService ?? throw new ArgumentNullException(nameof(sessionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid user ID in token");
        }
        return userId;
    }

    /// <summary>
    /// Create a new session
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SessionDto>> CreateSession([FromBody] CreateSessionDto createSessionDto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var session = await _sessionService.CreateSessionAsync(createSessionDto, userId);
            return CreatedAtAction(nameof(GetSession), new { id = session.Id }, session);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session");
            return StatusCode(500, "An error occurred while creating the session");
        }
    }

    /// <summary>
    /// Get session by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SessionDto>> GetSession(Guid id)
    {
        try
        {
            var session = await _sessionService.GetSessionAsync(id);
            
            // Ensure user can only access their own sessions
            var userId = GetCurrentUserId();
            if (session.UserId != userId)
            {
                return Forbid("You can only access your own sessions");
            }

            return Ok(session);
        }
        catch (ArgumentException)
        {
            return NotFound("Session not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session {SessionId}", id);
            return StatusCode(500, "An error occurred while retrieving the session");
        }
    }

    /// <summary>
    /// Get all sessions for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SessionDto>>> GetUserSessions()
    {
        try
        {
            var userId = GetCurrentUserId();
            var sessions = await _sessionService.GetUserSessionsAsync(userId);
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user sessions");
            return StatusCode(500, "An error occurred while retrieving sessions");
        }
    }

    /// <summary>
    /// Update a session
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SessionDto>> UpdateSession(Guid id, [FromBody] UpdateSessionDto updateSessionDto)
    {
        try
        {
            // First check if session exists and belongs to user
            var existingSession = await _sessionService.GetSessionAsync(id);
            var userId = GetCurrentUserId();
            
            if (existingSession.UserId != userId)
            {
                return Forbid("You can only update your own sessions");
            }

            var updatedSession = await _sessionService.UpdateSessionAsync(id, updateSessionDto);
            return Ok(updatedSession);
        }
        catch (ArgumentException)
        {
            return NotFound("Session not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session {SessionId}", id);
            return StatusCode(500, "An error occurred while updating the session");
        }
    }

    /// <summary>
    /// Get session statistics
    /// </summary>
    [HttpGet("{id}/stats")]
    public async Task<ActionResult<SessionStatsDto>> GetSessionStats(Guid id)
    {
        try
        {
            // First check if session belongs to user
            var session = await _sessionService.GetSessionAsync(id);
            var userId = GetCurrentUserId();
            
            if (session.UserId != userId)
            {
                return Forbid("You can only access your own session stats");
            }

            var stats = await _sessionService.GetSessionStatsAsync(id);
            return Ok(stats);
        }
        catch (ArgumentException)
        {
            return NotFound("Session not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session stats for {SessionId}", id);
            return StatusCode(500, "An error occurred while retrieving session stats");
        }
    }

    /// <summary>
    /// Complete an action during a session
    /// </summary>
    [HttpPost("{id}/actions")]
    public async Task<ActionResult<SessionDto>> CompleteAction(Guid id, [FromBody] CompleteActionDto completeActionDto)
    {
        try
        {
            // First check if session belongs to user
            var session = await _sessionService.GetSessionAsync(id);
            var userId = GetCurrentUserId();
            
            if (session.UserId != userId)
            {
                return Forbid("You can only complete actions in your own sessions");
            }

            var updatedSession = await _sessionService.CompleteActionAsync(id, completeActionDto);
            return Ok(updatedSession);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing action for session {SessionId}", id);
            return StatusCode(500, "An error occurred while completing the action");
        }
    }

    /// <summary>
    /// Get a random workout from the session's workout pool
    /// </summary>
    [HttpPost("{id}/workouts/random")]
    public async Task<ActionResult<WorkoutDto>> GetRandomWorkout(Guid id, [FromBody] int pointsCost = 1)
    {
        try
        {
            // First check if session belongs to user
            var session = await _sessionService.GetSessionAsync(id);
            var userId = GetCurrentUserId();
            
            if (session.UserId != userId)
            {
                return Forbid("You can only get workouts from your own sessions");
            }

            var workout = await _sessionService.GetRandomWorkoutAsync(id, pointsCost);
            return Ok(workout);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random workout for session {SessionId}", id);
            return StatusCode(500, "An error occurred while getting a random workout");
        }
    }

    /// <summary>
    /// Mark a workout as completed
    /// </summary>
    [HttpPost("{id}/workouts/{workoutId}/complete")]
    public async Task<ActionResult<SessionDto>> CompleteWorkout(Guid id, Guid workoutId, [FromBody] string? notes = null)
    {
        try
        {
            // First check if session belongs to user
            var session = await _sessionService.GetSessionAsync(id);
            var userId = GetCurrentUserId();
            
            if (session.UserId != userId)
            {
                return Forbid("You can only complete workouts in your own sessions");
            }

            var updatedSession = await _sessionService.CompleteWorkoutAsync(id, workoutId, notes);
            return Ok(updatedSession);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing workout {WorkoutId} for session {SessionId}", workoutId, id);
            return StatusCode(500, "An error occurred while completing the workout");
        }
    }

    /// <summary>
    /// End a session
    /// </summary>
    [HttpPost("{id}/end")]
    public async Task<ActionResult<SessionDto>> EndSession(Guid id)
    {
        try
        {
            // First check if session belongs to user
            var session = await _sessionService.GetSessionAsync(id);
            var userId = GetCurrentUserId();
            
            if (session.UserId != userId)
            {
                return Forbid("You can only end your own sessions");
            }

            var endedSession = await _sessionService.EndSessionAsync(id);
            return Ok(endedSession);
        }
        catch (ArgumentException)
        {
            return NotFound("Session not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session {SessionId}", id);
            return StatusCode(500, "An error occurred while ending the session");
        }
    }
}
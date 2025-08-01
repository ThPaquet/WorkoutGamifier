using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Services;
using WorkoutGamifier.Tests.TestData;

namespace WorkoutGamifier.Tests.Services;

/// <summary>
/// Comprehensive tests for SessionService covering all business logic scenarios
/// </summary>
public class SessionServiceTests : IDisposable
{
    private readonly DatabaseTestFixture _fixture;
    private readonly ISessionService _sessionService;

    public SessionServiceTests()
    {
        _fixture = new DatabaseTestFixture();
        _sessionService = _fixture.GetService<ISessionService>();
    }

    [Fact]
    public async Task StartSessionAsync_WithValidData_CreatesActiveSession()
    {
        // Arrange
        await _fixture.SeedMinimalData();
        var unitOfWork = _fixture.GetUnitOfWork();
        var pools = await unitOfWork.WorkoutPools.GetAllAsync();
        var pool = pools.First();

        // Act
        var session = await _sessionService.StartSessionAsync("Test Session", pool.Id, "Test description");

        // Assert
        Assert.NotNull(session);
        Assert.Equal("Test Session", session.Name);
        Assert.Equal("Test description", session.Description);
        Assert.Equal(pool.Id, session.WorkoutPoolId);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.Equal(0, session.PointsEarned);
        Assert.Equal(0, session.PointsSpent);
        Assert.Equal(0, session.CurrentPointBalance);
        Assert.True(session.Id > 0);
        Assert.True(session.StartTime > DateTime.MinValue);
        Assert.Null(session.EndTime);
    }

    [Fact]
    public async Task StartSessionAsync_WithEmptyName_ThrowsArgumentException()
    {
        // Arrange
        await _fixture.SeedMinimalData();
        var unitOfWork = _fixture.GetUnitOfWork();
        var pools = await unitOfWork.WorkoutPools.GetAllAsync();
        var pool = pools.First();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sessionService.StartSessionAsync("", pool.Id));
    }

    [Fact]
    public async Task GetActiveSessionAsync_WithActiveSession_ReturnsSession()
    {
        // Arrange
        await _fixture.SeedMinimalData();
        var unitOfWork = _fixture.GetUnitOfWork();
        var pools = await unitOfWork.WorkoutPools.GetAllAsync();
        var pool = pools.First();

        var createdSession = await _sessionService.StartSessionAsync("Active Session", pool.Id);

        // Act
        var activeSession = await _sessionService.GetActiveSessionAsync();

        // Assert
        Assert.NotNull(activeSession);
        Assert.Equal(createdSession.Id, activeSession.Id);
        Assert.Equal("Active Session", activeSession.Name);
        Assert.Equal(SessionStatus.Active, activeSession.Status);
    }

    [Fact]
    public async Task GetActiveSessionAsync_WithNoActiveSession_ReturnsNull()
    {
        // Act
        var activeSession = await _sessionService.GetActiveSessionAsync();

        // Assert
        Assert.Null(activeSession);
    }

    [Fact]
    public async Task EndSessionAsync_WithActiveSession_EndsSuccessfully()
    {
        // Arrange
        await _fixture.SeedMinimalData();
        var unitOfWork = _fixture.GetUnitOfWork();
        var pools = await unitOfWork.WorkoutPools.GetAllAsync();
        var pool = pools.First();

        var session = await _sessionService.StartSessionAsync("Test Session", pool.Id);

        // Act
        var endedSession = await _sessionService.EndSessionAsync(session.Id);

        // Assert
        Assert.NotNull(endedSession);
        Assert.Equal(SessionStatus.Completed, endedSession.Status);
        Assert.NotNull(endedSession.EndTime);
        Assert.True(endedSession.EndTime > endedSession.StartTime);
        Assert.True(endedSession.Duration > TimeSpan.Zero);
    }

    [Fact]
    public async Task CompleteActionAsync_WithValidData_CreatesCompletionAndUpdatesPoints()
    {
        // Arrange
        var scenario = await _fixture.CreateSessionScenario();
        var session = await _sessionService.StartSessionAsync("Test Session", scenario.WorkoutPool.Id);
        var action = scenario.Actions.First();

        // Act
        var completion = await _sessionService.CompleteActionAsync(session.Id, action.Id);

        // Assert
        Assert.NotNull(completion);
        Assert.Equal(session.Id, completion.SessionId);
        Assert.Equal(action.Id, completion.ActionId);
        Assert.Equal(action.PointValue, completion.PointsAwarded);
        Assert.True(completion.CompletedAt > DateTime.MinValue);

        // Verify session points were updated
        var updatedSession = await _sessionService.GetSessionByIdAsync(session.Id);
        Assert.NotNull(updatedSession);
        Assert.Equal(action.PointValue, updatedSession.PointsEarned);
        Assert.Equal(action.PointValue, updatedSession.CurrentPointBalance);
    }

    [Fact]
    public async Task SpendPointsForWorkoutAsync_WithSufficientPoints_CreatesWorkoutReceivedAndUpdatesPoints()
    {
        // Arrange
        var scenario = await _fixture.CreateSessionScenario();
        var session = await _sessionService.StartSessionAsync("Test Session", scenario.WorkoutPool.Id);
        var action = scenario.Actions.First();
        
        // Earn some points first
        await _sessionService.CompleteActionAsync(session.Id, action.Id);
        var pointCost = 5;

        // Act
        var workoutReceived = await _sessionService.SpendPointsForWorkoutAsync(session.Id, pointCost);

        // Assert
        Assert.NotNull(workoutReceived);
        Assert.Equal(session.Id, workoutReceived.SessionId);
        Assert.Equal(pointCost, workoutReceived.PointsSpent);
        Assert.True(workoutReceived.WorkoutId > 0);
        Assert.True(workoutReceived.ReceivedAt > DateTime.MinValue);

        // Verify session points were updated
        var updatedSession = await _sessionService.GetSessionByIdAsync(session.Id);
        Assert.NotNull(updatedSession);
        Assert.Equal(pointCost, updatedSession.PointsSpent);
        Assert.Equal(action.PointValue - pointCost, updatedSession.CurrentPointBalance);
    }

    [Fact]
    public async Task HasActiveSessionAsync_WithActiveSession_ReturnsTrue()
    {
        // Arrange
        await _fixture.SeedMinimalData();
        var unitOfWork = _fixture.GetUnitOfWork();
        var pools = await unitOfWork.WorkoutPools.GetAllAsync();
        var pool = pools.First();

        await _sessionService.StartSessionAsync("Active Session", pool.Id);

        // Act
        var hasActiveSession = await _sessionService.HasActiveSessionAsync();

        // Assert
        Assert.True(hasActiveSession);
    }

    [Fact]
    public async Task HasActiveSessionAsync_WithNoActiveSession_ReturnsFalse()
    {
        // Act
        var hasActiveSession = await _sessionService.HasActiveSessionAsync();

        // Assert
        Assert.False(hasActiveSession);
    }

    public void Dispose()
    {
        _fixture?.Dispose();
    }
}
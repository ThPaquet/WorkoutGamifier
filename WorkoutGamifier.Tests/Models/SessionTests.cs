using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Tests.Models;

public class SessionTests
{
    [Fact]
    public void CurrentPointBalance_CalculatesCorrectly()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 100,
            PointsSpent = 30
        };

        // Act
        var balance = session.CurrentPointBalance;

        // Assert
        Assert.Equal(70, balance);
    }

    [Fact]
    public void CurrentPointBalance_WithZeroPoints_ReturnsZero()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 0,
            PointsSpent = 0
        };

        // Act
        var balance = session.CurrentPointBalance;

        // Assert
        Assert.Equal(0, balance);
    }

    [Fact]
    public void CurrentPointBalance_WithNegativeBalance_ReturnsNegative()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 10,
            PointsSpent = 20
        };

        // Act
        var balance = session.CurrentPointBalance;

        // Assert
        Assert.Equal(-10, balance);
    }

    [Fact]
    public void Duration_CompletedSession_ReturnsCorrectDuration()
    {
        // Arrange
        var startTime = new DateTime(2024, 1, 1, 10, 0, 0);
        var endTime = new DateTime(2024, 1, 1, 11, 30, 0);
        var session = new Session
        {
            StartTime = startTime,
            EndTime = endTime
        };

        // Act
        var duration = session.Duration;

        // Assert
        Assert.NotNull(duration);
        Assert.Equal(TimeSpan.FromMinutes(90), duration.Value);
    }

    [Fact]
    public void Duration_ActiveSession_ReturnsNull()
    {
        // Arrange
        var session = new Session
        {
            StartTime = DateTime.UtcNow,
            EndTime = null
        };

        // Act
        var duration = session.Duration;

        // Assert
        Assert.Null(duration);
    }

    [Theory]
    [InlineData(SessionStatus.Active)]
    [InlineData(SessionStatus.Completed)]
    [InlineData(SessionStatus.Cancelled)]
    public void Status_CanBeSetToValidValues(SessionStatus status)
    {
        // Arrange
        var session = new Session();

        // Act
        session.Status = status;

        // Assert
        Assert.Equal(status, session.Status);
    }

    [Fact]
    public void SessionStatus_ActiveAndInProgress_AreEqual()
    {
        // Assert
        Assert.Equal((int)SessionStatus.Active, (int)SessionStatus.InProgress);
        Assert.True(SessionStatus.Active == SessionStatus.InProgress);
    }

    [Fact]
    public void Session_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var session = new Session();

        // Assert
        Assert.Equal(0, session.Id);
        Assert.Equal(string.Empty, session.Name);
        Assert.Null(session.Description);
        Assert.Equal(0, session.WorkoutPoolId);
        Assert.Equal(default(DateTime), session.StartTime);
        Assert.Null(session.EndTime);
        Assert.Equal(0, session.PointsEarned);
        Assert.Equal(0, session.PointsSpent);
        Assert.Equal(default(SessionStatus), session.Status);
        Assert.Equal(default(DateTime), session.CreatedAt);
        Assert.Equal(default(DateTime), session.UpdatedAt);
    }

    [Fact]
    public void Session_WithValidData_PropertiesSetCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var session = new Session
        {
            Id = 1,
            Name = "Test Session",
            Description = "Test Description",
            WorkoutPoolId = 5,
            StartTime = now,
            EndTime = now.AddHours(1),
            PointsEarned = 50,
            PointsSpent = 20,
            Status = SessionStatus.Completed,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Act & Assert
        Assert.Equal(1, session.Id);
        Assert.Equal("Test Session", session.Name);
        Assert.Equal("Test Description", session.Description);
        Assert.Equal(5, session.WorkoutPoolId);
        Assert.Equal(now, session.StartTime);
        Assert.Equal(now.AddHours(1), session.EndTime);
        Assert.Equal(50, session.PointsEarned);
        Assert.Equal(20, session.PointsSpent);
        Assert.Equal(SessionStatus.Completed, session.Status);
        Assert.Equal(now, session.CreatedAt);
        Assert.Equal(now, session.UpdatedAt);
        Assert.Equal(30, session.CurrentPointBalance);
        Assert.Equal(TimeSpan.FromHours(1), session.Duration);
    }
}
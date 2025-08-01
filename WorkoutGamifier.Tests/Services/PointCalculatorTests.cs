using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Tests.Services;

public class PointCalculatorTests
{
    private readonly PointCalculator _pointCalculator;

    public PointCalculatorTests()
    {
        _pointCalculator = new PointCalculator();
    }

    [Fact]
    public void CalculateSessionBalance_ValidSession_ReturnsCorrectBalance()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 50,
            PointsSpent = 20
        };

        // Act
        var balance = _pointCalculator.CalculateSessionBalance(session);

        // Assert
        Assert.Equal(30, balance);
    }

    [Fact]
    public void CalculateSessionBalance_NoPointsEarned_ReturnsNegativeBalance()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 0,
            PointsSpent = 10
        };

        // Act
        var balance = _pointCalculator.CalculateSessionBalance(session);

        // Assert
        Assert.Equal(-10, balance);
    }

    [Fact]
    public void CanAffordWorkout_SufficientPoints_ReturnsTrue()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 50,
            PointsSpent = 20
        };
        var workoutCost = 25;

        // Act
        var canAfford = _pointCalculator.CanAffordWorkout(session, workoutCost);

        // Assert
        Assert.True(canAfford);
    }

    [Fact]
    public void CanAffordWorkout_InsufficientPoints_ReturnsFalse()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 50,
            PointsSpent = 20
        };
        var workoutCost = 35;

        // Act
        var canAfford = _pointCalculator.CanAffordWorkout(session, workoutCost);

        // Assert
        Assert.False(canAfford);
    }

    [Fact]
    public void AddPointsToSession_ValidPoints_UpdatesSession()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 10,
            PointsSpent = 5,
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        var pointsToAdd = 15;
        var originalUpdatedAt = session.UpdatedAt;

        // Act
        var updatedSession = _pointCalculator.AddPointsToSession(session, pointsToAdd);

        // Assert
        Assert.Equal(25, updatedSession.PointsEarned);
        Assert.Equal(5, updatedSession.PointsSpent);
        Assert.True(updatedSession.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void AddPointsToSession_ZeroPoints_ThrowsException()
    {
        // Arrange
        var session = new Session();
        var pointsToAdd = 0;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _pointCalculator.AddPointsToSession(session, pointsToAdd));
    }

    [Fact]
    public void AddPointsToSession_NegativePoints_ThrowsException()
    {
        // Arrange
        var session = new Session();
        var pointsToAdd = -5;

        // Act & Assert
        Assert.Throws<ArgumentException>(() => _pointCalculator.AddPointsToSession(session, pointsToAdd));
    }

    [Fact]
    public void SpendPointsFromSession_SufficientPoints_UpdatesSession()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 50,
            PointsSpent = 20,
            UpdatedAt = DateTime.UtcNow.AddHours(-1)
        };
        var pointsToSpend = 15;
        var originalUpdatedAt = session.UpdatedAt;

        // Act
        var updatedSession = _pointCalculator.SpendPointsFromSession(session, pointsToSpend);

        // Assert
        Assert.Equal(50, updatedSession.PointsEarned);
        Assert.Equal(35, updatedSession.PointsSpent);
        Assert.True(updatedSession.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public void SpendPointsFromSession_InsufficientPoints_ThrowsException()
    {
        // Arrange
        var session = new Session
        {
            PointsEarned = 50,
            PointsSpent = 20
        };
        var pointsToSpend = 35; // More than available balance (30)

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => 
            _pointCalculator.SpendPointsFromSession(session, pointsToSpend));
        
        Assert.Contains("Insufficient points", exception.Message);
        Assert.Contains("Current balance: 30", exception.Message);
        Assert.Contains("Required: 35", exception.Message);
    }

    [Fact]
    public void CalculateSessionDuration_CompletedSession_ReturnsCorrectDuration()
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
        var duration = _pointCalculator.CalculateSessionDuration(session);

        // Assert
        Assert.Equal(TimeSpan.FromMinutes(90), duration);
    }

    [Fact]
    public void CalculateSessionDuration_ActiveSession_ReturnsCurrentDuration()
    {
        // Arrange
        var startTime = DateTime.UtcNow.AddMinutes(-30);
        var session = new Session
        {
            StartTime = startTime,
            EndTime = null
        };

        // Act
        var duration = _pointCalculator.CalculateSessionDuration(session);

        // Assert
        Assert.True(duration.TotalMinutes >= 29); // Allow for small timing differences
        Assert.True(duration.TotalMinutes <= 31);
    }

    [Fact]
    public void CalculateAverageSessionDuration_MultipleSessions_ReturnsCorrectAverage()
    {
        // Arrange
        var sessions = new List<Session>
        {
            new Session
            {
                StartTime = new DateTime(2024, 1, 1, 10, 0, 0),
                EndTime = new DateTime(2024, 1, 1, 11, 0, 0), // 60 minutes
                Status = SessionStatus.Completed
            },
            new Session
            {
                StartTime = new DateTime(2024, 1, 2, 10, 0, 0),
                EndTime = new DateTime(2024, 1, 2, 10, 30, 0), // 30 minutes
                Status = SessionStatus.Completed
            },
            new Session
            {
                StartTime = new DateTime(2024, 1, 3, 10, 0, 0),
                EndTime = new DateTime(2024, 1, 3, 11, 30, 0), // 90 minutes
                Status = SessionStatus.Completed
            }
        };

        // Act
        var average = _pointCalculator.CalculateAverageSessionDuration(sessions);

        // Assert
        Assert.Equal(60.0, average); // (60 + 30 + 90) / 3 = 60
    }

    [Fact]
    public void CalculateAverageSessionDuration_NoCompletedSessions_ReturnsZero()
    {
        // Arrange
        var sessions = new List<Session>
        {
            new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = null,
                Status = SessionStatus.Active
            }
        };

        // Act
        var average = _pointCalculator.CalculateAverageSessionDuration(sessions);

        // Assert
        Assert.Equal(0.0, average);
    }

    [Fact]
    public void CalculateAverageSessionDuration_EmptyList_ReturnsZero()
    {
        // Arrange
        var sessions = new List<Session>();

        // Act
        var average = _pointCalculator.CalculateAverageSessionDuration(sessions);

        // Assert
        Assert.Equal(0.0, average);
    }
}
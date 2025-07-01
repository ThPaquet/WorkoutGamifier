using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkoutGamifier.Domain.Entities;
using WorkoutGamifier.Domain.Enums;

namespace WorkoutGamifier.Domain.Tests.Entities;

[TestClass]
public class SessionTests
{
    [TestMethod]
    public void Session_Creation_ShouldSetDefaultValues()
    {
        // Arrange & Act
        var session = new Session
        {
            Name = "Test Session",
            Description = "Test Description",
            UserId = Guid.NewGuid(),
            WorkoutPoolId = Guid.NewGuid()
        };

        // Assert
        Assert.IsNotNull(session);
        Assert.AreEqual("Test Session", session.Name);
        Assert.AreEqual("Test Description", session.Description);
        Assert.AreEqual(SessionStatus.Active, session.Status);
        Assert.AreEqual(0, session.CurrentPoints);
        Assert.AreEqual(0, session.TotalPointsEarned);
        Assert.AreEqual(0, session.TotalPointsSpent);
        Assert.IsNull(session.EndTime);
        Assert.IsNotNull(session.Id);
        Assert.IsTrue(session.CreatedAt <= DateTime.UtcNow);
        Assert.IsTrue(session.UpdatedAt <= DateTime.UtcNow);
    }

    [TestMethod]
    public void Session_AddPoints_ShouldUpdateCurrentPointsAndTotalEarned()
    {
        // Arrange
        var session = new Session
        {
            Name = "Test Session",
            UserId = Guid.NewGuid(),
            WorkoutPoolId = Guid.NewGuid()
        };

        // Act
        session.CurrentPoints += 5;
        session.TotalPointsEarned += 5;

        // Assert
        Assert.AreEqual(5, session.CurrentPoints);
        Assert.AreEqual(5, session.TotalPointsEarned);
        Assert.AreEqual(0, session.TotalPointsSpent);
    }

    [TestMethod]
    public void Session_SpendPoints_ShouldUpdateCurrentPointsAndTotalSpent()
    {
        // Arrange
        var session = new Session
        {
            Name = "Test Session",
            UserId = Guid.NewGuid(),
            WorkoutPoolId = Guid.NewGuid(),
            CurrentPoints = 10,
            TotalPointsEarned = 10
        };

        // Act
        session.CurrentPoints -= 3;
        session.TotalPointsSpent += 3;

        // Assert
        Assert.AreEqual(7, session.CurrentPoints);
        Assert.AreEqual(10, session.TotalPointsEarned);
        Assert.AreEqual(3, session.TotalPointsSpent);
    }

    [TestMethod]
    public void Session_EndSession_ShouldSetEndTimeAndStatus()
    {
        // Arrange
        var session = new Session
        {
            Name = "Test Session",
            UserId = Guid.NewGuid(),
            WorkoutPoolId = Guid.NewGuid(),
            Status = SessionStatus.Active
        };

        // Act
        session.Status = SessionStatus.Completed;
        session.EndTime = DateTime.UtcNow;

        // Assert
        Assert.AreEqual(SessionStatus.Completed, session.Status);
        Assert.IsNotNull(session.EndTime);
        Assert.IsTrue(session.EndTime <= DateTime.UtcNow);
    }
}
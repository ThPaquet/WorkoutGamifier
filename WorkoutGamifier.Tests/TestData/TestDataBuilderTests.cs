using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Tests.TestData;

namespace WorkoutGamifier.Tests.TestData;

/// <summary>
/// Tests to verify the test data builders work correctly and produce valid test data
/// </summary>
public class TestDataBuilderTests
{
    [Fact]
    public void WorkoutBuilder_ShouldCreateValidWorkout()
    {
        // Act
        var workout = TestDataBuilder.Workout()
            .WithName("Test Workout")
            .WithDescription("Test Description")
            .WithDuration(30)
            .WithDifficulty(DifficultyLevel.Intermediate)
            .Build();

        // Assert
        Assert.Equal("Test Workout", workout.Name);
        Assert.Equal("Test Description", workout.Description);
        Assert.Equal(30, workout.DurationMinutes);
        Assert.Equal(DifficultyLevel.Intermediate, workout.Difficulty);
        Assert.False(workout.IsHidden);
        Assert.True(workout.CreatedAt > DateTime.MinValue);
        Assert.True(workout.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public void WorkoutBuilder_StaticFactories_ShouldCreateCorrectWorkouts()
    {
        // Act
        var beginnerWorkout = WorkoutBuilder.Beginner().Build();
        var intermediateWorkout = WorkoutBuilder.Intermediate().Build();
        var advancedWorkout = WorkoutBuilder.Advanced().Build();
        var hiddenWorkout = WorkoutBuilder.Hidden().Build();

        // Assert
        Assert.Equal(DifficultyLevel.Beginner, beginnerWorkout.Difficulty);
        Assert.Equal(15, beginnerWorkout.DurationMinutes);

        Assert.Equal(DifficultyLevel.Intermediate, intermediateWorkout.Difficulty);
        Assert.Equal(30, intermediateWorkout.DurationMinutes);

        Assert.Equal(DifficultyLevel.Advanced, advancedWorkout.Difficulty);
        Assert.Equal(45, advancedWorkout.DurationMinutes);

        Assert.True(hiddenWorkout.IsHidden);
    }

    [Fact]
    public void SessionBuilder_ShouldCreateValidSession()
    {
        // Arrange
        var pool = TestDataBuilder.Pool().WithId(1).Build();

        // Act
        var session = TestDataBuilder.Session()
            .WithName("Test Session")
            .WithWorkoutPool(pool)
            .WithPointBalance(50)
            .AsActive()
            .Build();

        // Assert
        Assert.Equal("Test Session", session.Name);
        Assert.Equal(1, session.WorkoutPoolId);
        Assert.Equal(50, session.CurrentPointBalance);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.Null(session.EndTime);
    }

    [Fact]
    public void SessionBuilder_StaticFactories_ShouldCreateCorrectSessions()
    {
        // Act
        var activeSession = SessionBuilder.Active().Build();
        var completedSession = SessionBuilder.Completed().Build();
        var sessionWithPoints = SessionBuilder.WithPoints(100).Build();

        // Assert
        Assert.Equal(SessionStatus.Active, activeSession.Status);
        Assert.Null(activeSession.EndTime);

        Assert.Equal(SessionStatus.Completed, completedSession.Status);
        Assert.NotNull(completedSession.EndTime);

        Assert.Equal(100, sessionWithPoints.CurrentPointBalance);
    }

    [Fact]
    public void PoolBuilder_ShouldCreateValidPool()
    {
        // Act
        var pool = TestDataBuilder.Pool()
            .WithName("Test Pool")
            .WithDescription("Test Description")
            .Build();

        // Assert
        Assert.Equal("Test Pool", pool.Name);
        Assert.Equal("Test Description", pool.Description);
        Assert.True(pool.CreatedAt > DateTime.MinValue);
        Assert.True(pool.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public void PoolBuilder_StaticFactories_ShouldCreateCorrectPools()
    {
        // Act
        var beginnerPool = WorkoutPoolBuilder.Beginner().Build();
        var cardioPool = WorkoutPoolBuilder.Cardio().Build();
        var strengthPool = WorkoutPoolBuilder.Strength().Build();

        // Assert
        Assert.Contains("Beginner", beginnerPool.Name);
        Assert.Contains("Cardio", cardioPool.Name);
        Assert.Contains("Strength", strengthPool.Name);
    }

    [Fact]
    public void ActionBuilder_ShouldCreateValidAction()
    {
        // Act
        var action = TestDataBuilder.Action()
            .WithDescription("Test Action")
            .WithPointValue(15)
            .Build();

        // Assert
        Assert.Equal("Test Action", action.Description);
        Assert.Equal(15, action.PointValue);
        Assert.True(action.CreatedAt > DateTime.MinValue);
        Assert.True(action.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public void ActionBuilder_StaticFactories_ShouldCreateCorrectActions()
    {
        // Act
        var lowValueAction = ActionBuilder.LowValue().Build();
        var mediumValueAction = ActionBuilder.MediumValue().Build();
        var highValueAction = ActionBuilder.HighValue().Build();

        // Assert
        Assert.Equal(5, lowValueAction.PointValue);
        Assert.Equal(10, mediumValueAction.PointValue);
        Assert.Equal(20, highValueAction.PointValue);
    }

    [Fact]
    public void PoolWorkoutBuilder_ShouldCreateValidRelationship()
    {
        // Arrange
        var pool = TestDataBuilder.Pool().WithId(1).Build();
        var workout = TestDataBuilder.Workout().WithId(2).Build();

        // Act
        var relationship = TestDataBuilder.PoolWorkout()
            .WithWorkoutPool(pool)
            .WithWorkout(workout)
            .Build();

        // Assert
        Assert.Equal(1, relationship.WorkoutPoolId);
        Assert.Equal(2, relationship.WorkoutId);
        Assert.Equal(pool, relationship.WorkoutPool);
        Assert.Equal(workout, relationship.Workout);
    }

    [Fact]
    public void ActionCompletionBuilder_ShouldCreateValidCompletion()
    {
        // Arrange
        var session = TestDataBuilder.Session().WithId(1).Build();
        var action = TestDataBuilder.Action().WithId(2).WithPointValue(10).Build();

        // Act
        var completion = TestDataBuilder.ActionCompletion()
            .WithSession(session)
            .WithAction(action)
            .WithPointsAwarded(10)
            .Build();

        // Assert
        Assert.Equal(1, completion.SessionId);
        Assert.Equal(2, completion.ActionId);
        Assert.Equal(10, completion.PointsAwarded);
        Assert.Equal(session, completion.Session);
        Assert.Equal(action, completion.Action);
        Assert.True(completion.CompletedAt > DateTime.MinValue);
    }

    [Fact]
    public void WorkoutReceivedBuilder_ShouldCreateValidReceived()
    {
        // Arrange
        var session = TestDataBuilder.Session().WithId(1).Build();
        var workout = TestDataBuilder.Workout().WithId(2).Build();

        // Act
        var received = TestDataBuilder.WorkoutReceived()
            .WithSession(session)
            .WithWorkout(workout)
            .WithPointsSpent(20)
            .Build();

        // Assert
        Assert.Equal(1, received.SessionId);
        Assert.Equal(2, received.WorkoutId);
        Assert.Equal(20, received.PointsSpent);
        Assert.Equal(session, received.Session);
        Assert.Equal(workout, received.Workout);
        Assert.True(received.ReceivedAt > DateTime.MinValue);
    }

    [Fact]
    public void TestDataBuilder_FluentInterface_ShouldChainCorrectly()
    {
        // Act
        var workout = TestDataBuilder.Workout()
            .WithName("Chained Workout")
            .WithDescription("Chained Description")
            .WithDuration(25)
            .WithDifficulty(DifficultyLevel.Advanced)
            .AsHidden()
            .Build();

        // Assert
        Assert.Equal("Chained Workout", workout.Name);
        Assert.Equal("Chained Description", workout.Description);
        Assert.Equal(25, workout.DurationMinutes);
        Assert.Equal(DifficultyLevel.Advanced, workout.Difficulty);
        Assert.True(workout.IsHidden);
    }

    [Fact]
    public void TestDataBuilder_RandomGeneration_ShouldProduceValidData()
    {
        // Act - Create multiple random entities
        var workouts = Enumerable.Range(0, 10)
            .Select(_ => TestDataBuilder.Workout().Build())
            .ToList();

        var sessions = Enumerable.Range(0, 10)
            .Select(_ => TestDataBuilder.Session().Build())
            .ToList();

        var actions = Enumerable.Range(0, 10)
            .Select(_ => TestDataBuilder.Action().Build())
            .ToList();

        // Assert - All should have valid data
        Assert.All(workouts, w =>
        {
            Assert.False(string.IsNullOrEmpty(w.Name));
            Assert.False(string.IsNullOrEmpty(w.Description));
            Assert.True(w.DurationMinutes > 0);
            Assert.True(Enum.IsDefined(typeof(DifficultyLevel), w.Difficulty));
        });

        Assert.All(sessions, s =>
        {
            Assert.False(string.IsNullOrEmpty(s.Name));
            Assert.True(Enum.IsDefined(typeof(SessionStatus), s.Status));
            Assert.True(s.StartTime > DateTime.MinValue);
        });

        Assert.All(actions, a =>
        {
            Assert.False(string.IsNullOrEmpty(a.Description));
            Assert.True(a.PointValue > 0);
        });

        // Verify some randomness (not all should be identical)
        Assert.True(workouts.Select(w => w.Name).Distinct().Count() > 1);
        Assert.True(sessions.Select(s => s.Name).Distinct().Count() > 1);
        Assert.True(actions.Select(a => a.Description).Distinct().Count() > 1);
    }
}
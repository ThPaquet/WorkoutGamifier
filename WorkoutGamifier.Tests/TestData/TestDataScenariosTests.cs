using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Tests.TestData;

namespace WorkoutGamifier.Tests.TestData;

/// <summary>
/// Tests to verify the test data scenarios work correctly and produce valid, realistic test data
/// </summary>
public class TestDataScenariosTests
{
    [Fact]
    public void BeginnerUserScenario_ShouldCreateValidBeginnerData()
    {
        // Act
        var workouts = TestDataScenarios.BeginnerUserScenario.GetWorkouts();
        var actions = TestDataScenarios.BeginnerUserScenario.GetActions();
        var pool = TestDataScenarios.BeginnerUserScenario.GetWorkoutPool();
        var session = TestDataScenarios.BeginnerUserScenario.GetTypicalSession();

        // Assert
        Assert.All(workouts, w =>
        {
            Assert.Equal(DifficultyLevel.Beginner, w.Difficulty);
            Assert.True(w.DurationMinutes <= 10); // Beginner workouts should be short
            Assert.False(string.IsNullOrEmpty(w.Name));
            Assert.False(string.IsNullOrEmpty(w.Description));
        });

        Assert.All(actions, a =>
        {
            Assert.True(a.PointValue <= 8); // Beginner actions should have low point values
            Assert.False(string.IsNullOrEmpty(a.Description));
        });

        Assert.Contains("Beginner", pool.Name);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.True(session.CurrentPointBalance >= 0);
    }

    [Fact]
    public void IntermediateUserScenario_ShouldCreateValidIntermediateData()
    {
        // Act
        var workouts = TestDataScenarios.IntermediateUserScenario.GetWorkouts();
        var actions = TestDataScenarios.IntermediateUserScenario.GetActions();
        var pool = TestDataScenarios.IntermediateUserScenario.GetWorkoutPool();
        var session = TestDataScenarios.IntermediateUserScenario.GetTypicalSession();

        // Assert
        Assert.All(workouts, w =>
        {
            Assert.Equal(DifficultyLevel.Intermediate, w.Difficulty);
            Assert.True(w.DurationMinutes >= 8 && w.DurationMinutes <= 20); // Intermediate duration range
            Assert.False(string.IsNullOrEmpty(w.Name));
            Assert.False(string.IsNullOrEmpty(w.Description));
        });

        Assert.All(actions, a =>
        {
            Assert.True(a.PointValue >= 10 && a.PointValue <= 15); // Intermediate point range
            Assert.False(string.IsNullOrEmpty(a.Description));
        });

        Assert.Contains("Strength", pool.Name);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.True(session.CurrentPointBalance > 0); // Should have positive balance
    }

    [Fact]
    public void AdvancedUserScenario_ShouldCreateValidAdvancedData()
    {
        // Act
        var workouts = TestDataScenarios.AdvancedUserScenario.GetWorkouts();
        var actions = TestDataScenarios.AdvancedUserScenario.GetActions();
        var pool = TestDataScenarios.AdvancedUserScenario.GetWorkoutPool();
        var session = TestDataScenarios.AdvancedUserScenario.GetTypicalSession();

        // Assert
        Assert.All(workouts, w =>
        {
            Assert.Equal(DifficultyLevel.Advanced, w.Difficulty);
            Assert.True(w.DurationMinutes >= 15); // Advanced workouts should be longer
            Assert.False(string.IsNullOrEmpty(w.Name));
            Assert.False(string.IsNullOrEmpty(w.Description));
        });

        Assert.All(actions, a =>
        {
            Assert.True(a.PointValue >= 18); // Advanced actions should have high point values
            Assert.False(string.IsNullOrEmpty(a.Description));
        });

        Assert.Contains("HIIT", pool.Name);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.True(session.CurrentPointBalance > 0);
    }

    [Fact]
    public void EdgeCaseScenarios_ShouldCreateValidBoundaryData()
    {
        // Act
        var minimalWorkout = TestDataScenarios.EdgeCaseScenarios.GetMinimalWorkout();
        var maximalWorkout = TestDataScenarios.EdgeCaseScenarios.GetMaximalWorkout();
        var zeroPointSession = TestDataScenarios.EdgeCaseScenarios.GetZeroPointSession();
        var highPointSession = TestDataScenarios.EdgeCaseScenarios.GetHighPointSession();
        var minimalAction = TestDataScenarios.EdgeCaseScenarios.GetMinimalAction();
        var maximalAction = TestDataScenarios.EdgeCaseScenarios.GetMaximalAction();

        // Assert
        Assert.Equal("A", minimalWorkout.Name);
        Assert.Equal(1, minimalWorkout.DurationMinutes);
        Assert.Equal(DifficultyLevel.Beginner, minimalWorkout.Difficulty);

        Assert.Equal(100, maximalWorkout.Name.Length);
        Assert.Equal(500, maximalWorkout.Description.Length);
        Assert.Equal(120, maximalWorkout.DurationMinutes);
        Assert.Equal(DifficultyLevel.Advanced, maximalWorkout.Difficulty);

        Assert.Equal(0, zeroPointSession.CurrentPointBalance);
        Assert.Equal(500, highPointSession.CurrentPointBalance); // 1000 - 500

        Assert.Equal(1, minimalAction.PointValue);
        Assert.Equal(100, maximalAction.PointValue);
    }

    [Fact]
    public void CompleteSessionWorkflow_ShouldCreateValidWorkflowData()
    {
        // Act
        var pool = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.Pool;
        var workouts = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.Workouts;
        var actions = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.Actions;
        var session = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.StartingSession;

        // Set IDs for relationship testing
        session.Id = 1;
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].Id = i + 1;
        }
        for (int i = 0; i < workouts.Count; i++)
        {
            workouts[i].Id = i + 1;
        }

        var completions = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.ExpectedCompletions(session, actions);
        var receivedWorkouts = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.ExpectedWorkouts(session, workouts);

        // Assert
        Assert.NotNull(pool);
        Assert.Equal(2, workouts.Count);
        Assert.Equal(3, actions.Count);
        Assert.Equal(SessionStatus.Active, session.Status);

        Assert.Equal(3, completions.Count);
        Assert.All(completions, c =>
        {
            Assert.Equal(session.Id, c.SessionId);
            Assert.True(c.PointsAwarded > 0);
        });

        Assert.Equal(2, receivedWorkouts.Count);
        Assert.All(receivedWorkouts, r =>
        {
            Assert.Equal(session.Id, r.SessionId);
            Assert.Equal(10, r.PointsSpent);
        });
    }

    [Fact]
    public void PoolManagementWorkflow_ShouldCreateValidPoolData()
    {
        // Act
        var emptyPool = TestDataScenarios.WorkflowScenarios.PoolManagementWorkflow.EmptyPool;
        var fullPool = TestDataScenarios.WorkflowScenarios.PoolManagementWorkflow.FullPool;
        var workouts = TestDataScenarios.WorkflowScenarios.PoolManagementWorkflow.PoolWorkouts;

        // Set IDs for relationship testing
        fullPool.Id = 1;
        for (int i = 0; i < workouts.Count; i++)
        {
            workouts[i].Id = i + 1;
        }

        var relationships = TestDataScenarios.WorkflowScenarios.PoolManagementWorkflow.PoolWorkoutRelationships(fullPool, workouts);

        // Assert
        Assert.Equal("Empty Pool", emptyPool.Name);
        Assert.Equal("Full Pool", fullPool.Name);
        Assert.Equal(5, workouts.Count);

        // Verify difficulty distribution
        var beginnerCount = workouts.Count(w => w.Difficulty == DifficultyLevel.Beginner);
        var intermediateCount = workouts.Count(w => w.Difficulty == DifficultyLevel.Intermediate);
        var advancedCount = workouts.Count(w => w.Difficulty == DifficultyLevel.Advanced);

        Assert.Equal(2, beginnerCount);
        Assert.Equal(2, intermediateCount);
        Assert.Equal(1, advancedCount);

        Assert.Equal(5, relationships.Count);
        Assert.All(relationships, r =>
        {
            Assert.Equal(fullPool.Id, r.WorkoutPoolId);
            Assert.True(r.WorkoutId > 0);
        });
    }

    [Fact]
    public void PerformanceScenarios_ShouldCreateLargeDataSets()
    {
        // Act
        var workouts = TestDataScenarios.PerformanceScenarios.GetLargeWorkoutSet(50);
        var actions = TestDataScenarios.PerformanceScenarios.GetLargeActionSet(25);
        var sessions = TestDataScenarios.PerformanceScenarios.GetLargeSessionSet(10);
        var pools = TestDataScenarios.PerformanceScenarios.GetLargePoolSet(5);

        // Assert
        Assert.Equal(50, workouts.Count);
        Assert.Equal(25, actions.Count);
        Assert.Equal(10, sessions.Count);
        Assert.Equal(5, pools.Count);

        // Verify uniqueness
        Assert.Equal(50, workouts.Select(w => w.Name).Distinct().Count());
        Assert.Equal(25, actions.Select(a => a.Description).Distinct().Count());
        Assert.Equal(10, sessions.Select(s => s.Name).Distinct().Count());
        Assert.Equal(5, pools.Select(p => p.Name).Distinct().Count());

        // Verify data variety
        var difficultyLevels = workouts.Select(w => w.Difficulty).Distinct().Count();
        Assert.True(difficultyLevels >= 2); // Should have multiple difficulty levels

        var pointValues = actions.Select(a => a.PointValue).Distinct().Count();
        Assert.True(pointValues >= 10); // Should have varied point values

        // Verify sessions have different dates
        var sessionDates = sessions.Select(s => s.StartTime.Date).Distinct().Count();
        Assert.True(sessionDates >= 5); // Should span multiple days
    }

    [Fact]
    public void DataConsistencyScenarios_ShouldCreateValidRelationships()
    {
        // Act
        var (pool, workouts, poolWorkoutRelationships) = TestDataScenarios.DataConsistencyScenarios.ValidRelationships.GetValidPoolWorkoutRelationships();
        var (session, actions, actionCompletions) = TestDataScenarios.DataConsistencyScenarios.ValidRelationships.GetValidSessionActionCompletions();

        // Set IDs to simulate database relationships
        pool.Id = 1;
        session.Id = 1;
        for (int i = 0; i < workouts.Count; i++)
        {
            workouts[i].Id = i + 1;
        }
        for (int i = 0; i < actions.Count; i++)
        {
            actions[i].Id = i + 1;
        }

        // Recreate relationships with correct IDs
        poolWorkoutRelationships = workouts.Select(w => TestDataBuilder.PoolWorkout()
            .WithWorkoutPool(pool.Id)
            .WithWorkout(w.Id)
            .Build()).ToList();

        actionCompletions = actions.Select(a => TestDataBuilder.ActionCompletion()
            .WithSession(session.Id)
            .WithAction(a.Id)
            .WithPointsAwarded(a.PointValue)
            .Build()).ToList();

        // Assert pool-workout relationships
        Assert.Equal(3, workouts.Count);
        Assert.Equal(3, poolWorkoutRelationships.Count);
        Assert.All(poolWorkoutRelationships, r =>
        {
            Assert.Equal(pool.Id, r.WorkoutPoolId);
            Assert.Contains(r.WorkoutId, workouts.Select(w => w.Id));
        });

        // Assert session-action relationships
        Assert.Equal(3, actions.Count);
        Assert.Equal(3, actionCompletions.Count);
        Assert.All(actionCompletions, c =>
        {
            Assert.Equal(session.Id, c.SessionId);
            Assert.Contains(c.ActionId, actions.Select(a => a.Id));
            
            // Verify points awarded match action point value
            var matchingAction = actions.FirstOrDefault(a => a.Id == c.ActionId);
            Assert.NotNull(matchingAction);
            Assert.Equal(matchingAction.PointValue, c.PointsAwarded);
        });
    }

    [Fact]
    public void AllScenarios_ShouldProduceValidTimestamps()
    {
        // Act - Get data from various scenarios
        var beginnerWorkouts = TestDataScenarios.BeginnerUserScenario.GetWorkouts();
        var intermediateActions = TestDataScenarios.IntermediateUserScenario.GetActions();
        var advancedSession = TestDataScenarios.AdvancedUserScenario.GetTypicalSession();
        var workflowPool = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.Pool;

        // Assert - All entities should have valid timestamps
        Assert.All(beginnerWorkouts, w =>
        {
            Assert.True(w.CreatedAt > DateTime.MinValue);
            Assert.True(w.UpdatedAt > DateTime.MinValue);
            Assert.True(w.CreatedAt <= DateTime.UtcNow);
            Assert.True(w.UpdatedAt <= DateTime.UtcNow);
        });

        Assert.All(intermediateActions, a =>
        {
            Assert.True(a.CreatedAt > DateTime.MinValue);
            Assert.True(a.UpdatedAt > DateTime.MinValue);
            Assert.True(a.CreatedAt <= DateTime.UtcNow);
            Assert.True(a.UpdatedAt <= DateTime.UtcNow);
        });

        Assert.True(advancedSession.CreatedAt > DateTime.MinValue);
        Assert.True(advancedSession.UpdatedAt > DateTime.MinValue);
        Assert.True(advancedSession.StartTime > DateTime.MinValue);

        Assert.True(workflowPool.CreatedAt > DateTime.MinValue);
        Assert.True(workflowPool.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public void AllScenarios_ShouldHaveReasonableDataDistribution()
    {
        // Act - Create large datasets to test distribution
        var workouts = TestDataScenarios.PerformanceScenarios.GetLargeWorkoutSet(30);
        var actions = TestDataScenarios.PerformanceScenarios.GetLargeActionSet(30);

        // Assert - Data should be reasonably distributed
        var workoutDurations = workouts.Select(w => w.DurationMinutes).ToList();
        var minDuration = workoutDurations.Min();
        var maxDuration = workoutDurations.Max();
        var avgDuration = workoutDurations.Average();

        Assert.True(minDuration >= 1);
        Assert.True(maxDuration <= 120);
        Assert.True(avgDuration > 10 && avgDuration < 100); // Reasonable average

        var actionPoints = actions.Select(a => a.PointValue).ToList();
        var minPoints = actionPoints.Min();
        var maxPoints = actionPoints.Max();
        var avgPoints = actionPoints.Average();

        Assert.True(minPoints >= 5);
        Assert.True(maxPoints <= 25);
        Assert.True(avgPoints > 8 && avgPoints < 20); // Reasonable average
    }
}
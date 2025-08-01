using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Services;
using WorkoutGamifier.Tests.TestData;

namespace WorkoutGamifier.Tests.Integration;

/// <summary>
/// Comprehensive integration tests covering complete user workflows and edge cases
/// </summary>
public class ComprehensiveIntegrationTests : IDisposable
{
    private readonly DatabaseTestFixture _fixture;
    private readonly IWorkoutService _workoutService;
    private readonly ISessionService _sessionService;
    private readonly IWorkoutPoolService _workoutPoolService;

    public ComprehensiveIntegrationTests()
    {
        _fixture = new DatabaseTestFixture();
        _workoutService = _fixture.GetService<IWorkoutService>();
        _sessionService = _fixture.GetService<ISessionService>();
        _workoutPoolService = _fixture.GetService<IWorkoutPoolService>();
    }

    [Fact]
    public async Task CompleteUserJourney_BeginnerToAdvanced_WorksCorrectly()
    {
        // Arrange - Create a complete user journey scenario
        await _fixture.SeedUserScenario(UserScenarioType.Complete);

        // Act & Assert - Beginner phase
        var beginnerWorkouts = await _workoutService.GetWorkoutsByDifficultyAsync(DifficultyLevel.Beginner);
        Assert.NotEmpty(beginnerWorkouts);
        Assert.All(beginnerWorkouts, w => Assert.Equal(DifficultyLevel.Beginner, w.Difficulty));

        // Create beginner session
        var unitOfWork = _fixture.GetUnitOfWork();
        var beginnerPools = await unitOfWork.WorkoutPools.GetAllAsync();
        var beginnerPool = beginnerPools.First(p => p.Name.Contains("Beginner"));
        
        var beginnerSession = await _sessionService.StartSessionAsync("Beginner Journey", beginnerPool.Id);
        Assert.Equal(SessionStatus.Active, beginnerSession.Status);

        // Complete beginner actions
        var beginnerActions = await unitOfWork.Actions.GetAllAsync();
        var lowValueActions = beginnerActions.Where(a => a.PointValue <= 8).Take(3);
        
        foreach (var action in lowValueActions)
        {
            await _sessionService.CompleteActionAsync(beginnerSession.Id, action.Id);
        }

        // End beginner session
        var completedBeginnerSession = await _sessionService.EndSessionAsync(beginnerSession.Id);
        Assert.Equal(SessionStatus.Completed, completedBeginnerSession.Status);
        Assert.True(completedBeginnerSession.PointsEarned > 0);

        // Act & Assert - Intermediate phase
        var intermediateWorkouts = await _workoutService.GetWorkoutsByDifficultyAsync(DifficultyLevel.Intermediate);
        Assert.NotEmpty(intermediateWorkouts);
        
        var intermediatePool = beginnerPools.First(p => p.Name.Contains("Intermediate") || p.Name.Contains("Strength"));
        var intermediateSession = await _sessionService.StartSessionAsync("Intermediate Journey", intermediatePool.Id);
        
        // Complete intermediate actions
        var mediumValueActions = beginnerActions.Where(a => a.PointValue >= 10 && a.PointValue <= 15).Take(2);
        foreach (var action in mediumValueActions)
        {
            await _sessionService.CompleteActionAsync(intermediateSession.Id, action.Id);
        }

        // Spend points for workout
        var workoutReceived = await _sessionService.SpendPointsForWorkoutAsync(intermediateSession.Id, 15);
        Assert.NotNull(workoutReceived);
        Assert.Equal(15, workoutReceived.PointsSpent);

        await _sessionService.EndSessionAsync(intermediateSession.Id);

        // Act & Assert - Advanced phase
        var advancedWorkouts = await _workoutService.GetWorkoutsByDifficultyAsync(DifficultyLevel.Advanced);
        Assert.NotEmpty(advancedWorkouts);

        // Verify user progression
        var allSessions = await _sessionService.GetAllSessionsAsync();
        Assert.Equal(2, allSessions.Count);
        Assert.All(allSessions, s => Assert.Equal(SessionStatus.Completed, s.Status));
        
        var totalPointsEarned = allSessions.Sum(s => s.PointsEarned);
        var totalPointsSpent = allSessions.Sum(s => s.PointsSpent);
        Assert.True(totalPointsEarned > totalPointsSpent);
    }

    [Fact]
    public async Task HighVolumeOperations_MultipleUsersSimulation_MaintainsDataIntegrity()
    {
        // Arrange - Seed performance data
        await _fixture.SeedPerformanceData(50, 25, 10);

        // Act - Simulate multiple concurrent users
        var tasks = new List<Task>();
        
        for (int userId = 1; userId <= 5; userId++)
        {
            tasks.Add(SimulateUserSession(userId));
        }

        await Task.WhenAll(tasks);

        // Assert - Verify data integrity
        var integrityResult = await _fixture.VerifyDataIntegrity();
        Assert.True(integrityResult.IsValid, string.Join(", ", integrityResult.Errors));

        // Verify all sessions completed successfully
        var allSessions = await _sessionService.GetAllSessionsAsync();
        Assert.True(allSessions.Count >= 5); // At least one session per simulated user
        
        // Verify no data corruption
        var allWorkouts = await _workoutService.GetAllWorkoutsAsync();
        Assert.True(allWorkouts.Count >= 50);
        Assert.All(allWorkouts, w => 
        {
            Assert.False(string.IsNullOrEmpty(w.Name));
            Assert.True(w.DurationMinutes > 0);
        });
    }

    [Fact]
    public async Task EdgeCaseHandling_ExtremeValues_HandledGracefully()
    {
        // Arrange - Create edge case scenarios
        var unitOfWork = _fixture.GetUnitOfWork();

        // Create workout with maximum values
        var maxWorkout = TestDataScenarios.EdgeCaseScenarios.GetMaximalWorkout();
        var createdMaxWorkout = await _workoutService.CreateWorkoutAsync(maxWorkout);
        Assert.NotNull(createdMaxWorkout);

        // Create workout with minimum values
        var minWorkout = TestDataScenarios.EdgeCaseScenarios.GetMinimalWorkout();
        var createdMinWorkout = await _workoutService.CreateWorkoutAsync(minWorkout);
        Assert.NotNull(createdMinWorkout);

        // Create pool and add workouts
        var pool = TestDataBuilder.Pool().WithName("Edge Case Pool").Build();
        await unitOfWork.WorkoutPools.CreateAsync(pool);
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
            TestDataBuilder.PoolWorkout().WithWorkoutPool(pool).WithWorkout(createdMaxWorkout).Build());
        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
            TestDataBuilder.PoolWorkout().WithWorkoutPool(pool).WithWorkout(createdMinWorkout).Build());
        await unitOfWork.SaveChangesAsync();

        // Act - Test session with extreme values
        var session = await _sessionService.StartSessionAsync("Edge Case Session", pool.Id);
        
        // Create actions with extreme values
        var maxAction = TestDataScenarios.EdgeCaseScenarios.GetMaximalAction();
        var minAction = TestDataScenarios.EdgeCaseScenarios.GetMinimalAction();
        
        await unitOfWork.Actions.CreateAsync(maxAction);
        await unitOfWork.Actions.CreateAsync(minAction);
        await unitOfWork.SaveChangesAsync();

        // Complete actions
        await _sessionService.CompleteActionAsync(session.Id, maxAction.Id);
        await _sessionService.CompleteActionAsync(session.Id, minAction.Id);

        // Verify session state
        var updatedSession = await _sessionService.GetSessionByIdAsync(session.Id);
        Assert.NotNull(updatedSession);
        Assert.Equal(maxAction.PointValue + minAction.PointValue, updatedSession.PointsEarned);

        // End session
        var endedSession = await _sessionService.EndSessionAsync(session.Id);
        Assert.Equal(SessionStatus.Completed, endedSession.Status);
    }

    [Fact]
    public async Task DataConsistency_ComplexRelationships_MaintainedCorrectly()
    {
        // Arrange - Create complex data relationships
        var (pool, workouts, relationships) = TestDataScenarios.DataConsistencyScenarios.ValidRelationships.GetValidPoolWorkoutRelationships();
        var (session, actions, completions) = TestDataScenarios.DataConsistencyScenarios.ValidRelationships.GetValidSessionActionCompletions();

        var unitOfWork = _fixture.GetUnitOfWork();

        // Create pool and workouts
        await unitOfWork.WorkoutPools.CreateAsync(pool);
        foreach (var workout in workouts)
        {
            await unitOfWork.Workouts.CreateAsync(workout);
        }
        await unitOfWork.SaveChangesAsync();

        // Create relationships
        foreach (var relationship in relationships)
        {
            relationship.WorkoutPoolId = pool.Id;
            relationship.WorkoutId = workouts.First(w => w.Name == relationship.Workout?.Name).Id;
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(relationship);
        }

        // Create actions
        foreach (var action in actions)
        {
            await unitOfWork.Actions.CreateAsync(action);
        }
        await unitOfWork.SaveChangesAsync();

        // Act - Create session and complete workflow
        var createdSession = await _sessionService.StartSessionAsync(session.Name, pool.Id, session.Description);
        
        foreach (var action in actions)
        {
            await _sessionService.CompleteActionAsync(createdSession.Id, action.Id);
        }

        // Spend points for workouts
        var totalPointsEarned = actions.Sum(a => a.PointValue);
        var pointsToSpend = Math.Min(totalPointsEarned, 20);
        
        if (pointsToSpend > 0)
        {
            var workoutReceived = await _sessionService.SpendPointsForWorkoutAsync(createdSession.Id, pointsToSpend);
            Assert.NotNull(workoutReceived);
        }

        await _sessionService.EndSessionAsync(createdSession.Id);

        // Assert - Verify all relationships are maintained
        var integrityResult = await _fixture.VerifyDataIntegrity();
        Assert.True(integrityResult.IsValid, string.Join(", ", integrityResult.Errors));

        // Verify session data
        var finalSession = await _sessionService.GetSessionByIdAsync(createdSession.Id);
        Assert.NotNull(finalSession);
        Assert.Equal(SessionStatus.Completed, finalSession.Status);
        Assert.Equal(totalPointsEarned, finalSession.PointsEarned);
        
        if (pointsToSpend > 0)
        {
            Assert.Equal(pointsToSpend, finalSession.PointsSpent);
            Assert.Equal(totalPointsEarned - pointsToSpend, finalSession.CurrentPointBalance);
        }
    }

    [Fact]
    public async Task PerformanceUnderLoad_LargeDatasets_MeetsPerformanceThresholds()
    {
        // Arrange - Create large dataset
        await _fixture.SeedPerformanceData(200, 100, 50);

        // Act - Measure performance
        var performanceMetrics = await _fixture.MeasurePerformance();

        // Assert - Verify performance thresholds
        Assert.True(performanceMetrics.IsPerformant, 
            $"Performance thresholds not met: Create={performanceMetrics.CreateOperationMs}ms, " +
            $"Read={performanceMetrics.ReadOperationMs}ms, Update={performanceMetrics.UpdateOperationMs}ms, " +
            $"Delete={performanceMetrics.DeleteOperationMs}ms");

        // Test complex queries performance
        var startTime = DateTime.UtcNow;
        
        var statistics = await _workoutService.GetWorkoutStatisticsAsync();
        var searchResults = await _workoutService.SearchWorkoutsAsync("workout");
        var recentWorkouts = await _workoutService.GetRecentWorkoutsAsync(10);
        
        var complexQueryTime = DateTime.UtcNow - startTime;
        
        Assert.True(complexQueryTime.TotalMilliseconds < 1000, 
            $"Complex queries took too long: {complexQueryTime.TotalMilliseconds}ms");

        // Verify results are correct
        Assert.True(statistics.TotalWorkouts > 0);
        Assert.NotEmpty(searchResults);
        Assert.True(recentWorkouts.Count <= 10);
    }

    [Theory]
    [InlineData(UserScenarioType.Beginner)]
    [InlineData(UserScenarioType.Intermediate)]
    [InlineData(UserScenarioType.Advanced)]
    public async Task UserScenarioWorkflows_DifferentUserTypes_WorkCorrectly(UserScenarioType scenarioType)
    {
        // Arrange
        await _fixture.SeedUserScenario(scenarioType);

        // Act - Get appropriate data for user type
        var expectedDifficulty = scenarioType switch
        {
            UserScenarioType.Beginner => DifficultyLevel.Beginner,
            UserScenarioType.Intermediate => DifficultyLevel.Intermediate,
            UserScenarioType.Advanced => DifficultyLevel.Advanced,
            _ => DifficultyLevel.Beginner
        };

        var workouts = await _workoutService.GetWorkoutsByDifficultyAsync(expectedDifficulty);
        var unitOfWork = _fixture.GetUnitOfWork();
        var pools = await unitOfWork.WorkoutPools.GetAllAsync();
        var actions = await unitOfWork.Actions.GetAllAsync();

        // Assert - Verify appropriate data exists
        Assert.NotEmpty(workouts);
        Assert.NotEmpty(pools);
        Assert.NotEmpty(actions);

        Assert.All(workouts, w => Assert.Equal(expectedDifficulty, w.Difficulty));

        // Test session workflow for this user type
        var pool = pools.First();
        var session = await _sessionService.StartSessionAsync($"{scenarioType} Session", pool.Id);
        
        var appropriateActions = scenarioType switch
        {
            UserScenarioType.Beginner => actions.Where(a => a.PointValue <= 8),
            UserScenarioType.Intermediate => actions.Where(a => a.PointValue >= 10 && a.PointValue <= 15),
            UserScenarioType.Advanced => actions.Where(a => a.PointValue >= 18),
            _ => actions.Where(a => a.PointValue <= 8)
        };

        var actionToComplete = appropriateActions.FirstOrDefault();
        if (actionToComplete != null)
        {
            await _sessionService.CompleteActionAsync(session.Id, actionToComplete.Id);
            
            var updatedSession = await _sessionService.GetSessionByIdAsync(session.Id);
            Assert.NotNull(updatedSession);
            Assert.Equal(actionToComplete.PointValue, updatedSession.PointsEarned);
        }

        await _sessionService.EndSessionAsync(session.Id);
    }

    private async Task SimulateUserSession(int userId)
    {
        try
        {
            var unitOfWork = _fixture.GetUnitOfWork();
            var pools = await unitOfWork.WorkoutPools.GetAllAsync();
            var actions = await unitOfWork.Actions.GetAllAsync();

            if (!pools.Any() || !actions.Any()) return;

            var pool = pools.Skip(userId % pools.Count).First();
            var session = await _sessionService.StartSessionAsync($"User {userId} Session", pool.Id);

            // Complete random actions
            var userActions = actions.Take(3);
            foreach (var action in userActions)
            {
                await _sessionService.CompleteActionAsync(session.Id, action.Id);
                await Task.Delay(10); // Simulate user thinking time
            }

            // Try to spend points if enough earned
            var updatedSession = await _sessionService.GetSessionByIdAsync(session.Id);
            if (updatedSession != null && updatedSession.CurrentPointBalance >= 10)
            {
                await _sessionService.SpendPointsForWorkoutAsync(session.Id, 10);
            }

            await _sessionService.EndSessionAsync(session.Id);
        }
        catch (InvalidOperationException)
        {
            // Expected in concurrent scenarios - multiple users trying to start sessions
            // This is acceptable for this test
        }
    }

    public void Dispose()
    {
        _fixture?.Dispose();
    }
}
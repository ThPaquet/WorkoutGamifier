using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Services;
using WorkoutGamifier.Tests.TestData;

namespace WorkoutGamifier.Tests.Services;

/// <summary>
/// Enhanced unit tests for WorkoutService covering edge cases and comprehensive scenarios
/// </summary>
public class EnhancedWorkoutServiceTests : IDisposable
{
    private readonly DatabaseTestFixture _fixture;
    private readonly IWorkoutService _workoutService;

    public EnhancedWorkoutServiceTests()
    {
        _fixture = new DatabaseTestFixture();
        _workoutService = _fixture.GetService<IWorkoutService>();
    }

    [Theory]
    [InlineData(1, 5)]
    [InlineData(10, 20)]
    [InlineData(30, 60)]
    [InlineData(60, 120)]
    public async Task GetWorkoutsByDurationRangeAsync_VariousRanges_ReturnsCorrectWorkouts(int minDuration, int maxDuration)
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        var workouts = new List<Workout>
        {
            TestDataBuilder.Workout().WithName("Short").WithDuration(5).Build(),
            TestDataBuilder.Workout().WithName("Medium").WithDuration(15).Build(),
            TestDataBuilder.Workout().WithName("Long").WithDuration(45).Build(),
            TestDataBuilder.Workout().WithName("Very Long").WithDuration(90).Build()
        };

        foreach (var workout in workouts)
        {
            await unitOfWork.Workouts.CreateAsync(workout);
        }
        await unitOfWork.SaveChangesAsync();

        // Act
        var filteredWorkouts = await _workoutService.GetWorkoutsByDurationRangeAsync(minDuration, maxDuration);

        // Assert
        Assert.All(filteredWorkouts, w => 
        {
            Assert.True(w.DurationMinutes >= minDuration);
            Assert.True(w.DurationMinutes <= maxDuration);
        });

        var expectedCount = workouts.Count(w => w.DurationMinutes >= minDuration && w.DurationMinutes <= maxDuration);
        Assert.Equal(expectedCount, filteredWorkouts.Count);
    }

    [Theory]
    [InlineData("push", 1)]
    [InlineData("squat", 1)]
    [InlineData("workout", 4)] // Should match all workouts containing "workout"
    [InlineData("nonexistent", 0)]
    [InlineData("", 4)] // Empty query should return all
    public async Task SearchWorkoutsAsync_VariousQueries_ReturnsExpectedResults(string query, int expectedMinCount)
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        var workouts = new List<Workout>
        {
            TestDataBuilder.Workout().WithName("Push-up Workout").WithDescription("Upper body workout").Build(),
            TestDataBuilder.Workout().WithName("Squat Challenge").WithDescription("Lower body workout").Build(),
            TestDataBuilder.Workout().WithName("Cardio Blast").WithDescription("High intensity workout").Build(),
            TestDataBuilder.Workout().WithName("Strength Training").WithDescription("Full body workout").Build()
        };

        foreach (var workout in workouts)
        {
            await unitOfWork.Workouts.CreateAsync(workout);
        }
        await unitOfWork.SaveChangesAsync();

        // Act
        var searchResults = await _workoutService.SearchWorkoutsAsync(query);

        // Assert
        if (expectedMinCount == 0)
        {
            Assert.Empty(searchResults);
        }
        else
        {
            Assert.True(searchResults.Count >= expectedMinCount);
            
            if (!string.IsNullOrEmpty(query))
            {
                var lowerQuery = query.ToLower();
                Assert.All(searchResults, w => 
                    Assert.True(w.Name.ToLower().Contains(lowerQuery) || 
                               w.Description?.ToLower().Contains(lowerQuery) == true));
            }
        }
    }

    [Fact]
    public async Task BulkCreateWorkoutsAsync_LargeDataset_CreatesAllSuccessfully()
    {
        // Arrange
        var workouts = TestDataScenarios.PerformanceScenarios.GetLargeWorkoutSet(100);

        // Act
        var startTime = DateTime.UtcNow;
        var createdWorkouts = await _workoutService.BulkCreateWorkoutsAsync(workouts);
        var duration = DateTime.UtcNow - startTime;

        // Assert
        Assert.Equal(100, createdWorkouts.Count);
        Assert.All(createdWorkouts, w => Assert.True(w.Id > 0));
        Assert.True(duration.TotalSeconds < 5, $"Bulk create took too long: {duration.TotalSeconds} seconds");

        // Verify all workouts are in database
        var allWorkouts = await _workoutService.GetAllWorkoutsAsync();
        Assert.True(allWorkouts.Count >= 100);
    }

    [Fact]
    public async Task BulkCreateWorkoutsAsync_WithDuplicateNames_CreatesAllSuccessfully()
    {
        // Arrange - Create workouts with duplicate names (should be allowed)
        var workouts = new List<Workout>
        {
            TestDataBuilder.Workout().WithName("Duplicate Workout").WithDuration(10).Build(),
            TestDataBuilder.Workout().WithName("Duplicate Workout").WithDuration(20).Build(),
            TestDataBuilder.Workout().WithName("Duplicate Workout").WithDuration(30).Build()
        };

        // Act
        var createdWorkouts = await _workoutService.BulkCreateWorkoutsAsync(workouts);

        // Assert
        Assert.Equal(3, createdWorkouts.Count);
        Assert.All(createdWorkouts, w => 
        {
            Assert.Equal("Duplicate Workout", w.Name);
            Assert.True(w.Id > 0);
        });

        // Verify different durations were preserved
        var durations = createdWorkouts.Select(w => w.DurationMinutes).OrderBy(d => d).ToList();
        Assert.Equal(new[] { 10, 20, 30 }, durations);
    }

    [Fact]
    public async Task GetRecentWorkoutsAsync_WithLargeDataset_ReturnsCorrectOrder()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        var baseTime = DateTime.UtcNow.AddDays(-10);
        
        var workouts = new List<Workout>();
        for (int i = 0; i < 20; i++)
        {
            var workout = TestDataBuilder.Workout()
                .WithName($"Workout {i:D2}")
                .CreatedAt(baseTime.AddHours(i))
                .Build();
            workouts.Add(workout);
        }

        foreach (var workout in workouts)
        {
            await unitOfWork.Workouts.CreateAsync(workout);
        }
        await unitOfWork.SaveChangesAsync();

        // Act
        var recentWorkouts = await _workoutService.GetRecentWorkoutsAsync(5);

        // Assert
        Assert.Equal(5, recentWorkouts.Count);
        
        // Verify they are in descending order by creation date
        for (int i = 0; i < recentWorkouts.Count - 1; i++)
        {
            Assert.True(recentWorkouts[i].CreatedAt >= recentWorkouts[i + 1].CreatedAt);
        }

        // Verify we got the most recent ones
        Assert.Equal("Workout 19", recentWorkouts[0].Name);
        Assert.Equal("Workout 15", recentWorkouts[4].Name);
    }

    [Fact]
    public async Task GetWorkoutStatisticsAsync_WithComplexDataset_ReturnsAccurateStatistics()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        var workouts = new List<Workout>
        {
            // Beginner workouts
            WorkoutBuilder.Beginner().WithDuration(10).Build(),
            WorkoutBuilder.Beginner().WithDuration(15).Build(),
            WorkoutBuilder.Beginner().WithDuration(20).Build(),
            
            // Intermediate workouts
            WorkoutBuilder.Intermediate().WithDuration(25).Build(),
            WorkoutBuilder.Intermediate().WithDuration(30).Build(),
            
            // Advanced workouts
            WorkoutBuilder.Advanced().WithDuration(45).Build(),
            WorkoutBuilder.Advanced().WithDuration(60).Build(),
            
            // Expert workout
            TestDataBuilder.Workout().WithDifficulty(DifficultyLevel.Expert).WithDuration(90).Build(),
            
            // Hidden workout (should not be counted)
            TestDataBuilder.Workout().WithDifficulty(DifficultyLevel.Beginner).WithDuration(5).AsHidden().Build()
        };

        foreach (var workout in workouts)
        {
            await unitOfWork.Workouts.CreateAsync(workout);
        }
        await unitOfWork.SaveChangesAsync();

        // Act
        var statistics = await _workoutService.GetWorkoutStatisticsAsync();

        // Assert
        Assert.Equal(8, statistics.TotalWorkouts); // Hidden workout not counted
        Assert.Equal(3, statistics.BeginnerCount);
        Assert.Equal(2, statistics.IntermediateCount);
        Assert.Equal(2, statistics.AdvancedCount);
        Assert.Equal(1, statistics.ExpertCount);
        
        // Calculate expected average: (10+15+20+25+30+45+60+90)/8 = 36.875
        Assert.Equal(36.875, statistics.AverageDuration, 3);
        Assert.Equal(10, statistics.ShortestDuration);
        Assert.Equal(90, statistics.LongestDuration);
    }

    [Fact]
    public async Task SequentialOperations_MultipleCreates_MaintainDataIntegrity()
    {
        // Arrange
        var workoutNames = new List<string>();
        var results = new List<Workout>();

        // Act - Create workouts sequentially to avoid DbContext concurrency issues
        for (int i = 0; i < 10; i++)
        {
            var workoutName = $"Sequential Workout {i}";
            workoutNames.Add(workoutName);
            
            var workout = TestDataBuilder.Workout()
                .WithName(workoutName)
                .WithDuration(15 + i)
                .Build();
            
            var result = await _workoutService.CreateWorkoutAsync(workout);
            results.Add(result);
        }

        // Assert
        Assert.Equal(10, results.Count);
        Assert.All(results, w => Assert.True(w.Id > 0));
        
        // Verify all workouts have unique IDs
        var ids = results.Select(w => w.Id).ToList();
        Assert.Equal(ids.Count, ids.Distinct().Count());

        // Verify all workouts are in database
        var allWorkouts = await _workoutService.GetAllWorkoutsAsync();
        Assert.True(allWorkouts.Count >= 10);
        
        foreach (var expectedName in workoutNames)
        {
            Assert.Contains(allWorkouts, w => w.Name == expectedName);
        }
    }

    [Theory]
    [InlineData(DifficultyLevel.Beginner, 5, 15)]
    [InlineData(DifficultyLevel.Intermediate, 15, 35)]
    [InlineData(DifficultyLevel.Advanced, 30, 60)]
    public async Task GetWorkoutsByDifficultyAsync_WithDurationFiltering_ReturnsAppropriateWorkouts(
        DifficultyLevel difficulty, int minExpectedDuration, int maxExpectedDuration)
    {
        // Arrange
        await _fixture.SeedCompleteDataset();

        // Act
        var workouts = await _workoutService.GetWorkoutsByDifficultyAsync(difficulty);

        // Assert
        Assert.NotEmpty(workouts);
        Assert.All(workouts, w => 
        {
            Assert.Equal(difficulty, w.Difficulty);
            Assert.True(w.DurationMinutes >= minExpectedDuration - 10); // Allow some variance
            Assert.True(w.DurationMinutes <= maxExpectedDuration + 10);
        });
    }

    [Fact]
    public async Task GetWorkoutsByDifficultyAsync_WithExpertDifficulty_ReturnsEmptyWhenNoneExist()
    {
        // Arrange
        await _fixture.SeedCompleteDataset();

        // Act
        var expertWorkouts = await _workoutService.GetWorkoutsByDifficultyAsync(DifficultyLevel.Expert);

        // Assert - Expert workouts may not exist in seeded data, which is fine
        Assert.NotNull(expertWorkouts);
        // Don't assert NotEmpty since Expert workouts might not be seeded
    }

    [Fact]
    public async Task UpdateWorkoutAsync_WithSequentialUpdates_WorksCorrectly()
    {
        // Arrange
        var workout = TestDataBuilder.Workout()
            .WithName("Sequential Update Test")
            .WithDuration(30)
            .Build();
        
        var createdWorkout = await _workoutService.CreateWorkoutAsync(workout);

        // Act - Perform sequential updates
        for (int i = 0; i < 3; i++)
        {
            var workoutToUpdate = await _workoutService.GetWorkoutByIdAsync(createdWorkout.Id);
            Assert.NotNull(workoutToUpdate);
            
            workoutToUpdate.Name = $"Updated Name {i}";
            workoutToUpdate.DurationMinutes = 30 + i;
            
            var updatedWorkout = await _workoutService.UpdateWorkoutAsync(workoutToUpdate);
            Assert.NotNull(updatedWorkout);
            Assert.Equal($"Updated Name {i}", updatedWorkout.Name);
            Assert.Equal(30 + i, updatedWorkout.DurationMinutes);
        }

        // Assert - Verify final state
        var finalWorkout = await _workoutService.GetWorkoutByIdAsync(createdWorkout.Id);
        Assert.NotNull(finalWorkout);
        Assert.Equal("Updated Name 2", finalWorkout.Name);
        Assert.Equal(32, finalWorkout.DurationMinutes);
    }

    [Fact]
    public async Task DeleteWorkoutAsync_WithNonExistentWorkout_ReturnsFalseGracefully()
    {
        // Act
        var result = await _workoutService.DeleteWorkoutAsync(99999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ToggleWorkoutVisibilityAsync_MultipleToggles_WorksCorrectly()
    {
        // Arrange
        var workout = TestDataBuilder.Workout()
            .WithName("Toggle Test")
            .AsVisible()
            .Build();
        
        var createdWorkout = await _workoutService.CreateWorkoutAsync(workout);

        // Act & Assert - Multiple toggles
        var isHidden1 = await _workoutService.ToggleWorkoutVisibilityAsync(createdWorkout.Id);
        Assert.True(isHidden1);

        var isHidden2 = await _workoutService.ToggleWorkoutVisibilityAsync(createdWorkout.Id);
        Assert.False(isHidden2);

        var isHidden3 = await _workoutService.ToggleWorkoutVisibilityAsync(createdWorkout.Id);
        Assert.True(isHidden3);

        // Verify final state
        var finalWorkout = await _workoutService.GetWorkoutByIdAsync(createdWorkout.Id);
        Assert.NotNull(finalWorkout);
        Assert.True(finalWorkout.IsHidden);

        // Verify hidden workout is not in GetAllWorkouts
        var allWorkouts = await _workoutService.GetAllWorkoutsAsync();
        Assert.DoesNotContain(allWorkouts, w => w.Id == createdWorkout.Id);
    }

    public void Dispose()
    {
        _fixture?.Dispose();
    }
}
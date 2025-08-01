using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Services;
using WorkoutGamifier.Tests.TestData;

namespace WorkoutGamifier.Tests.Services;

/// <summary>
/// Comprehensive tests for WorkoutService covering all business logic scenarios
/// </summary>
public class WorkoutServiceTests : IDisposable
{
    private readonly DatabaseTestFixture _fixture;
    private readonly IWorkoutService _workoutService;

    public WorkoutServiceTests()
    {
        _fixture = new DatabaseTestFixture();
        _workoutService = _fixture.GetService<IWorkoutService>();
    }

    [Fact]
    public async Task GetAllWorkoutsAsync_WithEmptyDatabase_ReturnsEmptyList()
    {
        // Act
        var workouts = await _workoutService.GetAllWorkoutsAsync();

        // Assert
        Assert.NotNull(workouts);
        Assert.Empty(workouts);
    }

    [Fact]
    public async Task GetAllWorkoutsAsync_WithWorkouts_ReturnsAllWorkouts()
    {
        // Arrange
        await _fixture.SeedCompleteDataset();

        // Act
        var workouts = await _workoutService.GetAllWorkoutsAsync();

        // Assert
        Assert.NotNull(workouts);
        Assert.True(workouts.Count >= 9); // We seed 9 workouts in complete dataset
        Assert.All(workouts, w => Assert.False(string.IsNullOrEmpty(w.Name)));
    }

    [Fact]
    public async Task GetAllWorkoutsAsync_ExcludesHiddenWorkouts()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        
        var visibleWorkout = TestDataBuilder.Workout()
            .WithName("Visible Workout")
            .AsVisible()
            .Build();
        
        var hiddenWorkout = TestDataBuilder.Workout()
            .WithName("Hidden Workout")
            .AsHidden()
            .Build();

        await unitOfWork.Workouts.CreateAsync(visibleWorkout);
        await unitOfWork.Workouts.CreateAsync(hiddenWorkout);
        await unitOfWork.SaveChangesAsync();

        // Act
        var workouts = await _workoutService.GetAllWorkoutsAsync();

        // Assert
        Assert.Single(workouts);
        Assert.Equal("Visible Workout", workouts.First().Name);
        Assert.False(workouts.First().IsHidden);
    }

    [Fact]
    public async Task GetWorkoutByIdAsync_WithValidId_ReturnsWorkout()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        var workout = TestDataBuilder.Workout()
            .WithName("Test Workout")
            .Build();
        
        await unitOfWork.Workouts.CreateAsync(workout);
        await unitOfWork.SaveChangesAsync();

        // Act
        var result = await _workoutService.GetWorkoutByIdAsync(workout.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(workout.Id, result.Id);
        Assert.Equal("Test Workout", result.Name);
    }

    [Fact]
    public async Task GetWorkoutByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Act
        var result = await _workoutService.GetWorkoutByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateWorkoutAsync_WithValidWorkout_CreatesSuccessfully()
    {
        // Arrange
        var workout = TestDataBuilder.Workout()
            .WithName("New Workout")
            .WithDescription("New workout description")
            .WithDuration(25)
            .WithDifficulty(DifficultyLevel.Intermediate)
            .Build();

        // Act
        var result = await _workoutService.CreateWorkoutAsync(workout);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("New Workout", result.Name);
        Assert.Equal("New workout description", result.Description);
        Assert.Equal(25, result.DurationMinutes);
        Assert.Equal(DifficultyLevel.Intermediate, result.Difficulty);
        Assert.True(result.CreatedAt > DateTime.MinValue);
        Assert.True(result.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task CreateWorkoutAsync_WithNullWorkout_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _workoutService.CreateWorkoutAsync(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateWorkoutAsync_WithInvalidName_ThrowsArgumentException(string invalidName)
    {
        // Arrange
        var workout = TestDataBuilder.Workout()
            .WithName(invalidName)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.CreateWorkoutAsync(workout));
    }

    [Fact]
    public async Task CreateWorkoutAsync_WithNullName_ThrowsArgumentException()
    {
        // Arrange
        var workout = TestDataBuilder.Workout()
            .WithName(null!)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.CreateWorkoutAsync(workout));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public async Task CreateWorkoutAsync_WithInvalidDuration_ThrowsArgumentException(int invalidDuration)
    {
        // Arrange
        var workout = TestDataBuilder.Workout()
            .WithDuration(invalidDuration)
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.CreateWorkoutAsync(workout));
    }

    [Fact]
    public async Task UpdateWorkoutAsync_WithValidWorkout_UpdatesSuccessfully()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        var workout = TestDataBuilder.Workout()
            .WithName("Original Name")
            .WithDuration(15)
            .Build();
        
        await unitOfWork.Workouts.CreateAsync(workout);
        await unitOfWork.SaveChangesAsync();

        // Modify the workout
        workout.Name = "Updated Name";
        workout.DurationMinutes = 30;

        // Act
        var result = await _workoutService.UpdateWorkoutAsync(workout);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(30, result.DurationMinutes);
        Assert.True(result.UpdatedAt > result.CreatedAt);
    }

    [Fact]
    public async Task UpdateWorkoutAsync_WithNonExistentWorkout_ThrowsInvalidOperationException()
    {
        // Arrange
        var workout = TestDataBuilder.Workout()
            .WithId(999)
            .WithName("Non-existent Workout")
            .Build();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutService.UpdateWorkoutAsync(workout));
    }

    [Fact]
    public async Task DeleteWorkoutAsync_WithValidId_DeletesSuccessfully()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        var workout = TestDataBuilder.Workout()
            .WithName("To Delete")
            .Build();
        
        await unitOfWork.Workouts.CreateAsync(workout);
        await unitOfWork.SaveChangesAsync();

        // Act
        var result = await _workoutService.DeleteWorkoutAsync(workout.Id);

        // Assert
        Assert.True(result);
        
        // Verify deletion
        var deletedWorkout = await _workoutService.GetWorkoutByIdAsync(workout.Id);
        Assert.Null(deletedWorkout);
    }

    [Fact]
    public async Task DeleteWorkoutAsync_WithNonExistentId_ReturnsFalse()
    {
        // Act
        var result = await _workoutService.DeleteWorkoutAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetWorkoutsByDifficultyAsync_WithValidDifficulty_ReturnsFilteredWorkouts()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        
        var beginnerWorkout = WorkoutBuilder.Beginner().WithName("Beginner Workout").Build();
        var intermediateWorkout = WorkoutBuilder.Intermediate().WithName("Intermediate Workout").Build();
        var advancedWorkout = WorkoutBuilder.Advanced().WithName("Advanced Workout").Build();

        await unitOfWork.Workouts.CreateAsync(beginnerWorkout);
        await unitOfWork.Workouts.CreateAsync(intermediateWorkout);
        await unitOfWork.Workouts.CreateAsync(advancedWorkout);
        await unitOfWork.SaveChangesAsync();

        // Act
        var beginnerWorkouts = await _workoutService.GetWorkoutsByDifficultyAsync(DifficultyLevel.Beginner);

        // Assert
        Assert.Single(beginnerWorkouts);
        Assert.Equal("Beginner Workout", beginnerWorkouts.First().Name);
        Assert.Equal(DifficultyLevel.Beginner, beginnerWorkouts.First().Difficulty);
    }

    [Fact]
    public async Task GetWorkoutsByDifficultyAsync_WithNonExistentDifficulty_ReturnsEmptyList()
    {
        // Arrange
        await _fixture.SeedCompleteDataset();

        // Act
        var expertWorkouts = await _workoutService.GetWorkoutsByDifficultyAsync(DifficultyLevel.Expert);

        // Assert
        Assert.Empty(expertWorkouts);
    }

    [Fact]
    public async Task GetWorkoutsByDurationRangeAsync_WithValidRange_ReturnsFilteredWorkouts()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        
        var shortWorkout = TestDataBuilder.Workout().WithName("Short").WithDuration(10).Build();
        var mediumWorkout = TestDataBuilder.Workout().WithName("Medium").WithDuration(20).Build();
        var longWorkout = TestDataBuilder.Workout().WithName("Long").WithDuration(40).Build();

        await unitOfWork.Workouts.CreateAsync(shortWorkout);
        await unitOfWork.Workouts.CreateAsync(mediumWorkout);
        await unitOfWork.Workouts.CreateAsync(longWorkout);
        await unitOfWork.SaveChangesAsync();

        // Act
        var mediumRangeWorkouts = await _workoutService.GetWorkoutsByDurationRangeAsync(15, 25);

        // Assert
        Assert.Single(mediumRangeWorkouts);
        Assert.Equal("Medium", mediumRangeWorkouts.First().Name);
        Assert.Equal(20, mediumRangeWorkouts.First().DurationMinutes);
    }

    [Fact]
    public async Task GetWorkoutsByDurationRangeAsync_WithInvalidRange_ThrowsArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.GetWorkoutsByDurationRangeAsync(30, 15)); // max < min
    }

    [Fact]
    public async Task SearchWorkoutsAsync_WithNameQuery_ReturnsMatchingWorkouts()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        
        var pushupWorkout = TestDataBuilder.Workout().WithName("Push-up Challenge").Build();
        var squatWorkout = TestDataBuilder.Workout().WithName("Squat Master").Build();
        var plankWorkout = TestDataBuilder.Workout().WithName("Plank Power").Build();

        await unitOfWork.Workouts.CreateAsync(pushupWorkout);
        await unitOfWork.Workouts.CreateAsync(squatWorkout);
        await unitOfWork.Workouts.CreateAsync(plankWorkout);
        await unitOfWork.SaveChangesAsync();

        // Act
        var searchResults = await _workoutService.SearchWorkoutsAsync("push");

        // Assert
        Assert.Single(searchResults);
        Assert.Equal("Push-up Challenge", searchResults.First().Name);
    }

    [Fact]
    public async Task SearchWorkoutsAsync_WithEmptyQuery_ReturnsAllWorkouts()
    {
        // Arrange
        await _fixture.SeedMinimalData();

        // Act
        var searchResults = await _workoutService.SearchWorkoutsAsync("");

        // Assert
        Assert.NotEmpty(searchResults);
    }

    [Fact]
    public async Task GetWorkoutStatisticsAsync_WithWorkouts_ReturnsCorrectStatistics()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        
        var beginnerWorkout = WorkoutBuilder.Beginner().WithDuration(10).Build();
        var intermediateWorkout = WorkoutBuilder.Intermediate().WithDuration(20).Build();
        var advancedWorkout = WorkoutBuilder.Advanced().WithDuration(30).Build();

        await unitOfWork.Workouts.CreateAsync(beginnerWorkout);
        await unitOfWork.Workouts.CreateAsync(intermediateWorkout);
        await unitOfWork.Workouts.CreateAsync(advancedWorkout);
        await unitOfWork.SaveChangesAsync();

        // Act
        var stats = await _workoutService.GetWorkoutStatisticsAsync();

        // Assert
        Assert.Equal(3, stats.TotalWorkouts);
        Assert.Equal(1, stats.BeginnerCount);
        Assert.Equal(1, stats.IntermediateCount);
        Assert.Equal(1, stats.AdvancedCount);
        Assert.Equal(20.0, stats.AverageDuration); // (10 + 20 + 30) / 3
        Assert.Equal(10, stats.ShortestDuration);
        Assert.Equal(30, stats.LongestDuration);
    }

    [Fact]
    public async Task GetWorkoutStatisticsAsync_WithEmptyDatabase_ReturnsZeroStatistics()
    {
        // Act
        var stats = await _workoutService.GetWorkoutStatisticsAsync();

        // Assert
        Assert.Equal(0, stats.TotalWorkouts);
        Assert.Equal(0, stats.BeginnerCount);
        Assert.Equal(0, stats.IntermediateCount);
        Assert.Equal(0, stats.AdvancedCount);
        Assert.Equal(0.0, stats.AverageDuration);
        Assert.Equal(0, stats.ShortestDuration);
        Assert.Equal(0, stats.LongestDuration);
    }

    [Fact]
    public async Task BulkCreateWorkoutsAsync_WithValidWorkouts_CreatesAllSuccessfully()
    {
        // Arrange
        var workouts = new List<Workout>
        {
            TestDataBuilder.Workout().WithName("Bulk Workout 1").Build(),
            TestDataBuilder.Workout().WithName("Bulk Workout 2").Build(),
            TestDataBuilder.Workout().WithName("Bulk Workout 3").Build()
        };

        // Act
        var results = await _workoutService.BulkCreateWorkoutsAsync(workouts);

        // Assert
        Assert.Equal(3, results.Count);
        Assert.All(results, w => Assert.True(w.Id > 0));
        Assert.Contains(results, w => w.Name == "Bulk Workout 1");
        Assert.Contains(results, w => w.Name == "Bulk Workout 2");
        Assert.Contains(results, w => w.Name == "Bulk Workout 3");
    }

    [Fact]
    public async Task BulkCreateWorkoutsAsync_WithEmptyList_ReturnsEmptyList()
    {
        // Act
        var results = await _workoutService.BulkCreateWorkoutsAsync(new List<Workout>());

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task BulkCreateWorkoutsAsync_WithInvalidWorkout_ThrowsException()
    {
        // Arrange
        var workouts = new List<Workout>
        {
            TestDataBuilder.Workout().WithName("Valid Workout").Build(),
            TestDataBuilder.Workout().WithName("").Build() // Invalid
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _workoutService.BulkCreateWorkoutsAsync(workouts));
    }

    [Fact]
    public async Task GetRecentWorkoutsAsync_WithLimit_ReturnsCorrectCount()
    {
        // Arrange
        var unitOfWork = _fixture.GetUnitOfWork();
        var baseTime = DateTime.UtcNow;
        
        for (int i = 0; i < 10; i++)
        {
            var workout = TestDataBuilder.Workout()
                .WithName($"Workout {i}")
                .CreatedAt(baseTime.AddDays(-i))
                .Build();
            await unitOfWork.Workouts.CreateAsync(workout);
        }
        await unitOfWork.SaveChangesAsync();

        // Act
        var recentWorkouts = await _workoutService.GetRecentWorkoutsAsync(5);

        // Assert
        Assert.Equal(5, recentWorkouts.Count);
        
        // Verify they are ordered by creation date (most recent first)
        for (int i = 0; i < recentWorkouts.Count - 1; i++)
        {
            Assert.True(recentWorkouts[i].CreatedAt >= recentWorkouts[i + 1].CreatedAt);
        }
    }

    [Theory]
    [InlineData(DifficultyLevel.Beginner, 15)]
    [InlineData(DifficultyLevel.Intermediate, 30)]
    [InlineData(DifficultyLevel.Advanced, 45)]
    public async Task CreateWorkoutAsync_WithDifferentDifficulties_SetsCorrectDefaults(DifficultyLevel difficulty, int expectedDuration)
    {
        // Arrange
        var workout = TestDataBuilder.Workout()
            .WithName($"{difficulty} Workout")
            .WithDifficulty(difficulty)
            .WithDuration(expectedDuration)
            .Build();

        // Act
        var result = await _workoutService.CreateWorkoutAsync(workout);

        // Assert
        Assert.Equal(difficulty, result.Difficulty);
        Assert.Equal(expectedDuration, result.DurationMinutes);
    }

    [Fact]
    public async Task ConcurrentOperations_MultipleCreates_AllSucceed()
    {
        // Arrange
        var tasks = new List<Task<Workout>>();
        
        for (int i = 0; i < 10; i++)
        {
            var workout = TestDataBuilder.Workout()
                .WithName($"Concurrent Workout {i}")
                .Build();
            tasks.Add(_workoutService.CreateWorkoutAsync(workout));
        }

        // Act
        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, results.Length);
        Assert.All(results, w => Assert.True(w.Id > 0));
        
        // Verify all workouts were created
        var allWorkouts = await _workoutService.GetAllWorkoutsAsync();
        Assert.True(allWorkouts.Count >= 10);
    }

    public void Dispose()
    {
        _fixture?.Dispose();
    }
}
using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;
using Xunit;

namespace WorkoutGamifier.Tests.Services;

public class WorkoutPoolServiceCustomWorkoutTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WorkoutPoolService _workoutPoolService;
    private readonly WorkoutService _workoutService;

    public WorkoutPoolServiceCustomWorkoutTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _workoutPoolService = new WorkoutPoolService(_unitOfWork);
        _workoutService = new WorkoutService(_unitOfWork);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AddCustomWorkoutToPool_ShouldWorkCorrectly()
    {
        // Arrange - Create a custom workout (not preloaded)
        var customWorkout = new Workout
        {
            Name = "Custom Push-ups",
            Description = "User-created push-up routine",
            Instructions = "Do 20 push-ups with proper form",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false, // This is a custom workout
            IsHidden = false
        };

        var pool = new WorkoutPool
        {
            Name = "Custom Workout Pool",
            Description = "Pool for testing custom workouts"
        };

        // Act
        var createdWorkout = await _workoutService.CreateWorkoutAsync(customWorkout);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);

        // Assert
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPool);
        Assert.Equal(createdWorkout.Id, workoutsInPool[0].Id);
        Assert.Equal("Custom Push-ups", workoutsInPool[0].Name);
        Assert.False(workoutsInPool[0].IsPreloaded);
    }

    [Fact]
    public async Task GetWorkoutsInPool_ShouldHandleMixedPreloadedAndCustomWorkouts()
    {
        // Arrange
        var preloadedWorkout = new Workout
        {
            Name = "Preloaded Workout",
            Description = "System workout",
            Instructions = "System instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = true,
            IsHidden = false
        };

        var customWorkout = new Workout
        {
            Name = "Custom Workout",
            Description = "User workout",
            Instructions = "User instructions",
            DurationMinutes = 45,
            Difficulty = DifficultyLevel.Advanced,
            IsPreloaded = false,
            IsHidden = false
        };

        var pool = new WorkoutPool
        {
            Name = "Mixed Pool",
            Description = "Pool with both types"
        };

        // Act
        var createdPreloaded = await _workoutService.CreateWorkoutAsync(preloadedWorkout);
        var createdCustom = await _workoutService.CreateWorkoutAsync(customWorkout);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdPreloaded.Id);
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdCustom.Id);

        // Assert
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Equal(2, workoutsInPool.Count);
        
        var preloaded = workoutsInPool.First(w => w.IsPreloaded);
        var custom = workoutsInPool.First(w => !w.IsPreloaded);
        
        Assert.Equal("Preloaded Workout", preloaded.Name);
        Assert.Equal("Custom Workout", custom.Name);
    }

    [Fact]
    public async Task GetRandomWorkoutFromPool_ShouldWorkWithCustomWorkouts()
    {
        // Arrange
        var customWorkout1 = new Workout
        {
            Name = "Custom Workout 1",
            Description = "First custom workout",
            Instructions = "Instructions 1",
            DurationMinutes = 20,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = false,
            IsHidden = false
        };

        var customWorkout2 = new Workout
        {
            Name = "Custom Workout 2",
            Description = "Second custom workout",
            Instructions = "Instructions 2",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false
        };

        var pool = new WorkoutPool
        {
            Name = "Custom Only Pool",
            Description = "Pool with only custom workouts"
        };

        // Act
        var created1 = await _workoutService.CreateWorkoutAsync(customWorkout1);
        var created2 = await _workoutService.CreateWorkoutAsync(customWorkout2);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, created1.Id);
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, created2.Id);

        // Assert
        var randomWorkout = await _workoutPoolService.GetRandomWorkoutFromPoolAsync(createdPool.Id);
        Assert.NotNull(randomWorkout);
        Assert.True(randomWorkout.Id == created1.Id || randomWorkout.Id == created2.Id);
        Assert.False(randomWorkout.IsPreloaded);
    }

    [Fact]
    public async Task RemoveCustomWorkoutFromPool_ShouldNotDeleteWorkout()
    {
        // Arrange
        var customWorkout = new Workout
        {
            Name = "Custom Workout to Remove",
            Description = "This workout should remain after removal from pool",
            Instructions = "Instructions",
            DurationMinutes = 25,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Pool for removal test"
        };

        // Act
        var createdWorkout = await _workoutService.CreateWorkoutAsync(customWorkout);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);
        
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);
        
        // Verify it was added
        var workoutsInPoolBefore = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPoolBefore);

        // Remove from pool
        await _workoutPoolService.RemoveWorkoutFromPoolAsync(createdPool.Id, createdWorkout.Id);

        // Assert
        var workoutsInPoolAfter = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Empty(workoutsInPoolAfter);

        // Verify the workout still exists
        var workoutStillExists = await _workoutService.GetWorkoutByIdAsync(createdWorkout.Id);
        Assert.NotNull(workoutStillExists);
        Assert.Equal("Custom Workout to Remove", workoutStillExists.Name);
    }

    [Fact]
    public async Task GetWorkoutsInPool_ShouldExcludeHiddenCustomWorkouts()
    {
        // Arrange
        var visibleCustomWorkout = new Workout
        {
            Name = "Visible Custom Workout",
            Description = "This should appear",
            Instructions = "Instructions",
            DurationMinutes = 20,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = false,
            IsHidden = false
        };

        var hiddenCustomWorkout = new Workout
        {
            Name = "Hidden Custom Workout",
            Description = "This should not appear",
            Instructions = "Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Advanced,
            IsPreloaded = false,
            IsHidden = true
        };

        var pool = new WorkoutPool
        {
            Name = "Visibility Test Pool",
            Description = "Testing visibility filtering"
        };

        // Act
        var createdVisible = await _workoutService.CreateWorkoutAsync(visibleCustomWorkout);
        var createdHidden = await _workoutService.CreateWorkoutAsync(hiddenCustomWorkout);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdVisible.Id);
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdHidden.Id);

        // Assert
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPool);
        Assert.Equal("Visible Custom Workout", workoutsInPool[0].Name);
        Assert.False(workoutsInPool[0].IsHidden);
        Assert.False(workoutsInPool[0].IsPreloaded);
    }

    [Fact]
    public async Task AddWorkoutToPool_ShouldValidateWorkoutExists()
    {
        // Arrange
        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Pool for validation test"
        };

        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);
        const int nonExistentWorkoutId = 99999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, nonExistentWorkoutId));
        
        Assert.Contains($"Workout with ID {nonExistentWorkoutId} not found", exception.Message);
    }

    [Fact]
    public async Task AddWorkoutToPool_ShouldValidatePoolExists()
    {
        // Arrange
        var customWorkout = new Workout
        {
            Name = "Test Workout",
            Description = "Test",
            Instructions = "Test",
            DurationMinutes = 20,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = false,
            IsHidden = false
        };

        var createdWorkout = await _workoutService.CreateWorkoutAsync(customWorkout);
        const int nonExistentPoolId = 99999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.AddWorkoutToPoolAsync(nonExistentPoolId, createdWorkout.Id));
        
        Assert.Contains($"Workout pool with ID {nonExistentPoolId} not found", exception.Message);
    }

    [Fact]
    public async Task GetRandomWorkoutFromPool_ShouldHandleEmptyPoolGracefully()
    {
        // Arrange
        var emptyPool = new WorkoutPool
        {
            Name = "Empty Pool",
            Description = "This pool has no workouts"
        };

        var createdPool = await _workoutPoolService.CreatePoolAsync(emptyPool);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.GetRandomWorkoutFromPoolAsync(createdPool.Id));
        
        Assert.Contains("Cannot get random workout from empty pool", exception.Message);
    }

    [Fact]
    public async Task GetRandomWorkoutFromPool_ShouldOnlyReturnVisibleWorkouts()
    {
        // Arrange
        var hiddenWorkout = new Workout
        {
            Name = "Hidden Workout",
            Description = "Should not be returned",
            Instructions = "Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = true
        };

        var pool = new WorkoutPool
        {
            Name = "Hidden Only Pool",
            Description = "Pool with only hidden workouts"
        };

        // Act
        var createdWorkout = await _workoutService.CreateWorkoutAsync(hiddenWorkout);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);

        // Assert - Should throw exception because no visible workouts available
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.GetRandomWorkoutFromPoolAsync(createdPool.Id));
        
        Assert.Contains("Cannot get random workout from empty pool", exception.Message);
    }
}
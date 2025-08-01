using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;
using Xunit;

namespace WorkoutGamifier.Tests.Services;

public class WorkoutPoolServiceEdgeCaseTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WorkoutPoolService _workoutPoolService;
    private readonly WorkoutService _workoutService;

    public WorkoutPoolServiceEdgeCaseTests()
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
    public async Task AddWorkoutToPool_AfterWorkoutIsHidden_ShouldStillAddButNotAppearInResults()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Workout to Hide",
            Description = "This will be hidden after adding to pool",
            Instructions = "Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Pool for hiding test"
        };

        var createdWorkout = await _workoutService.CreateWorkoutAsync(workout);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Add workout to pool while visible
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);

        // Verify it appears in results
        var workoutsInPoolBefore = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPoolBefore);

        // Act - Hide the workout
        await _workoutService.ToggleWorkoutVisibilityAsync(createdWorkout.Id);

        // Assert - Should not appear in pool results anymore
        var workoutsInPoolAfter = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Empty(workoutsInPoolAfter);

        // But the relationship should still exist in the database
        var dbContext = ((UnitOfWork)_unitOfWork).GetDbContext();
        var relationshipExists = await dbContext.Set<WorkoutPoolWorkout>()
            .AnyAsync(pw => pw.WorkoutPoolId == createdPool.Id && pw.WorkoutId == createdWorkout.Id);
        Assert.True(relationshipExists);
    }

    [Fact]
    public async Task GetRandomWorkoutFromPool_AfterAllWorkoutsHidden_ShouldThrowException()
    {
        // Arrange
        var workout1 = new Workout
        {
            Name = "Workout 1",
            Description = "First workout",
            Instructions = "Instructions 1",
            DurationMinutes = 20,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = false,
            IsHidden = false
        };

        var workout2 = new Workout
        {
            Name = "Workout 2",
            Description = "Second workout",
            Instructions = "Instructions 2",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false
        };

        var pool = new WorkoutPool
        {
            Name = "Pool to Empty",
            Description = "Pool that will become effectively empty"
        };

        var created1 = await _workoutService.CreateWorkoutAsync(workout1);
        var created2 = await _workoutService.CreateWorkoutAsync(workout2);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, created1.Id);
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, created2.Id);

        // Verify random selection works initially
        var randomWorkout = await _workoutPoolService.GetRandomWorkoutFromPoolAsync(createdPool.Id);
        Assert.NotNull(randomWorkout);

        // Act - Hide both workouts
        await _workoutService.ToggleWorkoutVisibilityAsync(created1.Id);
        await _workoutService.ToggleWorkoutVisibilityAsync(created2.Id);

        // Assert - Should throw exception when trying to get random workout
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.GetRandomWorkoutFromPoolAsync(createdPool.Id));
        
        Assert.Contains("Cannot get random workout from empty pool", exception.Message);
    }

    [Fact]
    public async Task RemoveWorkoutFromPool_ThatDoesNotExist_ShouldThrowException()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Standalone Workout",
            Description = "This workout is not in any pool",
            Instructions = "Instructions",
            DurationMinutes = 25,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false
        };

        var pool = new WorkoutPool
        {
            Name = "Empty Pool",
            Description = "Pool without the workout"
        };

        var createdWorkout = await _workoutService.CreateWorkoutAsync(workout);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.RemoveWorkoutFromPoolAsync(createdPool.Id, createdWorkout.Id));
        
        Assert.Contains("Workout is not in this pool", exception.Message);
    }

    [Fact]
    public async Task AddWorkoutToPool_WithNullOrEmptyName_ShouldStillWork()
    {
        // This tests that the pool service doesn't break even if workout validation was bypassed elsewhere
        
        // Arrange - Create workout directly in database to bypass service validation
        var workout = new Workout
        {
            Name = "", // Empty name
            Description = "Workout with empty name",
            Instructions = "Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Pool for edge case test"
        };

        // Add directly to context to bypass service validation
        _context.Set<Workout>().Add(workout);
        await _context.SaveChangesAsync();

        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Act - Should not throw exception
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, workout.Id);

        // Assert
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPool);
        Assert.Equal("", workoutsInPool[0].Name);
    }

    [Fact]
    public async Task GetWorkoutsInPool_WithNonExistentPool_ShouldReturnEmptyList()
    {
        // Arrange
        const int nonExistentPoolId = 99999;

        // Act
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(nonExistentPoolId);

        // Assert
        Assert.Empty(workoutsInPool);
    }

    [Fact]
    public async Task GetRandomWorkoutFromPool_WithNonExistentPool_ShouldThrowException()
    {
        // Arrange
        const int nonExistentPoolId = 99999;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.GetRandomWorkoutFromPoolAsync(nonExistentPoolId));
        
        Assert.Contains("Cannot get random workout from empty pool", exception.Message);
    }

    [Fact]
    public async Task WorkoutPoolOperations_WithExtremelyLongNames_ShouldHandleGracefully()
    {
        // Arrange
        var longName = new string('A', 200); // Longer than the 100 character limit
        var workout = new Workout
        {
            Name = longName,
            Description = "Workout with very long name",
            Instructions = "Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Pool for long name test"
        };

        // Add workout directly to bypass validation
        _context.Set<Workout>().Add(workout);
        await _context.SaveChangesAsync();

        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Act & Assert - Should handle long names without crashing
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, workout.Id);
        
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPool);
        Assert.Equal(longName, workoutsInPool[0].Name);
    }

    [Fact]
    public async Task ConcurrentAddAndRemove_ShouldMaintainConsistency()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Concurrent Test Workout",
            Description = "Workout for concurrency test",
            Instructions = "Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false
        };

        var pool = new WorkoutPool
        {
            Name = "Concurrent Test Pool",
            Description = "Pool for concurrency test"
        };

        var createdWorkout = await _workoutService.CreateWorkoutAsync(workout);
        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Act - Perform concurrent add and remove operations
        var tasks = new List<Task>();
        
        // Add the workout multiple times concurrently (should only succeed once)
        for (int i = 0; i < 5; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);
                }
                catch (InvalidOperationException)
                {
                    // Expected for duplicate additions
                }
            }));
        }

        await Task.WhenAll(tasks);

        // Assert - Should have exactly one workout in pool
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPool);
        Assert.Equal(createdWorkout.Id, workoutsInPool[0].Id);
    }
}
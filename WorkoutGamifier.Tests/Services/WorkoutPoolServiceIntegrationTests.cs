using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;
using Xunit;

namespace WorkoutGamifier.Tests.Services;

public class WorkoutPoolServiceIntegrationTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WorkoutPoolService _workoutPoolService;

    public WorkoutPoolServiceIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _workoutPoolService = new WorkoutPoolService(_unitOfWork);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task AddWorkoutToPool_ShouldAddWorkoutSuccessfully()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            Description = "Test Description",
            Instructions = "Test Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Test Pool Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdWorkout = await _unitOfWork.Workouts.CreateAsync(workout);
        var createdPool = await _unitOfWork.WorkoutPools.CreateAsync(pool);
        await _unitOfWork.SaveChangesAsync();

        // Act
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);

        // Assert
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPool);
        Assert.Equal(createdWorkout.Id, workoutsInPool[0].Id);
        Assert.Equal("Test Workout", workoutsInPool[0].Name);
    }

    [Fact]
    public async Task GetWorkoutsInPool_ShouldReturnOnlyVisibleWorkouts()
    {
        // Arrange
        var visibleWorkout = new Workout
        {
            Name = "Visible Workout",
            Description = "Visible Description",
            Instructions = "Visible Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsHidden = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var hiddenWorkout = new Workout
        {
            Name = "Hidden Workout",
            Description = "Hidden Description",
            Instructions = "Hidden Instructions",
            DurationMinutes = 45,
            Difficulty = DifficultyLevel.Advanced,
            IsHidden = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Test Pool Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdVisibleWorkout = await _unitOfWork.Workouts.CreateAsync(visibleWorkout);
        var createdHiddenWorkout = await _unitOfWork.Workouts.CreateAsync(hiddenWorkout);
        var createdPool = await _unitOfWork.WorkoutPools.CreateAsync(pool);
        await _unitOfWork.SaveChangesAsync();

        // Add both workouts to pool
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdVisibleWorkout.Id);
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdHiddenWorkout.Id);

        // Act
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);

        // Assert - Should only return visible workout
        Assert.Single(workoutsInPool);
        Assert.Equal("Visible Workout", workoutsInPool[0].Name);
        Assert.False(workoutsInPool[0].IsHidden);
    }

    [Fact]
    public async Task RemoveWorkoutFromPool_ShouldRemoveWorkoutSuccessfully()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            Description = "Test Description",
            Instructions = "Test Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Test Pool Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdWorkout = await _unitOfWork.Workouts.CreateAsync(workout);
        var createdPool = await _unitOfWork.WorkoutPools.CreateAsync(pool);
        await _unitOfWork.SaveChangesAsync();

        // Add workout to pool first
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);
        
        // Verify it was added
        var workoutsInPoolBefore = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPoolBefore);

        // Act
        await _workoutPoolService.RemoveWorkoutFromPoolAsync(createdPool.Id, createdWorkout.Id);

        // Assert
        var workoutsInPoolAfter = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Empty(workoutsInPoolAfter);
    }

    [Fact]
    public async Task AddWorkoutToPool_ShouldThrowException_WhenWorkoutAlreadyInPool()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            Description = "Test Description",
            Instructions = "Test Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Test Pool Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdWorkout = await _unitOfWork.Workouts.CreateAsync(workout);
        var createdPool = await _unitOfWork.WorkoutPools.CreateAsync(pool);
        await _unitOfWork.SaveChangesAsync();

        // Add workout to pool first time
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id));
        
        Assert.Contains("already in this pool", exception.Message);
    }

    [Fact]
    public async Task GetRandomWorkoutFromPool_ShouldReturnWorkoutFromPool()
    {
        // Arrange
        var workout1 = new Workout
        {
            Name = "Workout 1",
            Description = "Description 1",
            Instructions = "Instructions 1",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Beginner,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var workout2 = new Workout
        {
            Name = "Workout 2",
            Description = "Description 2",
            Instructions = "Instructions 2",
            DurationMinutes = 45,
            Difficulty = DifficultyLevel.Intermediate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Test Pool Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdWorkout1 = await _unitOfWork.Workouts.CreateAsync(workout1);
        var createdWorkout2 = await _unitOfWork.Workouts.CreateAsync(workout2);
        var createdPool = await _unitOfWork.WorkoutPools.CreateAsync(pool);
        await _unitOfWork.SaveChangesAsync();

        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout1.Id);
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout2.Id);

        // Act
        var randomWorkout = await _workoutPoolService.GetRandomWorkoutFromPoolAsync(createdPool.Id);

        // Assert
        Assert.NotNull(randomWorkout);
        Assert.True(randomWorkout.Id == createdWorkout1.Id || randomWorkout.Id == createdWorkout2.Id);
    }

    [Fact]
    public async Task GetRandomWorkoutFromPool_ShouldThrowException_WhenPoolIsEmpty()
    {
        // Arrange
        var pool = new WorkoutPool
        {
            Name = "Empty Pool",
            Description = "Empty Pool Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdPool = await _unitOfWork.WorkoutPools.CreateAsync(pool);
        await _unitOfWork.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _workoutPoolService.GetRandomWorkoutFromPoolAsync(createdPool.Id));
        
        Assert.Contains("Cannot get random workout from empty pool", exception.Message);
    }

    [Fact]
    public async Task DeletePool_ShouldRemoveAllRelationships_AndDeletePool()
    {
        // Arrange
        var workout = new Workout
        {
            Name = "Test Workout",
            Description = "Test Description",
            Instructions = "Test Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var pool = new WorkoutPool
        {
            Name = "Test Pool",
            Description = "Test Pool Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdWorkout = await _unitOfWork.Workouts.CreateAsync(workout);
        var createdPool = await _unitOfWork.WorkoutPools.CreateAsync(pool);
        await _unitOfWork.SaveChangesAsync();

        // Add workout to pool
        await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, createdWorkout.Id);

        // Verify relationship exists
        var workoutsInPoolBefore = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Single(workoutsInPoolBefore);

        // Act
        await _workoutPoolService.DeletePoolAsync(createdPool.Id);

        // Assert
        var deletedPool = await _unitOfWork.WorkoutPools.GetByIdAsync(createdPool.Id);
        Assert.Null(deletedPool);

        // Verify workout still exists (only relationship should be deleted)
        var existingWorkout = await _unitOfWork.Workouts.GetByIdAsync(createdWorkout.Id);
        Assert.NotNull(existingWorkout);
    }
}
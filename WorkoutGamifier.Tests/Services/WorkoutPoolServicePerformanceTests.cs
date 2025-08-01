using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;
using Xunit;

namespace WorkoutGamifier.Tests.Services;

public class WorkoutPoolServicePerformanceTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly WorkoutPoolService _workoutPoolService;
    private readonly WorkoutService _workoutService;

    public WorkoutPoolServicePerformanceTests()
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
    public async Task GetWorkoutsInPool_ShouldHandleLargeDatasetEfficiently()
    {
        // Arrange - Create a large dataset
        var pool = new WorkoutPool
        {
            Name = "Large Pool",
            Description = "Pool with many workouts"
        };

        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Create 100 workouts (mix of preloaded and custom, visible and hidden)
        var workouts = new List<Workout>();
        for (int i = 0; i < 100; i++)
        {
            var workout = new Workout
            {
                Name = $"Workout {i}",
                Description = $"Description {i}",
                Instructions = $"Instructions {i}",
                DurationMinutes = 20 + (i % 60),
                Difficulty = (DifficultyLevel)((i % 4) + 1),
                IsPreloaded = i % 2 == 0, // Half preloaded, half custom
                IsHidden = i % 10 == 0 // 10% hidden
            };
            workouts.Add(await _workoutService.CreateWorkoutAsync(workout));
        }

        // Add first 50 workouts to the pool
        for (int i = 0; i < 50; i++)
        {
            await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, workouts[i].Id);
        }

        // Act
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);

        // Assert
        // Should return 45 workouts (50 added - 5 hidden ones)
        Assert.Equal(45, workoutsInPool.Count);
        
        // Verify no hidden workouts are returned
        Assert.All(workoutsInPool, w => Assert.False(w.IsHidden));
        
        // Verify mix of preloaded and custom workouts
        var preloadedCount = workoutsInPool.Count(w => w.IsPreloaded);
        var customCount = workoutsInPool.Count(w => !w.IsPreloaded);
        Assert.True(preloadedCount > 0);
        Assert.True(customCount > 0);
    }

    [Fact]
    public async Task GetRandomWorkoutFromPool_ShouldWorkWithLargePool()
    {
        // Arrange
        var pool = new WorkoutPool
        {
            Name = "Random Test Pool",
            Description = "Pool for random selection testing"
        };

        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Create 20 visible workouts
        var workoutIds = new List<int>();
        for (int i = 0; i < 20; i++)
        {
            var workout = new Workout
            {
                Name = $"Random Workout {i}",
                Description = $"Description {i}",
                Instructions = $"Instructions {i}",
                DurationMinutes = 30,
                Difficulty = DifficultyLevel.Intermediate,
                IsPreloaded = false,
                IsHidden = false
            };
            var created = await _workoutService.CreateWorkoutAsync(workout);
            workoutIds.Add(created.Id);
            await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, created.Id);
        }

        // Act - Get multiple random workouts to test distribution
        var selectedWorkouts = new HashSet<int>();
        for (int i = 0; i < 10; i++)
        {
            var randomWorkout = await _workoutPoolService.GetRandomWorkoutFromPoolAsync(createdPool.Id);
            Assert.NotNull(randomWorkout);
            selectedWorkouts.Add(randomWorkout.Id);
        }

        // Assert - Should have selected from the available workouts
        Assert.All(selectedWorkouts, id => Assert.Contains(id, workoutIds));
        // With 10 selections from 20 workouts, we should get some variety (not always the same workout)
        Assert.True(selectedWorkouts.Count > 1, "Random selection should provide variety");
    }

    [Fact]
    public async Task ConcurrentOperations_ShouldNotCauseDataCorruption()
    {
        // Arrange
        var pool = new WorkoutPool
        {
            Name = "Concurrent Test Pool",
            Description = "Pool for concurrency testing"
        };

        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Create workouts for concurrent operations
        var workouts = new List<Workout>();
        for (int i = 0; i < 10; i++)
        {
            var workout = new Workout
            {
                Name = $"Concurrent Workout {i}",
                Description = $"Description {i}",
                Instructions = $"Instructions {i}",
                DurationMinutes = 25,
                Difficulty = DifficultyLevel.Beginner,
                IsPreloaded = false,
                IsHidden = false
            };
            workouts.Add(await _workoutService.CreateWorkoutAsync(workout));
        }

        // Act - Perform concurrent add operations
        var addTasks = workouts.Select(w => 
            _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, w.Id)).ToArray();
        
        await Task.WhenAll(addTasks);

        // Assert
        var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Equal(10, workoutsInPool.Count);
        
        // Verify all workouts were added correctly
        var addedWorkoutIds = workoutsInPool.Select(w => w.Id).OrderBy(id => id).ToList();
        var expectedWorkoutIds = workouts.Select(w => w.Id).OrderBy(id => id).ToList();
        Assert.Equal(expectedWorkoutIds, addedWorkoutIds);
    }

    [Fact]
    public async Task MultiplePoolOperations_ShouldMaintainDataIntegrity()
    {
        // Arrange - Create multiple pools and workouts
        var pools = new List<WorkoutPool>();
        for (int i = 0; i < 3; i++)
        {
            var pool = new WorkoutPool
            {
                Name = $"Pool {i}",
                Description = $"Description {i}"
            };
            pools.Add(await _workoutPoolService.CreatePoolAsync(pool));
        }

        var workouts = new List<Workout>();
        for (int i = 0; i < 15; i++)
        {
            var workout = new Workout
            {
                Name = $"Multi-Pool Workout {i}",
                Description = $"Description {i}",
                Instructions = $"Instructions {i}",
                DurationMinutes = 30,
                Difficulty = DifficultyLevel.Intermediate,
                IsPreloaded = i % 3 == 0, // Mix of preloaded and custom
                IsHidden = false
            };
            workouts.Add(await _workoutService.CreateWorkoutAsync(workout));
        }

        // Act - Add workouts to different pools
        // Pool 0: workouts 0-4
        // Pool 1: workouts 5-9  
        // Pool 2: workouts 10-14
        for (int i = 0; i < 15; i++)
        {
            var poolIndex = i / 5;
            await _workoutPoolService.AddWorkoutToPoolAsync(pools[poolIndex].Id, workouts[i].Id);
        }

        // Assert - Verify each pool has correct workouts
        for (int poolIndex = 0; poolIndex < 3; poolIndex++)
        {
            var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(pools[poolIndex].Id);
            Assert.Equal(5, workoutsInPool.Count);
            
            // Verify correct workouts are in each pool
            var expectedWorkoutNames = Enumerable.Range(poolIndex * 5, 5)
                .Select(i => $"Multi-Pool Workout {i}")
                .OrderBy(name => name)
                .ToList();
            
            var actualWorkoutNames = workoutsInPool
                .Select(w => w.Name)
                .OrderBy(name => name)
                .ToList();
            
            Assert.Equal(expectedWorkoutNames, actualWorkoutNames);
        }
    }

    [Fact]
    public async Task DeletePoolWithManyWorkouts_ShouldCleanupCorrectly()
    {
        // Arrange
        var pool = new WorkoutPool
        {
            Name = "Pool to Delete",
            Description = "This pool will be deleted"
        };

        var createdPool = await _workoutPoolService.CreatePoolAsync(pool);

        // Add many workouts to the pool
        var workouts = new List<Workout>();
        for (int i = 0; i < 25; i++)
        {
            var workout = new Workout
            {
                Name = $"Workout to Keep {i}",
                Description = $"This workout should remain after pool deletion",
                Instructions = $"Instructions {i}",
                DurationMinutes = 20,
                Difficulty = DifficultyLevel.Beginner,
                IsPreloaded = false,
                IsHidden = false
            };
            var created = await _workoutService.CreateWorkoutAsync(workout);
            workouts.Add(created);
            await _workoutPoolService.AddWorkoutToPoolAsync(createdPool.Id, created.Id);
        }

        // Verify workouts are in pool
        var workoutsInPoolBefore = await _workoutPoolService.GetWorkoutsInPoolAsync(createdPool.Id);
        Assert.Equal(25, workoutsInPoolBefore.Count);

        // Act
        await _workoutPoolService.DeletePoolAsync(createdPool.Id);

        // Assert
        // Pool should be deleted
        var deletedPool = await _workoutPoolService.GetPoolByIdAsync(createdPool.Id);
        Assert.Null(deletedPool);

        // All workouts should still exist
        foreach (var workout in workouts)
        {
            var existingWorkout = await _workoutService.GetWorkoutByIdAsync(workout.Id);
            Assert.NotNull(existingWorkout);
            Assert.Equal(workout.Name, existingWorkout.Name);
        }

        // No orphaned relationships should exist
        var dbContext = ((UnitOfWork)_unitOfWork).GetDbContext();
        var orphanedRelationships = await dbContext.Set<WorkoutPoolWorkout>()
            .Where(pw => pw.WorkoutPoolId == createdPool.Id)
            .CountAsync();
        Assert.Equal(0, orphanedRelationships);
    }
}
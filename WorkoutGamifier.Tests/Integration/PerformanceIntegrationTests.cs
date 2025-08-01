using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Tests.Integration;

public class PerformanceIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbContext _context;

    public PerformanceIntegrationTests()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<WorkoutSelector>();
        services.AddScoped<PointCalculator>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TestDbContext>();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Performance_CreateLargeNumberOfWorkouts_CompletesInReasonableTime()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        const int workoutCount = 1000;
        var stopwatch = Stopwatch.StartNew();

        // Act
        var tasks = new List<Task<Workout>>();
        for (int i = 0; i < workoutCount; i++)
        {
            var workout = new Workout
            {
                Name = $"Performance Workout {i}",
                Description = $"Description for workout {i}",
                Instructions = $"Instructions for workout {i}",
                DurationMinutes = 10 + (i % 50),
                Difficulty = (DifficultyLevel)((i % 4) + 1)
            };
            
            // Create workouts in batches to avoid overwhelming the system
            if (i % 100 == 0 && tasks.Any())
            {
                await Task.WhenAll(tasks);
                tasks.Clear();
            }
            
            tasks.Add(workoutService.CreateWorkoutAsync(workout));
        }
        
        if (tasks.Any())
        {
            await Task.WhenAll(tasks);
        }

        stopwatch.Stop();

        // Assert
        var allWorkouts = await workoutService.GetAllWorkoutsAsync();
        Assert.Equal(workoutCount, allWorkouts.Count);
        
        // Performance assertion - should complete within reasonable time (adjust as needed)
        Assert.True(stopwatch.ElapsedMilliseconds < 30000, 
            $"Creating {workoutCount} workouts took {stopwatch.ElapsedMilliseconds}ms, which exceeds the 30-second limit");
    }

    [Fact]
    public async Task Performance_ComplexSessionWorkflow_CompletesInReasonableTime()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create test data
        const int workoutCount = 100;
        const int actionCount = 50;
        
        var stopwatch = Stopwatch.StartNew();

        // Create workouts
        var workouts = new List<Workout>();
        for (int i = 0; i < workoutCount; i++)
        {
            var workout = await workoutService.CreateWorkoutAsync(new Workout
            {
                Name = $"Perf Workout {i}",
                DurationMinutes = 15,
                Difficulty = DifficultyLevel.Beginner
            });
            workouts.Add(workout);
        }

        // Create workout pool with all workouts
        var workoutPool = await unitOfWork.WorkoutPools.CreateAsync(new WorkoutPool
        {
            Name = "Large Performance Pool",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        foreach (var workout in workouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
            {
                WorkoutPoolId = workoutPool.Id,
                WorkoutId = workout.Id
            });
        }

        // Create actions
        var actions = new List<WorkoutGamifier.Core.Models.Action>();
        for (int i = 0; i < actionCount; i++)
        {
            var action = await unitOfWork.Actions.CreateAsync(new WorkoutGamifier.Core.Models.Action
            {
                Description = $"Performance Action {i}",
                PointValue = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            actions.Add(action);
        }
        await unitOfWork.SaveChangesAsync();

        // Act - Execute complex workflow
        var session = await sessionService.StartSessionAsync("Performance Test Session", workoutPool.Id);

        // Complete many actions
        for (int i = 0; i < Math.Min(actionCount, 20); i++)
        {
            await sessionService.CompleteActionAsync(session.Id, actions[i].Id);
        }

        // Spend points on multiple workouts
        for (int i = 0; i < 10; i++)
        {
            await sessionService.SpendPointsForWorkoutAsync(session.Id, 5);
        }

        await sessionService.EndSessionAsync(session.Id);

        stopwatch.Stop();

        // Assert
        var finalSession = await sessionService.GetSessionByIdAsync(session.Id);
        Assert.NotNull(finalSession);
        Assert.Equal(SessionStatus.Completed, finalSession.Status);
        Assert.Equal(100, finalSession.PointsEarned); // 20 actions * 5 points
        Assert.Equal(50, finalSession.PointsSpent);   // 10 workouts * 5 points
        
        // Performance assertion
        Assert.True(stopwatch.ElapsedMilliseconds < 10000, 
            $"Complex session workflow took {stopwatch.ElapsedMilliseconds}ms, which exceeds the 10-second limit");
    }

    [Fact]
    public async Task Performance_RandomWorkoutSelection_ScalesWell()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var workoutSelector = _serviceProvider.GetRequiredService<WorkoutSelector>();

        // Create large number of workouts
        const int workoutCount = 10000;
        var workouts = new List<Workout>();
        
        for (int i = 0; i < workoutCount; i++)
        {
            var workout = await workoutService.CreateWorkoutAsync(new Workout
            {
                Name = $"Selection Test Workout {i}",
                DurationMinutes = 10 + (i % 60),
                Difficulty = (DifficultyLevel)((i % 4) + 1),
                IsHidden = i % 10 == 0 // Hide 10% of workouts
            });
            workouts.Add(workout);
        }

        var stopwatch = Stopwatch.StartNew();

        // Act - Perform many random selections
        const int selectionCount = 1000;
        var selectedWorkouts = new List<Workout?>();
        
        for (int i = 0; i < selectionCount; i++)
        {
            var selected = workoutSelector.SelectRandomWorkout(workouts);
            selectedWorkouts.Add(selected);
        }

        stopwatch.Stop();

        // Assert
        Assert.Equal(selectionCount, selectedWorkouts.Count);
        Assert.All(selectedWorkouts, w => Assert.NotNull(w));
        Assert.All(selectedWorkouts, w => Assert.False(w!.IsHidden));
        
        // Verify randomness - should have selected different workouts
        var uniqueWorkouts = selectedWorkouts.Select(w => w!.Id).Distinct().Count();
        Assert.True(uniqueWorkouts > selectionCount / 10, 
            "Random selection should produce variety in selected workouts");
        
        // Performance assertion
        Assert.True(stopwatch.ElapsedMilliseconds < 1000, 
            $"Random workout selection took {stopwatch.ElapsedMilliseconds}ms, which exceeds the 1-second limit");
    }

    [Fact]
    public async Task Performance_DatabaseQueries_OptimizedForLargeDatasets()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create large dataset
        const int dataSize = 5000;
        
        // Create workouts
        for (int i = 0; i < dataSize; i++)
        {
            await workoutService.CreateWorkoutAsync(new Workout
            {
                Name = $"Query Test Workout {i}",
                DurationMinutes = 15,
                Difficulty = DifficultyLevel.Beginner
            });
        }

        // Create actions
        for (int i = 0; i < dataSize; i++)
        {
            await unitOfWork.Actions.CreateAsync(new WorkoutGamifier.Core.Models.Action
            {
                Description = $"Query Test Action {i}",
                PointValue = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        await unitOfWork.SaveChangesAsync();

        var stopwatch = Stopwatch.StartNew();

        // Act - Perform various queries
        var allWorkouts = await workoutService.GetAllWorkoutsAsync();
        var allActions = await unitOfWork.Actions.GetAllAsync();
        
        // Simulate filtering operations
        var beginnerWorkouts = allWorkouts.Where(w => w.Difficulty == DifficultyLevel.Beginner).ToList();
        var shortWorkouts = allWorkouts.Where(w => w.DurationMinutes <= 20).ToList();
        var highValueActions = allActions.Where(a => a.PointValue >= 5).ToList();

        stopwatch.Stop();

        // Assert
        Assert.Equal(dataSize, allWorkouts.Count);
        Assert.Equal(dataSize, allActions.Count);
        Assert.Equal(dataSize, beginnerWorkouts.Count); // All created as beginner
        Assert.Equal(dataSize, shortWorkouts.Count);    // All created with 15 minutes
        Assert.Equal(dataSize, highValueActions.Count); // All created with 5 points
        
        // Performance assertion
        Assert.True(stopwatch.ElapsedMilliseconds < 5000, 
            $"Database queries took {stopwatch.ElapsedMilliseconds}ms, which exceeds the 5-second limit");
    }

    [Fact]
    public async Task Performance_MemoryUsage_StaysWithinReasonableLimits()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var initialMemory = GC.GetTotalMemory(true);

        // Act - Create and process data
        const int iterations = 1000;
        for (int i = 0; i < iterations; i++)
        {
            var workout = await workoutService.CreateWorkoutAsync(new Workout
            {
                Name = $"Memory Test Workout {i}",
                Description = new string('A', 100), // 100 character description
                Instructions = new string('B', 500), // 500 character instructions
                DurationMinutes = 15,
                Difficulty = DifficultyLevel.Beginner
            });

            // Periodically force garbage collection to get accurate memory readings
            if (i % 100 == 0)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        var finalMemory = GC.GetTotalMemory(true);
        var memoryIncrease = finalMemory - initialMemory;

        // Assert
        var allWorkouts = await workoutService.GetAllWorkoutsAsync();
        Assert.Equal(iterations, allWorkouts.Count);
        
        // Memory assertion - should not use excessive memory (adjust threshold as needed)
        var memoryIncreaseInMB = memoryIncrease / (1024.0 * 1024.0);
        Assert.True(memoryIncreaseInMB < 100, 
            $"Memory usage increased by {memoryIncreaseInMB:F2} MB, which exceeds the 100 MB limit");
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
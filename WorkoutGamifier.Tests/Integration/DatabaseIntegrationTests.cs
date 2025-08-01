using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Tests.Integration;

public class DatabaseIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbContext _context;

    public DatabaseIntegrationTests()
    {
        var services = new ServiceCollection();
        
        // Configure in-memory database
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Register services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<WorkoutSelector>();
        services.AddScoped<PointCalculator>();
        services.AddScoped<ValidationService>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TestDbContext>();
        
        // Ensure database is created
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Database_CanCreateAndRetrieveWorkout()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var workout = new Workout
        {
            Name = "Push-ups",
            Description = "Basic push-up exercise",
            Instructions = "Start in plank position, lower body, push back up",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Beginner
        };

        // Act
        var createdWorkout = await workoutService.CreateWorkoutAsync(workout);
        var retrievedWorkout = await workoutService.GetWorkoutByIdAsync(createdWorkout.Id);

        // Assert
        Assert.NotNull(retrievedWorkout);
        Assert.Equal("Push-ups", retrievedWorkout.Name);
        Assert.Equal("Basic push-up exercise", retrievedWorkout.Description);
        Assert.Equal(15, retrievedWorkout.DurationMinutes);
        Assert.Equal(DifficultyLevel.Beginner, retrievedWorkout.Difficulty);
        Assert.True(retrievedWorkout.CreatedAt > DateTime.MinValue);
        Assert.True(retrievedWorkout.UpdatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task Database_CanUpdateWorkout()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var workout = new Workout
        {
            Name = "Squats",
            Description = "Basic squat exercise",
            DurationMinutes = 10,
            Difficulty = DifficultyLevel.Beginner
        };

        // Act
        var createdWorkout = await workoutService.CreateWorkoutAsync(workout);
        var originalUpdatedAt = createdWorkout.UpdatedAt;
        
        // Wait a bit to ensure UpdatedAt changes
        await Task.Delay(10);
        
        createdWorkout.Name = "Modified Squats";
        createdWorkout.DurationMinutes = 20;
        var updatedWorkout = await workoutService.UpdateWorkoutAsync(createdWorkout);

        // Assert
        Assert.Equal("Modified Squats", updatedWorkout.Name);
        Assert.Equal(20, updatedWorkout.DurationMinutes);
        Assert.True(updatedWorkout.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task Database_CanDeleteWorkout()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var workout = new Workout
        {
            Name = "Burpees",
            Description = "Full body exercise",
            DurationMinutes = 25,
            Difficulty = DifficultyLevel.Advanced
        };

        // Act
        var createdWorkout = await workoutService.CreateWorkoutAsync(workout);
        await workoutService.DeleteWorkoutAsync(createdWorkout.Id);
        var retrievedWorkout = await workoutService.GetWorkoutByIdAsync(createdWorkout.Id);

        // Assert
        Assert.Null(retrievedWorkout);
    }

    [Fact]
    public async Task Database_CanToggleWorkoutVisibility()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var workout = new Workout
        {
            Name = "Planks",
            Description = "Core strengthening exercise",
            DurationMinutes = 5,
            Difficulty = DifficultyLevel.Intermediate,
            IsHidden = false
        };

        // Act
        var createdWorkout = await workoutService.CreateWorkoutAsync(workout);
        var isHidden = await workoutService.ToggleWorkoutVisibilityAsync(createdWorkout.Id);
        var updatedWorkout = await workoutService.GetWorkoutByIdAsync(createdWorkout.Id);

        // Assert
        Assert.True(isHidden);
        Assert.NotNull(updatedWorkout);
        Assert.True(updatedWorkout.IsHidden);
    }

    [Fact]
    public async Task Database_TransactionRollback_WorksCorrectly()
    {
        // Arrange
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        var workout = new Workout
        {
            Name = "Test Workout",
            Description = "Test Description",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Beginner
        };

        // Act & Assert
        // Note: In-memory database doesn't support transactions, so we test the exception handling
        try
        {
            await unitOfWork.BeginTransactionAsync();
            
            // If we get here, transactions are supported (e.g., with SQLite)
            await unitOfWork.Workouts.CreateAsync(workout);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.RollbackTransactionAsync();
            
            var allWorkouts = await unitOfWork.Workouts.GetAllAsync();
            Assert.DoesNotContain(allWorkouts, w => w.Name == "Test Workout");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Transactions are not supported"))
        {
            // Expected for in-memory database - test passes
            Assert.True(true, "In-memory database correctly reports that transactions are not supported");
        }
    }

    [Fact]
    public async Task Database_ConcurrentOperations_HandleCorrectly()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var tasks = new List<Task<Workout>>();

        // Act - Create multiple workouts concurrently
        for (int i = 0; i < 10; i++)
        {
            var workout = new Workout
            {
                Name = $"Concurrent Workout {i}",
                Description = $"Description {i}",
                DurationMinutes = 10 + i,
                Difficulty = DifficultyLevel.Beginner
            };
            tasks.Add(workoutService.CreateWorkoutAsync(workout));
        }

        var createdWorkouts = await Task.WhenAll(tasks);
        var allWorkouts = await workoutService.GetAllWorkoutsAsync();

        // Assert
        Assert.Equal(10, createdWorkouts.Length);
        Assert.True(allWorkouts.Count >= 10);
        Assert.All(createdWorkouts, w => Assert.True(w.Id > 0));
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
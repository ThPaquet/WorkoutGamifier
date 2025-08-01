using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Tests.Integration;

public class DataSeedingIntegrationTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbContext _context;

    public DataSeedingIntegrationTests()
    {
        var services = new ServiceCollection();
        
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IWorkoutService, WorkoutService>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TestDbContext>();
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task DataSeeding_InitialWorkouts_CreatedSuccessfully()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var preloadedWorkouts = new List<Workout>
        {
            new Workout
            {
                Name = "Push-ups",
                Description = "Upper body strength exercise",
                Instructions = "Start in plank position, lower body to ground, push back up",
                DurationMinutes = 15,
                Difficulty = DifficultyLevel.Beginner,
                IsPreloaded = true
            },
            new Workout
            {
                Name = "Squats",
                Description = "Lower body strength exercise",
                Instructions = "Stand with feet shoulder-width apart, lower body as if sitting, return to standing",
                DurationMinutes = 20,
                Difficulty = DifficultyLevel.Intermediate,
                IsPreloaded = true
            },
            new Workout
            {
                Name = "Burpees",
                Description = "Full body cardio exercise",
                Instructions = "Squat down, jump back to plank, do push-up, jump forward, jump up",
                DurationMinutes = 25,
                Difficulty = DifficultyLevel.Advanced,
                IsPreloaded = true
            },
            new Workout
            {
                Name = "Mountain Climbers",
                Description = "Cardio and core exercise",
                Instructions = "Start in plank position, alternate bringing knees to chest rapidly",
                DurationMinutes = 10,
                Difficulty = DifficultyLevel.Expert,
                IsPreloaded = true
            }
        };

        // Act - Seed the workouts
        var createdWorkouts = new List<Workout>();
        foreach (var workout in preloadedWorkouts)
        {
            var created = await workoutService.CreateWorkoutAsync(workout);
            createdWorkouts.Add(created);
        }

        // Assert
        var allWorkouts = await workoutService.GetAllWorkoutsAsync();
        
        Assert.Equal(4, allWorkouts.Count);
        Assert.All(allWorkouts, w => Assert.True(w.IsPreloaded));
        Assert.All(allWorkouts, w => Assert.False(w.IsHidden));
        
        // Verify specific workouts
        var pushUps = allWorkouts.FirstOrDefault(w => w.Name == "Push-ups");
        Assert.NotNull(pushUps);
        Assert.Equal(DifficultyLevel.Beginner, pushUps.Difficulty);
        Assert.Equal(15, pushUps.DurationMinutes);
        
        var burpees = allWorkouts.FirstOrDefault(w => w.Name == "Burpees");
        Assert.NotNull(burpees);
        Assert.Equal(DifficultyLevel.Advanced, burpees.Difficulty);
        Assert.Equal(25, burpees.DurationMinutes);
    }

    [Fact]
    public async Task DataSeeding_DefaultActions_CreatedSuccessfully()
    {
        // Arrange
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
        var defaultActions = new List<WorkoutGamifier.Core.Models.Action>
        {
            new WorkoutGamifier.Core.Models.Action
            {
                Description = "Drink a glass of water",
                PointValue = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new WorkoutGamifier.Core.Models.Action
            {
                Description = "Take a 5-minute walk",
                PointValue = 3,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new WorkoutGamifier.Core.Models.Action
            {
                Description = "Do 10 jumping jacks",
                PointValue = 5,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new WorkoutGamifier.Core.Models.Action
            {
                Description = "Meditate for 5 minutes",
                PointValue = 8,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new WorkoutGamifier.Core.Models.Action
            {
                Description = "Complete a full workout",
                PointValue = 15,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        // Act - Seed the actions
        foreach (var action in defaultActions)
        {
            await unitOfWork.Actions.CreateAsync(action);
        }
        await unitOfWork.SaveChangesAsync();

        // Assert
        var allActions = await unitOfWork.Actions.GetAllAsync();
        
        Assert.Equal(5, allActions.Count);
        
        // Verify point values are reasonable
        Assert.All(allActions, a => Assert.InRange(a.PointValue, 1, 20));
        
        // Verify specific actions
        var waterAction = allActions.FirstOrDefault(a => a.Description.Contains("water"));
        Assert.NotNull(waterAction);
        Assert.Equal(1, waterAction.PointValue);
        
        var workoutAction = allActions.FirstOrDefault(a => a.Description.Contains("full workout"));
        Assert.NotNull(workoutAction);
        Assert.Equal(15, workoutAction.PointValue);
    }

    [Fact]
    public async Task DataSeeding_DefaultWorkoutPools_CreatedSuccessfully()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // First create some workouts
        var beginnerWorkout = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Beginner Push-ups",
            DurationMinutes = 10,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = true
        });

        var intermediateWorkout = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Intermediate Squats",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = true
        });

        var advancedWorkout = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Advanced Burpees",
            DurationMinutes = 20,
            Difficulty = DifficultyLevel.Advanced,
            IsPreloaded = true
        });

        // Create workout pools
        var beginnerPool = await unitOfWork.WorkoutPools.CreateAsync(new WorkoutPool
        {
            Name = "Beginner Workouts",
            Description = "Perfect for those just starting their fitness journey",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var mixedPool = await unitOfWork.WorkoutPools.CreateAsync(new WorkoutPool
        {
            Name = "Mixed Difficulty",
            Description = "A variety of workouts for all fitness levels",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await unitOfWork.SaveChangesAsync();

        // Add workouts to pools
        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = beginnerPool.Id,
            WorkoutId = beginnerWorkout.Id
        });

        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = mixedPool.Id,
            WorkoutId = beginnerWorkout.Id
        });

        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = mixedPool.Id,
            WorkoutId = intermediateWorkout.Id
        });

        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = mixedPool.Id,
            WorkoutId = advancedWorkout.Id
        });

        await unitOfWork.SaveChangesAsync();

        // Act & Assert
        var allPools = await unitOfWork.WorkoutPools.GetAllAsync();
        Assert.Equal(2, allPools.Count);

        // Verify beginner pool
        var beginnerPoolFromDb = allPools.FirstOrDefault(p => p.Name == "Beginner Workouts");
        Assert.NotNull(beginnerPoolFromDb);
        Assert.Contains("starting their fitness journey", beginnerPoolFromDb.Description);

        // Verify mixed pool has multiple workouts
        var mixedPoolWorkouts = await _context.WorkoutPoolWorkouts
            .Where(wpw => wpw.WorkoutPoolId == mixedPool.Id)
            .CountAsync();
        Assert.Equal(3, mixedPoolWorkouts);
    }

    [Fact]
    public async Task DataSeeding_ResetToDefaults_WorksCorrectly()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create some user data
        await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "User Workout",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = false
        });

        await unitOfWork.Actions.CreateAsync(new WorkoutGamifier.Core.Models.Action
        {
            Description = "User Action",
            PointValue = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await unitOfWork.SaveChangesAsync();

        // Verify user data exists
        var allWorkouts = await workoutService.GetAllWorkoutsAsync();
        var allActions = await unitOfWork.Actions.GetAllAsync();
        Assert.Single(allWorkouts);
        Assert.Single(allActions);

        // Act - Reset to defaults (simulate clearing user data and re-seeding)
        
        // Clear existing data
        var workoutsToDelete = await unitOfWork.Workouts.GetAllAsync();
        foreach (var workout in workoutsToDelete)
        {
            await unitOfWork.Workouts.DeleteAsync(workout.Id);
        }

        var actionsToDelete = await unitOfWork.Actions.GetAllAsync();
        foreach (var action in actionsToDelete)
        {
            await unitOfWork.Actions.DeleteAsync(action.Id);
        }

        await unitOfWork.SaveChangesAsync();

        // Re-seed with defaults
        var defaultWorkout = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Default Push-ups",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = true
        });

        await unitOfWork.Actions.CreateAsync(new WorkoutGamifier.Core.Models.Action
        {
            Description = "Default Action",
            PointValue = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await unitOfWork.SaveChangesAsync();

        // Assert
        var finalWorkouts = await workoutService.GetAllWorkoutsAsync();
        var finalActions = await unitOfWork.Actions.GetAllAsync();

        Assert.Single(finalWorkouts);
        Assert.Single(finalActions);
        Assert.True(finalWorkouts[0].IsPreloaded);
        Assert.Equal("Default Push-ups", finalWorkouts[0].Name);
        Assert.Equal("Default Action", finalActions[0].Description);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
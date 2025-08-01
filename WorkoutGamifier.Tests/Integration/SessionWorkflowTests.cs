using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Tests.Integration;

public class SessionWorkflowTests : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbContext _context;

    public SessionWorkflowTests()
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
    public async Task CompleteWorkoutSession_EndToEndWorkflow_WorksCorrectly()
    {
        // Arrange - Set up test data
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create workouts
        var workout1 = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Push-ups",
            Description = "Upper body exercise",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Beginner
        });

        var workout2 = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Squats",
            Description = "Lower body exercise",
            DurationMinutes = 20,
            Difficulty = DifficultyLevel.Intermediate
        });

        // Create workout pool
        var workoutPool = await unitOfWork.WorkoutPools.CreateAsync(new WorkoutPool
        {
            Name = "Full Body Workout",
            Description = "Complete workout routine",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        // Add workouts to pool
        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = workoutPool.Id,
            WorkoutId = workout1.Id
        });
        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = workoutPool.Id,
            WorkoutId = workout2.Id
        });
        await unitOfWork.SaveChangesAsync();

        // Create actions
        var action1 = await unitOfWork.Actions.CreateAsync(new WorkoutGamifier.Core.Models.Action
        {
            Description = "Complete 10 push-ups",
            PointValue = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        var action2 = await unitOfWork.Actions.CreateAsync(new WorkoutGamifier.Core.Models.Action
        {
            Description = "Complete 20 squats",
            PointValue = 8,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        // Act - Execute complete workflow
        
        // 1. Start session
        var session = await sessionService.StartSessionAsync("Test Session", workoutPool.Id, "Integration test session");
        Assert.NotNull(session);
        Assert.Equal(SessionStatus.Active, session.Status);
        Assert.Equal(0, session.CurrentPointBalance);

        // 2. Complete actions to earn points
        var completion1 = await sessionService.CompleteActionAsync(session.Id, action1.Id);
        var completion2 = await sessionService.CompleteActionAsync(session.Id, action2.Id);

        // Verify points were earned
        var updatedSession = await sessionService.GetSessionByIdAsync(session.Id);
        Assert.NotNull(updatedSession);
        Assert.Equal(13, updatedSession.CurrentPointBalance); // 5 + 8 points

        // 3. Spend points for workout
        var workoutReceived = await sessionService.SpendPointsForWorkoutAsync(session.Id, 10);
        Assert.NotNull(workoutReceived);
        Assert.Equal(10, workoutReceived.PointsSpent);
        Assert.True(workoutReceived.WorkoutId == workout1.Id || workoutReceived.WorkoutId == workout2.Id);

        // Verify points were spent
        updatedSession = await sessionService.GetSessionByIdAsync(session.Id);
        Assert.NotNull(updatedSession);
        Assert.Equal(3, updatedSession.CurrentPointBalance); // 13 - 10 points

        // 4. End session
        var endedSession = await sessionService.EndSessionAsync(session.Id);
        Assert.Equal(SessionStatus.Completed, endedSession.Status);
        Assert.NotNull(endedSession.EndTime);
        Assert.True(endedSession.Duration > TimeSpan.Zero);

        // Assert - Verify final state
        var finalSession = await sessionService.GetSessionByIdAsync(session.Id);
        Assert.NotNull(finalSession);
        Assert.Equal(SessionStatus.Completed, finalSession.Status);
        Assert.Equal(13, finalSession.PointsEarned);
        Assert.Equal(10, finalSession.PointsSpent);
        Assert.Equal(3, finalSession.CurrentPointBalance);

        // Verify no active session exists
        var activeSession = await sessionService.GetActiveSessionAsync();
        Assert.Null(activeSession);
    }

    [Fact]
    public async Task SessionWorkflow_InsufficientPoints_ThrowsException()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create minimal test data
        var workout = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Test Workout",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Beginner
        });

        var workoutPool = await unitOfWork.WorkoutPools.CreateAsync(new WorkoutPool
        {
            Name = "Test Pool",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = workoutPool.Id,
            WorkoutId = workout.Id
        });
        await unitOfWork.SaveChangesAsync();

        // Act & Assert
        var session = await sessionService.StartSessionAsync("Test Session", workoutPool.Id);
        
        // Try to spend more points than available (session starts with 0 points)
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sessionService.SpendPointsForWorkoutAsync(session.Id, 10));
        
        Assert.Contains("Insufficient points", exception.Message);
        Assert.Contains("Current balance: 0", exception.Message);
        Assert.Contains("Required: 10", exception.Message);
    }

    [Fact]
    public async Task SessionWorkflow_MultipleActiveSessions_ThrowsException()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create test data
        var workout = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Test Workout",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Beginner
        });

        var workoutPool = await unitOfWork.WorkoutPools.CreateAsync(new WorkoutPool
        {
            Name = "Test Pool",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = workoutPool.Id,
            WorkoutId = workout.Id
        });
        await unitOfWork.SaveChangesAsync();

        // Act & Assert
        var session1 = await sessionService.StartSessionAsync("Session 1", workoutPool.Id);
        Assert.NotNull(session1);

        // Try to start another session while first is active
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sessionService.StartSessionAsync("Session 2", workoutPool.Id));
        
        Assert.Contains("Cannot start a new session while another session is active", exception.Message);
    }

    [Fact]
    public async Task SessionWorkflow_EmptyWorkoutPool_ThrowsException()
    {
        // Arrange
        var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create empty workout pool
        var workoutPool = await unitOfWork.WorkoutPools.CreateAsync(new WorkoutPool
        {
            Name = "Empty Pool",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sessionService.StartSessionAsync("Test Session", workoutPool.Id));
        
        Assert.Contains("Cannot start session with an empty workout pool", exception.Message);
    }

    [Fact]
    public async Task SessionWorkflow_CompleteActionsInInactiveSession_ThrowsException()
    {
        // Arrange
        var workoutService = _serviceProvider.GetRequiredService<IWorkoutService>();
        var sessionService = _serviceProvider.GetRequiredService<ISessionService>();
        var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

        // Create test data
        var workout = await workoutService.CreateWorkoutAsync(new Workout
        {
            Name = "Test Workout",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Beginner
        });

        var workoutPool = await unitOfWork.WorkoutPools.CreateAsync(new WorkoutPool
        {
            Name = "Test Pool",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(new WorkoutPoolWorkout
        {
            WorkoutPoolId = workoutPool.Id,
            WorkoutId = workout.Id
        });

        var action = await unitOfWork.Actions.CreateAsync(new WorkoutGamifier.Core.Models.Action
        {
            Description = "Test Action",
            PointValue = 5,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await unitOfWork.SaveChangesAsync();

        // Act
        var session = await sessionService.StartSessionAsync("Test Session", workoutPool.Id);
        await sessionService.EndSessionAsync(session.Id);

        // Assert - Try to complete action in ended session
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => sessionService.CompleteActionAsync(session.Id, action.Id));
        
        Assert.Contains("Cannot complete actions in an inactive session", exception.Message);
    }

    public void Dispose()
    {
        _context?.Dispose();
        _serviceProvider?.Dispose();
    }
}
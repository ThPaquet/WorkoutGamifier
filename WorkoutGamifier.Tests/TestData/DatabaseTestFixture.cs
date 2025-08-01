using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Tests.TestData;

/// <summary>
/// Database test fixture providing isolated test database contexts and common test data scenarios
/// </summary>
public class DatabaseTestFixture : IDisposable
{
    private readonly ServiceProvider _serviceProvider;
    private readonly TestDbContext _context;
    private bool _disposed = false;

    public DatabaseTestFixture()
    {
        var services = new ServiceCollection();
        
        // Configure in-memory database with unique name for isolation
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()));
        
        // Register core services
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IWorkoutPoolService, WorkoutPoolService>();
        services.AddScoped<WorkoutSelector>();
        services.AddScoped<PointCalculator>();
        services.AddScoped<ValidationService>();

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<TestDbContext>();
        _context.Database.EnsureCreated();
    }

    /// <summary>
    /// Creates a new isolated database context for testing
    /// </summary>
    public TestDbContext CreateContext()
    {
        return _serviceProvider.GetRequiredService<TestDbContext>();
    }

    /// <summary>
    /// Gets the unit of work for database operations
    /// </summary>
    public IUnitOfWork GetUnitOfWork()
    {
        return _serviceProvider.GetRequiredService<IUnitOfWork>();
    }

    /// <summary>
    /// Gets a specific service from the test container
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Seeds the database with minimal test data
    /// </summary>
    public async Task SeedMinimalData()
    {
        var unitOfWork = GetUnitOfWork();

        // Create basic workout
        var workout = TestDataBuilder.Workout()
            .WithName("Basic Push-ups")
            .WithDifficulty(DifficultyLevel.Beginner)
            .WithDuration(10)
            .Build();
        await unitOfWork.Workouts.CreateAsync(workout);

        // Create basic action
        var action = TestDataBuilder.Action()
            .WithDescription("Complete 10 push-ups")
            .WithPointValue(5)
            .Build();
        await unitOfWork.Actions.CreateAsync(action);

        // Create basic workout pool
        var pool = TestDataBuilder.Pool()
            .WithName("Beginner Pool")
            .Build();
        await unitOfWork.WorkoutPools.CreateAsync(pool);

        await unitOfWork.SaveChangesAsync();

        // Add workout to pool
        var poolWorkout = TestDataBuilder.PoolWorkout()
            .WithWorkoutPool(pool)
            .WithWorkout(workout)
            .Build();
        await unitOfWork.WorkoutPoolWorkouts.CreateAsync(poolWorkout);

        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the database with a complete dataset for comprehensive testing
    /// </summary>
    public async Task SeedCompleteDataset()
    {
        var unitOfWork = GetUnitOfWork();

        // Create workouts of different difficulties
        var beginnerWorkouts = new[]
        {
            WorkoutBuilder.Beginner().WithName("Beginner Push-ups").Build(),
            WorkoutBuilder.Beginner().WithName("Beginner Squats").Build(),
            WorkoutBuilder.Beginner().WithName("Beginner Planks").Build()
        };

        var intermediateWorkouts = new[]
        {
            WorkoutBuilder.Intermediate().WithName("Intermediate Burpees").Build(),
            WorkoutBuilder.Intermediate().WithName("Intermediate Lunges").Build(),
            WorkoutBuilder.Intermediate().WithName("Intermediate Mountain Climbers").Build()
        };

        var advancedWorkouts = new[]
        {
            WorkoutBuilder.Advanced().WithName("Advanced Pull-ups").Build(),
            WorkoutBuilder.Advanced().WithName("Advanced Deadlifts").Build(),
            WorkoutBuilder.Advanced().WithName("Advanced HIIT Circuit").Build()
        };

        // Add all workouts
        foreach (var workout in beginnerWorkouts.Concat(intermediateWorkouts).Concat(advancedWorkouts))
        {
            await unitOfWork.Workouts.CreateAsync(workout);
        }

        // Create actions with different point values
        var actions = new[]
        {
            ActionBuilder.LowValue().WithDescription("Complete 10 push-ups").Build(),
            ActionBuilder.LowValue().WithDescription("Hold plank for 30 seconds").Build(),
            ActionBuilder.MediumValue().WithDescription("Complete 20 squats").Build(),
            ActionBuilder.MediumValue().WithDescription("Do 15 burpees").Build(),
            ActionBuilder.HighValue().WithDescription("Complete 10 pull-ups").Build(),
            ActionBuilder.HighValue().WithDescription("Run 1 mile").Build()
        };

        foreach (var action in actions)
        {
            await unitOfWork.Actions.CreateAsync(action);
        }

        // Create workout pools
        var beginnerPool = WorkoutPoolBuilder.Beginner().Build();
        var intermediatePool = WorkoutPoolBuilder.Intermediate().Build();
        var advancedPool = WorkoutPoolBuilder.Advanced().Build();
        var fullBodyPool = WorkoutPoolBuilder.FullBody().Build();

        await unitOfWork.WorkoutPools.CreateAsync(beginnerPool);
        await unitOfWork.WorkoutPools.CreateAsync(intermediatePool);
        await unitOfWork.WorkoutPools.CreateAsync(advancedPool);
        await unitOfWork.WorkoutPools.CreateAsync(fullBodyPool);

        await unitOfWork.SaveChangesAsync();

        // Add workouts to appropriate pools
        foreach (var workout in beginnerWorkouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(beginnerPool).WithWorkout(workout).Build());
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(fullBodyPool).WithWorkout(workout).Build());
        }

        foreach (var workout in intermediateWorkouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(intermediatePool).WithWorkout(workout).Build());
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(fullBodyPool).WithWorkout(workout).Build());
        }

        foreach (var workout in advancedWorkouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(advancedPool).WithWorkout(workout).Build());
        }

        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a complete session scenario with actions and workouts
    /// </summary>
    public async Task<SessionScenario> CreateSessionScenario()
    {
        var unitOfWork = GetUnitOfWork();

        // Create workout pool with workouts
        var pool = TestDataBuilder.Pool().WithName("Test Session Pool").Build();
        await unitOfWork.WorkoutPools.CreateAsync(pool);

        var workouts = new[]
        {
            TestDataBuilder.Workout().WithName("Session Workout 1").Build(),
            TestDataBuilder.Workout().WithName("Session Workout 2").Build()
        };

        foreach (var workout in workouts)
        {
            await unitOfWork.Workouts.CreateAsync(workout);
        }

        await unitOfWork.SaveChangesAsync();

        // Add workouts to pool
        foreach (var workout in workouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(pool).WithWorkout(workout).Build());
        }

        // Create actions
        var actions = new[]
        {
            TestDataBuilder.Action().WithDescription("Session Action 1").WithPointValue(10).Build(),
            TestDataBuilder.Action().WithDescription("Session Action 2").WithPointValue(15).Build(),
            TestDataBuilder.Action().WithDescription("Session Action 3").WithPointValue(20).Build()
        };

        foreach (var action in actions)
        {
            await unitOfWork.Actions.CreateAsync(action);
        }

        await unitOfWork.SaveChangesAsync();

        return new SessionScenario
        {
            WorkoutPool = pool,
            Workouts = workouts.ToList(),
            Actions = actions.ToList()
        };
    }

    /// <summary>
    /// Cleans the database by removing all data
    /// </summary>
    public async Task CleanDatabase()
    {
        var unitOfWork = GetUnitOfWork();

        // Remove all data in proper order to avoid foreign key constraints
        _context.WorkoutReceived.RemoveRange(_context.WorkoutReceived);
        _context.ActionCompletions.RemoveRange(_context.ActionCompletions);
        _context.Sessions.RemoveRange(_context.Sessions);
        _context.WorkoutPoolWorkouts.RemoveRange(_context.WorkoutPoolWorkouts);
        _context.WorkoutPools.RemoveRange(_context.WorkoutPools);
        _context.Workouts.RemoveRange(_context.Workouts);
        _context.Actions.RemoveRange(_context.Actions);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Executes an operation within a transaction context
    /// </summary>
    public async Task<T> ExecuteInTransaction<T>(Func<TestDbContext, Task<T>> operation)
    {
        var context = CreateContext();
        try
        {
            await context.Database.BeginTransactionAsync();
            var result = await operation(context);
            await context.Database.CommitTransactionAsync();
            return result;
        }
        catch
        {
            await context.Database.RollbackTransactionAsync();
            throw;
        }
        finally
        {
            context.Dispose();
        }
    }

    /// <summary>
    /// Seeds the database with a specific user scenario
    /// </summary>
    public async Task SeedUserScenario(UserScenarioType scenarioType)
    {
        var unitOfWork = GetUnitOfWork();

        switch (scenarioType)
        {
            case UserScenarioType.Beginner:
                await SeedBeginnerScenario(unitOfWork);
                break;
            case UserScenarioType.Intermediate:
                await SeedIntermediateScenario(unitOfWork);
                break;
            case UserScenarioType.Advanced:
                await SeedAdvancedScenario(unitOfWork);
                break;
            case UserScenarioType.Complete:
                await SeedCompleteScenario(unitOfWork);
                break;
        }

        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Seeds the database with performance testing data
    /// </summary>
    public async Task SeedPerformanceData(int workoutCount = 100, int actionCount = 50, int sessionCount = 20)
    {
        var unitOfWork = GetUnitOfWork();

        var workouts = TestDataScenarios.PerformanceScenarios.GetLargeWorkoutSet(workoutCount);
        var actions = TestDataScenarios.PerformanceScenarios.GetLargeActionSet(actionCount);
        var sessions = TestDataScenarios.PerformanceScenarios.GetLargeSessionSet(sessionCount);
        var pools = TestDataScenarios.PerformanceScenarios.GetLargePoolSet(10);

        foreach (var workout in workouts)
            await unitOfWork.Workouts.CreateAsync(workout);

        foreach (var action in actions)
            await unitOfWork.Actions.CreateAsync(action);

        foreach (var pool in pools)
            await unitOfWork.WorkoutPools.CreateAsync(pool);

        await unitOfWork.SaveChangesAsync();

        foreach (var session in sessions)
        {
            session.WorkoutPoolId = pools.First().Id;
            await unitOfWork.Sessions.CreateAsync(session);
        }

        await unitOfWork.SaveChangesAsync();
    }

    /// <summary>
    /// Creates a complete workflow scenario for testing end-to-end functionality
    /// </summary>
    public async Task<WorkflowScenarioResult> CreateCompleteWorkflowScenario()
    {
        var unitOfWork = GetUnitOfWork();

        var pool = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.Pool;
        var workouts = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.Workouts;
        var actions = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.Actions;
        var session = TestDataScenarios.WorkflowScenarios.CompleteSessionWorkflow.StartingSession;

        // Create pool
        await unitOfWork.WorkoutPools.CreateAsync(pool);
        await unitOfWork.SaveChangesAsync();

        // Create workouts
        foreach (var workout in workouts)
            await unitOfWork.Workouts.CreateAsync(workout);

        // Create actions
        foreach (var action in actions)
            await unitOfWork.Actions.CreateAsync(action);

        await unitOfWork.SaveChangesAsync();

        // Create pool-workout relationships
        foreach (var workout in workouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout()
                    .WithWorkoutPool(pool)
                    .WithWorkout(workout)
                    .Build());
        }

        // Create session
        session.WorkoutPoolId = pool.Id;
        await unitOfWork.Sessions.CreateAsync(session);

        await unitOfWork.SaveChangesAsync();

        return new WorkflowScenarioResult
        {
            Pool = pool,
            Workouts = workouts,
            Actions = actions,
            Session = session
        };
    }

    /// <summary>
    /// Verifies database integrity by checking foreign key constraints and data consistency
    /// </summary>
    public async Task<DatabaseIntegrityResult> VerifyDataIntegrity()
    {
        var result = new DatabaseIntegrityResult();
        var unitOfWork = GetUnitOfWork();

        try
        {
            // Check workout pool relationships
            var poolWorkouts = await unitOfWork.WorkoutPoolWorkouts.GetAllAsync();
            foreach (var pw in poolWorkouts)
            {
                var pool = await unitOfWork.WorkoutPools.GetByIdAsync(pw.WorkoutPoolId);
                var workout = await unitOfWork.Workouts.GetByIdAsync(pw.WorkoutId);
                
                if (pool == null)
                    result.Errors.Add($"WorkoutPoolWorkout references non-existent pool ID: {pw.WorkoutPoolId}");
                if (workout == null)
                    result.Errors.Add($"WorkoutPoolWorkout references non-existent workout ID: {pw.WorkoutId}");
            }

            // Check session relationships
            var sessions = await unitOfWork.Sessions.GetAllAsync();
            foreach (var session in sessions)
            {
                if (session.WorkoutPoolId > 0)
                {
                    var pool = await unitOfWork.WorkoutPools.GetByIdAsync(session.WorkoutPoolId);
                    if (pool == null)
                        result.Errors.Add($"Session references non-existent pool ID: {session.WorkoutPoolId}");
                }
            }

            // Check action completions
            var completions = await unitOfWork.ActionCompletions.GetAllAsync();
            foreach (var completion in completions)
            {
                var session = await unitOfWork.Sessions.GetByIdAsync(completion.SessionId);
                var action = await unitOfWork.Actions.GetByIdAsync(completion.ActionId);
                
                if (session == null)
                    result.Errors.Add($"ActionCompletion references non-existent session ID: {completion.SessionId}");
                if (action == null)
                    result.Errors.Add($"ActionCompletion references non-existent action ID: {completion.ActionId}");
            }

            // Check workout received relationships
            var workoutReceived = await unitOfWork.WorkoutReceived.GetAllAsync();
            foreach (var wr in workoutReceived)
            {
                var session = await unitOfWork.Sessions.GetByIdAsync(wr.SessionId);
                var workout = await unitOfWork.Workouts.GetByIdAsync(wr.WorkoutId);
                
                if (session == null)
                    result.Errors.Add($"WorkoutReceived references non-existent session ID: {wr.SessionId}");
                if (workout == null)
                    result.Errors.Add($"WorkoutReceived references non-existent workout ID: {wr.WorkoutId}");
            }

            result.IsValid = !result.Errors.Any();
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Database integrity check failed: {ex.Message}");
            result.IsValid = false;
        }

        return result;
    }

    /// <summary>
    /// Measures database operation performance
    /// </summary>
    public async Task<PerformanceMetrics> MeasurePerformance()
    {
        var metrics = new PerformanceMetrics();
        var unitOfWork = GetUnitOfWork();

        // Measure create operations
        var createStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var testWorkout = TestDataBuilder.Workout().Build();
        await unitOfWork.Workouts.CreateAsync(testWorkout);
        await unitOfWork.SaveChangesAsync();
        createStopwatch.Stop();
        metrics.CreateOperationMs = createStopwatch.ElapsedMilliseconds;

        // Measure read operations
        var readStopwatch = System.Diagnostics.Stopwatch.StartNew();
        var allWorkouts = await unitOfWork.Workouts.GetAllAsync();
        readStopwatch.Stop();
        metrics.ReadOperationMs = readStopwatch.ElapsedMilliseconds;
        metrics.RecordsRead = allWorkouts.Count();

        // Measure update operations
        var updateStopwatch = System.Diagnostics.Stopwatch.StartNew();
        testWorkout.Name = "Updated Name";
        await unitOfWork.Workouts.UpdateAsync(testWorkout);
        await unitOfWork.SaveChangesAsync();
        updateStopwatch.Stop();
        metrics.UpdateOperationMs = updateStopwatch.ElapsedMilliseconds;

        // Measure delete operations
        var deleteStopwatch = System.Diagnostics.Stopwatch.StartNew();
        await unitOfWork.Workouts.DeleteAsync(testWorkout.Id);
        await unitOfWork.SaveChangesAsync();
        deleteStopwatch.Stop();
        metrics.DeleteOperationMs = deleteStopwatch.ElapsedMilliseconds;

        return metrics;
    }

    private async Task SeedBeginnerScenario(IUnitOfWork unitOfWork)
    {
        var workouts = TestDataScenarios.BeginnerUserScenario.GetWorkouts();
        var actions = TestDataScenarios.BeginnerUserScenario.GetActions();
        var pool = TestDataScenarios.BeginnerUserScenario.GetWorkoutPool();

        await unitOfWork.WorkoutPools.CreateAsync(pool);
        foreach (var workout in workouts)
            await unitOfWork.Workouts.CreateAsync(workout);
        foreach (var action in actions)
            await unitOfWork.Actions.CreateAsync(action);

        await unitOfWork.SaveChangesAsync();

        foreach (var workout in workouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(pool).WithWorkout(workout).Build());
        }
    }

    private async Task SeedIntermediateScenario(IUnitOfWork unitOfWork)
    {
        var workouts = TestDataScenarios.IntermediateUserScenario.GetWorkouts();
        var actions = TestDataScenarios.IntermediateUserScenario.GetActions();
        var pool = TestDataScenarios.IntermediateUserScenario.GetWorkoutPool();

        await unitOfWork.WorkoutPools.CreateAsync(pool);
        foreach (var workout in workouts)
            await unitOfWork.Workouts.CreateAsync(workout);
        foreach (var action in actions)
            await unitOfWork.Actions.CreateAsync(action);

        await unitOfWork.SaveChangesAsync();

        foreach (var workout in workouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(pool).WithWorkout(workout).Build());
        }
    }

    private async Task SeedAdvancedScenario(IUnitOfWork unitOfWork)
    {
        var workouts = TestDataScenarios.AdvancedUserScenario.GetWorkouts();
        var actions = TestDataScenarios.AdvancedUserScenario.GetActions();
        var pool = TestDataScenarios.AdvancedUserScenario.GetWorkoutPool();

        await unitOfWork.WorkoutPools.CreateAsync(pool);
        foreach (var workout in workouts)
            await unitOfWork.Workouts.CreateAsync(workout);
        foreach (var action in actions)
            await unitOfWork.Actions.CreateAsync(action);

        await unitOfWork.SaveChangesAsync();

        foreach (var workout in workouts)
        {
            await unitOfWork.WorkoutPoolWorkouts.CreateAsync(
                TestDataBuilder.PoolWorkout().WithWorkoutPool(pool).WithWorkout(workout).Build());
        }
    }

    private async Task SeedCompleteScenario(IUnitOfWork unitOfWork)
    {
        await SeedBeginnerScenario(unitOfWork);
        await SeedIntermediateScenario(unitOfWork);
        await SeedAdvancedScenario(unitOfWork);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _context?.Dispose();
            _serviceProvider?.Dispose();
            _disposed = true;
        }
    }
}

/// <summary>
/// Represents a complete session testing scenario
/// </summary>
public class SessionScenario
{
    public WorkoutPool WorkoutPool { get; set; } = null!;
    public List<Workout> Workouts { get; set; } = new();
    public List<Core.Models.Action> Actions { get; set; } = new();
}

/// <summary>
/// Result of database integrity verification
/// </summary>
public class DatabaseIntegrityResult
{
    public bool IsValid { get; set; } = true;
    public List<string> Errors { get; set; } = new();
}

/// <summary>
/// Types of user scenarios for seeding test data
/// </summary>
public enum UserScenarioType
{
    Beginner,
    Intermediate,
    Advanced,
    Complete
}

/// <summary>
/// Result of creating a complete workflow scenario
/// </summary>
public class WorkflowScenarioResult
{
    public WorkoutPool Pool { get; set; } = null!;
    public List<Workout> Workouts { get; set; } = new();
    public List<Core.Models.Action> Actions { get; set; } = new();
    public Session Session { get; set; } = null!;
}

/// <summary>
/// Performance metrics for database operations
/// </summary>
public class PerformanceMetrics
{
    public long CreateOperationMs { get; set; }
    public long ReadOperationMs { get; set; }
    public long UpdateOperationMs { get; set; }
    public long DeleteOperationMs { get; set; }
    public int RecordsRead { get; set; }
    
    public bool IsPerformant => 
        CreateOperationMs < 100 && 
        ReadOperationMs < 100 && 
        UpdateOperationMs < 100 && 
        DeleteOperationMs < 100;
}
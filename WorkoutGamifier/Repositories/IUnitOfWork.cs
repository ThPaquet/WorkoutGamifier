using WorkoutGamifier.Models;

namespace WorkoutGamifier.Repositories;

public interface IUnitOfWork : IDisposable
{
    IRepository<Workout> Workouts { get; }
    IRepository<WorkoutPool> WorkoutPools { get; }
    IRepository<Models.Action> Actions { get; }
    IRepository<Session> Sessions { get; }
    IRepository<ActionCompletion> ActionCompletions { get; }
    IRepository<WorkoutReceived> WorkoutReceived { get; }
    IRepository<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
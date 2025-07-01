using WorkoutGamifier.Domain.Entities;

namespace WorkoutGamifier.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<User> Users { get; }
    IRepository<Workout> Workouts { get; }
    IRepository<WorkoutPool> WorkoutPools { get; }
    IRepository<WorkoutPoolWorkout> WorkoutPoolWorkouts { get; }
    IRepository<Session> Sessions { get; }
    IRepository<UserAction> UserActions { get; }
    IRepository<SessionAction> SessionActions { get; }
    IRepository<SessionWorkout> SessionWorkouts { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
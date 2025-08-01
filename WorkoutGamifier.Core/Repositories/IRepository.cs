namespace WorkoutGamifier.Core.Repositories;

public interface IRepository<T> where T : class
{
    Task<List<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task DeleteAsync(int id);
}

public interface IUnitOfWork : IDisposable
{
    IRepository<Models.Workout> Workouts { get; }
    IRepository<Models.WorkoutPool> WorkoutPools { get; }
    IRepository<Models.Action> Actions { get; }
    IRepository<Models.Session> Sessions { get; }
    IRepository<Models.ActionCompletion> ActionCompletions { get; }
    IRepository<Models.WorkoutReceived> WorkoutReceived { get; }
    IRepository<Models.WorkoutPoolWorkout> WorkoutPoolWorkouts { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
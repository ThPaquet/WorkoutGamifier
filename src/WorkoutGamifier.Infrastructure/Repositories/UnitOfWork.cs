using Microsoft.EntityFrameworkCore.Storage;
using WorkoutGamifier.Domain.Entities;
using WorkoutGamifier.Domain.Interfaces;
using WorkoutGamifier.Infrastructure.Data;

namespace WorkoutGamifier.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WorkoutGamifierDbContext _context;
    private IDbContextTransaction? _transaction;
    private bool _disposed = false;

    // Repository properties
    private IRepository<User>? _users;
    private IRepository<Workout>? _workouts;
    private IRepository<WorkoutPool>? _workoutPools;
    private IRepository<WorkoutPoolWorkout>? _workoutPoolWorkouts;
    private IRepository<Session>? _sessions;
    private IRepository<UserAction>? _userActions;
    private IRepository<SessionAction>? _sessionActions;
    private IRepository<SessionWorkout>? _sessionWorkouts;

    public UnitOfWork(WorkoutGamifierDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<User> Users => 
        _users ??= new Repository<User>(_context);

    public IRepository<Workout> Workouts => 
        _workouts ??= new Repository<Workout>(_context);

    public IRepository<WorkoutPool> WorkoutPools => 
        _workoutPools ??= new Repository<WorkoutPool>(_context);

    public IRepository<WorkoutPoolWorkout> WorkoutPoolWorkouts => 
        _workoutPoolWorkouts ??= new Repository<WorkoutPoolWorkout>(_context);

    public IRepository<Session> Sessions => 
        _sessions ??= new Repository<Session>(_context);

    public IRepository<UserAction> UserActions => 
        _userActions ??= new Repository<UserAction>(_context);

    public IRepository<SessionAction> SessionActions => 
        _sessionActions ??= new Repository<SessionAction>(_context);

    public IRepository<SessionWorkout> SessionWorkouts => 
        _sessionWorkouts ??= new Repository<SessionWorkout>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await _transaction.RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction in progress.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
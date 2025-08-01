using Microsoft.EntityFrameworkCore.Storage;
using WorkoutGamifier.Data;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<Workout>? _workouts;
    private IRepository<WorkoutPool>? _workoutPools;
    private IRepository<Models.Action>? _actions;
    private IRepository<Session>? _sessions;
    private IRepository<ActionCompletion>? _actionCompletions;
    private IRepository<WorkoutReceived>? _workoutReceived;
    private IRepository<WorkoutPoolWorkout>? _workoutPoolWorkouts;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Workout> Workouts =>
        _workouts ??= new Repository<Workout>(_context);

    public IRepository<WorkoutPool> WorkoutPools =>
        _workoutPools ??= new Repository<WorkoutPool>(_context);

    public IRepository<Models.Action> Actions =>
        _actions ??= new Repository<Models.Action>(_context);

    public IRepository<Session> Sessions =>
        _sessions ??= new Repository<Session>(_context);

    public IRepository<ActionCompletion> ActionCompletions =>
        _actionCompletions ??= new Repository<ActionCompletion>(_context);

    public IRepository<WorkoutReceived> WorkoutReceived =>
        _workoutReceived ??= new Repository<WorkoutReceived>(_context);

    public IRepository<WorkoutPoolWorkout> WorkoutPoolWorkouts =>
        _workoutPoolWorkouts ??= new Repository<WorkoutPoolWorkout>(_context);

    public async Task<int> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Log the exception here if needed
            throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
        }
    }

    public async Task BeginTransactionAsync()
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No transaction is in progress.");
        }

        try
        {
            await _transaction.RollbackAsync();
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public AppDbContext GetDbContext()
    {
        return _context;
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
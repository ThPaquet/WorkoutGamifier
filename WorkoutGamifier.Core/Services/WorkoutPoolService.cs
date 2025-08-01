using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;

namespace WorkoutGamifier.Core.Services;

public class WorkoutPoolService : IWorkoutPoolService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly Random _random;

    public WorkoutPoolService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _random = new Random();
    }

    public async Task<List<WorkoutPool>> GetAllPoolsAsync()
    {
        return await _unitOfWork.WorkoutPools.GetAllAsync();
    }

    public async Task<WorkoutPool?> GetPoolByIdAsync(int id)
    {
        return await _unitOfWork.WorkoutPools.GetByIdAsync(id);
    }

    public async Task<WorkoutPool> CreatePoolAsync(WorkoutPool pool)
    {
        ValidateWorkoutPool(pool);

        var createdPool = await _unitOfWork.WorkoutPools.CreateAsync(pool);
        await _unitOfWork.SaveChangesAsync();

        return createdPool;
    }

    public async Task<WorkoutPool> UpdatePoolAsync(WorkoutPool pool)
    {
        ValidateWorkoutPool(pool);

        var existingPool = await _unitOfWork.WorkoutPools.GetByIdAsync(pool.Id);
        if (existingPool == null)
        {
            throw new InvalidOperationException($"Workout pool with ID {pool.Id} not found.");
        }

        existingPool.Name = pool.Name;
        existingPool.Description = pool.Description;

        var updatedPool = await _unitOfWork.WorkoutPools.UpdateAsync(existingPool);
        await _unitOfWork.SaveChangesAsync();

        return updatedPool;
    }

    public async Task DeletePoolAsync(int poolId)
    {
        var pool = await _unitOfWork.WorkoutPools.GetByIdAsync(poolId);
        if (pool == null)
        {
            throw new InvalidOperationException($"Workout pool with ID {poolId} not found.");
        }

        // Check if pool can be deleted (not used in active sessions)
        if (!await CanDeletePoolAsync(poolId))
        {
            throw new InvalidOperationException("Cannot delete workout pool that is currently used in active sessions.");
        }

        // Remove all workout-pool relationships first
        var poolWorkouts = await GetWorkoutPoolWorkoutsAsync(poolId);
        var dbContext = ((UnitOfWork)_unitOfWork).GetDbContext();
        foreach (var poolWorkout in poolWorkouts)
        {
            dbContext.Remove(poolWorkout);
        }

        await _unitOfWork.WorkoutPools.DeleteAsync(poolId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<Workout?> GetRandomWorkoutFromPoolAsync(int poolId)
    {
        var workoutsInPool = await GetWorkoutsInPoolAsync(poolId);
        
        if (!workoutsInPool.Any())
        {
            throw new InvalidOperationException("Cannot get random workout from empty pool.");
        }

        var randomIndex = _random.Next(workoutsInPool.Count);
        return workoutsInPool[randomIndex];
    }

    public async Task AddWorkoutToPoolAsync(int poolId, int workoutId)
    {
        // Validate pool exists
        var pool = await _unitOfWork.WorkoutPools.GetByIdAsync(poolId);
        if (pool == null)
        {
            throw new InvalidOperationException($"Workout pool with ID {poolId} not found.");
        }

        // Validate workout exists
        var workout = await _unitOfWork.Workouts.GetByIdAsync(workoutId);
        if (workout == null)
        {
            throw new InvalidOperationException($"Workout with ID {workoutId} not found.");
        }

        // Check if relationship already exists
        var existingRelation = await GetWorkoutPoolWorkoutAsync(poolId, workoutId);
        if (existingRelation != null)
        {
            throw new InvalidOperationException("Workout is already in this pool.");
        }

        var poolWorkout = new WorkoutPoolWorkout
        {
            WorkoutPoolId = poolId,
            WorkoutId = workoutId
        };

        await _unitOfWork.WorkoutPoolWorkouts.CreateAsync(poolWorkout);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RemoveWorkoutFromPoolAsync(int poolId, int workoutId)
    {
        var poolWorkout = await GetWorkoutPoolWorkoutAsync(poolId, workoutId);
        if (poolWorkout == null)
        {
            throw new InvalidOperationException("Workout is not in this pool.");
        }

        await RemoveWorkoutPoolWorkoutAsync(poolId, workoutId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<Workout>> GetWorkoutsInPoolAsync(int poolId)
    {
        // More efficient: Use a single query with joins instead of loading all data
        var dbContext = ((UnitOfWork)_unitOfWork).GetDbContext();
        
        return await dbContext.Set<Workout>()
            .Where(w => dbContext.Set<WorkoutPoolWorkout>()
                .Any(pw => pw.WorkoutPoolId == poolId && pw.WorkoutId == w.Id))
            .Where(w => !w.IsHidden) // Only return visible workouts
            .ToListAsync();
    }

    private async Task<List<WorkoutPoolWorkout>> GetWorkoutPoolWorkoutsAsync(int poolId)
    {
        // More efficient: Use database query instead of loading all relationships
        var dbContext = ((UnitOfWork)_unitOfWork).GetDbContext();
        return await dbContext.Set<WorkoutPoolWorkout>()
            .Where(pw => pw.WorkoutPoolId == poolId)
            .ToListAsync();
    }

    private async Task<WorkoutPoolWorkout?> GetWorkoutPoolWorkoutAsync(int poolId, int workoutId)
    {
        // More efficient: Use database query with specific conditions
        var dbContext = ((UnitOfWork)_unitOfWork).GetDbContext();
        return await dbContext.Set<WorkoutPoolWorkout>()
            .FirstOrDefaultAsync(pw => pw.WorkoutPoolId == poolId && pw.WorkoutId == workoutId);
    }

    private async Task RemoveWorkoutPoolWorkoutAsync(int poolId, int workoutId)
    {
        // More efficient: Find and remove in a single operation
        var dbContext = ((UnitOfWork)_unitOfWork).GetDbContext();
        var toRemove = await dbContext.Set<WorkoutPoolWorkout>()
            .FirstOrDefaultAsync(pw => pw.WorkoutPoolId == poolId && pw.WorkoutId == workoutId);
        
        if (toRemove != null)
        {
            dbContext.Remove(toRemove);
        }
    }

    private async Task<bool> CanDeletePoolAsync(int poolId)
    {
        // Check if pool is used in any active sessions
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        return !allSessions.Any(s => s.WorkoutPoolId == poolId && s.Status == SessionStatus.Active);
    }

    private void ValidateWorkoutPool(WorkoutPool pool)
    {
        if (string.IsNullOrWhiteSpace(pool.Name))
        {
            throw new ArgumentException("Workout pool name is required.");
        }

        if (pool.Name.Length > 100)
        {
            throw new ArgumentException("Workout pool name cannot exceed 100 characters.");
        }

        if (pool.Description?.Length > 500)
        {
            throw new ArgumentException("Workout pool description cannot exceed 500 characters.");
        }
    }
}
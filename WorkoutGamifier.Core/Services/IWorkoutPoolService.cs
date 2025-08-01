using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Core.Services;

public interface IWorkoutPoolService
{
    Task<List<WorkoutPool>> GetAllPoolsAsync();
    Task<WorkoutPool?> GetPoolByIdAsync(int id);
    Task<WorkoutPool> CreatePoolAsync(WorkoutPool pool);
    Task<WorkoutPool> UpdatePoolAsync(WorkoutPool pool);
    Task DeletePoolAsync(int poolId);
    Task<Workout?> GetRandomWorkoutFromPoolAsync(int poolId);
    Task AddWorkoutToPoolAsync(int poolId, int workoutId);
    Task RemoveWorkoutFromPoolAsync(int poolId, int workoutId);
    Task<List<Workout>> GetWorkoutsInPoolAsync(int poolId);
}
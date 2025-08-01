using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public interface IWorkoutService
{
    Task<List<Workout>> GetAllWorkoutsAsync();
    Task<List<Workout>> GetVisibleWorkoutsAsync();
    Task<Workout?> GetWorkoutByIdAsync(int id);
    Task<Workout> CreateWorkoutAsync(Workout workout);
    Task<Workout> UpdateWorkoutAsync(Workout workout);
    Task DeleteWorkoutAsync(int workoutId);
    Task<bool> ToggleWorkoutVisibilityAsync(int workoutId);
    Task<bool> CanDeleteWorkoutAsync(int workoutId);
}
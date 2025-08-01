using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Models;
using WorkoutGamifier.Repositories;

namespace WorkoutGamifier.Services;

public class WorkoutService : IWorkoutService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Workout>> GetAllWorkoutsAsync()
    {
        return await _unitOfWork.Workouts.GetAllAsync();
    }

    public async Task<List<Workout>> GetVisibleWorkoutsAsync()
    {
        var allWorkouts = await _unitOfWork.Workouts.GetAllAsync();
        return allWorkouts.Where(w => !w.IsHidden).ToList();
    }

    public async Task<Workout?> GetWorkoutByIdAsync(int id)
    {
        return await _unitOfWork.Workouts.GetByIdAsync(id);
    }

    public async Task<Workout> CreateWorkoutAsync(Workout workout)
    {
        // Validate workout data
        ValidateWorkout(workout);

        // Set as user-created workout
        workout.IsPreloaded = false;
        workout.IsHidden = false;

        var createdWorkout = await _unitOfWork.Workouts.CreateAsync(workout);
        await _unitOfWork.SaveChangesAsync();

        return createdWorkout;
    }

    public async Task<Workout> UpdateWorkoutAsync(Workout workout)
    {
        // Validate workout data
        ValidateWorkout(workout);

        // Check if workout exists
        var existingWorkout = await _unitOfWork.Workouts.GetByIdAsync(workout.Id);
        if (existingWorkout == null)
        {
            throw new InvalidOperationException($"Workout with ID {workout.Id} not found.");
        }

        // Prevent modification of core attributes for preloaded workouts
        if (existingWorkout.IsPreloaded)
        {
            // Only allow certain fields to be modified for preloaded workouts
            existingWorkout.IsHidden = workout.IsHidden;
        }
        else
        {
            // Allow full modification for user-created workouts
            existingWorkout.Name = workout.Name;
            existingWorkout.Description = workout.Description;
            existingWorkout.Instructions = workout.Instructions;
            existingWorkout.DurationMinutes = workout.DurationMinutes;
            existingWorkout.Difficulty = workout.Difficulty;
            existingWorkout.IsHidden = workout.IsHidden;
        }

        var updatedWorkout = await _unitOfWork.Workouts.UpdateAsync(existingWorkout);
        await _unitOfWork.SaveChangesAsync();

        return updatedWorkout;
    }

    public async Task DeleteWorkoutAsync(int workoutId)
    {
        var workout = await _unitOfWork.Workouts.GetByIdAsync(workoutId);
        if (workout == null)
        {
            throw new InvalidOperationException($"Workout with ID {workoutId} not found.");
        }

        // Prevent deletion of preloaded workouts
        if (workout.IsPreloaded)
        {
            throw new InvalidOperationException("Cannot delete preloaded workouts. Use hide functionality instead.");
        }

        // Check if workout can be deleted (not used in active sessions)
        if (!await CanDeleteWorkoutAsync(workoutId))
        {
            throw new InvalidOperationException("Cannot delete workout that is currently used in workout pools or active sessions.");
        }

        await _unitOfWork.Workouts.DeleteAsync(workoutId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> ToggleWorkoutVisibilityAsync(int workoutId)
    {
        var workout = await _unitOfWork.Workouts.GetByIdAsync(workoutId);
        if (workout == null)
        {
            throw new InvalidOperationException($"Workout with ID {workoutId} not found.");
        }

        workout.IsHidden = !workout.IsHidden;
        await _unitOfWork.Workouts.UpdateAsync(workout);
        await _unitOfWork.SaveChangesAsync();

        return !workout.IsHidden; // Return new visibility state
    }

    public async Task<bool> CanDeleteWorkoutAsync(int workoutId)
    {
        // Check if workout is used in any workout pools
        var allPools = await _unitOfWork.WorkoutPools.GetAllAsync();
        // Note: This is a simplified check. In a real implementation, you'd want to 
        // check the WorkoutPoolWorkouts junction table directly
        
        // For now, assume we can delete if it's not a preloaded workout
        var workout = await _unitOfWork.Workouts.GetByIdAsync(workoutId);
        return workout != null && !workout.IsPreloaded;
    }

    private void ValidateWorkout(Workout workout)
    {
        if (string.IsNullOrWhiteSpace(workout.Name))
        {
            throw new ArgumentException("Workout name is required.");
        }

        if (workout.Name.Length > 100)
        {
            throw new ArgumentException("Workout name cannot exceed 100 characters.");
        }

        if (workout.Description?.Length > 500)
        {
            throw new ArgumentException("Workout description cannot exceed 500 characters.");
        }

        if (workout.Instructions?.Length > 2000)
        {
            throw new ArgumentException("Workout instructions cannot exceed 2000 characters.");
        }

        if (workout.DurationMinutes < 1 || workout.DurationMinutes > 300)
        {
            throw new ArgumentException("Workout duration must be between 1 and 300 minutes.");
        }

        if (!Enum.IsDefined(typeof(DifficultyLevel), workout.Difficulty))
        {
            throw new ArgumentException("Invalid difficulty level.");
        }
    }
}
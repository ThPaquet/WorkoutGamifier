using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;

namespace WorkoutGamifier.Core.Services;

public class WorkoutStatistics
{
    public int TotalWorkouts { get; set; }
    public int BeginnerCount { get; set; }
    public int IntermediateCount { get; set; }
    public int AdvancedCount { get; set; }
    public int ExpertCount { get; set; }
    public double AverageDuration { get; set; }
    public int ShortestDuration { get; set; }
    public int LongestDuration { get; set; }
}

public interface IWorkoutService
{
    Task<List<Workout>> GetAllWorkoutsAsync();
    Task<Workout?> GetWorkoutByIdAsync(int id);
    Task<Workout> CreateWorkoutAsync(Workout workout);
    Task<Workout> UpdateWorkoutAsync(Workout workout);
    Task<bool> DeleteWorkoutAsync(int workoutId);
    Task<bool> ToggleWorkoutVisibilityAsync(int workoutId);
    
    // Enhanced methods for comprehensive testing
    Task<List<Workout>> GetWorkoutsByDifficultyAsync(DifficultyLevel difficulty);
    Task<List<Workout>> GetWorkoutsByDurationRangeAsync(int minMinutes, int maxMinutes);
    Task<List<Workout>> SearchWorkoutsAsync(string query);
    Task<WorkoutStatistics> GetWorkoutStatisticsAsync();
    Task<List<Workout>> BulkCreateWorkoutsAsync(List<Workout> workouts);
    Task<List<Workout>> GetRecentWorkoutsAsync(int count);
}

public class WorkoutService : IWorkoutService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkoutService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Workout>> GetAllWorkoutsAsync()
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
        if (workout == null)
            throw new ArgumentNullException(nameof(workout));

        if (string.IsNullOrWhiteSpace(workout.Name))
            throw new ArgumentException("Workout name cannot be empty", nameof(workout));

        if (workout.DurationMinutes <= 0)
            throw new ArgumentException("Workout duration must be greater than zero", nameof(workout));

        workout.CreatedAt = DateTime.UtcNow;
        workout.UpdatedAt = DateTime.UtcNow;

        var createdWorkout = await _unitOfWork.Workouts.CreateAsync(workout);
        await _unitOfWork.SaveChangesAsync();

        return createdWorkout;
    }

    public async Task<Workout> UpdateWorkoutAsync(Workout workout)
    {
        if (workout == null)
            throw new ArgumentNullException(nameof(workout));

        var existingWorkout = await _unitOfWork.Workouts.GetByIdAsync(workout.Id);
        if (existingWorkout == null)
            throw new InvalidOperationException($"Workout with ID {workout.Id} not found.");

        workout.UpdatedAt = DateTime.UtcNow;
        var updatedWorkout = await _unitOfWork.Workouts.UpdateAsync(workout);
        await _unitOfWork.SaveChangesAsync();

        return updatedWorkout;
    }

    public async Task<bool> DeleteWorkoutAsync(int workoutId)
    {
        var workout = await _unitOfWork.Workouts.GetByIdAsync(workoutId);
        if (workout == null)
            return false;

        await _unitOfWork.Workouts.DeleteAsync(workoutId);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ToggleWorkoutVisibilityAsync(int workoutId)
    {
        var workout = await _unitOfWork.Workouts.GetByIdAsync(workoutId);
        if (workout == null)
            throw new InvalidOperationException($"Workout with ID {workoutId} not found.");

        workout.IsHidden = !workout.IsHidden;
        workout.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Workouts.UpdateAsync(workout);
        await _unitOfWork.SaveChangesAsync();

        return workout.IsHidden;
    }

    public async Task<List<Workout>> GetWorkoutsByDifficultyAsync(DifficultyLevel difficulty)
    {
        var allWorkouts = await _unitOfWork.Workouts.GetAllAsync();
        return allWorkouts.Where(w => w.Difficulty == difficulty && !w.IsHidden).ToList();
    }

    public async Task<List<Workout>> GetWorkoutsByDurationRangeAsync(int minMinutes, int maxMinutes)
    {
        if (minMinutes > maxMinutes)
            throw new ArgumentException("Minimum duration cannot be greater than maximum duration.");

        var allWorkouts = await _unitOfWork.Workouts.GetAllAsync();
        return allWorkouts.Where(w => w.DurationMinutes >= minMinutes && 
                                     w.DurationMinutes <= maxMinutes && 
                                     !w.IsHidden).ToList();
    }

    public async Task<List<Workout>> SearchWorkoutsAsync(string query)
    {
        var allWorkouts = await _unitOfWork.Workouts.GetAllAsync();
        
        if (string.IsNullOrEmpty(query))
            return allWorkouts.Where(w => !w.IsHidden).ToList();

        var lowerQuery = query.ToLower();
        return allWorkouts.Where(w => !w.IsHidden && 
                                     (w.Name.ToLower().Contains(lowerQuery) || 
                                      w.Description?.ToLower().Contains(lowerQuery) == true)).ToList();
    }

    public async Task<WorkoutStatistics> GetWorkoutStatisticsAsync()
    {
        var allWorkouts = await _unitOfWork.Workouts.GetAllAsync();
        var visibleWorkouts = allWorkouts.Where(w => !w.IsHidden).ToList();

        if (!visibleWorkouts.Any())
        {
            return new WorkoutStatistics
            {
                TotalWorkouts = 0,
                BeginnerCount = 0,
                IntermediateCount = 0,
                AdvancedCount = 0,
                ExpertCount = 0,
                AverageDuration = 0.0,
                ShortestDuration = 0,
                LongestDuration = 0
            };
        }

        return new WorkoutStatistics
        {
            TotalWorkouts = visibleWorkouts.Count,
            BeginnerCount = visibleWorkouts.Count(w => w.Difficulty == DifficultyLevel.Beginner),
            IntermediateCount = visibleWorkouts.Count(w => w.Difficulty == DifficultyLevel.Intermediate),
            AdvancedCount = visibleWorkouts.Count(w => w.Difficulty == DifficultyLevel.Advanced),
            ExpertCount = visibleWorkouts.Count(w => w.Difficulty == DifficultyLevel.Expert),
            AverageDuration = visibleWorkouts.Average(w => w.DurationMinutes),
            ShortestDuration = visibleWorkouts.Min(w => w.DurationMinutes),
            LongestDuration = visibleWorkouts.Max(w => w.DurationMinutes)
        };
    }

    public async Task<List<Workout>> BulkCreateWorkoutsAsync(List<Workout> workouts)
    {
        if (workouts == null || !workouts.Any())
            return new List<Workout>();

        var results = new List<Workout>();
        var now = DateTime.UtcNow;

        foreach (var workout in workouts)
        {
            if (workout == null)
                throw new ArgumentNullException(nameof(workout), "Workout cannot be null");
            
            if (string.IsNullOrWhiteSpace(workout.Name))
                throw new ArgumentException("Workout name cannot be empty", nameof(workout));

            workout.CreatedAt = now;
            workout.UpdatedAt = now;
            
            var created = await _unitOfWork.Workouts.CreateAsync(workout);
            results.Add(created);
        }

        await _unitOfWork.SaveChangesAsync();
        return results;
    }

    public async Task<List<Workout>> GetRecentWorkoutsAsync(int count)
    {
        var allWorkouts = await _unitOfWork.Workouts.GetAllAsync();
        return allWorkouts.Where(w => !w.IsHidden)
                         .OrderByDescending(w => w.CreatedAt)
                         .Take(count)
                         .ToList();
    }
}
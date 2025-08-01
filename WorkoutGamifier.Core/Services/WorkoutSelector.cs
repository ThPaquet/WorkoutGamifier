using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Core.Services;

public class WorkoutSelector
{
    private readonly Random _random;

    public WorkoutSelector() : this(new Random())
    {
    }

    public WorkoutSelector(Random random)
    {
        _random = random;
    }

    public Workout? SelectRandomWorkout(IList<Workout> workouts)
    {
        if (workouts == null || !workouts.Any())
            return null;

        var availableWorkouts = workouts.Where(w => !w.IsHidden).ToList();
        
        if (!availableWorkouts.Any())
            return null;

        var index = _random.Next(availableWorkouts.Count);
        return availableWorkouts[index];
    }

    public Workout? SelectRandomWorkoutByDifficulty(IList<Workout> workouts, DifficultyLevel difficulty)
    {
        if (workouts == null || !workouts.Any())
            return null;

        var filteredWorkouts = workouts
            .Where(w => !w.IsHidden && w.Difficulty == difficulty)
            .ToList();

        return SelectRandomWorkout(filteredWorkouts);
    }

    public IEnumerable<Workout> FilterWorkoutsByDuration(IList<Workout> workouts, int minDuration, int maxDuration)
    {
        if (workouts == null)
            return Enumerable.Empty<Workout>();

        return workouts.Where(w => 
            !w.IsHidden && 
            w.DurationMinutes >= minDuration && 
            w.DurationMinutes <= maxDuration);
    }

    public IEnumerable<Workout> GetWorkoutsByDifficulty(IList<Workout> workouts, DifficultyLevel difficulty)
    {
        if (workouts == null)
            return Enumerable.Empty<Workout>();

        return workouts.Where(w => !w.IsHidden && w.Difficulty == difficulty);
    }
}
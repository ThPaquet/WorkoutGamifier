using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Data;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public class DataSeedingService : IDataSeedingService
{
    private readonly AppDbContext _context;

    public DataSeedingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task SeedInitialDataAsync()
    {
        // Ensure database is created
        await _context.Database.EnsureCreatedAsync();

        // Check if data is already seeded
        if (await IsDataSeededAsync())
            return;

        // Seed default workouts
        await SeedDefaultWorkoutsAsync();

        await _context.SaveChangesAsync();
    }

    public async Task<bool> IsDataSeededAsync()
    {
        return await _context.Workouts.AnyAsync(w => w.IsPreloaded);
    }

    public async Task ResetToDefaultDataAsync()
    {
        // Remove all existing data
        _context.WorkoutReceived.RemoveRange(_context.WorkoutReceived);
        _context.ActionCompletions.RemoveRange(_context.ActionCompletions);
        _context.Sessions.RemoveRange(_context.Sessions);
        _context.WorkoutPoolWorkouts.RemoveRange(_context.WorkoutPoolWorkouts);
        _context.WorkoutPools.RemoveRange(_context.WorkoutPools);
        _context.Actions.RemoveRange(_context.Actions);
        _context.Workouts.RemoveRange(_context.Workouts);

        await _context.SaveChangesAsync();

        // Re-seed default data
        await SeedDefaultWorkoutsAsync();
        await _context.SaveChangesAsync();
    }

    private async Task SeedDefaultWorkoutsAsync()
    {
        var defaultWorkouts = new List<Workout>
        {
            new Workout
            {
                Name = "Push-ups",
                Description = "Classic upper body exercise",
                Instructions = "Start in plank position. Lower your body until chest nearly touches floor. Push back up to starting position.",
                DurationMinutes = 5,
                Difficulty = DifficultyLevel.Beginner,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "Squats",
                Description = "Lower body strength exercise",
                Instructions = "Stand with feet shoulder-width apart. Lower your body as if sitting back into a chair. Return to standing position.",
                DurationMinutes = 5,
                Difficulty = DifficultyLevel.Beginner,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "Plank",
                Description = "Core strengthening exercise",
                Instructions = "Hold a push-up position with forearms on ground. Keep body straight from head to heels.",
                DurationMinutes = 3,
                Difficulty = DifficultyLevel.Beginner,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "Jumping Jacks",
                Description = "Full body cardio exercise",
                Instructions = "Jump while spreading legs and raising arms overhead. Return to starting position.",
                DurationMinutes = 5,
                Difficulty = DifficultyLevel.Beginner,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "Burpees",
                Description = "High intensity full body exercise",
                Instructions = "Start standing. Drop to squat, jump back to plank, do push-up, jump feet to squat, jump up with arms overhead.",
                DurationMinutes = 10,
                Difficulty = DifficultyLevel.Intermediate,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "Mountain Climbers",
                Description = "Cardio and core exercise",
                Instructions = "Start in plank position. Alternate bringing knees to chest in running motion.",
                DurationMinutes = 5,
                Difficulty = DifficultyLevel.Intermediate,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "Lunges",
                Description = "Lower body strength and balance",
                Instructions = "Step forward into lunge position. Lower back knee toward ground. Return to standing and repeat.",
                DurationMinutes = 8,
                Difficulty = DifficultyLevel.Intermediate,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "High Knees",
                Description = "Cardio warm-up exercise",
                Instructions = "Run in place while lifting knees as high as possible toward chest.",
                DurationMinutes = 3,
                Difficulty = DifficultyLevel.Beginner,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "Wall Sit",
                Description = "Isometric leg strengthening",
                Instructions = "Lean back against wall with feet shoulder-width apart. Slide down until thighs are parallel to floor. Hold position.",
                DurationMinutes = 5,
                Difficulty = DifficultyLevel.Intermediate,
                IsPreloaded = true,
                IsHidden = false
            },
            new Workout
            {
                Name = "Russian Twists",
                Description = "Core and oblique exercise",
                Instructions = "Sit with knees bent, lean back slightly. Rotate torso side to side while keeping core engaged.",
                DurationMinutes = 5,
                Difficulty = DifficultyLevel.Intermediate,
                IsPreloaded = true,
                IsHidden = false
            }
        };

        await _context.Workouts.AddRangeAsync(defaultWorkouts);
    }
}
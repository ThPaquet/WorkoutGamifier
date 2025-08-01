using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Tests.TestData;

/// <summary>
/// Main entry point for creating test data using fluent builder pattern
/// </summary>
public static class TestDataBuilder
{
    public static WorkoutBuilder Workout() => new WorkoutBuilder();
    public static SessionBuilder Session() => new SessionBuilder();
    public static WorkoutPoolBuilder Pool() => new WorkoutPoolBuilder();
    public static ActionBuilder Action() => new ActionBuilder();
    public static WorkoutPoolWorkoutBuilder PoolWorkout() => new WorkoutPoolWorkoutBuilder();
    public static ActionCompletionBuilder ActionCompletion() => new ActionCompletionBuilder();
    public static WorkoutReceivedBuilder WorkoutReceived() => new WorkoutReceivedBuilder();
}

/// <summary>
/// Builder for creating Workout test entities
/// </summary>
public class WorkoutBuilder
{
    private readonly Workout _workout;
    private static readonly Random _random = new();

    public WorkoutBuilder()
    {
        _workout = new Workout
        {
            Name = GenerateRandomWorkoutName(),
            Description = GenerateRandomDescription(),
            Instructions = GenerateRandomInstructions(),
            DurationMinutes = GenerateRandomDuration(),
            Difficulty = GenerateRandomDifficulty(),
            IsHidden = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public WorkoutBuilder WithName(string name)
    {
        _workout.Name = name;
        return this;
    }

    public WorkoutBuilder WithDescription(string description)
    {
        _workout.Description = description;
        return this;
    }

    public WorkoutBuilder WithInstructions(string instructions)
    {
        _workout.Instructions = instructions;
        return this;
    }

    public WorkoutBuilder WithDuration(int minutes)
    {
        _workout.DurationMinutes = minutes;
        return this;
    }

    public WorkoutBuilder WithDifficulty(DifficultyLevel difficulty)
    {
        _workout.Difficulty = difficulty;
        return this;
    }

    public WorkoutBuilder AsHidden()
    {
        _workout.IsHidden = true;
        return this;
    }

    public WorkoutBuilder AsVisible()
    {
        _workout.IsHidden = false;
        return this;
    }

    public WorkoutBuilder WithId(int id)
    {
        _workout.Id = id;
        return this;
    }

    public WorkoutBuilder CreatedAt(DateTime createdAt)
    {
        _workout.CreatedAt = createdAt;
        return this;
    }

    public WorkoutBuilder UpdatedAt(DateTime updatedAt)
    {
        _workout.UpdatedAt = updatedAt;
        return this;
    }

    public Workout Build() => _workout;

    // Static factory methods for common scenarios
    public static WorkoutBuilder Beginner() => new WorkoutBuilder().WithDifficulty(DifficultyLevel.Beginner).WithDuration(15);
    public static WorkoutBuilder Intermediate() => new WorkoutBuilder().WithDifficulty(DifficultyLevel.Intermediate).WithDuration(30);
    public static WorkoutBuilder Advanced() => new WorkoutBuilder().WithDifficulty(DifficultyLevel.Advanced).WithDuration(45);
    public static WorkoutBuilder Quick() => new WorkoutBuilder().WithDuration(10);
    public static WorkoutBuilder Long() => new WorkoutBuilder().WithDuration(60);
    public static WorkoutBuilder Hidden() => new WorkoutBuilder().AsHidden();

    private static string GenerateRandomWorkoutName()
    {
        var workoutTypes = new[] { "Push-ups", "Squats", "Burpees", "Planks", "Lunges", "Mountain Climbers", "Jumping Jacks", "Pull-ups", "Sit-ups", "Deadlifts" };
        var modifiers = new[] { "Intense", "Quick", "Power", "Endurance", "Strength", "Cardio", "Core", "Dynamic", "Static", "Explosive" };
        return $"{modifiers[_random.Next(modifiers.Length)]} {workoutTypes[_random.Next(workoutTypes.Length)]}";
    }

    private static string GenerateRandomDescription()
    {
        var descriptions = new[]
        {
            "A great exercise for building strength and endurance.",
            "Perfect for improving cardiovascular health.",
            "Excellent for core strengthening and stability.",
            "Ideal for building muscle and burning calories.",
            "Great for improving flexibility and balance.",
            "Perfect for a quick and effective workout.",
            "Targets multiple muscle groups simultaneously.",
            "Builds functional strength for daily activities."
        };
        return descriptions[_random.Next(descriptions.Length)];
    }

    private static string GenerateRandomInstructions()
    {
        var instructions = new[]
        {
            "Start in the correct position and maintain proper form throughout the exercise.",
            "Begin slowly and gradually increase intensity as you become more comfortable.",
            "Focus on controlled movements and proper breathing technique.",
            "Maintain steady rhythm and avoid rushing through the movements.",
            "Keep your core engaged and maintain proper posture throughout.",
            "Start with lighter intensity and progress as your strength improves.",
            "Pay attention to your body and stop if you feel any pain or discomfort."
        };
        return instructions[_random.Next(instructions.Length)];
    }

    private static int GenerateRandomDuration()
    {
        var durations = new[] { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 60 };
        return durations[_random.Next(durations.Length)];
    }

    private static DifficultyLevel GenerateRandomDifficulty()
    {
        var difficulties = new[] { DifficultyLevel.Beginner, DifficultyLevel.Intermediate, DifficultyLevel.Advanced };
        return difficulties[_random.Next(difficulties.Length)];
    }
}

/// <summary>
/// Builder for creating Session test entities
/// </summary>
public class SessionBuilder
{
    private readonly Session _session;
    private static readonly Random _random = new();

    public SessionBuilder()
    {
        _session = new Session
        {
            Name = GenerateRandomSessionName(),
            Description = GenerateRandomDescription(),
            Status = SessionStatus.Active,
            StartTime = DateTime.UtcNow,
            WorkoutPoolId = 1, // Default pool ID
            PointsEarned = 0,
            PointsSpent = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public SessionBuilder WithName(string name)
    {
        _session.Name = name;
        return this;
    }

    public SessionBuilder WithDescription(string description)
    {
        _session.Description = description;
        return this;
    }

    public SessionBuilder WithStatus(SessionStatus status)
    {
        _session.Status = status;
        return this;
    }

    public SessionBuilder WithWorkoutPool(int workoutPoolId)
    {
        _session.WorkoutPoolId = workoutPoolId;
        return this;
    }

    public SessionBuilder WithWorkoutPool(WorkoutPool workoutPool)
    {
        _session.WorkoutPoolId = workoutPool.Id;
        return this;
    }

    public SessionBuilder WithStartTime(DateTime startTime)
    {
        _session.StartTime = startTime;
        return this;
    }

    public SessionBuilder WithEndTime(DateTime endTime)
    {
        _session.EndTime = endTime;
        _session.Status = SessionStatus.Completed;
        return this;
    }

    public SessionBuilder WithPointBalance(int points)
    {
        // Set PointsEarned to achieve the desired balance
        _session.PointsEarned = points + _session.PointsSpent;
        return this;
    }

    public SessionBuilder WithPointsEarned(int points)
    {
        _session.PointsEarned = points;
        return this;
    }

    public SessionBuilder WithPointsSpent(int points)
    {
        _session.PointsSpent = points;
        return this;
    }

    public SessionBuilder WithId(int id)
    {
        _session.Id = id;
        return this;
    }

    public SessionBuilder AsActive()
    {
        _session.Status = SessionStatus.Active;
        _session.EndTime = null;
        return this;
    }

    public SessionBuilder AsCompleted()
    {
        _session.Status = SessionStatus.Completed;
        _session.EndTime = DateTime.UtcNow;
        return this;
    }

    public Session Build() => _session;

    // Static factory methods for common scenarios
    public static SessionBuilder Active() => new SessionBuilder().AsActive();
    public static SessionBuilder Completed() => new SessionBuilder().AsCompleted();
    public static SessionBuilder WithPoints(int points) => new SessionBuilder().WithPointBalance(points);

    private static string GenerateRandomSessionName()
    {
        var sessionTypes = new[] { "Morning", "Evening", "Lunch", "Weekend", "Quick", "Full", "Recovery", "Intense", "Light", "Power" };
        var activities = new[] { "Workout", "Training", "Session", "Routine", "Exercise", "Practice", "Challenge" };
        return $"{sessionTypes[_random.Next(sessionTypes.Length)]} {activities[_random.Next(activities.Length)]}";
    }

    private static string GenerateRandomDescription()
    {
        var descriptions = new[]
        {
            "A focused workout session to build strength and endurance.",
            "Quick and effective training for busy schedules.",
            "Comprehensive workout covering all major muscle groups.",
            "High-intensity session for maximum calorie burn.",
            "Recovery-focused session for active rest days.",
            "Targeted training for specific fitness goals.",
            "Balanced workout combining strength and cardio."
        };
        return descriptions[_random.Next(descriptions.Length)];
    }
}

/// <summary>
/// Builder for creating WorkoutPool test entities
/// </summary>
public class WorkoutPoolBuilder
{
    private readonly WorkoutPool _pool;
    private static readonly Random _random = new();

    public WorkoutPoolBuilder()
    {
        _pool = new WorkoutPool
        {
            Name = GenerateRandomPoolName(),
            Description = GenerateRandomDescription(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public WorkoutPoolBuilder WithName(string name)
    {
        _pool.Name = name;
        return this;
    }

    public WorkoutPoolBuilder WithDescription(string description)
    {
        _pool.Description = description;
        return this;
    }

    public WorkoutPoolBuilder WithId(int id)
    {
        _pool.Id = id;
        return this;
    }

    public WorkoutPoolBuilder CreatedAt(DateTime createdAt)
    {
        _pool.CreatedAt = createdAt;
        return this;
    }

    public WorkoutPoolBuilder UpdatedAt(DateTime updatedAt)
    {
        _pool.UpdatedAt = updatedAt;
        return this;
    }

    public WorkoutPool Build() => _pool;

    // Static factory methods for common scenarios
    public static WorkoutPoolBuilder Beginner() => new WorkoutPoolBuilder().WithName("Beginner Pool").WithDescription("Perfect for those starting their fitness journey");
    public static WorkoutPoolBuilder Intermediate() => new WorkoutPoolBuilder().WithName("Intermediate Pool").WithDescription("For those with some fitness experience");
    public static WorkoutPoolBuilder Advanced() => new WorkoutPoolBuilder().WithName("Advanced Pool").WithDescription("Challenging workouts for experienced athletes");
    public static WorkoutPoolBuilder FullBody() => new WorkoutPoolBuilder().WithName("Full Body Pool").WithDescription("Complete workouts targeting all muscle groups");
    public static WorkoutPoolBuilder Cardio() => new WorkoutPoolBuilder().WithName("Cardio Pool").WithDescription("Heart-pumping cardiovascular exercises");
    public static WorkoutPoolBuilder Strength() => new WorkoutPoolBuilder().WithName("Strength Pool").WithDescription("Muscle-building strength training exercises");

    private static string GenerateRandomPoolName()
    {
        var poolTypes = new[] { "Beginner", "Intermediate", "Advanced", "Full Body", "Upper Body", "Lower Body", "Cardio", "Strength", "HIIT", "Yoga" };
        var suffixes = new[] { "Pool", "Collection", "Set", "Routine", "Program", "Series", "Circuit" };
        return $"{poolTypes[_random.Next(poolTypes.Length)]} {suffixes[_random.Next(suffixes.Length)]}";
    }

    private static string GenerateRandomDescription()
    {
        var descriptions = new[]
        {
            "A carefully curated collection of exercises for optimal results.",
            "Diverse workout selection to keep your routine interesting.",
            "Progressive exercises designed to challenge and improve fitness.",
            "Balanced mix of exercises targeting different muscle groups.",
            "Scientifically designed workout combinations for maximum effectiveness.",
            "Versatile exercise collection suitable for various fitness levels.",
            "Comprehensive workout pool for complete fitness development."
        };
        return descriptions[_random.Next(descriptions.Length)];
    }
}

/// <summary>
/// Builder for creating Action test entities
/// </summary>
public class ActionBuilder
{
    private readonly Core.Models.Action _action;
    private static readonly Random _random = new();

    public ActionBuilder()
    {
        _action = new Core.Models.Action
        {
            Description = GenerateRandomActionDescription(),
            PointValue = GenerateRandomPointValue(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public ActionBuilder WithDescription(string description)
    {
        _action.Description = description;
        return this;
    }

    public ActionBuilder WithPointValue(int points)
    {
        _action.PointValue = points;
        return this;
    }

    public ActionBuilder WithId(int id)
    {
        _action.Id = id;
        return this;
    }

    public ActionBuilder CreatedAt(DateTime createdAt)
    {
        _action.CreatedAt = createdAt;
        return this;
    }

    public ActionBuilder UpdatedAt(DateTime updatedAt)
    {
        _action.UpdatedAt = updatedAt;
        return this;
    }

    public Core.Models.Action Build() => _action;

    // Static factory methods for common scenarios
    public static ActionBuilder LowValue() => new ActionBuilder().WithPointValue(5);
    public static ActionBuilder MediumValue() => new ActionBuilder().WithPointValue(10);
    public static ActionBuilder HighValue() => new ActionBuilder().WithPointValue(20);
    public static ActionBuilder WithPoints(int points) => new ActionBuilder().WithPointValue(points);

    private static string GenerateRandomActionDescription()
    {
        var actions = new[] { "Complete", "Perform", "Finish", "Execute", "Do", "Accomplish", "Achieve" };
        var numbers = new[] { "5", "10", "15", "20", "25", "30", "50" };
        var exercises = new[] { "push-ups", "squats", "burpees", "sit-ups", "jumping jacks", "lunges", "planks", "pull-ups", "dips", "crunches" };
        return $"{actions[_random.Next(actions.Length)]} {numbers[_random.Next(numbers.Length)]} {exercises[_random.Next(exercises.Length)]}";
    }

    private static int GenerateRandomPointValue()
    {
        var pointValues = new[] { 5, 8, 10, 12, 15, 18, 20, 25 };
        return pointValues[_random.Next(pointValues.Length)];
    }
}

/// <summary>
/// Builder for creating WorkoutPoolWorkout relationship entities
/// </summary>
public class WorkoutPoolWorkoutBuilder
{
    private readonly WorkoutPoolWorkout _relationship;

    public WorkoutPoolWorkoutBuilder()
    {
        _relationship = new WorkoutPoolWorkout();
    }

    public WorkoutPoolWorkoutBuilder WithWorkoutPool(int workoutPoolId)
    {
        _relationship.WorkoutPoolId = workoutPoolId;
        return this;
    }

    public WorkoutPoolWorkoutBuilder WithWorkoutPool(WorkoutPool workoutPool)
    {
        _relationship.WorkoutPoolId = workoutPool.Id;
        _relationship.WorkoutPool = workoutPool;
        return this;
    }

    public WorkoutPoolWorkoutBuilder WithWorkout(int workoutId)
    {
        _relationship.WorkoutId = workoutId;
        return this;
    }

    public WorkoutPoolWorkoutBuilder WithWorkout(Workout workout)
    {
        _relationship.WorkoutId = workout.Id;
        _relationship.Workout = workout;
        return this;
    }

    public WorkoutPoolWorkout Build() => _relationship;
}

/// <summary>
/// Builder for creating ActionCompletion entities
/// </summary>
public class ActionCompletionBuilder
{
    private readonly ActionCompletion _completion;

    public ActionCompletionBuilder()
    {
        _completion = new ActionCompletion
        {
            CompletedAt = DateTime.UtcNow
        };
    }

    public ActionCompletionBuilder WithSession(int sessionId)
    {
        _completion.SessionId = sessionId;
        return this;
    }

    public ActionCompletionBuilder WithSession(Session session)
    {
        _completion.SessionId = session.Id;
        _completion.Session = session;
        return this;
    }

    public ActionCompletionBuilder WithAction(int actionId)
    {
        _completion.ActionId = actionId;
        return this;
    }

    public ActionCompletionBuilder WithAction(Core.Models.Action action)
    {
        _completion.ActionId = action.Id;
        _completion.Action = action;
        return this;
    }

    public ActionCompletionBuilder WithPointsAwarded(int points)
    {
        _completion.PointsAwarded = points;
        return this;
    }

    public ActionCompletionBuilder CompletedAt(DateTime completedAt)
    {
        _completion.CompletedAt = completedAt;
        return this;
    }

    public ActionCompletion Build() => _completion;
}

/// <summary>
/// Builder for creating WorkoutReceived entities
/// </summary>
public class WorkoutReceivedBuilder
{
    private readonly WorkoutReceived _received;

    public WorkoutReceivedBuilder()
    {
        _received = new WorkoutReceived
        {
            ReceivedAt = DateTime.UtcNow
        };
    }

    public WorkoutReceivedBuilder WithSession(int sessionId)
    {
        _received.SessionId = sessionId;
        return this;
    }

    public WorkoutReceivedBuilder WithSession(Session session)
    {
        _received.SessionId = session.Id;
        _received.Session = session;
        return this;
    }

    public WorkoutReceivedBuilder WithWorkout(int workoutId)
    {
        _received.WorkoutId = workoutId;
        return this;
    }

    public WorkoutReceivedBuilder WithWorkout(Workout workout)
    {
        _received.WorkoutId = workout.Id;
        _received.Workout = workout;
        return this;
    }

    public WorkoutReceivedBuilder WithPointsSpent(int points)
    {
        _received.PointsSpent = points;
        return this;
    }

    public WorkoutReceivedBuilder ReceivedAt(DateTime receivedAt)
    {
        _received.ReceivedAt = receivedAt;
        return this;
    }

    public WorkoutReceived Build() => _received;
}
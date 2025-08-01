using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Tests.TestData;

/// <summary>
/// Provides realistic test data sets that mirror production usage patterns
/// </summary>
public static class TestDataSets
{
    /// <summary>
    /// Creates a realistic set of beginner workouts
    /// </summary>
    public static List<Workout> BeginnerWorkouts => new()
    {
        TestDataBuilder.Workout()
            .WithName("Wall Push-ups")
            .WithDescription("Perfect for building upper body strength for beginners")
            .WithInstructions("Stand arm's length from wall, place palms flat against wall, push body toward wall and back")
            .WithDuration(5)
            .WithDifficulty(DifficultyLevel.Beginner)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Chair Squats")
            .WithDescription("Safe way to build leg strength using a chair for support")
            .WithInstructions("Stand in front of chair, lower body until you touch the chair, then stand back up")
            .WithDuration(8)
            .WithDifficulty(DifficultyLevel.Beginner)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Modified Plank")
            .WithDescription("Core strengthening exercise on knees")
            .WithInstructions("Start on hands and knees, hold straight line from head to knees")
            .WithDuration(3)
            .WithDifficulty(DifficultyLevel.Beginner)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Marching in Place")
            .WithDescription("Low-impact cardio exercise")
            .WithInstructions("Lift knees alternately while standing in place, swing arms naturally")
            .WithDuration(10)
            .WithDifficulty(DifficultyLevel.Beginner)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Seated Leg Extensions")
            .WithDescription("Strengthen quadriceps while seated")
            .WithInstructions("Sit in chair, extend one leg straight out, hold, then lower slowly")
            .WithDuration(6)
            .WithDifficulty(DifficultyLevel.Beginner)
            .Build()
    };

    /// <summary>
    /// Creates a realistic set of intermediate workouts
    /// </summary>
    public static List<Workout> IntermediateWorkouts => new()
    {
        TestDataBuilder.Workout()
            .WithName("Standard Push-ups")
            .WithDescription("Classic upper body strengthening exercise")
            .WithInstructions("Start in plank position, lower chest to floor, push back up maintaining straight line")
            .WithDuration(15)
            .WithDifficulty(DifficultyLevel.Intermediate)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Bodyweight Squats")
            .WithDescription("Full range of motion squats for leg strength")
            .WithInstructions("Stand with feet shoulder-width apart, lower until thighs parallel to floor, return to standing")
            .WithDuration(12)
            .WithDifficulty(DifficultyLevel.Intermediate)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Standard Plank")
            .WithDescription("Core stability exercise on toes")
            .WithInstructions("Hold straight line from head to heels, engage core, breathe normally")
            .WithDuration(8)
            .WithDifficulty(DifficultyLevel.Intermediate)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Jumping Jacks")
            .WithDescription("Full-body cardio exercise")
            .WithInstructions("Jump feet apart while raising arms overhead, return to starting position")
            .WithDuration(10)
            .WithDifficulty(DifficultyLevel.Intermediate)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Lunges")
            .WithDescription("Single-leg strengthening exercise")
            .WithInstructions("Step forward into lunge position, lower back knee toward floor, return to standing")
            .WithDuration(18)
            .WithDifficulty(DifficultyLevel.Intermediate)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Mountain Climbers")
            .WithDescription("Dynamic core and cardio exercise")
            .WithInstructions("Start in plank, alternate bringing knees to chest in running motion")
            .WithDuration(12)
            .WithDifficulty(DifficultyLevel.Intermediate)
            .Build()
    };

    /// <summary>
    /// Creates a realistic set of advanced workouts
    /// </summary>
    public static List<Workout> AdvancedWorkouts => new()
    {
        TestDataBuilder.Workout()
            .WithName("Burpees")
            .WithDescription("High-intensity full-body exercise")
            .WithInstructions("Squat down, jump back to plank, do push-up, jump feet to hands, jump up with arms overhead")
            .WithDuration(20)
            .WithDifficulty(DifficultyLevel.Advanced)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Pull-ups")
            .WithDescription("Upper body pulling exercise")
            .WithInstructions("Hang from bar with overhand grip, pull body up until chin clears bar, lower with control")
            .WithDuration(15)
            .WithDifficulty(DifficultyLevel.Advanced)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Single-Leg Squats")
            .WithDescription("Advanced unilateral leg strengthening")
            .WithInstructions("Stand on one leg, lower into squat position, return to standing without touching other foot down")
            .WithDuration(25)
            .WithDifficulty(DifficultyLevel.Advanced)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("Handstand Push-ups")
            .WithDescription("Advanced upper body and core exercise")
            .WithInstructions("Kick up to handstand against wall, lower head toward floor, push back up")
            .WithDuration(18)
            .WithDifficulty(DifficultyLevel.Advanced)
            .Build(),

        TestDataBuilder.Workout()
            .WithName("HIIT Circuit")
            .WithDescription("High-intensity interval training combination")
            .WithInstructions("Perform 30 seconds each: burpees, mountain climbers, jump squats, rest 30 seconds, repeat")
            .WithDuration(30)
            .WithDifficulty(DifficultyLevel.Advanced)
            .Build()
    };

    /// <summary>
    /// Creates a realistic set of actions with varied point values
    /// </summary>
    public static List<Core.Models.Action> RealisticActions => new()
    {
        // Low-value actions (5-8 points)
        TestDataBuilder.Action()
            .WithDescription("Complete 5 wall push-ups")
            .WithPointValue(5)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("Hold plank for 15 seconds")
            .WithPointValue(6)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("Do 10 chair squats")
            .WithPointValue(7)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("March in place for 1 minute")
            .WithPointValue(8)
            .Build(),

        // Medium-value actions (10-15 points)
        TestDataBuilder.Action()
            .WithDescription("Complete 10 standard push-ups")
            .WithPointValue(10)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("Do 20 bodyweight squats")
            .WithPointValue(12)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("Hold plank for 45 seconds")
            .WithPointValue(13)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("Complete 15 lunges per leg")
            .WithPointValue(15)
            .Build(),

        // High-value actions (18-25 points)
        TestDataBuilder.Action()
            .WithDescription("Complete 5 burpees")
            .WithPointValue(18)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("Do 3 pull-ups")
            .WithPointValue(20)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("Complete 10 single-leg squats")
            .WithPointValue(22)
            .Build(),

        TestDataBuilder.Action()
            .WithDescription("Finish full HIIT circuit")
            .WithPointValue(25)
            .Build()
    };

    /// <summary>
    /// Creates realistic workout pools with appropriate workout combinations
    /// </summary>
    public static List<WorkoutPool> RealisticWorkoutPools => new()
    {
        TestDataBuilder.Pool()
            .WithName("Beginner's Journey")
            .WithDescription("Perfect starting point for fitness newcomers with gentle, effective exercises")
            .Build(),

        TestDataBuilder.Pool()
            .WithName("Office Worker Special")
            .WithDescription("Quick exercises designed for busy professionals with limited time")
            .Build(),

        TestDataBuilder.Pool()
            .WithName("Strength Builder")
            .WithDescription("Progressive strength training exercises to build muscle and power")
            .Build(),

        TestDataBuilder.Pool()
            .WithName("Cardio Blast")
            .WithDescription("Heart-pumping exercises to improve cardiovascular fitness")
            .Build(),

        TestDataBuilder.Pool()
            .WithName("Core Focus")
            .WithDescription("Targeted exercises to strengthen and stabilize your core")
            .Build(),

        TestDataBuilder.Pool()
            .WithName("Full Body Challenge")
            .WithDescription("Comprehensive workouts targeting all major muscle groups")
            .Build(),

        TestDataBuilder.Pool()
            .WithName("HIIT Intensity")
            .WithDescription("High-intensity interval training for maximum calorie burn")
            .Build(),

        TestDataBuilder.Pool()
            .WithName("Flexibility & Mobility")
            .WithDescription("Gentle movements to improve flexibility and joint mobility")
            .Build()
    };

    /// <summary>
    /// Creates a complete realistic dataset for comprehensive testing
    /// </summary>
    public static class CompleteDataset
    {
        public static List<Workout> AllWorkouts => 
            BeginnerWorkouts
            .Concat(IntermediateWorkouts)
            .Concat(AdvancedWorkouts)
            .ToList();

        public static List<Core.Models.Action> AllActions => RealisticActions;

        public static List<WorkoutPool> AllPools => RealisticWorkoutPools;

        /// <summary>
        /// Creates realistic pool-workout relationships
        /// </summary>
        public static List<(string PoolName, List<string> WorkoutNames)> PoolWorkoutMappings => new()
        {
            ("Beginner's Journey", new List<string> 
            { 
                "Wall Push-ups", "Chair Squats", "Modified Plank", "Marching in Place", "Seated Leg Extensions" 
            }),
            
            ("Office Worker Special", new List<string> 
            { 
                "Wall Push-ups", "Chair Squats", "Seated Leg Extensions", "Standard Push-ups", "Bodyweight Squats" 
            }),
            
            ("Strength Builder", new List<string> 
            { 
                "Standard Push-ups", "Bodyweight Squats", "Lunges", "Pull-ups", "Single-Leg Squats" 
            }),
            
            ("Cardio Blast", new List<string> 
            { 
                "Jumping Jacks", "Mountain Climbers", "Burpees", "HIIT Circuit", "Marching in Place" 
            }),
            
            ("Core Focus", new List<string> 
            { 
                "Modified Plank", "Standard Plank", "Mountain Climbers", "Burpees" 
            }),
            
            ("Full Body Challenge", new List<string> 
            { 
                "Standard Push-ups", "Bodyweight Squats", "Lunges", "Mountain Climbers", "Burpees", "Pull-ups" 
            }),
            
            ("HIIT Intensity", new List<string> 
            { 
                "Burpees", "Mountain Climbers", "Jumping Jacks", "HIIT Circuit" 
            }),
            
            ("Flexibility & Mobility", new List<string> 
            { 
                "Marching in Place", "Seated Leg Extensions", "Modified Plank" 
            })
        };
    }

    /// <summary>
    /// Creates test scenarios for different user personas
    /// </summary>
    public static class UserPersonas
    {
        public static class BeginnerUser
        {
            public static List<string> PreferredPools => new() { "Beginner's Journey", "Office Worker Special", "Flexibility & Mobility" };
            public static List<string> AvoidedPools => new() { "HIIT Intensity", "Full Body Challenge" };
            public static int TypicalPointsPerSession => 25;
            public static int TypicalActionsPerSession => 4;
        }

        public static class IntermediateUser
        {
            public static List<string> PreferredPools => new() { "Strength Builder", "Cardio Blast", "Core Focus", "Full Body Challenge" };
            public static List<string> AvoidedPools => new() { "Beginner's Journey" };
            public static int TypicalPointsPerSession => 50;
            public static int TypicalActionsPerSession => 6;
        }

        public static class AdvancedUser
        {
            public static List<string> PreferredPools => new() { "HIIT Intensity", "Full Body Challenge", "Strength Builder" };
            public static List<string> AvoidedPools => new() { "Beginner's Journey", "Flexibility & Mobility" };
            public static int TypicalPointsPerSession => 75;
            public static int TypicalActionsPerSession => 8;
        }
    }
}
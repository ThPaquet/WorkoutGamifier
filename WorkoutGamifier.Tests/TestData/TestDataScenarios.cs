using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Tests.TestData;

/// <summary>
/// Provides comprehensive test data scenarios for different testing needs
/// </summary>
public static class TestDataScenarios
{
    /// <summary>
    /// Creates a complete beginner user scenario with appropriate workouts and actions
    /// </summary>
    public static class BeginnerUserScenario
    {
        public static List<Workout> GetWorkouts() => new()
        {
            WorkoutBuilder.Beginner()
                .WithName("Wall Push-ups")
                .WithDescription("Perfect for building upper body strength for beginners")
                .WithDuration(5)
                .Build(),

            WorkoutBuilder.Beginner()
                .WithName("Chair Squats")
                .WithDescription("Safe way to build leg strength using a chair for support")
                .WithDuration(8)
                .Build(),

            WorkoutBuilder.Beginner()
                .WithName("Modified Plank")
                .WithDescription("Core strengthening exercise on knees")
                .WithDuration(3)
                .Build(),

            WorkoutBuilder.Beginner()
                .WithName("Marching in Place")
                .WithDescription("Low-impact cardio exercise")
                .WithDuration(10)
                .Build()
        };

        public static List<Core.Models.Action> GetActions() => new()
        {
            ActionBuilder.LowValue()
                .WithDescription("Complete 5 wall push-ups")
                .WithPointValue(5)
                .Build(),

            ActionBuilder.LowValue()
                .WithDescription("Hold modified plank for 15 seconds")
                .WithPointValue(6)
                .Build(),

            ActionBuilder.LowValue()
                .WithDescription("Do 10 chair squats")
                .WithPointValue(7)
                .Build(),

            ActionBuilder.LowValue()
                .WithDescription("March in place for 1 minute")
                .WithPointValue(8)
                .Build()
        };

        public static WorkoutPool GetWorkoutPool() =>
            WorkoutPoolBuilder.Beginner()
                .WithName("Beginner's Journey")
                .WithDescription("Perfect starting point for fitness newcomers with gentle, effective exercises")
                .Build();

        public static Session GetTypicalSession() =>
            SessionBuilder.Active()
                .WithName("Morning Beginner Session")
                .WithDescription("Gentle morning workout to start the day")
                .WithPointsEarned(25)
                .WithPointsSpent(0)
                .Build();
    }

    /// <summary>
    /// Creates an intermediate user scenario with balanced workouts and actions
    /// </summary>
    public static class IntermediateUserScenario
    {
        public static List<Workout> GetWorkouts() => new()
        {
            WorkoutBuilder.Intermediate()
                .WithName("Standard Push-ups")
                .WithDescription("Classic upper body strengthening exercise")
                .WithDuration(15)
                .Build(),

            WorkoutBuilder.Intermediate()
                .WithName("Bodyweight Squats")
                .WithDescription("Full range of motion squats for leg strength")
                .WithDuration(12)
                .Build(),

            WorkoutBuilder.Intermediate()
                .WithName("Standard Plank")
                .WithDescription("Core stability exercise on toes")
                .WithDuration(8)
                .Build(),

            WorkoutBuilder.Intermediate()
                .WithName("Jumping Jacks")
                .WithDescription("Full-body cardio exercise")
                .WithDuration(10)
                .Build(),

            WorkoutBuilder.Intermediate()
                .WithName("Lunges")
                .WithDescription("Single-leg strengthening exercise")
                .WithDuration(18)
                .Build()
        };

        public static List<Core.Models.Action> GetActions() => new()
        {
            ActionBuilder.MediumValue()
                .WithDescription("Complete 10 standard push-ups")
                .WithPointValue(10)
                .Build(),

            ActionBuilder.MediumValue()
                .WithDescription("Do 20 bodyweight squats")
                .WithPointValue(12)
                .Build(),

            ActionBuilder.MediumValue()
                .WithDescription("Hold plank for 45 seconds")
                .WithPointValue(13)
                .Build(),

            ActionBuilder.MediumValue()
                .WithDescription("Complete 15 lunges per leg")
                .WithPointValue(15)
                .Build(),

            ActionBuilder.MediumValue()
                .WithDescription("Do 30 jumping jacks")
                .WithPointValue(11)
                .Build()
        };

        public static WorkoutPool GetWorkoutPool() =>
            WorkoutPoolBuilder.Intermediate()
                .WithName("Strength Builder")
                .WithDescription("Progressive strength training exercises to build muscle and power")
                .Build();

        public static Session GetTypicalSession() =>
            SessionBuilder.Active()
                .WithName("Lunch Break Workout")
                .WithDescription("Efficient workout for busy schedules")
                .WithPointsEarned(50)
                .WithPointsSpent(15)
                .Build();
    }

    /// <summary>
    /// Creates an advanced user scenario with challenging workouts and high-value actions
    /// </summary>
    public static class AdvancedUserScenario
    {
        public static List<Workout> GetWorkouts() => new()
        {
            WorkoutBuilder.Advanced()
                .WithName("Burpees")
                .WithDescription("High-intensity full-body exercise")
                .WithDuration(20)
                .Build(),

            WorkoutBuilder.Advanced()
                .WithName("Pull-ups")
                .WithDescription("Upper body pulling exercise")
                .WithDuration(15)
                .Build(),

            WorkoutBuilder.Advanced()
                .WithName("Single-Leg Squats")
                .WithDescription("Advanced unilateral leg strengthening")
                .WithDuration(25)
                .Build(),

            WorkoutBuilder.Advanced()
                .WithName("HIIT Circuit")
                .WithDescription("High-intensity interval training combination")
                .WithDuration(30)
                .Build()
        };

        public static List<Core.Models.Action> GetActions() => new()
        {
            ActionBuilder.HighValue()
                .WithDescription("Complete 5 burpees")
                .WithPointValue(18)
                .Build(),

            ActionBuilder.HighValue()
                .WithDescription("Do 3 pull-ups")
                .WithPointValue(20)
                .Build(),

            ActionBuilder.HighValue()
                .WithDescription("Complete 10 single-leg squats")
                .WithPointValue(22)
                .Build(),

            ActionBuilder.HighValue()
                .WithDescription("Finish full HIIT circuit")
                .WithPointValue(25)
                .Build()
        };

        public static WorkoutPool GetWorkoutPool() =>
            WorkoutPoolBuilder.Advanced()
                .WithName("HIIT Intensity")
                .WithDescription("High-intensity interval training for maximum calorie burn")
                .Build();

        public static Session GetTypicalSession() =>
            SessionBuilder.Active()
                .WithName("Evening Power Session")
                .WithDescription("High-intensity workout for experienced athletes")
                .WithPointsEarned(75)
                .WithPointsSpent(30)
                .Build();
    }

    /// <summary>
    /// Creates edge case scenarios for testing boundary conditions
    /// </summary>
    public static class EdgeCaseScenarios
    {
        public static Workout GetMinimalWorkout() =>
            TestDataBuilder.Workout()
                .WithName("A")
                .WithDescription("B")
                .WithDuration(1)
                .WithDifficulty(DifficultyLevel.Beginner)
                .Build();

        public static Workout GetMaximalWorkout() =>
            TestDataBuilder.Workout()
                .WithName(new string('A', 100)) // Max length
                .WithDescription(new string('B', 500)) // Max length
                .WithDuration(120) // 2 hours
                .WithDifficulty(DifficultyLevel.Advanced)
                .Build();

        public static Session GetZeroPointSession() =>
            TestDataBuilder.Session()
                .WithName("Zero Point Session")
                .WithPointsEarned(0)
                .WithPointsSpent(0)
                .Build();

        public static Session GetHighPointSession() =>
            TestDataBuilder.Session()
                .WithName("High Point Session")
                .WithPointsEarned(1000)
                .WithPointsSpent(500)
                .Build();

        public static Core.Models.Action GetMinimalAction() =>
            TestDataBuilder.Action()
                .WithDescription("A")
                .WithPointValue(1)
                .Build();

        public static Core.Models.Action GetMaximalAction() =>
            TestDataBuilder.Action()
                .WithDescription(new string('A', 200))
                .WithPointValue(100)
                .Build();

        public static Session GetLongRunningSession() =>
            TestDataBuilder.Session()
                .WithName("Marathon Session")
                .WithStartTime(DateTime.UtcNow.AddHours(-8))
                .AsActive()
                .Build();

        public static Session GetVeryOldSession() =>
            TestDataBuilder.Session()
                .WithName("Ancient Session")
                .WithStartTime(DateTime.UtcNow.AddYears(-1))
                .AsCompleted()
                .Build();
    }

    /// <summary>
    /// Creates realistic workflow scenarios for integration testing
    /// </summary>
    public static class WorkflowScenarios
    {
        /// <summary>
        /// Creates a complete session workflow from start to finish
        /// </summary>
        public static class CompleteSessionWorkflow
        {
            public static WorkoutPool Pool => WorkoutPoolBuilder.Intermediate()
                .WithName("Complete Workflow Pool")
                .WithDescription("Pool for testing complete session workflows")
                .Build();

            public static List<Workout> Workouts => new()
            {
                WorkoutBuilder.Intermediate()
                    .WithName("Workflow Workout 1")
                    .WithDescription("First workout in the workflow")
                    .WithDuration(15)
                    .Build(),

                WorkoutBuilder.Intermediate()
                    .WithName("Workflow Workout 2")
                    .WithDescription("Second workout in the workflow")
                    .WithDuration(20)
                    .Build()
            };

            public static List<Core.Models.Action> Actions => new()
            {
                ActionBuilder.MediumValue()
                    .WithDescription("Complete workflow action 1")
                    .WithPointValue(10)
                    .Build(),

                ActionBuilder.MediumValue()
                    .WithDescription("Complete workflow action 2")
                    .WithPointValue(15)
                    .Build(),

                ActionBuilder.HighValue()
                    .WithDescription("Complete workflow action 3")
                    .WithPointValue(20)
                    .Build()
            };

            public static Session StartingSession => TestDataBuilder.Session()
                .WithName("Complete Workflow Session")
                .WithDescription("Session for testing complete workflows")
                .AsActive()
                .WithPointsEarned(0)
                .WithPointsSpent(0)
                .Build();

            public static List<ActionCompletion> ExpectedCompletions(Session session, List<Core.Models.Action> actions) =>
                actions.Select(action => TestDataBuilder.ActionCompletion()
                    .WithSession(session)
                    .WithAction(action)
                    .WithPointsAwarded(action.PointValue)
                    .Build()).ToList();

            public static List<WorkoutReceived> ExpectedWorkouts(Session session, List<Workout> workouts) =>
                workouts.Select(workout => TestDataBuilder.WorkoutReceived()
                    .WithSession(session)
                    .WithWorkout(workout)
                    .WithPointsSpent(10) // Standard cost
                    .Build()).ToList();
        }

        /// <summary>
        /// Creates a pool management workflow scenario
        /// </summary>
        public static class PoolManagementWorkflow
        {
            public static WorkoutPool EmptyPool => TestDataBuilder.Pool()
                .WithName("Empty Pool")
                .WithDescription("Pool with no workouts for testing")
                .Build();

            public static WorkoutPool FullPool => TestDataBuilder.Pool()
                .WithName("Full Pool")
                .WithDescription("Pool with many workouts for testing")
                .Build();

            public static List<Workout> PoolWorkouts => new()
            {
                WorkoutBuilder.Beginner().WithName("Pool Workout 1").Build(),
                WorkoutBuilder.Intermediate().WithName("Pool Workout 2").Build(),
                WorkoutBuilder.Advanced().WithName("Pool Workout 3").Build(),
                WorkoutBuilder.Beginner().WithName("Pool Workout 4").Build(),
                WorkoutBuilder.Intermediate().WithName("Pool Workout 5").Build()
            };

            public static List<WorkoutPoolWorkout> PoolWorkoutRelationships(WorkoutPool pool, List<Workout> workouts) =>
                workouts.Select(workout => TestDataBuilder.PoolWorkout()
                    .WithWorkoutPool(pool)
                    .WithWorkout(workout)
                    .Build()).ToList();
        }
    }

    /// <summary>
    /// Creates performance testing scenarios with large datasets
    /// </summary>
    public static class PerformanceScenarios
    {
        public static List<Workout> GetLargeWorkoutSet(int count = 100) =>
            Enumerable.Range(1, count)
                .Select(i => TestDataBuilder.Workout()
                    .WithName($"Performance Workout {i}")
                    .WithDescription($"Workout {i} for performance testing")
                    .WithDuration(15 + (i % 45)) // Vary duration
                    .WithDifficulty((DifficultyLevel)((i % 3) + 1)) // Cycle through difficulties
                    .Build())
                .ToList();

        public static List<Core.Models.Action> GetLargeActionSet(int count = 50) =>
            Enumerable.Range(1, count)
                .Select(i => TestDataBuilder.Action()
                    .WithDescription($"Performance Action {i}")
                    .WithPointValue(5 + (i % 20)) // Vary point values
                    .Build())
                .ToList();

        public static List<Session> GetLargeSessionSet(int count = 20) =>
            Enumerable.Range(1, count)
                .Select(i => TestDataBuilder.Session()
                    .WithName($"Performance Session {i}")
                    .WithDescription($"Session {i} for performance testing")
                    .WithStartTime(DateTime.UtcNow.AddDays(-i))
                    .WithPointsEarned(i * 10)
                    .WithPointsSpent(i * 5)
                    .Build())
                .ToList();

        public static List<WorkoutPool> GetLargePoolSet(int count = 10) =>
            Enumerable.Range(1, count)
                .Select(i => TestDataBuilder.Pool()
                    .WithName($"Performance Pool {i}")
                    .WithDescription($"Pool {i} for performance testing")
                    .Build())
                .ToList();
    }

    /// <summary>
    /// Creates data consistency scenarios for testing referential integrity
    /// </summary>
    public static class DataConsistencyScenarios
    {
        public static class ValidRelationships
        {
            public static (WorkoutPool Pool, List<Workout> Workouts, List<WorkoutPoolWorkout> Relationships) GetValidPoolWorkoutRelationships()
            {
                var pool = TestDataBuilder.Pool()
                    .WithName("Consistency Test Pool")
                    .Build();

                var workouts = new List<Workout>
                {
                    TestDataBuilder.Workout().WithName("Consistency Workout 1").Build(),
                    TestDataBuilder.Workout().WithName("Consistency Workout 2").Build(),
                    TestDataBuilder.Workout().WithName("Consistency Workout 3").Build()
                };

                var relationships = workouts.Select(w => TestDataBuilder.PoolWorkout()
                    .WithWorkoutPool(pool)
                    .WithWorkout(w)
                    .Build()).ToList();

                return (pool, workouts, relationships);
            }

            public static (Session Session, List<Core.Models.Action> Actions, List<ActionCompletion> Completions) GetValidSessionActionCompletions()
            {
                var session = TestDataBuilder.Session()
                    .WithName("Consistency Test Session")
                    .Build();

                var actions = new List<Core.Models.Action>
                {
                    TestDataBuilder.Action().WithDescription("Consistency Action 1").WithPointValue(10).Build(),
                    TestDataBuilder.Action().WithDescription("Consistency Action 2").WithPointValue(15).Build(),
                    TestDataBuilder.Action().WithDescription("Consistency Action 3").WithPointValue(20).Build()
                };

                var completions = actions.Select(a => TestDataBuilder.ActionCompletion()
                    .WithSession(session)
                    .WithAction(a)
                    .WithPointsAwarded(a.PointValue)
                    .Build()).ToList();

                return (session, actions, completions);
            }
        }
    }
}
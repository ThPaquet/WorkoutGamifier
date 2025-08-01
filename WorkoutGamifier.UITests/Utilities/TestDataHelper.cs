namespace WorkoutGamifier.UITests.Utilities;

public class TestDataHelper
{
    private readonly Random _random = new();

    public async Task SetupMinimalTestData()
    {
        // This will be expanded when we integrate with the actual app database
        // For now, we'll rely on the app's built-in data seeding
        await Task.CompletedTask;
    }

    public async Task CleanupTestData()
    {
        // This will be implemented to clean up test-specific data
        // For now, we rely on app reset between tests
        await Task.CompletedTask;
    }

    /// <summary>
    /// Creates a realistic test data scenario for UI testing
    /// </summary>
    public UITestScenario CreateRealisticScenario()
    {
        return new UITestScenario
        {
            SessionName = GenerateRandomSessionName(),
            PoolName = GenerateRandomPoolName(),
            WorkoutNames = GenerateRealisticWorkoutNames(),
            ActionDescriptions = GenerateRealisticActionDescriptions(),
            ExpectedPoints = CalculateExpectedPoints()
        };
    }

    /// <summary>
    /// Creates a beginner-focused test scenario
    /// </summary>
    public UITestScenario CreateBeginnerScenario()
    {
        return new UITestScenario
        {
            SessionName = "Beginner Morning Routine",
            PoolName = "Beginner's Journey",
            WorkoutNames = new List<string> { "Wall Push-ups", "Chair Squats", "Modified Plank" },
            ActionDescriptions = new List<string> 
            { 
                "Complete 5 wall push-ups", 
                "Do 10 chair squats", 
                "Hold plank for 15 seconds" 
            },
            ExpectedPoints = 18
        };
    }

    /// <summary>
    /// Creates an advanced test scenario
    /// </summary>
    public UITestScenario CreateAdvancedScenario()
    {
        return new UITestScenario
        {
            SessionName = "Advanced HIIT Challenge",
            PoolName = "HIIT Intensity",
            WorkoutNames = new List<string> { "Burpees", "Mountain Climbers", "HIIT Circuit" },
            ActionDescriptions = new List<string> 
            { 
                "Complete 5 burpees", 
                "Finish full HIIT circuit", 
                "Do 3 pull-ups" 
            },
            ExpectedPoints = 63
        };
    }

    /// <summary>
    /// Creates test scenarios for different user personas
    /// </summary>
    public Dictionary<string, UITestScenario> CreatePersonaScenarios()
    {
        return new Dictionary<string, UITestScenario>
        {
            ["Beginner"] = CreateBeginnerScenario(),
            ["Intermediate"] = new UITestScenario
            {
                SessionName = "Intermediate Strength Session",
                PoolName = "Strength Builder",
                WorkoutNames = new List<string> { "Standard Push-ups", "Bodyweight Squats", "Lunges" },
                ActionDescriptions = new List<string> 
                { 
                    "Complete 10 standard push-ups", 
                    "Do 20 bodyweight squats", 
                    "Complete 15 lunges per leg" 
                },
                ExpectedPoints = 37
            },
            ["Advanced"] = CreateAdvancedScenario()
        };
    }

    private List<string> GenerateRealisticWorkoutNames()
    {
        var workoutSets = new[]
        {
            new[] { "Wall Push-ups", "Chair Squats", "Modified Plank" },
            new[] { "Standard Push-ups", "Bodyweight Squats", "Standard Plank" },
            new[] { "Burpees", "Pull-ups", "HIIT Circuit" },
            new[] { "Jumping Jacks", "Mountain Climbers", "Lunges" }
        };
        
        return workoutSets[_random.Next(workoutSets.Length)].ToList();
    }

    private List<string> GenerateRealisticActionDescriptions()
    {
        var actionSets = new[]
        {
            new[] { "Complete 5 wall push-ups", "Do 10 chair squats", "Hold plank for 15 seconds" },
            new[] { "Complete 10 standard push-ups", "Do 20 bodyweight squats", "Hold plank for 45 seconds" },
            new[] { "Complete 5 burpees", "Do 3 pull-ups", "Finish full HIIT circuit" },
            new[] { "Do 30 jumping jacks", "Complete 20 mountain climbers", "Complete 15 lunges per leg" }
        };
        
        return actionSets[_random.Next(actionSets.Length)].ToList();
    }

    private int CalculateExpectedPoints()
    {
        // Simulate realistic point calculations based on typical action values
        var pointValues = new[] { 5, 6, 7, 8, 10, 12, 13, 15, 18, 20, 22, 25 };
        var actionCount = _random.Next(3, 7);
        var totalPoints = 0;
        
        for (int i = 0; i < actionCount; i++)
        {
            totalPoints += pointValues[_random.Next(pointValues.Length)];
        }
        
        return totalPoints;
    }

    #region Test Data Generators

    public string GenerateRandomWorkoutName()
    {
        var workoutTypes = new[] { "Push-ups", "Squats", "Burpees", "Planks", "Lunges", "Mountain Climbers", "Jumping Jacks" };
        var modifiers = new[] { "Intense", "Quick", "Power", "Endurance", "Strength", "Cardio", "Core" };
        
        return $"{modifiers[_random.Next(modifiers.Length)]} {workoutTypes[_random.Next(workoutTypes.Length)]}";
    }

    public string GenerateRandomSessionName()
    {
        var sessionTypes = new[] { "Morning", "Evening", "Lunch", "Weekend", "Quick", "Full", "Recovery" };
        var activities = new[] { "Workout", "Training", "Session", "Routine", "Exercise" };
        
        return $"{sessionTypes[_random.Next(sessionTypes.Length)]} {activities[_random.Next(activities.Length)]}";
    }

    public string GenerateRandomPoolName()
    {
        var poolTypes = new[] { "Beginner", "Intermediate", "Advanced", "Full Body", "Upper Body", "Lower Body", "Cardio", "Strength" };
        var suffixes = new[] { "Pool", "Collection", "Set", "Routine", "Program" };
        
        return $"{poolTypes[_random.Next(poolTypes.Length)]} {suffixes[_random.Next(suffixes.Length)]}";
    }

    public string GenerateRandomActionDescription()
    {
        var actions = new[] { "Complete", "Perform", "Finish", "Execute", "Do" };
        var numbers = new[] { "10", "15", "20", "25", "30" };
        var exercises = new[] { "push-ups", "squats", "burpees", "sit-ups", "jumping jacks", "lunges" };
        
        return $"{actions[_random.Next(actions.Length)]} {numbers[_random.Next(numbers.Length)]} {exercises[_random.Next(exercises.Length)]}";
    }

    public int GenerateRandomPointValue()
    {
        var pointValues = new[] { 5, 10, 15, 20, 25 };
        return pointValues[_random.Next(pointValues.Length)];
    }

    public int GenerateRandomDuration()
    {
        var durations = new[] { 5, 10, 15, 20, 25, 30, 45, 60 };
        return durations[_random.Next(durations.Length)];
    }

    public string GenerateRandomDescription()
    {
        var descriptions = new[]
        {
            "A great exercise for building strength and endurance.",
            "Perfect for improving cardiovascular health.",
            "Excellent for core strengthening and stability.",
            "Ideal for building muscle and burning calories.",
            "Great for improving flexibility and balance.",
            "Perfect for a quick and effective workout."
        };
        
        return descriptions[_random.Next(descriptions.Length)];
    }

    #endregion

    #region Validation Helpers

    public bool IsValidWorkoutName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length >= 3 && name.Length <= 100;
    }

    public bool IsValidSessionName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length >= 3 && name.Length <= 100;
    }

    public bool IsValidPoolName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Length >= 3 && name.Length <= 100;
    }

    public bool IsValidActionDescription(string description)
    {
        return !string.IsNullOrWhiteSpace(description) && description.Length >= 5 && description.Length <= 200;
    }

    public bool IsValidPointValue(int points)
    {
        return points > 0 && points <= 100;
    }

    public bool IsValidDuration(int minutes)
    {
        return minutes > 0 && minutes <= 180; // Max 3 hours
    }

    #endregion

    #region Test Scenarios

    public class TestScenario
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
    }

    public TestScenario CreateWorkoutCreationScenario()
    {
        return new TestScenario
        {
            Name = "Create New Workout",
            Description = "Test scenario for creating a new workout",
            Data = new Dictionary<string, object>
            {
                ["WorkoutName"] = GenerateRandomWorkoutName(),
                ["Description"] = GenerateRandomDescription(),
                ["Duration"] = GenerateRandomDuration(),
                ["Difficulty"] = new[] { "Beginner", "Intermediate", "Advanced" }[_random.Next(3)]
            }
        };
    }

    public TestScenario CreateSessionScenario()
    {
        return new TestScenario
        {
            Name = "Create and Complete Session",
            Description = "Test scenario for creating and completing a workout session",
            Data = new Dictionary<string, object>
            {
                ["SessionName"] = GenerateRandomSessionName(),
                ["Description"] = GenerateRandomDescription(),
                ["ActionsToComplete"] = _random.Next(2, 6),
                ["PointsToEarn"] = GenerateRandomPointValue() * 3
            }
        };
    }

    public TestScenario CreatePoolManagementScenario()
    {
        return new TestScenario
        {
            Name = "Manage Workout Pool",
            Description = "Test scenario for creating and managing workout pools",
            Data = new Dictionary<string, object>
            {
                ["PoolName"] = GenerateRandomPoolName(),
                ["Description"] = GenerateRandomDescription(),
                ["WorkoutsToAdd"] = _random.Next(2, 5),
                ["WorkoutNames"] = Enumerable.Range(0, 5).Select(_ => GenerateRandomWorkoutName()).ToList()
            }
        };
    }

    #endregion
}
  
  /// <summary>
    /// Represents a complete UI test scenario with all necessary data
    /// </summary>
    public class UITestScenario
    {
        public string SessionName { get; set; } = string.Empty;
        public string PoolName { get; set; } = string.Empty;
        public List<string> WorkoutNames { get; set; } = new();
        public List<string> ActionDescriptions { get; set; } = new();
        public int ExpectedPoints { get; set; }
        public Dictionary<string, object> AdditionalData { get; set; } = new();
    }

    /// <summary>
    /// Creates test data for form validation scenarios
    /// </summary>
    public class FormValidationTestData
    {
        public static Dictionary<string, string> ValidWorkoutData => new()
        {
            ["Name"] = "Test Workout",
            ["Description"] = "A test workout for validation",
            ["Instructions"] = "Follow the instructions carefully",
            ["Duration"] = "15",
            ["Difficulty"] = "Intermediate"
        };

        public static Dictionary<string, string> InvalidWorkoutData => new()
        {
            ["Name"] = "", // Invalid: empty name
            ["Description"] = "A", // Invalid: too short
            ["Instructions"] = "",
            ["Duration"] = "0", // Invalid: zero duration
            ["Difficulty"] = "Invalid" // Invalid: not a valid difficulty
        };

        public static Dictionary<string, string> ValidSessionData => new()
        {
            ["Name"] = "Test Session",
            ["Description"] = "A test session for validation",
            ["PoolName"] = "Test Pool"
        };

        public static Dictionary<string, string> InvalidSessionData => new()
        {
            ["Name"] = "", // Invalid: empty name
            ["Description"] = "",
            ["PoolName"] = "" // Invalid: no pool selected
        };

        public static Dictionary<string, string> ValidPoolData => new()
        {
            ["Name"] = "Test Pool",
            ["Description"] = "A test pool for validation"
        };

        public static Dictionary<string, string> InvalidPoolData => new()
        {
            ["Name"] = "", // Invalid: empty name
            ["Description"] = ""
        };

        public static Dictionary<string, string> ValidActionData => new()
        {
            ["Description"] = "Complete 10 test exercises",
            ["PointValue"] = "10"
        };

        public static Dictionary<string, string> InvalidActionData => new()
        {
            ["Description"] = "", // Invalid: empty description
            ["PointValue"] = "0" // Invalid: zero points
        };
    }

    /// <summary>
    /// Provides edge case test data for boundary testing
    /// </summary>
    public class EdgeCaseTestData
    {
        public static class StringLimits
        {
            public static string MinValidName => "ABC"; // 3 characters
            public static string MaxValidName => new string('A', 100); // 100 characters
            public static string TooShortName => "AB"; // 2 characters
            public static string TooLongName => new string('A', 101); // 101 characters
            
            public static string MinValidDescription => "ABCDE"; // 5 characters
            public static string MaxValidDescription => new string('A', 500); // 500 characters
            public static string TooShortDescription => "ABCD"; // 4 characters
            public static string TooLongDescription => new string('A', 501); // 501 characters
        }

        public static class NumericLimits
        {
            public static int MinValidDuration => 1;
            public static int MaxValidDuration => 180; // 3 hours
            public static int TooShortDuration => 0;
            public static int TooLongDuration => 181;
            
            public static int MinValidPoints => 1;
            public static int MaxValidPoints => 100;
            public static int TooLowPoints => 0;
            public static int TooHighPoints => 101;
        }

        public static class SpecialCharacters
        {
            public static string WithEmojis => "Test Workout 💪🏋️‍♂️";
            public static string WithUnicode => "Test Wørkøut Ñamé";
            public static string WithSpecialChars => "Test-Workout_123!@#";
            public static string WithQuotes => "Test \"Workout\" Name";
            public static string WithApostrophes => "Test's Workout";
        }
    }
}
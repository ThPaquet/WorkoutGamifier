namespace WorkoutGamifier.Models;

public enum DifficultyLevel
{
    Beginner = 1,
    Intermediate = 2,
    Advanced = 3,
    Expert = 4
}

public enum SessionStatus
{
    Active = 1,
    InProgress = 1, // Alias for Active
    Completed = 2,
    Cancelled = 3
}

public enum ErrorCategory
{
    DataValidation = 1,
    Storage = 2,
    BusinessLogic = 3,
    Application = 4,
    UserInput = 5
}

public enum ErrorSeverity
{
    Info = 1,
    Warning = 2,
    Error = 3,
    Critical = 4
}
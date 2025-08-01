namespace WorkoutGamifier.Services;

public interface IValidationService
{
    ValidationResult ValidateWorkout(string name, string? description, string? instructions, string durationText, int difficultyIndex);
    ValidationResult ValidateAction(string description, string pointValueText);
    ValidationResult ValidateWorkoutPool(string name, string? description);
    ValidationResult ValidateSession(string name, int workoutPoolId);
    ValidationResult ValidatePointCost(int pointCost, int currentBalance);
}

public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<ValidationError> Errors { get; set; } = new();
    
    public void AddError(string fieldName, string message)
    {
        Errors.Add(new ValidationError { FieldName = fieldName, Message = message });
        IsValid = false;
    }
}

public class ValidationError
{
    public string FieldName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
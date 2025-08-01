namespace WorkoutGamifier.Core.Services;

public class ValidationService
{
    public ValidationResult ValidateWorkout(string name, string? description, string? instructions, string durationText, int difficultyIndex)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            result.AddError("Name", "Workout name is required");
        }
        else if (name.Length > 100)
        {
            result.AddError("Name", "Workout name cannot exceed 100 characters");
        }

        // Validate description
        if (!string.IsNullOrEmpty(description) && description.Length > 500)
        {
            result.AddError("Description", "Description cannot exceed 500 characters");
        }

        // Validate instructions
        if (!string.IsNullOrEmpty(instructions) && instructions.Length > 2000)
        {
            result.AddError("Instructions", "Instructions cannot exceed 2000 characters");
        }

        // Validate duration
        if (string.IsNullOrWhiteSpace(durationText))
        {
            result.AddError("Duration", "Duration is required");
        }
        else if (!int.TryParse(durationText, out int duration))
        {
            result.AddError("Duration", "Duration must be a valid number");
        }
        else if (duration <= 0)
        {
            result.AddError("Duration", "Duration must be greater than 0 minutes");
        }
        else if (duration > 480) // 8 hours max
        {
            result.AddError("Duration", "Duration cannot exceed 480 minutes (8 hours)");
        }

        // Validate difficulty
        if (difficultyIndex < 0 || difficultyIndex > 3)
        {
            result.AddError("Difficulty", "Please select a valid difficulty level");
        }

        return result;
    }

    public ValidationResult ValidateAction(string description, string pointValueText)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate description
        if (string.IsNullOrWhiteSpace(description))
        {
            result.AddError("Description", "Action description is required");
        }
        else if (description.Length > 200)
        {
            result.AddError("Description", "Description cannot exceed 200 characters");
        }

        // Validate point value
        if (string.IsNullOrWhiteSpace(pointValueText))
        {
            result.AddError("Point Value", "Point value is required");
        }
        else if (!int.TryParse(pointValueText, out int pointValue))
        {
            result.AddError("Point Value", "Point value must be a valid number");
        }
        else if (pointValue <= 0)
        {
            result.AddError("Point Value", "Point value must be greater than 0");
        }
        else if (pointValue > 1000)
        {
            result.AddError("Point Value", "Point value cannot exceed 1000");
        }

        return result;
    }

    public ValidationResult ValidateSession(string name, int workoutPoolId)
    {
        var result = new ValidationResult { IsValid = true };

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
        {
            result.AddError("Name", "Session name is required");
        }
        else if (name.Length > 100)
        {
            result.AddError("Name", "Session name cannot exceed 100 characters");
        }

        // Validate workout pool
        if (workoutPoolId <= 0)
        {
            result.AddError("Workout Pool", "Please select a workout pool");
        }

        return result;
    }

    public ValidationResult ValidatePointCost(int pointCost, int currentBalance)
    {
        var result = new ValidationResult { IsValid = true };

        if (pointCost <= 0)
        {
            result.AddError("Point Cost", "Point cost must be greater than 0");
        }
        else if (pointCost > currentBalance)
        {
            result.AddError("Point Cost", $"Insufficient points. You have {currentBalance} points but need {pointCost}");
        }

        return result;
    }
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
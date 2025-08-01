using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Tests.Services;

public class ValidationServiceTests
{
    private readonly ValidationService _validationService;

    public ValidationServiceTests()
    {
        _validationService = new ValidationService();
    }

    [Fact]
    public void ValidateWorkout_ValidInput_ReturnsValid()
    {
        // Arrange
        var name = "Push-ups";
        var description = "Basic push-up exercise";
        var instructions = "Start in plank position, lower body, push back up";
        var duration = "15";
        var difficultyIndex = 0; // Beginner

        // Act
        var result = _validationService.ValidateWorkout(name, description, instructions, duration, difficultyIndex);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateWorkout_EmptyName_ReturnsInvalid()
    {
        // Arrange
        var name = "";
        var description = "Test description";
        var instructions = "Test instructions";
        var duration = "15";
        var difficultyIndex = 0;

        // Act
        var result = _validationService.ValidateWorkout(name, description, instructions, duration, difficultyIndex);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].FieldName);
        Assert.Equal("Workout name is required", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateWorkout_NameTooLong_ReturnsInvalid()
    {
        // Arrange
        var name = new string('A', 101); // 101 characters
        var description = "Test description";
        var instructions = "Test instructions";
        var duration = "15";
        var difficultyIndex = 0;

        // Act
        var result = _validationService.ValidateWorkout(name, description, instructions, duration, difficultyIndex);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Name", result.Errors[0].FieldName);
        Assert.Equal("Workout name cannot exceed 100 characters", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateWorkout_InvalidDuration_ReturnsInvalid()
    {
        // Arrange
        var name = "Push-ups";
        var description = "Test description";
        var instructions = "Test instructions";
        var duration = "invalid";
        var difficultyIndex = 0;

        // Act
        var result = _validationService.ValidateWorkout(name, description, instructions, duration, difficultyIndex);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Duration", result.Errors[0].FieldName);
        Assert.Equal("Duration must be a valid number", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateWorkout_ZeroDuration_ReturnsInvalid()
    {
        // Arrange
        var name = "Push-ups";
        var description = "Test description";
        var instructions = "Test instructions";
        var duration = "0";
        var difficultyIndex = 0;

        // Act
        var result = _validationService.ValidateWorkout(name, description, instructions, duration, difficultyIndex);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Duration", result.Errors[0].FieldName);
        Assert.Equal("Duration must be greater than 0 minutes", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateWorkout_DurationTooLong_ReturnsInvalid()
    {
        // Arrange
        var name = "Push-ups";
        var description = "Test description";
        var instructions = "Test instructions";
        var duration = "500"; // Over 480 minutes (8 hours)
        var difficultyIndex = 0;

        // Act
        var result = _validationService.ValidateWorkout(name, description, instructions, duration, difficultyIndex);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Duration", result.Errors[0].FieldName);
        Assert.Equal("Duration cannot exceed 480 minutes (8 hours)", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateWorkout_InvalidDifficulty_ReturnsInvalid()
    {
        // Arrange
        var name = "Push-ups";
        var description = "Test description";
        var instructions = "Test instructions";
        var duration = "15";
        var difficultyIndex = -1; // Invalid difficulty

        // Act
        var result = _validationService.ValidateWorkout(name, description, instructions, duration, difficultyIndex);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Difficulty", result.Errors[0].FieldName);
        Assert.Equal("Please select a valid difficulty level", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateAction_ValidInput_ReturnsValid()
    {
        // Arrange
        var description = "Complete 10 push-ups";
        var pointValue = "5";

        // Act
        var result = _validationService.ValidateAction(description, pointValue);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidateAction_EmptyDescription_ReturnsInvalid()
    {
        // Arrange
        var description = "";
        var pointValue = "5";

        // Act
        var result = _validationService.ValidateAction(description, pointValue);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Description", result.Errors[0].FieldName);
        Assert.Equal("Action description is required", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateAction_DescriptionTooLong_ReturnsInvalid()
    {
        // Arrange
        var description = new string('A', 201); // 201 characters
        var pointValue = "5";

        // Act
        var result = _validationService.ValidateAction(description, pointValue);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Description", result.Errors[0].FieldName);
        Assert.Equal("Description cannot exceed 200 characters", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateAction_InvalidPointValue_ReturnsInvalid()
    {
        // Arrange
        var description = "Complete 10 push-ups";
        var pointValue = "invalid";

        // Act
        var result = _validationService.ValidateAction(description, pointValue);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Point Value", result.Errors[0].FieldName);
        Assert.Equal("Point value must be a valid number", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateAction_ZeroPointValue_ReturnsInvalid()
    {
        // Arrange
        var description = "Complete 10 push-ups";
        var pointValue = "0";

        // Act
        var result = _validationService.ValidateAction(description, pointValue);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Point Value", result.Errors[0].FieldName);
        Assert.Equal("Point value must be greater than 0", result.Errors[0].Message);
    }

    [Fact]
    public void ValidateAction_PointValueTooHigh_ReturnsInvalid()
    {
        // Arrange
        var description = "Complete 10 push-ups";
        var pointValue = "1001"; // Over 1000

        // Act
        var result = _validationService.ValidateAction(description, pointValue);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Point Value", result.Errors[0].FieldName);
        Assert.Equal("Point value cannot exceed 1000", result.Errors[0].Message);
    }

    [Fact]
    public void ValidatePointCost_SufficientPoints_ReturnsValid()
    {
        // Arrange
        var pointCost = 10;
        var currentBalance = 15;

        // Act
        var result = _validationService.ValidatePointCost(pointCost, currentBalance);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void ValidatePointCost_InsufficientPoints_ReturnsInvalid()
    {
        // Arrange
        var pointCost = 20;
        var currentBalance = 15;

        // Act
        var result = _validationService.ValidatePointCost(pointCost, currentBalance);

        // Assert
        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Point Cost", result.Errors[0].FieldName);
        Assert.Equal("Insufficient points. You have 15 points but need 20", result.Errors[0].Message);
    }
}
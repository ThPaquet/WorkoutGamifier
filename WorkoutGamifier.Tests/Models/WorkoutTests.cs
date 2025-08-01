using System.ComponentModel.DataAnnotations;
using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Tests.Models;

public class WorkoutTests
{
    [Fact]
    public void Workout_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var workout = new Workout();

        // Assert
        Assert.Equal(0, workout.Id);
        Assert.Equal(string.Empty, workout.Name);
        Assert.Null(workout.Description);
        Assert.Null(workout.Instructions);
        Assert.Equal(0, workout.DurationMinutes);
        Assert.Equal(default(DifficultyLevel), workout.Difficulty);
        Assert.False(workout.IsPreloaded);
        Assert.False(workout.IsHidden);
        Assert.Equal(default(DateTime), workout.CreatedAt);
        Assert.Equal(default(DateTime), workout.UpdatedAt);
    }

    [Fact]
    public void Workout_WithValidData_PropertiesSetCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var workout = new Workout
        {
            Id = 1,
            Name = "Push-ups",
            Description = "Basic push-up exercise",
            Instructions = "Start in plank position, lower body, push back up",
            DurationMinutes = 15,
            Difficulty = DifficultyLevel.Beginner,
            IsPreloaded = true,
            IsHidden = false,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Act & Assert
        Assert.Equal(1, workout.Id);
        Assert.Equal("Push-ups", workout.Name);
        Assert.Equal("Basic push-up exercise", workout.Description);
        Assert.Equal("Start in plank position, lower body, push back up", workout.Instructions);
        Assert.Equal(15, workout.DurationMinutes);
        Assert.Equal(DifficultyLevel.Beginner, workout.Difficulty);
        Assert.True(workout.IsPreloaded);
        Assert.False(workout.IsHidden);
        Assert.Equal(now, workout.CreatedAt);
        Assert.Equal(now, workout.UpdatedAt);
    }

    [Theory]
    [InlineData(DifficultyLevel.Beginner)]
    [InlineData(DifficultyLevel.Intermediate)]
    [InlineData(DifficultyLevel.Advanced)]
    [InlineData(DifficultyLevel.Expert)]
    public void Workout_DifficultyLevel_CanBeSetToValidValues(DifficultyLevel difficulty)
    {
        // Arrange
        var workout = new Workout();

        // Act
        workout.Difficulty = difficulty;

        // Assert
        Assert.Equal(difficulty, workout.Difficulty);
    }

    [Fact]
    public void DifficultyLevel_EnumValues_AreCorrect()
    {
        // Assert
        Assert.Equal(1, (int)DifficultyLevel.Beginner);
        Assert.Equal(2, (int)DifficultyLevel.Intermediate);
        Assert.Equal(3, (int)DifficultyLevel.Advanced);
        Assert.Equal(4, (int)DifficultyLevel.Expert);
    }

    [Fact]
    public void Workout_ValidationAttributes_ArePresent()
    {
        // Arrange
        var workoutType = typeof(Workout);

        // Act & Assert - Check Name property attributes
        var nameProperty = workoutType.GetProperty(nameof(Workout.Name));
        Assert.NotNull(nameProperty);
        
        var requiredAttribute = nameProperty.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;
        Assert.NotNull(requiredAttribute);
        
        var stringLengthAttribute = nameProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault() as StringLengthAttribute;
        Assert.NotNull(stringLengthAttribute);
        Assert.Equal(100, stringLengthAttribute.MaximumLength);

        // Check Description property attributes
        var descriptionProperty = workoutType.GetProperty(nameof(Workout.Description));
        Assert.NotNull(descriptionProperty);
        
        var descriptionStringLengthAttribute = descriptionProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault() as StringLengthAttribute;
        Assert.NotNull(descriptionStringLengthAttribute);
        Assert.Equal(500, descriptionStringLengthAttribute.MaximumLength);

        // Check Instructions property attributes
        var instructionsProperty = workoutType.GetProperty(nameof(Workout.Instructions));
        Assert.NotNull(instructionsProperty);
        
        var instructionsStringLengthAttribute = instructionsProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault() as StringLengthAttribute;
        Assert.NotNull(instructionsStringLengthAttribute);
        Assert.Equal(2000, instructionsStringLengthAttribute.MaximumLength);

        // Check DurationMinutes property attributes
        var durationProperty = workoutType.GetProperty(nameof(Workout.DurationMinutes));
        Assert.NotNull(durationProperty);
        
        var rangeAttribute = durationProperty.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault() as RangeAttribute;
        Assert.NotNull(rangeAttribute);
        Assert.Equal(1, rangeAttribute.Minimum);
        Assert.Equal(480, rangeAttribute.Maximum);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Workout_BooleanProperties_CanBeSetCorrectly(bool isPreloaded, bool isHidden)
    {
        // Arrange
        var workout = new Workout();

        // Act
        workout.IsPreloaded = isPreloaded;
        workout.IsHidden = isHidden;

        // Assert
        Assert.Equal(isPreloaded, workout.IsPreloaded);
        Assert.Equal(isHidden, workout.IsHidden);
    }
}
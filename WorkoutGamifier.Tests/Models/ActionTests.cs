using System.ComponentModel.DataAnnotations;
using WorkoutGamifier.Core.Models;

namespace WorkoutGamifier.Tests.Models;

public class ActionTests
{
    [Fact]
    public void Action_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var action = new WorkoutGamifier.Core.Models.Action();

        // Assert
        Assert.Equal(0, action.Id);
        Assert.Equal(string.Empty, action.Description);
        Assert.Equal(0, action.PointValue);
        Assert.Equal(default(DateTime), action.CreatedAt);
        Assert.Equal(default(DateTime), action.UpdatedAt);
    }

    [Fact]
    public void Action_WithValidData_PropertiesSetCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var action = new WorkoutGamifier.Core.Models.Action
        {
            Id = 1,
            Description = "Complete 10 push-ups",
            PointValue = 5,
            CreatedAt = now,
            UpdatedAt = now
        };

        // Act & Assert
        Assert.Equal(1, action.Id);
        Assert.Equal("Complete 10 push-ups", action.Description);
        Assert.Equal(5, action.PointValue);
        Assert.Equal(now, action.CreatedAt);
        Assert.Equal(now, action.UpdatedAt);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Action_PointValue_CanBeSetToValidValues(int pointValue)
    {
        // Arrange
        var action = new WorkoutGamifier.Core.Models.Action();

        // Act
        action.PointValue = pointValue;

        // Assert
        Assert.Equal(pointValue, action.PointValue);
    }

    [Fact]
    public void Action_ValidationAttributes_ArePresent()
    {
        // Arrange
        var actionType = typeof(WorkoutGamifier.Core.Models.Action);

        // Act & Assert - Check Description property attributes
        var descriptionProperty = actionType.GetProperty(nameof(WorkoutGamifier.Core.Models.Action.Description));
        Assert.NotNull(descriptionProperty);
        
        var requiredAttribute = descriptionProperty.GetCustomAttributes(typeof(RequiredAttribute), false).FirstOrDefault() as RequiredAttribute;
        Assert.NotNull(requiredAttribute);
        
        var stringLengthAttribute = descriptionProperty.GetCustomAttributes(typeof(StringLengthAttribute), false).FirstOrDefault() as StringLengthAttribute;
        Assert.NotNull(stringLengthAttribute);
        Assert.Equal(200, stringLengthAttribute.MaximumLength);

        // Check PointValue property attributes
        var pointValueProperty = actionType.GetProperty(nameof(WorkoutGamifier.Core.Models.Action.PointValue));
        Assert.NotNull(pointValueProperty);
        
        var rangeAttribute = pointValueProperty.GetCustomAttributes(typeof(RangeAttribute), false).FirstOrDefault() as RangeAttribute;
        Assert.NotNull(rangeAttribute);
        Assert.Equal(1, rangeAttribute.Minimum);
        Assert.Equal(1000, rangeAttribute.Maximum);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Action_EmptyDescription_ShouldFailValidation(string? description)
    {
        // Arrange
        var action = new WorkoutGamifier.Core.Models.Action
        {
            Description = description ?? string.Empty,
            PointValue = 5
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(action);
        var isValid = Validator.TryValidateObject(action, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(WorkoutGamifier.Core.Models.Action.Description)));
    }

    [Fact]
    public void Action_DescriptionTooLong_ShouldFailValidation()
    {
        // Arrange
        var action = new WorkoutGamifier.Core.Models.Action
        {
            Description = new string('A', 201), // 201 characters
            PointValue = 5
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(action);
        var isValid = Validator.TryValidateObject(action, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(WorkoutGamifier.Core.Models.Action.Description)));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public void Action_InvalidPointValue_ShouldFailValidation(int pointValue)
    {
        // Arrange
        var action = new WorkoutGamifier.Core.Models.Action
        {
            Description = "Valid description",
            PointValue = pointValue
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(action);
        var isValid = Validator.TryValidateObject(action, validationContext, validationResults, true);

        // Assert
        Assert.False(isValid);
        Assert.Contains(validationResults, vr => vr.MemberNames.Contains(nameof(WorkoutGamifier.Core.Models.Action.PointValue)));
    }

    [Fact]
    public void Action_ValidData_ShouldPassValidation()
    {
        // Arrange
        var action = new WorkoutGamifier.Core.Models.Action
        {
            Description = "Complete 10 push-ups",
            PointValue = 5
        };

        // Act
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(action);
        var isValid = Validator.TryValidateObject(action, validationContext, validationResults, true);

        // Assert
        Assert.True(isValid);
        Assert.Empty(validationResults);
    }
}
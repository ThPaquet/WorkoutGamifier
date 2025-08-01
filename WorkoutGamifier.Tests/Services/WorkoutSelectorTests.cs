using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Services;

namespace WorkoutGamifier.Tests.Services;

public class WorkoutSelectorTests
{
    private readonly WorkoutSelector _workoutSelector;
    private readonly List<Workout> _testWorkouts;

    public WorkoutSelectorTests()
    {
        // Use a seeded random for predictable tests
        var seededRandom = new Random(42);
        _workoutSelector = new WorkoutSelector(seededRandom);
        
        _testWorkouts = new List<Workout>
        {
            new Workout
            {
                Id = 1,
                Name = "Push-ups",
                Difficulty = DifficultyLevel.Beginner,
                DurationMinutes = 15,
                IsHidden = false
            },
            new Workout
            {
                Id = 2,
                Name = "Squats",
                Difficulty = DifficultyLevel.Intermediate,
                DurationMinutes = 20,
                IsHidden = false
            },
            new Workout
            {
                Id = 3,
                Name = "Burpees",
                Difficulty = DifficultyLevel.Advanced,
                DurationMinutes = 25,
                IsHidden = false
            },
            new Workout
            {
                Id = 4,
                Name = "Hidden Workout",
                Difficulty = DifficultyLevel.Beginner,
                DurationMinutes = 10,
                IsHidden = true
            }
        };
    }

    [Fact]
    public void SelectRandomWorkout_ValidWorkouts_ReturnsWorkout()
    {
        // Act
        var selectedWorkout = _workoutSelector.SelectRandomWorkout(_testWorkouts);

        // Assert
        Assert.NotNull(selectedWorkout);
        Assert.Contains(selectedWorkout, _testWorkouts.Where(w => !w.IsHidden));
    }

    [Fact]
    public void SelectRandomWorkout_EmptyList_ReturnsNull()
    {
        // Arrange
        var emptyWorkouts = new List<Workout>();

        // Act
        var selectedWorkout = _workoutSelector.SelectRandomWorkout(emptyWorkouts);

        // Assert
        Assert.Null(selectedWorkout);
    }

    [Fact]
    public void SelectRandomWorkout_NullList_ReturnsNull()
    {
        // Act
        var selectedWorkout = _workoutSelector.SelectRandomWorkout(null!);

        // Assert
        Assert.Null(selectedWorkout);
    }

    [Fact]
    public void SelectRandomWorkout_AllHiddenWorkouts_ReturnsNull()
    {
        // Arrange
        var hiddenWorkouts = new List<Workout>
        {
            new Workout { Id = 1, Name = "Hidden 1", IsHidden = true },
            new Workout { Id = 2, Name = "Hidden 2", IsHidden = true }
        };

        // Act
        var selectedWorkout = _workoutSelector.SelectRandomWorkout(hiddenWorkouts);

        // Assert
        Assert.Null(selectedWorkout);
    }

    [Fact]
    public void SelectRandomWorkout_ExcludesHiddenWorkouts()
    {
        // Act
        var selectedWorkout = _workoutSelector.SelectRandomWorkout(_testWorkouts);

        // Assert
        Assert.NotNull(selectedWorkout);
        Assert.False(selectedWorkout.IsHidden);
        Assert.NotEqual(4, selectedWorkout.Id); // Hidden workout should not be selected
    }

    [Fact]
    public void SelectRandomWorkoutByDifficulty_ValidDifficulty_ReturnsCorrectWorkout()
    {
        // Act
        var selectedWorkout = _workoutSelector.SelectRandomWorkoutByDifficulty(_testWorkouts, DifficultyLevel.Beginner);

        // Assert
        Assert.NotNull(selectedWorkout);
        Assert.Equal(DifficultyLevel.Beginner, selectedWorkout.Difficulty);
        Assert.False(selectedWorkout.IsHidden);
    }

    [Fact]
    public void SelectRandomWorkoutByDifficulty_NoDifficultyMatch_ReturnsNull()
    {
        // Act
        var selectedWorkout = _workoutSelector.SelectRandomWorkoutByDifficulty(_testWorkouts, DifficultyLevel.Expert);

        // Assert
        Assert.Null(selectedWorkout);
    }

    [Fact]
    public void FilterWorkoutsByDuration_ValidRange_ReturnsFilteredWorkouts()
    {
        // Act
        var filteredWorkouts = _workoutSelector.FilterWorkoutsByDuration(_testWorkouts, 15, 20).ToList();

        // Assert
        Assert.Equal(2, filteredWorkouts.Count);
        Assert.All(filteredWorkouts, w => Assert.InRange(w.DurationMinutes, 15, 20));
        Assert.All(filteredWorkouts, w => Assert.False(w.IsHidden));
    }

    [Fact]
    public void FilterWorkoutsByDuration_NoMatches_ReturnsEmpty()
    {
        // Act
        var filteredWorkouts = _workoutSelector.FilterWorkoutsByDuration(_testWorkouts, 100, 200).ToList();

        // Assert
        Assert.Empty(filteredWorkouts);
    }

    [Fact]
    public void FilterWorkoutsByDuration_NullList_ReturnsEmpty()
    {
        // Act
        var filteredWorkouts = _workoutSelector.FilterWorkoutsByDuration(null!, 15, 20).ToList();

        // Assert
        Assert.Empty(filteredWorkouts);
    }

    [Fact]
    public void GetWorkoutsByDifficulty_ValidDifficulty_ReturnsCorrectWorkouts()
    {
        // Act
        var beginnerWorkouts = _workoutSelector.GetWorkoutsByDifficulty(_testWorkouts, DifficultyLevel.Beginner).ToList();

        // Assert
        Assert.Single(beginnerWorkouts); // Only one non-hidden beginner workout
        Assert.All(beginnerWorkouts, w => Assert.Equal(DifficultyLevel.Beginner, w.Difficulty));
        Assert.All(beginnerWorkouts, w => Assert.False(w.IsHidden));
    }

    [Fact]
    public void GetWorkoutsByDifficulty_NoDifficultyMatch_ReturnsEmpty()
    {
        // Act
        var expertWorkouts = _workoutSelector.GetWorkoutsByDifficulty(_testWorkouts, DifficultyLevel.Expert).ToList();

        // Assert
        Assert.Empty(expertWorkouts);
    }

    [Fact]
    public void GetWorkoutsByDifficulty_NullList_ReturnsEmpty()
    {
        // Act
        var workouts = _workoutSelector.GetWorkoutsByDifficulty(null!, DifficultyLevel.Beginner).ToList();

        // Assert
        Assert.Empty(workouts);
    }

    [Fact]
    public void SelectRandomWorkout_MultipleCallsWithSameSeed_ReturnsSameResult()
    {
        // Arrange
        var seededSelector1 = new WorkoutSelector(new Random(123));
        var seededSelector2 = new WorkoutSelector(new Random(123));

        // Act
        var workout1 = seededSelector1.SelectRandomWorkout(_testWorkouts);
        var workout2 = seededSelector2.SelectRandomWorkout(_testWorkouts);

        // Assert
        Assert.NotNull(workout1);
        Assert.NotNull(workout2);
        Assert.Equal(workout1.Id, workout2.Id);
    }

    [Fact]
    public void SelectRandomWorkout_DifferentSeeds_CanReturnDifferentResults()
    {
        // Arrange
        var selector1 = new WorkoutSelector(new Random(123));
        var selector2 = new WorkoutSelector(new Random(456));
        var results1 = new List<int>();
        var results2 = new List<int>();

        // Act - Select multiple times to increase chance of different results
        for (int i = 0; i < 10; i++)
        {
            var workout1 = selector1.SelectRandomWorkout(_testWorkouts);
            var workout2 = selector2.SelectRandomWorkout(_testWorkouts);
            
            if (workout1 != null) results1.Add(workout1.Id);
            if (workout2 != null) results2.Add(workout2.Id);
        }

        // Assert - At least one different result (with high probability)
        Assert.NotEqual(results1, results2);
    }
}
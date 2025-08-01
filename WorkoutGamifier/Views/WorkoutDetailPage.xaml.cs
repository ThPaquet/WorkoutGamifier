using WorkoutGamifier.Services;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Views;

public partial class WorkoutDetailPage : BasePage
{
    private readonly IWorkoutService _workoutService;
    private Workout? _currentWorkout;
    private bool _isEditMode;

    public WorkoutDetailPage(IWorkoutService workoutService, IErrorHandler errorHandler, IValidationService validationService)
        : base(errorHandler, validationService)
    {
        InitializeComponent();
        _workoutService = workoutService;
    }

    public void SetWorkout(Workout? workout)
    {
        _currentWorkout = workout;
        _isEditMode = workout != null;
        
        if (_isEditMode)
        {
            PageTitle.Text = "Edit Workout";
            LoadWorkoutData();
            DeleteButton.IsVisible = true;
            
            // Handle preloaded workouts
            if (workout!.IsPreloaded)
            {
                PreloadedInfoFrame.IsVisible = true;
                NameEntry.IsEnabled = false;
                DescriptionEditor.IsEnabled = false;
                InstructionsEditor.IsEnabled = false;
                DurationEntry.IsEnabled = false;
                DifficultyPicker.IsEnabled = false;
                SaveButton.Text = "Toggle Visibility";
                DeleteButton.IsVisible = false;
            }
        }
        else
        {
            PageTitle.Text = "Add New Workout";
            DeleteButton.IsVisible = false;
        }
    }

    private void LoadWorkoutData()
    {
        if (_currentWorkout == null) return;

        NameEntry.Text = _currentWorkout.Name;
        DescriptionEditor.Text = _currentWorkout.Description;
        InstructionsEditor.Text = _currentWorkout.Instructions;
        DurationEntry.Text = _currentWorkout.DurationMinutes.ToString();
        DifficultyPicker.SelectedIndex = (int)_currentWorkout.Difficulty - 1;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_currentWorkout?.IsPreloaded == true)
        {
            // Toggle visibility for preloaded workouts
            var toggleResult = await ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    await _workoutService.ToggleWorkoutVisibilityAsync(_currentWorkout.Id);
                    await ShowSuccessMessageAsync("Workout visibility updated!");
                    await Shell.Current.GoToAsync("..");
                },
                "update workout visibility",
                showRetryOption: true
            );
            return;
        }

        // Validate input using the validation service
        var validationResult = ValidationService.ValidateWorkout(
            NameEntry.Text ?? string.Empty,
            DescriptionEditor.Text,
            InstructionsEditor.Text,
            DurationEntry.Text ?? string.Empty,
            DifficultyPicker.SelectedIndex
        );

        if (!await ShowValidationErrorsAsync(validationResult))
        {
            return;
        }

        // Parse duration (we know it's valid from validation)
        int.TryParse(DurationEntry.Text, out int duration);

        var workout = _currentWorkout ?? new Workout();
        workout.Name = NameEntry.Text!.Trim();
        workout.Description = DescriptionEditor.Text?.Trim();
        workout.Instructions = InstructionsEditor.Text?.Trim();
        workout.DurationMinutes = duration;
        workout.Difficulty = (DifficultyLevel)(DifficultyPicker.SelectedIndex + 1);

        var operationName = _isEditMode ? "update workout" : "create workout";
        var successMessage = _isEditMode ? "Workout updated successfully!" : "Workout created successfully!";

        var success = await ExecuteWithErrorHandlingAsync(
            async () =>
            {
                if (_isEditMode)
                {
                    await _workoutService.UpdateWorkoutAsync(workout);
                }
                else
                {
                    await _workoutService.CreateWorkoutAsync(workout);
                }
                
                await ShowSuccessMessageAsync(successMessage);
                await Shell.Current.GoToAsync("..");
            },
            operationName,
            showRetryOption: true
        );
    }

    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        if (_currentWorkout == null) return;

        var confirmed = await ConfirmDestructiveActionAsync(
            "Delete Workout",
            $"Are you sure you want to delete '{_currentWorkout.Name}'?\n\nThis action cannot be undone.",
            "Delete",
            "Cancel"
        );

        if (confirmed)
        {
            var success = await ExecuteWithErrorHandlingAsync(
                async () =>
                {
                    await _workoutService.DeleteWorkoutAsync(_currentWorkout.Id);
                    await ShowSuccessMessageAsync("Workout deleted successfully!");
                    await Shell.Current.GoToAsync("..");
                },
                "delete workout",
                showRetryOption: true
            );
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
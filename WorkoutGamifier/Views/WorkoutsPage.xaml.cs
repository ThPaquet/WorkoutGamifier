using WorkoutGamifier.Services;
using WorkoutGamifier.Models;
using System.Collections.ObjectModel;

namespace WorkoutGamifier.Views;

public partial class WorkoutsPage : ContentPage
{
    private readonly IWorkoutService _workoutService;
    private List<Workout> _allWorkouts = new();
    private ObservableCollection<Workout> _filteredWorkouts = new();
    private bool _showHidden = false;

    public WorkoutsPage(IWorkoutService workoutService)
    {
        InitializeComponent();
        _workoutService = workoutService;
        WorkoutsCollectionView.ItemsSource = _filteredWorkouts;
        DifficultyPicker.SelectedIndex = 0; // Select "All"
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadWorkoutsAsync();
    }

    private async Task LoadWorkoutsAsync()
    {
        try
        {
            _allWorkouts = _showHidden 
                ? await _workoutService.GetAllWorkoutsAsync()
                : await _workoutService.GetVisibleWorkoutsAsync();
            
            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load workouts: {ex.Message}", "OK");
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allWorkouts.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchBar.Text))
        {
            var searchText = SearchBar.Text.ToLower();
            filtered = filtered.Where(w => 
                w.Name.ToLower().Contains(searchText) || 
                (w.Description?.ToLower().Contains(searchText) ?? false));
        }

        // Apply difficulty filter
        if (DifficultyPicker.SelectedIndex > 0)
        {
            var selectedDifficulty = (DifficultyLevel)(DifficultyPicker.SelectedIndex);
            filtered = filtered.Where(w => w.Difficulty == selectedDifficulty);
        }

        _filteredWorkouts.Clear();
        foreach (var workout in filtered.OrderBy(w => w.Name))
        {
            _filteredWorkouts.Add(workout);
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        ApplyFilters();
    }

    private void OnDifficultyFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    private async void OnToggleHiddenClicked(object sender, EventArgs e)
    {
        _showHidden = !_showHidden;
        ShowHiddenBtn.Text = _showHidden ? "Hide Hidden" : "Show Hidden";
        await LoadWorkoutsAsync();
    }

    private async void OnAddWorkoutClicked(object sender, EventArgs e)
    {
        try
        {
            var name = await DisplayPromptAsync("New Workout", "Enter workout name:", "OK", "Cancel");
            if (string.IsNullOrEmpty(name)) return;

            var description = await DisplayPromptAsync("Description", "Enter workout description:", "OK", "Cancel");
            if (string.IsNullOrEmpty(description)) return;

            var instructions = await DisplayPromptAsync("Instructions", "Enter workout instructions:", "OK", "Cancel");
            if (string.IsNullOrEmpty(instructions)) return;

            var durationText = await DisplayPromptAsync("Duration", "Enter duration in minutes:", "OK", "Cancel", keyboard: Keyboard.Numeric);
            if (string.IsNullOrEmpty(durationText)) return;

            if (!int.TryParse(durationText, out int duration) || duration <= 0)
            {
                await DisplayAlert("Error", "Please enter a valid duration in minutes.", "OK");
                return;
            }

            var difficultyOptions = new[] { "Beginner", "Intermediate", "Advanced", "Expert" };
            var selectedDifficulty = await DisplayActionSheet("Select Difficulty", "Cancel", null, difficultyOptions);
            if (selectedDifficulty == null || selectedDifficulty == "Cancel") return;

            var difficulty = Enum.Parse<DifficultyLevel>(selectedDifficulty);

            var workout = new Workout
            {
                Name = name,
                Description = description,
                Instructions = instructions,
                DurationMinutes = duration,
                Difficulty = difficulty
            };

            await _workoutService.CreateWorkoutAsync(workout);
            await DisplayAlert("Success", "Workout created successfully!", "OK");
            await LoadWorkoutsAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to create workout: {ex.Message}", "OK");
        }
    }

    private async void OnViewDetailsClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Workout workout)
        {
            var details = $"Name: {workout.Name}\n\n" +
                         $"Description: {workout.Description}\n\n" +
                         $"Instructions: {workout.Instructions}\n\n" +
                         $"Duration: {workout.DurationMinutes} minutes\n" +
                         $"Difficulty: {workout.Difficulty}\n" +
                         $"Type: {(workout.IsPreloaded ? "Pre-loaded" : "Custom")}\n" +
                         $"Status: {(workout.IsHidden ? "Hidden" : "Visible")}";

            await DisplayAlert("Workout Details", details, "OK");
        }
    }

    private async void OnEditWorkoutClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Workout workout)
        {
            try
            {
                if (workout.IsPreloaded)
                {
                    await DisplayAlert("Info", "Pre-loaded workouts can only be hidden/shown, not edited.", "OK");
                    return;
                }

                var name = await DisplayPromptAsync("Edit Workout", "Enter workout name:", "OK", "Cancel", initialValue: workout.Name);
                if (string.IsNullOrEmpty(name)) return;

                var description = await DisplayPromptAsync("Description", "Enter workout description:", "OK", "Cancel", initialValue: workout.Description);
                if (string.IsNullOrEmpty(description)) return;

                var instructions = await DisplayPromptAsync("Instructions", "Enter workout instructions:", "OK", "Cancel", initialValue: workout.Instructions);
                if (string.IsNullOrEmpty(instructions)) return;

                var durationText = await DisplayPromptAsync("Duration", "Enter duration in minutes:", "OK", "Cancel", initialValue: workout.DurationMinutes.ToString(), keyboard: Keyboard.Numeric);
                if (string.IsNullOrEmpty(durationText)) return;

                if (!int.TryParse(durationText, out int duration) || duration <= 0)
                {
                    await DisplayAlert("Error", "Please enter a valid duration in minutes.", "OK");
                    return;
                }

                var difficultyOptions = new[] { "Beginner", "Intermediate", "Advanced", "Expert" };
                var selectedDifficulty = await DisplayActionSheet("Select Difficulty", "Cancel", null, difficultyOptions);
                if (selectedDifficulty == null || selectedDifficulty == "Cancel") return;

                workout.Name = name;
                workout.Description = description;
                workout.Instructions = instructions;
                workout.DurationMinutes = duration;
                workout.Difficulty = Enum.Parse<DifficultyLevel>(selectedDifficulty);

                await _workoutService.UpdateWorkoutAsync(workout);
                await DisplayAlert("Success", "Workout updated successfully!", "OK");
                await LoadWorkoutsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to update workout: {ex.Message}", "OK");
            }
        }
    }

    private async void OnToggleVisibilityClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Workout workout)
        {
            try
            {
                var newVisibility = await _workoutService.ToggleWorkoutVisibilityAsync(workout.Id);
                var status = newVisibility ? "shown" : "hidden";
                await DisplayAlert("Success", $"Workout {status} successfully!", "OK");
                await LoadWorkoutsAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to toggle workout visibility: {ex.Message}", "OK");
            }
        }
    }

    private async void OnDeleteWorkoutClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Workout workout)
        {
            try
            {
                var result = await DisplayAlert("Confirm Delete", 
                    $"Delete workout '{workout.Name}'? This action cannot be undone.", 
                    "Yes", "No");
                
                if (result)
                {
                    await _workoutService.DeleteWorkoutAsync(workout.Id);
                    await DisplayAlert("Success", "Workout deleted successfully!", "OK");
                    await LoadWorkoutsAsync();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete workout: {ex.Message}", "OK");
            }
        }
    }
}
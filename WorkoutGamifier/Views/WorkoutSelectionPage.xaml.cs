using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WorkoutGamifier.Models;
using WorkoutGamifier.Services;

namespace WorkoutGamifier.Views;

[QueryProperty(nameof(PoolId), "PoolId")]
public partial class WorkoutSelectionPage : ContentPage, INotifyPropertyChanged
{
    private readonly IWorkoutService _workoutService;
    private readonly IWorkoutPoolService _workoutPoolService;
    
    private int _poolId;
    private string _searchText = string.Empty;
    private string _selectedDifficulty = "All";
    private bool _showHiddenWorkouts = false;
    private bool _isBusy = false;
    
    private ObservableCollection<SelectableWorkout> _allWorkouts;
    private ObservableCollection<SelectableWorkout> _filteredWorkouts;
    private List<int> _workoutsAlreadyInPool;

    public WorkoutSelectionPage(IWorkoutService workoutService, IWorkoutPoolService workoutPoolService)
    {
        InitializeComponent();
        _workoutService = workoutService;
        _workoutPoolService = workoutPoolService;
        
        _allWorkouts = new ObservableCollection<SelectableWorkout>();
        _filteredWorkouts = new ObservableCollection<SelectableWorkout>();
        _workoutsAlreadyInPool = new List<int>();
        
        DifficultyOptions = new List<string> { "All", "Beginner", "Intermediate", "Advanced" };
        
        SearchCommand = new Command(() => ApplyFilters());
        AddSelectedCommand = new Command(async () => await OnAddSelected(), () => HasSelectedWorkouts);
        CancelCommand = new Command(async () => await OnCancel());
        
        BindingContext = this;
    }

    public int PoolId
    {
        get => _poolId;
        set
        {
            _poolId = value;
            OnPropertyChanged();
            _ = LoadWorkoutsAsync();
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public string SelectedDifficulty
    {
        get => _selectedDifficulty;
        set
        {
            _selectedDifficulty = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public bool ShowHiddenWorkouts
    {
        get => _showHiddenWorkouts;
        set
        {
            _showHiddenWorkouts = value;
            OnPropertyChanged();
            ApplyFilters();
        }
    }

    public new bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<SelectableWorkout> FilteredWorkouts
    {
        get => _filteredWorkouts;
        set
        {
            _filteredWorkouts = value;
            OnPropertyChanged();
        }
    }

    public List<string> DifficultyOptions { get; }

    public string SelectionCountText
    {
        get
        {
            var selectedCount = _allWorkouts?.Count(w => w.IsSelected) ?? 0;
            return $"{selectedCount} workout(s) selected";
        }
    }

    public string AddButtonText
    {
        get
        {
            var selectedCount = _allWorkouts?.Count(w => w.IsSelected) ?? 0;
            return selectedCount == 0 ? "Add Workouts" : $"Add {selectedCount} Workout(s)";
        }
    }

    public bool HasSelectedWorkouts => _allWorkouts?.Any(w => w.IsSelected) == true;

    public ICommand SearchCommand { get; }
    public ICommand AddSelectedCommand { get; }
    public ICommand CancelCommand { get; }

    private async Task LoadWorkoutsAsync()
    {
        IsBusy = true;
        try
        {
            // Load all workouts
            var workouts = await _workoutService.GetAllWorkoutsAsync();
            
            // Load workouts already in the pool
            var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(PoolId);
            _workoutsAlreadyInPool = workoutsInPool.Select(w => w.Id).ToList();
            
            // Create selectable workout objects
            _allWorkouts.Clear();
            foreach (var workout in workouts)
            {
                var selectableWorkout = new SelectableWorkout(workout)
                {
                    IsAlreadyInPool = _workoutsAlreadyInPool.Contains(workout.Id)
                };
                selectableWorkout.PropertyChanged += OnWorkoutSelectionChanged;
                _allWorkouts.Add(selectableWorkout);
            }
            
            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load workouts: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnWorkoutSelectionChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectableWorkout.IsSelected))
        {
            OnPropertyChanged(nameof(SelectionCountText));
            OnPropertyChanged(nameof(AddButtonText));
            OnPropertyChanged(nameof(HasSelectedWorkouts));
            ((Command)AddSelectedCommand).ChangeCanExecute();
        }
    }

    private void ApplyFilters()
    {
        var filtered = _allWorkouts.AsEnumerable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(w => w.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                                          (w.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));
        }

        // Apply difficulty filter
        if (SelectedDifficulty != "All")
        {
            filtered = filtered.Where(w => w.DifficultyLevel.ToString() == SelectedDifficulty);
        }

        // Apply hidden filter
        if (!ShowHiddenWorkouts)
        {
            filtered = filtered.Where(w => !w.IsHidden);
        }

        FilteredWorkouts.Clear();
        foreach (var workout in filtered)
        {
            FilteredWorkouts.Add(workout);
        }
    }

    private async Task OnAddSelected()
    {
        var selectedWorkouts = _allWorkouts.Where(w => w.IsSelected && !w.IsAlreadyInPool).ToList();
        
        if (!selectedWorkouts.Any())
        {
            await DisplayAlert("No Selection", "Please select at least one workout that is not already in the pool.", "OK");
            return;
        }

        IsBusy = true;
        try
        {
            foreach (var workout in selectedWorkouts)
            {
                await _workoutPoolService.AddWorkoutToPoolAsync(PoolId, workout.Id);
            }

            await DisplayAlert("Success", $"Added {selectedWorkouts.Count} workout(s) to the pool!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to add workouts to pool: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnCancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class SelectableWorkout : INotifyPropertyChanged
{
    private bool _isSelected;

    public SelectableWorkout(Workout workout)
    {
        Id = workout.Id;
        Name = workout.Name;
        Description = workout.Description;
        Instructions = workout.Instructions;
        DifficultyLevel = workout.Difficulty;
        EstimatedDurationMinutes = workout.DurationMinutes;
        IsPreLoaded = workout.IsPreloaded;
        IsHidden = workout.IsHidden;
    }

    public int Id { get; }
    public string Name { get; }
    public string? Description { get; }
    public string? Instructions { get; }
    public DifficultyLevel DifficultyLevel { get; }
    public int EstimatedDurationMinutes { get; }
    public bool IsPreLoaded { get; }
    public bool IsHidden { get; }
    public bool IsAlreadyInPool { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
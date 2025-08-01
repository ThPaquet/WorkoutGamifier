using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WorkoutGamifier.Models;
using WorkoutGamifier.Services;

namespace WorkoutGamifier.Views;

[QueryProperty(nameof(PoolId), "PoolId")]
public partial class WorkoutPoolDetailPage : ContentPage
{
    private readonly IWorkoutPoolService _workoutPoolService;
    private readonly IWorkoutService _workoutService;
    private WorkoutPool _pool = null!;
    private ObservableCollection<Workout> _workoutsInPool;
    private int _poolId;

    public WorkoutPoolDetailPage(IWorkoutPoolService workoutPoolService, IWorkoutService workoutService)
    {
        InitializeComponent();
        _workoutPoolService = workoutPoolService;
        _workoutService = workoutService;
        _workoutsInPool = new ObservableCollection<Workout>();
        
        BackCommand = new Command(async () => await OnBack());
        EditPoolCommand = new Command(async () => await OnEditPool());
        AddWorkoutsCommand = new Command(async () => await OnAddWorkouts());
        DeletePoolCommand = new Command(async () => await OnDeletePool());
        RemoveWorkoutCommand = new Command<Workout>(async (workout) => await OnRemoveWorkout(workout));
        
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Refresh the workouts in pool when returning to this page
        if (Pool != null)
        {
            await LoadWorkoutsInPool();
        }
    }

    public WorkoutPool Pool
    {
        get => _pool;
        set
        {
            _pool = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasDescription));
            OnPropertyChanged(nameof(WorkoutCountText));
        }
    }

    public ObservableCollection<Workout> WorkoutsInPool
    {
        get => _workoutsInPool;
        set
        {
            _workoutsInPool = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(WorkoutCountText));
        }
    }

    public bool HasDescription => !string.IsNullOrWhiteSpace(Pool?.Description);
    
    public string WorkoutCountText => $"{WorkoutsInPool?.Count ?? 0} workout(s) in pool";

    public int PoolId
    {
        get => _poolId;
        set
        {
            _poolId = value;
            OnPropertyChanged();
            _ = InitializeAsync(value);
        }
    }

    public ICommand BackCommand { get; }
    public ICommand EditPoolCommand { get; }
    public ICommand AddWorkoutsCommand { get; }
    public ICommand DeletePoolCommand { get; }
    public ICommand RemoveWorkoutCommand { get; }

    public async Task InitializeAsync(int poolId)
    {
        try
        {
            var pool = await _workoutPoolService.GetPoolByIdAsync(poolId);
            if (pool != null)
            {
                Pool = pool;
                await LoadWorkoutsInPool();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load pool details: {ex.Message}", "OK");
        }
    }

    private async Task LoadWorkoutsInPool()
    {
        try
        {
            var workouts = await _workoutPoolService.GetWorkoutsInPoolAsync(Pool.Id);
            WorkoutsInPool.Clear();
            foreach (var workout in workouts)
            {
                WorkoutsInPool.Add(workout);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load workouts: {ex.Message}", "OK");
        }
    }

    private async Task OnBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    private async Task OnEditPool()
    {
        var parameters = new Dictionary<string, object>
        {
            { "Pool", Pool }
        };
        await Shell.Current.GoToAsync("PoolFormPage", parameters);
    }

    private async Task OnAddWorkouts()
    {
        var parameters = new Dictionary<string, object>
        {
            { "PoolId", Pool.Id }
        };
        await Shell.Current.GoToAsync("WorkoutSelectionPage", parameters);
    }

    private async Task OnDeletePool()
    {
        var result = await DisplayAlert("Confirm Delete", 
            $"Are you sure you want to delete the pool '{Pool.Name}'? This action cannot be undone.", 
            "Delete", "Cancel");

        if (result)
        {
            try
            {
                await _workoutPoolService.DeletePoolAsync(Pool.Id);
                await DisplayAlert("Success", "Pool deleted successfully", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete pool: {ex.Message}", "OK");
            }
        }
    }

    private async Task OnRemoveWorkout(Workout workout)
    {
        var result = await DisplayAlert("Confirm Remove", 
            $"Remove '{workout.Name}' from this pool?", 
            "Remove", "Cancel");

        if (result)
        {
            try
            {
                await _workoutPoolService.RemoveWorkoutFromPoolAsync(Pool.Id, workout.Id);
                WorkoutsInPool.Remove(workout);
                OnPropertyChanged(nameof(WorkoutCountText));
                await DisplayAlert("Success", "Workout removed from pool", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to remove workout: {ex.Message}", "OK");
            }
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
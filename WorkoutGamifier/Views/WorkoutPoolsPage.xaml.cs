using System.Collections.ObjectModel;
using WorkoutGamifier.Services;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Views;

public partial class WorkoutPoolsPage : ContentPage
{
    private readonly IWorkoutPoolService _workoutPoolService;
    private ObservableCollection<PoolDisplayModel> _pools;

    public WorkoutPoolsPage(IWorkoutPoolService workoutPoolService)
    {
        InitializeComponent();
        _workoutPoolService = workoutPoolService;
        _pools = new ObservableCollection<PoolDisplayModel>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPoolsAsync();
    }

    private async Task LoadPoolsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        
        try
        {
            var pools = await _workoutPoolService.GetAllPoolsAsync();
            _pools.Clear();
            
            foreach (var pool in pools)
            {
                var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(pool.Id);
                _pools.Add(new PoolDisplayModel
                {
                    Id = pool.Id,
                    Name = pool.Name,
                    Description = pool.Description,
                    CreatedAt = pool.CreatedAt,
                    UpdatedAt = pool.UpdatedAt,
                    WorkoutCount = workoutsInPool.Count
                });
            }
            
            PoolsCollectionView.ItemsSource = _pools;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load workout pools: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnAddPoolClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("PoolFormPage");
    }

    private async void OnManagePoolClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is PoolDisplayModel pool)
        {
            var parameters = new Dictionary<string, object>
            {
                { "PoolId", pool.Id }
            };
            await Shell.Current.GoToAsync("WorkoutPoolDetailPage", parameters);
        }
    }

    private async void OnEditPoolClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is PoolDisplayModel poolDisplay)
        {
            try
            {
                var pool = await _workoutPoolService.GetPoolByIdAsync(poolDisplay.Id);
                if (pool != null)
                {
                    var parameters = new Dictionary<string, object>
                    {
                        { "Pool", pool }
                    };
                    await Shell.Current.GoToAsync("PoolFormPage", parameters);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load pool for editing: {ex.Message}", "OK");
            }
        }
    }
}

public class PoolDisplayModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int WorkoutCount { get; set; }
}
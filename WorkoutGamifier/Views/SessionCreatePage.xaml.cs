using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WorkoutGamifier.Models;
using WorkoutGamifier.Services;

namespace WorkoutGamifier.Views;

public partial class SessionCreatePage : ContentPage
{
    private readonly IWorkoutPoolService _workoutPoolService;
    private readonly ISessionService _sessionService;
    
    private string _searchText = string.Empty;
    private bool _isBusy = false;
    private SelectablePool? _selectedPool;
    
    private ObservableCollection<SelectablePool> _allPools;
    private ObservableCollection<SelectablePool> _filteredPools;

    public SessionCreatePage(IWorkoutPoolService workoutPoolService, ISessionService sessionService)
    {
        InitializeComponent();
        _workoutPoolService = workoutPoolService;
        _sessionService = sessionService;
        
        _allPools = new ObservableCollection<SelectablePool>();
        _filteredPools = new ObservableCollection<SelectablePool>();
        
        SearchCommand = new Command(ApplyFilter);
        PoolSelectedCommand = new Command<SelectablePool>(OnPoolSelected);
        StartSessionCommand = new Command(async () => await OnStartSession(), () => CanStartSession);
        CancelCommand = new Command(async () => await OnCancel());
        CreatePoolCommand = new Command(async () => await OnCreatePool());
        
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadPoolsAsync();
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            ApplyFilter();
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

    public SelectablePool? SelectedPool
    {
        get => _selectedPool;
        set
        {
            if (_selectedPool != value)
            {
                // Deselect previous pool
                if (_selectedPool != null)
                {
                    _selectedPool.IsSelected = false;
                }

                _selectedPool = value;

                // Select new pool
                if (_selectedPool != null)
                {
                    _selectedPool.IsSelected = true;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(CanStartSession));
                ((Command)StartSessionCommand).ChangeCanExecute();
            }
        }
    }

    public ObservableCollection<SelectablePool> FilteredPools
    {
        get => _filteredPools;
        set
        {
            _filteredPools = value;
            OnPropertyChanged();
        }
    }

    public bool CanStartSession => SelectedPool != null;

    public ICommand SearchCommand { get; }
    public ICommand PoolSelectedCommand { get; }
    public ICommand StartSessionCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand CreatePoolCommand { get; }

    private async Task LoadPoolsAsync()
    {
        IsBusy = true;
        try
        {
            var pools = await _workoutPoolService.GetAllPoolsAsync();
            
            _allPools.Clear();
            foreach (var pool in pools)
            {
                var workoutsInPool = await _workoutPoolService.GetWorkoutsInPoolAsync(pool.Id);
                var selectablePool = new SelectablePool
                {
                    Id = pool.Id,
                    Name = pool.Name,
                    Description = pool.Description,
                    WorkoutCount = workoutsInPool.Count,
                    IsSelected = false
                };
                _allPools.Add(selectablePool);
            }
            
            ApplyFilter();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load workout pools: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void ApplyFilter()
    {
        var filtered = _allPools.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            filtered = filtered.Where(p => 
                p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                (p.Description?.Contains(SearchText, StringComparison.OrdinalIgnoreCase) == true));
        }

        FilteredPools.Clear();
        foreach (var pool in filtered)
        {
            FilteredPools.Add(pool);
        }
    }

    private void OnPoolSelected(SelectablePool pool)
    {
        SelectedPool = pool;
    }

    private async Task OnStartSession()
    {
        if (SelectedPool == null) return;

        IsBusy = true;
        try
        {
            // Create a new session
            var session = new WorkoutGamifier.Core.Models.Session
            {
                WorkoutPoolId = SelectedPool.Id,
                StartTime = DateTime.UtcNow,
                Status = WorkoutGamifier.Core.Models.SessionStatus.InProgress
            };

            var createdSession = await _sessionService.StartSessionAsync("Session", SelectedPool.Id);

            // Navigate to active session page
            var parameters = new Dictionary<string, object>
            {
                { "SessionId", createdSession.Id }
            };
            await Shell.Current.GoToAsync("ActiveSessionPage", parameters);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to start session: {ex.Message}", "OK");
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

    private async Task OnCreatePool()
    {
        await Shell.Current.GoToAsync("PoolFormPage");
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class SelectablePool : INotifyPropertyChanged
{
    private bool _isSelected;

    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int WorkoutCount { get; set; }

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
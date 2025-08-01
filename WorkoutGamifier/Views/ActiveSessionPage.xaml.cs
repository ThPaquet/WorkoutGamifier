using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WorkoutGamifier.Models;
using WorkoutGamifier.Services;

namespace WorkoutGamifier.Views;

[QueryProperty(nameof(SessionId), "SessionId")]
public partial class ActiveSessionPage : ContentPage, INotifyPropertyChanged
{
    private readonly ISessionService _sessionService;
    private readonly IActionService _actionService;
    private readonly IWorkoutPoolService _workoutPoolService;
    
    private int _sessionId;
    private Session? _session;
    private WorkoutPool? _pool;
    private bool _isBusy = false;
    private int _currentPoints = 0;
    private int _workoutCost = 10;
    private WorkoutViewModel? _currentWorkout;
    private System.Timers.Timer? _timer;
    private DateTime _sessionStartTime;
    private TimeSpan _elapsedTime = TimeSpan.Zero;
    
    private ObservableCollection<ActionViewModel> _availableActions = new();
    private ObservableCollection<WorkoutHistoryViewModel> _workoutHistory = new();

    public ActiveSessionPage(ISessionService sessionService, IActionService actionService, IWorkoutPoolService workoutPoolService)
    {
        InitializeComponent();
        _sessionService = sessionService;
        _actionService = actionService;
        _workoutPoolService = workoutPoolService;
        
        CompleteActionCommand = new Command<ActionViewModel>(async (action) => await OnCompleteAction(action));
        GetWorkoutCommand = new Command(async () => await OnGetWorkout(), () => CanGetWorkout);
        MarkWorkoutDoneCommand = new Command(OnMarkWorkoutDone);
        PauseSessionCommand = new Command(async () => await OnPauseSession());
        EndSessionCommand = new Command(async () => await OnEndSession());
        
        BindingContext = this;
        
        // Start the timer for elapsed time
        _timer = new System.Timers.Timer(1000); // Update every second
        _timer.Elapsed += (s, e) => 
        {
            _elapsedTime = DateTime.UtcNow - _sessionStartTime;
            MainThread.BeginInvokeOnMainThread(() => 
            {
                OnPropertyChanged(nameof(ElapsedTime));
            });
        };
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    public int SessionId
    {
        get => _sessionId;
        set
        {
            _sessionId = value;
            OnPropertyChanged();
            _ = LoadSessionDataAsync(value);
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }

    public string SessionName => _session?.Name ?? "Active Session";
    public string PoolName => _pool?.Name ?? "Unknown Pool";
    public string ElapsedTime => $"{(int)_elapsedTime.TotalHours:00}:{_elapsedTime.Minutes:00}:{_elapsedTime.Seconds:00}";

    public int CurrentPoints
    {
        get => _currentPoints;
        set
        {
            _currentPoints = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanGetWorkout));
            ((Command)GetWorkoutCommand).ChangeCanExecute();
        }
    }

    public int WorkoutCost
    {
        get => _workoutCost;
        set
        {
            _workoutCost = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanGetWorkout));
            ((Command)GetWorkoutCommand).ChangeCanExecute();
        }
    }

    public bool CanGetWorkout => CurrentPoints >= WorkoutCost;

    public WorkoutViewModel? CurrentWorkout
    {
        get => _currentWorkout;
        set
        {
            _currentWorkout = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasCurrentWorkout));
        }
    }

    public bool HasCurrentWorkout => CurrentWorkout != null;

    public ObservableCollection<ActionViewModel> AvailableActions
    {
        get => _availableActions;
        set
        {
            _availableActions = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<WorkoutHistoryViewModel> WorkoutHistory
    {
        get => _workoutHistory;
        set
        {
            _workoutHistory = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasWorkoutHistory));
        }
    }

    public bool HasWorkoutHistory => WorkoutHistory.Any();

    public ICommand CompleteActionCommand { get; }
    public ICommand GetWorkoutCommand { get; }
    public ICommand MarkWorkoutDoneCommand { get; }
    public ICommand PauseSessionCommand { get; }
    public ICommand EndSessionCommand { get; }

    private async Task LoadSessionDataAsync(int sessionId)
    {
        IsBusy = true;
        try
        {
            // Load session
            _session = await _sessionService.GetSessionByIdAsync(sessionId);
            if (_session == null)
            {
                await DisplayAlert("Error", "Session not found", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            // Load pool
            _pool = await _workoutPoolService.GetPoolByIdAsync(_session.WorkoutPoolId);
            
            // Set session start time and start timer
            _sessionStartTime = _session.StartTime;
            _elapsedTime = DateTime.UtcNow - _sessionStartTime;
            _timer?.Start();
            
            // Update current points
            CurrentPoints = _session.CurrentPointBalance;
            
            // Load available actions
            await LoadActionsAsync();
            
            // Load workout history
            await LoadWorkoutHistoryAsync();
            
            // Update UI
            OnPropertyChanged(nameof(SessionName));
            OnPropertyChanged(nameof(PoolName));
            OnPropertyChanged(nameof(ElapsedTime));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load session: {ex.Message}", "OK");
            await Shell.Current.GoToAsync("..");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadActionsAsync()
    {
        try
        {
            var actions = await _actionService.GetAllActionsAsync();
            var actionViewModels = new List<ActionViewModel>();

            foreach (var action in actions)
            {
                // Count how many times this action was completed in this session
                var completionCount = _session?.ActionCompletions?.Count(ac => ac.ActionId == action.Id) ?? 0;
                
                actionViewModels.Add(new ActionViewModel
                {
                    Id = action.Id,
                    Description = action.Description,
                    PointValue = action.PointValue,
                    CompletionCount = completionCount
                });
            }

            AvailableActions.Clear();
            foreach (var action in actionViewModels)
            {
                AvailableActions.Add(action);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load actions: {ex.Message}", "OK");
        }
    }

    private async Task LoadWorkoutHistoryAsync()
    {
        try
        {
            if (_session?.WorkoutReceived == null) return;

            var historyViewModels = new List<WorkoutHistoryViewModel>();
            
            foreach (var workoutReceived in _session.WorkoutReceived.OrderByDescending(wr => wr.ReceivedAt))
            {
                historyViewModels.Add(new WorkoutHistoryViewModel
                {
                    WorkoutName = workoutReceived.Workout?.Name ?? "Unknown Workout",
                    ReceivedAt = workoutReceived.ReceivedAt,
                    PointsSpent = workoutReceived.PointsSpent
                });
            }

            WorkoutHistory.Clear();
            foreach (var history in historyViewModels)
            {
                WorkoutHistory.Add(history);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load workout history: {ex.Message}", "OK");
        }
    }

    private async Task OnCompleteAction(ActionViewModel action)
    {
        if (_session == null || action == null) return;

        IsBusy = true;
        try
        {
            var completion = await _sessionService.CompleteActionAsync(_session.Id, action.Id);
            
            // Update current points
            CurrentPoints += action.PointValue;
            
            // Update action completion count
            action.CompletionCount++;
            
            await DisplayAlert("Success", $"Action completed! +{action.PointValue} points", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to complete action: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnGetWorkout()
    {
        if (_session == null || !CanGetWorkout) return;

        IsBusy = true;
        try
        {
            var workoutReceived = await _sessionService.SpendPointsForWorkoutAsync(_session.Id, WorkoutCost);
            
            // Update current points
            CurrentPoints -= WorkoutCost;
            
            // Set current workout
            CurrentWorkout = new WorkoutViewModel(workoutReceived.Workout);
            
            // Add to workout history
            WorkoutHistory.Insert(0, new WorkoutHistoryViewModel
            {
                WorkoutName = workoutReceived.Workout.Name,
                ReceivedAt = workoutReceived.ReceivedAt,
                PointsSpent = workoutReceived.PointsSpent
            });
            
            OnPropertyChanged(nameof(HasWorkoutHistory));
            
            await DisplayAlert("New Workout!", $"You received: {workoutReceived.Workout.Name}", "Let's Go!");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to get workout: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnMarkWorkoutDone()
    {
        CurrentWorkout = null;
    }

    private async Task OnPauseSession()
    {
        var result = await DisplayAlert("Pause Session", "Do you want to pause this session? You can resume it later.", "Pause", "Cancel");
        if (result)
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    private async Task OnEndSession()
    {
        if (_session == null) return;

        var result = await DisplayAlert("End Session", 
            $"Are you sure you want to end this session?\n\nPoints earned: {_session.PointsEarned}\nPoints spent: {_session.PointsSpent}\nFinal balance: {CurrentPoints}", 
            "End Session", "Cancel");
        
        if (result)
        {
            IsBusy = true;
            try
            {
                await _sessionService.EndSessionAsync(_session.Id);
                await DisplayAlert("Session Ended", "Your session has been completed successfully!", "OK");
                await Shell.Current.GoToAsync("//SessionsPage");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to end session: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class ActionViewModel : INotifyPropertyChanged
{
    private int _completionCount;

    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int PointValue { get; set; }

    public int CompletionCount
    {
        get => _completionCount;
        set
        {
            _completionCount = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasCompletions));
        }
    }

    public bool HasCompletions => CompletionCount > 0;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class WorkoutViewModel
{
    private readonly Workout _workout;

    public WorkoutViewModel(Workout workout)
    {
        _workout = workout;
    }

    public string Name => _workout.Name;
    public string? Description => _workout.Description;
    public string? Instructions => _workout.Instructions;
    public string Difficulty => _workout.Difficulty.ToString();
    public int DurationMinutes => _workout.DurationMinutes;
}

public class WorkoutHistoryViewModel
{
    public string WorkoutName { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public int PointsSpent { get; set; }
}
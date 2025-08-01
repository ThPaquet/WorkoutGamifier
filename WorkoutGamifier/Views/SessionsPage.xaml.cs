using WorkoutGamifier.Services;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Views;

public partial class SessionsPage : ContentPage
{
    private readonly ISessionService _sessionService;
    private readonly IWorkoutPoolService _workoutPoolService;

    public SessionsPage(ISessionService sessionService, IWorkoutPoolService workoutPoolService)
    {
        InitializeComponent();
        _sessionService = sessionService;
        _workoutPoolService = workoutPoolService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSessionsAsync();
        await CheckActiveSessionAsync();
    }

    private async Task LoadSessionsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        
        try
        {
            var sessions = await _sessionService.GetAllSessionsAsync();
            var sessionViewModels = new List<SessionViewModel>();

            foreach (var session in sessions)
            {
                var pool = await _workoutPoolService.GetPoolByIdAsync(session.WorkoutPoolId);
                
                // Calculate duration
                TimeSpan duration;
                if (session.EndTime.HasValue)
                {
                    duration = session.EndTime.Value - session.StartTime;
                }
                else
                {
                    duration = DateTime.UtcNow - session.StartTime;
                }
                
                sessionViewModels.Add(new SessionViewModel
                {
                    Id = session.Id,
                    Name = $"Session {session.Id}",
                    PoolName = pool?.Name ?? "Unknown Pool",
                    StartTime = session.StartTime,
                    EndTime = session.EndTime,
                    Status = session.Status.ToString(),
                    StatusColor = GetStatusColor(session.Status),
                    Duration = FormatDuration(duration),
                    CurrentPointBalance = session.CurrentPointBalance
                });
            }

            // Sort by start time (newest first)
            sessionViewModels = sessionViewModels.OrderByDescending(s => s.StartTime).ToList();
            SessionsCollectionView.ItemsSource = sessionViewModels;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load sessions: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async Task CheckActiveSessionAsync()
    {
        try
        {
            var activeSession = await _sessionService.GetActiveSessionAsync();
            if (activeSession != null)
            {
                var pool = await _workoutPoolService.GetPoolByIdAsync(activeSession.WorkoutPoolId);
                
                ActiveSessionFrame.IsVisible = true;
                ActiveSessionName.Text = $"Session {activeSession.Id} - {pool?.Name ?? "Unknown Pool"}";
                ActiveSessionPoints.Text = $"Points: {activeSession.CurrentPointBalance}";
                StartSessionBtn.Text = "End Current Session";
            }
            else
            {
                ActiveSessionFrame.IsVisible = false;
                StartSessionBtn.Text = "Start New Session";
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to check active session: {ex.Message}", "OK");
        }
    }

    private string FormatDuration(TimeSpan duration)
    {
        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}h {duration.Minutes}m";
        }
        else
        {
            return $"{duration.Minutes}m {duration.Seconds}s";
        }
    }

    private string GetStatusColor(SessionStatus status)
    {
        return status switch
        {
            SessionStatus.InProgress => "Orange",
            SessionStatus.Completed => "Green",
            SessionStatus.Cancelled => "Red",
            _ => "Gray"
        };
    }

    private async void OnStartSessionClicked(object sender, EventArgs e)
    {
        try
        {
            var activeSession = await _sessionService.GetActiveSessionAsync();
            if (activeSession != null)
            {
                // End current session
                var result = await DisplayAlert("Confirm", "End the current session?", "Yes", "No");
                if (result)
                {
                    await _sessionService.EndSessionAsync(activeSession.Id);
                    await DisplayAlert("Success", "Session ended successfully!", "OK");
                    await CheckActiveSessionAsync();
                    await LoadSessionsAsync();
                }
            }
            else
            {
                // Navigate to session creation page
                await Shell.Current.GoToAsync("SessionCreatePage");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to manage session: {ex.Message}", "OK");
        }
    }

    private async void OnViewActiveSessionClicked(object sender, EventArgs e)
    {
        try
        {
            var activeSession = await _sessionService.GetActiveSessionAsync();
            if (activeSession != null)
            {
                // Navigate to active session interface
                var parameters = new Dictionary<string, object>
                {
                    { "SessionId", activeSession.Id }
                };
                await Shell.Current.GoToAsync("ActiveSessionPage", parameters);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to view active session: {ex.Message}", "OK");
        }
    }

    private async void OnViewSessionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SessionViewModel session)
        {
            // Navigate to session details or summary
            await DisplayAlert("Session Details", 
                $"Session: {session.Name}\n" +
                $"Pool: {session.PoolName}\n" +
                $"Status: {session.Status}\n" +
                $"Points: {session.CurrentPointBalance}\n" +
                $"Duration: {session.Duration}", 
                "OK");
        }
    }
}

public class SessionViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PoolName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string StatusColor { get; set; } = "Gray";
    public string Duration { get; set; } = string.Empty;
    public int CurrentPointBalance { get; set; }
}
using System.Collections.ObjectModel;
using WorkoutGamifier.Services;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Views;

public partial class ActionsPage : ContentPage
{
    private readonly IActionService _actionService;
    private ObservableCollection<Models.Action> _actions;

    public ActionsPage(IActionService actionService)
    {
        InitializeComponent();
        _actionService = actionService;
        _actions = new ObservableCollection<Models.Action>();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadActionsAsync();
    }

    private async Task LoadActionsAsync()
    {
        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        
        try
        {
            var actions = await _actionService.GetAllActionsAsync();
            _actions.Clear();
            
            foreach (var action in actions)
            {
                _actions.Add(action);
            }
            
            ActionsCollectionView.ItemsSource = _actions;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load actions: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }

    private async void OnAddActionClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("ActionFormPage");
    }

    private async void OnEditActionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Models.Action action)
        {
            var parameters = new Dictionary<string, object>
            {
                { "Action", action }
            };
            await Shell.Current.GoToAsync("ActionFormPage", parameters);
        }
    }

    private async void OnDeleteActionClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Models.Action action)
        {
            try
            {
                // Check if action can be deleted (not used in active sessions)
                var canDelete = await _actionService.CanDeleteActionAsync(action.Id);
                if (!canDelete)
                {
                    await DisplayAlert("Cannot Delete", 
                        "This action cannot be deleted because it has been used in active sessions. " +
                        "You can create a new action instead.", "OK");
                    return;
                }

                var result = await DisplayAlert("Confirm Delete", 
                    $"Are you sure you want to delete the action '{action.Description}'?\n\n" +
                    "This action cannot be undone.", 
                    "Delete", "Cancel");

                if (result)
                {
                    await _actionService.DeleteActionAsync(action.Id);
                    _actions.Remove(action);
                    await DisplayAlert("Success", "Action deleted successfully!", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to delete action: {ex.Message}", "OK");
            }
        }
    }
}
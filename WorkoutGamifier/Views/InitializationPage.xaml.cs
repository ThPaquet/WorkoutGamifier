using WorkoutGamifier.Services;

namespace WorkoutGamifier.Views;

public partial class InitializationPage : ContentPage
{
    private readonly IAppInitializationService _initializationService;

    public InitializationPage(IAppInitializationService initializationService)
    {
        InitializeComponent();
        _initializationService = initializationService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await InitializeAppAsync();
    }

    private async Task InitializeAppAsync()
    {
        try
        {
            // Show loading state
            LoadingIndicator.IsRunning = true;
            ErrorPanel.IsVisible = false;
            StatusLabel.Text = "Initializing app...";

            // Perform initialization
            var result = await _initializationService.InitializeAppAsync();

            if (result.InitializationSuccessful)
            {
                // Update status for successful initialization
                StatusLabel.Text = "Initialization complete!";
                
                // Small delay to show completion message
                await Task.Delay(500);
                
                // Navigate based on whether it's first run
                if (result.IsFirstRun)
                {
                    await Shell.Current.GoToAsync("//welcome");
                }
                else
                {
                    await Shell.Current.GoToAsync("//workouts");
                }
            }
            else
            {
                // Show error state
                ShowError(result.ErrorMessage ?? "An unknown error occurred during initialization.");
            }
        }
        catch (Exception ex)
        {
            ShowError($"Failed to initialize app: {ex.Message}");
        }
    }

    private void ShowError(string errorMessage)
    {
        LoadingIndicator.IsRunning = false;
        ErrorPanel.IsVisible = true;
        ErrorMessageLabel.Text = errorMessage;
    }

    private async void OnRetryClicked(object sender, EventArgs e)
    {
        await InitializeAppAsync();
    }

    private async void OnContinueAnywayClicked(object sender, EventArgs e)
    {
        // Navigate to main app even if initialization failed
        await Shell.Current.GoToAsync("//workouts");
    }
}
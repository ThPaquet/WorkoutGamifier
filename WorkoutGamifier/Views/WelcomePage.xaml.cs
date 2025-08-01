namespace WorkoutGamifier.Views;

public partial class WelcomePage : ContentPage
{
    public WelcomePage()
    {
        InitializeComponent();
    }

    private async void OnGetStartedClicked(object sender, EventArgs e)
    {
        // Navigate to the main app shell
        await Shell.Current.GoToAsync("//workouts");
    }

    private async void OnSkipClicked(object sender, EventArgs e)
    {
        // Navigate to the main app shell
        await Shell.Current.GoToAsync("//workouts");
    }
}
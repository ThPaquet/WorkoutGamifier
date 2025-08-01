using WorkoutGamifier.Services;

namespace WorkoutGamifier;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		// Start with initialization page instead of main shell
		var window = new Window(new AppShell());
		return window;
	}
}
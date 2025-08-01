namespace WorkoutGamifier;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
		// Register routes for navigation
		Routing.RegisterRoute("WorkoutDetailPage", typeof(Views.WorkoutDetailPage));
		Routing.RegisterRoute("WorkoutPoolDetailPage", typeof(Views.WorkoutPoolDetailPage));
		Routing.RegisterRoute("PoolFormPage", typeof(Views.PoolFormPage));
		Routing.RegisterRoute("WorkoutSelectionPage", typeof(Views.WorkoutSelectionPage));
		Routing.RegisterRoute("ActionFormPage", typeof(Views.ActionFormPage));
		Routing.RegisterRoute("SessionCreatePage", typeof(Views.SessionCreatePage));
		Routing.RegisterRoute("ActiveSessionPage", typeof(Views.ActiveSessionPage));
	}
}

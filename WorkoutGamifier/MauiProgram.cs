using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using WorkoutGamifier.Data;
using WorkoutGamifier.Services;
using WorkoutGamifier.Repositories;

namespace WorkoutGamifier;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		// Configure SQLite database
		var dbPath = Path.Combine(FileSystem.AppDataDirectory, "workoutgamifier.db");
		builder.Services.AddDbContext<AppDbContext>(options =>
			options.UseSqlite($"Data Source={dbPath}"));

		// Register repositories
		builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
		builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

		// Register services
		builder.Services.AddScoped<IDataSeedingService, DataSeedingService>();
		builder.Services.AddScoped<IAppInitializationService, AppInitializationService>();
		builder.Services.AddScoped<IWorkoutService, WorkoutService>();
		builder.Services.AddScoped<IWorkoutPoolService, WorkoutPoolService>();
		builder.Services.AddScoped<IActionService, ActionService>();
		builder.Services.AddScoped<ISessionService, SessionService>();
		builder.Services.AddScoped<IStatisticsService, StatisticsService>();
		builder.Services.AddScoped<WorkoutGamifier.Core.Services.IBackupService, WorkoutGamifier.Core.Services.BackupService>();
		builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
		builder.Services.AddScoped<IValidationService, ValidationService>();
		builder.Services.AddScoped<IToastService, ToastService>();

		// Register pages
		builder.Services.AddTransient<Views.InitializationPage>();
		builder.Services.AddTransient<Views.WelcomePage>();
		builder.Services.AddTransient<Views.WorkoutsPage>();
		builder.Services.AddTransient<Views.WorkoutPoolsPage>();
		builder.Services.AddTransient<Views.SessionsPage>();
		builder.Services.AddTransient<Views.ActionsPage>();
		builder.Services.AddTransient<Views.ProfilePage>();
		builder.Services.AddTransient<Views.WorkoutDetailPage>();
		builder.Services.AddTransient<Views.WorkoutPoolDetailPage>();
		builder.Services.AddTransient<Views.PoolFormPage>();
		builder.Services.AddTransient<Views.WorkoutSelectionPage>();
		builder.Services.AddTransient<Views.ActionFormPage>();
		builder.Services.AddTransient<Views.SessionCreatePage>();
		builder.Services.AddTransient<Views.ActiveSessionPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

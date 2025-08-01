namespace WorkoutGamifier.Services;

public interface IToastService
{
    Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 3000);
}

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}
namespace WorkoutGamifier.Services;

public class ToastService : IToastService
{
    public async Task ShowToastAsync(string message, ToastType type = ToastType.Info, int durationMs = 3000)
    {
        // For now, we'll use DisplayAlert as a fallback
        // In a real implementation, you might use a third-party toast library
        // or implement a custom toast overlay
        
        var title = GetTitleFromType(type);
        var icon = GetIconFromType(type);
        
        if (Application.Current?.MainPage != null)
        {
            // Show as a brief alert that auto-dismisses
            var alertTask = Application.Current.MainPage.DisplayAlert($"{icon} {title}", message, "OK");
            
            // Auto-dismiss after specified duration
            var timeoutTask = Task.Delay(durationMs);
            await Task.WhenAny(alertTask, timeoutTask);
        }
    }

    private string GetTitleFromType(ToastType type)
    {
        return type switch
        {
            ToastType.Info => "Info",
            ToastType.Success => "Success",
            ToastType.Warning => "Warning",
            ToastType.Error => "Error",
            _ => "Info"
        };
    }

    private string GetIconFromType(ToastType type)
    {
        return type switch
        {
            ToastType.Info => "ℹ️",
            ToastType.Success => "✅",
            ToastType.Warning => "⚠️",
            ToastType.Error => "❌",
            _ => "ℹ️"
        };
    }
}
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public class ErrorHandler : IErrorHandler
{
    public async Task HandleErrorAsync(Exception exception)
    {
        var appException = exception as AppException;
        if (appException != null)
        {
            await ShowUserErrorAsync(appException.UserMessage, GetSeverityFromCategory(appException.Category));
        }
        else
        {
            // Log the exception (in a real app, you'd use a logging framework)
            System.Diagnostics.Debug.WriteLine($"Unhandled exception: {exception}");
            
            // Show a generic error message to the user
            await ShowUserErrorAsync("An unexpected error occurred. Please try again.", ErrorSeverity.Error);
        }
    }

    public async Task ShowUserErrorAsync(string message, ErrorSeverity severity = ErrorSeverity.Error)
    {
        var title = GetTitleFromSeverity(severity);
        var icon = GetIconFromSeverity(severity);
        
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert($"{icon} {title}", message, "OK");
        }
    }

    public async Task<bool> PromptRetryAsync(string operation)
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(
                "⚠️ Operation Failed", 
                $"Failed to {operation}. Would you like to try again?", 
                "Retry", 
                "Cancel");
        }
        return false;
    }

    public async Task ShowValidationErrorAsync(string fieldName, string message)
    {
        await ShowUserErrorAsync($"{fieldName}: {message}", ErrorSeverity.Warning);
    }

    public async Task<bool> ConfirmDestructiveActionAsync(string title, string message, string confirmText = "Yes", string cancelText = "Cancel")
    {
        if (Application.Current?.MainPage != null)
        {
            return await Application.Current.MainPage.DisplayAlert(
                $"⚠️ {title}", 
                message, 
                confirmText, 
                cancelText);
        }
        return false;
    }

    private string GetTitleFromSeverity(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Info => "Information",
            ErrorSeverity.Warning => "Warning",
            ErrorSeverity.Error => "Error",
            ErrorSeverity.Critical => "Critical Error",
            _ => "Error"
        };
    }

    private string GetIconFromSeverity(ErrorSeverity severity)
    {
        return severity switch
        {
            ErrorSeverity.Info => "ℹ️",
            ErrorSeverity.Warning => "⚠️",
            ErrorSeverity.Error => "❌",
            ErrorSeverity.Critical => "🚨",
            _ => "❌"
        };
    }

    private ErrorSeverity GetSeverityFromCategory(ErrorCategory category)
    {
        return category switch
        {
            ErrorCategory.DataValidation => ErrorSeverity.Warning,
            ErrorCategory.Storage => ErrorSeverity.Error,
            ErrorCategory.BusinessLogic => ErrorSeverity.Warning,
            ErrorCategory.Application => ErrorSeverity.Critical,
            ErrorCategory.UserInput => ErrorSeverity.Warning,
            _ => ErrorSeverity.Error
        };
    }
}
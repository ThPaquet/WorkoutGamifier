using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public interface IErrorHandler
{
    Task HandleErrorAsync(Exception exception);
    Task ShowUserErrorAsync(string message, ErrorSeverity severity = ErrorSeverity.Error);
    Task<bool> PromptRetryAsync(string operation);
    Task ShowValidationErrorAsync(string fieldName, string message);
    Task<bool> ConfirmDestructiveActionAsync(string title, string message, string confirmText = "Yes", string cancelText = "Cancel");
}
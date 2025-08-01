using WorkoutGamifier.Services;
using WorkoutGamifier.Models;

namespace WorkoutGamifier.Views;

public abstract class BasePage : ContentPage
{
    protected IErrorHandler ErrorHandler { get; private set; }
    protected IValidationService ValidationService { get; private set; }

    protected BasePage(IErrorHandler errorHandler, IValidationService validationService)
    {
        ErrorHandler = errorHandler;
        ValidationService = validationService;
    }

    protected async Task<bool> ExecuteWithErrorHandlingAsync(Func<Task> operation, string operationName, bool showRetryOption = false)
    {
        try
        {
            await operation();
            return true;
        }
        catch (Exception ex)
        {
            await ErrorHandler.HandleErrorAsync(ex);
            
            if (showRetryOption && ex is AppException appEx && appEx.IsRetryable)
            {
                var retry = await ErrorHandler.PromptRetryAsync(operationName);
                if (retry)
                {
                    return await ExecuteWithErrorHandlingAsync(operation, operationName, showRetryOption);
                }
            }
            
            return false;
        }
    }

    protected async Task<T?> ExecuteWithErrorHandlingAsync<T>(Func<Task<T>> operation, string operationName, bool showRetryOption = false)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            await ErrorHandler.HandleErrorAsync(ex);
            
            if (showRetryOption && ex is AppException appEx && appEx.IsRetryable)
            {
                var retry = await ErrorHandler.PromptRetryAsync(operationName);
                if (retry)
                {
                    return await ExecuteWithErrorHandlingAsync(operation, operationName, showRetryOption);
                }
            }
            
            return default(T);
        }
    }

    protected async Task<bool> ShowValidationErrorsAsync(ValidationResult validationResult)
    {
        if (!validationResult.IsValid)
        {
            var errorMessage = string.Join("\n", validationResult.Errors.Select(e => $"• {e.FieldName}: {e.Message}"));
            await ErrorHandler.ShowUserErrorAsync($"Please fix the following errors:\n\n{errorMessage}", ErrorSeverity.Warning);
            return false;
        }
        return true;
    }

    protected async Task<bool> ConfirmDestructiveActionAsync(string title, string message, string confirmText = "Delete", string cancelText = "Cancel")
    {
        return await ErrorHandler.ConfirmDestructiveActionAsync(title, message, confirmText, cancelText);
    }

    protected async Task ShowSuccessMessageAsync(string message)
    {
        await ErrorHandler.ShowUserErrorAsync(message, ErrorSeverity.Info);
    }
}
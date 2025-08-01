using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public static class RetryHelper
{
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        TimeSpan? delay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        var actualDelay = delay ?? TimeSpan.FromSeconds(1);
        var retryCount = 0;

        while (true)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (retryCount < maxRetries)
            {
                // Check if we should retry this exception
                if (shouldRetry != null && !shouldRetry(ex))
                {
                    throw;
                }

                // Don't retry validation errors or business logic errors
                if (ex is ValidationException || ex is BusinessLogicException)
                {
                    throw;
                }

                retryCount++;
                
                // Exponential backoff
                var currentDelay = TimeSpan.FromMilliseconds(actualDelay.TotalMilliseconds * Math.Pow(2, retryCount - 1));
                await Task.Delay(currentDelay);
            }
        }
    }

    public static async Task ExecuteWithRetryAsync(
        Func<Task> operation,
        int maxRetries = 3,
        TimeSpan? delay = null,
        Func<Exception, bool>? shouldRetry = null)
    {
        await ExecuteWithRetryAsync(async () =>
        {
            await operation();
            return true; // Dummy return value
        }, maxRetries, delay, shouldRetry);
    }
}
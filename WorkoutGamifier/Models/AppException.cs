namespace WorkoutGamifier.Models;

public class AppException : Exception
{
    public ErrorCategory Category { get; }
    public string UserMessage { get; }
    public bool IsRetryable { get; }
    
    public AppException(ErrorCategory category, string userMessage, 
                       bool isRetryable = false, Exception? innerException = null)
        : base(userMessage, innerException)
    {
        Category = category;
        UserMessage = userMessage;
        IsRetryable = isRetryable;
    }
}

public class ValidationException : AppException
{
    public string FieldName { get; }
    
    public ValidationException(string fieldName, string message)
        : base(ErrorCategory.DataValidation, message, false)
    {
        FieldName = fieldName;
    }
}

public class BusinessLogicException : AppException
{
    public BusinessLogicException(string message, bool isRetryable = false)
        : base(ErrorCategory.BusinessLogic, message, isRetryable)
    {
    }
}

public class StorageException : AppException
{
    public StorageException(string message, bool isRetryable = true, Exception? innerException = null)
        : base(ErrorCategory.Storage, message, isRetryable, innerException)
    {
    }
}
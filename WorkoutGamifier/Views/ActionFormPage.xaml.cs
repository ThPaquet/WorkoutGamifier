using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WorkoutGamifier.Models;
using WorkoutGamifier.Services;

namespace WorkoutGamifier.Views;

[QueryProperty(nameof(Action), "Action")]
public partial class ActionFormPage : BasePage, INotifyPropertyChanged
{
    private readonly IActionService _actionService;
    private Models.Action? _action;
    private string _actionDescription = string.Empty;
    private string _pointValueText = string.Empty;
    private string _descriptionError = string.Empty;
    private string _pointValueError = string.Empty;
    private bool _isBusy;

    public ActionFormPage(IActionService actionService, IErrorHandler errorHandler, IValidationService validationService)
        : base(errorHandler, validationService)
    {
        InitializeComponent();
        _actionService = actionService;
        
        SaveCommand = new Command(async () => await OnSave(), () => CanSave);
        CancelCommand = new Command(async () => await OnCancel());
        
        BindingContext = this;
    }

    public Models.Action? Action
    {
        get => _action;
        set
        {
            _action = value;
            if (_action != null)
            {
                ActionDescription = _action.Description;
                PointValueText = _action.PointValue.ToString();
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public string ActionDescription
    {
        get => _actionDescription;
        set
        {
            _actionDescription = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(ShowPreview));
            OnPropertyChanged(nameof(CharacterCountText));
            ValidateDescription();
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }

    public string PointValueText
    {
        get => _pointValueText;
        set
        {
            _pointValueText = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
            OnPropertyChanged(nameof(ShowPreview));
            ValidatePointValue();
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }

    public string DescriptionError
    {
        get => _descriptionError;
        set
        {
            _descriptionError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasDescriptionError));
        }
    }

    public string PointValueError
    {
        get => _pointValueError;
        set
        {
            _pointValueError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasPointValueError));
        }
    }

    public new bool IsBusy
    {
        get => _isBusy;
        set
        {
            _isBusy = value;
            OnPropertyChanged();
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }

    public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
    public bool HasPointValueError => !string.IsNullOrEmpty(PointValueError);
    public string PageTitle => Action == null ? "Create New Action" : "Edit Action";
    public string SaveButtonText => Action == null ? "Create Action" : "Update Action";
    public string CharacterCountText => $"{ActionDescription.Length}/200 characters";
    public bool ShowPreview => !string.IsNullOrWhiteSpace(ActionDescription) && 
                              !string.IsNullOrWhiteSpace(PointValueText) && 
                              int.TryParse(PointValueText, out _);
    
    public bool CanSave => !string.IsNullOrWhiteSpace(ActionDescription) && 
                          !string.IsNullOrWhiteSpace(PointValueText) &&
                          !HasDescriptionError && 
                          !HasPointValueError && 
                          !IsBusy;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private void ValidateDescription()
    {
        if (string.IsNullOrWhiteSpace(ActionDescription))
        {
            DescriptionError = "Action description is required";
        }
        else if (ActionDescription.Length > 200)
        {
            DescriptionError = "Description cannot exceed 200 characters";
        }
        else
        {
            DescriptionError = string.Empty;
        }
    }

    private void ValidatePointValue()
    {
        if (string.IsNullOrWhiteSpace(PointValueText))
        {
            PointValueError = "Point value is required";
        }
        else if (!int.TryParse(PointValueText, out int points))
        {
            PointValueError = "Point value must be a valid number";
        }
        else if (points < 1)
        {
            PointValueError = "Point value must be at least 1";
        }
        else if (points > 1000)
        {
            PointValueError = "Point value cannot exceed 1000";
        }
        else
        {
            PointValueError = string.Empty;
        }
    }

    private async Task OnSave()
    {
        if (!CanSave) return;

        IsBusy = true;

        // Use the validation service for comprehensive validation
        var validationResult = ValidationService.ValidateAction(ActionDescription, PointValueText);
        
        if (!await ShowValidationErrorsAsync(validationResult))
        {
            IsBusy = false;
            return;
        }

        // Parse point value (we know it's valid from validation)
        int.TryParse(PointValueText, out int pointValue);

        var operationName = Action == null ? "create action" : "update action";
        var successMessage = Action == null ? "Action created successfully!" : "Action updated successfully!";

        var success = await ExecuteWithErrorHandlingAsync(
            async () =>
            {
                if (Action == null)
                {
                    // Create new action
                    var newAction = new Models.Action
                    {
                        Description = ActionDescription.Trim(),
                        PointValue = pointValue,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _actionService.CreateActionAsync(newAction);
                }
                else
                {
                    // Update existing action
                    Action.Description = ActionDescription.Trim();
                    Action.PointValue = pointValue;
                    Action.UpdatedAt = DateTime.UtcNow;

                    await _actionService.UpdateActionAsync(Action);
                }

                await ShowSuccessMessageAsync(successMessage);
                await Shell.Current.GoToAsync("..");
            },
            operationName,
            showRetryOption: true
        );

        IsBusy = false;
    }

    private async Task OnCancel()
    {
        await Shell.Current.GoToAsync("..");
    }

    public new event PropertyChangedEventHandler? PropertyChanged;

    protected new virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
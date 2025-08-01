using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WorkoutGamifier.Models;
using WorkoutGamifier.Services;

namespace WorkoutGamifier.Views;

[QueryProperty(nameof(Pool), "Pool")]
public partial class PoolFormPage : ContentPage, INotifyPropertyChanged
{
    private readonly IWorkoutPoolService _workoutPoolService;
    private WorkoutPool? _pool;
    private string _poolName = string.Empty;
    private string _poolDescription = string.Empty;
    private string _nameError = string.Empty;
    private string _descriptionError = string.Empty;
    private bool _isBusy;

    public PoolFormPage(IWorkoutPoolService workoutPoolService)
    {
        InitializeComponent();
        _workoutPoolService = workoutPoolService;
        
        SaveCommand = new Command(async () => await OnSave(), () => CanSave);
        CancelCommand = new Command(async () => await OnCancel());
        
        BindingContext = this;
    }

    public WorkoutPool? Pool
    {
        get => _pool;
        set
        {
            _pool = value;
            if (_pool != null)
            {
                PoolName = _pool.Name;
                PoolDescription = _pool.Description ?? string.Empty;
            }
            OnPropertyChanged();
            OnPropertyChanged(nameof(PageTitle));
            OnPropertyChanged(nameof(SaveButtonText));
        }
    }

    public string PoolName
    {
        get => _poolName;
        set
        {
            _poolName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
            ValidateName();
            ((Command)SaveCommand).ChangeCanExecute();
        }
    }

    public string PoolDescription
    {
        get => _poolDescription;
        set
        {
            _poolDescription = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CharacterCountText));
            ValidateDescription();
        }
    }

    public string NameError
    {
        get => _nameError;
        set
        {
            _nameError = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasNameError));
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

    public bool HasNameError => !string.IsNullOrEmpty(NameError);
    public bool HasDescriptionError => !string.IsNullOrEmpty(DescriptionError);
    public string PageTitle => Pool == null ? "Create New Pool" : "Edit Pool";
    public string SaveButtonText => Pool == null ? "Create Pool" : "Update Pool";
    public string CharacterCountText => $"{PoolDescription.Length}/500 characters";
    
    public bool CanSave => !string.IsNullOrWhiteSpace(PoolName) && 
                          !HasNameError && 
                          !HasDescriptionError && 
                          !IsBusy;

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    private void ValidateName()
    {
        if (string.IsNullOrWhiteSpace(PoolName))
        {
            NameError = "Pool name is required";
        }
        else if (PoolName.Length > 100)
        {
            NameError = "Pool name cannot exceed 100 characters";
        }
        else
        {
            NameError = string.Empty;
        }
    }

    private void ValidateDescription()
    {
        if (PoolDescription.Length > 500)
        {
            DescriptionError = "Description cannot exceed 500 characters";
        }
        else
        {
            DescriptionError = string.Empty;
        }
    }

    private async Task OnSave()
    {
        if (!CanSave) return;

        IsBusy = true;
        try
        {
            if (Pool == null)
            {
                // Create new pool
                var newPool = new WorkoutPool
                {
                    Name = PoolName.Trim(),
                    Description = string.IsNullOrWhiteSpace(PoolDescription) ? null : PoolDescription.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _workoutPoolService.CreatePoolAsync(newPool);
                await DisplayAlert("Success", "Pool created successfully!", "OK");
            }
            else
            {
                // Update existing pool
                Pool.Name = PoolName.Trim();
                Pool.Description = string.IsNullOrWhiteSpace(PoolDescription) ? null : PoolDescription.Trim();
                Pool.UpdatedAt = DateTime.UtcNow;

                await _workoutPoolService.UpdatePoolAsync(Pool);
                await DisplayAlert("Success", "Pool updated successfully!", "OK");
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to save pool: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
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
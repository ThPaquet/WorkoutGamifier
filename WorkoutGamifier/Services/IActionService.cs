using WorkoutGamifier.Models;

namespace WorkoutGamifier.Services;

public interface IActionService
{
    Task<List<Models.Action>> GetAllActionsAsync();
    Task<Models.Action?> GetActionByIdAsync(int id);
    Task<Models.Action> CreateActionAsync(Models.Action action);
    Task<Models.Action> UpdateActionAsync(Models.Action action);
    Task DeleteActionAsync(int actionId);
    Task<bool> CanDeleteActionAsync(int actionId);
}
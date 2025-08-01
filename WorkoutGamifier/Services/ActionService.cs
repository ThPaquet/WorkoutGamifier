using WorkoutGamifier.Models;
using WorkoutGamifier.Repositories;

namespace WorkoutGamifier.Services;

public class ActionService : IActionService
{
    private readonly IUnitOfWork _unitOfWork;

    public ActionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Models.Action>> GetAllActionsAsync()
    {
        return await _unitOfWork.Actions.GetAllAsync();
    }

    public async Task<Models.Action?> GetActionByIdAsync(int id)
    {
        return await _unitOfWork.Actions.GetByIdAsync(id);
    }

    public async Task<Models.Action> CreateActionAsync(Models.Action action)
    {
        ValidateAction(action);

        var createdAction = await _unitOfWork.Actions.CreateAsync(action);
        await _unitOfWork.SaveChangesAsync();

        return createdAction;
    }

    public async Task<Models.Action> UpdateActionAsync(Models.Action action)
    {
        ValidateAction(action);

        var existingAction = await _unitOfWork.Actions.GetByIdAsync(action.Id);
        if (existingAction == null)
        {
            throw new InvalidOperationException($"Action with ID {action.Id} not found.");
        }

        existingAction.Description = action.Description;
        existingAction.PointValue = action.PointValue;

        var updatedAction = await _unitOfWork.Actions.UpdateAsync(existingAction);
        await _unitOfWork.SaveChangesAsync();

        return updatedAction;
    }

    public async Task DeleteActionAsync(int actionId)
    {
        var action = await _unitOfWork.Actions.GetByIdAsync(actionId);
        if (action == null)
        {
            throw new InvalidOperationException($"Action with ID {actionId} not found.");
        }

        // Check if action can be deleted (not used in active sessions)
        if (!await CanDeleteActionAsync(actionId))
        {
            throw new InvalidOperationException("Cannot delete action that is currently used in active sessions.");
        }

        await _unitOfWork.Actions.DeleteAsync(actionId);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> CanDeleteActionAsync(int actionId)
    {
        // Check if action is used in any active sessions
        var allSessions = await _unitOfWork.Sessions.GetAllAsync();
        var activeSessions = allSessions.Where(s => s.Status == SessionStatus.Active).ToList();

        if (!activeSessions.Any())
        {
            return true; // No active sessions, safe to delete
        }

        // Check if any active session has completions for this action
        var allCompletions = await _unitOfWork.ActionCompletions.GetAllAsync();
        var activeSessionIds = activeSessions.Select(s => s.Id).ToList();
        
        return !allCompletions.Any(ac => 
            activeSessionIds.Contains(ac.SessionId) && 
            ac.ActionId == actionId);
    }

    private void ValidateAction(Models.Action action)
    {
        if (string.IsNullOrWhiteSpace(action.Description))
        {
            throw new ArgumentException("Action description is required.");
        }

        if (action.Description.Length > 200)
        {
            throw new ArgumentException("Action description cannot exceed 200 characters.");
        }

        if (action.PointValue <= 0)
        {
            throw new ArgumentException("Point value must be a positive integer.");
        }

        if (action.PointValue > 1000)
        {
            throw new ArgumentException("Point value cannot exceed 1000 points.");
        }
    }
}
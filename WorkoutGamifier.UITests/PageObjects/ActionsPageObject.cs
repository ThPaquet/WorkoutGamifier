using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using WorkoutGamifier.UITests.Configuration;

namespace WorkoutGamifier.UITests.PageObjects;

public class ActionsPageObject : PageObjectBase
{
    public ActionsPageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config)
    {
    }

    #region Page Identification

    protected override By PageIdentifier => By.XPath("//android.widget.TextView[@text='Actions']");
    protected override string ExpectedPageTitle => "Actions";

    #endregion

    #region Element Locators

    // Main elements
    public By ActionsTab => By.XPath("//android.widget.TextView[@text='Actions']");
    public By CreateActionButton => By.XPath("//android.widget.Button[contains(@text, 'Create') or contains(@text, 'Add Action') or contains(@text, 'New Action')]");
    public By ActionsList => By.XPath("//*[contains(@resource-id, 'actions') or contains(@class, 'list')]");
    
    // Action list items
    public By ActionListItems => By.XPath("//android.widget.TextView[contains(@text, 'Complete') or contains(@text, 'Do') or contains(@text, 'Perform')]");
    public By ActionDescriptions => By.XPath("//*[contains(@class, 'action-description') or contains(@resource-id, 'action_description')]");
    public By ActionPointValues => By.XPath("//*[contains(@text, 'point') or contains(@text, 'pts') or matches(@text, '\\d+\\s*(point|pts)')]");
    
    // Action buttons
    public By CompleteActionButton => By.XPath("//android.widget.Button[contains(@text, 'Complete') or contains(@text, 'Done') or contains(@text, 'Finish')]");
    public By EditActionButton => By.XPath("//android.widget.Button[contains(@text, 'Edit') or contains(@text, 'Modify')]");
    public By DeleteActionButton => By.XPath("//android.widget.Button[contains(@text, 'Delete') or contains(@text, 'Remove')]");
    
    // Action status indicators
    public By CompletedActionIndicator => By.XPath("//*[contains(@text, 'Completed') or contains(@text, 'Done') or contains(@class, 'completed')]");
    public By AvailableActionIndicator => By.XPath("//*[contains(@text, 'Available') or contains(@text, 'Ready') or contains(@class, 'available')]");
    
    // Points and progress
    public By TotalPointsDisplay => By.XPath("//*[contains(@text, 'Total') and contains(@text, 'point')]");
    public By EarnedPointsDisplay => By.XPath("//*[contains(@text, 'Earned') and contains(@text, 'point')]");
    public By ProgressIndicator => By.XPath("//*[contains(@class, 'progress') or contains(@resource-id, 'progress')]");
    
    // Empty state
    public By NoActionsMessage => By.XPath("//*[contains(@text, 'No actions') or contains(@text, 'Create your first')]");
    public By EmptyStateImage => By.XPath("//android.widget.Image");

    #endregion

    #region Page Actions

    /// <summary>
    /// Create a new action
    /// </summary>
    public async Task<bool> CreateNewAction()
    {
        await WaitForPageToLoad();
        return await TapElement(CreateActionButton);
    }

    /// <summary>
    /// Get list of all action descriptions displayed
    /// </summary>
    public async Task<List<string>> GetActionDescriptions()
    {
        await WaitForPageToLoad();
        await Task.Delay(1000); // Allow actions to load
        
        // Try to get action descriptions from specific elements first
        var descriptions = GetElementTexts(ActionDescriptions);
        if (descriptions.Any())
        {
            return descriptions;
        }
        
        // Fallback to general action list items
        return GetElementTexts(ActionListItems);
    }

    /// <summary>
    /// Get the count of actions displayed
    /// </summary>
    public async Task<int> GetActionCount()
    {
        await WaitForPageToLoad();
        return await GetElementCount(ActionListItems);
    }

    /// <summary>
    /// Complete a specific action by description
    /// </summary>
    public async Task<bool> CompleteAction(string actionDescription)
    {
        await WaitForPageToLoad();
        
        // First try to find the action by description
        var actionLocator = By.XPath($"//android.widget.TextView[contains(@text, '{actionDescription}')]");
        if (await TapElement(actionLocator))
        {
            // Look for complete button
            return await TapElement(CompleteActionButton);
        }
        
        // If that doesn't work, try scrolling to find it
        await ScrollToElement(actionLocator);
        if (await TapElement(actionLocator))
        {
            return await TapElement(CompleteActionButton);
        }
        
        return false;
    }

    /// <summary>
    /// Complete the first available action
    /// </summary>
    public async Task<bool> CompleteFirstAvailableAction()
    {
        await WaitForPageToLoad();
        
        // Look for the first available action
        var availableActions = GetElements(AvailableActionIndicator);
        if (availableActions.Any())
        {
            availableActions.First().Click();
            await Task.Delay(500);
            return await TapElement(CompleteActionButton);
        }
        
        // If no specific available indicator, try the first action
        var firstAction = GetElements(ActionListItems).FirstOrDefault();
        if (firstAction != null)
        {
            firstAction.Click();
            await Task.Delay(500);
            return await TapElement(CompleteActionButton);
        }
        
        return false;
    }

    /// <summary>
    /// Get point value for a specific action
    /// </summary>
    public async Task<int> GetActionPointValue(string actionDescription)
    {
        await WaitForPageToLoad();
        
        // Navigate to the action first
        var actionLocator = By.XPath($"//android.widget.TextView[contains(@text, '{actionDescription}')]");
        if (await TapElement(actionLocator))
        {
            // Look for point value
            var pointText = GetElementText(ActionPointValues);
            if (!string.IsNullOrEmpty(pointText))
            {
                // Extract number from text like "5 points" or "Points: 5"
                var match = System.Text.RegularExpressions.Regex.Match(pointText, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int points))
                {
                    return points;
                }
            }
        }
        
        return 0;
    }

    /// <summary>
    /// Get total points earned from completed actions
    /// </summary>
    public async Task<int> GetTotalPointsEarned()
    {
        await WaitForPageToLoad();
        
        var pointsText = GetElementText(EarnedPointsDisplay);
        if (!string.IsNullOrEmpty(pointsText))
        {
            var match = System.Text.RegularExpressions.Regex.Match(pointsText, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int points))
            {
                return points;
            }
        }
        
        return 0;
    }

    /// <summary>
    /// Get count of completed actions
    /// </summary>
    public async Task<int> GetCompletedActionCount()
    {
        await WaitForPageToLoad();
        return await GetElementCount(CompletedActionIndicator);
    }

    /// <summary>
    /// Get count of available (not completed) actions
    /// </summary>
    public async Task<int> GetAvailableActionCount()
    {
        await WaitForPageToLoad();
        var totalActions = await GetActionCount();
        var completedActions = await GetCompletedActionCount();
        return totalActions - completedActions;
    }

    /// <summary>
    /// Edit a specific action
    /// </summary>
    public async Task<bool> EditAction(string actionDescription)
    {
        await WaitForPageToLoad();
        
        // First navigate to the action
        var actionLocator = By.XPath($"//android.widget.TextView[contains(@text, '{actionDescription}')]");
        if (await TapElement(actionLocator))
        {
            // Then look for edit button
            return await TapElement(EditActionButton);
        }
        
        return false;
    }

    /// <summary>
    /// Delete a specific action
    /// </summary>
    public async Task<bool> DeleteAction(string actionDescription)
    {
        await WaitForPageToLoad();
        
        // First navigate to the action
        var actionLocator = By.XPath($"//android.widget.TextView[contains(@text, '{actionDescription}')]");
        if (await TapElement(actionLocator))
        {
            // Then look for delete button
            if (await TapElement(DeleteActionButton))
            {
                // Handle confirmation dialog if it appears
                var confirmButton = By.XPath("//android.widget.Button[contains(@text, 'Delete') or contains(@text, 'Confirm') or contains(@text, 'Yes')]");
                await Task.Delay(1000); // Wait for dialog
                if (IsElementPresent(confirmButton))
                {
                    return await TapElement(confirmButton);
                }
                return true;
            }
        }
        
        return false;
    }

    /// <summary>
    /// Check if the page is showing empty state (no actions)
    /// </summary>
    public async Task<bool> IsEmptyState()
    {
        await WaitForPageToLoad();
        return IsElementPresent(NoActionsMessage) || (await GetActionCount() == 0);
    }

    /// <summary>
    /// Verify that the actions page is properly loaded with expected elements
    /// </summary>
    public async Task<bool> VerifyPageLoaded()
    {
        if (!await WaitForPageToLoad())
        {
            return false;
        }

        // Check for either actions list or empty state
        var hasActionsList = IsElementPresent(ActionsList);
        var hasEmptyState = IsElementPresent(NoActionsMessage);
        var hasCreateButton = IsElementPresent(CreateActionButton);

        return hasCreateButton && (hasActionsList || hasEmptyState);
    }

    /// <summary>
    /// Wait for actions to load and display
    /// </summary>
    public async Task<bool> WaitForActionsToLoad(int timeoutSeconds = 15)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime)
        {
            // Check if actions have loaded or empty state is shown
            if (await GetActionCount() > 0 || IsElementPresent(NoActionsMessage))
            {
                return true;
            }
            
            await Task.Delay(1000);
        }
        
        return false;
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Verify that an action with the given description exists
    /// </summary>
    public async Task<bool> VerifyActionExists(string actionDescription)
    {
        var actionDescriptions = await GetActionDescriptions();
        return actionDescriptions.Any(desc => desc.Contains(actionDescription, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verify that an action is marked as completed
    /// </summary>
    public async Task<bool> VerifyActionCompleted(string actionDescription)
    {
        await WaitForPageToLoad();
        
        // Navigate to the action first
        var actionLocator = By.XPath($"//android.widget.TextView[contains(@text, '{actionDescription}')]");
        if (await TapElement(actionLocator))
        {
            return IsElementPresent(CompletedActionIndicator);
        }
        
        return false;
    }

    /// <summary>
    /// Verify that an action is available (not completed)
    /// </summary>
    public async Task<bool> VerifyActionAvailable(string actionDescription)
    {
        await WaitForPageToLoad();
        
        // Navigate to the action first
        var actionLocator = By.XPath($"//android.widget.TextView[contains(@text, '{actionDescription}')]");
        if (await TapElement(actionLocator))
        {
            return IsElementPresent(AvailableActionIndicator) || !IsElementPresent(CompletedActionIndicator);
        }
        
        return false;
    }

    /// <summary>
    /// Verify that the total points display shows the expected value
    /// </summary>
    public async Task<bool> VerifyTotalPoints(int expectedPoints)
    {
        var actualPoints = await GetTotalPointsEarned();
        return actualPoints == expectedPoints;
    }

    #endregion
}
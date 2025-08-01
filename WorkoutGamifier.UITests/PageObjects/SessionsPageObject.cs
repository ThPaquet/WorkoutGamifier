using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using WorkoutGamifier.UITests.Configuration;

namespace WorkoutGamifier.UITests.PageObjects;

public class SessionsPageObject : PageObjectBase
{
    public SessionsPageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config)
    {
    }

    #region Page Identification

    protected override By PageIdentifier => By.XPath("//android.widget.TextView[@text='Sessions']");
    protected override string ExpectedPageTitle => "Sessions";

    #endregion

    #region Element Locators

    // Main elements
    public By SessionsTab => By.XPath("//android.widget.TextView[@text='Sessions']");
    public By StartSessionButton => By.XPath("//android.widget.Button[contains(@text, 'Start') or contains(@text, 'New Session')]");
    public By SessionsList => By.XPath("//*[contains(@resource-id, 'sessions') or contains(@class, 'list')]");
    
    // Session list items
    public By SessionListItems => By.XPath("//android.widget.TextView[contains(@text, 'Session') or contains(@text, 'Workout')]");
    public By ActiveSessionIndicator => By.XPath("//*[contains(@text, 'Active') or contains(@text, 'Running')]");
    public By CompletedSessionIndicator => By.XPath("//*[contains(@text, 'Completed') or contains(@text, 'Finished')]");
    
    // Session actions
    public By ResumeSessionButton => By.XPath("//android.widget.Button[contains(@text, 'Resume') or contains(@text, 'Continue')]");
    public By EndSessionButton => By.XPath("//android.widget.Button[contains(@text, 'End') or contains(@text, 'Stop')]");
    public By ViewSessionButton => By.XPath("//android.widget.Button[contains(@text, 'View') or contains(@text, 'Details')]");
    
    // Empty state
    public By NoSessionsMessage => By.XPath("//*[contains(@text, 'No sessions') or contains(@text, 'Start your first')]");
    public By EmptyStateImage => By.XPath("//android.widget.Image");

    #endregion

    #region Page Actions

    /// <summary>
    /// Start a new workout session
    /// </summary>
    public async Task<bool> StartNewSession()
    {
        await WaitForPageToLoad();
        return await TapElement(StartSessionButton);
    }

    /// <summary>
    /// Get list of all session names displayed
    /// </summary>
    public async Task<List<string>> GetSessionNames()
    {
        await WaitForPageToLoad();
        await Task.Delay(1000); // Allow sessions to load
        return GetElementTexts(SessionListItems);
    }

    /// <summary>
    /// Check if there are any active sessions
    /// </summary>
    public async Task<bool> HasActiveSessions()
    {
        await WaitForPageToLoad();
        return IsElementPresent(ActiveSessionIndicator);
    }

    /// <summary>
    /// Get the count of sessions displayed
    /// </summary>
    public async Task<int> GetSessionCount()
    {
        await WaitForPageToLoad();
        return await GetElementCount(SessionListItems);
    }

    /// <summary>
    /// Resume an active session
    /// </summary>
    public async Task<bool> ResumeActiveSession()
    {
        await WaitForPageToLoad();
        if (await HasActiveSessions())
        {
            return await TapElement(ResumeSessionButton);
        }
        return false;
    }

    /// <summary>
    /// End the current active session
    /// </summary>
    public async Task<bool> EndActiveSession()
    {
        await WaitForPageToLoad();
        if (await HasActiveSessions())
        {
            return await TapElement(EndSessionButton);
        }
        return false;
    }

    /// <summary>
    /// View details of a specific session
    /// </summary>
    public async Task<bool> ViewSessionDetails(string sessionName)
    {
        await WaitForPageToLoad();
        
        // First try to find and tap the session by name
        var sessionLocator = By.XPath($"//android.widget.TextView[contains(@text, '{sessionName}')]");
        if (await TapElement(sessionLocator))
        {
            return true;
        }
        
        // If that doesn't work, try the view button
        return await TapElement(ViewSessionButton);
    }

    /// <summary>
    /// Check if the page is showing empty state (no sessions)
    /// </summary>
    public async Task<bool> IsEmptyState()
    {
        await WaitForPageToLoad();
        return IsElementPresent(NoSessionsMessage) || (await GetSessionCount() == 0);
    }

    /// <summary>
    /// Verify that the sessions page is properly loaded with expected elements
    /// </summary>
    public async Task<bool> VerifyPageLoaded()
    {
        if (!await WaitForPageToLoad())
        {
            return false;
        }

        // Check for either sessions list or empty state
        var hasSessionsList = IsElementPresent(SessionsList);
        var hasEmptyState = IsElementPresent(NoSessionsMessage);
        var hasStartButton = IsElementPresent(StartSessionButton);

        return hasStartButton && (hasSessionsList || hasEmptyState);
    }

    /// <summary>
    /// Wait for sessions to load and display
    /// </summary>
    public async Task<bool> WaitForSessionsToLoad(int timeoutSeconds = 15)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime)
        {
            // Check if sessions have loaded or empty state is shown
            if (await GetSessionCount() > 0 || IsElementPresent(NoSessionsMessage))
            {
                return true;
            }
            
            await Task.Delay(1000);
        }
        
        return false;
    }

    /// <summary>
    /// Refresh the sessions list by pulling down or using refresh button
    /// </summary>
    public async Task<bool> RefreshSessions()
    {
        await WaitForPageToLoad();
        
        // Try to find a refresh button first
        var refreshButton = By.XPath("//android.widget.Button[contains(@text, 'Refresh')]");
        if (IsElementPresent(refreshButton))
        {
            return await TapElement(refreshButton);
        }
        
        // If no refresh button, try pull-to-refresh gesture
        try
        {
            Driver.ExecuteScript("mobile: scrollGesture", new Dictionary<string, object>
            {
                ["left"] = 200,
                ["top"] = 300,
                ["width"] = 200,
                ["height"] = 200,
                ["direction"] = "down",
                ["percent"] = 1.0
            });
            
            await Task.Delay(2000); // Wait for refresh to complete
            return true;
        }
        catch
        {
            return false;
        }
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Verify that a session with the given name exists
    /// </summary>
    public async Task<bool> VerifySessionExists(string sessionName)
    {
        var sessionNames = await GetSessionNames();
        return sessionNames.Any(name => name.Contains(sessionName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verify that the active session indicator is shown
    /// </summary>
    public async Task<bool> VerifyActiveSessionShown()
    {
        await WaitForPageToLoad();
        return await WaitForElement(ActiveSessionIndicator, 5);
    }

    /// <summary>
    /// Verify that no active sessions are shown
    /// </summary>
    public async Task<bool> VerifyNoActiveSession()
    {
        await WaitForPageToLoad();
        await Task.Delay(1000); // Give time for UI to update
        return !IsElementPresent(ActiveSessionIndicator);
    }

    #endregion
}
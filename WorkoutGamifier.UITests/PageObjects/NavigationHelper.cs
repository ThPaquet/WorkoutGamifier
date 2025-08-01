using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using WorkoutGamifier.UITests.Configuration;

namespace WorkoutGamifier.UITests.PageObjects;

public class NavigationHelper
{
    private readonly AndroidDriver _driver;
    private readonly TestConfiguration _config;

    public NavigationHelper(AndroidDriver driver, TestConfiguration config)
    {
        _driver = driver;
        _config = config;
    }

    #region Tab Navigation

    /// <summary>
    /// Navigate to the Sessions tab
    /// </summary>
    public async Task<SessionsPageObject> NavigateToSessions()
    {
        await NavigateToTab("Sessions");
        var sessionsPage = new SessionsPageObject(_driver, _config);
        await sessionsPage.WaitForPageToLoad();
        return sessionsPage;
    }

    /// <summary>
    /// Navigate to the Workouts tab
    /// </summary>
    public async Task<WorkoutsPageObject> NavigateToWorkouts()
    {
        await NavigateToTab("Workouts");
        var workoutsPage = new WorkoutsPageObject(_driver, _config);
        await workoutsPage.WaitForPageToLoad();
        return workoutsPage;
    }

    /// <summary>
    /// Navigate to the Pools tab
    /// </summary>
    public async Task<WorkoutPoolsPageObject> NavigateToPools()
    {
        await NavigateToTab("Pools");
        var poolsPage = new WorkoutPoolsPageObject(_driver, _config);
        await poolsPage.WaitForPageToLoad();
        return poolsPage;
    }

    /// <summary>
    /// Navigate to the Actions tab
    /// </summary>
    public async Task<ActionsPageObject> NavigateToActions()
    {
        await NavigateToTab("Actions");
        var actionsPage = new ActionsPageObject(_driver, _config);
        await actionsPage.WaitForPageToLoad();
        return actionsPage;
    }

    /// <summary>
    /// Navigate to the Profile tab
    /// </summary>
    public async Task<ProfilePageObject> NavigateToProfile()
    {
        await NavigateToTab("Profile");
        var profilePage = new ProfilePageObject(_driver, _config);
        await profilePage.WaitForPageToLoad();
        return profilePage;
    }

    #endregion

    #region Deep Navigation

    /// <summary>
    /// Navigate to session creation page
    /// </summary>
    public async Task<SessionCreatePageObject> NavigateToSessionCreation()
    {
        var sessionsPage = await NavigateToSessions();
        await sessionsPage.StartNewSession();
        
        var sessionCreatePage = new SessionCreatePageObject(_driver, _config);
        await sessionCreatePage.WaitForPageToLoad();
        return sessionCreatePage;
    }

    /// <summary>
    /// Navigate to pool creation page
    /// </summary>
    public async Task<PoolFormPageObject> NavigateToPoolCreation()
    {
        var poolsPage = await NavigateToPools();
        await poolsPage.CreateNewPool();
        
        var poolFormPage = new PoolFormPageObject(_driver, _config);
        await poolFormPage.WaitForPageToLoad();
        return poolFormPage;
    }

    /// <summary>
    /// Navigate to action creation page
    /// </summary>
    public async Task<ActionFormPageObject> NavigateToActionCreation()
    {
        var actionsPage = await NavigateToActions();
        await actionsPage.CreateNewAction();
        
        var actionFormPage = new ActionFormPageObject(_driver, _config);
        await actionFormPage.WaitForPageToLoad();
        return actionFormPage;
    }

    /// <summary>
    /// Navigate to a specific pool's detail page
    /// </summary>
    public async Task<WorkoutPoolDetailPageObject> NavigateToPoolDetails(string poolName)
    {
        var poolsPage = await NavigateToPools();
        await poolsPage.ViewPoolDetails(poolName);
        
        var poolDetailPage = new WorkoutPoolDetailPageObject(_driver, _config);
        await poolDetailPage.WaitForPageToLoad();
        return poolDetailPage;
    }

    #endregion

    #region Navigation Utilities

    /// <summary>
    /// Navigate to a specific tab by name
    /// </summary>
    private async Task<bool> NavigateToTab(string tabName)
    {
        var tabLocator = By.XPath($"//android.widget.TextView[@text='{tabName}']");
        
        try
        {
            var element = _driver.FindElement(tabLocator);
            element.Click();
            await Task.Delay(1000); // Wait for navigation to complete
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    /// <summary>
    /// Go back to the previous page
    /// </summary>
    public async Task<bool> GoBack()
    {
        try
        {
            _driver.Navigate().Back();
            await Task.Delay(500);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Get the currently active tab name
    /// </summary>
    public async Task<string> GetCurrentTab()
    {
        var tabNames = new[] { "Sessions", "Workouts", "Pools", "Actions", "Profile" };
        
        foreach (var tabName in tabNames)
        {
            var tabLocator = By.XPath($"//android.widget.TextView[@text='{tabName}']");
            try
            {
                var element = _driver.FindElement(tabLocator);
                // Check if this tab is selected/active (this might need adjustment based on actual UI)
                if (element.Selected || element.GetAttribute("selected") == "true")
                {
                    return tabName;
                }
            }
            catch (NoSuchElementException)
            {
                continue;
            }
        }
        
        return "Unknown";
    }

    /// <summary>
    /// Verify that we can navigate between all main tabs
    /// </summary>
    public async Task<bool> VerifyAllTabsAccessible()
    {
        var tabs = new[] { "Sessions", "Workouts", "Pools", "Actions", "Profile" };
        
        foreach (var tab in tabs)
        {
            if (!await NavigateToTab(tab))
            {
                return false;
            }
            
            // Verify we're on the correct tab
            await Task.Delay(1000);
            var currentTab = await GetCurrentTab();
            if (currentTab != tab)
            {
                return false;
            }
        }
        
        return true;
    }

    /// <summary>
    /// Wait for the main navigation to be available
    /// </summary>
    public async Task<bool> WaitForMainNavigation(int timeoutSeconds = 30)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime)
        {
            // Check if at least one main tab is visible
            var tabLocators = new[]
            {
                By.XPath("//android.widget.TextView[@text='Sessions']"),
                By.XPath("//android.widget.TextView[@text='Workouts']"),
                By.XPath("//android.widget.TextView[@text='Pools']"),
                By.XPath("//android.widget.TextView[@text='Actions']"),
                By.XPath("//android.widget.TextView[@text='Profile']")
            };
            
            foreach (var locator in tabLocators)
            {
                try
                {
                    _driver.FindElement(locator);
                    return true; // Found at least one tab
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }
            
            await Task.Delay(1000);
        }
        
        return false;
    }

    /// <summary>
    /// Handle app initialization and navigate to main content
    /// </summary>
    public async Task<bool> HandleAppInitialization()
    {
        // Wait for app to load
        await Task.Delay(2000);
        
        // Check if we're on initialization page
        var initializingLocator = By.XPath("//android.widget.TextView[@text='Initializing']");
        if (IsElementPresent(initializingLocator))
        {
            // Wait for initialization to complete
            await WaitForElementToDisappear(initializingLocator, 60);
        }
        
        // Check if we're on welcome page (first run)
        var welcomeLocator = By.XPath("//android.widget.TextView[@text='Welcome']");
        if (IsElementPresent(welcomeLocator))
        {
            // Handle welcome/onboarding flow
            var continueButton = By.XPath("//android.widget.Button[contains(@text, 'Continue') or contains(@text, 'Get Started') or contains(@text, 'Next')]");
            if (IsElementPresent(continueButton))
            {
                _driver.FindElement(continueButton).Click();
                await Task.Delay(2000);
            }
        }
        
        // Wait for main navigation to be available
        return await WaitForMainNavigation();
    }

    #endregion

    #region Helper Methods

    private bool IsElementPresent(By locator)
    {
        try
        {
            _driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private async Task<bool> WaitForElementToDisappear(By locator, int timeoutSeconds)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime && IsElementPresent(locator))
        {
            await Task.Delay(500);
        }
        
        return !IsElementPresent(locator);
    }

    #endregion
}

#region Placeholder Page Objects

// These are placeholder classes for page objects that will be implemented in future tasks
// They extend PageObjectBase to maintain consistency

public class WorkoutsPageObject : PageObjectBase
{
    public WorkoutsPageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config) { }
    protected override By PageIdentifier => By.XPath("//android.widget.TextView[@text='Workouts']");
    protected override string ExpectedPageTitle => "Workouts";
}

public class SessionCreatePageObject : PageObjectBase
{
    public SessionCreatePageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config) { }
    protected override By PageIdentifier => By.XPath("//android.widget.TextView[contains(@text, 'Create') and contains(@text, 'Session')]");
    protected override string ExpectedPageTitle => "Create Session";
}

public class PoolFormPageObject : PageObjectBase
{
    public PoolFormPageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config) { }
    protected override By PageIdentifier => By.XPath("//android.widget.TextView[contains(@text, 'Create') and contains(@text, 'Pool')]");
    protected override string ExpectedPageTitle => "Create Pool";
}

public class ActionFormPageObject : PageObjectBase
{
    public ActionFormPageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config) { }
    protected override By PageIdentifier => By.XPath("//android.widget.TextView[contains(@text, 'Create') and contains(@text, 'Action')]");
    protected override string ExpectedPageTitle => "Create Action";
}

public class WorkoutPoolDetailPageObject : PageObjectBase
{
    public WorkoutPoolDetailPageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config) { }
    protected override By PageIdentifier => By.XPath("//android.widget.TextView[contains(@text, 'Pool') and contains(@text, 'Detail')]");
    protected override string ExpectedPageTitle => "Pool Details";
}

#endregion
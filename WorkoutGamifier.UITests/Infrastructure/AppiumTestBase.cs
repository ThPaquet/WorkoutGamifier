using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using WorkoutGamifier.UITests.Configuration;
using WorkoutGamifier.UITests.Utilities;

namespace WorkoutGamifier.UITests.Infrastructure;

public abstract class AppiumTestBase : IAsyncLifetime, IDisposable
{
    protected AndroidDriver Driver { get; private set; } = null!;
    protected TestConfiguration Config { get; private set; } = null!;
    protected ScreenshotManager Screenshots { get; private set; } = null!;
    protected TestDataHelper TestData { get; private set; } = null!;
    
    private bool _disposed = false;

    public virtual async Task InitializeAsync()
    {
        Config = TestConfiguration.Load();
        Config.Validate();
        
        Screenshots = new ScreenshotManager(Config);
        TestData = new TestDataHelper();
        
        await StartDriver();
        await WaitForAppToLoad();
    }

    public virtual async Task DisposeAsync()
    {
        await CleanupAsync();
        Dispose();
    }

    public virtual void Dispose()
    {
        if (!_disposed)
        {
            Driver?.Quit();
            Driver?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }

    protected virtual async Task StartDriver()
    {
        var options = new AppiumOptions();
        
        // Platform capabilities
        options.PlatformName = Config.PlatformName;
        options.PlatformVersion = Config.PlatformVersion;
        options.DeviceName = Config.DeviceName;
        
        // App capabilities
        if (!string.IsNullOrEmpty(Config.AppPath))
        {
            options.AddAdditionalAppiumOption("app", Config.AppPath);
        }
        else
        {
            options.AddAdditionalAppiumOption("appPackage", Config.AppPackage);
            options.AddAdditionalAppiumOption("appActivity", Config.AppActivity);
        }
        
        // Behavior capabilities
        options.AddAdditionalAppiumOption("autoGrantPermissions", Config.AutoGrantPermissions);
        options.AddAdditionalAppiumOption("noReset", Config.NoReset);
        options.AddAdditionalAppiumOption("fullReset", Config.FullReset);
        options.AddAdditionalAppiumOption("newCommandTimeout", Config.CommandTimeoutMinutes * 60);
        
        // Performance capabilities
        options.AutomationName = "UiAutomator2";
        options.AddAdditionalAppiumOption("skipServerInstallation", true);
        options.AddAdditionalAppiumOption("skipDeviceInitialization", true);
        
        Driver = new AndroidDriver(new Uri(Config.AppiumServerUrl), options);
        Driver.Manage().Timeouts().ImplicitWait = Config.ImplicitWait;
        
        await Task.Delay(2000); // Give the driver time to initialize
    }

    protected virtual async Task WaitForAppToLoad()
    {
        // Wait for the app to fully load by checking for a common element
        // This could be the initialization page or main tab bar
        var loaded = await WaitForAnyElement(30,
            By.XPath("//android.widget.TextView[@text='Initializing']"),
            By.XPath("//android.widget.TextView[@text='Sessions']"),
            By.XPath("//android.widget.TextView[@text='Welcome']")
        );
        
        if (!loaded)
        {
            await TakeScreenshot("app-load-timeout");
            throw new TimeoutException("App failed to load within the expected time");
        }
        
        // If we see the initialization page, wait for it to complete
        if (IsElementPresent(By.XPath("//android.widget.TextView[@text='Initializing']")))
        {
            await WaitForElementToDisappear(By.XPath("//android.widget.TextView[@text='Initializing']"), 60);
        }
    }

    protected virtual async Task CleanupAsync()
    {
        if (Driver != null)
        {
            try
            {
                // Clear app data for next test
                Driver.TerminateApp(Config.AppPackage);
                await Task.Delay(500);
                Driver.ActivateApp(Config.AppPackage);
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to reset app during cleanup: {ex.Message}");
            }
        }
    }

    #region Element Interaction Methods

    protected Task<bool> WaitForElement(By locator, int timeoutSeconds = 30)
    {
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(driver => driver.FindElement(locator));
            return Task.FromResult(true);
        }
        catch (WebDriverTimeoutException)
        {
            return Task.FromResult(false);
        }
    }

    protected async Task<bool> WaitForAnyElement(params By[] locators)
    {
        return await WaitForAnyElement(30, locators);
    }

    protected async Task<bool> WaitForAnyElement(int timeoutSeconds, params By[] locators)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime)
        {
            foreach (var locator in locators)
            {
                if (IsElementPresent(locator))
                {
                    return true;
                }
            }
            await Task.Delay(500);
        }
        
        return false;
    }

    protected async Task<bool> WaitForElementToDisappear(By locator, int timeoutSeconds = 30)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime && IsElementPresent(locator))
        {
            await Task.Delay(500);
        }
        
        return !IsElementPresent(locator);
    }

    protected bool IsElementPresent(By locator)
    {
        try
        {
            Driver.FindElement(locator);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    protected IWebElement? FindElementSafely(By locator)
    {
        try
        {
            return Driver.FindElement(locator);
        }
        catch (NoSuchElementException)
        {
            return null;
        }
    }

    protected async Task<bool> TapElement(By locator, int timeoutSeconds = 10)
    {
        if (await WaitForElement(locator, timeoutSeconds))
        {
            var element = Driver.FindElement(locator);
            element.Click();
            await Task.Delay(500); // Small delay after tap
            return true;
        }
        return false;
    }

    protected async Task<bool> EnterText(By locator, string text, bool clearFirst = true, int timeoutSeconds = 10)
    {
        if (await WaitForElement(locator, timeoutSeconds))
        {
            var element = Driver.FindElement(locator);
            if (clearFirst)
            {
                element.Clear();
            }
            element.SendKeys(text);
            await Task.Delay(300); // Small delay after text entry
            return true;
        }
        return false;
    }

    protected string? GetElementText(By locator)
    {
        var element = FindElementSafely(locator);
        return element?.Text;
    }

    protected async Task ScrollToElement(By locator, int maxScrolls = 10)
    {
        for (int i = 0; i < maxScrolls; i++)
        {
            if (IsElementPresent(locator))
            {
                return;
            }
            
            // Perform scroll down
            Driver.ExecuteScript("mobile: scrollGesture", new Dictionary<string, object>
            {
                ["left"] = 100,
                ["top"] = 100,
                ["width"] = 200,
                ["height"] = 200,
                ["direction"] = "down",
                ["percent"] = 3.0
            });
            
            await Task.Delay(500);
        }
    }

    #endregion

    #region Screenshot and Reporting Methods

    protected async Task TakeScreenshot(string stepName)
    {
        await Screenshots.TakeScreenshot(Driver, GetType().Name, stepName);
    }

    protected async Task TakeScreenshotOnFailure(string testName, Exception? exception = null)
    {
        if (Config.TakeScreenshotsOnFailure)
        {
            await Screenshots.TakeScreenshot(Driver, testName, "failure", exception?.Message);
        }
    }

    protected async Task TakeScreenshotOnSuccess(string testName)
    {
        if (Config.TakeScreenshotsOnSuccess)
        {
            await Screenshots.TakeScreenshot(Driver, testName, "success");
        }
    }

    #endregion

    #region Test Data Methods

    protected async Task SetupTestData()
    {
        await TestData.SetupMinimalTestData();
    }

    protected async Task CleanupTestData()
    {
        await TestData.CleanupTestData();
    }

    #endregion

    #region Navigation Helpers

    protected async Task<bool> NavigateToTab(string tabName)
    {
        var tabLocator = By.XPath($"//android.widget.TextView[@text='{tabName}']");
        return await TapElement(tabLocator);
    }

    protected async Task<bool> GoBack()
    {
        try
        {
            Driver.Navigate().Back();
            await Task.Delay(500);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    protected Task<string> GetCurrentPageTitle()
    {
        // Try to find common title elements
        var titleLocators = new[]
        {
            By.XPath("//android.widget.TextView[@resource-id='title']"),
            By.XPath("//android.widget.TextView[contains(@class, 'title')]"),
            By.XPath("//android.widget.TextView[1]") // First text view as fallback
        };

        foreach (var locator in titleLocators)
        {
            var text = GetElementText(locator);
            if (!string.IsNullOrEmpty(text))
            {
                return Task.FromResult(text);
            }
        }

        return Task.FromResult("Unknown Page");
    }

    #endregion
}
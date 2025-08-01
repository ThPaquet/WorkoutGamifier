using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Support.UI;
using WorkoutGamifier.UITests.Configuration;

namespace WorkoutGamifier.UITests.PageObjects;

public abstract class PageObjectBase
{
    protected AndroidDriver Driver { get; }
    protected TestConfiguration Config { get; }
    protected WebDriverWait Wait { get; }

    protected PageObjectBase(AndroidDriver driver, TestConfiguration config)
    {
        Driver = driver;
        Config = config;
        Wait = new WebDriverWait(driver, config.DefaultTimeout);
    }

    #region Abstract Properties and Methods

    /// <summary>
    /// The main identifier for this page - used to verify we're on the correct page
    /// </summary>
    protected abstract By PageIdentifier { get; }

    /// <summary>
    /// The expected page title or header text
    /// </summary>
    protected abstract string ExpectedPageTitle { get; }

    /// <summary>
    /// Verify that we are currently on this page
    /// </summary>
    public virtual async Task<bool> IsOnPage()
    {
        try
        {
            await Task.Delay(500); // Small delay to allow page to load
            return IsElementPresent(PageIdentifier);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Wait for this page to load completely
    /// </summary>
    public virtual async Task<bool> WaitForPageToLoad(int timeoutSeconds = 30)
    {
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(driver => driver.FindElement(PageIdentifier));
            await Task.Delay(1000); // Additional delay for content to load
            return true;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
    }

    #endregion

    #region Element Interaction Methods

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

    protected async Task<bool> WaitForElement(By locator, int timeoutSeconds = 10)
    {
        try
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            wait.Until(driver => driver.FindElement(locator));
            return true;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
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

    protected async Task<bool> WaitForElementToDisappear(By locator, int timeoutSeconds = 10)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime && IsElementPresent(locator))
        {
            await Task.Delay(500);
        }
        
        return !IsElementPresent(locator);
    }

    protected async Task ScrollToElement(By locator, int maxScrolls = 5)
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

    #region List and Collection Methods

    protected List<IWebElement> GetElements(By locator)
    {
        try
        {
            return Driver.FindElements(locator).Cast<IWebElement>().ToList();
        }
        catch
        {
            return new List<IWebElement>();
        }
    }

    protected List<string> GetElementTexts(By locator)
    {
        var elements = GetElements(locator);
        return elements.Select(e => e.Text).Where(text => !string.IsNullOrEmpty(text)).ToList();
    }

    protected async Task<int> GetElementCount(By locator)
    {
        await Task.Delay(500); // Small delay to ensure elements are loaded
        return GetElements(locator).Count;
    }

    #endregion

    #region Navigation Methods

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

    #endregion

    #region Validation Methods

    /// <summary>
    /// Verify that expected elements are present on the page
    /// </summary>
    protected async Task<bool> VerifyPageElements(params By[] expectedElements)
    {
        foreach (var element in expectedElements)
        {
            if (!await WaitForElement(element, 5))
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Verify that the page title matches expected value
    /// </summary>
    public virtual async Task<bool> VerifyPageTitle()
    {
        await Task.Delay(500);
        var actualTitle = await GetPageTitle();
        return actualTitle.Contains(ExpectedPageTitle, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Get the current page title
    /// </summary>
    protected virtual async Task<string> GetPageTitle()
    {
        // Try different common title element patterns
        var titleLocators = new[]
        {
            By.XPath("//android.widget.TextView[@resource-id='title']"),
            By.XPath("//android.widget.TextView[contains(@class, 'title')]"),
            By.XPath($"//android.widget.TextView[@text='{ExpectedPageTitle}']"),
            By.XPath("//android.widget.TextView[1]") // First text view as fallback
        };

        foreach (var locator in titleLocators)
        {
            var text = GetElementText(locator);
            if (!string.IsNullOrEmpty(text))
            {
                return text;
            }
        }

        return "Unknown Page";
    }

    #endregion

    #region Form Interaction Methods

    protected async Task<bool> FillForm(Dictionary<By, string> formData)
    {
        foreach (var field in formData)
        {
            if (!await EnterText(field.Key, field.Value))
            {
                return false;
            }
        }
        return true;
    }

    protected async Task<bool> SelectFromDropdown(By dropdownLocator, string optionText)
    {
        if (await TapElement(dropdownLocator))
        {
            var optionLocator = By.XPath($"//android.widget.TextView[@text='{optionText}']");
            return await TapElement(optionLocator);
        }
        return false;
    }

    #endregion

    #region Wait Methods

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

    protected async Task<bool> WaitForTextToAppear(string text, int timeoutSeconds = 10)
    {
        var locator = By.XPath($"//*[contains(text(), '{text}')]");
        return await WaitForElement(locator, timeoutSeconds);
    }

    protected async Task<bool> WaitForTextToDisappear(string text, int timeoutSeconds = 10)
    {
        var locator = By.XPath($"//*[contains(text(), '{text}')]");
        return await WaitForElementToDisappear(locator, timeoutSeconds);
    }

    #endregion
}
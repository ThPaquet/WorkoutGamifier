using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using WorkoutGamifier.UITests.Configuration;

namespace WorkoutGamifier.UITests.PageObjects;

public class WorkoutPoolsPageObject : PageObjectBase
{
    public WorkoutPoolsPageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config)
    {
    }

    #region Page Identification

    protected override By PageIdentifier => By.XPath("//android.widget.TextView[@text='Pools']");
    protected override string ExpectedPageTitle => "Pools";

    #endregion

    #region Element Locators

    // Main elements
    public By PoolsTab => By.XPath("//android.widget.TextView[@text='Pools']");
    public By CreatePoolButton => By.XPath("//android.widget.Button[contains(@text, 'Create') or contains(@text, 'Add Pool') or contains(@text, 'New Pool')]");
    public By PoolsList => By.XPath("//*[contains(@resource-id, 'pools') or contains(@class, 'list')]");
    
    // Pool list items
    public By PoolListItems => By.XPath("//android.widget.TextView[contains(@text, 'Pool') or contains(@text, 'Workout')]");
    public By PoolNames => By.XPath("//*[contains(@class, 'pool-name') or contains(@resource-id, 'pool_name')]");
    public By PoolDescriptions => By.XPath("//*[contains(@class, 'pool-description') or contains(@resource-id, 'pool_description')]");
    
    // Pool actions
    public By ViewPoolButton => By.XPath("//android.widget.Button[contains(@text, 'View') or contains(@text, 'Open')]");
    public By EditPoolButton => By.XPath("//android.widget.Button[contains(@text, 'Edit') or contains(@text, 'Modify')]");
    public By DeletePoolButton => By.XPath("//android.widget.Button[contains(@text, 'Delete') or contains(@text, 'Remove')]");
    
    // Pool details
    public By WorkoutCountIndicator => By.XPath("//*[contains(@text, 'workout') and (contains(@text, 'count') or matches(@text, '\\d+'))]");
    public By PoolTypeIndicator => By.XPath("//*[contains(@text, 'Beginner') or contains(@text, 'Intermediate') or contains(@text, 'Advanced')]");
    
    // Empty state
    public By NoPoolsMessage => By.XPath("//*[contains(@text, 'No pools') or contains(@text, 'Create your first')]");
    public By EmptyStateImage => By.XPath("//android.widget.Image");

    #endregion

    #region Page Actions

    /// <summary>
    /// Create a new workout pool
    /// </summary>
    public async Task<bool> CreateNewPool()
    {
        await WaitForPageToLoad();
        return await TapElement(CreatePoolButton);
    }

    /// <summary>
    /// Get list of all pool names displayed
    /// </summary>
    public async Task<List<string>> GetPoolNames()
    {
        await WaitForPageToLoad();
        await Task.Delay(1000); // Allow pools to load
        
        // Try to get pool names from specific pool name elements first
        var poolNames = GetElementTexts(PoolNames);
        if (poolNames.Any())
        {
            return poolNames;
        }
        
        // Fallback to general pool list items
        return GetElementTexts(PoolListItems);
    }

    /// <summary>
    /// Get the count of pools displayed
    /// </summary>
    public async Task<int> GetPoolCount()
    {
        await WaitForPageToLoad();
        return await GetElementCount(PoolListItems);
    }

    /// <summary>
    /// View details of a specific pool
    /// </summary>
    public async Task<bool> ViewPoolDetails(string poolName)
    {
        await WaitForPageToLoad();
        
        // First try to find and tap the pool by name
        var poolLocator = By.XPath($"//android.widget.TextView[contains(@text, '{poolName}')]");
        if (await TapElement(poolLocator))
        {
            return true;
        }
        
        // If that doesn't work, try scrolling to find it
        await ScrollToElement(poolLocator);
        if (await TapElement(poolLocator))
        {
            return true;
        }
        
        // Last resort: try the view button
        return await TapElement(ViewPoolButton);
    }

    /// <summary>
    /// Edit a specific pool
    /// </summary>
    public async Task<bool> EditPool(string poolName)
    {
        await WaitForPageToLoad();
        
        // First navigate to the pool
        var poolLocator = By.XPath($"//android.widget.TextView[contains(@text, '{poolName}')]");
        if (await TapElement(poolLocator))
        {
            // Then look for edit button
            return await TapElement(EditPoolButton);
        }
        
        return false;
    }

    /// <summary>
    /// Delete a specific pool
    /// </summary>
    public async Task<bool> DeletePool(string poolName)
    {
        await WaitForPageToLoad();
        
        // First navigate to the pool
        var poolLocator = By.XPath($"//android.widget.TextView[contains(@text, '{poolName}')]");
        if (await TapElement(poolLocator))
        {
            // Then look for delete button
            if (await TapElement(DeletePoolButton))
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
    /// Check if the page is showing empty state (no pools)
    /// </summary>
    public async Task<bool> IsEmptyState()
    {
        await WaitForPageToLoad();
        return IsElementPresent(NoPoolsMessage) || (await GetPoolCount() == 0);
    }

    /// <summary>
    /// Get workout count for a specific pool
    /// </summary>
    public async Task<int> GetWorkoutCountForPool(string poolName)
    {
        await WaitForPageToLoad();
        
        // Navigate to the pool first
        var poolLocator = By.XPath($"//android.widget.TextView[contains(@text, '{poolName}')]");
        if (await TapElement(poolLocator))
        {
            // Look for workout count indicator
            var countText = GetElementText(WorkoutCountIndicator);
            if (!string.IsNullOrEmpty(countText))
            {
                // Extract number from text like "5 workouts" or "Workouts: 5"
                var match = System.Text.RegularExpressions.Regex.Match(countText, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int count))
                {
                    return count;
                }
            }
        }
        
        return 0;
    }

    /// <summary>
    /// Verify that the pools page is properly loaded with expected elements
    /// </summary>
    public async Task<bool> VerifyPageLoaded()
    {
        if (!await WaitForPageToLoad())
        {
            return false;
        }

        // Check for either pools list or empty state
        var hasPoolsList = IsElementPresent(PoolsList);
        var hasEmptyState = IsElementPresent(NoPoolsMessage);
        var hasCreateButton = IsElementPresent(CreatePoolButton);

        return hasCreateButton && (hasPoolsList || hasEmptyState);
    }

    /// <summary>
    /// Wait for pools to load and display
    /// </summary>
    public async Task<bool> WaitForPoolsToLoad(int timeoutSeconds = 15)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime)
        {
            // Check if pools have loaded or empty state is shown
            if (await GetPoolCount() > 0 || IsElementPresent(NoPoolsMessage))
            {
                return true;
            }
            
            await Task.Delay(1000);
        }
        
        return false;
    }

    /// <summary>
    /// Search for pools by name
    /// </summary>
    public async Task<bool> SearchPools(string searchTerm)
    {
        await WaitForPageToLoad();
        
        // Look for search box
        var searchBox = By.XPath("//android.widget.EditText[contains(@hint, 'Search') or contains(@resource-id, 'search')]");
        if (IsElementPresent(searchBox))
        {
            await EnterText(searchBox, searchTerm);
            await Task.Delay(1000); // Wait for search results
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Clear search and show all pools
    /// </summary>
    public async Task<bool> ClearSearch()
    {
        await WaitForPageToLoad();
        
        var searchBox = By.XPath("//android.widget.EditText[contains(@hint, 'Search') or contains(@resource-id, 'search')]");
        if (IsElementPresent(searchBox))
        {
            await EnterText(searchBox, "", true); // Clear the search box
            await Task.Delay(1000); // Wait for results to refresh
            return true;
        }
        
        return false;
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Verify that a pool with the given name exists
    /// </summary>
    public async Task<bool> VerifyPoolExists(string poolName)
    {
        var poolNames = await GetPoolNames();
        return poolNames.Any(name => name.Contains(poolName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verify that a pool with the given name does not exist
    /// </summary>
    public async Task<bool> VerifyPoolDoesNotExist(string poolName)
    {
        var poolNames = await GetPoolNames();
        return !poolNames.Any(name => name.Contains(poolName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verify that pools are displayed in the expected order
    /// </summary>
    public async Task<bool> VerifyPoolsOrder(List<string> expectedOrder)
    {
        var actualPoolNames = await GetPoolNames();
        
        if (actualPoolNames.Count != expectedOrder.Count)
        {
            return false;
        }
        
        for (int i = 0; i < expectedOrder.Count; i++)
        {
            if (!actualPoolNames[i].Contains(expectedOrder[i], StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }
        
        return true;
    }

    #endregion
}
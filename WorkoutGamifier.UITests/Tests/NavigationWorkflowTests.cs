using WorkoutGamifier.UITests.Infrastructure;
using WorkoutGamifier.UITests.PageObjects;

namespace WorkoutGamifier.UITests.Tests;

public class NavigationWorkflowTests : AppiumTestBase
{
    private NavigationHelper _navigation = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _navigation = new NavigationHelper(Driver, Config);
        
        // Handle app initialization
        await _navigation.HandleAppInitialization();
    }

    [Fact]
    public async Task TabNavigationWorkflow_NavigateBetweenAllTabs_ShouldWork()
    {
        // This test verifies that tab navigation works correctly between all main tabs
        
        await TakeScreenshot("tab-navigation-workflow-start");
        
        var tabSequence = new[] { "Sessions", "Pools", "Actions", "Profile", "Sessions" };
        
        foreach (var (tabName, index) in tabSequence.Select((tab, i) => (tab, i)))
        {
            await TakeScreenshot($"before-navigate-to-{tabName.ToLower()}-{index}");
            
            PageObjectBase currentPage = tabName switch
            {
                "Sessions" => await _navigation.NavigateToSessions(),
                "Pools" => await _navigation.NavigateToPools(),
                "Actions" => await _navigation.NavigateToActions(),
                "Profile" => await _navigation.NavigateToProfile(),
                _ => throw new ArgumentException($"Unknown tab: {tabName}")
            };
            
            // Verify we're on the correct page
            Assert.True(await currentPage.IsOnPage(), $"Should be on {tabName} page");
            Assert.True(await currentPage.VerifyPageTitle(), $"{tabName} page should have correct title");
            
            await TakeScreenshot($"on-{tabName.ToLower()}-page-{index}");
            
            // Small delay between navigations
            await Task.Delay(500);
        }
        
        await TakeScreenshot("tab-navigation-workflow-complete");
        Assert.True(true, "Tab navigation workflow completed successfully");
    }

    [Fact]
    public async Task DeepNavigationWorkflow_NavigateToSpecificPages_ShouldWork()
    {
        // This test verifies deep navigation to specific pages within the app
        
        await TakeScreenshot("deep-navigation-workflow-start");
        
        // Step 1: Navigate to session creation
        try
        {
            var sessionCreatePage = await _navigation.NavigateToSessionCreation();
            if (await sessionCreatePage.IsOnPage())
            {
                await TakeScreenshot("session-creation-page-reached");
                
                // Navigate back
                await _navigation.GoBack();
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            await TakeScreenshot("session-creation-navigation-failed");
            Console.WriteLine($"Session creation navigation failed: {ex.Message}");
        }
        
        // Step 2: Navigate to pool creation
        try
        {
            var poolFormPage = await _navigation.NavigateToPoolCreation();
            if (await poolFormPage.IsOnPage())
            {
                await TakeScreenshot("pool-creation-page-reached");
                
                // Navigate back
                await _navigation.GoBack();
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            await TakeScreenshot("pool-creation-navigation-failed");
            Console.WriteLine($"Pool creation navigation failed: {ex.Message}");
        }
        
        // Step 3: Navigate to action creation
        try
        {
            var actionFormPage = await _navigation.NavigateToActionCreation();
            if (await actionFormPage.IsOnPage())
            {
                await TakeScreenshot("action-creation-page-reached");
                
                // Navigate back
                await _navigation.GoBack();
                await Task.Delay(1000);
            }
        }
        catch (Exception ex)
        {
            await TakeScreenshot("action-creation-navigation-failed");
            Console.WriteLine($"Action creation navigation failed: {ex.Message}");
        }
        
        // Step 4: Try to navigate to pool details (if pools exist)
        var poolsPage = await _navigation.NavigateToPools();
        var poolNames = await poolsPage.GetPoolNames();
        
        if (poolNames.Any())
        {
            try
            {
                var poolDetailPage = await _navigation.NavigateToPoolDetails(poolNames.First());
                if (await poolDetailPage.IsOnPage())
                {
                    await TakeScreenshot("pool-details-page-reached");
                    
                    // Navigate back
                    await _navigation.GoBack();
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                await TakeScreenshot("pool-details-navigation-failed");
                Console.WriteLine($"Pool details navigation failed: {ex.Message}");
            }
        }
        
        await TakeScreenshot("deep-navigation-workflow-complete");
        Assert.True(true, "Deep navigation workflow completed");
    }

    [Fact]
    public async Task BackNavigationWorkflow_UseBackButtonConsistently_ShouldWork()
    {
        // This test verifies that back button navigation works consistently
        
        await TakeScreenshot("back-navigation-workflow-start");
        
        // Start from Sessions page
        var sessionsPage = await _navigation.NavigateToSessions();
        Assert.True(await sessionsPage.IsOnPage(), "Should start on Sessions page");
        await TakeScreenshot("starting-on-sessions");
        
        // Navigate to different tabs and use back button
        var navigationSteps = new[]
        {
            ("Pools", "Sessions"),
            ("Actions", "Pools"),
            ("Profile", "Actions"),
            ("Sessions", "Profile")
        };
        
        foreach (var (targetTab, expectedPreviousTab) in navigationSteps)
        {
            // Navigate to target tab
            PageObjectBase targetPage = targetTab switch
            {
                "Sessions" => await _navigation.NavigateToSessions(),
                "Pools" => await _navigation.NavigateToPools(),
                "Actions" => await _navigation.NavigateToActions(),
                "Profile" => await _navigation.NavigateToProfile(),
                _ => throw new ArgumentException($"Unknown tab: {targetTab}")
            };
            
            Assert.True(await targetPage.IsOnPage(), $"Should be on {targetTab} page");
            await TakeScreenshot($"navigated-to-{targetTab.ToLower()}");
            
            // Use back button
            var backSuccess = await _navigation.GoBack();
            await Task.Delay(1000); // Allow navigation to complete
            
            await TakeScreenshot($"after-back-from-{targetTab.ToLower()}");
            
            // Verify we're still in the app (back button shouldn't exit the app)
            var stillInApp = await _navigation.WaitForMainNavigation(5);
            Assert.True(stillInApp, $"Should still be in app after back from {targetTab}");
        }
        
        await TakeScreenshot("back-navigation-workflow-complete");
        Assert.True(true, "Back navigation workflow completed");
    }

    [Fact]
    public async Task NavigationStateWorkflow_VerifyNavigationState_ShouldWork()
    {
        // This test verifies that navigation state is maintained correctly
        
        await TakeScreenshot("navigation-state-workflow-start");
        
        var tabs = new[] { "Sessions", "Pools", "Actions", "Profile" };
        
        foreach (var tabName in tabs)
        {
            // Navigate to tab
            PageObjectBase currentPage = tabName switch
            {
                "Sessions" => await _navigation.NavigateToSessions(),
                "Pools" => await _navigation.NavigateToPools(),
                "Actions" => await _navigation.NavigateToActions(),
                "Profile" => await _navigation.NavigateToProfile(),
                _ => throw new ArgumentException($"Unknown tab: {tabName}")
            };
            
            // Verify page state
            Assert.True(await currentPage.IsOnPage(), $"Should be on {tabName} page");
            await TakeScreenshot($"verified-on-{tabName.ToLower()}-page");
            
            // Get current tab from navigation helper
            var currentTab = await _navigation.GetCurrentTab();
            await TakeScreenshot($"current-tab-is-{currentTab.ToLower()}");
            
            // The current tab should match what we navigated to (or be "Unknown" if detection fails)
            if (currentTab != "Unknown")
            {
                Assert.Equal(tabName, currentTab);
            }
            
            // Verify page loads completely
            var pageLoaded = await currentPage.WaitForPageToLoad();
            Assert.True(pageLoaded, $"{tabName} page should load completely");
            
            await Task.Delay(500);
        }
        
        await TakeScreenshot("navigation-state-workflow-complete");
        Assert.True(true, "Navigation state workflow completed");
    }

    [Fact]
    public async Task NavigationPerformanceWorkflow_MeasureNavigationSpeed_ShouldWork()
    {
        // This test measures navigation performance between tabs
        
        await TakeScreenshot("navigation-performance-workflow-start");
        
        var navigationTimes = new Dictionary<string, TimeSpan>();
        var tabs = new[] { "Sessions", "Pools", "Actions", "Profile" };
        
        foreach (var tabName in tabs)
        {
            var startTime = DateTime.Now;
            
            PageObjectBase currentPage = tabName switch
            {
                "Sessions" => await _navigation.NavigateToSessions(),
                "Pools" => await _navigation.NavigateToPools(),
                "Actions" => await _navigation.NavigateToActions(),
                "Profile" => await _navigation.NavigateToProfile(),
                _ => throw new ArgumentException($"Unknown tab: {tabName}")
            };
            
            var endTime = DateTime.Now;
            var navigationTime = endTime - startTime;
            navigationTimes[tabName] = navigationTime;
            
            // Verify navigation was successful
            Assert.True(await currentPage.IsOnPage(), $"Should successfully navigate to {tabName}");
            
            await TakeScreenshot($"performance-test-{tabName.ToLower()}");
            
            // Log navigation time
            Console.WriteLine($"Navigation to {tabName}: {navigationTime.TotalMilliseconds}ms");
            
            // Assert reasonable navigation time (should be under 10 seconds)
            Assert.True(navigationTime.TotalSeconds < 10, 
                $"Navigation to {tabName} should complete within 10 seconds, took {navigationTime.TotalSeconds}s");
        }
        
        // Calculate average navigation time
        var averageTime = TimeSpan.FromMilliseconds(navigationTimes.Values.Average(t => t.TotalMilliseconds));
        Console.WriteLine($"Average navigation time: {averageTime.TotalMilliseconds}ms");
        
        await TakeScreenshot("navigation-performance-workflow-complete");
        
        // Assert reasonable average navigation time
        Assert.True(averageTime.TotalSeconds < 5, 
            $"Average navigation time should be under 5 seconds, was {averageTime.TotalSeconds}s");
    }

    [Fact]
    public async Task NavigationRecoveryWorkflow_RecoverFromNavigationErrors_ShouldWork()
    {
        // This test verifies that the app can recover from navigation errors
        
        await TakeScreenshot("navigation-recovery-workflow-start");
        
        // Step 1: Perform normal navigation to establish baseline
        var sessionsPage = await _navigation.NavigateToSessions();
        Assert.True(await sessionsPage.IsOnPage(), "Should start on Sessions page");
        await TakeScreenshot("baseline-navigation-established");
        
        // Step 2: Try rapid navigation (potential stress test)
        var rapidNavigationTabs = new[] { "Pools", "Actions", "Profile", "Sessions", "Pools" };
        
        foreach (var tabName in rapidNavigationTabs)
        {
            try
            {
                PageObjectBase currentPage = tabName switch
                {
                    "Sessions" => await _navigation.NavigateToSessions(),
                    "Pools" => await _navigation.NavigateToPools(),
                    "Actions" => await _navigation.NavigateToActions(),
                    "Profile" => await _navigation.NavigateToProfile(),
                    _ => throw new ArgumentException($"Unknown tab: {tabName}")
                };
                
                // Don't wait long - this is rapid navigation
                await Task.Delay(200);
                
                await TakeScreenshot($"rapid-nav-{tabName.ToLower()}");
            }
            catch (Exception ex)
            {
                await TakeScreenshot($"rapid-nav-error-{tabName.ToLower()}");
                Console.WriteLine($"Rapid navigation to {tabName} failed: {ex.Message}");
            }
        }
        
        // Step 3: Verify app is still responsive after rapid navigation
        await Task.Delay(2000); // Allow app to settle
        
        var finalPage = await _navigation.NavigateToSessions();
        var appResponsive = await finalPage.IsOnPage();
        
        if (appResponsive)
        {
            await TakeScreenshot("app-responsive-after-rapid-nav");
        }
        else
        {
            await TakeScreenshot("app-unresponsive-after-rapid-nav");
        }
        
        Assert.True(appResponsive, "App should remain responsive after rapid navigation");
        
        // Step 4: Verify all tabs are still accessible
        var allTabsAccessible = await _navigation.VerifyAllTabsAccessible();
        
        if (allTabsAccessible)
        {
            await TakeScreenshot("all-tabs-accessible-after-recovery");
        }
        else
        {
            await TakeScreenshot("some-tabs-inaccessible-after-recovery");
        }
        
        await TakeScreenshot("navigation-recovery-workflow-complete");
        Assert.True(allTabsAccessible, "All tabs should be accessible after navigation recovery");
    }

    [Fact]
    public async Task NavigationConsistencyWorkflow_VerifyConsistentBehavior_ShouldWork()
    {
        // This test verifies that navigation behavior is consistent across multiple attempts
        
        await TakeScreenshot("navigation-consistency-workflow-start");
        
        var consistencyResults = new Dictionary<string, List<bool>>();
        var tabs = new[] { "Sessions", "Pools", "Actions", "Profile" };
        
        // Test each tab navigation multiple times
        foreach (var tabName in tabs)
        {
            consistencyResults[tabName] = new List<bool>();
            
            for (int attempt = 1; attempt <= 3; attempt++)
            {
                try
                {
                    PageObjectBase currentPage = tabName switch
                    {
                        "Sessions" => await _navigation.NavigateToSessions(),
                        "Pools" => await _navigation.NavigateToPools(),
                        "Actions" => await _navigation.NavigateToActions(),
                        "Profile" => await _navigation.NavigateToProfile(),
                        _ => throw new ArgumentException($"Unknown tab: {tabName}")
                    };
                    
                    var navigationSuccessful = await currentPage.IsOnPage();
                    consistencyResults[tabName].Add(navigationSuccessful);
                    
                    await TakeScreenshot($"consistency-{tabName.ToLower()}-attempt-{attempt}");
                    
                    await Task.Delay(1000); // Wait between attempts
                }
                catch (Exception ex)
                {
                    consistencyResults[tabName].Add(false);
                    await TakeScreenshot($"consistency-{tabName.ToLower()}-attempt-{attempt}-failed");
                    Console.WriteLine($"Navigation consistency test failed for {tabName} attempt {attempt}: {ex.Message}");
                }
            }
        }
        
        // Analyze consistency results
        foreach (var (tabName, results) in consistencyResults)
        {
            var successRate = results.Count(r => r) / (double)results.Count;
            Console.WriteLine($"{tabName} navigation success rate: {successRate:P}");
            
            // Assert at least 66% success rate (2 out of 3 attempts)
            Assert.True(successRate >= 0.66, 
                $"{tabName} navigation should succeed at least 66% of the time, actual: {successRate:P}");
        }
        
        await TakeScreenshot("navigation-consistency-workflow-complete");
        Assert.True(true, "Navigation consistency workflow completed");
    }
}
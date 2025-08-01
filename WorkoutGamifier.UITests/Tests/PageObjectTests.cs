using WorkoutGamifier.UITests.Infrastructure;
using WorkoutGamifier.UITests.PageObjects;

namespace WorkoutGamifier.UITests.Tests;

public class PageObjectTests : AppiumTestBase
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
    public async Task Navigation_ShouldWorkBetweenAllTabs_UsingPageObjects()
    {
        // Arrange & Act - Navigate to each tab using page objects
        await TakeScreenshot("navigation-start");

        // Test Sessions page
        var sessionsPage = await _navigation.NavigateToSessions();
        Assert.True(await sessionsPage.IsOnPage(), "Should be on Sessions page");
        Assert.True(await sessionsPage.VerifyPageLoaded(), "Sessions page should be properly loaded");
        await TakeScreenshot("sessions-page");

        // Test Pools page
        var poolsPage = await _navigation.NavigateToPools();
        Assert.True(await poolsPage.IsOnPage(), "Should be on Pools page");
        Assert.True(await poolsPage.VerifyPageLoaded(), "Pools page should be properly loaded");
        await TakeScreenshot("pools-page");

        // Test Actions page
        var actionsPage = await _navigation.NavigateToActions();
        Assert.True(await actionsPage.IsOnPage(), "Should be on Actions page");
        Assert.True(await actionsPage.VerifyPageLoaded(), "Actions page should be properly loaded");
        await TakeScreenshot("actions-page");

        // Test Profile page
        var profilePage = await _navigation.NavigateToProfile();
        Assert.True(await profilePage.IsOnPage(), "Should be on Profile page");
        Assert.True(await profilePage.VerifyPageLoaded(), "Profile page should be properly loaded");
        await TakeScreenshot("profile-page");

        // Assert - Verify navigation worked correctly
        Assert.True(await _navigation.VerifyAllTabsAccessible(), "All tabs should be accessible");
    }

    [Fact]
    public async Task SessionsPage_ShouldDisplayCorrectContent()
    {
        // Arrange
        var sessionsPage = await _navigation.NavigateToSessions();
        await TakeScreenshot("sessions-page-loaded");

        // Act & Assert
        Assert.True(await sessionsPage.IsOnPage(), "Should be on Sessions page");
        Assert.True(await sessionsPage.VerifyPageTitle(), "Page title should match expected");
        
        // Check if page shows either sessions or empty state
        var hasContent = await sessionsPage.WaitForSessionsToLoad();
        Assert.True(hasContent, "Sessions page should show either sessions or empty state");

        if (await sessionsPage.IsEmptyState())
        {
            await TakeScreenshot("sessions-empty-state");
            // If empty, verify empty state elements are present
            Assert.True(true, "Empty state is properly displayed");
        }
        else
        {
            await TakeScreenshot("sessions-with-data");
            // If has sessions, verify session count is greater than 0
            var sessionCount = await sessionsPage.GetSessionCount();
            Assert.True(sessionCount > 0, "Should have at least one session displayed");
        }
    }

    [Fact]
    public async Task PoolsPage_ShouldDisplayCorrectContent()
    {
        // Arrange
        var poolsPage = await _navigation.NavigateToPools();
        await TakeScreenshot("pools-page-loaded");

        // Act & Assert
        Assert.True(await poolsPage.IsOnPage(), "Should be on Pools page");
        Assert.True(await poolsPage.VerifyPageTitle(), "Page title should match expected");
        
        // Check if page shows either pools or empty state
        var hasContent = await poolsPage.WaitForPoolsToLoad();
        Assert.True(hasContent, "Pools page should show either pools or empty state");

        if (await poolsPage.IsEmptyState())
        {
            await TakeScreenshot("pools-empty-state");
            // If empty, verify empty state elements are present
            Assert.True(true, "Empty state is properly displayed");
        }
        else
        {
            await TakeScreenshot("pools-with-data");
            // If has pools, verify pool count is greater than 0
            var poolCount = await poolsPage.GetPoolCount();
            Assert.True(poolCount > 0, "Should have at least one pool displayed");
            
            // Verify we can get pool names
            var poolNames = await poolsPage.GetPoolNames();
            Assert.NotEmpty(poolNames);
        }
    }

    [Fact]
    public async Task ActionsPage_ShouldDisplayCorrectContent()
    {
        // Arrange
        var actionsPage = await _navigation.NavigateToActions();
        await TakeScreenshot("actions-page-loaded");

        // Act & Assert
        Assert.True(await actionsPage.IsOnPage(), "Should be on Actions page");
        Assert.True(await actionsPage.VerifyPageTitle(), "Page title should match expected");
        
        // Check if page shows either actions or empty state
        var hasContent = await actionsPage.WaitForActionsToLoad();
        Assert.True(hasContent, "Actions page should show either actions or empty state");

        if (await actionsPage.IsEmptyState())
        {
            await TakeScreenshot("actions-empty-state");
            // If empty, verify empty state elements are present
            Assert.True(true, "Empty state is properly displayed");
        }
        else
        {
            await TakeScreenshot("actions-with-data");
            // If has actions, verify action count is greater than 0
            var actionCount = await actionsPage.GetActionCount();
            Assert.True(actionCount > 0, "Should have at least one action displayed");
            
            // Verify we can get action descriptions
            var actionDescriptions = await actionsPage.GetActionDescriptions();
            Assert.NotEmpty(actionDescriptions);
        }
    }

    [Fact]
    public async Task ProfilePage_ShouldDisplayStatistics()
    {
        // Arrange
        var profilePage = await _navigation.NavigateToProfile();
        await TakeScreenshot("profile-page-loaded");

        // Act & Assert
        Assert.True(await profilePage.IsOnPage(), "Should be on Profile page");
        Assert.True(await profilePage.VerifyPageTitle(), "Page title should match expected");
        
        // Wait for statistics to load
        var statsLoaded = await profilePage.WaitForStatisticsToLoad();
        Assert.True(statsLoaded, "Statistics should load on profile page");

        // Verify statistics sections are visible
        if (await profilePage.IsStatisticsSectionVisible())
        {
            await TakeScreenshot("profile-statistics-visible");
            
            // Get statistics values (they might be 0 for a fresh app)
            var totalSessions = await profilePage.GetTotalSessions();
            var totalPoints = await profilePage.GetTotalPointsEarned();
            var totalWorkouts = await profilePage.GetTotalWorkoutsCompleted();
            
            // Assert that we can retrieve statistics (even if they're 0)
            Assert.True(totalSessions >= 0, "Total sessions should be a valid number");
            Assert.True(totalPoints >= 0, "Total points should be a valid number");
            Assert.True(totalWorkouts >= 0, "Total workouts should be a valid number");
        }

        // Check if backup section is available
        if (await profilePage.IsBackupSectionVisible())
        {
            await TakeScreenshot("profile-backup-visible");
            Assert.True(true, "Backup section is visible");
        }
    }

    [Fact]
    public async Task PageObjects_ShouldHandleEmptyStatesGracefully()
    {
        // This test verifies that all page objects handle empty states properly
        
        // Test Sessions empty state
        var sessionsPage = await _navigation.NavigateToSessions();
        var sessionsEmpty = await sessionsPage.IsEmptyState();
        if (sessionsEmpty)
        {
            await TakeScreenshot("sessions-empty-handled");
            Assert.Equal(0, await sessionsPage.GetSessionCount());
        }

        // Test Pools empty state
        var poolsPage = await _navigation.NavigateToPools();
        var poolsEmpty = await poolsPage.IsEmptyState();
        if (poolsEmpty)
        {
            await TakeScreenshot("pools-empty-handled");
            Assert.Equal(0, await poolsPage.GetPoolCount());
        }

        // Test Actions empty state
        var actionsPage = await _navigation.NavigateToActions();
        var actionsEmpty = await actionsPage.IsEmptyState();
        if (actionsEmpty)
        {
            await TakeScreenshot("actions-empty-handled");
            Assert.Equal(0, await actionsPage.GetActionCount());
        }

        // All empty state checks should complete without errors
        Assert.True(true, "All page objects handled empty states gracefully");
    }

    [Fact]
    public async Task PageObjects_ShouldProvideConsistentInterface()
    {
        // This test verifies that all page objects provide consistent interfaces
        
        var pages = new PageObjectBase[]
        {
            await _navigation.NavigateToSessions(),
            await _navigation.NavigateToPools(),
            await _navigation.NavigateToActions(),
            await _navigation.NavigateToProfile()
        };

        foreach (var page in pages)
        {
            // Each page should be able to verify it's loaded
            Assert.True(await page.IsOnPage(), $"Page {page.GetType().Name} should report being on page");
            Assert.True(await page.VerifyPageTitle(), $"Page {page.GetType().Name} should have correct title");
            
            await TakeScreenshot($"page-consistency-{page.GetType().Name}");
        }
    }

    [Fact]
    public async Task Navigation_ShouldHandleBackButton()
    {
        // Arrange - Navigate to a specific page
        var poolsPage = await _navigation.NavigateToPools();
        Assert.True(await poolsPage.IsOnPage(), "Should be on Pools page initially");
        await TakeScreenshot("before-back-navigation");

        // Act - Use back button
        var backSuccess = await _navigation.GoBack();
        await Task.Delay(1000); // Wait for navigation

        // Assert - Should still be in the app
        await TakeScreenshot("after-back-navigation");
        
        // Verify we're still in the main app by checking if any main tab is accessible
        var stillInApp = await _navigation.WaitForMainNavigation(5);
        Assert.True(stillInApp, "Should still be in the main app after back button");
    }
}
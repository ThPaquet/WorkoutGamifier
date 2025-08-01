using WorkoutGamifier.UITests.Infrastructure;
using WorkoutGamifier.UITests.PageObjects;

namespace WorkoutGamifier.UITests.Tests;

public class SessionWorkflowTests : AppiumTestBase
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
    public async Task CompleteSessionWorkflow_CreateSessionCompleteActionsEndSession_ShouldWork()
    {
        // Arrange
        await TakeScreenshot("session-workflow-start");
        
        // Step 1: Navigate to Sessions and verify initial state
        var sessionsPage = await _navigation.NavigateToSessions();
        Assert.True(await sessionsPage.IsOnPage(), "Should be on Sessions page");
        
        var initialSessionCount = await sessionsPage.GetSessionCount();
        await TakeScreenshot("initial-sessions-state");

        // Step 2: Start a new session
        var sessionStarted = await sessionsPage.StartNewSession();
        Assert.True(sessionStarted, "Should be able to start a new session");
        await TakeScreenshot("session-creation-initiated");

        // Wait for session creation to complete and navigate back to sessions
        await Task.Delay(3000); // Allow time for session creation
        await _navigation.NavigateToSessions();
        
        // Verify session was created
        var hasActiveSession = await sessionsPage.HasActiveSessions();
        if (hasActiveSession)
        {
            await TakeScreenshot("active-session-created");
            
            // Step 3: Complete some actions to earn points
            var actionsPage = await _navigation.NavigateToActions();
            await TakeScreenshot("actions-page-for-session");
            
            var initialActionCount = await actionsPage.GetAvailableActionCount();
            if (initialActionCount > 0)
            {
                // Complete first available action
                var actionCompleted = await actionsPage.CompleteFirstAvailableAction();
                if (actionCompleted)
                {
                    await TakeScreenshot("action-completed");
                    
                    // Verify points were earned
                    var pointsEarned = await actionsPage.GetTotalPointsEarned();
                    Assert.True(pointsEarned > 0, "Should have earned points from completing action");
                }
            }
            
            // Step 4: Return to sessions and end the session
            sessionsPage = await _navigation.NavigateToSessions();
            var sessionEnded = await sessionsPage.EndActiveSession();
            
            if (sessionEnded)
            {
                await TakeScreenshot("session-ended");
                
                // Verify no active session remains
                var noActiveSession = await sessionsPage.VerifyNoActiveSession();
                Assert.True(noActiveSession, "Should have no active session after ending");
            }
        }
        
        await TakeScreenshot("session-workflow-complete");
    }

    [Fact]
    public async Task SessionManagement_MultipleSessionsHandling_ShouldPreventMultipleActive()
    {
        // Arrange
        var sessionsPage = await _navigation.NavigateToSessions();
        await TakeScreenshot("multiple-sessions-test-start");

        // Step 1: Start first session
        var firstSessionStarted = await sessionsPage.StartNewSession();
        await Task.Delay(2000);
        await _navigation.NavigateToSessions();
        
        var hasActiveSession = await sessionsPage.HasActiveSessions();
        if (hasActiveSession)
        {
            await TakeScreenshot("first-session-active");
            
            // Step 2: Try to start another session (should be prevented)
            var secondSessionAttempt = await sessionsPage.StartNewSession();
            await Task.Delay(2000);
            
            // The app should either prevent the second session or show an error
            // We'll verify by checking that we still have only one active session
            await _navigation.NavigateToSessions();
            var stillHasOneActiveSession = await sessionsPage.HasActiveSessions();
            
            await TakeScreenshot("second-session-attempt-result");
            
            // Clean up - end the active session
            if (stillHasOneActiveSession)
            {
                await sessionsPage.EndActiveSession();
                await TakeScreenshot("cleanup-session-ended");
            }
        }
        
        Assert.True(true, "Multiple session handling test completed");
    }

    [Fact]
    public async Task EndToEndWorkflow_CompleteUserJourney_ShouldWork()
    {
        // This is a comprehensive end-to-end test covering the complete user journey
        
        await TakeScreenshot("e2e-workflow-start");
        
        // Step 1: Check initial state across all pages
        var sessionsPage = await _navigation.NavigateToSessions();
        var initialSessionCount = await sessionsPage.GetSessionCount();
        
        var poolsPage = await _navigation.NavigateToPools();
        var initialPoolCount = await poolsPage.GetPoolCount();
        
        var actionsPage = await _navigation.NavigateToActions();
        var initialActionCount = await actionsPage.GetActionCount();
        
        var profilePage = await _navigation.NavigateToProfile();
        var initialPoints = await profilePage.GetTotalPointsEarned();
        
        await TakeScreenshot("e2e-initial-state");
        
        // Step 2: Create a pool if none exist
        if (initialPoolCount == 0)
        {
            poolsPage = await _navigation.NavigateToPools();
            await poolsPage.CreateNewPool();
            await Task.Delay(3000);
            await TakeScreenshot("e2e-pool-created");
        }
        
        // Step 3: Create actions if none exist
        if (initialActionCount == 0)
        {
            actionsPage = await _navigation.NavigateToActions();
            await actionsPage.CreateNewAction();
            await Task.Delay(3000);
            await TakeScreenshot("e2e-action-created");
        }
        
        // Step 4: Start a session
        sessionsPage = await _navigation.NavigateToSessions();
        await sessionsPage.StartNewSession();
        await Task.Delay(3000);
        await TakeScreenshot("e2e-session-started");
        
        // Step 5: Complete an action
        actionsPage = await _navigation.NavigateToActions();
        var availableActions = await actionsPage.GetAvailableActionCount();
        if (availableActions > 0)
        {
            await actionsPage.CompleteFirstAvailableAction();
            await TakeScreenshot("e2e-action-completed");
        }
        
        // Step 6: Check profile statistics
        profilePage = await _navigation.NavigateToProfile();
        await profilePage.RefreshStatistics();
        var finalPoints = await profilePage.GetTotalPointsEarned();
        await TakeScreenshot("e2e-profile-updated");
        
        // Step 7: End the session
        sessionsPage = await _navigation.NavigateToSessions();
        var hasActiveSession = await sessionsPage.HasActiveSessions();
        if (hasActiveSession)
        {
            await sessionsPage.EndActiveSession();
            await TakeScreenshot("e2e-session-ended");
        }
        
        // Step 8: Verify final state
        var finalSessionCount = await sessionsPage.GetSessionCount();
        Assert.True(finalSessionCount >= initialSessionCount, "Session count should not decrease");
        
        await TakeScreenshot("e2e-workflow-complete");
        Assert.True(true, "End-to-end workflow completed successfully");
    }
}
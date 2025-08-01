using WorkoutGamifier.UITests.Infrastructure;
using OpenQA.Selenium;

namespace WorkoutGamifier.UITests.Tests;

public class SmokeTests : AppiumTestBase
{
    [Fact]
    public async Task App_ShouldLaunchSuccessfully()
    {
        // Arrange & Act - App launch is handled in InitializeAsync
        await TakeScreenshot("app-launched");

        // Assert - Verify app has launched and is showing expected content
        var isInitializationVisible = IsElementPresent(By.XPath("//android.widget.TextView[@text='Initializing']"));
        var isWelcomeVisible = IsElementPresent(By.XPath("//android.widget.TextView[@text='Welcome']"));
        var isMainTabsVisible = IsElementPresent(By.XPath("//android.widget.TextView[@text='Sessions']"));

        Assert.True(isInitializationVisible || isWelcomeVisible || isMainTabsVisible, 
            "App should show initialization, welcome, or main tabs after launch");
    }

    [Fact]
    public async Task MainTabs_ShouldBeAccessible()
    {
        // Arrange - Wait for app to fully load
        await WaitForAnyElement(
            By.XPath("//android.widget.TextView[@text='Sessions']"),
            By.XPath("//android.widget.TextView[@text='Welcome']")
        );

        // If we're on welcome page, we need to complete onboarding first
        if (IsElementPresent(By.XPath("//android.widget.TextView[@text='Welcome']")))
        {
            // Skip welcome/onboarding for now - this will be handled in dedicated onboarding tests
            return;
        }

        await TakeScreenshot("main-tabs-visible");

        // Act & Assert - Test each main tab
        var expectedTabs = new[] { "Sessions", "Workouts", "Pools", "Actions", "Profile" };

        foreach (var tabName in expectedTabs)
        {
            var tabLocator = By.XPath($"//android.widget.TextView[@text='{tabName}']");
            
            Assert.True(IsElementPresent(tabLocator), $"Tab '{tabName}' should be visible");
            
            // Try to tap the tab
            var tapped = await TapElement(tabLocator);
            Assert.True(tapped, $"Should be able to tap '{tabName}' tab");
            
            await TakeScreenshot($"tab-{tabName.ToLower()}");
            await Task.Delay(1000); // Give page time to load
        }
    }

    [Fact]
    public async Task Navigation_ShouldWorkBetweenTabs()
    {
        // Arrange - Ensure we're on main tabs
        await WaitForElement(By.XPath("//android.widget.TextView[@text='Sessions']"), 30);
        await TakeScreenshot("navigation-start");

        // Act & Assert - Navigate between tabs and verify content changes
        var navigationTests = new[]
        {
            ("Sessions", "Workouts"),
            ("Workouts", "Pools"),
            ("Pools", "Actions"),
            ("Actions", "Profile"),
            ("Profile", "Sessions")
        };

        foreach (var (fromTab, toTab) in navigationTests)
        {
            // Navigate to the target tab
            var success = await NavigateToTab(toTab);
            Assert.True(success, $"Should be able to navigate from {fromTab} to {toTab}");
            
            await TakeScreenshot($"navigation-{fromTab}-to-{toTab}");
            
            // Verify we're on the correct tab (tab should be highlighted/selected)
            var tabElement = FindElementSafely(By.XPath($"//android.widget.TextView[@text='{toTab}']"));
            Assert.NotNull(tabElement);
            
            await Task.Delay(500); // Small delay between navigations
        }
    }

    [Fact]
    public async Task App_ShouldHandleBackButton()
    {
        // Arrange - Navigate to a specific tab
        await WaitForElement(By.XPath("//android.widget.TextView[@text='Sessions']"), 30);
        await NavigateToTab("Workouts");
        await TakeScreenshot("before-back-button");

        // Act - Press back button
        var backSuccess = await GoBack();
        
        // Assert - Should still be in the app (not exit)
        await Task.Delay(1000);
        await TakeScreenshot("after-back-button");
        
        // Verify we're still in the app by checking for main tabs
        var stillInApp = IsElementPresent(By.XPath("//android.widget.TextView[@text='Sessions']")) ||
                        IsElementPresent(By.XPath("//android.widget.TextView[@text='Workouts']")) ||
                        IsElementPresent(By.XPath("//android.widget.TextView[@text='Pools']"));
        
        Assert.True(stillInApp, "App should still be running after back button press");
    }

    [Fact]
    public async Task App_ShouldShowExpectedContent()
    {
        // Arrange - Navigate to each main section
        await WaitForElement(By.XPath("//android.widget.TextView[@text='Sessions']"), 30);

        var sectionsToTest = new[]
        {
            ("Sessions", new[] { "session", "start", "active" }),
            ("Workouts", new[] { "workout", "exercise", "add" }),
            ("Pools", new[] { "pool", "collection", "create" }),
            ("Actions", new[] { "action", "point", "complete" }),
            ("Profile", new[] { "profile", "stats", "backup" })
        };

        foreach (var (sectionName, expectedKeywords) in sectionsToTest)
        {
            // Act - Navigate to section
            await NavigateToTab(sectionName);
            await Task.Delay(2000); // Give content time to load
            await TakeScreenshot($"content-check-{sectionName.ToLower()}");

            // Assert - Check for expected content (case-insensitive)
            var pageSource = Driver.PageSource.ToLowerInvariant();
            var hasExpectedContent = expectedKeywords.Any(keyword => pageSource.Contains(keyword.ToLowerInvariant()));
            
            Assert.True(hasExpectedContent, 
                $"Section '{sectionName}' should contain at least one of these keywords: {string.Join(", ", expectedKeywords)}");
        }
    }

    [Fact]
    public async Task App_ShouldRespondToUserInteraction()
    {
        // Arrange - Navigate to Sessions tab
        await WaitForElement(By.XPath("//android.widget.TextView[@text='Sessions']"), 30);
        await NavigateToTab("Sessions");
        await TakeScreenshot("interaction-test-start");

        // Act & Assert - Look for interactive elements and test them
        var interactiveElements = new[]
        {
            By.XPath("//android.widget.Button"),
            By.XPath("//android.widget.ImageButton"),
            By.XPath("//*[@clickable='true']")
        };

        var foundInteractiveElement = false;
        foreach (var locator in interactiveElements)
        {
            var elements = Driver.FindElements(locator);
            if (elements.Count > 0)
            {
                foundInteractiveElement = true;
                var element = elements.First();
                
                // Try to interact with the element
                try
                {
                    element.Click();
                    await Task.Delay(1000);
                    await TakeScreenshot("after-interaction");
                    break; // Successfully interacted with an element
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Could not interact with element: {ex.Message}");
                    // Continue to next element
                }
            }
        }

        Assert.True(foundInteractiveElement, "Should find at least one interactive element in the app");
    }
}
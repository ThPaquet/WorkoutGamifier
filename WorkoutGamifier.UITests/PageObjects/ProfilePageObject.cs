using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using WorkoutGamifier.UITests.Configuration;

namespace WorkoutGamifier.UITests.PageObjects;

public class ProfilePageObject : PageObjectBase
{
    public ProfilePageObject(AndroidDriver driver, TestConfiguration config) : base(driver, config)
    {
    }

    #region Page Identification

    protected override By PageIdentifier => By.XPath("//android.widget.TextView[@text='Profile']");
    protected override string ExpectedPageTitle => "Profile";

    #endregion

    #region Element Locators

    // Main elements
    public By ProfileTab => By.XPath("//android.widget.TextView[@text='Profile']");
    public By ProfileHeader => By.XPath("//*[contains(@class, 'profile-header') or contains(@resource-id, 'profile_header')]");
    
    // Statistics section
    public By StatisticsSection => By.XPath("//*[contains(@text, 'Statistics') or contains(@text, 'Stats')]");
    public By TotalSessionsCount => By.XPath("//*[contains(@text, 'Sessions') and contains(@text, 'Total')]");
    public By TotalPointsEarned => By.XPath("//*[contains(@text, 'Points') and contains(@text, 'Earned')]");
    public By TotalWorkoutsCompleted => By.XPath("//*[contains(@text, 'Workouts') and contains(@text, 'Completed')]");
    public By AverageSessionDuration => By.XPath("//*[contains(@text, 'Average') and contains(@text, 'Duration')]");
    
    // Statistics values
    public By SessionsCountValue => By.XPath("//*[contains(@resource-id, 'sessions_count') or contains(@class, 'stat-value')]");
    public By PointsEarnedValue => By.XPath("//*[contains(@resource-id, 'points_earned') or contains(@class, 'points-value')]");
    public By WorkoutsCompletedValue => By.XPath("//*[contains(@resource-id, 'workouts_completed') or contains(@class, 'workouts-value')]");
    
    // Backup and data section
    public By BackupSection => By.XPath("//*[contains(@text, 'Backup') or contains(@text, 'Data')]");
    public By ExportDataButton => By.XPath("//android.widget.Button[contains(@text, 'Export') or contains(@text, 'Backup')]");
    public By ImportDataButton => By.XPath("//android.widget.Button[contains(@text, 'Import') or contains(@text, 'Restore')]");
    public By ClearDataButton => By.XPath("//android.widget.Button[contains(@text, 'Clear') or contains(@text, 'Reset')]");
    
    // Settings section
    public By SettingsSection => By.XPath("//*[contains(@text, 'Settings') or contains(@text, 'Preferences')]");
    public By NotificationSettings => By.XPath("//*[contains(@text, 'Notification') or contains(@text, 'Alerts')]");
    public By ThemeSettings => By.XPath("//*[contains(@text, 'Theme') or contains(@text, 'Appearance')]");
    public By LanguageSettings => By.XPath("//*[contains(@text, 'Language') or contains(@text, 'Locale')]");
    
    // Achievement section
    public By AchievementsSection => By.XPath("//*[contains(@text, 'Achievement') or contains(@text, 'Badge')]");
    public By AchievementsList => By.XPath("//*[contains(@class, 'achievements') or contains(@resource-id, 'achievements')]");
    public By UnlockedAchievements => By.XPath("//*[contains(@text, 'Unlocked') or contains(@class, 'unlocked')]");
    
    // Progress indicators
    public By ProgressBars => By.XPath("//*[contains(@class, 'progress') or contains(@resource-id, 'progress')]");
    public By LevelIndicator => By.XPath("//*[contains(@text, 'Level') or contains(@text, 'Rank')]");
    
    // Action buttons
    public By RefreshStatsButton => By.XPath("//android.widget.Button[contains(@text, 'Refresh') or contains(@text, 'Update')]");
    public By ShareStatsButton => By.XPath("//android.widget.Button[contains(@text, 'Share') or contains(@text, 'Export Stats')]");

    #endregion

    #region Page Actions

    /// <summary>
    /// Get the total number of sessions from statistics
    /// </summary>
    public async Task<int> GetTotalSessions()
    {
        await WaitForPageToLoad();
        
        var sessionsText = GetElementText(TotalSessionsCount) ?? GetElementText(SessionsCountValue);
        if (!string.IsNullOrEmpty(sessionsText))
        {
            var match = System.Text.RegularExpressions.Regex.Match(sessionsText, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int count))
            {
                return count;
            }
        }
        
        return 0;
    }

    /// <summary>
    /// Get the total points earned from statistics
    /// </summary>
    public async Task<int> GetTotalPointsEarned()
    {
        await WaitForPageToLoad();
        
        var pointsText = GetElementText(TotalPointsEarned) ?? GetElementText(PointsEarnedValue);
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
    /// Get the total workouts completed from statistics
    /// </summary>
    public async Task<int> GetTotalWorkoutsCompleted()
    {
        await WaitForPageToLoad();
        
        var workoutsText = GetElementText(TotalWorkoutsCompleted) ?? GetElementText(WorkoutsCompletedValue);
        if (!string.IsNullOrEmpty(workoutsText))
        {
            var match = System.Text.RegularExpressions.Regex.Match(workoutsText, @"\d+");
            if (match.Success && int.TryParse(match.Value, out int count))
            {
                return count;
            }
        }
        
        return 0;
    }

    /// <summary>
    /// Export user data/backup
    /// </summary>
    public async Task<bool> ExportData()
    {
        await WaitForPageToLoad();
        
        if (await TapElement(ExportDataButton))
        {
            // Wait for export to complete or confirmation dialog
            await Task.Delay(2000);
            
            // Handle any confirmation dialogs
            var confirmButton = By.XPath("//android.widget.Button[contains(@text, 'Export') or contains(@text, 'Save') or contains(@text, 'OK')]");
            if (IsElementPresent(confirmButton))
            {
                return await TapElement(confirmButton);
            }
            
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Import user data/restore backup
    /// </summary>
    public async Task<bool> ImportData()
    {
        await WaitForPageToLoad();
        
        if (await TapElement(ImportDataButton))
        {
            // Wait for file picker or confirmation dialog
            await Task.Delay(2000);
            
            // Handle any confirmation dialogs
            var confirmButton = By.XPath("//android.widget.Button[contains(@text, 'Import') or contains(@text, 'Restore') or contains(@text, 'OK')]");
            if (IsElementPresent(confirmButton))
            {
                return await TapElement(confirmButton);
            }
            
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Clear all user data
    /// </summary>
    public async Task<bool> ClearAllData()
    {
        await WaitForPageToLoad();
        
        if (await TapElement(ClearDataButton))
        {
            // Wait for confirmation dialog
            await Task.Delay(1000);
            
            // Handle confirmation dialog - this is destructive so look for specific confirmation
            var confirmButton = By.XPath("//android.widget.Button[contains(@text, 'Clear') or contains(@text, 'Delete') or contains(@text, 'Confirm')]");
            if (IsElementPresent(confirmButton))
            {
                return await TapElement(confirmButton);
            }
        }
        
        return false;
    }

    /// <summary>
    /// Refresh statistics display
    /// </summary>
    public async Task<bool> RefreshStatistics()
    {
        await WaitForPageToLoad();
        
        if (IsElementPresent(RefreshStatsButton))
        {
            await TapElement(RefreshStatsButton);
            await Task.Delay(2000); // Wait for refresh to complete
            return true;
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

    /// <summary>
    /// Share statistics
    /// </summary>
    public async Task<bool> ShareStatistics()
    {
        await WaitForPageToLoad();
        
        if (await TapElement(ShareStatsButton))
        {
            // Wait for share dialog to appear
            await Task.Delay(2000);
            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Get list of unlocked achievements
    /// </summary>
    public async Task<List<string>> GetUnlockedAchievements()
    {
        await WaitForPageToLoad();
        
        // Scroll to achievements section if needed
        if (IsElementPresent(AchievementsSection))
        {
            await ScrollToElement(AchievementsSection);
        }
        
        return GetElementTexts(UnlockedAchievements);
    }

    /// <summary>
    /// Get current user level or rank
    /// </summary>
    public async Task<string> GetUserLevel()
    {
        await WaitForPageToLoad();
        
        var levelText = GetElementText(LevelIndicator);
        if (!string.IsNullOrEmpty(levelText))
        {
            return levelText;
        }
        
        return "Unknown";
    }

    /// <summary>
    /// Check if statistics section is visible
    /// </summary>
    public async Task<bool> IsStatisticsSectionVisible()
    {
        await WaitForPageToLoad();
        return IsElementPresent(StatisticsSection);
    }

    /// <summary>
    /// Check if backup section is visible
    /// </summary>
    public async Task<bool> IsBackupSectionVisible()
    {
        await WaitForPageToLoad();
        return IsElementPresent(BackupSection);
    }

    /// <summary>
    /// Verify that the profile page is properly loaded with expected elements
    /// </summary>
    public async Task<bool> VerifyPageLoaded()
    {
        if (!await WaitForPageToLoad())
        {
            return false;
        }

        // Check for key profile elements
        var hasStatistics = IsElementPresent(StatisticsSection);
        var hasBackup = IsElementPresent(BackupSection);
        var hasProfileHeader = IsElementPresent(ProfileHeader);

        return hasStatistics || hasBackup || hasProfileHeader;
    }

    /// <summary>
    /// Wait for statistics to load and display
    /// </summary>
    public async Task<bool> WaitForStatisticsToLoad(int timeoutSeconds = 15)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < endTime)
        {
            // Check if any statistics are loaded
            if (IsElementPresent(TotalSessionsCount) || 
                IsElementPresent(TotalPointsEarned) || 
                IsElementPresent(TotalWorkoutsCompleted))
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
    /// Verify that statistics show expected values
    /// </summary>
    public async Task<bool> VerifyStatistics(int expectedSessions, int expectedPoints, int expectedWorkouts)
    {
        await WaitForStatisticsToLoad();
        
        var actualSessions = await GetTotalSessions();
        var actualPoints = await GetTotalPointsEarned();
        var actualWorkouts = await GetTotalWorkoutsCompleted();
        
        return actualSessions == expectedSessions && 
               actualPoints == expectedPoints && 
               actualWorkouts == expectedWorkouts;
    }

    /// <summary>
    /// Verify that an achievement is unlocked
    /// </summary>
    public async Task<bool> VerifyAchievementUnlocked(string achievementName)
    {
        var achievements = await GetUnlockedAchievements();
        return achievements.Any(achievement => achievement.Contains(achievementName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verify that user level matches expected value
    /// </summary>
    public async Task<bool> VerifyUserLevel(string expectedLevel)
    {
        var actualLevel = await GetUserLevel();
        return actualLevel.Contains(expectedLevel, StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
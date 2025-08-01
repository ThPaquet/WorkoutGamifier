using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Android;
using WorkoutGamifier.UITests.Configuration;

namespace WorkoutGamifier.UITests.Utilities;

public class ScreenshotManager
{
    private readonly TestConfiguration _config;
    private readonly string _screenshotDirectory;

    public ScreenshotManager(TestConfiguration config)
    {
        _config = config;
        _screenshotDirectory = Path.Combine(Directory.GetCurrentDirectory(), config.ScreenshotPath);
        Directory.CreateDirectory(_screenshotDirectory);
    }

    public async Task<string> TakeScreenshot(AndroidDriver driver, string testName, string stepName, string? additionalInfo = null)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff");
            var fileName = $"{testName}_{stepName}_{timestamp}.png";
            var filePath = Path.Combine(_screenshotDirectory, fileName);

            var screenshot = driver.GetScreenshot();
            await File.WriteAllBytesAsync(filePath, screenshot.AsByteArray);

            // Log screenshot info
            var logMessage = $"Screenshot taken: {fileName}";
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                logMessage += $" - {additionalInfo}";
            }
            Console.WriteLine(logMessage);

            return filePath;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to take screenshot: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<List<string>> TakeScreenshotSeries(AndroidDriver driver, string testName, string seriesName, int count, int intervalMs = 1000)
    {
        var screenshots = new List<string>();
        
        for (int i = 0; i < count; i++)
        {
            var stepName = $"{seriesName}_step_{i + 1}";
            var filePath = await TakeScreenshot(driver, testName, stepName);
            if (!string.IsNullOrEmpty(filePath))
            {
                screenshots.Add(filePath);
            }
            
            if (i < count - 1) // Don't wait after the last screenshot
            {
                await Task.Delay(intervalMs);
            }
        }
        
        return screenshots;
    }

    public void CleanupOldScreenshots(int daysToKeep = 7)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
            var files = Directory.GetFiles(_screenshotDirectory, "*.png");
            
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    File.Delete(file);
                    Console.WriteLine($"Deleted old screenshot: {Path.GetFileName(file)}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to cleanup old screenshots: {ex.Message}");
        }
    }

    public string GetScreenshotDirectory() => _screenshotDirectory;
}
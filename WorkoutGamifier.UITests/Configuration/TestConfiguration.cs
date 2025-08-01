using Microsoft.Extensions.Configuration;

namespace WorkoutGamifier.UITests.Configuration;

public class TestConfiguration
{
    public string AppPath { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string PlatformName { get; set; } = "Android";
    public string PlatformVersion { get; set; } = "11.0";
    public string DeviceName { get; set; } = "Android Emulator";
    public string AppPackage { get; set; } = "com.workoutgamifier.app";
    public string AppActivity { get; set; } = "crc64e1fb321c08285b90.MainActivity";
    public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);
    public TimeSpan ImplicitWait { get; set; } = TimeSpan.FromSeconds(10);
    public bool TakeScreenshotsOnFailure { get; set; } = true;
    public bool TakeScreenshotsOnSuccess { get; set; } = false;
    public string ReportOutputPath { get; set; } = "TestResults";
    public string ScreenshotPath { get; set; } = "Screenshots";
    public string AppiumServerUrl { get; set; } = "http://127.0.0.1:4723";
    public bool AutoGrantPermissions { get; set; } = true;
    public bool NoReset { get; set; } = false;
    public bool FullReset { get; set; } = false;
    public int CommandTimeoutMinutes { get; set; } = 5;

    public static TestConfiguration Load()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development"}.json", optional: true)
            .AddEnvironmentVariables("UITEST_")
            .Build();

        var config = new TestConfiguration();
        configuration.GetSection("TestConfiguration").Bind(config);
        
        // Ensure directories exist
        Directory.CreateDirectory(config.ReportOutputPath);
        Directory.CreateDirectory(config.ScreenshotPath);
        
        return config;
    }

    public void Validate()
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(AppPath) && string.IsNullOrEmpty(AppPackage))
        {
            errors.Add("Either AppPath or AppPackage must be specified");
        }

        if (!string.IsNullOrEmpty(AppPath) && !File.Exists(AppPath))
        {
            errors.Add($"App file not found at path: {AppPath}");
        }

        if (string.IsNullOrEmpty(AppiumServerUrl))
        {
            errors.Add("AppiumServerUrl must be specified");
        }

        if (errors.Any())
        {
            throw new InvalidOperationException($"Test configuration validation failed:\n{string.Join("\n", errors)}");
        }
    }
}
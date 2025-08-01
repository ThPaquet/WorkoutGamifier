using WorkoutGamifier.Core.Services;
using WorkoutGamifier.Tests.TestData;

namespace WorkoutGamifier.Tests.Performance;

/// <summary>
/// Comprehensive performance tests for the WorkoutGamifier application
/// </summary>
public class PerformanceTests : IDisposable
{
    private readonly PerformanceTestRunner _performanceRunner;
    private readonly PerformanceTrendReporter _trendReporter;
    private readonly DatabaseTestFixture _fixture;

    public PerformanceTests()
    {
        _performanceRunner = new PerformanceTestRunner();
        _trendReporter = new PerformanceTrendReporter();
        _fixture = new DatabaseTestFixture();
    }

    [Fact]
    public async Task AppStartup_ShouldMeetPerformanceThreshold()
    {
        // Act
        var result = await _performanceRunner.MeasureAppStartupAsync();

        // Assert
        Assert.True(result.PassedBenchmark, 
            $"App startup took {result.Duration.TotalMilliseconds:F1}ms, exceeding threshold");
        Assert.True(result.Duration.TotalMilliseconds < 3000, 
            "App startup should complete within 3 seconds");
        Assert.False(string.IsNullOrEmpty(result.OperationName));
        Assert.True(result.Timestamp > DateTime.MinValue);
    }

    [Theory]
    [InlineData("Home", "Sessions")]
    [InlineData("Sessions", "WorkoutPools")]
    [InlineData("WorkoutPools", "Actions")]
    [InlineData("Actions", "Profile")]
    public async Task PageNavigation_ShouldMeetPerformanceThreshold(string fromPage, string toPage)
    {
        // Act
        var result = await _performanceRunner.MeasurePageNavigationAsync(fromPage, toPage);

        // Assert
        Assert.True(result.PassedBenchmark, 
            $"Navigation from {fromPage} to {toPage} took {result.Duration.TotalMilliseconds:F1}ms, exceeding threshold");
        Assert.True(result.Duration.TotalMilliseconds < 500, 
            "Page navigation should complete within 500ms");
        Assert.Contains(fromPage, result.OperationName);
        Assert.Contains(toPage, result.OperationName);
    }

    [Fact]
    public async Task DatabaseOperations_ShouldMeetPerformanceThresholds()
    {
        // Arrange
        var workoutService = _fixture.GetService<IWorkoutService>();
        await _fixture.SeedMinimalData();

        var operations = new Dictionary<string, Func<Task>>
        {
            ["Create Workout"] = async () => await workoutService.CreateWorkoutAsync(TestDataBuilder.Workout().Build()),
            ["Get All Workouts"] = async () => await workoutService.GetAllWorkoutsAsync(),
            ["Get Workout By ID"] = async () => await workoutService.GetWorkoutByIdAsync(1),
            ["Search Workouts"] = async () => await workoutService.SearchWorkoutsAsync("test"),
            ["Get Statistics"] = async () => await workoutService.GetWorkoutStatisticsAsync(),
            ["Update Workout"] = async () =>
            {
                var workout = await workoutService.GetWorkoutByIdAsync(1);
                if (workout != null)
                {
                    workout.Name = "Updated Name";
                    await workoutService.UpdateWorkoutAsync(workout);
                }
            }
        };

        // Act & Assert
        foreach (var (operationName, operation) in operations)
        {
            var result = await _performanceRunner.MeasureDatabaseOperationAsync(operationName, operation);
            
            Assert.True(result.PassedBenchmark, 
                $"Database operation '{operationName}' took {result.Duration.TotalMilliseconds:F1}ms, exceeding threshold");
            Assert.True(result.Duration.TotalMilliseconds < 100, 
                $"Database operation '{operationName}' should complete within 100ms");
        }
    }

    [Fact]
    public async Task MemoryUsage_ShouldStayWithinLimits()
    {
        // Act
        var result = await _performanceRunner.MeasureMemoryUsageAsync(TimeSpan.FromSeconds(5));

        // Assert
        Assert.True(result.PassedBenchmark, 
            $"Memory usage exceeded threshold: {result.MemoryUsed / (1024.0 * 1024.0):F2}MB");
        Assert.True(result.Duration >= TimeSpan.FromSeconds(4.5), 
            "Memory monitoring should run for the specified duration");
        Assert.Contains("Memory Usage", result.OperationName);
        
        // Check metadata
        Assert.True(result.Metadata.ContainsKey("MaxMemory"));
        Assert.True(result.Metadata.ContainsKey("AverageMemory"));
        Assert.True(result.Metadata.ContainsKey("SampleCount"));
    }

    [Fact]
    public async Task CpuUsage_ShouldStayWithinLimits()
    {
        // Arrange
        var workoutService = _fixture.GetService<IWorkoutService>();
        await _fixture.SeedPerformanceData(50, 25, 10);

        // Act - Measure CPU during intensive operation
        var result = await _performanceRunner.MeasureCpuUsageAsync("Bulk Data Processing", async () =>
        {
            // Simulate CPU-intensive work
            var workouts = await workoutService.GetAllWorkoutsAsync();
            var statistics = await workoutService.GetWorkoutStatisticsAsync();
            
            // Perform multiple searches
            for (int i = 0; i < 10; i++)
            {
                await workoutService.SearchWorkoutsAsync($"search{i}");
            }
        });

        // Assert
        Assert.True(result.PassedBenchmark, 
            $"CPU usage exceeded threshold: {result.CpuUsage:F1}%");
        Assert.Contains("CPU Usage", result.OperationName);
        
        // Check metadata
        Assert.True(result.Metadata.ContainsKey("AverageCpuUsage"));
        Assert.True(result.Metadata.ContainsKey("MaxCpuUsage"));
        Assert.True(result.Metadata.ContainsKey("SampleCount"));
    }

    [Fact]
    public async Task BulkOperations_ShouldMeetPerformanceThresholds()
    {
        // Arrange
        var workoutService = _fixture.GetService<IWorkoutService>();
        var bulkWorkouts = TestDataScenarios.PerformanceScenarios.GetLargeWorkoutSet(100);

        // Act
        var result = await _performanceRunner.MeasureDatabaseOperationAsync("Bulk Create 100 Workouts", async () =>
        {
            await workoutService.BulkCreateWorkoutsAsync(bulkWorkouts);
        });

        // Assert
        Assert.True(result.PassedBenchmark, 
            $"Bulk operation took {result.Duration.TotalMilliseconds:F1}ms, exceeding threshold");
        Assert.True(result.Duration.TotalSeconds < 5, 
            "Bulk creation of 100 workouts should complete within 5 seconds");
    }

    [Fact]
    public async Task ComplexQueries_ShouldMeetPerformanceThresholds()
    {
        // Arrange
        var workoutService = _fixture.GetService<IWorkoutService>();
        await _fixture.SeedPerformanceData(200, 100, 50);

        var complexOperations = new Dictionary<string, Func<Task>>
        {
            ["Filter by Difficulty"] = async () => await workoutService.GetWorkoutsByDifficultyAsync(Core.Models.DifficultyLevel.Intermediate),
            ["Filter by Duration Range"] = async () => await workoutService.GetWorkoutsByDurationRangeAsync(15, 45),
            ["Complex Search"] = async () => await workoutService.SearchWorkoutsAsync("workout strength"),
            ["Get Recent Workouts"] = async () => await workoutService.GetRecentWorkoutsAsync(20),
            ["Calculate Statistics"] = async () => await workoutService.GetWorkoutStatisticsAsync()
        };

        // Act & Assert
        foreach (var (operationName, operation) in complexOperations)
        {
            var result = await _performanceRunner.MeasureDatabaseOperationAsync(operationName, operation);
            
            Assert.True(result.PassedBenchmark, 
                $"Complex query '{operationName}' took {result.Duration.TotalMilliseconds:F1}ms, exceeding threshold");
            Assert.True(result.Duration.TotalMilliseconds < 200, 
                $"Complex query '{operationName}' should complete within 200ms");
        }
    }

    [Fact]
    public async Task SessionWorkflow_ShouldMeetPerformanceThresholds()
    {
        // Arrange
        var sessionService = _fixture.GetService<ISessionService>();
        var scenario = await _fixture.CreateCompleteWorkflowScenario();

        // Act - Measure complete session workflow
        var result = await _performanceRunner.MeasureCpuUsageAsync("Complete Session Workflow", async () =>
        {
            // Start session
            var session = await sessionService.StartSessionAsync("Performance Test Session", scenario.Pool.Id);
            
            // Complete actions
            foreach (var action in scenario.Actions.Take(3))
            {
                await sessionService.CompleteActionAsync(session.Id, action.Id);
            }
            
            // Spend points for workout
            await sessionService.SpendPointsForWorkoutAsync(session.Id, 10);
            
            // End session
            await sessionService.EndSessionAsync(session.Id);
        });

        // Assert
        Assert.True(result.Duration.TotalSeconds < 2, 
            "Complete session workflow should finish within 2 seconds");
        
        // CPU measurement might be 0 on fast operations, which is acceptable
        Assert.True(result.CpuUsage >= 0, "CPU usage should be non-negative");
        
        // For this test, we mainly care about duration, not CPU usage benchmark
        // since CPU measurement can be unreliable in test environments
    }

    [Fact]
    public async Task ComprehensiveBenchmarkSuite_ShouldPass()
    {
        // Act
        var benchmarkResult = await _performanceRunner.RunBenchmarkSuiteAsync();

        // Assert
        Assert.True(benchmarkResult.OverallPassed, 
            $"Benchmark suite failed: {benchmarkResult.GetSummary()}");
        Assert.True(benchmarkResult.PassRate >= 80, 
            $"Pass rate too low: {benchmarkResult.PassRate:F1}%");
        Assert.True(benchmarkResult.TotalCount >= 10, 
            "Benchmark suite should include at least 10 tests");
        
        // Verify no critical failures
        var failedResults = benchmarkResult.GetFailedResults();
        var criticalFailures = failedResults.Where(r => 
            r.Duration.TotalSeconds > 5 || 
            r.MemoryUsed > 200 * 1024 * 1024 || // 200MB
            !string.IsNullOrEmpty(r.ErrorMessage)).ToList();
        
        Assert.Empty(criticalFailures);
    }

    [Fact]
    public async Task PerformanceTrendReporting_ShouldGenerateReport()
    {
        // Arrange
        var benchmarkResult = await _performanceRunner.RunBenchmarkSuiteAsync();

        // Act
        var trendReport = await _trendReporter.GenerateTrendReportAsync(
            benchmarkResult, 
            version: "1.0.0-test", 
            environment: "test");

        // Assert
        Assert.NotNull(trendReport);
        Assert.Contains("Performance Trend Report", trendReport);
        Assert.Contains("Current Results Summary", trendReport);
        Assert.Contains("Performance Statistics", trendReport);
        Assert.Contains("Recommendations", trendReport);
    }

    [Fact]
    public async Task PerformanceHtmlDashboard_ShouldGenerate()
    {
        // Arrange
        var benchmarkResult = await _performanceRunner.RunBenchmarkSuiteAsync();

        // Act
        var htmlDashboard = await _trendReporter.GenerateHtmlDashboardAsync(benchmarkResult);

        // Assert
        Assert.NotNull(htmlDashboard);
        Assert.Contains("<!DOCTYPE html>", htmlDashboard);
        Assert.Contains("Performance Dashboard", htmlDashboard);
        Assert.Contains("Test Results", htmlDashboard);
        Assert.Contains("table", htmlDashboard);
    }

    [Fact]
    public async Task PerformanceConfiguration_ShouldAffectThresholds()
    {
        // Arrange
        var strictConfig = PerformanceConfiguration.Strict();
        var relaxedConfig = PerformanceConfiguration.Relaxed();
        
        using var strictRunner = new PerformanceTestRunner(strictConfig);
        using var relaxedRunner = new PerformanceTestRunner(relaxedConfig);

        // Act
        var strictResult = await strictRunner.MeasureAppStartupAsync();
        var relaxedResult = await relaxedRunner.MeasureAppStartupAsync();

        // Assert - Same operation, different thresholds should potentially give different results
        Assert.Equal(strictResult.OperationName, relaxedResult.OperationName);
        
        // Strict config should have lower thresholds
        Assert.True(strictConfig.AppStartupThresholdMs < relaxedConfig.AppStartupThresholdMs);
        Assert.True(strictConfig.MaxMemoryUsageBytes < relaxedConfig.MaxMemoryUsageBytes);
        Assert.True(strictConfig.MaxCpuUsagePercent < relaxedConfig.MaxCpuUsagePercent);
    }

    [Fact]
    public async Task PerformanceResults_ShouldContainMetadata()
    {
        // Act
        var result = await _performanceRunner.MeasureAppStartupAsync();

        // Assert
        Assert.NotNull(result.Metadata);
        Assert.True(result.Metadata.Count > 0);
        
        var metrics = result.GetMetrics();
        Assert.Contains("OperationName", metrics.Keys);
        Assert.Contains("DurationMs", metrics.Keys);
        Assert.Contains("MemoryUsedMB", metrics.Keys);
        Assert.Contains("CpuUsagePercent", metrics.Keys);
        Assert.Contains("PassedBenchmark", metrics.Keys);
        
        var summary = result.GetSummary();
        Assert.Contains(result.OperationName, summary);
        Assert.Contains(result.PassedBenchmark ? "PASSED" : "FAILED", summary);
    }

    public void Dispose()
    {
        _performanceRunner?.Dispose();
        _fixture?.Dispose();
    }
}
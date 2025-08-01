using System.Text;
using System.Text.Json;

namespace WorkoutGamifier.Tests.Performance;

/// <summary>
/// Generates performance trend reports and tracks performance changes over time
/// </summary>
public class PerformanceTrendReporter
{
    private readonly string _dataDirectory;
    private readonly string _reportDirectory;

    public PerformanceTrendReporter(string? dataDirectory = null, string? reportDirectory = null)
    {
        _dataDirectory = dataDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "PerformanceData");
        _reportDirectory = reportDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "PerformanceReports");
        
        Directory.CreateDirectory(_dataDirectory);
        Directory.CreateDirectory(_reportDirectory);
    }

    /// <summary>
    /// Saves performance results for trend tracking
    /// </summary>
    public async Task SavePerformanceDataAsync(PerformanceBenchmarkResult benchmarkResult, string version = "", string environment = "")
    {
        var trendPoints = benchmarkResult.Results.Select(result => new PerformanceTrendPoint
        {
            Timestamp = result.Timestamp,
            OperationName = result.OperationName,
            DurationMs = result.Duration.TotalMilliseconds,
            MemoryUsageBytes = result.MemoryUsed,
            CpuUsagePercent = result.CpuUsage,
            PassedBenchmark = result.PassedBenchmark,
            Version = version,
            Environment = environment
        }).ToList();

        var fileName = $"performance_data_{DateTime.UtcNow:yyyyMMdd_HHmmss}.json";
        var filePath = Path.Combine(_dataDirectory, fileName);
        
        var jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(trendPoints, jsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// Loads historical performance data for trend analysis
    /// </summary>
    public async Task<List<PerformanceTrendPoint>> LoadHistoricalDataAsync(int maxDays = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-maxDays);
        var allTrendPoints = new List<PerformanceTrendPoint>();

        var dataFiles = Directory.GetFiles(_dataDirectory, "performance_data_*.json")
            .Where(f => File.GetCreationTime(f) >= cutoffDate)
            .OrderBy(f => f);

        foreach (var file in dataFiles)
        {
            try
            {
                var json = await File.ReadAllTextAsync(file);
                var trendPoints = JsonSerializer.Deserialize<List<PerformanceTrendPoint>>(json, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                
                if (trendPoints != null)
                {
                    allTrendPoints.AddRange(trendPoints);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not load performance data from {file}: {ex.Message}");
            }
        }

        return allTrendPoints.OrderBy(p => p.Timestamp).ToList();
    }

    /// <summary>
    /// Generates a comprehensive performance trend report
    /// </summary>
    public async Task<string> GenerateTrendReportAsync(PerformanceBenchmarkResult currentResults, string version = "", string environment = "")
    {
        // Save current results
        await SavePerformanceDataAsync(currentResults, version, environment);
        
        // Load historical data
        var historicalData = await LoadHistoricalDataAsync();
        
        // Generate report
        var report = new StringBuilder();
        report.AppendLine("# Performance Trend Report");
        report.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
        report.AppendLine($"Version: {version}");
        report.AppendLine($"Environment: {environment}");
        report.AppendLine();

        // Current results summary
        report.AppendLine("## Current Results Summary");
        report.AppendLine(currentResults.GetSummary());
        report.AppendLine();

        // Current results details
        report.AppendLine("## Current Test Results");
        report.AppendLine("| Operation | Duration (ms) | Memory (MB) | CPU (%) | Status |");
        report.AppendLine("|-----------|---------------|-------------|---------|--------|");
        
        foreach (var result in currentResults.Results)
        {
            var memoryMB = result.MemoryUsed / (1024.0 * 1024.0);
            var status = result.PassedBenchmark ? "✅ PASS" : "❌ FAIL";
            report.AppendLine($"| {result.OperationName} | {result.Duration.TotalMilliseconds:F1} | {memoryMB:F2} | {result.CpuUsage:F1} | {status} |");
        }
        report.AppendLine();

        // Trend analysis
        if (historicalData.Any())
        {
            report.AppendLine("## Trend Analysis");
            
            var operationNames = currentResults.Results.Select(r => r.OperationName).Distinct();
            
            foreach (var operationName in operationNames)
            {
                var operationData = historicalData.Where(h => h.OperationName == operationName).ToList();
                if (operationData.Count >= 2)
                {
                    var trendAnalysis = PerformanceTrendAnalysis.Analyze(operationName, operationData);
                    
                    report.AppendLine($"### {operationName}");
                    report.AppendLine($"**Trend Summary:** {trendAnalysis.TrendSummary}");
                    report.AppendLine($"- Duration Trend: {trendAnalysis.DurationTrendPercent:F1}%");
                    report.AppendLine($"- Memory Trend: {trendAnalysis.MemoryTrendPercent:F1}%");
                    report.AppendLine($"- CPU Trend: {trendAnalysis.CpuTrendPercent:F1}%");
                    
                    if (trendAnalysis.IsDegrading)
                    {
                        report.AppendLine("⚠️ **Performance degradation detected!**");
                    }
                    else if (trendAnalysis.IsImproving)
                    {
                        report.AppendLine("✅ **Performance improvement detected!**");
                    }
                    
                    report.AppendLine();
                }
            }
        }
        else
        {
            report.AppendLine("## Trend Analysis");
            report.AppendLine("No historical data available for trend analysis. This is the first run.");
            report.AppendLine();
        }

        // Performance statistics
        var stats = currentResults.GetStatistics();
        var memoryStats = stats.GetMemoryUsageMB();
        
        report.AppendLine("## Performance Statistics");
        report.AppendLine("### Duration (ms)");
        report.AppendLine($"- Average: {stats.AverageDurationMs:F1}");
        report.AppendLine($"- Min: {stats.MinDurationMs:F1}");
        report.AppendLine($"- Max: {stats.MaxDurationMs:F1}");
        report.AppendLine($"- Median: {stats.MedianDurationMs:F1}");
        report.AppendLine();
        
        report.AppendLine("### Memory Usage (MB)");
        report.AppendLine($"- Average: {memoryStats.Average:F2}");
        report.AppendLine($"- Min: {memoryStats.Min:F2}");
        report.AppendLine($"- Max: {memoryStats.Max:F2}");
        report.AppendLine($"- Median: {memoryStats.Median:F2}");
        report.AppendLine();
        
        report.AppendLine("### CPU Usage (%)");
        report.AppendLine($"- Average: {stats.AverageCpuUsagePercent:F1}");
        report.AppendLine($"- Min: {stats.MinCpuUsagePercent:F1}");
        report.AppendLine($"- Max: {stats.MaxCpuUsagePercent:F1}");
        report.AppendLine($"- Median: {stats.MedianCpuUsagePercent:F1}");
        report.AppendLine();

        // Failed tests details
        var failedResults = currentResults.GetFailedResults();
        if (failedResults.Any())
        {
            report.AppendLine("## Failed Tests Details");
            foreach (var failed in failedResults)
            {
                report.AppendLine($"### {failed.OperationName}");
                report.AppendLine($"- Duration: {failed.Duration.TotalMilliseconds:F1}ms");
                report.AppendLine($"- Memory: {failed.MemoryUsed / (1024.0 * 1024.0):F2}MB");
                report.AppendLine($"- CPU: {failed.CpuUsage:F1}%");
                
                if (!string.IsNullOrEmpty(failed.ErrorMessage))
                {
                    report.AppendLine($"- Error: {failed.ErrorMessage}");
                }
                
                if (failed.Metadata.Any())
                {
                    report.AppendLine("- Metadata:");
                    foreach (var kvp in failed.Metadata)
                    {
                        report.AppendLine($"  - {kvp.Key}: {kvp.Value}");
                    }
                }
                report.AppendLine();
            }
        }

        // Recommendations
        report.AppendLine("## Recommendations");
        var recommendations = GenerateRecommendations(currentResults, historicalData);
        foreach (var recommendation in recommendations)
        {
            report.AppendLine($"- {recommendation}");
        }

        var reportContent = report.ToString();
        
        // Save report to file
        var reportFileName = $"performance_report_{DateTime.UtcNow:yyyyMMdd_HHmmss}.md";
        var reportPath = Path.Combine(_reportDirectory, reportFileName);
        await File.WriteAllTextAsync(reportPath, reportContent);
        
        return reportContent;
    }

    /// <summary>
    /// Generates an HTML performance dashboard
    /// </summary>
    public async Task<string> GenerateHtmlDashboardAsync(PerformanceBenchmarkResult currentResults)
    {
        var historicalData = await LoadHistoricalDataAsync();
        
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html>");
        html.AppendLine("<head>");
        html.AppendLine("    <title>Performance Dashboard</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("        .header { background-color: #f0f0f0; padding: 20px; border-radius: 5px; }");
        html.AppendLine("        .summary { display: flex; gap: 20px; margin: 20px 0; }");
        html.AppendLine("        .metric { background-color: #e8f4f8; padding: 15px; border-radius: 5px; flex: 1; }");
        html.AppendLine("        .pass { color: green; }");
        html.AppendLine("        .fail { color: red; }");
        html.AppendLine("        table { border-collapse: collapse; width: 100%; margin: 20px 0; }");
        html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
        html.AppendLine("        th { background-color: #f2f2f2; }");
        html.AppendLine("        .chart { margin: 20px 0; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        
        // Header
        html.AppendLine("    <div class='header'>");
        html.AppendLine("        <h1>Performance Dashboard</h1>");
        html.AppendLine($"        <p>Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        html.AppendLine($"        <p>{currentResults.GetSummary()}</p>");
        html.AppendLine("    </div>");
        
        // Summary metrics
        var stats = currentResults.GetStatistics();
        html.AppendLine("    <div class='summary'>");
        html.AppendLine("        <div class='metric'>");
        html.AppendLine("            <h3>Average Duration</h3>");
        html.AppendLine($"            <p>{stats.AverageDurationMs:F1} ms</p>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div class='metric'>");
        html.AppendLine("            <h3>Average Memory</h3>");
        html.AppendLine($"            <p>{stats.AverageMemoryUsageBytes / (1024.0 * 1024.0):F2} MB</p>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div class='metric'>");
        html.AppendLine("            <h3>Average CPU</h3>");
        html.AppendLine($"            <p>{stats.AverageCpuUsagePercent:F1}%</p>");
        html.AppendLine("        </div>");
        html.AppendLine("        <div class='metric'>");
        html.AppendLine("            <h3>Pass Rate</h3>");
        html.AppendLine($"            <p>{currentResults.PassRate:F1}%</p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");
        
        // Results table
        html.AppendLine("    <h2>Test Results</h2>");
        html.AppendLine("    <table>");
        html.AppendLine("        <tr>");
        html.AppendLine("            <th>Operation</th>");
        html.AppendLine("            <th>Duration (ms)</th>");
        html.AppendLine("            <th>Memory (MB)</th>");
        html.AppendLine("            <th>CPU (%)</th>");
        html.AppendLine("            <th>Status</th>");
        html.AppendLine("        </tr>");
        
        foreach (var result in currentResults.Results)
        {
            var memoryMB = result.MemoryUsed / (1024.0 * 1024.0);
            var statusClass = result.PassedBenchmark ? "pass" : "fail";
            var statusText = result.PassedBenchmark ? "PASS" : "FAIL";
            
            html.AppendLine("        <tr>");
            html.AppendLine($"            <td>{result.OperationName}</td>");
            html.AppendLine($"            <td>{result.Duration.TotalMilliseconds:F1}</td>");
            html.AppendLine($"            <td>{memoryMB:F2}</td>");
            html.AppendLine($"            <td>{result.CpuUsage:F1}</td>");
            html.AppendLine($"            <td class='{statusClass}'>{statusText}</td>");
            html.AppendLine("        </tr>");
        }
        
        html.AppendLine("    </table>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        
        var htmlContent = html.ToString();
        
        // Save HTML dashboard
        var dashboardFileName = $"performance_dashboard_{DateTime.UtcNow:yyyyMMdd_HHmmss}.html";
        var dashboardPath = Path.Combine(_reportDirectory, dashboardFileName);
        await File.WriteAllTextAsync(dashboardPath, htmlContent);
        
        return htmlContent;
    }

    private List<string> GenerateRecommendations(PerformanceBenchmarkResult currentResults, List<PerformanceTrendPoint> historicalData)
    {
        var recommendations = new List<string>();
        
        // Check for failed tests
        var failedResults = currentResults.GetFailedResults();
        if (failedResults.Any())
        {
            recommendations.Add($"Address {failedResults.Count} failing performance tests to meet benchmarks");
            
            foreach (var failed in failedResults.Take(3)) // Top 3 failures
            {
                if (failed.Duration.TotalMilliseconds > 1000)
                {
                    recommendations.Add($"Optimize {failed.OperationName} - duration exceeds 1 second");
                }
                
                if (failed.MemoryUsed > 50 * 1024 * 1024) // 50MB
                {
                    recommendations.Add($"Investigate memory usage in {failed.OperationName} - using {failed.MemoryUsed / (1024.0 * 1024.0):F1}MB");
                }
            }
        }
        
        // Check for trends
        if (historicalData.Any())
        {
            var operationNames = currentResults.Results.Select(r => r.OperationName).Distinct();
            
            foreach (var operationName in operationNames)
            {
                var operationData = historicalData.Where(h => h.OperationName == operationName).ToList();
                if (operationData.Count >= 5) // Need enough data points
                {
                    var trendAnalysis = PerformanceTrendAnalysis.Analyze(operationName, operationData);
                    
                    if (trendAnalysis.IsDegrading)
                    {
                        recommendations.Add($"Performance regression detected in {operationName} - investigate recent changes");
                    }
                }
            }
        }
        
        // General recommendations
        var stats = currentResults.GetStatistics();
        if (stats.MaxDurationMs > 2000)
        {
            recommendations.Add("Consider implementing caching for operations taking longer than 2 seconds");
        }
        
        if (stats.MaxMemoryUsageBytes > 100 * 1024 * 1024) // 100MB
        {
            recommendations.Add("Monitor memory usage - some operations are using significant memory");
        }
        
        if (recommendations.Count == 0)
        {
            recommendations.Add("All performance tests are passing - maintain current optimization levels");
        }
        
        return recommendations;
    }
}
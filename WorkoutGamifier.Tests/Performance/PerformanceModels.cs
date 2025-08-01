namespace WorkoutGamifier.Tests.Performance;

/// <summary>
/// Represents the result of a single performance test
/// </summary>
public class PerformanceResult
{
    public string OperationName { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public long MemoryUsed { get; set; }
    public double CpuUsage { get; set; }
    public bool PassedBenchmark { get; set; }
    public DateTime Timestamp { get; set; }
    public string? ErrorMessage { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Gets a human-readable summary of the performance result
    /// </summary>
    public string GetSummary()
    {
        var status = PassedBenchmark ? "PASSED" : "FAILED";
        var memoryMB = MemoryUsed / (1024.0 * 1024.0);
        
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            return $"{OperationName}: ERROR - {ErrorMessage}";
        }
        
        return $"{OperationName}: {status} - {Duration.TotalMilliseconds:F1}ms, {memoryMB:F2}MB, {CpuUsage:F1}% CPU";
    }

    /// <summary>
    /// Gets performance metrics as a dictionary for reporting
    /// </summary>
    public Dictionary<string, object> GetMetrics()
    {
        var metrics = new Dictionary<string, object>
        {
            ["OperationName"] = OperationName,
            ["DurationMs"] = Duration.TotalMilliseconds,
            ["MemoryUsedBytes"] = MemoryUsed,
            ["MemoryUsedMB"] = MemoryUsed / (1024.0 * 1024.0),
            ["CpuUsagePercent"] = CpuUsage,
            ["PassedBenchmark"] = PassedBenchmark,
            ["Timestamp"] = Timestamp,
            ["HasError"] = !string.IsNullOrEmpty(ErrorMessage)
        };

        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            metrics["ErrorMessage"] = ErrorMessage;
        }

        foreach (var kvp in Metadata)
        {
            metrics[$"Meta_{kvp.Key}"] = kvp.Value;
        }

        return metrics;
    }
}

/// <summary>
/// Represents the result of a complete performance benchmark suite
/// </summary>
public class PerformanceBenchmarkResult
{
    public string SuiteName { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public List<PerformanceResult> Results { get; set; } = new();
    public int PassedCount { get; set; }
    public int FailedCount { get; set; }
    public bool OverallPassed { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Gets the total number of tests in the suite
    /// </summary>
    public int TotalCount => Results.Count;

    /// <summary>
    /// Gets the pass rate as a percentage
    /// </summary>
    public double PassRate => TotalCount > 0 ? (PassedCount / (double)TotalCount) * 100 : 0;

    /// <summary>
    /// Gets a summary of the benchmark results
    /// </summary>
    public string GetSummary()
    {
        if (!string.IsNullOrEmpty(ErrorMessage))
        {
            return $"{SuiteName}: ERROR - {ErrorMessage}";
        }

        var status = OverallPassed ? "PASSED" : "FAILED";
        return $"{SuiteName}: {status} - {PassedCount}/{TotalCount} tests passed ({PassRate:F1}%) in {TotalDuration.TotalSeconds:F1}s";
    }

    /// <summary>
    /// Gets failed test results
    /// </summary>
    public List<PerformanceResult> GetFailedResults()
    {
        return Results.Where(r => !r.PassedBenchmark).ToList();
    }

    /// <summary>
    /// Gets performance statistics for the suite
    /// </summary>
    public PerformanceStatistics GetStatistics()
    {
        if (!Results.Any())
        {
            return new PerformanceStatistics();
        }

        var durations = Results.Select(r => r.Duration.TotalMilliseconds).ToList();
        var memoryUsages = Results.Select(r => r.MemoryUsed).ToList();
        var cpuUsages = Results.Select(r => r.CpuUsage).ToList();

        return new PerformanceStatistics
        {
            AverageDurationMs = durations.Average(),
            MinDurationMs = durations.Min(),
            MaxDurationMs = durations.Max(),
            MedianDurationMs = GetMedian(durations),
            
            AverageMemoryUsageBytes = (long)memoryUsages.Average(),
            MinMemoryUsageBytes = memoryUsages.Min(),
            MaxMemoryUsageBytes = memoryUsages.Max(),
            MedianMemoryUsageBytes = (long)GetMedian(memoryUsages.Select(m => (double)m).ToList()),
            
            AverageCpuUsagePercent = cpuUsages.Average(),
            MinCpuUsagePercent = cpuUsages.Min(),
            MaxCpuUsagePercent = cpuUsages.Max(),
            MedianCpuUsagePercent = GetMedian(cpuUsages)
        };
    }

    private static double GetMedian(List<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        var count = sorted.Count;
        
        if (count == 0) return 0;
        if (count % 2 == 0)
        {
            return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
        }
        else
        {
            return sorted[count / 2];
        }
    }
}

/// <summary>
/// Statistical analysis of performance results
/// </summary>
public class PerformanceStatistics
{
    public double AverageDurationMs { get; set; }
    public double MinDurationMs { get; set; }
    public double MaxDurationMs { get; set; }
    public double MedianDurationMs { get; set; }
    
    public long AverageMemoryUsageBytes { get; set; }
    public long MinMemoryUsageBytes { get; set; }
    public long MaxMemoryUsageBytes { get; set; }
    public long MedianMemoryUsageBytes { get; set; }
    
    public double AverageCpuUsagePercent { get; set; }
    public double MinCpuUsagePercent { get; set; }
    public double MaxCpuUsagePercent { get; set; }
    public double MedianCpuUsagePercent { get; set; }

    /// <summary>
    /// Gets memory usage statistics in MB
    /// </summary>
    public (double Average, double Min, double Max, double Median) GetMemoryUsageMB()
    {
        return (
            AverageMemoryUsageBytes / (1024.0 * 1024.0),
            MinMemoryUsageBytes / (1024.0 * 1024.0),
            MaxMemoryUsageBytes / (1024.0 * 1024.0),
            MedianMemoryUsageBytes / (1024.0 * 1024.0)
        );
    }
}

/// <summary>
/// Configuration for performance testing thresholds and settings
/// </summary>
public class PerformanceConfiguration
{
    /// <summary>
    /// Maximum acceptable app startup time in milliseconds
    /// </summary>
    public int AppStartupThresholdMs { get; set; } = 3000; // 3 seconds

    /// <summary>
    /// Maximum acceptable page navigation time in milliseconds
    /// </summary>
    public int PageNavigationThresholdMs { get; set; } = 500; // 500ms

    /// <summary>
    /// Maximum acceptable database operation time in milliseconds
    /// </summary>
    public int DatabaseOperationThresholdMs { get; set; } = 100; // 100ms

    /// <summary>
    /// Maximum acceptable memory usage in bytes
    /// </summary>
    public long MaxMemoryUsageBytes { get; set; } = 150 * 1024 * 1024; // 150MB

    /// <summary>
    /// Maximum acceptable CPU usage percentage
    /// </summary>
    public double MaxCpuUsagePercent { get; set; } = 80.0; // 80%

    /// <summary>
    /// Number of warmup iterations before measuring performance
    /// </summary>
    public int WarmupIterations { get; set; } = 3;

    /// <summary>
    /// Number of measurement iterations for averaging
    /// </summary>
    public int MeasurementIterations { get; set; } = 5;

    /// <summary>
    /// Whether to force garbage collection before measurements
    /// </summary>
    public bool ForceGCBeforeMeasurement { get; set; } = true;

    /// <summary>
    /// Creates a configuration with strict performance requirements
    /// </summary>
    public static PerformanceConfiguration Strict()
    {
        return new PerformanceConfiguration
        {
            AppStartupThresholdMs = 2000,
            PageNavigationThresholdMs = 300,
            DatabaseOperationThresholdMs = 50,
            MaxMemoryUsageBytes = 100 * 1024 * 1024, // 100MB
            MaxCpuUsagePercent = 60.0
        };
    }

    /// <summary>
    /// Creates a configuration with relaxed performance requirements
    /// </summary>
    public static PerformanceConfiguration Relaxed()
    {
        return new PerformanceConfiguration
        {
            AppStartupThresholdMs = 5000,
            PageNavigationThresholdMs = 1000,
            DatabaseOperationThresholdMs = 200,
            MaxMemoryUsageBytes = 200 * 1024 * 1024, // 200MB
            MaxCpuUsagePercent = 90.0
        };
    }
}

/// <summary>
/// Represents a performance trend data point for tracking changes over time
/// </summary>
public class PerformanceTrendPoint
{
    public DateTime Timestamp { get; set; }
    public string OperationName { get; set; } = string.Empty;
    public double DurationMs { get; set; }
    public long MemoryUsageBytes { get; set; }
    public double CpuUsagePercent { get; set; }
    public bool PassedBenchmark { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
}

/// <summary>
/// Analyzes performance trends over time
/// </summary>
public class PerformanceTrendAnalysis
{
    public string OperationName { get; set; } = string.Empty;
    public List<PerformanceTrendPoint> DataPoints { get; set; } = new();
    public double DurationTrendPercent { get; set; }
    public double MemoryTrendPercent { get; set; }
    public double CpuTrendPercent { get; set; }
    public bool IsImproving { get; set; }
    public bool IsDegrading { get; set; }
    public string TrendSummary { get; set; } = string.Empty;

    /// <summary>
    /// Analyzes trend from a list of performance trend points
    /// </summary>
    public static PerformanceTrendAnalysis Analyze(string operationName, List<PerformanceTrendPoint> points)
    {
        if (points.Count < 2)
        {
            return new PerformanceTrendAnalysis
            {
                OperationName = operationName,
                DataPoints = points,
                TrendSummary = "Insufficient data for trend analysis"
            };
        }

        var sortedPoints = points.OrderBy(p => p.Timestamp).ToList();
        var first = sortedPoints.First();
        var last = sortedPoints.Last();

        var durationTrend = ((last.DurationMs - first.DurationMs) / first.DurationMs) * 100;
        var memoryTrend = ((last.MemoryUsageBytes - first.MemoryUsageBytes) / (double)first.MemoryUsageBytes) * 100;
        var cpuTrend = ((last.CpuUsagePercent - first.CpuUsagePercent) / first.CpuUsagePercent) * 100;

        var isImproving = durationTrend < -5 || memoryTrend < -5 || cpuTrend < -5; // 5% improvement threshold
        var isDegrading = durationTrend > 10 || memoryTrend > 10 || cpuTrend > 10; // 10% degradation threshold

        var trendSummary = GenerateTrendSummary(durationTrend, memoryTrend, cpuTrend, isImproving, isDegrading);

        return new PerformanceTrendAnalysis
        {
            OperationName = operationName,
            DataPoints = sortedPoints,
            DurationTrendPercent = durationTrend,
            MemoryTrendPercent = memoryTrend,
            CpuTrendPercent = cpuTrend,
            IsImproving = isImproving,
            IsDegrading = isDegrading,
            TrendSummary = trendSummary
        };
    }

    private static string GenerateTrendSummary(double durationTrend, double memoryTrend, double cpuTrend, bool isImproving, bool isDegrading)
    {
        if (isDegrading)
        {
            var worstTrend = Math.Max(Math.Max(durationTrend, memoryTrend), cpuTrend);
            return $"Performance degrading: {worstTrend:F1}% increase in worst metric";
        }
        
        if (isImproving)
        {
            var bestTrend = Math.Min(Math.Min(durationTrend, memoryTrend), cpuTrend);
            return $"Performance improving: {Math.Abs(bestTrend):F1}% decrease in best metric";
        }
        
        return "Performance stable: no significant trends detected";
    }
}
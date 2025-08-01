using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorkoutGamifier.Core.Data;
using WorkoutGamifier.Core.Models;
using WorkoutGamifier.Core.Repositories;
using WorkoutGamifier.Core.Services;
using Xunit;

namespace WorkoutGamifier.Tests.Services;

public class BackupServiceTests : IDisposable
{
    private readonly TestDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly BackupService _backupService;

    public BackupServiceTests()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _backupService = new BackupService(_unitOfWork);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task ExportDataAsync_WithEmptyDatabase_ReturnsValidJson()
    {
        // Act
        var result = await _backupService.ExportDataAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        var backupData = JsonSerializer.Deserialize<BackupData>(result, options);
        Assert.NotNull(backupData);
        Assert.NotNull(backupData.Workouts);
        Assert.NotNull(backupData.Actions);
        Assert.NotNull(backupData.WorkoutPools);
        Assert.NotNull(backupData.Sessions);
        Assert.True(backupData.ExportedAt > DateTime.MinValue);
        Assert.NotEmpty(backupData.AppVersion);
    }

    [Fact]
    public async Task ExportDataAsync_WithSampleData_ExportsAllEntities()
    {
        // Arrange
        await SeedSampleDataAsync();

        // Act
        var result = await _backupService.ExportDataAsync();

        // Assert
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };
        var backupData = JsonSerializer.Deserialize<BackupData>(result, options);
        Assert.NotNull(backupData);
        
        Assert.Single(backupData.Workouts);
        Assert.Single(backupData.Actions);
        Assert.Single(backupData.WorkoutPools);
        Assert.Single(backupData.Sessions);
        Assert.Single(backupData.WorkoutPoolWorkouts);
        Assert.Single(backupData.ActionCompletions);
        Assert.Single(backupData.WorkoutReceived);
    }

    [Fact]
    public async Task ValidateBackupDataAsync_WithValidData_ReturnsValid()
    {
        // Arrange
        await SeedSampleDataAsync();
        var jsonData = await _backupService.ExportDataAsync();

        // Act
        var result = await _backupService.ValidateBackupDataAsync(jsonData);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task ValidateBackupDataAsync_WithEmptyData_ReturnsInvalid()
    {
        // Act
        var result = await _backupService.ValidateBackupDataAsync("");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Backup data is empty or null", result.Errors);
    }

    [Fact]
    public async Task ValidateBackupDataAsync_WithInvalidJson_ReturnsInvalid()
    {
        // Act
        var result = await _backupService.ValidateBackupDataAsync("invalid json");

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Invalid JSON format"));
    }

    [Fact]
    public async Task ValidateBackupDataAsync_WithInvalidWorkout_ReturnsErrors()
    {
        // Arrange
        var backupData = new BackupData
        {
            Workouts = new List<Workout>
            {
                new Workout
                {
                    Id = 1,
                    Name = "", // Invalid - required field
                    DurationMinutes = 0, // Invalid - must be > 0
                    Difficulty = (DifficultyLevel)999 // Invalid enum value
                }
            },
            Actions = new List<WorkoutGamifier.Core.Models.Action>(),
            WorkoutPools = new List<WorkoutPool>(),
            Sessions = new List<Session>(),
            WorkoutPoolWorkouts = new List<WorkoutPoolWorkout>(),
            ActionCompletions = new List<ActionCompletion>(),
            WorkoutReceived = new List<WorkoutReceived>(),
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0"
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonData = JsonSerializer.Serialize(backupData, options);

        // Act
        var result = await _backupService.ValidateBackupDataAsync(jsonData);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("Workout at index 0"));
    }

    [Fact]
    public async Task ValidateBackupDataAsync_WithBrokenForeignKeys_ReturnsErrors()
    {
        // Arrange
        var backupData = new BackupData
        {
            Workouts = new List<Workout>(),
            Actions = new List<WorkoutGamifier.Core.Models.Action>(),
            WorkoutPools = new List<WorkoutPool>(),
            Sessions = new List<Session>
            {
                new Session
                {
                    Id = 1,
                    Name = "Test Session",
                    WorkoutPoolId = 999, // Non-existent pool
                    StartTime = DateTime.UtcNow,
                    Status = SessionStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            },
            WorkoutPoolWorkouts = new List<WorkoutPoolWorkout>(),
            ActionCompletions = new List<ActionCompletion>(),
            WorkoutReceived = new List<WorkoutReceived>(),
            ExportedAt = DateTime.UtcNow,
            AppVersion = "1.0.0"
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        var jsonData = JsonSerializer.Serialize(backupData, options);

        // Act
        var result = await _backupService.ValidateBackupDataAsync(jsonData);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Contains("references non-existent WorkoutPool ID: 999"));
    }

    [Fact]
    public async Task ImportDataAsync_WithValidData_ImportsSuccessfully()
    {
        // Arrange
        await SeedSampleDataAsync();
        var jsonData = await _backupService.ExportDataAsync();
        
        // Clear database
        await ClearDatabaseAsync();

        // Act
        var result = await _backupService.ImportDataAsync(jsonData, false);



        // Assert
        Assert.True(result.Success);
        Assert.Equal(1, result.WorkoutsImported);
        Assert.Equal(1, result.ActionsImported);
        Assert.Equal(1, result.WorkoutPoolsImported);
        Assert.Equal(1, result.SessionsImported);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public async Task ImportDataAsync_WithOverwriteTrue_ReplacesExistingData()
    {
        // Arrange
        await SeedSampleDataAsync();
        var originalJsonData = await _backupService.ExportDataAsync();
        
        // Modify some data
        var workout = await _unitOfWork.Workouts.GetByIdAsync(1);
        workout!.Name = "Modified Workout";
        await _unitOfWork.Workouts.UpdateAsync(workout);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var result = await _backupService.ImportDataAsync(originalJsonData, true);

        // Assert
        Assert.True(result.Success);
        
        // Verify data was restored
        var restoredWorkout = await _unitOfWork.Workouts.GetByIdAsync(1);
        Assert.Equal("Test Workout", restoredWorkout!.Name);
    }

    [Fact]
    public async Task ImportDataAsync_WithInvalidData_ReturnsFailure()
    {
        // Act
        var result = await _backupService.ImportDataAsync("invalid json", false);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Backup data validation failed", result.Message);
        Assert.NotEmpty(result.Errors);
    }

    [Fact]
    public async Task GetBackupFilePathAsync_ReturnsValidPath()
    {
        // Act
        var result = await _backupService.GetBackupFilePathAsync();

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Contains("WorkoutGamifier", result);
        Assert.Contains("Backups", result);
        Assert.EndsWith(".json", result);
    }

    [Fact]
    public async Task SaveBackupToFileAsync_CreatesFile()
    {
        // Arrange
        var testData = "test backup data";
        var tempPath = Path.GetTempFileName();
        
        try
        {
            // Act
            await _backupService.SaveBackupToFileAsync(testData, tempPath);

            // Assert
            Assert.True(File.Exists(tempPath));
            var content = await File.ReadAllTextAsync(tempPath);
            Assert.Equal(testData, content);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task LoadBackupFromFileAsync_ReadsFileContent()
    {
        // Arrange
        var testData = "test backup data";
        var tempPath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempPath, testData);
        
        try
        {
            // Act
            var result = await _backupService.LoadBackupFromFileAsync(tempPath);

            // Assert
            Assert.Equal(testData, result);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    [Fact]
    public async Task LoadBackupFromFileAsync_WithNonExistentFile_ThrowsException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _backupService.LoadBackupFromFileAsync("non-existent-file.json"));
    }

    private async Task SeedSampleDataAsync()
    {
        var workout = new Workout
        {
            Id = 1,
            Name = "Test Workout",
            Description = "Test Description",
            Instructions = "Test Instructions",
            DurationMinutes = 30,
            Difficulty = DifficultyLevel.Intermediate,
            IsPreloaded = false,
            IsHidden = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var action = new WorkoutGamifier.Core.Models.Action
        {
            Id = 1,
            Description = "Test Action",
            PointValue = 10,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var workoutPool = new WorkoutPool
        {
            Id = 1,
            Name = "Test Pool",
            Description = "Test Pool Description",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var session = new Session
        {
            Id = 1,
            Name = "Test Session",
            Description = "Test Session Description",
            WorkoutPoolId = 1,
            StartTime = DateTime.UtcNow.AddHours(-1),
            EndTime = DateTime.UtcNow,
            PointsEarned = 10,
            PointsSpent = 5,
            Status = SessionStatus.Completed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var workoutPoolWorkout = new WorkoutPoolWorkout
        {
            WorkoutPoolId = 1,
            WorkoutId = 1
        };

        var actionCompletion = new ActionCompletion
        {
            Id = 1,
            SessionId = 1,
            ActionId = 1,
            CompletedAt = DateTime.UtcNow,
            PointsAwarded = 10
        };

        var workoutReceived = new WorkoutReceived
        {
            Id = 1,
            SessionId = 1,
            WorkoutId = 1,
            ReceivedAt = DateTime.UtcNow,
            PointsSpent = 5
        };

        await _unitOfWork.Workouts.CreateAsync(workout);
        await _unitOfWork.Actions.CreateAsync(action);
        await _unitOfWork.WorkoutPools.CreateAsync(workoutPool);
        await _unitOfWork.Sessions.CreateAsync(session);
        await _unitOfWork.WorkoutPoolWorkouts.CreateAsync(workoutPoolWorkout);
        await _unitOfWork.ActionCompletions.CreateAsync(actionCompletion);
        await _unitOfWork.WorkoutReceived.CreateAsync(workoutReceived);
        
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task ClearDatabaseAsync()
    {
        var workoutReceived = await _unitOfWork.WorkoutReceived.GetAllAsync();
        foreach (var item in workoutReceived)
        {
            await _unitOfWork.WorkoutReceived.DeleteAsync(item.Id);
        }

        var actionCompletions = await _unitOfWork.ActionCompletions.GetAllAsync();
        foreach (var item in actionCompletions)
        {
            await _unitOfWork.ActionCompletions.DeleteAsync(item.Id);
        }

        var sessions = await _unitOfWork.Sessions.GetAllAsync();
        foreach (var item in sessions)
        {
            await _unitOfWork.Sessions.DeleteAsync(item.Id);
        }

        var workoutPoolWorkouts = await _unitOfWork.WorkoutPoolWorkouts.GetAllAsync();
        foreach (var item in workoutPoolWorkouts)
        {
            _context.Remove(item);
        }

        var workoutPools = await _unitOfWork.WorkoutPools.GetAllAsync();
        foreach (var item in workoutPools)
        {
            await _unitOfWork.WorkoutPools.DeleteAsync(item.Id);
        }

        var actions = await _unitOfWork.Actions.GetAllAsync();
        foreach (var item in actions)
        {
            await _unitOfWork.Actions.DeleteAsync(item.Id);
        }

        var workouts = await _unitOfWork.Workouts.GetAllAsync();
        foreach (var item in workouts)
        {
            await _unitOfWork.Workouts.DeleteAsync(item.Id);
        }

        await _unitOfWork.SaveChangesAsync();
    }
}
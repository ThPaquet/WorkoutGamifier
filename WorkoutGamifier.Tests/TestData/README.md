# Test Data Infrastructure

This directory contains a comprehensive test data infrastructure for the WorkoutGamifier application, designed to support all types of testing from unit tests to integration tests and performance testing.

## Overview

The test data infrastructure follows the Builder pattern and provides:

- **Fluent API builders** for creating test entities
- **Realistic test data scenarios** for different user personas
- **Database test fixtures** with automatic cleanup
- **Performance testing datasets** with large data volumes
- **Edge case scenarios** for boundary testing
- **Workflow scenarios** for integration testing

## Core Components

### TestDataBuilder

The main entry point for creating test data using fluent builder pattern:

```csharp
// Create a workout
var workout = TestDataBuilder.Workout()
    .WithName("Test Workout")
    .WithDifficulty(DifficultyLevel.Intermediate)
    .WithDuration(30)
    .Build();

// Create a session
var session = TestDataBuilder.Session()
    .WithName("Test Session")
    .WithWorkoutPool(pool)
    .AsActive()
    .WithPointBalance(50)
    .Build();

// Create an action
var action = TestDataBuilder.Action()
    .WithDescription("Complete 10 push-ups")
    .WithPointValue(10)
    .Build();
```

### Individual Builders

Each entity has its own builder class with specific methods:

#### WorkoutBuilder
- `WithName(string)` - Set workout name
- `WithDescription(string)` - Set description
- `WithDuration(int)` - Set duration in minutes
- `WithDifficulty(DifficultyLevel)` - Set difficulty level
- `AsHidden()` / `AsVisible()` - Set visibility
- Static factories: `Beginner()`, `Intermediate()`, `Advanced()`, `Quick()`, `Long()`, `Hidden()`

#### SessionBuilder
- `WithName(string)` - Set session name
- `WithWorkoutPool(WorkoutPool)` - Associate with pool
- `WithPointBalance(int)` - Set point balance
- `WithPointsEarned(int)` - Set points earned
- `WithPointsSpent(int)` - Set points spent
- `AsActive()` / `AsCompleted()` - Set status
- Static factories: `Active()`, `Completed()`, `WithPoints(int)`

#### WorkoutPoolBuilder
- `WithName(string)` - Set pool name
- `WithDescription(string)` - Set description
- Static factories: `Beginner()`, `Intermediate()`, `Advanced()`, `FullBody()`, `Cardio()`, `Strength()`

#### ActionBuilder
- `WithDescription(string)` - Set action description
- `WithPointValue(int)` - Set point value
- Static factories: `LowValue()`, `MediumValue()`, `HighValue()`, `WithPoints(int)`

### TestDataScenarios

Provides realistic test data scenarios for different testing needs:

#### User Persona Scenarios

**BeginnerUserScenario**
- Low-difficulty workouts (5-10 minutes)
- Low-value actions (5-8 points)
- Beginner-friendly workout pool
- Typical session with 25 points earned

**IntermediateUserScenario**
- Medium-difficulty workouts (8-20 minutes)
- Medium-value actions (10-15 points)
- Strength-building workout pool
- Typical session with 50 points earned

**AdvancedUserScenario**
- High-difficulty workouts (15+ minutes)
- High-value actions (18+ points)
- HIIT-focused workout pool
- Typical session with 75 points earned

#### Edge Case Scenarios

- **Minimal entities** - Smallest valid data
- **Maximal entities** - Largest valid data
- **Zero point sessions** - Boundary testing
- **Long-running sessions** - Time-based edge cases

#### Workflow Scenarios

**CompleteSessionWorkflow**
- Complete session from start to finish
- Multiple workouts and actions
- Expected completions and received workouts

**PoolManagementWorkflow**
- Empty and full workout pools
- Pool-workout relationships
- Difficulty distribution testing

#### Performance Scenarios

- **Large datasets** for performance testing
- **Configurable sizes** (100 workouts, 50 actions, 20 sessions)
- **Realistic data distribution**
- **Unique data generation**

### DatabaseTestFixture

Provides isolated database testing with automatic cleanup:

```csharp
using var fixture = new DatabaseTestFixture();

// Seed with minimal data
await fixture.SeedMinimalData();

// Seed with complete dataset
await fixture.SeedCompleteDataset();

// Seed specific user scenario
await fixture.SeedUserScenario(UserScenarioType.Beginner);

// Create workflow scenario
var scenario = await fixture.CreateCompleteWorkflowScenario();

// Verify data integrity
var integrity = await fixture.VerifyDataIntegrity();

// Measure performance
var metrics = await fixture.MeasurePerformance();

// Clean up
await fixture.CleanDatabase();
```

#### Key Features

- **Isolated contexts** - Each test gets fresh database
- **Automatic cleanup** - No test pollution
- **Transaction support** - Rollback on failure
- **Integrity verification** - Check foreign key constraints
- **Performance measurement** - CRUD operation timing
- **Multiple seeding options** - Different data scenarios

## Usage Patterns

### Unit Testing

```csharp
[Fact]
public void WorkoutService_ShouldCreateWorkout()
{
    // Arrange
    var workout = TestDataBuilder.Workout()
        .WithName("Test Workout")
        .WithDifficulty(DifficultyLevel.Beginner)
        .Build();

    // Act & Assert
    // ... test logic
}
```

### Integration Testing

```csharp
[Fact]
public async Task SessionWorkflow_ShouldCompleteSuccessfully()
{
    // Arrange
    using var fixture = new DatabaseTestFixture();
    var scenario = await fixture.CreateCompleteWorkflowScenario();

    // Act & Assert
    // ... test complete workflow
}
```

### Performance Testing

```csharp
[Fact]
public async Task DatabaseOperations_ShouldMeetPerformanceThresholds()
{
    // Arrange
    using var fixture = new DatabaseTestFixture();
    await fixture.SeedPerformanceData(1000, 500, 100);

    // Act
    var metrics = await fixture.MeasurePerformance();

    // Assert
    Assert.True(metrics.IsPerformant);
}
```

### Scenario Testing

```csharp
[Theory]
[InlineData(UserScenarioType.Beginner)]
[InlineData(UserScenarioType.Intermediate)]
[InlineData(UserScenarioType.Advanced)]
public async Task UserScenario_ShouldWorkCorrectly(UserScenarioType scenarioType)
{
    // Arrange
    using var fixture = new DatabaseTestFixture();
    await fixture.SeedUserScenario(scenarioType);

    // Act & Assert
    // ... test scenario-specific logic
}
```

## Data Quality

### Realistic Data
- **Meaningful names** - Actual workout and action names
- **Appropriate durations** - Realistic workout times
- **Balanced point values** - Fair reward system
- **Proper difficulty progression** - Beginner to advanced

### Data Consistency
- **Foreign key integrity** - All relationships valid
- **Timestamp consistency** - Proper created/updated times
- **Business rule compliance** - Valid state transitions
- **Referential integrity** - No orphaned records

### Performance Characteristics
- **Large dataset support** - Thousands of records
- **Efficient generation** - Fast test data creation
- **Memory efficient** - Minimal memory footprint
- **Cleanup optimization** - Fast database cleanup

## Testing Coverage

The test data infrastructure supports:

- **Unit tests** - Individual component testing
- **Integration tests** - Multi-component workflows
- **Performance tests** - Load and stress testing
- **Edge case tests** - Boundary condition testing
- **Regression tests** - Prevent functionality breaks
- **User scenario tests** - Real-world usage patterns

## Best Practices

### Test Isolation
- Always use `DatabaseTestFixture` for database tests
- Clean up after each test
- Use unique test data per test

### Data Realism
- Use scenario-based data for integration tests
- Prefer realistic data over minimal data
- Test with data that mirrors production

### Performance Considerations
- Use performance scenarios for load testing
- Measure and assert on performance metrics
- Test with realistic data volumes

### Maintainability
- Use builders for consistent data creation
- Leverage static factories for common scenarios
- Document custom test data requirements

## Extension Points

The infrastructure is designed to be extensible:

### Adding New Builders
1. Create new builder class following the pattern
2. Add factory method to `TestDataBuilder`
3. Add static factory methods for common scenarios
4. Write tests for the new builder

### Adding New Scenarios
1. Add scenario methods to `TestDataScenarios`
2. Create corresponding test methods
3. Update `DatabaseTestFixture` if needed
4. Document the new scenario

### Adding New Fixtures
1. Extend `DatabaseTestFixture` with new methods
2. Add corresponding result classes if needed
3. Write tests for new functionality
4. Update documentation

This comprehensive test data infrastructure ensures that all testing needs are met with realistic, consistent, and maintainable test data.
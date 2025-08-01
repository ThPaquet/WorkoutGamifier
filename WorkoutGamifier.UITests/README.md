# WorkoutGamifier UI Tests

This project contains automated UI tests for the WorkoutGamifier MAUI application using Appium WebDriver.

## Prerequisites

### 1. Install Appium
```bash
npm install -g appium
npm install -g @appium/doctor
appium driver install uiautomator2
```

### 2. Verify Appium Setup
```bash
appium-doctor --android
```

### 3. Android Setup
- Install Android SDK and Android Studio
- Set up Android emulator or connect physical device
- Enable Developer Options and USB Debugging on device

## Configuration

### Test Configuration
Edit `appsettings.json` to configure test settings:

```json
{
  "TestConfiguration": {
    "AppPath": "path/to/your/app.apk",
    "DeviceName": "Android Emulator",
    "PlatformVersion": "11.0",
    "AppPackage": "com.workoutgamifier.app"
  }
}
```

### Environment-Specific Configuration
Create `appsettings.Development.json` or `appsettings.Production.json` for environment-specific settings.

### Environment Variables
You can override configuration using environment variables with the `UITEST_` prefix:
- `UITEST_AppPath`
- `UITEST_DeviceName`
- `UITEST_PlatformVersion`

## Running Tests

### 1. Start Appium Server
```bash
appium
```

### 2. Start Android Emulator or Connect Device
```bash
# List available emulators
emulator -list-avds

# Start emulator
emulator -avd YourEmulatorName
```

### 3. Build and Install App
```bash
# Build the app
dotnet build WorkoutGamifier -c Release -f net9.0-android

# The APK will be in: WorkoutGamifier/bin/Release/net9.0-android/
```

### 4. Run UI Tests
```bash
# Run all UI tests
dotnet test WorkoutGamifier.UITests

# Run specific test class
dotnet test WorkoutGamifier.UITests --filter "ClassName=SmokeTests"

# Run with verbose output
dotnet test WorkoutGamifier.UITests --logger "console;verbosity=detailed"
```

## Test Structure

### Base Classes
- `AppiumTestBase`: Base class for all UI tests with common functionality
- `PageObjectBase`: Abstract base class for all page object models

### Page Object Models
- `SessionsPageObject`: Handles interactions with the Sessions page
- `WorkoutPoolsPageObject`: Handles interactions with the Workout Pools page
- `ActionsPageObject`: Handles interactions with the Actions page
- `ProfilePageObject`: Handles interactions with the Profile page
- `NavigationHelper`: Manages navigation between pages and app initialization

### Utilities
- `ScreenshotManager`: Handles screenshot capture and management
- `TestDataHelper`: Provides test data generation and validation

### Test Categories
- `SmokeTests`: Basic functionality and app launch tests
- `PageObjectTests`: Tests demonstrating Page Object Model usage
- `SessionWorkflowTests`: Complete session management workflows
- `PoolManagementWorkflowTests`: Pool creation, editing, and management workflows
- `NavigationWorkflowTests`: Tab navigation and deep navigation workflows
- More test categories will be added as the framework expands

## Screenshots

Screenshots are automatically captured:
- On test failures (configurable)
- On test success (configurable)
- Manually using `TakeScreenshot()` method

Screenshots are stored in the `Screenshots` directory with timestamps.

## Troubleshooting

### Common Issues

1. **Appium server not running**
   - Start Appium server: `appium`
   - Check if running on correct port (default: 4723)

2. **Device not found**
   - Check device connection: `adb devices`
   - Verify emulator is running
   - Check device name in configuration

3. **App not installed**
   - Verify APK path in configuration
   - Check if app package name is correct
   - Ensure app is compatible with device/emulator

4. **Element not found**
   - Check if app has fully loaded
   - Verify element locators are correct
   - Use screenshot debugging to see current state

### Debug Tips

1. **Enable verbose logging**
   ```bash
   dotnet test WorkoutGamifier.UITests --logger "console;verbosity=detailed"
   ```

2. **Take screenshots for debugging**
   ```csharp
   await TakeScreenshot("debug-step-name");
   ```

3. **Check Appium logs**
   - Appium server logs show detailed interaction information
   - Use `--log-level debug` when starting Appium

4. **Inspect app elements**
   - Use Appium Inspector to examine app elements
   - Install: `npm install -g appium-inspector`

## Page Object Model

The framework uses the Page Object Model pattern for maintainable and reusable UI tests:

### Using Page Objects
```csharp
[Fact]
public async Task Example_UsingPageObjects()
{
    // Initialize navigation helper
    var navigation = new NavigationHelper(Driver, Config);
    await navigation.HandleAppInitialization();
    
    // Navigate to Sessions page
    var sessionsPage = await navigation.NavigateToSessions();
    
    // Interact with the page
    var sessionCount = await sessionsPage.GetSessionCount();
    var isEmpty = await sessionsPage.IsEmptyState();
    
    // Perform actions
    if (!isEmpty)
    {
        await sessionsPage.ViewSessionDetails("My Session");
    }
}
```

### Page Object Features
- **Consistent Interface**: All page objects extend `PageObjectBase`
- **Element Locators**: Centralized element location strategies
- **Action Methods**: High-level methods for user interactions
- **Validation Methods**: Built-in verification capabilities
- **Wait Strategies**: Automatic waiting for elements and page loads

### Navigation Helper
The `NavigationHelper` class provides:
- Tab navigation methods
- Deep navigation to specific pages
- App initialization handling
- Navigation verification utilities

## Critical User Workflow Tests

The framework includes comprehensive workflow tests covering:

### Session Management Workflows
- **Complete Session Workflow**: Create session → Complete actions → End session
- **Multiple Session Handling**: Verify prevention of multiple active sessions
- **Session with Pool Workflow**: Create pool → Start session → Complete workouts
- **Action Completion Workflow**: Complete multiple actions and earn points
- **Profile Statistics Workflow**: Verify statistics update after actions
- **End-to-End Workflow**: Complete user journey across all features
- **Navigation Stability**: Extensive navigation testing for UI stability

### Pool Management Workflows
- **Pool Creation Workflow**: Create pool → Add workouts → Verify creation
- **Pool Management Workflow**: View → Edit → Delete pool operations
- **Pool Search Workflow**: Search and filter pools functionality
- **Pool Workout Management**: Add and remove workouts from pools
- **Pool Validation Workflow**: Verify pool constraints and validation
- **Pool Ordering Workflow**: Verify consistent pool display order

### Navigation Workflows
- **Tab Navigation Workflow**: Navigate between all main tabs
- **Deep Navigation Workflow**: Navigate to specific pages within app
- **Back Navigation Workflow**: Verify back button behavior consistency
- **Navigation State Workflow**: Verify navigation state maintenance
- **Navigation Performance Workflow**: Measure navigation speed
- **Navigation Recovery Workflow**: Recover from navigation errors
- **Navigation Consistency Workflow**: Verify consistent navigation behavior

### Running Workflow Tests
```bash
# Run all workflow tests
dotnet test WorkoutGamifier.UITests --filter "Category=Workflow"

# Run specific workflow test class
dotnet test WorkoutGamifier.UITests --filter "ClassName=SessionWorkflowTests"

# Run with detailed output
dotnet test WorkoutGamifier.UITests --logger "console;verbosity=detailed" --filter "SessionWorkflow"
```

## Next Steps

Critical user workflow tests are now complete. Future enhancements will include:
- Performance testing integration
- Visual regression testing
- CI/CD pipeline integration
- Accessibility testing framework

## Contributing

When adding new tests:
1. Extend `AppiumTestBase` for test classes
2. Use the `TestDataHelper` for generating test data
3. Take screenshots at key points for debugging
4. Follow the existing naming conventions
5. Add appropriate assertions and error handling
#!/bin/bash

# Bash script to run UI tests with proper setup

# Default values
TEST_FILTER=""
CONFIGURATION="Debug"
APP_PATH=""
START_APPIUM=false
START_EMULATOR=false
EMULATOR_NAME="Pixel_5_API_30"
BUILD_APP=false
INSTALL_APP=false
CLEAN_SCREENSHOTS=false

# Parse command line arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --test-filter)
            TEST_FILTER="$2"
            shift 2
            ;;
        --configuration)
            CONFIGURATION="$2"
            shift 2
            ;;
        --app-path)
            APP_PATH="$2"
            shift 2
            ;;
        --start-appium)
            START_APPIUM=true
            shift
            ;;
        --start-emulator)
            START_EMULATOR=true
            shift
            ;;
        --emulator-name)
            EMULATOR_NAME="$2"
            shift 2
            ;;
        --build-app)
            BUILD_APP=true
            shift
            ;;
        --install-app)
            INSTALL_APP=true
            shift
            ;;
        --clean-screenshots)
            CLEAN_SCREENSHOTS=true
            shift
            ;;
        --help)
            echo "Usage: $0 [OPTIONS]"
            echo "Options:"
            echo "  --test-filter FILTER      Run tests matching the filter"
            echo "  --configuration CONFIG    Build configuration (Debug/Release)"
            echo "  --app-path PATH          Path to APK file"
            echo "  --start-appium           Start Appium server"
            echo "  --start-emulator         Start Android emulator"
            echo "  --emulator-name NAME     Emulator AVD name"
            echo "  --build-app              Build the app before testing"
            echo "  --install-app            Install the app before testing"
            echo "  --clean-screenshots      Clean old screenshots"
            echo "  --help                   Show this help message"
            exit 0
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

echo -e "\033[32mWorkoutGamifier UI Test Runner\033[0m"
echo -e "\033[32m================================\033[0m"

# Function to check if a process is running
is_process_running() {
    pgrep -f "$1" > /dev/null 2>&1
}

# Function to wait for a process to start
wait_for_process() {
    local process_name="$1"
    local timeout_seconds="${2:-30}"
    local end_time=$(($(date +%s) + timeout_seconds))
    
    while [ $(date +%s) -lt $end_time ]; do
        if is_process_running "$process_name"; then
            return 0
        fi
        sleep 2
        echo -e "\033[33mWaiting for $process_name to start...\033[0m"
    done
    return 1
}

# Clean screenshots if requested
if [ "$CLEAN_SCREENSHOTS" = true ]; then
    echo -e "\033[33mCleaning old screenshots...\033[0m"
    SCREENSHOT_PATH="$(dirname "$0")/../Screenshots"
    if [ -d "$SCREENSHOT_PATH" ]; then
        rm -f "$SCREENSHOT_PATH"/*.png
        echo -e "\033[32mScreenshots cleaned.\033[0m"
    fi
fi

# Build app if requested
if [ "$BUILD_APP" = true ]; then
    echo -e "\033[33mBuilding WorkoutGamifier app...\033[0m"
    if ! dotnet build ../WorkoutGamifier -c "$CONFIGURATION" -f net9.0-android; then
        echo -e "\033[31mApp build failed\033[0m"
        exit 1
    fi
    echo -e "\033[32mApp built successfully.\033[0m"
    
    # Set app path if not provided
    if [ -z "$APP_PATH" ]; then
        APP_PATH="../WorkoutGamifier/bin/$CONFIGURATION/net9.0-android/com.workoutgamifier.app-Signed.apk"
    fi
fi

# Start emulator if requested
if [ "$START_EMULATOR" = true ]; then
    echo -e "\033[33mStarting Android emulator: $EMULATOR_NAME...\033[0m"
    
    # Check if emulator is already running
    if ! adb devices | grep -q "emulator.*device"; then
        emulator -avd "$EMULATOR_NAME" &
        
        # Wait for emulator to start
        EMULATOR_STARTED=false
        END_TIME=$(($(date +%s) + 300)) # 5 minutes timeout
        
        while [ $(date +%s) -lt $END_TIME ]; do
            if adb devices | grep -q "emulator.*device"; then
                EMULATOR_STARTED=true
                break
            fi
            sleep 5
            echo -e "\033[33mWaiting for emulator to boot...\033[0m"
        done
        
        if [ "$EMULATOR_STARTED" = false ]; then
            echo -e "\033[31mEmulator failed to start within timeout\033[0m"
            exit 1
        fi
        echo -e "\033[32mEmulator started successfully.\033[0m"
    else
        echo -e "\033[32mEmulator already running.\033[0m"
    fi
fi

# Start Appium server if requested
if [ "$START_APPIUM" = true ]; then
    echo -e "\033[33mStarting Appium server...\033[0m"
    
    if ! is_process_running "appium"; then
        appium &
        
        if ! wait_for_process "appium" 30; then
            echo -e "\033[31mAppium server failed to start\033[0m"
            exit 1
        fi
        echo -e "\033[32mAppium server started successfully.\033[0m"
    else
        echo -e "\033[32mAppium server already running.\033[0m"
    fi
fi

# Install app if requested
if [ "$INSTALL_APP" = true ] && [ -n "$APP_PATH" ]; then
    echo -e "\033[33mInstalling app: $APP_PATH...\033[0m"
    
    if [ ! -f "$APP_PATH" ]; then
        echo -e "\033[31mApp file not found: $APP_PATH\033[0m"
        exit 1
    fi
    
    if ! adb install -r "$APP_PATH"; then
        echo -e "\033[31mApp installation failed\033[0m"
        exit 1
    fi
    echo -e "\033[32mApp installed successfully.\033[0m"
fi

# Set environment variable for app path if provided
if [ -n "$APP_PATH" ]; then
    export UITEST_AppPath="$APP_PATH"
    echo -e "\033[36mUsing app path: $APP_PATH\033[0m"
fi

# Build test command
TEST_COMMAND="dotnet test WorkoutGamifier.UITests --configuration $CONFIGURATION --logger \"console;verbosity=detailed\""

if [ -n "$TEST_FILTER" ]; then
    TEST_COMMAND="$TEST_COMMAND --filter \"$TEST_FILTER\""
    echo -e "\033[36mRunning tests with filter: $TEST_FILTER\033[0m"
else
    echo -e "\033[36mRunning all UI tests...\033[0m"
fi

# Run tests
echo -e "\033[37mExecuting: $TEST_COMMAND\033[0m"
eval $TEST_COMMAND

if [ $? -eq 0 ]; then
    echo -e "\033[32mAll tests completed successfully!\033[0m"
else
    echo -e "\033[31mSome tests failed. Check the output above for details.\033[0m"
    exit 1
fi

echo -e "\033[32mTest run completed.\033[0m"
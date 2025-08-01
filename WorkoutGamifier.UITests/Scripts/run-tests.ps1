# PowerShell script to run UI tests with proper setup

param(
    [string]$TestFilter = "",
    [string]$Configuration = "Debug",
    [string]$AppPath = "",
    [switch]$StartAppium = $false,
    [switch]$StartEmulator = $false,
    [string]$EmulatorName = "Pixel_5_API_30",
    [switch]$BuildApp = $false,
    [switch]$InstallApp = $false,
    [switch]$CleanScreenshots = $false
)

Write-Host "WorkoutGamifier UI Test Runner" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green

# Function to check if a process is running
function Test-ProcessRunning {
    param([string]$ProcessName)
    return (Get-Process -Name $ProcessName -ErrorAction SilentlyContinue) -ne $null
}

# Function to wait for a process to start
function Wait-ForProcess {
    param([string]$ProcessName, [int]$TimeoutSeconds = 30)
    
    $timeout = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $timeout) {
        if (Test-ProcessRunning $ProcessName) {
            return $true
        }
        Start-Sleep -Seconds 2
        Write-Host "Waiting for $ProcessName to start..." -ForegroundColor Yellow
    }
    return $false
}

try {
    # Clean screenshots if requested
    if ($CleanScreenshots) {
        Write-Host "Cleaning old screenshots..." -ForegroundColor Yellow
        $screenshotPath = Join-Path $PSScriptRoot "..\Screenshots"
        if (Test-Path $screenshotPath) {
            Remove-Item "$screenshotPath\*.png" -Force
            Write-Host "Screenshots cleaned." -ForegroundColor Green
        }
    }

    # Build app if requested
    if ($BuildApp) {
        Write-Host "Building WorkoutGamifier app..." -ForegroundColor Yellow
        $buildResult = dotnet build ..\WorkoutGamifier -c $Configuration -f net9.0-android
        if ($LASTEXITCODE -ne 0) {
            throw "App build failed"
        }
        Write-Host "App built successfully." -ForegroundColor Green
        
        # Set app path if not provided
        if ([string]::IsNullOrEmpty($AppPath)) {
            $AppPath = "..\WorkoutGamifier\bin\$Configuration\net9.0-android\com.workoutgamifier.app-Signed.apk"
        }
    }

    # Start emulator if requested
    if ($StartEmulator) {
        Write-Host "Starting Android emulator: $EmulatorName..." -ForegroundColor Yellow
        
        # Check if emulator is already running
        $runningEmulators = & adb devices | Select-String "emulator"
        if ($runningEmulators.Count -eq 0) {
            Start-Process -FilePath "emulator" -ArgumentList "-avd", $EmulatorName -WindowStyle Hidden
            
            # Wait for emulator to start
            $emulatorStarted = $false
            $timeout = (Get-Date).AddMinutes(5)
            while ((Get-Date) -lt $timeout) {
                $devices = & adb devices | Select-String "emulator.*device$"
                if ($devices.Count -gt 0) {
                    $emulatorStarted = $true
                    break
                }
                Start-Sleep -Seconds 5
                Write-Host "Waiting for emulator to boot..." -ForegroundColor Yellow
            }
            
            if (-not $emulatorStarted) {
                throw "Emulator failed to start within timeout"
            }
            Write-Host "Emulator started successfully." -ForegroundColor Green
        } else {
            Write-Host "Emulator already running." -ForegroundColor Green
        }
    }

    # Start Appium server if requested
    if ($StartAppium) {
        Write-Host "Starting Appium server..." -ForegroundColor Yellow
        
        if (-not (Test-ProcessRunning "node")) {
            Start-Process -FilePath "appium" -WindowStyle Hidden
            
            if (-not (Wait-ForProcess "node" 30)) {
                throw "Appium server failed to start"
            }
            Write-Host "Appium server started successfully." -ForegroundColor Green
        } else {
            Write-Host "Appium server already running." -ForegroundColor Green
        }
    }

    # Install app if requested
    if ($InstallApp -and -not [string]::IsNullOrEmpty($AppPath)) {
        Write-Host "Installing app: $AppPath..." -ForegroundColor Yellow
        
        if (-not (Test-Path $AppPath)) {
            throw "App file not found: $AppPath"
        }
        
        & adb install -r $AppPath
        if ($LASTEXITCODE -ne 0) {
            throw "App installation failed"
        }
        Write-Host "App installed successfully." -ForegroundColor Green
    }

    # Set environment variable for app path if provided
    if (-not [string]::IsNullOrEmpty($AppPath)) {
        $env:UITEST_AppPath = $AppPath
        Write-Host "Using app path: $AppPath" -ForegroundColor Cyan
    }

    # Build test command
    $testCommand = "dotnet test WorkoutGamifier.UITests --configuration $Configuration --logger `"console;verbosity=detailed`""
    
    if (-not [string]::IsNullOrEmpty($TestFilter)) {
        $testCommand += " --filter `"$TestFilter`""
        Write-Host "Running tests with filter: $TestFilter" -ForegroundColor Cyan
    } else {
        Write-Host "Running all UI tests..." -ForegroundColor Cyan
    }

    # Run tests
    Write-Host "Executing: $testCommand" -ForegroundColor Gray
    Invoke-Expression $testCommand
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "All tests completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Some tests failed. Check the output above for details." -ForegroundColor Red
    }

} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "Test run completed." -ForegroundColor Green
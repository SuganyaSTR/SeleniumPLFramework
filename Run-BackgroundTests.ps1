# Background Test Execution Script
# This script runs tests in the background with minimal resource usage

param(
    [string]$TestFilter = "Category=Smoke",
    [string]$LogLevel = "Error",
    [switch]$Headless = $true,
    [string]$Priority = "BelowNormal"
)

Write-Host "🌙 Starting Background Test Execution..." -ForegroundColor Blue
Write-Host "Filter: $TestFilter | Priority: $Priority | Headless: $Headless" -ForegroundColor Yellow

# Set low priority for the process
$currentProcess = Get-Process -Id $PID
$currentProcess.PriorityClass = $Priority

# Set environment variables for minimal resource usage
$env:ASPNETCORE_ENVIRONMENT = "fastexecution"
$env:WEBDRIVER_HEADLESS = $Headless.ToString().ToLower()
$env:WEBDRIVER_BROWSER = "chrome"

# Build with minimal output
Write-Host "🔨 Building (minimal output)..." -ForegroundColor Blue
dotnet build SeleniumPLFramework.sln --configuration Release --verbosity quiet --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Create background job for test execution
Write-Host "🚀 Starting tests in background..." -ForegroundColor Blue

$jobScript = {
    param($TestFilter, $LogLevel)
    
    Set-Location $using:PWD
    
    $testCommand = @(
        "dotnet test"
        "SeleniumPL.Tests/SeleniumPL.Tests.csproj"
        "--configuration Release"
        "--logger trx"
        "--logger console;verbosity=$LogLevel"
        "--settings nunit.runsettings"
        "--filter `"$TestFilter`""
        "--results-directory TestResults"
        "--nologo"
    )
    
    $commandString = $testCommand -join " "
    Invoke-Expression $commandString
    
    return @{
        ExitCode = $LASTEXITCODE
        Timestamp = Get-Date
        Filter = $TestFilter
    }
}

$job = Start-Job -ScriptBlock $jobScript -ArgumentList $TestFilter, $LogLevel

Write-Host "✅ Tests running in background (Job ID: $($job.Id))" -ForegroundColor Green
Write-Host "Use the following commands to monitor:" -ForegroundColor Yellow
Write-Host "  Get-Job $($job.Id)                    # Check job status" -ForegroundColor Gray
Write-Host "  Receive-Job $($job.Id) -Keep          # View job output" -ForegroundColor Gray
Write-Host "  Receive-Job $($job.Id) -Wait          # Wait for completion" -ForegroundColor Gray
Write-Host "  Stop-Job $($job.Id)                   # Stop if needed" -ForegroundColor Gray

# Optional: Wait for completion with minimal impact
$waitChoice = Read-Host "`nWait for completion? (y/N)"
if ($waitChoice -eq 'y' -or $waitChoice -eq 'Y') {
    Write-Host "⏳ Waiting for tests to complete..." -ForegroundColor Blue
    $result = Receive-Job $job -Wait
    
    if ($result.ExitCode -eq 0) {
        Write-Host "✅ All tests completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Some tests failed. Check TestResults folder." -ForegroundColor Red
    }
    
    Remove-Job $job
} else {
    Write-Host "ℹ️ Tests continue running in background. Check job status later." -ForegroundColor Cyan
}

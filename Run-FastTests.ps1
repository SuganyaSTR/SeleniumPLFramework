# Fast Parallel Test Execution Script
# This script runs tests in parallel with optimized settings

param(
    [string]$TestCategory = "Smoke",
    [int]$MaxParallel = 3,
    [string]$Browser = "chrome",
    [switch]$Headless = $true
)

Write-Host "🚀 Starting Fast Parallel Test Execution..." -ForegroundColor Green
Write-Host "Category: $TestCategory | Max Parallel: $MaxParallel | Browser: $Browser | Headless: $Headless" -ForegroundColor Yellow

# Set environment variables for fast execution
$env:ASPNETCORE_ENVIRONMENT = "fastexecution"
$env:WEBDRIVER_HEADLESS = $Headless.ToString().ToLower()
$env:WEBDRIVER_BROWSER = $Browser

# Build the solution first
Write-Host "🔨 Building solution..." -ForegroundColor Blue
dotnet build SeleniumPLFramework.sln --configuration Release --verbosity minimal

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build failed!" -ForegroundColor Red
    exit 1
}

# Run tests with parallel execution
Write-Host "🧪 Running tests in parallel..." -ForegroundColor Blue

$testCommand = @(
    "dotnet test"
    "SeleniumPL.Tests/SeleniumPL.Tests.csproj"
    "--configuration Release"
    "--logger trx"
    "--logger console;verbosity=minimal"
    "--settings nunit.runsettings"
    "--filter Category=$TestCategory"
    "--collect:`"XPlat Code Coverage`""
    "--results-directory TestResults"
    "-- NUnit.NumberOfTestWorkers=$MaxParallel"
    "NUnit.ParallelScope=Fixtures"
)

$commandString = $testCommand -join " "
Write-Host "Executing: $commandString" -ForegroundColor Gray

Invoke-Expression $commandString

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ All tests passed!" -ForegroundColor Green
} else {
    Write-Host "❌ Some tests failed. Check the results." -ForegroundColor Red
}

# Display execution summary
Write-Host "`n📊 Execution Summary:" -ForegroundColor Cyan
Write-Host "Results saved in: TestResults/" -ForegroundColor Gray
Write-Host "Use 'Open-TestResults' to view detailed results" -ForegroundColor Gray

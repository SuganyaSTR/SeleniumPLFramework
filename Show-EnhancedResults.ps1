# Enhanced Test Results Demonstration Script
# This script demonstrates the new comprehensive test result capture system

Write-Host "Enhanced Test Results Capture System Demo" -ForegroundColor Green
Write-Host ("=" * 60) -ForegroundColor Gray

# Show current directory structure
Write-Host "`nCurrent Output Directory Structure:" -ForegroundColor Cyan
Get-ChildItem -Path "." -Directory | Where-Object { $_.Name -match "(TestResults|Reports|Screenshots|Logs)" } | ForEach-Object {
    Write-Host "  $($_.Name)/" -ForegroundColor Yellow
    
    # Show recent files in each directory
    $recentFiles = Get-ChildItem -Path $_.FullName | Sort-Object LastWriteTime -Descending | Select-Object -First 3
    foreach ($file in $recentFiles) {
        $timeAgo = [math]::Round((Get-Date).Subtract($file.LastWriteTime).TotalMinutes, 1)
        Write-Host "    $($file.Name) ($timeAgo min ago)" -ForegroundColor Gray
    }
}

Write-Host "`nWhat's New in Enhanced Results Capture:" -ForegroundColor Cyan
Write-Host "  Detailed step-by-step execution tracking" -ForegroundColor Green
Write-Host "  JSON and TXT format detailed reports" -ForegroundColor Green
Write-Host "  HTML dashboard with interactive results" -ForegroundColor Green
Write-Host "  Performance metrics capture" -ForegroundColor Green
Write-Host "  Comprehensive failure diagnostics" -ForegroundColor Green
Write-Host "  Browser logs and page source capture" -ForegroundColor Green
Write-Host "  Environment information logging" -ForegroundColor Green

Write-Host "`nRecent Test Results:" -ForegroundColor Cyan
if (Test-Path "TestResults") {
    $recentResults = Get-ChildItem "TestResults" -Filter "*.trx" | Sort-Object LastWriteTime -Descending | Select-Object -First 3
    if ($recentResults) {
        foreach ($result in $recentResults) {
            Write-Host "  $($result.Name) - $($result.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
        }
    } else {
        Write-Host "  No recent test results found" -ForegroundColor Yellow
    }
} else {
    Write-Host "  TestResults directory not found" -ForegroundColor Yellow
}

Write-Host "`nRecent Reports:" -ForegroundColor Cyan
if (Test-Path "Reports") {
    $recentReports = Get-ChildItem "Reports" -Filter "*.html" | Sort-Object LastWriteTime -Descending | Select-Object -First 3
    if ($recentReports) {
        foreach ($report in $recentReports) {
            Write-Host "  $($report.Name) - $($report.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
        }
    } else {
        Write-Host "  No HTML reports found yet" -ForegroundColor Yellow
    }
} else {
    Write-Host "  Reports directory not found" -ForegroundColor Yellow
}

Write-Host "`nRecent Screenshots:" -ForegroundColor Cyan
if (Test-Path "Screenshots") {
    $recentScreenshots = Get-ChildItem "Screenshots" -Filter "*.png" | Sort-Object LastWriteTime -Descending | Select-Object -First 3
    if ($recentScreenshots) {
        foreach ($screenshot in $recentScreenshots) {
            Write-Host "  $($screenshot.Name) - $($screenshot.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
        }
    } else {
        Write-Host "  No recent screenshots found" -ForegroundColor Yellow
    }
} else {
    Write-Host "  Screenshots directory not found" -ForegroundColor Yellow
}

Write-Host "`nRecent Logs:" -ForegroundColor Cyan
if (Test-Path "Logs") {
    $recentLogs = Get-ChildItem "Logs" -Filter "*.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 3
    if ($recentLogs) {
        foreach ($log in $recentLogs) {
            Write-Host "  $($log.Name) - $($log.LastWriteTime.ToString('yyyy-MM-dd HH:mm:ss'))" -ForegroundColor White
        }
    } else {
        Write-Host "  No recent logs found" -ForegroundColor Yellow
    }
} else {
    Write-Host "  Logs directory not found" -ForegroundColor Yellow
}

Write-Host "`nHow to Run Tests with Enhanced Reporting:" -ForegroundColor Cyan
Write-Host "  1. Standard test run:" -ForegroundColor White
Write-Host "     dotnet test SeleniumPL.Tests/SeleniumPL.Tests.csproj" -ForegroundColor Gray
Write-Host ""
Write-Host "  2. Run specific category:" -ForegroundColor White
Write-Host "     dotnet test --filter Category=PracticeArea" -ForegroundColor Gray
Write-Host ""
Write-Host "  3. Use provided scripts:" -ForegroundColor White
Write-Host "     .\Run-FastTests.ps1 -TestCategory 'PracticeArea'" -ForegroundColor Gray
Write-Host ""

Write-Host "Output Files Explained:" -ForegroundColor Cyan
Write-Host "  TestResults/" -ForegroundColor Yellow
Write-Host "     - Individual test JSON/TXT reports with complete execution details" -ForegroundColor Gray
Write-Host "  Reports/" -ForegroundColor Yellow  
Write-Host "     - HTML dashboard and session summaries" -ForegroundColor Gray
Write-Host "  Screenshots/" -ForegroundColor Yellow
Write-Host "     - Failure screenshots, page sources, browser logs" -ForegroundColor Gray
Write-Host "  Logs/" -ForegroundColor Yellow
Write-Host "     - Detailed execution logs with timestamps" -ForegroundColor Gray

Write-Host "`nKey Benefits:" -ForegroundColor Cyan
Write-Host "  Complete visibility into test execution" -ForegroundColor Green
Write-Host "  Faster debugging with comprehensive failure data" -ForegroundColor Green
Write-Host "  Professional reporting for stakeholders" -ForegroundColor Green
Write-Host "  Performance tracking and metrics" -ForegroundColor Green
Write-Host "  CI/CD integration ready with JSON outputs" -ForegroundColor Green

Write-Host "`nThe system is now active! Run any test to see the enhanced results capture in action." -ForegroundColor Green
Write-Host ("=" * 60) -ForegroundColor Gray

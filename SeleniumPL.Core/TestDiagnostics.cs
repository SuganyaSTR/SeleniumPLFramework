using OpenQA.Selenium;
using Serilog;
using System;
using System.IO;
using System.Text;

namespace SeleniumPL.Core
{
    /// <summary>
    /// Enhanced test diagnostics and reporting capabilities
    /// </summary>
    public class TestDiagnostics
    {
        private readonly IWebDriver _driver;
        private readonly ILogger _logger;
        private readonly string _screenshotPath;

        public TestDiagnostics(IWebDriver driver, ILogger logger, string screenshotPath = "Screenshots")
        {
            _driver = driver;
            _logger = logger;
            _screenshotPath = screenshotPath;
            
            // Ensure screenshot directory exists
            Directory.CreateDirectory(_screenshotPath);
        }

        /// <summary>
        /// Capture enhanced screenshot with timestamp and test context
        /// </summary>
        public string CaptureScreenshot(string testName, string stepDescription = "", bool onFailure = false)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var status = onFailure ? "FAILED" : "SUCCESS";
                var filename = $"{testName}_{status}_{timestamp}.png";
                var fullPath = Path.Combine(_screenshotPath, filename);

                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                screenshot.SaveAsFile(fullPath);

                _logger.Information("Screenshot captured: {Path} - {Description}", fullPath, stepDescription);
                return fullPath;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to capture screenshot: {Error}", ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Capture page source for debugging
        /// </summary>
        public string CapturePageSource(string testName)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"{testName}_pagesource_{timestamp}.html";
                var fullPath = Path.Combine(_screenshotPath, filename);

                var pageSource = _driver.PageSource;
                File.WriteAllText(fullPath, pageSource, Encoding.UTF8);

                _logger.Information("Page source captured: {Path}", fullPath);
                return fullPath;
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to capture page source: {Error}", ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Capture browser console logs
        /// </summary>
        public void CaptureBrowserLogs(string testName)
        {
            try
            {
                var logs = _driver.Manage().Logs.GetLog(LogType.Browser);
                if (logs.Count > 0)
                {
                    var logBuilder = new StringBuilder();
                    logBuilder.AppendLine($"Browser Console Logs for test: {testName}");
                    logBuilder.AppendLine($"Captured at: {DateTime.Now}");
                    logBuilder.AppendLine(new string('=', 50));

                    foreach (var logEntry in logs)
                    {
                        logBuilder.AppendLine($"[{logEntry.Timestamp}] {logEntry.Level}: {logEntry.Message}");
                    }

                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var filename = $"{testName}_browserlogs_{timestamp}.txt";
                    var fullPath = Path.Combine(_screenshotPath, filename);

                    File.WriteAllText(fullPath, logBuilder.ToString(), Encoding.UTF8);
                    _logger.Information("Browser logs captured: {Path}", fullPath);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to capture browser logs: {Error}", ex.Message);
            }
        }

        /// <summary>
        /// Capture comprehensive test environment information
        /// </summary>
        public void CaptureEnvironmentInfo(string testName)
        {
            try
            {
                var envInfo = new StringBuilder();
                envInfo.AppendLine($"Test Environment Information for: {testName}");
                envInfo.AppendLine($"Timestamp: {DateTime.Now}");
                envInfo.AppendLine(new string('=', 50));
                
                // Browser information
                envInfo.AppendLine($"Browser: {((IJavaScriptExecutor)_driver).ExecuteScript("return navigator.userAgent;")}");
                envInfo.AppendLine($"Window Size: {_driver.Manage().Window.Size}");
                envInfo.AppendLine($"Current URL: {_driver.Url}");
                envInfo.AppendLine($"Page Title: {_driver.Title}");
                
                // System information
                envInfo.AppendLine($"OS: {Environment.OSVersion}");
                envInfo.AppendLine($"Machine Name: {Environment.MachineName}");
                envInfo.AppendLine($"User: {Environment.UserName}");
                envInfo.AppendLine($"Working Directory: {Environment.CurrentDirectory}");
                envInfo.AppendLine($"CLR Version: {Environment.Version}");

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var filename = $"{testName}_envinfo_{timestamp}.txt";
                var fullPath = Path.Combine(_screenshotPath, filename);

                File.WriteAllText(fullPath, envInfo.ToString(), Encoding.UTF8);
                _logger.Information("Environment info captured: {Path}", fullPath);
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to capture environment info: {Error}", ex.Message);
            }
        }

        /// <summary>
        /// Capture network performance metrics
        /// </summary>
        public void CapturePerformanceMetrics(string testName)
        {
            try
            {
                var performanceScript = @"
                    var perfData = window.performance.getEntriesByType('navigation')[0];
                    return {
                        'loadEventEnd': perfData.loadEventEnd,
                        'loadEventStart': perfData.loadEventStart,
                        'domContentLoadedEventEnd': perfData.domContentLoadedEventEnd,
                        'domContentLoadedEventStart': perfData.domContentLoadedEventStart,
                        'responseEnd': perfData.responseEnd,
                        'responseStart': perfData.responseStart,
                        'requestStart': perfData.requestStart,
                        'connectEnd': perfData.connectEnd,
                        'connectStart': perfData.connectStart,
                        'domainLookupEnd': perfData.domainLookupEnd,
                        'domainLookupStart': perfData.domainLookupStart
                    };";

                var perfData = ((IJavaScriptExecutor)_driver).ExecuteScript(performanceScript);
                
                if (perfData != null)
                {
                    _logger.Information("Performance metrics captured for test: {TestName}, Data: {PerfData}", testName, perfData);
                }
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to capture performance metrics: {Error}", ex.Message);
            }
        }
    }
}

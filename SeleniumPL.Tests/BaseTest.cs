using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Microsoft.Extensions.Configuration;
using Serilog;
using SeleniumPL.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace SeleniumPL.Tests
{
    public abstract class BaseTest
    {
        protected IWebDriver Driver { get; private set; } = null!;
        protected WebDriverManager DriverManager { get; private set; } = null!;
        protected IConfiguration Configuration { get; private set; } = null!;
        protected ILogger Logger { get; private set; } = null!;
        protected TestUserManager UserManager { get; private set; } = null!;
        protected TestUser? CurrentTestUser { get; private set; }
        
        // Retry mechanism properties
        private static readonly Dictionary<string, int> TestRetryCount = new Dictionary<string, int>();
        private const int MaxRetryAttempts = 2; // Maximum retry attempts per test

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Setup configuration
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();

            // Setup logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(Configuration)
                .WriteTo.Console()
                .WriteTo.File(
                    path: Configuration["Logging:LogFilePath"] ?? "Logs/test-log.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            Logger = Log.Logger;
            Logger.Information("Starting test session");

            // Initialize User Manager
            UserManager = new TestUserManager(Configuration);
            Logger.Information("Test User Manager initialized with {UserCount} users", UserManager.GetAvailableUserIds().Count);

            // Create directories if they don't exist
            CreateDirectoriesIfNotExist();
        }

        [SetUp]
        public void SetUp()
        {
            Logger.Information("Starting test: {TestName}", TestContext.CurrentContext.Test.Name);
            
            DriverManager = new WebDriverManager(Configuration, Logger);
            var browserType = Configuration["WebDriver:Browser"] ?? "chrome";
            Driver = DriverManager.CreateDriver(browserType);

            // Navigate to base URL if configured
            var baseUrl = Configuration["TestSettings:BaseUrl"];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                Driver.Navigate().GoToUrl(baseUrl);
            }
        }

        [TearDown]
        public void TearDown()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            var testResult = TestContext.CurrentContext.Result.Outcome;
            var fullTestName = $"{TestContext.CurrentContext.Test.ClassName}.{testName}";

            Logger.Information("Test completed: {TestName}, Result: {Result}", testName, testResult);

            // Handle retry logic for failed tests
            if (testResult == NUnit.Framework.Interfaces.ResultState.Failure ||
                testResult == NUnit.Framework.Interfaces.ResultState.Error)
            {
                HandleTestFailure(fullTestName, testName, testResult);
            }
            else
            {
                // Clear retry count for successful tests
                if (TestRetryCount.ContainsKey(fullTestName))
                {
                    TestRetryCount.Remove(fullTestName);
                }
            }

            // Take screenshot on failure (only if driver is still available)
            if (testResult == NUnit.Framework.Interfaces.ResultState.Failure ||
                testResult == NUnit.Framework.Interfaces.ResultState.Error)
            {
                try
                {
                    if (Driver != null)
                    {
                        TakeScreenshot(testName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("Could not take screenshot: {Error}", ex.Message);
                }
            }

            // Release test user
            ReleaseTestUser();

            // Check if this is a login test that should maintain session
            var testClassName = TestContext.CurrentContext.Test.ClassName;
            var isLoginTest = testClassName?.Contains("PracticalLawLoginTestsPOM") == true;
            var isPracticeAreaTest = testClassName?.Contains("PracticeAreaTestsPOM") == true;
            
            // Always close browser on test failure or for Practice Area tests
            bool shouldCloseBrowser = isPracticeAreaTest || 
                                    testResult == NUnit.Framework.Interfaces.ResultState.Failure || 
                                    testResult == NUnit.Framework.Interfaces.ResultState.Error ||
                                    (!isLoginTest);
            
            if (shouldCloseBrowser)
            {
                // For Practice Area tests, the TearDown method handles browser closure
                if (!isPracticeAreaTest)
                {
                    try
                    {
                        if (Driver != null)
                        {
                            Logger.Information("Closing browser for test: {TestName} (Result: {Result})", testName, testResult);
                            DriverManager?.QuitDriver(Driver);
                            
                            // Force cleanup for failed tests
                            if (testResult == NUnit.Framework.Interfaces.ResultState.Failure ||
                                testResult == NUnit.Framework.Interfaces.ResultState.Error)
                            {
                                ForceCloseBrowserProcesses();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning("Error quitting driver: {Error}", ex.Message);
                        ForceCloseBrowserProcesses();
                    }
                }
            }
            else if (!isPracticeAreaTest)
            {
                Logger.Information("Preserving driver session for subsequent tests");
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Logger.Information("Test session completed");
            Log.CloseAndFlush();
        }

        protected void TakeScreenshot(string testName)
        {
            try
            {
                var screenshotPath = Configuration["TestSettings:ScreenshotPath"] ?? "Screenshots";
                var fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var fullPath = Path.Combine(screenshotPath, fileName);

                Directory.CreateDirectory(screenshotPath);

                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                screenshot.SaveAsFile(fullPath);
                
                Logger.Information("Screenshot saved: {ScreenshotPath}", fullPath);
                
                // Add screenshot to test context for reporting
                TestContext.AddTestAttachment(fullPath, "Screenshot on failure");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Failed to take screenshot");
            }
        }

        private void CreateDirectoriesIfNotExist()
        {
            var directories = new[]
            {
                Configuration["TestSettings:ScreenshotPath"] ?? "Screenshots",
                Configuration["TestSettings:ReportsPath"] ?? "Reports",
                Path.GetDirectoryName(Configuration["Logging:LogFilePath"]) ?? "Logs"
            };

            foreach (var directory in directories)
            {
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        // Helper method to get configuration value with default
        protected T GetConfigValue<T>(string key, T defaultValue = default(T)!) where T : notnull
        {
            return Configuration.GetValue(key, defaultValue)!;
        }

        // Helper method to wait
        protected void Wait(int seconds)
        {
            Thread.Sleep(TimeSpan.FromSeconds(seconds));
        }

        // Intelligent wait helper methods for optimized test performance
        protected IWebElement WaitForElementToBeClickable(By locator, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                var element = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(locator));
                Logger.Debug("Element is clickable: {Locator}", locator);
                return element;
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException ex)
            {
                Logger.Warning("Element not clickable within {Timeout} seconds: {Locator}", timeoutSeconds, locator);
                throw new TimeoutException($"Element not clickable: {locator}", ex);
            }
        }

        protected IWebElement WaitForElementToBeVisible(By locator, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                var element = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(locator));
                Logger.Debug("Element is visible: {Locator}", locator);
                return element;
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException ex)
            {
                Logger.Warning("Element not visible within {Timeout} seconds: {Locator}", timeoutSeconds, locator);
                throw new TimeoutException($"Element not visible: {locator}", ex);
            }
        }

        protected bool WaitForElementToDisappear(By locator, int timeoutSeconds = 5)
        {
            try
            {
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.InvisibilityOfElementLocated(locator));
                Logger.Debug("Element disappeared: {Locator}", locator);
                return true;
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException)
            {
                Logger.Debug("Element still visible after {Timeout} seconds: {Locator}", timeoutSeconds, locator);
                return false;
            }
        }

        protected void WaitForPageLoad(int timeoutSeconds = 5)
        {
            try
            {
                var wait = new OpenQA.Selenium.Support.UI.WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
                Logger.Debug("Page load completed");
            }
            catch (OpenQA.Selenium.WebDriverTimeoutException)
            {
                Logger.Debug("Page load timeout after {Timeout} seconds, continuing anyway", timeoutSeconds);
            }
        }

        // User management methods
        protected TestUser GetTestUser()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            // Always use User1 (WnIndigoTestUser1@mailinator.com) for all tests
            CurrentTestUser = UserManager.GetSpecificUser("User1", testName);
            Logger.Information("Assigned test user {UserId} to test {TestName}", CurrentTestUser.UserId, testName);
            return CurrentTestUser;
        }

        protected TestUser GetSpecificTestUser(string userId)
        {
            var testName = TestContext.CurrentContext.Test.Name;
            CurrentTestUser = UserManager.GetSpecificUser(userId, testName);
            Logger.Information("Assigned specific test user {UserId} to test {TestName}", CurrentTestUser.UserId, testName);
            return CurrentTestUser;
        }

        protected void ReleaseTestUser()
        {
            if (CurrentTestUser != null)
            {
                Logger.Information("Releasing test user {UserId} from test {TestName}", CurrentTestUser.UserId, CurrentTestUser.AssignedTest);
                UserManager.ReleaseUser(CurrentTestUser.UserId);
                CurrentTestUser = null;
            }
        }

        // Test failure handling and retry mechanism
        private void HandleTestFailure(string fullTestName, string testName, NUnit.Framework.Interfaces.ResultState testResult)
        {
            // Only retry on specific error conditions (browser crashes, timeouts, etc.)
            // Don't retry on assertion failures or test logic errors
            var testContext = TestContext.CurrentContext;
            var exception = testContext.Result.Message;
            
            bool shouldRetry = false;
            if (!string.IsNullOrEmpty(exception))
            {
                // Retry only on browser/driver related errors
                var retryableErrors = new[]
                {
                    "session not created",
                    "chrome not reachable", 
                    "connection refused",
                    "timeout",
                    "WebDriverException",
                    "NoSuchSessionException",
                    "browser disconnected"
                };
                
                shouldRetry = retryableErrors.Any(error => 
                    exception.Contains(error, StringComparison.OrdinalIgnoreCase));
            }
            
            if (!shouldRetry)
            {
                Logger.Information("❌ Test failed with assertion/logic error - no retry needed: {TestName}", testName);
                return;
            }
            
            // Increment retry count only for retryable errors
            if (!TestRetryCount.ContainsKey(fullTestName))
            {
                TestRetryCount[fullTestName] = 0;
            }
            
            TestRetryCount[fullTestName]++;
            int currentAttempt = TestRetryCount[fullTestName];

            Logger.Warning("❌ Test failed with retryable error: {TestName} (Attempt {Attempt}/{MaxAttempts})", 
                testName, currentAttempt, MaxRetryAttempts + 1);

            if (currentAttempt <= MaxRetryAttempts)
            {
                Logger.Information("🔄 Test will be retried after browser cleanup");
                
                // Force close browser for retry
                try
                {
                    if (Driver != null)
                    {
                        Driver.Quit();
                        Driver.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("Error during browser cleanup for retry: {Error}", ex.Message);
                }
                
                ForceCloseBrowserProcesses();
                
                // Brief wait before retry
                System.Threading.Thread.Sleep(2000);
                Logger.Information("✅ Browser cleanup completed, test ready for retry");
            }
            else
            {
                Logger.Error("❌ Test {TestName} has exceeded maximum retry attempts ({MaxAttempts})", 
                    testName, MaxRetryAttempts);
                TestRetryCount.Remove(fullTestName);
            }
        }

        // Force close browser processes
        private void ForceCloseBrowserProcesses()
        {
            try
            {
                var browserProcessNames = new[] { "chrome", "msedge", "firefox", "chromedriver", "geckodriver", "msedgedriver" };
                
                foreach (var processName in browserProcessNames)
                {
                    try
                    {
                        var processes = System.Diagnostics.Process.GetProcessesByName(processName);
                        foreach (var process in processes)
                        {
                            try
                            {
                                if (!process.HasExited)
                                {
                                    process.Kill();
                                    process.WaitForExit(2000); // Wait max 2 seconds
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Debug("Could not kill process {ProcessName}: {Error}", processName, ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Error finding processes {ProcessName}: {Error}", processName, ex.Message);
                    }
                }
                
                Logger.Information("✅ Force-closed browser processes for test retry");
            }
            catch (Exception ex)
            {
                Logger.Warning("Error in ForceCloseBrowserProcesses: {Error}", ex.Message);
            }
        }
    }
}

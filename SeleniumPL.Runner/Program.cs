using Microsoft.Extensions.Configuration;
using Serilog;
using SeleniumPL.Core;
using SeleniumPL.Tests.Pages;

namespace SeleniumPL.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("SeleniumPL Framework Demo");
            Console.WriteLine("=========================");

            // Setup configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ENVIRONMENT") ?? "Development"}.json", optional: true)
                .Build();

            // Setup logging
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console()
                .WriteTo.File(
                    path: configuration["Logging:LogFilePath"] ?? "Logs/runner-log.log",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)
                .CreateLogger();

            var logger = Log.Logger;
            logger.Information("Starting SeleniumPL Framework Demo");

            try
            {
                // Create WebDriver
                var driverManager = new WebDriverManager(configuration, logger);
                var browserType = configuration["WebDriver:Browser"] ?? "chrome";
                var driver = driverManager.CreateDriver(browserType);

                try
                {
                    // Demo the framework
                    RunDemo(driver, logger);
                }
                finally
                {
                    // Clean up
                    driverManager.QuitDriver(driver);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred during demo execution");
            }
            finally
            {
                Log.CloseAndFlush();
            }

            Console.WriteLine("\nDemo completed. Press any key to exit...");
            Console.ReadKey();
        }

        static void RunDemo(OpenQA.Selenium.IWebDriver driver, ILogger logger)
        {
            logger.Information("Starting framework demo");

            // Create page object
            var practicalLawPage = new PracticalLawHomePagePOM(driver, logger);

            // Navigate to Practical Law
            logger.Information("Navigating to Practical Law homepage");
            practicalLawPage.NavigateTo();

            // Verify page elements
            logger.Information("Verifying page elements");
            try
            {
                practicalLawPage.HandleCookieConsent();
                logger.Information("✓ Practical Law homepage loaded successfully");
                Console.WriteLine("✓ Practical Law homepage loaded successfully");
                Console.WriteLine($"Page Title: {driver.Title}");
                Console.WriteLine($"Current URL: {driver.Url}");
            }
            catch (Exception ex)
            {
                logger.Warning("✗ Practical Law homepage elements verification failed: {Error}", ex.Message);
                Console.WriteLine("✗ Practical Law homepage elements verification failed");
                return;
            }

            // Check if user is logged in
            logger.Information("Checking for login status");
            if (practicalLawPage.IsUserLoggedIn())
            {
                logger.Information("✓ User is already logged in");
                Console.WriteLine("✓ User is already logged in");
            }
            else
            {
                logger.Information("◦ User is not logged in");
                Console.WriteLine("◦ User is not logged in");
            }

            // Take a screenshot
            logger.Information("Taking screenshot");
            var screenshot = practicalLawPage.TakeScreenshot();
            var screenshotPath = Path.Combine("Screenshots", $"demo_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            Directory.CreateDirectory("Screenshots");
            File.WriteAllBytes(screenshotPath, screenshot);
            logger.Information("Screenshot saved: {ScreenshotPath}", screenshotPath);
            Console.WriteLine($"Screenshot saved: {screenshotPath}");

            logger.Information("Demo completed successfully");
        }
    }
}

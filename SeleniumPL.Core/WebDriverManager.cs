using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace SeleniumPL.Core
{
    public class WebDriverManager
    {
        private readonly IConfiguration _config;
        private readonly ILogger _logger;
        
        public WebDriverManager(IConfiguration config, ILogger logger)
        {
            _config = config;
            _logger = logger;
        }

        public IWebDriver CreateDriver(string browserType = "chrome")
        {
            var browser = browserType?.ToLower() ?? "chrome";
            _logger.Information("Creating WebDriver for browser: {Browser}", browser);

            return browser switch
            {
                "chrome" => CreateChromeDriver(),
                "firefox" => CreateFirefoxDriver(),
                "edge" => CreateEdgeDriver(),
                _ => throw new ArgumentException($"Unsupported browser type: {browserType}")
            };
        }

        private IWebDriver CreateChromeDriver()
        {
            var options = new ChromeOptions();
            
            // Create unique user data directory to avoid session conflicts
            var tempUserDataDir = Path.Combine(Path.GetTempPath(), "SeleniumChrome_" + Guid.NewGuid().ToString("N")[..8]);
            options.AddArgument($"--user-data-dir={tempUserDataDir}");
            _logger.Information("Using unique Chrome user data directory: {UserDataDir}", tempUserDataDir);
            
            // Load options from configuration
            var headless = _config.GetValue<bool>("WebDriver:Headless");
            var windowSize = _config.GetValue<string>("WebDriver:WindowSize");
            var implicitWait = _config.GetValue<int>("WebDriver:ImplicitWaitSeconds");

            if (headless)
            {
                options.AddArgument("--headless");
                _logger.Information("Chrome running in headless mode");
            }

            if (!string.IsNullOrEmpty(windowSize))
            {
                options.AddArgument($"--window-size={windowSize}");
            }

            // Enhanced anti-detection arguments
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-web-security");
            options.AddArgument("--allow-running-insecure-content");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--disable-features=VizDisplayCompositor");
            
            // Enable incognito mode for clean sessions and better privacy
            options.AddArgument("--incognito");
            
            // Disable automation indicators
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            
            // Disable password save prompts and autofill
            options.AddArgument("--disable-save-password-bubble");
            options.AddArgument("--disable-password-generation");
            options.AddArgument("--disable-autofill");
            options.AddArgument("--disable-autofill-keyboard-accessory-view");
            options.AddArgument("--disable-full-form-autofill-ios");
            
            // Additional stealth arguments
            options.AddArgument("--disable-extensions-file-access-check");
            options.AddArgument("--disable-extensions-http-throttling");
            options.AddArgument("--disable-ipc-flooding-protection");
            options.AddArgument("--disable-renderer-backgrounding");
            options.AddArgument("--disable-backgrounding-occluded-windows");
            options.AddArgument("--disable-features=TranslateUI");
            options.AddArgument("--disable-component-extensions-with-background-pages");
            
            // Set realistic browser preferences
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
            options.AddUserProfilePreference("autofill.profile_enabled", false);
            options.AddUserProfilePreference("autofill.credit_card_enabled", false);
            options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
            options.AddUserProfilePreference("profile.managed_default_content_settings.images", 1);
            
            // Set realistic user agent - use latest Chrome version
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

            var driver = new ChromeDriver(options);
            
            // Execute stealth JavaScript to hide automation traces
            try
            {
                driver.ExecuteScript(@"
                    // Override navigator.webdriver
                    Object.defineProperty(navigator, 'webdriver', {
                        get: () => undefined,
                    });
                    
                    // Override chrome runtime
                    window.chrome = {
                        runtime: {},
                    };
                    
                    // Override plugins
                    Object.defineProperty(navigator, 'plugins', {
                        get: () => [1, 2, 3, 4, 5],
                    });
                    
                    // Override languages
                    Object.defineProperty(navigator, 'languages', {
                        get: () => ['en-US', 'en'],
                    });
                    
                    // Override permissions
                    const originalQuery = window.navigator.permissions.query;
                    return window.navigator.permissions.query = (parameters) => (
                        parameters.name === 'notifications' ?
                            Promise.resolve({ state: Notification.permission }) :
                            originalQuery(parameters)
                    );
                ");
                _logger.Information("Stealth JavaScript injected successfully");
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to inject stealth JavaScript: {Message}", ex.Message);
            }
            
            if (implicitWait > 0)
            {
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(implicitWait);
            }
            
            // Set longer page load timeout for slow networks
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);

            driver.Manage().Window.Maximize();
            _logger.Information("Chrome WebDriver created successfully with enhanced anti-detection");
            
            return driver;
        }

        private IWebDriver CreateFirefoxDriver()
        {
            var options = new FirefoxOptions();
            
            var headless = _config.GetValue<bool>("WebDriver:Headless");
            if (headless)
            {
                options.AddArgument("--headless");
                _logger.Information("Firefox running in headless mode");
            }

            var driver = new FirefoxDriver(options);
            driver.Manage().Window.Maximize();
            _logger.Information("Firefox WebDriver created successfully");
            
            return driver;
        }

        private IWebDriver CreateEdgeDriver()
        {
            var options = new EdgeOptions();
            
            var headless = _config.GetValue<bool>("WebDriver:Headless");
            if (headless)
            {
                options.AddArgument("--headless");
                _logger.Information("Edge running in headless mode");
            }

            var driver = new EdgeDriver(options);
            driver.Manage().Window.Maximize();
            _logger.Information("Edge WebDriver created successfully");
            
            return driver;
        }

        public void QuitDriver(IWebDriver driver)
        {
            try
            {
                driver?.Quit();
                _logger.Information("WebDriver quit successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error occurred while quitting WebDriver");
            }
        }
    }
}

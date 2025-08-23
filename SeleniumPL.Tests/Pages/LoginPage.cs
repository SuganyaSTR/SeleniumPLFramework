using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumPL.Core;
using Serilog;
using System;
using System.Threading;

namespace SeleniumPL.Tests.Pages
{
    /// <summary>
    /// Page Object Model for Login Page
    /// Handles username entry, navigation to password page, and password entry
    /// </summary>
    public class LoginPage : BasePage
    {
        #region Locators
        
        // Username field locators
        private readonly By[] UsernameFieldLocators = new[]
        {
            By.XPath("//*[@id=\"Username\"]"),
            By.Id("Username"),
            By.Id("username"),
            By.Name("username"),
            By.CssSelector("input[type='email']"),
            By.CssSelector("input[name*='user']"),
            By.CssSelector("input[placeholder*='username']"),
            By.CssSelector("input[placeholder*='email']")
        };

        // Password field locators
        private readonly By[] PasswordFieldLocators = new[]
        {
            By.XPath("//*[@id=\"password\"]"),
            By.Id("password"),
            By.Name("password"),
            By.CssSelector("input[type='password']")
        };

        // Submit button locators
        private readonly By[] SubmitButtonLocators = new[]
        {
            By.Id("signInBtn"),
            By.CssSelector("button[type='submit']"),
            By.CssSelector("input[type='submit']"),
            By.CssSelector("button[contains(text(), 'Sign in')]"),
            By.CssSelector("button[contains(text(), 'Login')]"),
            By.CssSelector("input[value*='Sign in']"),
            By.CssSelector("input[value*='Login']")
        };

        // Error message locators
        private readonly By[] ErrorMessageLocators = new[]
        {
            By.CssSelector(".error"),
            By.CssSelector(".error-message"),
            By.CssSelector(".alert-danger"),
            By.CssSelector("[role='alert']"),
            By.CssSelector(".validation-error"),
            By.XPath("//*[contains(@class, 'error') and contains(text(), 'Invalid')]")
        };

        #endregion

        public LoginPage(IWebDriver driver, ILogger? logger = null) : base(driver, logger)
        {
        }

        #region Page Actions

        /// <summary>
        /// Enter username in the username field
        /// </summary>
        /// <param name="username">Username/email to enter</param>
        /// <returns>LoginPage for method chaining</returns>
        public LoginPage EnterUsername(string username)
        {
            Logger.Information("Entering username: {Username}", username);
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            var usernameField = wait.Until(d => 
            {
                foreach (var selector in UsernameFieldLocators)
                {
                    try
                    {
                        var element = d.FindElement(selector);
                        if (element.Displayed && element.Enabled)
                        {
                            Logger.Information("Found username field using selector: {Selector}", selector);
                            return element;
                        }
                    }
                    catch { }
                }
                return null;
            });
            
            if (usernameField == null)
                throw new Exception("Username field not found on the page");
                
            // Use human-like typing to avoid detection
            TypeHumanLike(usernameField, username);
            Logger.Information("Username entered successfully with human-like typing");
            
            return this;
        }

        /// <summary>
        /// Trigger navigation to password page by clicking somewhere on the page
        /// This is specific to the Thomson Reuters login flow
        /// </summary>
        /// <returns>LoginPage for method chaining</returns>
        public LoginPage TriggerPasswordPageNavigation()
        {
            Logger.Information("Triggering navigation to password page");
            
            try
            {
                // Click on the body element or somewhere neutral on the page
                var body = Driver.FindElement(By.TagName("body"));
                body.Click();
                Logger.Information("Clicked on body element to trigger navigation");
            }
            catch (Exception ex)
            {
                Logger.Warning("Failed to click on body: {Error}", ex.Message);
                try
                {
                    // Alternative: Use JavaScript to click
                    ((IJavaScriptExecutor)Driver).ExecuteScript("document.body.click();");
                    Logger.Information("Used JavaScript to click on document body");
                }
                catch (Exception jsEx)
                {
                    Logger.Warning("JavaScript click also failed: {Error}", jsEx.Message);
                }
            }
            
            // Wait for navigation to password page
            Logger.Information("Waiting for navigation to password page");
            System.Threading.Thread.Sleep(3000); // Give time for navigation
            
            return this;
        }

        /// <summary>
        /// Enter password in the password field
        /// </summary>
        /// <param name="password">Password to enter</param>
        /// <returns>LoginPage for method chaining</returns>
        public LoginPage EnterPassword(string password)
        {
            Logger.Information("Entering password");
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            var passwordField = wait.Until(d => 
            {
                foreach (var selector in PasswordFieldLocators)
                {
                    try
                    {
                        var element = d.FindElement(selector);
                        if (element.Displayed && element.Enabled)
                        {
                            Logger.Information("Found password field using selector: {Selector}", selector);
                            return element;
                        }
                    }
                    catch { }
                }
                return null;
            });
            
            if (passwordField == null)
                throw new Exception("Password field not found on the page");
                
            // Add more realistic delay before entering password (1-2 seconds)
            Thread.Sleep(Random.Shared.Next(1000, 2000));
            
            // Use human-like typing to avoid detection
            TypeHumanLike(passwordField, password);
            Logger.Information("Password entered successfully with human-like typing");
            
            return this;
        }

        /// <summary>
        /// Click the final sign in button to complete login
        /// </summary>
        /// <returns>DashboardPage or appropriate page after login</returns>
        public DashboardPage ClickSignIn()
        {
            Logger.Information("Clicking final Sign In button");
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            var signInButton = wait.Until(d => 
            {
                foreach (var selector in SubmitButtonLocators)
                {
                    try
                    {
                        var element = d.FindElement(selector);
                        if (element.Displayed && element.Enabled)
                        {
                            Logger.Information("Found sign in button using selector: {Selector}", selector);
                            return element;
                        }
                    }
                    catch { }
                }
                return null;
            });
            
            if (signInButton == null)
                throw new Exception("Sign in button not found on the page");
                
            // Use human-like clicking with random delay (1-2 seconds)
            Thread.Sleep(Random.Shared.Next(1000, 2000));
            ClickHumanLike(signInButton);
            Logger.Information("Sign in button clicked with human-like behavior - waiting for login to complete");
            
            // Wait for login to complete with longer delay
            Thread.Sleep(8000);
            
            // Handle password save dialog if it appears
            try
            {
                HandlePasswordSaveDialog();
                Logger.Information("Password save dialog handling completed");
            }
            catch (Exception ex)
            {
                Logger.Warning("Password save dialog handling failed: {Message}", ex.Message);
            }
            
            return new DashboardPage(Driver);
        }

        /// <summary>
        /// Perform complete login flow with username and password
        /// </summary>
        /// <param name="username">Username/email</param>
        /// <param name="password">Password</param>
        /// <returns>DashboardPage after successful login</returns>
        public DashboardPage Login(string username, string password)
        {
            Logger.Information("Performing complete login flow for user: {Username}", username);
            
            return EnterUsername(username)
                   .TriggerPasswordPageNavigation()
                   .EnterPassword(password)
                   .ClickSignIn();
        }

        #endregion

        #region Page Validations

        /// <summary>
        /// Check if there are any login error messages displayed
        /// </summary>
        /// <returns>Error message text if found, null otherwise</returns>
        public string GetErrorMessage()
        {
            foreach (var selector in ErrorMessageLocators)
            {
                try
                {
                    var elements = Driver.FindElements(selector);
                    foreach (var element in elements)
                    {
                        if (element.Displayed && !string.IsNullOrWhiteSpace(element.Text))
                        {
                            var errorText = element.Text;
                            Logger.Warning("Login error message found: {ErrorMessage}", errorText);
                            return errorText;
                        }
                    }
                }
                catch { }
            }
            
            return null;
        }

        /// <summary>
        /// Validate that we're on a login page
        /// </summary>
        /// <returns>True if on login page</returns>
        public bool IsOnLoginPage()
        {
            var currentUrl = Driver.Url.ToLower();
            Logger.Information("Login page validation - Current URL: {Url}", Driver.Url);
            
            // Check URL patterns for login/auth pages
            var urlIndicators = new[] { "login", "signin", "auth", "signon", "authenticate" };
            var isLoginUrlPattern = urlIndicators.Any(indicator => currentUrl.Contains(indicator));
            
            // Also check if username field is visible (more reliable indicator)
            var isUsernameFieldVisible = IsUsernameFieldVisible();
            
            // Page title check
            var pageTitle = Driver.Title?.ToLower() ?? "";
            var isTitleIndicatingLogin = urlIndicators.Any(indicator => pageTitle.Contains(indicator));
            
            var isLoginPage = isLoginUrlPattern || isUsernameFieldVisible || isTitleIndicatingLogin;
            
            Logger.Information("Login page validation - URL pattern: {UrlPattern}, Username field visible: {UsernameVisible}, Title indicates login: {TitleLogin}, Overall result: {IsValid}", 
                isLoginUrlPattern, isUsernameFieldVisible, isTitleIndicatingLogin, isLoginPage);
                
            return isLoginPage;
        }

        /// <summary>
        /// Check if username field is visible and ready for input
        /// </summary>
        /// <returns>True if username field is available</returns>
        public bool IsUsernameFieldVisible()
        {
            foreach (var selector in UsernameFieldLocators)
            {
                try
                {
                    var element = Driver.FindElement(selector);
                    if (element.Displayed && element.Enabled)
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        /// <summary>
        /// Check if password field is visible and ready for input
        /// </summary>
        /// <returns>True if password field is available</returns>
        public bool IsPasswordFieldVisible()
        {
            foreach (var selector in PasswordFieldLocators)
            {
                try
                {
                    var element = Driver.FindElement(selector);
                    if (element.Displayed && element.Enabled)
                    {
                        return true;
                    }
                }
                catch { }
            }
            return false;
        }

        #endregion
    }
}

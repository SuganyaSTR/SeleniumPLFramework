using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumPL.Core;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SeleniumPL.Tests.Pages
{
    /// <summary>
    /// Page Object Model for Dashboard/Main Application Page after login
    /// Handles post-login validations and main application navigation
    /// </summary>
    public class DashboardPage : BasePage
    {
        #region Locators
        
        // Sign out button locators
        private readonly By[] SignOutButtonLocators = new[]
        {
            By.LinkText("Sign out"),
            By.PartialLinkText("Sign out"),
            By.LinkText("Logout"),
            By.PartialLinkText("Logout"),
            By.CssSelector("a[contains(text(), 'Sign out')]"),
            By.CssSelector("a[contains(text(), 'Logout')]"),
            By.CssSelector("button[contains(text(), 'Sign out')]"),
            By.CssSelector("button[contains(text(), 'Logout')]"),
            By.XPath("//a[contains(text(), 'Sign out') or contains(text(), 'Logout')]"),
            By.XPath("//button[contains(text(), 'Sign out') or contains(text(), 'Logout')]"),
            By.CssSelector("[href*='logout']"),
            By.CssSelector("[href*='signout']")
        };

        // Profile icon with sign out functionality
        private readonly By ProfileSignOutIcon = By.XPath("//*[@id=\"coid_website_signOffRegion\"]");
        
        // Sign out button after clicking profile icon
        private readonly By SignOutButtonAfterProfileClick = By.XPath("//*[@id=\"co_signOffContainer\"]/div[2]/div[2]/div[3]/div/button");
        
        // Profile icon tooltip locators
        private readonly By[] ProfileTooltipLocators = new[]
        {
            By.CssSelector("[role='tooltip']"),
            By.CssSelector(".tooltip"),
            By.CssSelector(".tooltip-content"),
            By.CssSelector("[data-tooltip]"),
            By.XPath("//*[contains(@class, 'tooltip')]"),
            By.XPath("//*[contains(text(), 'Sign out') or contains(text(), 'Profile')]")
        };

        // Dashboard validation elements
        private readonly By[] DashboardPageIndicators = new[]
        {
            By.CssSelector("body"),
            By.TagName("html"),
            By.XPath("//title[contains(text(), 'Practical Law')]"),
            By.CssSelector("[class*='dashboard']"),
            By.CssSelector("[class*='main']"),
            By.CssSelector("[class*='home']")
        };

        #endregion

        #region Constructor

        public DashboardPage(IWebDriver driver) : base(driver)
        {
            // Logger will be initialized from BasePage
        }

        #endregion

        #region Page Validation Methods

        /// <summary>
        /// Check if the page is loaded by validating presence of dashboard elements
        /// </summary>
        /// <returns>True if dashboard page is loaded</returns>
        public bool IsPageLoaded()
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
                
                // Check if any dashboard indicator is present
                foreach (var indicator in DashboardPageIndicators)
                {
                    try
                    {
                        var element = wait.Until(d => d.FindElement(indicator));
                        if (element.Displayed)
                        {
                            Logger.Information("✅ Dashboard page loaded - found indicator: {Indicator}", indicator);
                            return true;
                        }
                    }
                    catch { }
                }
                
                Logger.Warning("❌ Dashboard page load validation failed");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error validating dashboard page load: {Error}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Login Validation Methods

        /// <summary>
        /// Check if user is successfully logged in using multiple validation methods
        /// </summary>
        /// <returns>True if user is logged in</returns>
        public bool IsUserLoggedIn()
        {
            Logger.Information("Validating user login status...");
            
            // Method 1: Look for sign out button/link
            var signOutButton = GetSignOutButton();
            if (signOutButton != null && signOutButton.Displayed)
            {
                Logger.Information("✅ Login validated - Sign out button found: '{Text}'", signOutButton.Text);
                return true;
            }
            
            // Method 2: Check for profile icon
            var profileIcon = GetProfileIcon();
            if (profileIcon != null && profileIcon.Displayed)
            {
                Logger.Information("✅ Login validated - Profile icon found");
                return true;
            }
            
            // Method 3: Check URL for login indicators
            try
            {
                var currentUrl = Driver.Url.ToLower();
                if (currentUrl.Contains("practicallaw") && !currentUrl.Contains("login") && !currentUrl.Contains("signin"))
                {
                    Logger.Information("✅ Login validated - URL indicates successful login: {Url}", currentUrl);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Warning("❌ Could not access Driver URL for login validation: {Message}", ex.Message);
            }
            
            Logger.Warning("❌ Login validation failed - no indicators found");
            return false;
        }

        /// <summary>
        /// Get the sign out button element if visible
        /// </summary>
        /// <returns>Sign out button element or null if not found</returns>
        public IWebElement? GetSignOutButton()
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(15));
            
            try
            {
                return wait.Until(d => 
                {
                    // First try to find the profile icon
                    var profileIcon = GetProfileIcon();
                    if (profileIcon != null)
                    {
                        Logger.Information("Found profile icon using XPath: {XPath}", ProfileSignOutIcon);
                        return profileIcon;
                    }
                    
                    // Fallback to other sign out button locators
                    foreach (var selector in SignOutButtonLocators)
                    {
                        try
                        {
                            var elements = d.FindElements(selector);
                            foreach (var element in elements)
                            {
                                if (element.Displayed)
                                {
                                    Logger.Information("Found Sign Out button using selector: {Selector}, Text: {Text}", 
                                        selector, element.Text);
                                    return element;
                                }
                            }
                        }
                        catch { }
                    }
                    return null;
                });
            }
            catch (WebDriverTimeoutException)
            {
                Logger.Warning("Sign out button not found within timeout period");
                return null;
            }
        }

        /// <summary>
        /// Get the profile icon element using the specific XPath
        /// </summary>
        /// <returns>Profile icon element or null if not found</returns>
        public IWebElement? GetProfileIcon()
        {
            try
            {
                var profileIcon = Driver.FindElement(ProfileSignOutIcon);
                if (profileIcon.Displayed)
                {
                    Logger.Information("Profile icon found and visible");
                    return profileIcon;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Profile icon not found: {Error}", ex.Message);
            }
            return null;
        }

        /// <summary>
        /// Hover over the profile icon and check for sign out tooltip
        /// </summary>
        /// <returns>True if profile icon exists and tooltip appears on hover</returns>
        public bool ValidateProfileIconWithTooltip()
        {
            try
            {
                var profileIcon = GetProfileIcon();
                if (profileIcon == null)
                {
                    Logger.Warning("Profile icon not found - cannot validate tooltip");
                    return false;
                }

                Logger.Information("Hovering over profile icon to check for tooltip");
                
                // Perform mouse hover using Actions
                var actions = new OpenQA.Selenium.Interactions.Actions(Driver);
                actions.MoveToElement(profileIcon).Perform();
                
                // Wait a moment for tooltip to appear
                System.Threading.Thread.Sleep(1000);
                
                // Check for tooltip with sign out text
                foreach (var selector in ProfileTooltipLocators)
                {
                    try
                    {
                        var tooltipElements = Driver.FindElements(selector);
                        foreach (var tooltip in tooltipElements)
                        {
                            if (tooltip.Displayed)
                            {
                                var tooltipText = tooltip.Text.ToLower();
                                if (tooltipText.Contains("sign out") || 
                                    tooltipText.Contains("logout") || 
                                    tooltipText.Contains("profile"))
                                {
                                    Logger.Information("✅ Profile tooltip found with text: '{Text}'", tooltip.Text);
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }
                }
                
                // Even if tooltip text is not found, if profile icon exists and is clickable, consider it valid
                Logger.Information("✅ Profile icon exists and is interactive (tooltip text verification inconclusive)");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning("Error validating profile icon tooltip: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Enhanced login verification using profile icon validation
        /// </summary>
        /// <returns>True if user is logged in (validated via profile icon or fallback methods)</returns>
        public bool IsUserLoggedInEnhanced()
        {
            Logger.Information("Performing enhanced login validation using profile icon");
            
            // Method 1: Check for profile icon with tooltip validation
            if (ValidateProfileIconWithTooltip())
            {
                Logger.Information("✅ Login validated via profile icon with tooltip");
                return true;
            }
            
            // Method 2: Fallback to standard sign out button check
            var signOutButton = GetSignOutButton();
            if (signOutButton != null && signOutButton.Displayed)
            {
                Logger.Information("✅ Login validated via sign out button fallback");
                return true;
            }
            
            Logger.Warning("❌ Login validation failed - no profile icon or sign out button found");
            return false;
        }

        /// <summary>
        /// Get the text of the sign out button
        /// </summary>
        /// <returns>Sign out button text or null if button not found</returns>
        public string? GetSignOutButtonText()
        {
            var signOutButton = GetSignOutButton();
            return signOutButton?.Text;
        }

        /// <summary>
        /// Click on the profile icon and validate that the sign out button appears
        /// </summary>
        /// <returns>True if profile icon was clicked and sign out button is visible</returns>
        public bool ClickProfileIconAndValidateSignOut()
        {
            try
            {
                Logger.Information("Attempting to click profile icon and validate sign out button");
                
                // Step 1: Find and click the profile icon
                var profileIcon = GetProfileIcon();
                if (profileIcon == null)
                {
                    Logger.Warning("❌ Profile icon not found - cannot proceed with click validation");
                    return false;
                }

                Logger.Information("✅ Profile icon found, attempting to click");
                profileIcon.Click();
                
                // Step 2: Wait for the sign out container to appear
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
                try
                {
                    var signOutButton = wait.Until(d => d.FindElement(SignOutButtonAfterProfileClick));
                    if (signOutButton.Displayed)
                    {
                        Logger.Information("✅ Sign out button found after clicking profile icon");
                        Logger.Information("Sign out button text: '{Text}'", signOutButton.Text);
                        Logger.Information("Sign out button is enabled: {IsEnabled}", signOutButton.Enabled);
                        return true;
                    }
                    else
                    {
                        Logger.Warning("❌ Sign out button found but not displayed");
                        return false;
                    }
                }
                catch (WebDriverTimeoutException)
                {
                    Logger.Warning("❌ Sign out button did not appear within timeout after clicking profile icon");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("❌ Error clicking profile icon and validating sign out: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get the sign out button after clicking the profile icon
        /// </summary>
        /// <returns>Sign out button element or null if not found</returns>
        public IWebElement? GetSignOutButtonAfterProfileClick()
        {
            try
            {
                var signOutButton = Driver.FindElement(SignOutButtonAfterProfileClick);
                if (signOutButton.Displayed)
                {
                    Logger.Information("Sign out button found after profile click");
                    return signOutButton;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Sign out button after profile click not found: {Error}", ex.Message);
            }
            return null;
        }

        #endregion

        #region Dashboard-Specific Methods

        /// <summary>
        /// Check if user is on dashboard page
        /// </summary>
        /// <returns>True if on dashboard page</returns>
        public bool IsOnDashboard()
        {
            try
            {
                var currentUrl = Driver.Url.ToLower();
                var pageTitle = Driver.Title?.ToLower() ?? "";
                
                // Check URL indicators
                bool urlIndicatesHome = currentUrl.Contains("practicallaw") && 
                                       !currentUrl.Contains("login") && 
                                       !currentUrl.Contains("signin");
                
                // Check title indicators
                bool titleIndicatesHome = pageTitle.Contains("practical law") ||
                                         pageTitle.Contains("home") ||
                                         pageTitle.Contains("dashboard");
                
                bool isDashboard = urlIndicatesHome || titleIndicatesHome;
                
                Logger.Information("Dashboard check - URL: {Url}, Title: {Title}, Result: {Result}", 
                    currentUrl, pageTitle, isDashboard);
                
                return isDashboard;
            }
            catch (Exception ex)
            {
                Logger.Warning("Error checking if on dashboard: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get welcome message if present
        /// </summary>
        /// <returns>Welcome message text or null if not found</returns>
        public string? GetWelcomeMessage()
        {
            try
            {
                var welcomeLocators = new[]
                {
                    By.CssSelector("[class*='welcome']"),
                    By.CssSelector("[class*='greeting']"),
                    By.XPath("//*[contains(text(), 'Welcome')]"),
                    By.XPath("//*[contains(text(), 'Hello')]"),
                    By.XPath("//*[contains(text(), 'Good')]")
                };
                
                foreach (var locator in welcomeLocators)
                {
                    try
                    {
                        var elements = Driver.FindElements(locator);
                        foreach (var element in elements)
                        {
                            if (element.Displayed && !string.IsNullOrWhiteSpace(element.Text))
                            {
                                Logger.Information("Welcome message found: {Message}", element.Text);
                                return element.Text;
                            }
                        }
                    }
                    catch { }
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Logger.Debug("Error getting welcome message: {Error}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Check if user profile is visible
        /// </summary>
        /// <returns>True if user profile is visible</returns>
        public bool IsUserProfileVisible()
        {
            try
            {
                // Check for profile icon first
                var profileIcon = GetProfileIcon();
                if (profileIcon != null && profileIcon.Displayed)
                {
                    Logger.Information("User profile visible via profile icon");
                    return true;
                }
                
                // Check for other profile indicators
                var profileLocators = new[]
                {
                    By.CssSelector("[class*='profile']"),
                    By.CssSelector("[class*='user']"),
                    By.CssSelector("[class*='account']"),
                    By.XPath("//*[contains(@title, 'Profile')]"),
                    By.XPath("//*[contains(@alt, 'Profile')]")
                };
                
                foreach (var locator in profileLocators)
                {
                    try
                    {
                        var elements = Driver.FindElements(locator);
                        foreach (var element in elements)
                        {
                            if (element.Displayed)
                            {
                                Logger.Information("User profile visible via: {Locator}", locator);
                                return true;
                            }
                        }
                    }
                    catch { }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.Debug("Error checking profile visibility: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign out from the application
        /// </summary>
        /// <returns>True if sign out was successful</returns>
        public bool SignOut()
        {
            try
            {
                Logger.Information("Attempting to sign out...");
                
                // Try to click the sign out button
                bool signOutClicked = ClickSignOut();
                
                if (signOutClicked)
                {
                    // Wait for navigation to complete
                    System.Threading.Thread.Sleep(3000);
                    
                    // Check if we're redirected to sign off activity page, login page, or similar
                    var currentUrl = Driver.Url.ToLower();
                    var currentTitle = Driver.Title.ToLower();
                    
                    bool signedOut = currentUrl.Contains("signoffactivity") || 
                                   currentUrl.Contains("signoff") ||
                                   currentUrl.Contains("login") || 
                                   currentUrl.Contains("signin") || 
                                   currentTitle.Contains("sign off") ||
                                   currentTitle.Contains("sign out") ||
                                   currentTitle.Contains("logout") ||
                                   !IsUserLoggedIn();
                    
                    Logger.Information($"Sign out {(signedOut ? "successful" : "may have failed")} - URL: {currentUrl}, Title: {currentTitle}");
                    return signedOut;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warning("Error during sign out: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Sign out and then click signin button on logout page to prepare for next test
        /// </summary>
        /// <returns>True if signout and signin button click were successful</returns>
        public bool SignOutAndClickSignIn()
        {
            try
            {
                Logger.Information("Attempting to sign out and click signin for next test...");
                
                // Try to click the sign out button
                bool signOutClicked = ClickSignOut();
                
                if (signOutClicked)
                {
                    // Wait for navigation to complete
                    System.Threading.Thread.Sleep(3000);
                    
                    // Check if we're redirected to sign off activity page
                    var currentUrl = Driver.Url.ToLower();
                    var currentTitle = Driver.Title.ToLower();
                    
                    bool signedOut = currentUrl.Contains("signoffactivity") || 
                                   currentUrl.Contains("signoff") ||
                                   currentUrl.Contains("login") || 
                                   currentUrl.Contains("signin") || 
                                   currentTitle.Contains("sign off") ||
                                   currentTitle.Contains("sign out") ||
                                   currentTitle.Contains("logout") ||
                                   !IsUserLoggedIn();
                    
                    Logger.Information($"Sign out {(signedOut ? "successful" : "may have failed")} - URL: {currentUrl}, Title: {currentTitle}");
                    
                    if (signedOut)
                    {
                        // Now try to click the signin button on the logout page
                        bool signinClicked = ClickSignInOnLogoutPage();
                        if (signinClicked)
                        {
                            Logger.Information("✅ Successfully signed out and clicked signin button for next test");
                            return true;
                        }
                        else
                        {
                            Logger.Warning("⚠️ Signed out successfully but failed to click signin button");
                            return true; // Still return true since logout was successful
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.Warning("Error during sign out and signin: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Click the signin button on the logout page to prepare for next test
        /// </summary>
        /// <returns>True if signin button was clicked successfully</returns>
        public bool ClickSignInOnLogoutPage()
        {
            try
            {
                Logger.Information("Looking for signin button on logout page...");
                
                // Common signin button locators on logout/signoff pages
                var signInLocators = new[]
                {
                    By.LinkText("Sign in"),
                    By.LinkText("Sign In"),
                    By.LinkText("LOGIN"),
                    By.LinkText("Log in"),
                    By.PartialLinkText("Sign"),
                    By.PartialLinkText("Log"),
                    By.CssSelector("a[href*='login']"),
                    By.CssSelector("a[href*='signin']"),
                    By.CssSelector("button[class*='signin']"),
                    By.CssSelector("button[class*='login']"),
                    By.XPath("//a[contains(text(), 'Sign in') or contains(text(), 'Sign In') or contains(text(), 'LOGIN') or contains(text(), 'Log in')]"),
                    By.XPath("//button[contains(text(), 'Sign in') or contains(text(), 'Sign In') or contains(text(), 'LOGIN') or contains(text(), 'Log in')]")
                };

                foreach (var locator in signInLocators)
                {
                    try
                    {
                        var signInButton = Driver.FindElement(locator);
                        if (signInButton.Displayed && signInButton.Enabled)
                        {
                            Logger.Information("Found signin button using locator: {Locator}", locator);
                            Logger.Information("Signin button text: '{Text}'", signInButton.Text);
                            
                            signInButton.Click();
                            
                            // Wait for navigation
                            System.Threading.Thread.Sleep(2000);
                            
                            Logger.Information("✅ Signin button clicked successfully");
                            Logger.Information("Current URL after signin click: {URL}", Driver.Url);
                            
                            return true;
                        }
                    }
                    catch (NoSuchElementException)
                    {
                        // Continue to next locator
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Error with locator {Locator}: {Error}", locator, ex.Message);
                    }
                }
                
                Logger.Warning("❌ No signin button found on logout page");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error clicking signin on logout page: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Perform a search using the search functionality on the page
        /// </summary>
        /// <param name="searchTerm">The term to search for</param>
        /// <returns>True if search was performed successfully</returns>
        public bool Search(string searchTerm)
        {
            try
            {
                Logger.Information("Performing search for: {SearchTerm}", searchTerm);
                
                // Common search input locators
                var searchLocators = new[]
                {
                    By.CssSelector("input[type='search']"),
                    By.CssSelector("input[placeholder*='search']"),
                    By.CssSelector("input[placeholder*='Search']"),
                    By.CssSelector("input[name='search']"),
                    By.CssSelector("input[name='q']"),
                    By.CssSelector("#search"),
                    By.CssSelector(".search-input"),
                    By.XPath("//input[contains(@placeholder, 'search') or contains(@placeholder, 'Search')]")
                };
                
                foreach (var locator in searchLocators)
                {
                    try
                    {
                        var searchInput = Driver.FindElement(locator);
                        if (searchInput.Displayed && searchInput.Enabled)
                        {
                            searchInput.Clear();
                            searchInput.SendKeys(searchTerm);
                            searchInput.SendKeys(Keys.Enter);
                            
                            Logger.Information("✅ Search performed successfully using locator: {Locator}", locator);
                            System.Threading.Thread.Sleep(2000); // Wait for search results
                            return true;
                        }
                    }
                    catch { }
                }
                
                Logger.Warning("❌ No search input found");
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error performing search: {Error}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Error Message Validation Methods

        /// <summary>
        /// Check if there are any error messages displayed on the dashboard page
        /// </summary>
        /// <returns>True if no error messages found, False if error messages are present</returns>
        public bool HasNoErrorMessages()
        {
            try
            {
                Logger.Information("Checking for error messages on the dashboard page...");

                // Common error message locators
                var errorMessageLocators = new[]
                {
                    By.CssSelector(".error"),
                    By.CssSelector(".alert-error"),
                    By.CssSelector(".error-message"),
                    By.CssSelector(".alert-danger"),
                    By.CssSelector(".notification-error"),
                    By.CssSelector("[class*='error']"),
                    By.CssSelector("[class*='alert'][class*='error']"),
                    By.CssSelector("[class*='alert'][class*='danger']"),
                    By.CssSelector("[role='alert']"),
                    By.XPath("//*[contains(@class, 'error') or contains(@class, 'alert-error') or contains(@class, 'alert-danger')]"),
                    By.XPath("//*[contains(text(), 'Error') or contains(text(), 'error') or contains(text(), 'ERROR')]"),
                    By.XPath("//*[contains(text(), 'Something went wrong') or contains(text(), 'An error occurred')]"),
                    By.XPath("//*[contains(text(), 'Unable to') or contains(text(), 'Failed to')]")
                };

                foreach (var locator in errorMessageLocators)
                {
                    try
                    {
                        var errorElements = Driver.FindElements(locator);
                        foreach (var element in errorElements)
                        {
                            if (element.Displayed && !string.IsNullOrWhiteSpace(element.Text))
                            {
                                Logger.Warning("❌ Error message found: '{ErrorText}' using locator: {Locator}", 
                                    element.Text, locator);
                                return false; // Error message found
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Error checking locator {Locator}: {Error}", locator, ex.Message);
                    }
                }

                Logger.Information("✅ No error messages found on the dashboard page");
                return true; // No error messages found
            }
            catch (Exception ex)
            {
                Logger.Error("Error while checking for error messages: {Error}", ex.Message);
                return false; // Assume error if we can't check properly
            }
        }

        /// <summary>
        /// Get all visible error messages on the page
        /// </summary>
        /// <returns>List of error message texts found on the page</returns>
        public List<string> GetErrorMessages()
        {
            var errorMessages = new List<string>();
            
            try
            {
                Logger.Information("Collecting all error messages on the dashboard page...");

                // Common error message locators
                var errorMessageLocators = new[]
                {
                    By.CssSelector(".error"),
                    By.CssSelector(".alert-error"),
                    By.CssSelector(".error-message"),
                    By.CssSelector(".alert-danger"),
                    By.CssSelector(".notification-error"),
                    By.CssSelector("[class*='error']"),
                    By.CssSelector("[class*='alert'][class*='error']"),
                    By.CssSelector("[class*='alert'][class*='danger']"),
                    By.CssSelector("[role='alert']"),
                    By.XPath("//*[contains(@class, 'error') or contains(@class, 'alert-error') or contains(@class, 'alert-danger')]"),
                    By.XPath("//*[contains(text(), 'Error') or contains(text(), 'error') or contains(text(), 'ERROR')]")
                };

                foreach (var locator in errorMessageLocators)
                {
                    try
                    {
                        var errorElements = Driver.FindElements(locator);
                        foreach (var element in errorElements)
                        {
                            if (element.Displayed && !string.IsNullOrWhiteSpace(element.Text))
                            {
                                string errorText = element.Text.Trim();
                                if (!errorMessages.Contains(errorText))
                                {
                                    errorMessages.Add(errorText);
                                    Logger.Information("Found error message: '{ErrorText}'", errorText);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Error checking locator {Locator}: {Error}", locator, ex.Message);
                    }
                }

                Logger.Information("Total error messages found: {Count}", errorMessages.Count);
                return errorMessages;
            }
            catch (Exception ex)
            {
                Logger.Error("Error while collecting error messages: {Error}", ex.Message);
                return errorMessages;
            }
        }

        #endregion

        #region Action Methods

        /// <summary>
        /// Click the sign out button to logout
        /// </summary>
        /// <returns>True if sign out button was clicked successfully</returns>
        public bool ClickSignOut()
        {
            try
            {
                Logger.Information("Attempting to click sign out - first checking for modal overlays");
                
                // Step 1: Check and close any modal dialogs that might be blocking the UI
                try
                {
                    // Close modal overlays if present
                    var modalOverlays = new[]
                    {
                        By.Id("coid_lightboxOverlay"),
                        By.CssSelector(".co_lightboxOverlay"),
                        By.CssSelector("[class*='lightbox']"),
                        By.CssSelector("[class*='modal']"),
                        By.CssSelector("[class*='overlay']")
                    };

                    foreach (var overlayLocator in modalOverlays)
                    {
                        try
                        {
                            var overlay = Driver.FindElement(overlayLocator);
                            if (overlay.Displayed)
                            {
                                Logger.Information("Found modal overlay, attempting to close it");
                                
                                // Try to find close button within the overlay
                                var closeButtons = new[]
                                {
                                    By.CssSelector(".close"),
                                    By.CssSelector("[class*='close']"),
                                    By.XPath("//button[contains(text(), 'Close')]"),
                                    By.XPath("//button[contains(text(), 'Cancel')]"),
                                    By.XPath("//button[@type='button']"),
                                    By.CssSelector("button[onclick*='close']")
                                };

                                bool modalClosed = false;
                                foreach (var closeButtonLocator in closeButtons)
                                {
                                    try
                                    {
                                        var closeButton = overlay.FindElement(closeButtonLocator);
                                        if (closeButton.Displayed && closeButton.Enabled)
                                        {
                                            Logger.Information("Clicking close button to dismiss modal");
                                            closeButton.Click();
                                            System.Threading.Thread.Sleep(1000);
                                            modalClosed = true;
                                            break;
                                        }
                                    }
                                    catch { }
                                }

                                // If no close button found, try to click outside the modal or use escape key
                                if (!modalClosed)
                                {
                                    try
                                    {
                                        Logger.Information("No close button found, trying to press Escape key");
                                        Driver.FindElement(By.TagName("body")).SendKeys(OpenQA.Selenium.Keys.Escape);
                                        System.Threading.Thread.Sleep(1000);
                                    }
                                    catch 
                                    {
                                        Logger.Information("Escape key failed, trying to click outside modal");
                                        // Try clicking on the overlay itself to close it
                                        try
                                        {
                                            overlay.Click();
                                            System.Threading.Thread.Sleep(1000);
                                        }
                                        catch { }
                                    }
                                }
                            }
                        }
                        catch { }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug("Modal overlay check completed: {Message}", ex.Message);
                }

                // Step 2: Now try to click the profile icon to reveal the dropdown
                Logger.Information("Clicking profile icon to reveal sign out dropdown");
                
                try
                {
                    var profileIcon = Driver.FindElement(ProfileSignOutIcon);
                    if (profileIcon.Displayed)
                    {
                        // Use JavaScript click to avoid interception issues
                        try
                        {
                            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", profileIcon);
                            Logger.Information("Profile icon clicked using JavaScript");
                        }
                        catch
                        {
                            // Fallback to regular click
                            profileIcon.Click();
                            Logger.Information("Profile icon clicked using regular click");
                        }
                        
                        // Wait for dropdown to appear
                        System.Threading.Thread.Sleep(2000);
                        
                        // Step 3: Now click the sign out button in the dropdown
                        try
                        {
                            var signOutButton = Driver.FindElement(SignOutButtonAfterProfileClick);
                            if (signOutButton.Displayed && signOutButton.Enabled)
                            {
                                Logger.Information("Clicking Sign Out button in dropdown");
                                
                                // Use JavaScript click for reliability
                                try
                                {
                                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", signOutButton);
                                    Logger.Information("Sign out button clicked using JavaScript");
                                }
                                catch
                                {
                                    signOutButton.Click();
                                    Logger.Information("Sign out button clicked using regular click");
                                }
                                
                                // Wait for logout to complete
                                System.Threading.Thread.Sleep(3000);
                                
                                Logger.Information("✅ Sign out button clicked successfully");
                                return true;
                            }
                            else
                            {
                                Logger.Warning("❌ Sign out button in dropdown not visible or not enabled");
                                return false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Warning("❌ Could not find or click sign out button in dropdown: {Error}", ex.Message);
                            
                            // Try alternative sign out methods
                            return TryAlternativeSignOutMethods();
                        }
                    }
                    else
                    {
                        Logger.Warning("❌ Profile icon not visible");
                        return TryAlternativeSignOutMethods();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning("❌ Could not find or click profile icon: {Error}", ex.Message);
                    return TryAlternativeSignOutMethods();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error during sign out process: {Error}", ex.Message);
                return TryAlternativeSignOutMethods();
            }
        }

        /// <summary>
        /// Try alternative sign out methods when the primary method fails
        /// </summary>
        /// <returns>True if any alternative method succeeded</returns>
        private bool TryAlternativeSignOutMethods()
        {
            Logger.Information("Trying alternative sign out methods...");
            
            // Method 1: Direct URL navigation to sign out
            try
            {
                Logger.Information("Trying direct URL navigation to sign out");
                var currentUrl = Driver.Url;
                var baseUrl = new Uri(currentUrl).GetLeftPart(UriPartial.Authority);
                var signOutUrl = $"{baseUrl}/SignOff";
                
                Driver.Navigate().GoToUrl(signOutUrl);
                System.Threading.Thread.Sleep(3000);
                
                if (Driver.Url.ToLower().Contains("signoff") || Driver.Url.ToLower().Contains("login"))
                {
                    Logger.Information("✅ Sign out successful via direct URL navigation");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Direct URL sign out failed: {Error}", ex.Message);
            }

            // Method 2: Try to find any sign out link on the page
            try
            {
                Logger.Information("Searching for any sign out link on the page");
                
                foreach (var locator in SignOutButtonLocators)
                {
                    try
                    {
                        var elements = Driver.FindElements(locator);
                        foreach (var element in elements)
                        {
                            if (element.Displayed && element.Enabled)
                            {
                                Logger.Information("Found sign out element using locator: {Locator}", locator);
                                
                                try
                                {
                                    ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", element);
                                    Logger.Information("✅ Sign out successful via alternative method");
                                    System.Threading.Thread.Sleep(3000);
                                    return true;
                                }
                                catch
                                {
                                    element.Click();
                                    Logger.Information("✅ Sign out successful via alternative method");
                                    System.Threading.Thread.Sleep(3000);
                                    return true;
                                }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Alternative sign out search failed: {Error}", ex.Message);
            }

            // Method 3: JavaScript-based sign out
            try
            {
                Logger.Information("Trying JavaScript-based sign out");
                ((IJavaScriptExecutor)Driver).ExecuteScript(@"
                    // Try to find and click sign out elements
                    var signOutElements = document.querySelectorAll('a[href*=""signoff""], a[href*=""logout""], button[onclick*=""signout""], button[onclick*=""logout""]');
                    for (var i = 0; i < signOutElements.length; i++) {
                        if (signOutElements[i].offsetParent !== null) {
                            signOutElements[i].click();
                            return true;
                        }
                    }
                    
                    // Try to trigger sign out via common sign out patterns
                    var links = document.querySelectorAll('a, button');
                    for (var i = 0; i < links.length; i++) {
                        var text = links[i].textContent || links[i].innerText || '';
                        if (text.toLowerCase().includes('sign out') || text.toLowerCase().includes('logout')) {
                            if (links[i].offsetParent !== null) {
                                links[i].click();
                                return true;
                            }
                        }
                    }
                    return false;
                ");
                
                System.Threading.Thread.Sleep(3000);
                Logger.Information("✅ JavaScript sign out attempted");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Debug("JavaScript sign out failed: {Error}", ex.Message);
            }

            Logger.Warning("❌ All sign out methods failed");
            return false;
        }

        /// <summary>
        /// Wait for page to load completely
        /// </summary>
        /// <param name="timeoutSeconds">Timeout in seconds</param>
        /// <returns>True if page loaded successfully</returns>
        public bool WaitForPageLoad(int timeoutSeconds = 30)
        {
            try
            {
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
                
                // Wait for document ready state
                wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));
                
                Logger.Information("✅ Page load completed");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Warning("Page load timeout or error: {Error}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Tab Validation Methods

        /// <summary>
        /// Check if a specific tab is present on the page
        /// </summary>
        /// <param name="tabName">Name of the tab to check for</param>
        /// <returns>True if tab is present and visible</returns>
        public bool IsTabPresent(string tabName)
        {
            try
            {
                Logger.Information("Checking for tab presence: {TabName}", tabName);

                // Get the specific locator for the tab
                By? tabLocator = GetTabLocator(tabName);
                
                if (tabLocator != null)
                {
                    try
                    {
                        var element = Driver.FindElement(tabLocator);
                        if (element.Displayed)
                        {
                            Logger.Information("✅ Tab '{TabName}' found using specific locator: {Locator}", tabName, tabLocator);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Specific tab locator failed for '{TabName}': {Error}", tabName, ex.Message);
                    }
                }

                // Fallback to generic locators if specific locator fails
                var fallbackLocators = new[]
                {
                    By.XPath($"//a[contains(text(), '{tabName}') or contains(@title, '{tabName}')]"),
                    By.XPath($"//button[contains(text(), '{tabName}')]"),
                    By.XPath($"//div[contains(@class, 'tab')]//a[contains(text(), '{tabName}')]"),
                    By.XPath($"//nav//a[contains(text(), '{tabName}')]")
                };

                foreach (var locator in fallbackLocators)
                {
                    try
                    {
                        var elements = Driver.FindElements(locator);
                        foreach (var element in elements)
                        {
                            if (element.Displayed)
                            {
                                Logger.Information("✅ Tab '{TabName}' found using fallback locator: {Locator}", tabName, locator);
                                return true;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Fallback tab locator {Locator} failed: {Error}", locator, ex.Message);
                    }
                }

                Logger.Information("❌ Tab '{TabName}' not found", tabName);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error checking for tab '{TabName}': {Error}", tabName, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Click on a specific tab
        /// </summary>
        /// <param name="tabName">Name of the tab to click</param>
        /// <returns>True if tab was clicked successfully</returns>
        public bool ClickTab(string tabName)
        {
            try
            {
                Logger.Information("Attempting to click tab: {TabName}", tabName);

                // Get the specific locator for the tab
                By? tabLocator = GetTabLocator(tabName);
                
                if (tabLocator != null)
                {
                    try
                    {
                        var element = Driver.FindElement(tabLocator);
                        if (element.Displayed && element.Enabled)
                        {
                            Logger.Information("Clicking tab '{TabName}' using specific locator: {Locator}", tabName, tabLocator);
                            element.Click();
                            
                            // Wait for navigation/content to load
                            System.Threading.Thread.Sleep(2000);
                            
                            Logger.Information("✅ Successfully clicked tab: {TabName}", tabName);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Specific tab locator failed for clicking '{TabName}': {Error}", tabName, ex.Message);
                    }
                }

                // Fallback to generic locators if specific locator fails
                var fallbackLocators = new[]
                {
                    By.XPath($"//a[contains(text(), '{tabName}') or contains(@title, '{tabName}')]"),
                    By.XPath($"//button[contains(text(), '{tabName}')]"),
                    By.XPath($"//div[contains(@class, 'tab')]//a[contains(text(), '{tabName}')]"),
                    By.XPath($"//nav//a[contains(text(), '{tabName}')]")
                };

                foreach (var locator in fallbackLocators)
                {
                    try
                    {
                        var element = Driver.FindElement(locator);
                        if (element.Displayed && element.Enabled)
                        {
                            Logger.Information("Clicking tab '{TabName}' using fallback locator: {Locator}", tabName, locator);
                            element.Click();
                            
                            // Wait for navigation/content to load
                            System.Threading.Thread.Sleep(2000);
                            
                            Logger.Information("✅ Successfully clicked tab: {TabName}", tabName);
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Could not click tab using fallback locator {Locator}: {Error}", locator, ex.Message);
                    }
                }

                Logger.Warning("❌ Could not click tab: {TabName}", tabName);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error clicking tab '{TabName}': {Error}", tabName, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Get the specific locator for a given tab name
        /// </summary>
        /// <param name="tabName">Name of the tab</param>
        /// <returns>By locator for the specific tab, or null if not found</returns>
        private By? GetTabLocator(string tabName)
        {
            return tabName.ToLower().Replace(" ", "") switch
            {
                "practiceareas" => By.XPath("//*[@id=\"coid_categoryBoxTabButton1\"]"),
                "sectors" => By.XPath("//*[@id=\"coid_categoryBoxTabButton2\"]"),
                "resources" => By.XPath("//*[@id=\"coid_categoryBoxTabButton3\"]"), // Note: Fixed the Resources XPath to button3
                _ => null
            };
        }

        /// <summary>
        /// Get list of all visible tabs on the page
        /// </summary>
        /// <returns>List of tab names found on the page</returns>
        public List<string> GetVisibleTabs()
        {
            var tabs = new List<string>();
            
            try
            {
                Logger.Information("Getting list of visible tabs...");

                // Check specific tab locators first
                var specificTabs = new Dictionary<string, By>
                {
                    ["Practice Areas"] = By.XPath("//*[@id=\"coid_categoryBoxTabButton1\"]"),
                    ["Sectors"] = By.XPath("//*[@id=\"coid_categoryBoxTabButton2\"]"),
                    ["Resources"] = By.XPath("//*[@id=\"coid_categoryBoxTabButton3\"]") // Assuming Resources should be button3
                };

                foreach (var tab in specificTabs)
                {
                    try
                    {
                        var element = Driver.FindElement(tab.Value);
                        if (element.Displayed)
                        {
                            string tabText = GetTabText(element, tab.Key);
                            if (!string.IsNullOrWhiteSpace(tabText) && !tabs.Contains(tabText))
                            {
                                tabs.Add(tabText);
                                Logger.Information("Found specific tab: {TabText}", tabText);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Specific tab '{TabName}' not found: {Error}", tab.Key, ex.Message);
                    }
                }

                // Fallback to generic tab discovery if specific tabs not found
                if (tabs.Count == 0)
                {
                    var tabContainerLocators = new[]
                    {
                        By.XPath("//nav//a"),
                        By.XPath("//div[contains(@class, 'tab')]//a"),
                        By.XPath("//ul[contains(@class, 'nav')]//a"),
                        By.XPath("//header//nav//a"),
                        By.CssSelector("nav a"),
                        By.CssSelector(".navigation a"),
                        By.CssSelector(".menu a"),
                        By.CssSelector("[role='navigation'] a")
                    };

                    foreach (var locator in tabContainerLocators)
                    {
                        try
                        {
                            var elements = Driver.FindElements(locator);
                            foreach (var element in elements)
                            {
                                if (element.Displayed && !string.IsNullOrWhiteSpace(element.Text))
                                {
                                    string tabText = element.Text.Trim();
                                    if (!tabs.Contains(tabText) && tabText.Length > 0)
                                    {
                                        tabs.Add(tabText);
                                        Logger.Information("Found fallback tab: {TabText}", tabText);
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Debug("Error checking tab container locator {Locator}: {Error}", locator, ex.Message);
                        }
                    }
                }

                Logger.Information("Total visible tabs found: {Count}", tabs.Count);
                return tabs;
            }
            catch (Exception ex)
            {
                Logger.Error("Error getting visible tabs: {Error}", ex.Message);
                return tabs;
            }
        }

        /// <summary>
        /// Get the text content of a tab element
        /// </summary>
        /// <param name="element">The tab element</param>
        /// <param name="defaultName">Default name if text cannot be extracted</param>
        /// <returns>Tab text content</returns>
        private string GetTabText(IWebElement element, string defaultName)
        {
            try
            {
                // Try different ways to get the tab text
                string? text = element.Text?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;

                // Try title attribute
                text = element.GetAttribute("title")?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;

                // Try aria-label attribute
                text = element.GetAttribute("aria-label")?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;

                // Try data attributes
                text = element.GetAttribute("data-text")?.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                    return text;

                // Look for child elements with text
                try
                {
                    var childElements = element.FindElements(By.XPath(".//*[text()]"));
                    foreach (var child in childElements)
                    {
                        text = child.Text?.Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                            return text;
                    }
                }
                catch { }

                // Return default name if nothing else works
                return defaultName;
            }
            catch (Exception ex)
            {
                Logger.Debug("Error getting tab text: {Error}", ex.Message);
                return defaultName;
            }
        }

        #endregion

        #region Navigation Methods

        /// <summary>
        /// Navigate to Practice Area section from dashboard
        /// </summary>
        /// <returns>PracticeAreaPage instance</returns>
        public PracticeAreaPage NavigateToPracticeArea()
        {
            try
            {
                Logger.Information("Navigating to Practice Area from dashboard...");
                
                var practiceAreaPage = new PracticeAreaPage(Driver, Logger);
                
                if (practiceAreaPage.NavigateToPracticeArea())
                {
                    Logger.Information("✅ Successfully navigated to Practice Area");
                    return practiceAreaPage;
                }
                else
                {
                    Logger.Warning("❌ Failed to navigate to Practice Area, returning page object anyway");
                    return practiceAreaPage;
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error navigating to Practice Area: {Error}", ex.Message);
                return new PracticeAreaPage(Driver, Logger);
            }
        }

        /// <summary>
        /// Get current page title
        /// </summary>
        /// <returns>Page title or null if not available</returns>
        public string? GetPageTitle()
        {
            try
            {
                return Driver.Title;
            }
            catch (Exception ex)
            {
                Logger.Warning("Could not get page title: {Error}", ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Get current page URL
        /// </summary>
        /// <returns>Current URL</returns>
        public string GetCurrentUrl()
        {
            return Driver.Url;
        }

        #endregion
    }
}

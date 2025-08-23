using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumPL.Core;
using Serilog;
using System;
using System.Linq;

namespace SeleniumPL.Tests.Pages
{
    /// <summary>
    /// Page Object Model for Practical Law Home Page
    /// Handles cookie consent, navigation, and initial login link interactions
    /// </summary>
    public class PracticalLawHomePagePOM : BasePage
    {
        #region Locators
        
        // Cookie consent locators - ordered by specificity to avoid clicking privacy policy links
        private readonly By[] CookieConsentButtons = new[]
        {
            By.Id("onetrust-accept-btn-handler"),
            By.CssSelector(".ot-sdk-btn-primary"),
            By.CssSelector("button[id*='accept'][id*='all']"),
            By.CssSelector("button[class*='accept'][class*='all']"),
            By.CssSelector("#onetrust-button-group button[class*='primary']"),
            By.CssSelector(".onetrust-close-btn-handler")
        };

        // Privacy policy and reject button locators to avoid clicking
        private readonly By[] ElementsToAvoid = new[]
        {
            By.CssSelector("a[href*='privacy']"),
            By.CssSelector("a[href*='policy']"),
            By.CssSelector("button[contains(text(), 'Reject')]"),
            By.CssSelector("button[contains(text(), 'Decline')]"),
            By.CssSelector("button[contains(text(), 'Privacy')]"),
            By.CssSelector("a[contains(text(), 'Privacy')]"),
            By.CssSelector("a[contains(text(), 'Policy')]"),
            By.CssSelector("a[contains(text(), 'Learn More')]"),
            By.CssSelector("a[contains(text(), 'More Info')]"),
            By.CssSelector(".ot-sdk-btn-secondary"),
            By.CssSelector("button[class*='reject']"),
            By.CssSelector("button[class*='decline']")
        };

        // Sign in link locators
        private readonly By[] SignInLinkLocators = new[]
        {
            By.XPath("//*[@id=\"SignIn\"]"),
            By.Id("SignIn"),
            By.LinkText("Sign in"),
            By.PartialLinkText("Sign in"),
            By.LinkText("Login"),
            By.PartialLinkText("Login"),
            By.CssSelector("a[href*='login']"),
            By.CssSelector("a[href*='signin']"),
            By.CssSelector("a[href*='signon']"),
            By.XPath("//a[contains(text(), 'Sign in') or contains(text(), 'Login') or contains(text(), 'Sign')]")
        };

        // Page validation locators
        private readonly By[] PageLoadIndicators = new[]
        {
            By.TagName("body"),
            By.CssSelector("main"),
            By.CssSelector(".main-content"),
            By.CssSelector("#content")
        };

        #endregion

        public PracticalLawHomePagePOM(IWebDriver driver, ILogger? logger = null) : base(driver, logger)
        {
        }

        #region Page Actions

        /// <summary>
        /// Navigate to Practical Law home page
        /// </summary>
        /// <param name="url">Optional custom URL, defaults to UK Practical Law QED environment</param>
        public new PracticalLawHomePagePOM NavigateTo(string url = "https://uk.practicallaw.qed.thomsonreuters.com")
        {
            Logger.Information("Navigating to Practical Law: {Url}", url);
            Driver.Navigate().GoToUrl(url);
            WaitForPageToLoad();
            return this;
        }

        /// <summary>
        /// Handle cookie consent if present on the page, avoiding privacy policy links
        /// </summary>
        /// <returns>True if cookie consent was handled, false if not present</returns>
        public bool HandleCookieConsent()
        {
            Logger.Information("Checking for cookie consent dialog");
            
            try
            {
                // Wait for page to load
                System.Threading.Thread.Sleep(2000);
                
                foreach (var selector in CookieConsentButtons)
                {
                    try
                    {
                        var elements = Driver.FindElements(selector);
                        foreach (var element in elements)
                        {
                            if (element.Displayed && element.Enabled)
                            {
                                // Verify this is not a privacy policy or reject button
                                if (IsElementSafeToClick(element))
                                {
                                    Logger.Information("Found valid cookie consent element - Tag: {Tag}, Text: '{Text}', ID: '{Id}', Class: '{Class}'", 
                                        element.TagName, 
                                        element.Text, 
                                        element.GetAttribute("id") ?? "N/A", 
                                        element.GetAttribute("class") ?? "N/A");
                                    
                                    element.Click();
                                    Logger.Information("Cookie consent accepted using selector: {Selector}", selector);
                                    
                                    // Wait for overlay to disappear
                                    try
                                    {
                                        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
                                        wait.Until(d => d.FindElements(By.CssSelector(".onetrust-pc-dark-filter, .ot-fade-in")).All(e => !e.Displayed));
                                    }
                                    catch
                                    {
                                        System.Threading.Thread.Sleep(2000); // Fallback wait
                                    }
                                    
                                    return true;
                                }
                                else
                                {
                                    Logger.Information("Skipping element that appears to be privacy policy or reject button - Text: '{Text}', Href: '{Href}'", 
                                        element.Text, 
                                        element.GetAttribute("href") ?? "N/A");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Cookie selector {Selector} not found or not clickable: {Error}", selector, ex.Message);
                    }
                }
                
                Logger.Information("No valid cookie consent dialog found");

                // Extra: forcibly remove overlay if still present
                try
                {
                    var overlays = Driver.FindElements(By.CssSelector(".onetrust-pc-dark-filter, .ot-fade-in"));
                    if (overlays.Any(e => e.Displayed))
                    {
                        Logger.Warning("Cookie overlay still present, attempting to remove with JavaScript");
                        var js = (IJavaScriptExecutor)Driver;
                        js.ExecuteScript(@"
                            var overlays = document.querySelectorAll('.onetrust-pc-dark-filter, .ot-fade-in');
                            overlays.forEach(function(el) { el.parentNode.removeChild(el); });
                        ");
                        Logger.Information("Overlay forcibly removed via JavaScript");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to forcibly remove overlay: {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error("Error handling cookie consent: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Checks if an element is safe to click (not a privacy policy link or reject button)
        /// </summary>
        /// <param name="element">Element to check</param>
        /// <returns>True if safe to click, false otherwise</returns>
        private bool IsElementSafeToClick(IWebElement element)
        {
            try
            {
                string elementText = element.Text?.ToLower() ?? "";
                string elementHref = element.GetAttribute("href")?.ToLower() ?? "";
                string elementClass = element.GetAttribute("class")?.ToLower() ?? "";
                string elementId = element.GetAttribute("id")?.ToLower() ?? "";
                string tagName = element.TagName?.ToLower() ?? "";
                
                // Enhanced patterns to avoid clicking
                string[] unsafePatterns = {
                    "privacy", "policy", "reject", "decline", "learn more", "more info", 
                    "manage", "settings", "preferences", "customize", "options",
                    "cookies policy", "privacy notice", "data protection", "terms",
                    "legal", "notice", "details", "read more", "find out more"
                };
                
                // Immediately reject if it's clearly a link to another page
                if (tagName == "a" && !string.IsNullOrEmpty(elementHref) && 
                    !elementHref.Contains("javascript") && 
                    !elementHref.Contains("#") && 
                    elementHref.Length > 10)
                {
                    Logger.Debug("Element rejected - appears to be a navigation link: '{Href}'", elementHref);
                    return false;
                }
                
                // Check text content for unsafe patterns
                foreach (var pattern in unsafePatterns)
                {
                    if (elementText.Contains(pattern))
                    {
                        Logger.Debug("Element rejected due to text pattern '{Pattern}' in text '{Text}'", pattern, elementText);
                        return false;
                    }
                }
                
                // Check href attribute (for links)
                foreach (var pattern in unsafePatterns)
                {
                    if (elementHref.Contains(pattern))
                    {
                        Logger.Debug("Element rejected due to href pattern '{Pattern}' in href '{Href}'", pattern, elementHref);
                        return false;
                    }
                }
                
                // Check class attributes for unsafe patterns
                if (elementClass.Contains("reject") || elementClass.Contains("decline") || 
                    elementClass.Contains("secondary") || elementClass.Contains("policy") ||
                    elementClass.Contains("privacy"))
                {
                    Logger.Debug("Element rejected due to class '{Class}'", elementClass);
                    return false;
                }
                
                // Specifically check for the exact Accept All Cookies button first
                if (elementId == "onetrust-accept-btn-handler" || 
                    (elementText.Contains("accept all") && elementText.Contains("cookies")))
                {
                    Logger.Debug("Found specific 'Accept All Cookies' button - safe to click");
                    return true;
                }
                
                // Check for positive accept patterns
                string[] acceptPatterns = { "accept all", "accept", "agree", "ok", "continue", "allow", "yes" };
                bool hasAcceptPattern = false;
                
                foreach (var pattern in acceptPatterns)
                {
                    if (elementText.Contains(pattern) || 
                        elementClass.Contains(pattern) || 
                        elementId.Contains(pattern))
                    {
                        hasAcceptPattern = true;
                        break;
                    }
                }
                
                // If it's the onetrust primary button, it's usually safe
                if (elementId.Contains("onetrust") && (elementClass.Contains("primary") || elementId.Contains("accept")))
                {
                    return true;
                }
                
                // If it has accept patterns and is a button (not a link), it's likely safe
                if (hasAcceptPattern && tagName == "button")
                {
                    return true;
                }
                
                // If no clear accept pattern but it's a specific cookie button ID, allow it
                if ((elementId.Contains("onetrust") || elementId.Contains("cookie")) && tagName == "button")
                {
                    return true;
                }
                
                Logger.Debug("Element validation - Text: '{Text}', Tag: '{Tag}', HasAcceptPattern: {HasAccept}, Href: '{Href}'", 
                    elementText, tagName, hasAcceptPattern, elementHref);
                
                // Only allow if it's a button with accept pattern (not a link)
                return hasAcceptPattern && tagName == "button";
            }
            catch (Exception ex)
            {
                Logger.Debug("Error checking element safety: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Click the Sign In link to navigate to login page
        /// </summary>
        /// <returns>LoginPage object for method chaining</returns>
        public LoginPage ClickSignIn()
        {
            Logger.Information("Looking for Sign In link");
            
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10));
            var signInLink = wait.Until(d => 
            {
                foreach (var selector in SignInLinkLocators)
                {
                    try
                    {
                        var element = d.FindElement(selector);
                        if (element.Displayed && element.Enabled)
                        {
                            Logger.Information("Found sign in link using selector: {Selector}", selector);
                            return element;
                        }
                    }
                    catch { }
                }
                return null;
            });
            
            if (signInLink == null)
                throw new Exception("Sign in link not found on the page");
                
            signInLink.Click();
            Logger.Information("Clicked Sign In link");
            
            // Wait a moment for the page transition to begin
            System.Threading.Thread.Sleep(1000);
            
            return new LoginPage(Driver, Logger);
        }

        /// <summary>
        /// Check if user is already logged in by looking for sign out elements
        /// </summary>
        /// <returns>True if user appears to be logged in</returns>
        public bool IsUserLoggedIn()
        {
            var signOutSelectors = new[]
            {
                By.LinkText("Sign out"),
                By.PartialLinkText("Sign out"),
                By.LinkText("Logout"),
                By.PartialLinkText("Logout"),
                By.XPath("//a[contains(text(), 'Sign out') or contains(text(), 'Logout')]"),
                By.CssSelector("[href*='logout']"),
                By.CssSelector("[href*='signout']")
            };

            foreach (var selector in signOutSelectors)
            {
                try
                {
                    var elements = Driver.FindElements(selector);
                    if (elements.Any(e => e.Displayed))
                    {
                        Logger.Information("User appears to be logged in - found sign out element");
                        return true;
                    }
                }
                catch { }
            }
            
            Logger.Information("User does not appear to be logged in");
            return false;
        }

        #endregion

        #region Page Validations

        /// <summary>
        /// Wait for the page to load completely
        /// </summary>
        public new void WaitForPageToLoad()
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            
            try
            {
                wait.Until(d => 
                {
                    // Check if page is loaded by looking for common page elements
                    foreach (var indicator in PageLoadIndicators)
                    {
                        try
                        {
                            var element = d.FindElement(indicator);
                            if (element.Displayed)
                            {
                                return true;
                            }
                        }
                        catch { }
                    }
                    return false;
                });
                
                Logger.Information("Page loaded successfully");
            }
            catch (WebDriverTimeoutException)
            {
                Logger.Warning("Page load timeout - proceeding anyway");
            }
        }

        /// <summary>
        /// Validate that we're on the correct Practical Law page
        /// </summary>
        /// <returns>True if on Practical Law domain</returns>
        public bool IsOnPracticalLawPage()
        {
            var currentUrl = Driver.Url.ToLower();
            var isPracticalLawPage = currentUrl.Contains("practicallaw") || 
                                    currentUrl.Contains("westlaw.com") || 
                                    currentUrl.Contains("thomsonreuters.com");
            
            Logger.Information("Page validation - On Practical Law page: {IsValid}, URL: {Url}", 
                isPracticalLawPage, Driver.Url);
                
            return isPracticalLawPage;
        }

        #endregion
    }
}

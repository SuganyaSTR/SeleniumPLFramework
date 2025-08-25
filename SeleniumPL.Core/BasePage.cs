using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Serilog;
using System;

namespace SeleniumPL.Core
{
    public abstract class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WaitHelper Wait;
        protected readonly ILogger Logger;

        protected BasePage(IWebDriver driver, ILogger? logger = null)
        {
            Driver = driver;
            Logger = logger ?? Log.Logger;
            Wait = new WaitHelper(driver, 10, Logger);
        }

        // Page properties
        public string Title => Driver.Title;
        public string Url => Driver.Url;

        // Navigation methods
        public void NavigateTo(string url)
        {
            Logger.Information("Navigating to: {Url}", url);
            Driver.Navigate().GoToUrl(url);
        }

        public void Refresh()
        {
            Logger.Information("Refreshing page");
            Driver.Navigate().Refresh();
        }

        public void GoBack()
        {
            Logger.Information("Navigating back");
            Driver.Navigate().Back();
        }

        public void GoForward()
        {
            Logger.Information("Navigating forward");
            Driver.Navigate().Forward();
        }

        // Element interaction methods
        protected IWebElement FindElement(By locator)
        {
            try
            {
                var element = Driver.FindElement(locator);
                Logger.Debug("Found element: {Locator}", locator);
                return element;
            }
            catch (NoSuchElementException ex)
            {
                Logger.Error("Element not found: {Locator}", locator);
                throw new NoSuchElementException($"Element not found: {locator}", ex);
            }
        }

        protected IList<IWebElement> FindElements(By locator)
        {
            var elements = Driver.FindElements(locator);
            Logger.Debug("Found {Count} elements: {Locator}", elements.Count, locator);
            return elements;
        }

        protected void Click(By locator)
        {
            var element = Wait.WaitForElementToBeClickable(locator);
            element.Click();
            Logger.Information("Clicked element: {Locator}", locator);
        }

        protected void SendKeys(By locator, string text)
        {
            var element = Wait.WaitForElementToBeVisible(locator);
            element.Clear();
            element.SendKeys(text);
            Logger.Information("Entered text in element: {Locator}", locator);
        }

        /// <summary>
        /// Types text with human-like delays to avoid detection
        /// </summary>
        protected void TypeHumanLike(By locator, string text)
        {
            var element = Wait.WaitForElementToBeVisible(locator);
            element.Clear();
            
            // Add a small delay before starting to type
            Thread.Sleep(Random.Shared.Next(100, 300));
            
            foreach (char c in text)
            {
                element.SendKeys(c.ToString());
                // Random delay between keystrokes (50-150ms)
                Thread.Sleep(Random.Shared.Next(50, 150));
            }
            
            // Small delay after typing
            Thread.Sleep(Random.Shared.Next(200, 500));
            Logger.Information("Typed text human-like in element: {Locator}", locator);
        }

        /// <summary>
        /// Types text with human-like delays directly on an element
        /// </summary>
        protected void TypeHumanLike(IWebElement element, string text)
        {
            element.Clear();
            
            // Add a small delay before starting to type
            Thread.Sleep(Random.Shared.Next(100, 300));
            
            foreach (char c in text)
            {
                element.SendKeys(c.ToString());
                // Random delay between keystrokes (50-150ms)
                Thread.Sleep(Random.Shared.Next(50, 150));
            }
            
            // Small delay after typing
            Thread.Sleep(Random.Shared.Next(200, 500));
            Logger.Information("Typed text human-like in element");
        }

        /// <summary>
        /// Clicks an element with human-like delay
        /// </summary>
        protected void ClickHumanLike(By locator)
        {
            var element = Wait.WaitForElementToBeClickable(locator);
            
            // Small delay before clicking
            Thread.Sleep(Random.Shared.Next(100, 300));
            
            element.Click();
            
            // Small delay after clicking
            Thread.Sleep(Random.Shared.Next(200, 500));
            Logger.Information("Clicked element human-like: {Locator}", locator);
        }

        /// <summary>
        /// Clicks an element with human-like delay
        /// </summary>
        protected void ClickHumanLike(IWebElement element)
        {
            // Small delay before clicking
            Thread.Sleep(Random.Shared.Next(100, 300));
            
            element.Click();
            
            // Small delay after clicking
            Thread.Sleep(Random.Shared.Next(200, 500));
            Logger.Information("Clicked element human-like");
        }

        /// <summary>
        /// Check if current page is showing CIAM Access Denied error
        /// </summary>
        public bool IsCIAMAccessDeniedError()
        {
            try
            {
                var currentUrl = Driver.Url;
                
                // Check for CIAM error in URL
                if (currentUrl.Contains("GenericCosiErrorNoSession") || 
                    currentUrl.Contains("CIAMAccessDeniedException") ||
                    currentUrl.Contains("errorToken"))
                {
                    Logger.Warning("CIAM Access Denied error detected in URL: {Url}", currentUrl);
                    return true;
                }
                
                // Check for error text on page
                var pageSource = Driver.PageSource;
                if (pageSource.Contains("An error occurred while processing your information") ||
                    pageSource.Contains("CIAMAccessDeniedException") ||
                    pageSource.Contains("Please try again"))
                {
                    Logger.Warning("CIAM Access Denied error detected in page content");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                Logger.Debug("Error checking for CIAM error: {Message}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Handle CIAM Access Denied error by waiting and retrying
        /// </summary>
        public void HandleCIAMError()
        {
            if (IsCIAMAccessDeniedError())
            {
                Logger.Warning("CIAM Access Denied detected. Implementing recovery strategy...");
                
                // Clear cookies and cache
                try
                {
                    Driver.Manage().Cookies.DeleteAllCookies();
                    Logger.Information("Cleared cookies for CIAM error recovery");
                }
                catch (Exception ex)
                {
                    Logger.Warning("Failed to clear cookies: {Message}", ex.Message);
                }
                
                // Wait before retry (5-10 seconds)
                var waitTime = Random.Shared.Next(5000, 10000);
                Logger.Information("Waiting {WaitTime}ms before retry...", waitTime);
                Thread.Sleep(waitTime);
                
                // Navigate back to home page
                try
                {
                    Driver.Navigate().GoToUrl("https://uk.practicallaw.qed.thomsonreuters.com");
                    Thread.Sleep(3000);
                    Logger.Information("Navigated back to home page for recovery");
                }
                catch (Exception ex)
                {
                    Logger.Error("Failed to navigate back to home page: {Message}", ex.Message);
                    throw;
                }
            }
        }

        protected string GetText(By locator)
        {
            var element = Wait.WaitForElementToBeVisible(locator);
            var text = element.Text;
            Logger.Debug("Got text '{Text}' from element: {Locator}", text, locator);
            return text;
        }

        protected string GetAttribute(By locator, string attributeName)
        {
            var element = Wait.WaitForElementToBeVisible(locator);
            var value = element.GetAttribute(attributeName);
            Logger.Debug("Got attribute '{Attribute}' = '{Value}' from element: {Locator}", attributeName, value, locator);
            return value;
        }

        protected bool IsElementDisplayed(By locator)
        {
            try
            {
                var element = FindElement(locator);
                return element.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        protected bool IsElementEnabled(By locator)
        {
            try
            {
                var element = FindElement(locator);
                return element.Enabled;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        protected void SelectDropdownByText(By locator, string text)
        {
            var element = Wait.WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            select.SelectByText(text);
            Logger.Information("Selected dropdown option by text '{Text}': {Locator}", text, locator);
        }

        protected void SelectDropdownByValue(By locator, string value)
        {
            var element = Wait.WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            select.SelectByValue(value);
            Logger.Information("Selected dropdown option by value '{Value}': {Locator}", value, locator);
        }

        protected void SelectDropdownByIndex(By locator, int index)
        {
            var element = Wait.WaitForElementToBeVisible(locator);
            var select = new SelectElement(element);
            select.SelectByIndex(index);
            Logger.Information("Selected dropdown option by index {Index}: {Locator}", index, locator);
        }

        // JavaScript execution
        protected object ExecuteJavaScript(string script, params object[] args)
        {
            var jsExecutor = (IJavaScriptExecutor)Driver;
            var result = jsExecutor.ExecuteScript(script, args);
            Logger.Debug("Executed JavaScript: {Script}", script);
            return result;
        }

        protected void ScrollToElement(By locator)
        {
            var element = FindElement(locator);
            ExecuteJavaScript("arguments[0].scrollIntoView(true);", element);
            Logger.Information("Scrolled to element: {Locator}", locator);
        }

        protected void ScrollToTop()
        {
            ExecuteJavaScript("window.scrollTo(0, 0);");
            Logger.Information("Scrolled to top of page");
        }

        protected void ScrollToBottom()
        {
            ExecuteJavaScript("window.scrollTo(0, document.body.scrollHeight);");
            Logger.Information("Scrolled to bottom of page");
        }

        // Wait for page to be ready
        protected void WaitForPageToLoad()
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
            wait.Until(driver => ExecuteJavaScript("return document.readyState").Equals("complete"));
            Logger.Information("Page loaded completely");
        }

        // Screenshot functionality
        public byte[] TakeScreenshot()
        {
            var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
            Logger.Information("Screenshot taken");
            return screenshot.AsByteArray;
        }

        // Window/Tab management
        protected void SwitchToWindow(string windowHandle)
        {
            Driver.SwitchTo().Window(windowHandle);
            Logger.Information("Switched to window: {WindowHandle}", windowHandle);
        }

        protected void SwitchToFrame(By frameLocator)
        {
            var frameElement = Wait.WaitForElementToBeVisible(frameLocator);
            Driver.SwitchTo().Frame(frameElement);
            Logger.Information("Switched to frame: {Locator}", frameLocator);
        }

        protected void SwitchToDefaultContent()
        {
            Driver.SwitchTo().DefaultContent();
            Logger.Information("Switched to default content");
        }

        // Password dialog handler
        protected void HandlePasswordSaveDialog()
        {
            try
            {
                // Check for Chrome's password save dialog
                var passwordDialogSelectors = new[]
                {
                    "button[data-automation-id='neverButton']", // Chrome "Never" button
                    "button[data-automation-id='noButton']",     // Chrome "No" button  
                    "button:contains('Never')",                  // Generic Never button
                    "button:contains('No')",                     // Generic No button
                    "button:contains('Not now')",                // Generic Not now button
                    "[data-testid='password-save-never']",       // Alternative selector
                    ".save-password-prompt button:last-child"    // Last button in save password prompt
                };

                foreach (var selector in passwordDialogSelectors)
                {
                    try
                    {
                        var elements = Driver.FindElements(By.CssSelector(selector));
                        if (elements.Any() && elements.First().Displayed)
                        {
                            elements.First().Click();
                            Logger.Information("Dismissed password save dialog using selector: {Selector}", selector);
                            Thread.Sleep(500); // Brief wait after dismissing
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Debug("Selector {Selector} not found or not clickable: {Message}", selector, ex.Message);
                    }
                }

                // Try JavaScript approach to dismiss any password dialogs
                try
                {
                    ExecuteJavaScript(@"
                        // Find and click Never/No buttons in password dialogs
                        const buttons = document.querySelectorAll('button');
                        buttons.forEach(button => {
                            const text = button.textContent.toLowerCase();
                            if (text.includes('never') || text.includes('not now') || text.includes('no thanks')) {
                                button.click();
                            }
                        });
                    ");
                    Logger.Information("Attempted to dismiss password dialog via JavaScript");
                }
                catch (Exception ex)
                {
                    Logger.Debug("JavaScript password dialog dismissal failed: {Message}", ex.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.Debug("Password dialog handling completed with exception: {Message}", ex.Message);
            }
        }

        /// <summary>
        /// Enhanced method to handle cover page checkbox and text entry
        /// </summary>
        protected (bool CheckboxSelected, bool TextEntered) HandleCoverPageOptions(string coverPageText = "Test Cover Page Note - Automated Test")
        {
            Logger.Information("Handling cover page options: checkbox selection and text entry");
            
            bool checkboxSelected = false;
            bool textEntered = false;

            // Enhanced cover page checkbox locators
            var checkboxLocators = new[]
            {
                By.XPath("//input[@type='checkbox'][contains(@id, 'coverpage') or contains(@name, 'coverpage')]"),
                By.XPath("//input[@type='checkbox'][contains(@id, 'cover')]"),
                By.XPath("//input[@type='checkbox'][contains(@id, 'CoverPage')]"),
                By.XPath("//*[contains(text(), 'Cover page')]//input[@type='checkbox']"),
                By.XPath("//label[contains(text(), 'Cover page')]/input[@type='checkbox']"),
                By.XPath("//label[contains(text(), 'Cover page')]//input[@type='checkbox']"),
                By.XPath("//span[contains(text(), 'Cover page')]/..//input[@type='checkbox']"),
                By.XPath("//div[contains(text(), 'Cover page')]//input[@type='checkbox']"),
                By.CssSelector("input[type='checkbox'][id*='cover'], input[type='checkbox'][name*='cover']")
            };

            // Try to find and select cover page checkbox
            foreach (var locator in checkboxLocators)
            {
                try
                {
                    var checkbox = Wait.WaitForElementToBeClickable(locator, 3);
                    if (checkbox != null && checkbox.Displayed && checkbox.Enabled)
                    {
                        if (!checkbox.Selected)
                        {
                            try
                            {
                                checkbox.Click();
                            }
                            catch (Exception)
                            {
                                // Fallback to JavaScript click
                                ExecuteJavaScript("arguments[0].click();", checkbox);
                            }
                            Thread.Sleep(1000); // Wait for any dynamic content to load
                        }
                        Logger.Information("Successfully selected cover page checkbox using locator: {Locator}", locator);
                        checkboxSelected = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug("Cover page checkbox locator {Locator} failed: {Error}", locator, ex.Message);
                }
            }

            // Enhanced cover page text field locators
            var textLocators = new[]
            {
                By.XPath("//*[@id='coid_DdcLayoutCoverPageComment']"),
                By.XPath("//textarea[contains(@id, 'CoverPage')]"),
                By.XPath("//input[contains(@id, 'CoverPage')]"),
                By.XPath("//textarea[contains(@name, 'cover')]"),
                By.XPath("//textarea[contains(@id, 'coverpage')]"),
                By.XPath("//input[contains(@name, 'coverpage')]"),
                By.XPath("//textarea[contains(@placeholder, 'cover')]"),
                By.XPath("//input[@type='text'][contains(@id, 'cover')]"),
                By.XPath("//textarea[contains(@class, 'cover')]"),
                By.CssSelector("textarea[id*='cover'], input[type='text'][id*='cover']"),
                By.CssSelector("textarea[name*='cover'], input[type='text'][name*='cover']")
            };

            // Try to find and fill cover page text field
            foreach (var locator in textLocators)
            {
                try
                {
                    var textField = Wait.WaitForElementToBeVisible(locator, 3);
                    if (textField != null && textField.Displayed && textField.Enabled)
                    {
                        // Clear and enter text
                        textField.Clear();
                        Thread.Sleep(500);
                        textField.SendKeys(coverPageText);
                        Thread.Sleep(500);

                        // Verify text was entered
                        var enteredText = textField.GetAttribute("value");
                        if (string.IsNullOrEmpty(enteredText))
                        {
                            enteredText = textField.Text;
                        }

                        if (!string.IsNullOrEmpty(enteredText) && enteredText.Contains(coverPageText.Substring(0, Math.Min(10, coverPageText.Length))))
                        {
                            Logger.Information("Successfully entered cover page text using locator: {Locator}. Text: '{Text}'", locator, enteredText);
                            textEntered = true;
                            break;
                        }
                        else
                        {
                            Logger.Warning("Cover page text field found but text verification failed using locator: {Locator}", locator);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Debug("Cover page text locator {Locator} failed: {Error}", locator, ex.Message);
                }
            }

            Logger.Information("Cover page options handling completed. Checkbox: {CheckboxSelected}, Text: {TextEntered}", 
                checkboxSelected, textEntered);

            return (checkboxSelected, textEntered);
        }
    }
}

using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;
using System;

namespace SeleniumPL.Core
{
    /// <summary>
    /// Smart wait conditions that adapt to different scenarios
    /// </summary>
    public class SmartWaitConditions
    {
        private readonly IWebDriver _driver;
        private readonly ILogger _logger;

        public SmartWaitConditions(IWebDriver driver, ILogger logger)
        {
            _driver = driver;
            _logger = logger;
        }

        /// <summary>
        /// Wait for page to be fully loaded (DOM ready + no active requests)
        /// </summary>
        public bool WaitForPageToLoad(int timeoutSeconds = 30)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                
                // Wait for document ready state
                wait.Until(driver => ((IJavaScriptExecutor)driver)
                    .ExecuteScript("return document.readyState").Equals("complete"));
                
                // Wait for jQuery if present
                try
                {
                    wait.Until(driver => (bool)((IJavaScriptExecutor)driver)
                        .ExecuteScript("return typeof jQuery === 'undefined' || jQuery.active == 0"));
                }
                catch
                {
                    // jQuery not present, continue
                }
                
                // Wait for Angular if present
                try
                {
                    wait.Until(driver => (bool)((IJavaScriptExecutor)driver)
                        .ExecuteScript("return typeof angular === 'undefined' || angular.element(document).injector().get('$http').pendingRequests.length === 0"));
                }
                catch
                {
                    // Angular not present, continue
                }
                
                _logger.Debug("Page fully loaded");
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.Warning("Page load timeout after {Timeout} seconds", timeoutSeconds);
                return false;
            }
        }

        /// <summary>
        /// Wait for element to be stable and interactable
        /// </summary>
        public IWebElement WaitForStableElement(By locator, int timeoutSeconds = 10)
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
            
            return wait.Until(driver =>
            {
                try
                {
                    var element = driver.FindElement(locator);
                    
                    // Check if element is displayed and enabled
                    if (!element.Displayed || !element.Enabled)
                        return null;
                    
                    // Check element stability
                    var initialLocation = element.Location;
                    var initialSize = element.Size;
                    
                    System.Threading.Thread.Sleep(100);
                    
                    // Verify element is still found and hasn't moved
                    var stableElement = driver.FindElement(locator);
                    if (stableElement.Location == initialLocation && stableElement.Size == initialSize)
                    {
                        return stableElement;
                    }
                    
                    return null;
                }
                catch (NoSuchElementException)
                {
                    return null;
                }
                catch (StaleElementReferenceException)
                {
                    return null;
                }
            });
        }

        /// <summary>
        /// Wait for overlay/loading indicators to disappear
        /// </summary>
        public bool WaitForLoadingToComplete(int timeoutSeconds = 30)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                
                // Common loading indicators
                var loadingSelectors = new[]
                {
                    "div.loading",
                    "div.spinner",
                    ".loading-overlay",
                    ".loader",
                    "[data-loading='true']",
                    ".progress-bar",
                    ".sk-spinner"
                };

                foreach (var selector in loadingSelectors)
                {
                    try
                    {
                        wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.CssSelector(selector)));
                    }
                    catch (WebDriverTimeoutException)
                    {
                        // Continue with next selector
                    }
                }

                _logger.Debug("Loading indicators cleared");
                return true;
            }
            catch (Exception ex)
            {
                _logger.Warning("Error waiting for loading to complete: {Error}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Wait for network requests to complete
        /// </summary>
        public bool WaitForNetworkIdle(int timeoutSeconds = 10, int idleTimeMs = 1000)
        {
            try
            {
                var endTime = DateTime.Now.AddSeconds(timeoutSeconds);
                var lastActiveTime = DateTime.Now;

                while (DateTime.Now < endTime)
                {
                    try
                    {
                        // Check for active network requests
                        var activeRequests = (long)((IJavaScriptExecutor)_driver)
                            .ExecuteScript(@"
                                return (typeof window.performance !== 'undefined' && 
                                       typeof window.performance.getEntriesByType !== 'undefined') 
                                       ? window.performance.getEntriesByType('navigation').length + 
                                         window.performance.getEntriesByType('resource')
                                           .filter(r => r.responseEnd === 0).length 
                                       : 0;");

                        if (activeRequests > 0)
                        {
                            lastActiveTime = DateTime.Now;
                        }
                        else if (DateTime.Now.Subtract(lastActiveTime).TotalMilliseconds >= idleTimeMs)
                        {
                            _logger.Debug("Network idle detected");
                            return true;
                        }

                        System.Threading.Thread.Sleep(100);
                    }
                    catch
                    {
                        // If we can't check network status, assume idle
                        return true;
                    }
                }

                _logger.Warning("Network idle timeout after {Timeout} seconds", timeoutSeconds);
                return false;
            }
            catch (Exception ex)
            {
                _logger.Warning("Error waiting for network idle: {Error}", ex.Message);
                return false;
            }
        }
    }
}

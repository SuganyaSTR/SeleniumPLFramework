using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using Serilog;
using System;
using System.Threading;

namespace SeleniumPL.Core
{
    /// <summary>
    /// Enhanced element interaction helper with stability checks and retry mechanisms
    /// </summary>
    public class StableElementInteractions
    {
        private readonly IWebDriver _driver;
        private readonly WaitHelper _waitHelper;
        private readonly ILogger _logger;

        public StableElementInteractions(IWebDriver driver, WaitHelper waitHelper, ILogger logger)
        {
            _driver = driver;
            _waitHelper = waitHelper;
            _logger = logger;
        }

        /// <summary>
        /// Click element with stability checks and retry logic
        /// </summary>
        public void StableClick(By locator, int maxAttempts = 3, int stabilityWaitMs = 500)
        {
            RetryHelper.ExecuteWithRetry(() =>
            {
                var element = _waitHelper.WaitForElementToBeClickable(locator);
                
                // Ensure element is stable before clicking
                WaitForElementStability(element, stabilityWaitMs);
                
                // Scroll element into view if needed
                ScrollToElementIfNeeded(element);
                
                // Try regular click first
                try
                {
                    element.Click();
                    _logger.Debug("Successfully clicked element: {Locator}", locator);
                }
                catch (ElementClickInterceptedException)
                {
                    _logger.Warning("Element click intercepted, trying JavaScript click: {Locator}", locator);
                    JavaScriptClick(element);
                }
                catch (StaleElementReferenceException)
                {
                    _logger.Warning("Stale element reference, refinding element: {Locator}", locator);
                    var freshElement = _waitHelper.WaitForElementToBeClickable(locator);
                    freshElement.Click();
                }

            }, maxAttempts, TimeSpan.FromMilliseconds(500), true, _logger);
        }

        /// <summary>
        /// Type text with stability checks
        /// </summary>
        public void StableTypeText(By locator, string text, bool clearFirst = true, int maxAttempts = 3)
        {
            RetryHelper.ExecuteWithRetry(() =>
            {
                var element = _waitHelper.WaitForElementToBeVisible(locator);
                WaitForElementStability(element, 300);
                
                if (clearFirst)
                {
                    element.Clear();
                    Thread.Sleep(100); // Brief pause after clear
                }
                
                element.SendKeys(text);
                
                // Verify text was entered correctly
                var actualText = element.GetAttribute("value") ?? element.Text;
                if (!actualText.Contains(text))
                {
                    throw new InvalidOperationException($"Text verification failed. Expected: {text}, Actual: {actualText}");
                }
                
                _logger.Debug("Successfully typed text into element: {Locator}", locator);

            }, maxAttempts, TimeSpan.FromMilliseconds(300), true, _logger);
        }

        /// <summary>
        /// Select dropdown option with stability
        /// </summary>
        public void StableSelectDropdown(By locator, string optionText, int maxAttempts = 3)
        {
            RetryHelper.ExecuteWithRetry(() =>
            {
                var element = _waitHelper.WaitForElementToBeClickable(locator);
                WaitForElementStability(element, 300);
                
                var select = new SelectElement(element);
                select.SelectByText(optionText);
                
                // Verify selection
                var selectedOption = select.SelectedOption;
                if (selectedOption?.Text != optionText)
                {
                    throw new InvalidOperationException($"Selection verification failed. Expected: {optionText}, Actual: {selectedOption?.Text}");
                }
                
                _logger.Debug("Successfully selected dropdown option: {Option} for {Locator}", optionText, locator);

            }, maxAttempts, TimeSpan.FromMilliseconds(300), true, _logger);
        }

        /// <summary>
        /// Hover over element with stability
        /// </summary>
        public void StableHover(By locator, int maxAttempts = 3)
        {
            RetryHelper.ExecuteWithRetry(() =>
            {
                var element = _waitHelper.WaitForElementToBeVisible(locator);
                WaitForElementStability(element, 300);
                
                var actions = new Actions(_driver);
                actions.MoveToElement(element).Perform();
                
                _logger.Debug("Successfully hovered over element: {Locator}", locator);

            }, maxAttempts, TimeSpan.FromMilliseconds(300), true, _logger);
        }

        /// <summary>
        /// Wait for element to be stable (not moving/changing)
        /// </summary>
        private void WaitForElementStability(IWebElement element, int stabilityWaitMs)
        {
            var initialLocation = element.Location;
            var initialSize = element.Size;
            
            Thread.Sleep(stabilityWaitMs);
            
            var finalLocation = element.Location;
            var finalSize = element.Size;
            
            if (initialLocation != finalLocation || initialSize != finalSize)
            {
                _logger.Debug("Element position/size changed, waiting for stability");
                Thread.Sleep(stabilityWaitMs);
            }
        }

        /// <summary>
        /// Scroll element into view if needed
        /// </summary>
        private void ScrollToElementIfNeeded(IWebElement element)
        {
            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block: 'center'});", element);
                Thread.Sleep(200); // Brief pause after scroll
            }
            catch (Exception ex)
            {
                _logger.Warning("Failed to scroll to element: {Error}", ex.Message);
            }
        }

        /// <summary>
        /// Click using JavaScript as fallback
        /// </summary>
        private void JavaScriptClick(IWebElement element)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", element);
        }
    }
}

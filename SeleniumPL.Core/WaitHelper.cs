using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;

namespace SeleniumPL.Core
{
    public class WaitHelper
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly ILogger _logger;

        public WaitHelper(IWebDriver driver, int timeoutSeconds = 10, ILogger? logger = null)
        {
            _driver = driver;
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
            _logger = logger ?? Log.Logger;
        }

        public IWebElement WaitForElementToBeVisible(By locator, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                var element = wait.Until(ExpectedConditions.ElementIsVisible(locator));
                _logger.Debug("Element is visible: {Locator}", locator);
                return element;
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.Error("Element not visible within {Timeout} seconds: {Locator}", timeoutSeconds, locator);
                throw new TimeoutException($"Element not visible: {locator}", ex);
            }
        }

        public IWebElement WaitForElementToBeClickable(By locator, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                var element = wait.Until(ExpectedConditions.ElementToBeClickable(locator));
                _logger.Debug("Element is clickable: {Locator}", locator);
                return element;
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.Error("Element not clickable within {Timeout} seconds: {Locator}", timeoutSeconds, locator);
                throw new TimeoutException($"Element not clickable: {locator}", ex);
            }
        }

        public bool WaitForElementToDisappear(By locator, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
                _logger.Debug("Element disappeared: {Locator}", locator);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.Warning("Element still visible after {Timeout} seconds: {Locator}", timeoutSeconds, locator);
                return false;
            }
        }

        public bool WaitForTextToBePresentInElement(By locator, string text, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(ExpectedConditions.TextToBePresentInElementLocated(locator, text));
                _logger.Debug("Text '{Text}' found in element: {Locator}", text, locator);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.Warning("Text '{Text}' not found in element within {Timeout} seconds: {Locator}", text, timeoutSeconds, locator);
                return false;
            }
        }

        public bool WaitForPageTitle(string title, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(ExpectedConditions.TitleContains(title));
                _logger.Debug("Page title contains: {Title}", title);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.Warning("Page title does not contain '{Title}' within {Timeout} seconds", title, timeoutSeconds);
                return false;
            }
        }

        public bool WaitForUrl(string url, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                wait.Until(ExpectedConditions.UrlContains(url));
                _logger.Debug("URL contains: {Url}", url);
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.Warning("URL does not contain '{Url}' within {Timeout} seconds", url, timeoutSeconds);
                return false;
            }
        }

        public IWebElement WaitForElementToExist(By locator, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                var element = wait.Until(ExpectedConditions.ElementExists(locator));
                _logger.Debug("Element exists: {Locator}", locator);
                return element;
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.Error("Element does not exist within {Timeout} seconds: {Locator}", timeoutSeconds, locator);
                throw new TimeoutException($"Element does not exist: {locator}", ex);
            }
        }

        public IList<IWebElement> WaitForElementsToExist(By locator, int timeoutSeconds = 10)
        {
            try
            {
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(timeoutSeconds));
                var elements = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(locator));
                _logger.Debug("Elements exist: {Locator}, Count: {Count}", locator, elements.Count);
                return elements;
            }
            catch (WebDriverTimeoutException ex)
            {
                _logger.Error("Elements do not exist within {Timeout} seconds: {Locator}", timeoutSeconds, locator);
                throw new TimeoutException($"Elements do not exist: {locator}", ex);
            }
        }
    }
}
